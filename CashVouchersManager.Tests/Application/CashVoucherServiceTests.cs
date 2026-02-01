using Microsoft.EntityFrameworkCore;
using CashVouchersManager.Application.Services;
using CashVouchersManager.Domain.Services;
using CashVouchersManager.Infrastructure.Data;
using CashVouchersManager.Infrastructure.Repositories;
using CashVouchersManager.DTO.DTOs;
using CashVouchersManager.DTO.Enums;

namespace CashVouchersManager.Tests.Application;

/// <summary>
/// Unit tests for the CashVoucherService
/// </summary>
public class CashVoucherServiceTests : IDisposable
{
    private readonly CashVouchersDbContext _context;
    private readonly CashVoucherRepository _repository;
    private readonly VoucherCodeGenerator _codeGenerator;
    private readonly CashVoucherService _service;

    /// <summary>
    /// Initializes a new instance of the CashVoucherServiceTests class
    /// Sets up an in-memory database and service dependencies
    /// </summary>
    public CashVoucherServiceTests()
    {
        var options = new DbContextOptionsBuilder<CashVouchersDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new CashVouchersDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repository = new CashVoucherRepository(_context);
        _codeGenerator = new VoucherCodeGenerator();
        _service = new CashVoucherService(_repository, _codeGenerator);
    }

    /// <summary>
    /// Tests that GenerateCashVoucherAsync creates a voucher with valid EAN13 code
    /// </summary>
    [Fact]
    public async Task GenerateCashVoucherAsync_ShouldCreateVoucherWithValidEan13Code()
    {
        // Arrange
        var request = new GenerateCashVoucherRequestDTO
        {
            Amount = 100.50m,
            IssuingStoreId = 1234,
            ExpirationDate = DateTime.UtcNow.AddDays(30),
            IssuingSaleId = "SALE-001"
        };

        // Act
        var result = await _service.GenerateCashVoucherAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(13, result.Code.Length);
        Assert.Equal(request.Amount, result.Amount);
        Assert.Equal(request.IssuingStoreId, result.IssuingStoreId);
        Assert.Equal(request.IssuingSaleId, result.IssuingSaleId);
        Assert.Equal(CashVoucherStatusEnum.Active, result.Status);
    }

    /// <summary>
    /// Tests that GenerateCashVoucherAsync generates unique codes
    /// </summary>
    [Fact]
    public async Task GenerateCashVoucherAsync_ShouldGenerateUniqueCodes()
    {
        // Arrange
        var request = new GenerateCashVoucherRequestDTO
        {
            Amount = 50.00m,
            IssuingStoreId = 1234,
            ExpirationDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var voucher1 = await _service.GenerateCashVoucherAsync(request);
        var voucher2 = await _service.GenerateCashVoucherAsync(request);
        var voucher3 = await _service.GenerateCashVoucherAsync(request);

        // Assert
        Assert.NotEqual(voucher1.Code, voucher2.Code);
        Assert.NotEqual(voucher2.Code, voucher3.Code);
        Assert.NotEqual(voucher1.Code, voucher3.Code);
    }

    /// <summary>
    /// Tests that GetCashVoucherByCodeAsync returns all vouchers with the specified code
    /// </summary>
    [Fact]
    public async Task GetCashVoucherByCodeAsync_ShouldReturnAllVouchersWithCode()
    {
        // Arrange
        var request = new GenerateCashVoucherRequestDTO
        {
            Amount = 75.00m,
            IssuingStoreId = 5678
        };
        var voucher1 = await _service.GenerateCashVoucherAsync(request);
        
        // Seed another voucher with same code manually
        await SeedVoucherAsync(voucher1.Code, isRedeemed: false, isExpired: false);

        // Act
        var result = await _service.GetCashVoucherByCodeAsync(voucher1.Code, onlyActives: false);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, v => Assert.Equal(voucher1.Code, v.Code));
    }

    /// <summary>
    /// Tests that GetCashVoucherByCodeAsync with onlyActives returns only active vouchers
    /// </summary>
    [Fact]
    public async Task GetCashVoucherByCodeAsync_WithOnlyActives_ShouldReturnOnlyActiveVouchers()
    {
        // Arrange
        var request = new GenerateCashVoucherRequestDTO
        {
            Amount = 50.00m,
            IssuingStoreId = 1234
        };
        var voucher = await _service.GenerateCashVoucherAsync(request);
        
        // Seed a redeemed voucher with same code
        await SeedVoucherAsync(voucher.Code, isRedeemed: true, isExpired: false);

        // Act
        var activeVouchers = await _service.GetCashVoucherByCodeAsync(voucher.Code, onlyActives: true);
        var allVouchers = await _service.GetCashVoucherByCodeAsync(voucher.Code, onlyActives: false);

        // Assert
        Assert.Single(activeVouchers);
        Assert.Equal(2, allVouchers.Count);
        Assert.All(activeVouchers, v => Assert.Equal(CashVoucherStatusEnum.Active, v.Status));
    }

