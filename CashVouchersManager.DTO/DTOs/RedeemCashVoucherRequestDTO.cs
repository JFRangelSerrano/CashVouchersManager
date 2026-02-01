namespace CashVouchersManager.DTO.DTOs;

/// <summary>
/// Request DTO for redeeming a cash voucher
/// </summary>
public class RedeemCashVoucherRequestDTO
{
    /// <summary>
    /// The redemption date in UTC. If not provided, current UTC time will be used
    /// </summary>
    public DateTime? RedemptionDate { get; set; }

    /// <summary>
    /// Optional redemption sale ID (max 128 characters)
    /// </summary>
    public string? RedemptionSaleId { get; set; }
}
