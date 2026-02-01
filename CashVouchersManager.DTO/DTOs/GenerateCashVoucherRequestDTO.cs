namespace CashVouchersManager.DTO.DTOs;

/// <summary>
/// Request DTO for generating a new cash voucher
/// </summary>
public class GenerateCashVoucherRequestDTO
{
    /// <summary>
    /// The amount of the voucher in euros
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// The ID of the issuing store (max 4 digits)
    /// </summary>
    public int IssuingStoreId { get; set; }

    /// <summary>
    /// Optional expiration date
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Optional issuing sale ID (max 128 characters)
    /// </summary>
    public string? IssuingSaleId { get; set; }
}
