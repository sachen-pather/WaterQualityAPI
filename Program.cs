using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WaterQualityAPI.Repositories;
using WaterQualityAPI.Services;
using WaterQualityAPI.Data;
using System.Reflection;
using Microsoft.OpenApi.Models;
using HealthChecks.UI.Client;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
if (builder.Environment.IsProduction())
{
    builder.Logging.AddAzureWebAppDiagnostics();
}
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Enhanced Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SeaClear API",
        Version = "v1",
        Description = "API for managing beach water quality and community discussions"
    });

    // Include XML comments for Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add Authorization (allow anonymous access by default)
builder.Services.AddAuthorization();

// Add CORS
// Add CORS - Update this section in your Program.cs
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173") // Your frontend port
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Add this line
    });
});

// Configure Supabase connection
var supabaseConnectionString = builder.Configuration.GetConnectionString("SupabaseConnection");

if (string.IsNullOrEmpty(supabaseConnectionString))
{
    // Fallback to environment variables or hardcoded for development
    var host = Environment.GetEnvironmentVariable("SUPABASE_HOST") ?? "aws-0-eu-west-2.pooler.supabase.com";
    var database = Environment.GetEnvironmentVariable("SUPABASE_DB") ?? "postgres";
    var username = Environment.GetEnvironmentVariable("SUPABASE_USER") ?? "postgres.onhcovwfqfkyrifyfyix";
    var password = Environment.GetEnvironmentVariable("SUPABASE_PASSWORD") ?? "Brucechlo_0107";
    var port = Environment.GetEnvironmentVariable("SUPABASE_PORT") ?? "5432";

    supabaseConnectionString = $"Host={host};Database={database};Username={username};Password={password};Port={port};SSL Mode=Require;Trust Server Certificate=true";

    builder.Services.AddLogging(logging => logging.AddConsole());
    var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<Program>();
    logger.LogWarning("SupabaseConnection string not found in configuration. Using fallback connection.");
}

// Add Supabase database context (removing the old ApplicationDbContext)
builder.Services.AddDbContext<SupabaseContext>(options =>
{
    options.UseNpgsql(supabaseConnectionString);
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
});

// Register services and repositories
builder.Services.AddScoped<PdfParsingService>();
builder.Services.AddScoped<IBeachRepository, BeachRepository>();
builder.Services.AddScoped<IWaterQualityRepository, WaterQualityRepository>();
builder.Services.AddScoped<ICommunityPostRepository, CommunityPostRepository>();
builder.Services.AddScoped<ICommunityRepository, CommunityRepository>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("supabase", () =>
    {
        var data = new Dictionary<string, object>
        {
            { "ConnectionString", supabaseConnectionString?.Substring(0, Math.Min(50, supabaseConnectionString.Length)) + "..." ?? "Not found" },
            { "Environment", builder.Environment.EnvironmentName }
        };

        try
        {
            using var connection = new NpgsqlConnection(supabaseConnectionString);
            connection.Open();

            // Test the connection with a simple query
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT version()";
            var version = command.ExecuteScalar()?.ToString();

            data["PostgreSQLVersion"] = version ?? "Unknown";
            return HealthCheckResult.Healthy("Successfully connected to Supabase PostgreSQL.", data: data);
        }
        catch (Exception ex)
        {
            data["ErrorDetails"] = ex.Message;
            data["InnerException"] = ex.InnerException?.Message ?? "No inner exception";
            return HealthCheckResult.Unhealthy($"Failed to connect to Supabase: {ex.Message}", exception: ex, data: data);
        }
    });

var app = builder.Build();

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SupabaseContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
        app.Logger.LogInformation("Database connection verified and tables created if needed.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to ensure database is created.");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SeaClear API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Debug endpoint for connection testing
app.MapGet("/debug/connection", (SupabaseContext context) =>
{
    try
    {
        var canConnect = context.Database.CanConnect();
        return Results.Ok(new
        {
            CanConnect = canConnect,
            DatabaseProvider = context.Database.ProviderName,
            Environment = app.Environment.EnvironmentName,
            Timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
});

// Configure health check endpoint with detailed response
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

app.MapControllers();

app.Run();