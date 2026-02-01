using Microsoft.EntityFrameworkCore;
using CashVouchersManager.Application.Interfaces;
using CashVouchersManager.Application.Services;
using CashVouchersManager.Domain.Interfaces;
using CashVouchersManager.Domain.Services;
using CashVouchersManager.Infrastructure.Data;
using CashVouchersManager.Infrastructure.Repositories;
using CashVouchersManager.API.BackgroundServices;
using CashVouchersManager.API.Configuration;
using CashVouchersManager.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure application settings
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();

// Configure Kestrel to listen on configured port
// Use PORT environment variable if available (for Railway, Render, etc.), otherwise use appsettings
var port = Environment.GetEnvironmentVariable("PORT") ?? appSettings.Port.ToString();
var isDevelopment = builder.Environment.IsDevelopment();
var host = isDevelopment ? "localhost" : "0.0.0.0";
builder.WebHost.UseUrls($"http://{host}:{port}");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure database
// In production (Railway), use /data directory for persistent volume
// In development, use application directory
var dbDirectory = builder.Environment.IsProduction() 
    ? "/data" 
    : AppContext.BaseDirectory;

// Ensure directory exists
if (!Directory.Exists(dbDirectory))
{
    Directory.CreateDirectory(dbDirectory);
}

var dbPath = Path.Combine(dbDirectory, "CashVouchers.db");
builder.Services.AddDbContext<CashVouchersDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Register domain services
builder.Services.AddScoped<VoucherCodeGenerator>();

// Register repositories
builder.Services.AddScoped<ICashVoucherRepository, CashVoucherRepository>();

// Register application services
builder.Services.AddScoped<ICashVoucherService, CashVoucherService>();

// Register background services
builder.Services.AddHostedService<VoucherCleanupService>();

var app = builder.Build();

// Apply migrations automatically with error handling
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<CashVouchersDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Applying database migrations...");
        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while applying database migrations.");
    throw; // Re-throw to prevent app from starting with broken database
}

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

// Use Basic Authentication middleware
app.UseMiddleware<BasicAuthenticationMiddleware>();

// Use HTTPS redirection if configured
if (appSettings.UseHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

// Log startup information
app.Logger.LogInformation("Starting Cash Vouchers Manager API");
app.Logger.LogInformation($"Environment: {builder.Environment.EnvironmentName}");
app.Logger.LogInformation($"Listening on: http://{host}:{port}");
app.Logger.LogInformation($"Database path: {dbPath}");

app.Run();

// Make the implicit Program class public for testing
public partial class Program { }
