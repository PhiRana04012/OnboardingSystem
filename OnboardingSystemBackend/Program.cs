using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

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

// Add RIMS Integration Service
builder.Services.AddScoped<IRimsIntegrationService, RimsIntegrationService>();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=.\\SQLEXPRESS;Database=onboarding;Integrated Security=true;TrustServerCertificate=true;";
    options.UseSqlServer(connectionString);
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
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
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
app.UseAuthorization();
app.MapControllers();

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
            DbInitializer.Initialize(context);
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