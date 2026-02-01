using CashVouchersManager.DTO.DTOs;
using CashVouchersManager.DTO.Enums;

namespace CashVouchersManager.Application.Interfaces;

/// <summary>
/// Service interface for cash voucher operations
/// </summary>
public interface ICashVoucherService
{
    /// <summary>
    /// Generates a new cash voucher
    /// </summary>
    /// <param name="request">The request data for generating the voucher</param>
    /// <returns>The generated cash voucher DTO</returns>
    Task<CashVoucherDTO> GenerateCashVoucherAsync(GenerateCashVoucherRequestDTO request);

    /// <summary>
    /// Gets cash vouchers by code
    /// </summary>
    /// <param name="code">The voucher code</param>
    /// <param name="onlyActives">If true, returns only active vouchers</param>
    /// <returns>A list of cash voucher DTOs</returns>
    Task<List<CashVoucherDTO>> GetCashVoucherByCodeAsync(string code, bool onlyActives = true);

    /// <summary>
    /// Gets filtered cash vouchers based on criteria
    /// </summary>
    /// <param name="status">The status filter</param>
    /// <param name="issuingStoreId">The issuing store ID filter</param>
    /// <param name="dateFrom">The start date filter</param>
    /// <param name="dateTo">The end date filter</param>
    /// <param name="dateType">The date type to filter by</param>
    /// <returns>A list of cash voucher DTOs</returns>
    Task<List<CashVoucherDTO>> GetFilteredCashVouchersAsync(
        CashVoucherStatusEnum? status = null,
        int? issuingStoreId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CashVoucherDateTypeEnum dateType = CashVoucherDateTypeEnum.Creation);

    /// <summary>
    /// Redeems all active cash vouchers with the specified code
    /// </summary>
    /// <param name="code">The voucher code</param>
    /// <param name="request">The redemption request data</param>
    /// <returns>A list of redeemed cash voucher DTOs</returns>
    Task<List<CashVoucherDTO>> RedeemCashVoucherAsync(string code, RedeemCashVoucherRequestDTO request);

    /// <summary>
    /// Sets the InUse flag for all vouchers with the specified code
    /// </summary>
    /// <param name="code">The voucher code</param>
    /// <param name="inUse">The value to set for the InUse flag</param>
    /// <returns>A list of updated cash voucher DTOs</returns>
    Task<List<CashVoucherDTO>> SetCashVouchersInUseAsync(string code, bool inUse);
}