    /// <summary>
    /// Tests that GetFilteredCashVouchersAsync returns vouchers filtered by status
    /// </summary>
    [Fact]
    public async Task GetFilteredCashVouchersAsync_ShouldFilterByStatus()
    {
        // Arrange
        await SeedVoucherAsync("1111111111111", isRedeemed: false, isExpired: false); // Active
        await SeedVoucherAsync("2222222222222", isRedeemed: true, isExpired: false);  // Redeemed
        await SeedVoucherAsync("3333333333333", isRedeemed: false, isExpired: true);  // Expired

        // Act
        var activeVouchers = await _service.GetFilteredCashVouchersAsync(status: CashVoucherStatusEnum.Active);
        var redeemedVouchers = await _service.GetFilteredCashVouchersAsync(status: CashVoucherStatusEnum.Redeemed);
        var expiredVouchers = await _service.GetFilteredCashVouchersAsync(status: CashVoucherStatusEnum.Expired);

        // Assert
        Assert.Single(activeVouchers);
        Assert.Single(redeemedVouchers);
        Assert.Single(expiredVouchers);
        Assert.Equal(CashVoucherStatusEnum.Active, activeVouchers[0].Status);
        Assert.Equal(CashVoucherStatusEnum.Redeemed, redeemedVouchers[0].Status);
        Assert.Equal(CashVoucherStatusEnum.Expired, expiredVouchers[0].Status);
    }

    /// <summary>
    /// Tests that GetFilteredCashVouchersAsync returns vouchers filtered by store ID
    /// </summary>
    [Fact]
    public async Task GetFilteredCashVouchersAsync_ShouldFilterByIssuingStoreId()
    {
        // Arrange
        await SeedVoucherAsync("1111111111111", isRedeemed: false, isExpired: false, storeId: 1234);
        await SeedVoucherAsync("2222222222222", isRedeemed: false, isExpired: false, storeId: 5678);
        await SeedVoucherAsync("3333333333333", isRedeemed: false, isExpired: false, storeId: 1234);

        // Act
        var vouchers = await _service.GetFilteredCashVouchersAsync(issuingStoreId: 1234);

        // Assert
        Assert.Equal(2, vouchers.Count);
        Assert.All(vouchers, v => Assert.Equal(1234, v.IssuingStoreId));
    }

    /// <summary>
    /// Tests that RedeemCashVoucherAsync redeems all active vouchers with the specified code
    /// </summary>
    [Fact]
    public async Task RedeemCashVoucherAsync_ShouldRedeemAllActiveVouchers()
    {
        // Arrange
        var request = new GenerateCashVoucherRequestDTO
        {
            Amount = 50.00m,
            IssuingStoreId = 1234
        };
        var voucher = await _service.GenerateCashVoucherAsync(request);
        
        // Seed another active voucher with same code
        await SeedVoucherAsync(voucher.Code, isRedeemed: false, isExpired: false);

        var redeemRequest = new RedeemCashVoucherRequestDTO
        {
            RedemptionSaleId = "REDEMPTION-123"
        };

        // Act
        var redeemedVouchers = await _service.RedeemCashVoucherAsync(voucher.Code, redeemRequest);

        // Assert
        Assert.Equal(2, redeemedVouchers.Count);
        Assert.All(redeemedVouchers, v =>
        {
            Assert.NotNull(v.RedemptionDate);
            Assert.Equal("REDEMPTION-123", v.RedemptionSaleId);
            Assert.Equal(CashVoucherStatusEnum.Redeemed, v.Status);
        });
    }

    /// <summary>
    /// Tests that RedeemCashVoucherAsync does not redeem already redeemed vouchers
    /// </summary>
    [Fact]
    public async Task RedeemCashVoucherAsync_ShouldNotRedeemAlreadyRedeemedVouchers()
    {
        // Arrange
        string code = "1234567890123";
        await SeedVoucherAsync(code, isRedeemed: true, isExpired: false);
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: false);

        var redeemRequest = new RedeemCashVoucherRequestDTO
        {
            RedemptionSaleId = "NEW-REDEMPTION"
        };

        // Act
        var redeemedVouchers = await _service.RedeemCashVoucherAsync(code, redeemRequest);

