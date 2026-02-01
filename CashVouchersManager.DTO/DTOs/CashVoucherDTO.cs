using CashVouchersManager.DTO.Enums;

namespace CashVouchersManager.DTO.DTOs;

/// <summary>
/// Data transfer object representing a cash voucher
/// </summary>
public class CashVoucherDTO
{
    /// <summary>
    /// The voucher code in EAN13 format
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// The amount of the voucher in euros
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// The creation date in UTC
    /// </summary>
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// The ID of the issuing store
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
    /// The issuing sale ID, if set
    /// </summary>
    public string? IssuingSaleId { get; set; }

    /// <summary>
    /// The redemption sale ID, if redeemed
    /// </summary>
    public string? RedemptionSaleId { get; set; }

    /// <summary>
    /// The current status of the voucher
    /// </summary>
    public CashVoucherStatusEnum Status { get; set; }
}
