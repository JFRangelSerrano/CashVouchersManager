using CashVouchersManager.Domain.Entities;
using CashVouchersManager.DTO.Enums;

namespace CashVouchersManager.Tests.Domain;

/// <summary>
/// Unit tests for the CashVoucher entity status calculation
/// </summary>
public class CashVoucherStatusTests
{
    /// <summary>
    /// Tests that a voucher is Active when not redeemed and not expired
    /// </summary>
    [Fact]
    public void Status_ShouldBeActive_WhenNotRedeemedAndNotExpired()
    {
        // Arrange
        var voucher = new CashVoucher
        {
            Code = "1234567890123",
            Amount = 50.00m,
            CreationDate = DateTime.UtcNow,
            IssuingStoreId = 1234,
            RedemptionDate = null,
            ExpirationDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var status = voucher.Status;

        // Assert
        Assert.Equal(CashVoucherStatusEnum.Active, status);
    }

    /// <summary>
    /// Tests that a voucher is Active when not redeemed and no expiration date
    /// </summary>
    [Fact]
    public void Status_ShouldBeActive_WhenNotRedeemedAndNoExpirationDate()
    {
        // Arrange
        var voucher = new CashVoucher
        {
            Code = "1234567890123",
            Amount = 50.00m,
            CreationDate = DateTime.UtcNow,
            IssuingStoreId = 1234,
            RedemptionDate = null,
            ExpirationDate = null
        };

        // Act
        var status = voucher.Status;

        // Assert
        Assert.Equal(CashVoucherStatusEnum.Active, status);
    }

    /// <summary>
    /// Tests that a voucher is Redeemed when redemption date is set
    /// </summary>
    [Fact]
    public void Status_ShouldBeRedeemed_WhenRedemptionDateIsSet()
    {
        // Arrange
        var voucher = new CashVoucher
        {
            Code = "1234567890123",
            Amount = 50.00m,
            CreationDate = DateTime.UtcNow.AddDays(-10),
            IssuingStoreId = 1234,
            RedemptionDate = DateTime.UtcNow,
            ExpirationDate = null
        };

        // Act
        var status = voucher.Status;

        // Assert
        Assert.Equal(CashVoucherStatusEnum.Redeemed, status);
    }

    /// <summary>
    /// Tests that a voucher is Redeemed even if also expired
    /// </summary>
    [Fact]
    public void Status_ShouldBeRedeemed_WhenBothRedeemedAndExpired()
    {
        // Arrange
        var voucher = new CashVoucher
        {
            Code = "1234567890123",
            Amount = 50.00m,
            CreationDate = DateTime.UtcNow.AddDays(-60),
            IssuingStoreId = 1234,
            RedemptionDate = DateTime.UtcNow.AddDays(-5),
            ExpirationDate = DateTime.UtcNow.AddDays(-10)
        };

        // Act
        var status = voucher.Status;

        // Assert
        Assert.Equal(CashVoucherStatusEnum.Redeemed, status);
    }

    /// <summary>
    /// Tests that a voucher is Expired when expiration date has passed
    /// </summary>
    [Fact]
    public void Status_ShouldBeExpired_WhenExpirationDateHasPassed()
    {
        // Arrange
        var voucher = new CashVoucher
        {
            Code = "1234567890123",
            Amount = 50.00m,
            CreationDate = DateTime.UtcNow.AddDays(-60),
            IssuingStoreId = 1234,
            RedemptionDate = null,
            ExpirationDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var status = voucher.Status;

        // Assert
        Assert.Equal(CashVoucherStatusEnum.Expired, status);
    }

    /// <summary>
    /// Tests that a voucher is Expired even if expiration was more than 30 days ago
    /// </summary>
    [Fact]
    public void Status_ShouldBeExpired_WhenExpiredMoreThan30DaysAgo()
    {
        // Arrange
        var voucher = new CashVoucher
        {
            Code = "1234567890123",
            Amount = 50.00m,
            CreationDate = DateTime.UtcNow.AddDays(-100),
            IssuingStoreId = 1234,
            RedemptionDate = null,
            ExpirationDate = DateTime.UtcNow.AddDays(-50)
        };

        // Act
        var status = voucher.Status;

        // Assert
        Assert.Equal(CashVoucherStatusEnum.Expired, status);
    }

    /// <summary>
    /// Tests that Redeemed status takes precedence over Expired
    /// </summary>
    [Fact]
    public void Status_RedeemedTakesPrecedence_OverExpired()
    {
        // Arrange
        var voucher = new CashVoucher
        {
            Code = "1234567890123",
            Amount = 50.00m,
            CreationDate = DateTime.UtcNow.AddDays(-40),
            IssuingStoreId = 1234,
            RedemptionDate = DateTime.UtcNow.AddDays(-100), // Redeemed long ago
            ExpirationDate = DateTime.UtcNow.AddDays(-50)   // Also expired
        };

        // Act
        var status = voucher.Status;

        // Assert
        Assert.Equal(CashVoucherStatusEnum.Redeemed, status);
    }
}
