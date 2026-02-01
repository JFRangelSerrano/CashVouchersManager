namespace CashVouchersManager.DTO.DTOs;

/// <summary>
/// Request DTO for setting the InUse flag on cash vouchers
/// </summary>
public class SetInUseRequestDTO
{
    /// <summary>
    /// The value to set for the InUse flag
    /// </summary>
    public bool InUse { get; set; }
}
