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
var port = appSettings.Port;
builder.WebHost.UseUrls($"http://localhost:{port}");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure database
var dbPath = Path.Combine(AppContext.BaseDirectory, "CashVouchers.db");
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

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CashVouchersDbContext>();
    dbContext.Database.Migrate();
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

app.Run();

// Make the implicit Program class public for testing
public partial class Program { }