        // Assert
        Assert.Single(redeemedVouchers); // Only the active one should be redeemed
    }

    /// <summary>
    /// Tests that RedeemCashVoucherAsync returns empty list when no active vouchers exist
    /// </summary>
    [Fact]
    public async Task RedeemCashVoucherAsync_ShouldReturnEmptyList_WhenNoActiveVouchers()
    {
        // Arrange
        string code = "9999999999999";
        var redeemRequest = new RedeemCashVoucherRequestDTO();

        // Act
        var result = await _service.RedeemCashVoucherAsync(code, redeemRequest);

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Tests that RedeemCashVoucherAsync uses current UTC time when no date is provided
    /// </summary>
    [Fact]
    public async Task RedeemCashVoucherAsync_ShouldUseUtcNow_WhenNoDateProvided()
    {
        // Arrange
        var request = new GenerateCashVoucherRequestDTO
        {
            Amount = 50.00m,
            IssuingStoreId = 1234
        };
        var voucher = await _service.GenerateCashVoucherAsync(request);

        var redeemRequest = new RedeemCashVoucherRequestDTO(); // No date specified
        var beforeRedemption = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var redeemedVouchers = await _service.RedeemCashVoucherAsync(voucher.Code, redeemRequest);
        var afterRedemption = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.Single(redeemedVouchers);
        Assert.NotNull(redeemedVouchers[0].RedemptionDate);
        Assert.True(redeemedVouchers[0].RedemptionDate >= beforeRedemption);
        Assert.True(redeemedVouchers[0].RedemptionDate <= afterRedemption);
    }

    /// <summary>
    /// Tests that RedeemCashVoucherAsync uses provided redemption date
    /// </summary>
    [Fact]
    public async Task RedeemCashVoucherAsync_ShouldUseProvidedDate()
    {
        // Arrange
        var request = new GenerateCashVoucherRequestDTO
        {
            Amount = 50.00m,
            IssuingStoreId = 1234
        };
        var voucher = await _service.GenerateCashVoucherAsync(request);

        var specificDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var redeemRequest = new RedeemCashVoucherRequestDTO
        {
            RedemptionDate = specificDate
        };

        // Act
        var redeemedVouchers = await _service.RedeemCashVoucherAsync(voucher.Code, redeemRequest);

        // Assert
        Assert.Single(redeemedVouchers);
        Assert.Equal(specificDate, redeemedVouchers[0].RedemptionDate);
    }

    /// <summary>
    /// Tests that RedeemCashVoucherAsync sets InUse to false after redemption
    /// </summary>
    [Fact]
    public async Task RedeemCashVoucherAsync_ShouldSetInUseToFalse()
    {
        // Arrange
        string code = "1234567890123";
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: false, inUse: true);

        var redeemRequest = new RedeemCashVoucherRequestDTO
        {
            RedemptionSaleId = "REDEMPTION-001"
        };

        // Act
        var redeemedVouchers = await _service.RedeemCashVoucherAsync(code, redeemRequest);

        // Assert
        Assert.Single(redeemedVouchers);
        Assert.False(redeemedVouchers[0].InUse);
        Assert.Equal(CashVoucherStatusEnum.Redeemed, redeemedVouchers[0].Status);
    }

    /// <summary>
    /// Tests that SetCashVouchersInUseAsync only sets InUse for active vouchers when setting to true
    /// </summary>
    [Fact]
    public async Task SetCashVouchersInUseAsync_WithTrue_ShouldOnlyUpdateActiveVouchers()
    {
        // Arrange
        string code = "1234567890123";
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: false, inUse: false); // Active
        await SeedVoucherAsync(code, isRedeemed: true, isExpired: false, inUse: false);  // Redeemed

        // Act
        var result = await _service.SetCashVouchersInUseAsync(code, true);

        // Assert
        Assert.Equal(2, result.Count);
        
        var activeVoucher = result.FirstOrDefault(v => v.Status == CashVoucherStatusEnum.InUse);
        var redeemedVoucher = result.FirstOrDefault(v => v.Status == CashVoucherStatusEnum.Redeemed);

        Assert.NotNull(activeVoucher);
        Assert.True(activeVoucher.InUse);
        
        Assert.NotNull(redeemedVoucher);
        Assert.False(redeemedVoucher.InUse);
    }

    /// <summary>
    /// Tests that SetCashVouchersInUseAsync with false updates all vouchers
    /// </summary>
    [Fact]
    public async Task SetCashVouchersInUseAsync_WithFalse_ShouldUpdateAllVouchers()
    {
        // Arrange
        string code = "1234567890123";
        await SeedVoucherAsync(code, isRedeemed: false, isExpired: false, inUse: true); // Active
        await SeedVoucherAsync(code, isRedeemed: true, isExpired: false, inUse: true);  // Redeemed

        // Act
        var result = await _service.SetCashVouchersInUseAsync(code, false);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, v => Assert.False(v.InUse));
    }

    /// <summary>
    /// Seeds a voucher into the database using raw SQL
    /// </summary>
    /// <param name="code">The voucher code</param>
    /// <param name="isRedeemed">Whether the voucher is redeemed</param>
    /// <param name="isExpired">Whether the voucher is expired</param>
    /// <param name="storeId">The issuing store ID</param>
    /// <param name="inUse">Whether the voucher is in use</param>
    private async Task SeedVoucherAsync(
        string code,
        bool isRedeemed,
        bool isExpired,
        int storeId = 1234,
        bool inUse = false)
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
