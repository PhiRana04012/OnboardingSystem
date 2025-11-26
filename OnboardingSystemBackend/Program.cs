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

app.Run();