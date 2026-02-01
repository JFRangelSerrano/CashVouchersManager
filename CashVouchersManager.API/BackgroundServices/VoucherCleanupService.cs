using CashVouchersManager.Domain.Interfaces;

namespace CashVouchersManager.API.BackgroundServices;

/// <summary>
/// Background service that performs daily cleanup of old vouchers
/// </summary>
public class VoucherCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VoucherCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromDays(1);

    /// <summary>
    /// Initializes a new instance of the <see cref="VoucherCleanupService"/> class
    /// </summary>
    /// <param name="serviceProvider">The service provider for creating scoped services</param>
    /// <param name="logger">The logger instance</param>
    public VoucherCleanupService(
        IServiceProvider serviceProvider,
        ILogger<VoucherCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Executes the background service task
    /// </summary>
    /// <param name="stoppingToken">The cancellation token to stop the service</param>
    /// <returns>A task representing the asynchronous operation</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Voucher Cleanup Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting voucher cleanup at: {Time}", DateTime.UtcNow);

                await PerformCleanupAsync();

                _logger.LogInformation("Voucher cleanup completed at: {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during voucher cleanup");
            }

            // Wait for the next cleanup interval
            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("Voucher Cleanup Service is stopping.");
    }

    /// <summary>
    /// Performs the cleanup operation by deleting old vouchers
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    private async Task PerformCleanupAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICashVoucherRepository>();

        var deletedCount = await repository.DeleteOldVouchersAsync();

        _logger.LogInformation("Deleted {Count} old vouchers", deletedCount);
    }
}
