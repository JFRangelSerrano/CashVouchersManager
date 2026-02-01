using CashVouchersManager.Domain.Entities;

namespace CashVouchersManager.Domain.Interfaces;

/// <summary>
/// Repository interface for cash voucher operations
/// </summary>
public interface ICashVoucherRepository
{
    /// <summary>
    /// Gets all cash vouchers with the specified code
    /// </summary>
    /// <param name="code">The voucher code</param>
    /// <param name="onlyActives">If true, returns only active vouchers</param>
    /// <returns>A list of cash vouchers matching the code</returns>
    Task<List<CashVoucher>> GetByCodeAsync(string code, bool onlyActives = true);

    /// <summary>
    /// Gets filtered cash vouchers based on the provided criteria
    /// </summary>
    /// <param name="status">The status filter</param>
    /// <param name="issuingStoreId">The issuing store ID filter</param>
    /// <param name="dateFrom">The start date filter</param>
    /// <param name="dateTo">The end date filter</param>
    /// <param name="dateType">The date type to filter by</param>
    /// <returns>A list of cash vouchers matching the filters</returns>
    Task<List<CashVoucher>> GetFilteredAsync(
        CashVouchersManager.DTO.Enums.CashVoucherStatusEnum? status = null,
        int? issuingStoreId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CashVouchersManager.DTO.Enums.CashVoucherDateTypeEnum dateType = CashVouchersManager.DTO.Enums.CashVoucherDateTypeEnum.Creation);

    /// <summary>
    /// Adds a new cash voucher
    /// </summary>
    /// <param name="cashVoucher">The cash voucher to add</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task AddAsync(CashVoucher cashVoucher);

    /// <summary>
    /// Updates multiple cash vouchers
    /// </summary>
    /// <param name="cashVouchers">The cash vouchers to update</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task UpdateAsync(List<CashVoucher> cashVouchers);

    /// <summary>
    /// Checks if a code exists in active vouchers
    /// </summary>
    /// <param name="code">The voucher code to check</param>
    /// <returns>True if the code exists in active vouchers, false otherwise</returns>
    Task<bool> CodeExistsInActiveVouchersAsync(string code);

    /// <summary>
    /// Sets the InUse flag for all vouchers with the specified code
    /// </summary>
    /// <param name="code">The voucher code</param>
    /// <param name="inUse">The value to set for InUse flag</param>
    /// <returns>A list of updated cash vouchers</returns>
    Task<List<CashVoucher>> SetInUseAsync(string code, bool inUse);

    /// <summary>
    /// Gets all vouchers with the specified code without any filtering
    /// </summary>
    /// <param name="code">The voucher code</param>
    /// <returns>A list of all cash vouchers matching the code</returns>
    Task<List<CashVoucher>> GetAllByCodeAsync(string code);

    /// <summary>
    /// Deletes old vouchers that meet the cleanup criteria
    /// Removes redeemed vouchers older than 1 year and expired vouchers older than 1 year
    /// </summary>
    /// <returns>The number of vouchers deleted</returns>
    Task<int> DeleteOldVouchersAsync();
}
