using Microsoft.EntityFrameworkCore;
using CashVouchersManager.Infrastructure.Data;
using CashVouchersManager.Infrastructure.Repositories;

namespace CashVouchersManager.Tests.Infrastructure;

/// <summary>
/// Unit tests for the voucher cleanup functionality in CashVoucherRepository
/// </summary>
public class VoucherCleanupTests : IDisposable
{
    private readonly CashVouchersDbContext _context;
    private readonly CashVoucherRepository _repository;

    /// <summary>
    /// Initializes a new instance of the VoucherCleanupTests class
    /// </summary>
    public VoucherCleanupTests()
    {
        var options = new DbContextOptionsBuilder<CashVouchersDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new CashVouchersDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repository = new CashVoucherRepository(_context);
    }

    /// <summary>
    /// Tests that DeleteOldVouchersAsync deletes redeemed vouchers older than 1 year
    /// </summary>
    [Fact]
    public async Task DeleteOldVouchersAsync_ShouldDeleteRedeemedVouchersOlderThanOneYear()
    {
        // Arrange
        var oldRedemptionDate = DateTime.UtcNow.AddYears(-2);
        var recentRedemptionDate = DateTime.UtcNow.AddDays(-30);

        await SeedVoucherAsync("1234567890123", redemptionDate: oldRedemptionDate, expirationDate: DateTime.UtcNow.AddDays(30));
        await SeedVoucherAsync("1234567890124", redemptionDate: recentRedemptionDate, expirationDate: DateTime.UtcNow.AddDays(30));

        // Act
        var deletedCount = await _repository.DeleteOldVouchersAsync();

        // Assert
        Assert.Equal(1, deletedCount);

        var remainingVouchers = await _context.CashVouchers.ToListAsync();
        Assert.Single(remainingVouchers);
        Assert.Equal("1234567890124", remainingVouchers[0].Code);
    }

    /// <summary>
    /// Tests that DeleteOldVouchersAsync deletes expired vouchers older than 1 year
    /// </summary>
    [Fact]
    public async Task DeleteOldVouchersAsync_ShouldDeleteExpiredVouchersOlderThanOneYear()
    {
        // Arrange
        var oldExpirationDate = DateTime.UtcNow.AddYears(-2);
        var recentExpirationDate = DateTime.UtcNow.AddDays(-30);

        await SeedVoucherAsync("1234567890123", redemptionDate: null, expirationDate: oldExpirationDate);
        await SeedVoucherAsync("1234567890124", redemptionDate: null, expirationDate: recentExpirationDate);

        // Act
        var deletedCount = await _repository.DeleteOldVouchersAsync();

        // Assert
        Assert.Equal(1, deletedCount);

        var remainingVouchers = await _context.CashVouchers.ToListAsync();
        Assert.Single(remainingVouchers);
        Assert.Equal("1234567890124", remainingVouchers[0].Code);
    }

    /// <summary>
    /// Tests that DeleteOldVouchersAsync deletes both old redeemed and old expired vouchers
    /// </summary>
    [Fact]
    public async Task DeleteOldVouchersAsync_ShouldDeleteBothOldRedeemedAndOldExpiredVouchers()
    {
        // Arrange
        var oldDate = DateTime.UtcNow.AddYears(-2);

        await SeedVoucherAsync("1234567890123", redemptionDate: oldDate, expirationDate: DateTime.UtcNow.AddDays(30));
        await SeedVoucherAsync("1234567890124", redemptionDate: null, expirationDate: oldDate);
        await SeedVoucherAsync("1234567890125", redemptionDate: null, expirationDate: DateTime.UtcNow.AddDays(30));

        // Act
        var deletedCount = await _repository.DeleteOldVouchersAsync();

        // Assert
        Assert.Equal(2, deletedCount);

        var remainingVouchers = await _context.CashVouchers.ToListAsync();
        Assert.Single(remainingVouchers);
        Assert.Equal("1234567890125", remainingVouchers[0].Code);
    }

    /// <summary>
    /// Tests that DeleteOldVouchersAsync does not delete recent vouchers
    /// </summary>
    [Fact]
    public async Task DeleteOldVouchersAsync_ShouldNotDeleteRecentVouchers()
    {
        // Arrange
        var recentDate = DateTime.UtcNow.AddDays(-30);

        await SeedVoucherAsync("1234567890123", redemptionDate: recentDate, expirationDate: DateTime.UtcNow.AddDays(30));
        await SeedVoucherAsync("1234567890124", redemptionDate: null, expirationDate: recentDate);
        await SeedVoucherAsync("1234567890125", redemptionDate: null, expirationDate: DateTime.UtcNow.AddDays(30));

        // Act
        var deletedCount = await _repository.DeleteOldVouchersAsync();

        // Assert
        Assert.Equal(0, deletedCount);

        var remainingVouchers = await _context.CashVouchers.ToListAsync();
        Assert.Equal(3, remainingVouchers.Count);
    }

    /// <summary>
    /// Tests that DeleteOldVouchersAsync returns zero when no vouchers exist
    /// </summary>
    [Fact]
    public async Task DeleteOldVouchersAsync_ShouldReturnZero_WhenNoVouchersExist()
    {
        // Act
        var deletedCount = await _repository.DeleteOldVouchersAsync();

        // Assert
        Assert.Equal(0, deletedCount);
    }

    /// <summary>
    /// Tests that DeleteOldVouchersAsync deletes vouchers exactly at the 1 year boundary
    /// </summary>
    [Fact]
    public async Task DeleteOldVouchersAsync_ShouldDeleteVouchersAtExactlyOneYearBoundary()
    {
        // Arrange
        var exactlyOneYearAgo = DateTime.UtcNow.AddYears(-1).AddSeconds(-1);
        var justUnderOneYear = DateTime.UtcNow.AddYears(-1).AddSeconds(1);

        await SeedVoucherAsync("1234567890123", redemptionDate: exactlyOneYearAgo, expirationDate: DateTime.UtcNow.AddDays(30));
        await SeedVoucherAsync("1234567890124", redemptionDate: justUnderOneYear, expirationDate: DateTime.UtcNow.AddDays(30));

        // Act
        var deletedCount = await _repository.DeleteOldVouchersAsync();

        // Assert
        Assert.Equal(1, deletedCount);

        var remainingVouchers = await _context.CashVouchers.ToListAsync();
        Assert.Single(remainingVouchers);
        Assert.Equal("1234567890124", remainingVouchers[0].Code);
    }

    /// <summary>
    /// Seeds a voucher into the database using raw SQL
    /// </summary>
    /// <param name="code">The voucher code</param>
    /// <param name="redemptionDate">The redemption date (null if not redeemed)</param>
    /// <param name="expirationDate">The expiration date</param>
    /// <param name="storeId">The issuing store ID</param>
    private async Task SeedVoucherAsync(
        string code,
        DateTime? redemptionDate,
        DateTime? expirationDate,
        int storeId = 1234)
    {
        var creation = DateTime.UtcNow;

        await _context.Database.ExecuteSqlRawAsync(
            @"INSERT INTO CashVouchers 
            (Code, Amount, CreationDate, IssuingStoreId, RedemptionDate, ExpirationDate, IssuingSaleId, RedemptionSaleId, InUse) 
            VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})",
            code, 50.00m, creation, storeId, redemptionDate!, expirationDate!, null!, null!, false);
    }

    /// <summary>
    /// Disposes the database context and closes the connection
    /// </summary>
    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
}
