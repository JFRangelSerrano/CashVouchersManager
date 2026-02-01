using Microsoft.EntityFrameworkCore;
using CashVouchersManager.Infrastructure.Data;
using CashVouchersManager.Infrastructure.Repositories;
using CashVouchersManager.Domain.Entities;
using CashVouchersManager.DTO.Enums;

namespace CashVouchersManager.Tests.Infrastructure;

/// <summary>
/// Unit tests for the InUse functionality in CashVoucherRepository
/// </summary>
public class CashVoucherInUseTests : IDisposable
{
    private readonly CashVouchersDbContext _context;
    private readonly CashVoucherRepository _repository;

    /// <summary>
    /// Initializes a new instance of the CashVoucherInUseTests class
    /// </summary>
    public CashVoucherInUseTests()
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
    /// Tests that SetInUseAsync sets InUse flag to true for active vouchers
    /// </summary>
    [Fact]
    public async Task SetInUseAsync_ShouldSetInUseFlagToTrue_ForActiveVouchers()
    {
        // Arrange
        string code = "1234567890123";
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: false, inUse: false);

        // Act
        var result = await _repository.SetInUseAsync(code, true);

        // Assert
        Assert.Single(result);
        Assert.True(result[0].InUse);
    }

    /// <summary>
    /// Tests that SetInUseAsync sets InUse flag to false
    /// </summary>
    [Fact]
    public async Task SetInUseAsync_ShouldSetInUseFlagToFalse()
    {
        // Arrange
        string code = "1234567890123";
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: false, inUse: true);

        // Act
        var result = await _repository.SetInUseAsync(code, false);

        // Assert
        Assert.Single(result);
        Assert.False(result[0].InUse);
    }

    /// <summary>
    /// Tests that SetInUseAsync only sets InUse=true for non-redeemed, non-expired vouchers
    /// </summary>
    [Fact]
    public async Task SetInUseAsync_WithTrue_ShouldOnlyUpdateActiveVouchers()
    {
        // Arrange
        string code = "1234567890123";
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: false, inUse: false); // Active - should be updated
        await SeedVoucherAsync(code, isRedeemed: true, isExpired: false, inUse: false);  // Redeemed - should NOT be updated
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: true, inUse: false);  // Expired - should NOT be updated

        // Act
        var result = await _repository.SetInUseAsync(code, true);

        // Assert
        Assert.Equal(3, result.Count);
        
        // Only the active voucher should have InUse=true
        var activeVoucher = result.FirstOrDefault(v => v.RedemptionDate == null && v.ExpirationDate > DateTime.UtcNow);
        var redeemedVoucher = result.FirstOrDefault(v => v.RedemptionDate != null);
        var expiredVoucher = result.FirstOrDefault(v => v.RedemptionDate == null && v.ExpirationDate <= DateTime.UtcNow);

        Assert.NotNull(activeVoucher);
        Assert.True(activeVoucher.InUse);
        
        Assert.NotNull(redeemedVoucher);
        Assert.False(redeemedVoucher.InUse); // Should remain false
        
        Assert.NotNull(expiredVoucher);
        Assert.False(expiredVoucher.InUse); // Should remain false
    }

    /// <summary>
    /// Tests that SetInUseAsync with false updates all vouchers regardless of status
    /// </summary>
    [Fact]
    public async Task SetInUseAsync_WithFalse_ShouldUpdateAllVouchers()
    {
        // Arrange
        string code = "1234567890123";
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: false, inUse: true); // Active
        await SeedVoucherAsync(code, isRedeemed: true, isExpired: false, inUse: true);  // Redeemed
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: true, inUse: true);  // Expired

        // Act
        var result = await _repository.SetInUseAsync(code, false);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, v => Assert.False(v.InUse));
    }

    /// <summary>
    /// Tests that SetInUseAsync allows setting InUse=false for redeemed vouchers
    /// </summary>
    [Fact]
    public async Task SetInUseAsync_ShouldAllowSettingFalse_ForRedeemedVoucher()
    {
        // Arrange
        string code = "1234567890123";
        await SeedVoucherAsync(code, isRedeemed: true, isExpired: false, inUse: true);

        // Act
        var result = await _repository.SetInUseAsync(code, false);

        // Assert
        Assert.Single(result);
        Assert.False(result[0].InUse);
    }

    /// <summary>
    /// Tests that SetInUseAsync returns empty list for non-existent code
    /// </summary>
    [Fact]
    public async Task SetInUseAsync_ShouldReturnEmptyList_ForNonExistentCode()
    {
        // Act
        var result = await _repository.SetInUseAsync("9999999999999", true);

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Tests that GetAllByCodeAsync returns all vouchers regardless of status
    /// </summary>
    [Fact]
    public async Task GetAllByCodeAsync_ShouldReturnAllVouchers_RegardlessOfStatus()
    {
        // Arrange
        string code = "1234567890123";
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: false, inUse: false); // Active
        await SeedVoucherAsync(code, isRedeemed: true, isExpired: false, inUse: false);  // Redeemed
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: true, inUse: false);  // Expired
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: false, inUse: true);  // InUse

        // Act
        var result = await _repository.GetAllByCodeAsync(code);

        // Assert
        Assert.Equal(4, result.Count);
    }

    /// <summary>
    /// Seeds a voucher into the database using raw SQL
    /// </summary>
    /// <param name="code">The voucher code</param>
    /// <param name="isRedeemed">Whether the voucher is redeemed</param>
    /// <param name="isExpired">Whether the voucher is expired</param>
    /// <param name="inUse">Whether the voucher is in use</param>
    /// <param name="storeId">The issuing store ID</param>
    private async Task SeedVoucherAsync(
        string code,
        bool isRedeemed,
        bool isExpired,
        bool inUse,
        int storeId = 1234)
    {
        var creation = DateTime.UtcNow;
        var redemption = isRedeemed ? DateTime.UtcNow.AddDays(-5) : (DateTime?)null;
        var expiration = isExpired ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddDays(30);

        await _context.Database.ExecuteSqlRawAsync(
            @"INSERT INTO CashVouchers 
            (Code, Amount, CreationDate, IssuingStoreId, RedemptionDate, ExpirationDate, IssuingSaleId, RedemptionSaleId, InUse) 
            VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})",
            code, 50.00m, creation, storeId, redemption!, expiration!, null!, null!, inUse);
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
