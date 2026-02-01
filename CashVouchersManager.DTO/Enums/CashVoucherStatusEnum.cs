namespace CashVouchersManager.DTO.Enums;

/// <summary>
/// Represents the status of a cash voucher
/// </summary>
public enum CashVoucherStatusEnum
{
    /// <summary>
    /// The voucher is active and can be redeemed
    /// </summary>
    Active,

    /// <summary>
    /// The voucher has been redeemed
    /// </summary>
    Redeemed,

    /// <summary>
    /// The voucher has expired
    /// </summary>
    Expired
}
