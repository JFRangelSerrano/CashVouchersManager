using CashVouchersManager.Application.Interfaces;
using CashVouchersManager.Domain.Entities;
using CashVouchersManager.Domain.Interfaces;
using CashVouchersManager.Domain.Services;
using CashVouchersManager.DTO.DTOs;
using CashVouchersManager.DTO.Enums;

namespace CashVouchersManager.Application.Services;

/// <summary>
/// Service implementation for cash voucher operations
/// </summary>
public class CashVoucherService : ICashVoucherService
{
    private readonly ICashVoucherRepository _repository;
    private readonly VoucherCodeGenerator _codeGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CashVoucherService"/> class
    /// </summary>
    /// <param name="repository">The cash voucher repository</param>
    /// <param name="codeGenerator">The voucher code generator</param>
    public CashVoucherService(
        ICashVoucherRepository repository,
        VoucherCodeGenerator codeGenerator)
    {
        _repository = repository;
        _codeGenerator = codeGenerator;
    }

    /// <summary>
    /// Generates a new cash voucher
    /// </summary>
    /// <param name="request">The request data for generating the voucher</param>
    /// <returns>The generated cash voucher DTO</returns>
    public async Task<CashVoucherDTO> GenerateCashVoucherAsync(GenerateCashVoucherRequestDTO request)
    {
        // Generate unique code
        string code;
        bool codeExists;
        do
        {
            code = _codeGenerator.GenerateCode(request.IssuingStoreId);
            codeExists = await _repository.CodeExistsInActiveVouchersAsync(code);
        } while (codeExists);

        // Create voucher entity
        var voucher = new CashVoucher
        {
            Code = code,
            Amount = request.Amount,
            CreationDate = DateTime.UtcNow,
            IssuingStoreId = request.IssuingStoreId,
            ExpirationDate = request.ExpirationDate,
            IssuingSaleId = request.IssuingSaleId
        };

        // Save to repository
        await _repository.AddAsync(voucher);

        // Return DTO
        return MapToDto(voucher);
    }

    /// <summary>
    /// Gets cash vouchers by code
    /// </summary>
    /// <param name="code">The voucher code</param>
    /// <param name="onlyActives">If true, returns only active vouchers</param>
    /// <returns>A list of cash voucher DTOs</returns>
    public async Task<List<CashVoucherDTO>> GetCashVoucherByCodeAsync(string code, bool onlyActives = true)
    {
        var vouchers = await _repository.GetByCodeAsync(code, onlyActives);
        return vouchers.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Gets filtered cash vouchers based on criteria
    /// </summary>
    /// <param name="status">The status filter</param>
    /// <param name="issuingStoreId">The issuing store ID filter</param>
    /// <param name="dateFrom">The start date filter</param>
    /// <param name="dateTo">The end date filter</param>
    /// <param name="dateType">The date type to filter by</param>
    /// <returns>A list of cash voucher DTOs</returns>
    public async Task<List<CashVoucherDTO>> GetFilteredCashVouchersAsync(
        CashVoucherStatusEnum? status = null,
        int? issuingStoreId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CashVoucherDateTypeEnum dateType = CashVoucherDateTypeEnum.Creation)
    {
        var vouchers = await _repository.GetFilteredAsync(
            status, 
            issuingStoreId, 
            dateFrom, 
            dateTo, 
            dateType);
        
        return vouchers.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Redeems all active cash vouchers with the specified code
    /// </summary>
    /// <param name="code">The voucher code</param>
    /// <param name="request">The redemption request data</param>
    /// <returns>A list of redeemed cash voucher DTOs</returns>
    public async Task<List<CashVoucherDTO>> RedeemCashVoucherAsync(
        string code, 
        RedeemCashVoucherRequestDTO request)
    {
        // Get all active vouchers with this code (regardless of InUse flag)
        var activeVouchers = await _repository.GetByCodeAsync(code, onlyActives: true);

        if (activeVouchers.Count == 0)
        {
            return new List<CashVoucherDTO>();
        }

        // Set redemption data
        var redemptionDate = request.RedemptionDate ?? DateTime.UtcNow;
        
        foreach (var voucher in activeVouchers)
        {
            voucher.RedemptionDate = redemptionDate;
            voucher.RedemptionSaleId = request.RedemptionSaleId;
            voucher.InUse = false; // Always set InUse to false after redemption
        }

        // Update in repository
        await _repository.UpdateAsync(activeVouchers);

        // Return updated DTOs
        return activeVouchers.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Sets the InUse flag for all vouchers with the specified code
    /// </summary>
    /// <param name="code">The voucher code</param>
    /// <param name="inUse">The value to set for the InUse flag</param>
    /// <returns>A list of updated cash voucher DTOs</returns>
    public async Task<List<CashVoucherDTO>> SetCashVouchersInUseAsync(string code, bool inUse)
    {
        var updatedVouchers = await _repository.SetInUseAsync(code, inUse);
        return updatedVouchers.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Deletes all vouchers from the database
    /// WARNING: This operation cannot be undone
    /// </summary>
    /// <returns>The number of vouchers deleted</returns>
    public async Task<int> DeleteAllVouchersAsync()
    {
        return await _repository.DeleteAllVouchersAsync();
    }

    /// <summary>
    /// Maps a CashVoucher entity to a CashVoucherDTO
    /// </summary>
    /// <param name="voucher">The voucher entity</param>
    /// <returns>The voucher DTO</returns>
    private static CashVoucherDTO MapToDto(CashVoucher voucher)
    {
        return new CashVoucherDTO
        {
            Code = voucher.Code,
            Amount = voucher.Amount,
            CreationDate = voucher.CreationDate,
            IssuingStoreId = voucher.IssuingStoreId,
            RedemptionDate = voucher.RedemptionDate,
            ExpirationDate = voucher.ExpirationDate,
            IssuingSaleId = voucher.IssuingSaleId,
            RedemptionSaleId = voucher.RedemptionSaleId,
            InUse = voucher.InUse,
            Status = voucher.Status
        };
    }
}
