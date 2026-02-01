using Microsoft.EntityFrameworkCore;
using CashVouchersManager.Infrastructure.Data;
using CashVouchersManager.Infrastructure.Repositories;
using CashVouchersManager.Domain.Entities;
using CashVouchersManager.DTO.Enums;

namespace CashVouchersManager.Tests.Infrastructure;

/// <summary>
/// Unit tests for the CashVoucherRepository using SQLite In-Memory database
/// </summary>
public class CashVoucherRepositoryTests : IDisposable
{
    private readonly CashVouchersDbContext _context;
    private readonly CashVoucherRepository _repository;

    /// <summary>
    /// Initializes a new instance of the CashVoucherRepositoryTests class
    /// Sets up an in-memory SQLite database for testing
    /// </summary>
    public CashVoucherRepositoryTests()
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
    /// Tests that AddAsync successfully adds a voucher to the database
    /// </summary>
    [Fact]
    public async Task AddAsync_ShouldAddVoucherToDatabase()
    {
        // Arrange
        var voucher = new CashVoucher
        {
            Code = "1234567890123",
            Amount = 50.00m,
            CreationDate = DateTime.UtcNow,
            IssuingStoreId = 1234,
            ExpirationDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        await _repository.AddAsync(voucher);

        // Assert
        var savedVouchers = await _repository.GetByCodeAsync(voucher.Code, onlyActives: false);
        Assert.Single(savedVouchers);
        Assert.Equal(voucher.Code, savedVouchers[0].Code);
        Assert.Equal(voucher.Amount, savedVouchers[0].Amount);
    }

    /// <summary>
    /// Tests that GetByCodeAsync returns all vouchers with the specified code
    /// </summary>
    [Fact]
    public async Task GetByCodeAsync_ShouldReturnAllVouchersWithCode()
    {
        // Arrange
        string code = "1234567890123";
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: false);
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: false);

        // Act
        var vouchers = await _repository.GetByCodeAsync(code, onlyActives: false);

