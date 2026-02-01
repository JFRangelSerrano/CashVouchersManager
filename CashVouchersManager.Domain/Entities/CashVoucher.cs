using CashVouchersManager.DTO.Enums;

namespace CashVouchersManager.Domain.Entities;

/// <summary>
/// Represents a cash voucher entity
/// Note: This entity has no primary key property and is not tracked by EF Core
/// </summary>
public class CashVoucher
{
    /// <summary>
    /// The voucher code in EAN13 format
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// The amount of the voucher in euros (2 decimals)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// The creation date in UTC
    /// </summary>
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// The ID of the issuing store (max 4 digits)
    /// </summary>
    public int IssuingStoreId { get; set; }

    /// <summary>
    /// The redemption date in UTC, if redeemed
    /// </summary>
    public DateTime? RedemptionDate { get; set; }

    /// <summary>
    /// The expiration date in UTC, if set
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// The issuing sale ID (max 128 characters)
    /// </summary>
    public string? IssuingSaleId { get; set; }

    /// <summary>
    /// The redemption sale ID (max 128 characters)
    /// </summary>
    public string? RedemptionSaleId { get; set; }

    /// <summary>
    /// Indicates if the voucher is currently in use
    /// </summary>
    public bool InUse { get; set; }

    /// <summary>
    /// Gets the calculated status of the voucher based on its properties
    /// </summary>
    public CashVoucherStatusEnum Status
    {
        get
        {
            if (RedemptionDate.HasValue)
                return CashVoucherStatusEnum.Redeemed;

            if (ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow)
                return CashVoucherStatusEnum.Expired;

            if (InUse)
                return CashVoucherStatusEnum.InUse;

            return CashVoucherStatusEnum.Active;
        }
    }
}
