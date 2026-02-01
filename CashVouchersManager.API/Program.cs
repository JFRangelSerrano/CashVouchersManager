using Microsoft.EntityFrameworkCore;
using CashVouchersManager.Application.Interfaces;
using CashVouchersManager.Application.Services;
using CashVouchersManager.Domain.Interfaces;
using CashVouchersManager.Domain.Services;
using CashVouchersManager.Infrastructure.Data;
using CashVouchersManager.Infrastructure.Repositories;
using CashVouchersManager.API.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen only on HTTP
builder.WebHost.UseUrls("http://localhost:5000");

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

// app.UseHttpsRedirection(); // Disabled for testing
app.UseAuthorization();
app.MapControllers();

app.Run();