        // Assert
        Assert.Equal(2, vouchers.Count);
        Assert.All(vouchers, v => Assert.Equal(code, v.Code));
    }

    /// <summary>
    /// Tests that GetByCodeAsync with onlyActives=true filters out inactive vouchers
    /// </summary>
    [Fact]
    public async Task GetByCodeAsync_WithOnlyActives_ShouldReturnOnlyActiveVouchers()
    {
        // Arrange
        string code = "1234567890123";
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: false); // Active
        await SeedVoucherAsync(code, isRedeemed: true, isExpired: false);  // Redeemed (inactive)
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: true);  // Expired (inactive)

        // Act
        var activeVouchers = await _repository.GetByCodeAsync(code, onlyActives: true);

        // Assert
        Assert.Single(activeVouchers);
        Assert.Null(activeVouchers[0].RedemptionDate);
    }

    /// <summary>
    /// Tests that GetFilteredAsync returns vouchers filtered by status
    /// </summary>
    [Fact]
    public async Task GetFilteredAsync_ShouldFilterByStatus()
    {
        // Arrange
        await SeedVoucherAsync("1111111111111", isRedeemed: false, isExpired: false); // Active
        await SeedVoucherAsync("2222222222222", isRedeemed: true, isExpired: false);  // Redeemed
        await SeedVoucherAsync("3333333333333", isRedeemed: false, isExpired: true);  // Expired

        // Act
        var activeVouchers = await _repository.GetFilteredAsync(status: CashVoucherStatusEnum.Active);
        var redeemedVouchers = await _repository.GetFilteredAsync(status: CashVoucherStatusEnum.Redeemed);
        var expiredVouchers = await _repository.GetFilteredAsync(status: CashVoucherStatusEnum.Expired);

        // Assert
        Assert.Single(activeVouchers);
        Assert.Single(redeemedVouchers);
        Assert.Single(expiredVouchers);
    }

    /// <summary>
    /// Tests that GetFilteredAsync returns vouchers filtered by issuing store ID
    /// </summary>
    [Fact]
    public async Task GetFilteredAsync_ShouldFilterByIssuingStoreId()
    {
        // Arrange
        await SeedVoucherAsync("1111111111111", isRedeemed: false, isExpired: false, storeId: 1234);
        await SeedVoucherAsync("2222222222222", isRedeemed: false, isExpired: false, storeId: 5678);
        await SeedVoucherAsync("3333333333333", isRedeemed: false, isExpired: false, storeId: 1234);

        // Act
        var vouchers = await _repository.GetFilteredAsync(issuingStoreId: 1234);

        // Assert
        Assert.Equal(2, vouchers.Count);
        Assert.All(vouchers, v => Assert.Equal(1234, v.IssuingStoreId));
    }

    /// <summary>
    /// Tests that GetFilteredAsync returns vouchers filtered by creation date range
    /// </summary>
    [Fact]
    public async Task GetFilteredAsync_ShouldFilterByCreationDateRange()
    {
        // Arrange
        var now = DateTime.UtcNow;
        await SeedVoucherAsync("1111111111111", isRedeemed: false, isExpired: false, creationDate: now.AddDays(-10));
        await SeedVoucherAsync("2222222222222", isRedeemed: false, isExpired: false, creationDate: now.AddDays(-5));
        await SeedVoucherAsync("3333333333333", isRedeemed: false, isExpired: false, creationDate: now.AddDays(-1));

        // Act
        var vouchers = await _repository.GetFilteredAsync(
            dateFrom: now.AddDays(-6),
            dateTo: now.AddDays(-2),
            dateType: CashVoucherDateTypeEnum.Creation);

        // Assert
        Assert.Single(vouchers);
        Assert.Equal("2222222222222", vouchers[0].Code);
    }

    /// <summary>
    /// Tests that UpdateAsync successfully updates vouchers
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldUpdateVouchers()
    {
        // Arrange
        string code = "1234567890123";
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: false);
        
        var vouchersToUpdate = await _repository.GetByCodeAsync(code, onlyActives: false);
        var redemptionDate = DateTime.UtcNow;
        foreach (var voucher in vouchersToUpdate)
        {
            voucher.RedemptionDate = redemptionDate;
            voucher.RedemptionSaleId = "SALE-999";
        }

        // Act
        await _repository.UpdateAsync(vouchersToUpdate);

        // Assert
        var updatedVouchers = await _repository.GetByCodeAsync(code, onlyActives: false);
        Assert.All(updatedVouchers, v =>
        {
            Assert.NotNull(v.RedemptionDate);
            Assert.Equal("SALE-999", v.RedemptionSaleId);
        });
    }

    /// <summary>
    /// Tests that CodeExistsInActiveVouchersAsync returns true for active voucher codes
    /// </summary>
    [Fact]
    public async Task CodeExistsInActiveVouchersAsync_ShouldReturnTrue_ForActiveVoucher()
    {
        // Arrange
        string code = "1234567890123";
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: false);

        // Act
        bool exists = await _repository.CodeExistsInActiveVouchersAsync(code);

        // Assert
        Assert.True(exists);
    }

    /// <summary>
    /// Tests that CodeExistsInActiveVouchersAsync returns false for redeemed vouchers
    /// Validates that redeemed vouchers are always considered inactive regardless of time
    /// </summary>
    [Fact]
    public async Task CodeExistsInActiveVouchersAsync_ShouldReturnFalse_ForRedeemedVoucher()
    {
        // Arrange
        string code = "1234567890123";
        await SeedVoucherAsync(code, isRedeemed: true, isExpired: false);

        // Act
        bool exists = await _repository.CodeExistsInActiveVouchersAsync(code);

        // Assert
        Assert.False(exists);
    }

    /// <summary>
    /// Tests that CodeExistsInActiveVouchersAsync returns false for vouchers expired more than 30 days ago
    /// </summary>
    [Fact]
    public async Task CodeExistsInActiveVouchersAsync_ShouldReturnFalse_ForVoucherExpiredMoreThan30DaysAgo()
    {
        // Arrange
        string code = "1234567890123";
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: true, daysExpired: 35);

        // Act
        bool exists = await _repository.CodeExistsInActiveVouchersAsync(code);

        // Assert
        Assert.False(exists);
    }

    /// <summary>
    /// Tests that CodeExistsInActiveVouchersAsync returns true for vouchers expired less than 30 days ago
    /// </summary>
    [Fact]
    public async Task CodeExistsInActiveVouchersAsync_ShouldReturnTrue_ForVoucherExpiredLessThan30DaysAgo()
    {
        // Arrange
        string code = "1234567890123";
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: true, daysExpired: 15);

        // Act
        bool exists = await _repository.CodeExistsInActiveVouchersAsync(code);

        // Assert
        Assert.True(exists);
    }

    /// <summary>
    /// Seeds a voucher into the database using raw SQL
    /// </summary>
    /// <param name="code">The voucher code</param>
    /// <param name="isRedeemed">Whether the voucher is redeemed</param>
    /// <param name="isExpired">Whether the voucher is expired</param>
    /// <param name="daysExpired">Days since expiration (if expired)</param>
    /// <param name="storeId">The issuing store ID</param>
    /// <param name="creationDate">The creation date (null for current UTC)</param>
    private async Task SeedVoucherAsync(
        string code,
        bool isRedeemed,
        bool isExpired,
        int daysExpired = 1,
        int storeId = 1234,
        DateTime? creationDate = null)
    {
        var creation = creationDate ?? DateTime.UtcNow;
        var redemption = isRedeemed ? DateTime.UtcNow.AddDays(-5) : (DateTime?)null;
        var expiration = isExpired ? DateTime.UtcNow.AddDays(-daysExpired) : DateTime.UtcNow.AddDays(30);

        await _context.Database.ExecuteSqlRawAsync(
            @"INSERT INTO CashVouchers 
            (Code, Amount, CreationDate, IssuingStoreId, RedemptionDate, ExpirationDate, IssuingSaleId, RedemptionSaleId) 
            VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
            code, 50.00m, creation, storeId, redemption!, expiration!, null!, null!);
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
