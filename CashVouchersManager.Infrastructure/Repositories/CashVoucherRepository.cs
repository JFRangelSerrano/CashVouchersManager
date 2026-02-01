using Microsoft.EntityFrameworkCore;
using CashVouchersManager.Domain.Entities;
using CashVouchersManager.Domain.Interfaces;
using CashVouchersManager.Infrastructure.Data;
using CashVouchersManager.DTO.Enums;

namespace CashVouchersManager.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for cash voucher operations
/// </summary>
public class CashVoucherRepository : ICashVoucherRepository
{
    private readonly CashVouchersDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CashVoucherRepository"/> class
    /// </summary>
    /// <param name="context">The database context</param>
    public CashVoucherRepository(CashVouchersDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all cash vouchers with the specified code
    /// </summary>
    /// <param name="code">The voucher code</param>
    /// <param name="onlyActives">If true, returns only active vouchers</param>
    /// <returns>A list of cash vouchers matching the code</returns>
    public async Task<List<CashVoucher>> GetByCodeAsync(string code, bool onlyActives = true)
    {
        var query = _context.CashVouchers.Where(v => v.Code == code);

        if (onlyActives)
        {
            var utcNow = DateTime.UtcNow;
            // Active means: not redeemed AND not expired
            query = query.Where(v => 
                v.RedemptionDate == null && 
                (v.ExpirationDate == null || v.ExpirationDate >= utcNow));
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets filtered cash vouchers based on the provided criteria
    /// </summary>
    /// <param name="status">The status filter</param>
    /// <param name="issuingStoreId">The issuing store ID filter</param>
    /// <param name="dateFrom">The start date filter</param>
    /// <param name="dateTo">The end date filter</param>
    /// <param name="dateType">The date type to filter by</param>
    /// <returns>A list of cash vouchers matching the filters</returns>
    public async Task<List<CashVoucher>> GetFilteredAsync(
        CashVoucherStatusEnum? status = null,
        int? issuingStoreId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CashVoucherDateTypeEnum dateType = CashVoucherDateTypeEnum.Creation)
    {
        var query = _context.CashVouchers.AsQueryable();
        var utcNow = DateTime.UtcNow;

        // Filter by status
        if (status.HasValue)
        {
            switch (status.Value)
            {
                case CashVoucherStatusEnum.Redeemed:
                    query = query.Where(v => v.RedemptionDate != null);
                    break;
                case CashVoucherStatusEnum.Expired:
                    query = query.Where(v => 
                        v.RedemptionDate == null && 
                        v.ExpirationDate != null && 
                        v.ExpirationDate < utcNow);
                    break;
                case CashVoucherStatusEnum.Active:
                    query = query.Where(v => 
                        v.RedemptionDate == null && 
                        (v.ExpirationDate == null || v.ExpirationDate >= utcNow));
                    break;
            }
        }

        // Filter by issuing store
        if (issuingStoreId.HasValue)
        {
            query = query.Where(v => v.IssuingStoreId == issuingStoreId.Value);
        }

        // Filter by dates
        if (dateFrom.HasValue || dateTo.HasValue)
        {
            switch (dateType)
            {
                case CashVoucherDateTypeEnum.Creation:
                    if (dateFrom.HasValue)
                        query = query.Where(v => v.CreationDate >= dateFrom.Value);
                    if (dateTo.HasValue)
                        query = query.Where(v => v.CreationDate <= dateTo.Value);
                    break;
                case CashVoucherDateTypeEnum.Redemption:
                    if (dateFrom.HasValue)
                        query = query.Where(v => v.RedemptionDate != null && v.RedemptionDate >= dateFrom.Value);
                    if (dateTo.HasValue)
                        query = query.Where(v => v.RedemptionDate != null && v.RedemptionDate <= dateTo.Value);
                    break;
                case CashVoucherDateTypeEnum.Expiration:
                    if (dateFrom.HasValue)
                        query = query.Where(v => v.ExpirationDate != null && v.ExpirationDate >= dateFrom.Value);
                    if (dateTo.HasValue)
                        query = query.Where(v => v.ExpirationDate != null && v.ExpirationDate <= dateTo.Value);
                    break;
            }
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Adds a new cash voucher
    /// </summary>
    /// <param name="cashVoucher">The cash voucher to add</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task AddAsync(CashVoucher cashVoucher)
    {
        // Use raw SQL because entity has no key and cannot be tracked
        await _context.Database.ExecuteSqlRawAsync(
            @"INSERT INTO CashVouchers 
            (Code, Amount, CreationDate, IssuingStoreId, RedemptionDate, ExpirationDate, IssuingSaleId, RedemptionSaleId) 
            VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
            cashVoucher.Code,
            cashVoucher.Amount,
            cashVoucher.CreationDate,
            cashVoucher.IssuingStoreId,
            cashVoucher.RedemptionDate!,
            cashVoucher.ExpirationDate!,
            cashVoucher.IssuingSaleId!,
            cashVoucher.RedemptionSaleId!);
    }

    /// <summary>
    /// Updates multiple cash vouchers
    /// </summary>
    /// <param name="cashVouchers">The cash vouchers to update</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task UpdateAsync(List<CashVoucher> cashVouchers)
    {
        // Use raw SQL for each voucher update
        foreach (var voucher in cashVouchers)
        {
            await _context.Database.ExecuteSqlRawAsync(
                @"UPDATE CashVouchers 
                SET Amount = {0}, 
                    CreationDate = {1}, 
                    IssuingStoreId = {2}, 
                    RedemptionDate = {3}, 
                    ExpirationDate = {4}, 
                    IssuingSaleId = {5}, 
                    RedemptionSaleId = {6}
                WHERE Code = {7}",
                voucher.Amount,
                voucher.CreationDate,
                voucher.IssuingStoreId,
                voucher.RedemptionDate!,
                voucher.ExpirationDate!,
                voucher.IssuingSaleId!,
                voucher.RedemptionSaleId!,
                voucher.Code);
        }
    }

    /// <summary>
    /// Checks if a code exists in active vouchers
    /// </summary>
    /// <param name="code">The voucher code to check</param>
    /// <returns>True if the code exists in active vouchers, false otherwise</returns>
    public async Task<bool> CodeExistsInActiveVouchersAsync(string code)
    {
        var utcNow = DateTime.UtcNow;
        var thirtyDaysAgo = utcNow.AddDays(-30);

        // Active vouchers are those that are not redeemed and not expired for more than 30 days
        var exists = await _context.CashVouchers
            .AnyAsync(v => v.Code == code &&
                (v.RedemptionDate == null || v.RedemptionDate >= thirtyDaysAgo) &&
                (v.ExpirationDate == null || v.ExpirationDate >= thirtyDaysAgo));

        return exists;
    }
}
