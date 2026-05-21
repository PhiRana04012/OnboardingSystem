using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnboardingSystem.Data;
using OnboardingSystem.Hubs;
using OnboardingSystem.Services;
using System.Reflection;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure QuestPDF
QuestPDF.Settings.License = LicenseType.Community;


// Add services
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IProgressAnalyticsService, ProgressAnalyticsService>();
builder.Services.AddScoped<ILearningPathService, LearningPathService>();

// Add HTTP Client for RIMS API
builder.Services.AddHttpClient("RimsApi", client =>
{
    var baseUrl = builder.Configuration["RimsApi:BaseUrl"] ?? string.Empty;
    if (!string.IsNullOrEmpty(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
    client.Timeout = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("RimsApi:TimeoutSeconds", 30));
    
    // Add API Key authentication if configured
    var apiKey = builder.Configuration["RimsApi:ApiKey"];
    if (!string.IsNullOrEmpty(apiKey))
    {
        client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        // Alternative: client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
    }
});

// builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IGamificationService, GamificationService>();
builder.Services.AddHttpClient<IRimsIntegrationService, RimsIntegrationService>();

// Add Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// Add Export Service
builder.Services.AddScoped<IReportExportService, ReportExportService>();

// JWT Bearer — токен с логина подставляется в User, фильтрация по ролям в контроллерах
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"];
if (!string.IsNullOrWhiteSpace(jwtKey))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
            // Просроченный/битый токен не роняет login и прочие анонимные запросы
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    context.NoResult();
                    return Task.CompletedTask;
                }
            };
        });
    builder.Services.AddAuthorization();
}

// Add Authentication Services
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddScoped<IAuthenticationProvider, LocalAuthProvider>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddHttpClient("AiMentor")
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(300); // 5 minutes for AI processing
    });
builder.Services.AddScoped<IAiMentorService, AiMentorService>();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=.\\SQLEXPRESS;Database=onboarding;Integrated Security=true;TrustServerCertificate=true;";
    options.UseSqlServer(connectionString, sql => sql.CommandTimeout(60));
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Система онбординга API",
        Version = "v1",
        Description = "API для управления процессом онбординга новых сотрудников"
    });

    // Include XML comments if available
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Enable annotations
    c.EnableAnnotations();
    
    // Use full name for schema IDs to avoid conflicts
    c.CustomSchemaIds(type => type.FullName);
});

// Add CORS
builder.Services.AddCors(options =>
{
    // Default policy for REST APIs
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
    
    // Specific policy for SignalR hubs - must allow credentials
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.MapGet("/", () => "Система онбординга API запущена. Перейдите на /swagger для документации.");

// Configure Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Система онбординга API v1");
        c.RoutePrefix = "swagger"; // Swagger UI доступен на /swagger
        c.DocumentTitle = "Система онбординга - API Documentation";
    });
}

// Middleware
app.UseHttpsRedirection();
// CORS must be before UseAuthorization and MapControllers
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications").RequireCors("SignalRPolicy");

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    // Retry strategy for database migration
    int maxRetries = 12;
    int delaySeconds = 5;
    
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            
            // Log connection string (safe version)
            var connStr = context.Database.GetConnectionString();
            var safeConnStr = System.Text.RegularExpressions.Regex.Replace(connStr ?? "", "Password=.*?;", "Password=***;");
            logger.LogInformation("Attempting to connect to database number {Attempt}/{MaxRetries}. Connection String: {ConnStr}", i + 1, maxRetries, safeConnStr);

            if (i == 0) 
            {
               // Brief wait on first attempt to give SQL Server a moment to start listening
               System.Threading.Thread.Sleep(2000);
            }

            context.Database.Migrate();
            var passwordHasher = services.GetRequiredService<PasswordHasher>();
            DbInitializer.Initialize(context, passwordHasher);
            logger.LogInformation("Database migration and seeding completed successfully.");
            break; // Success, exit loop
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Attempt {Attempt} of {MaxRetries} to migrate database failed. Waiting {Delay} seconds...", i + 1, maxRetries, delaySeconds);
            if (i == maxRetries - 1)
            {
                logger.LogError(ex, "Failed to migrate database after {MaxRetries} attempts.", maxRetries);
                // We might want to rethrow here if DB is critical
                // throw; 
            }
            // Sync wait is okay here as it's startup
            System.Threading.Thread.Sleep(delaySeconds * 1000); 
        }
    }
}

app.Run();