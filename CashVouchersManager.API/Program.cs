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

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure application settings
    builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
    var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();

    // Configure Kestrel to listen on configured port
    // Use PORT environment variable if available (for Railway, Render, etc.), otherwise use appsettings
    var port = Environment.GetEnvironmentVariable("PORT") ?? appSettings.Port.ToString();
    var host = builder.Environment.IsDevelopment() ? "localhost" : "0.0.0.0";
    var listenUrl = $"http://{host}:{port}";
    
    builder.WebHost.UseUrls(listenUrl);

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Configure database
    // Try multiple directories for SQLite database
    string dbPath;
    try
    {
        // Try /data first (for Railway volume)
        var dataDir = "/data";
        if (builder.Environment.IsProduction() && Directory.Exists(dataDir))
        {
            dbPath = Path.Combine(dataDir, "CashVouchers.db");
        }
        else
        {
            // Fallback to app directory
            dbPath = Path.Combine(AppContext.BaseDirectory, "CashVouchers.db");
        }
    }
    catch
    {
        // Ultimate fallback
        dbPath = Path.Combine(AppContext.BaseDirectory, "CashVouchers.db");
    }

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

    // Log startup information early
    app.Logger.LogInformation("=== Cash Vouchers Manager API Starting ===");
    app.Logger.LogInformation($"Environment: {builder.Environment.EnvironmentName}");
    app.Logger.LogInformation($"Listen URL: {listenUrl}");
    app.Logger.LogInformation($"Database: {dbPath}");

    // Apply migrations automatically with error handling
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CashVouchersDbContext>();
            
            app.Logger.LogInformation("Applying database migrations...");
            dbContext.Database.Migrate();
            app.Logger.LogInformation("Database migrations applied successfully.");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "FATAL: Database migration failed");
        throw;
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

    app.Logger.LogInformation("=== Application configured successfully ===");
    
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"FATAL ERROR: Application failed to start");
    Console.WriteLine($"Exception Type: {ex.GetType().FullName}");
    Console.WriteLine($"Message: {ex.Message}");
    Console.WriteLine($"StackTrace: {ex.StackTrace}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
    }
    throw;
}

// Make the implicit Program class public for testing
public partial class Program { }
