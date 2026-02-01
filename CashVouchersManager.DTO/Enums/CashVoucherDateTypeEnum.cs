namespace CashVouchersManager.DTO.Enums;

/// <summary>
/// Represents the type of date for filtering cash vouchers
/// </summary>
public enum CashVoucherDateTypeEnum
{
    /// <summary>
    /// Filter by creation date
    /// </summary>
    Creation,

    /// <summary>
    /// Filter by redemption date
    /// </summary>
    Redemption,

    /// <summary>
    /// Filter by expiration date
    /// </summary>
    Expiration
}
