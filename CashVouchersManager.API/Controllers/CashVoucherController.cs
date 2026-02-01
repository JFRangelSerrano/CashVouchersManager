using Microsoft.AspNetCore.Mvc;
using CashVouchersManager.Application.Interfaces;
using CashVouchersManager.DTO.DTOs;
using CashVouchersManager.DTO.Enums;

namespace CashVouchersManager.API.Controllers;

/// <summary>
/// API controller for cash voucher operations
/// </summary>
[ApiController]
[Route("api")]
public class CashVoucherController : ControllerBase
{
    private readonly ICashVoucherService _cashVoucherService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CashVoucherController"/> class
    /// </summary>
    /// <param name="cashVoucherService">The cash voucher service</param>
    public CashVoucherController(ICashVoucherService cashVoucherService)
    {
        _cashVoucherService = cashVoucherService;
    }

    /// <summary>
    /// Generates a new cash voucher
    /// </summary>
    /// <param name="request">The request data for generating the voucher</param>
    /// <returns>The generated cash voucher</returns>
    [HttpPost("GenerateCashVoucher")]
    public async Task<ActionResult<CashVoucherDTO>> GenerateCashVoucher(
        [FromBody] GenerateCashVoucherRequestDTO request)
    {
        var result = await _cashVoucherService.GenerateCashVoucherAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Gets cash vouchers by code
    /// </summary>
    /// <param name="code">The voucher code</param>
    /// <param name="onlyActives">If true, returns only active vouchers (default: true)</param>
    /// <returns>A list of cash vouchers matching the code</returns>
    [HttpGet("GetCashVoucherByCode/{code}")]
    public async Task<ActionResult<List<CashVoucherDTO>>> GetCashVoucherByCode(
        string code,
        [FromQuery] bool onlyActives = true)
    {
        var result = await _cashVoucherService.GetCashVoucherByCodeAsync(code, onlyActives);
        return Ok(result);
    }

    /// <summary>
    /// Gets filtered cash vouchers based on criteria
    /// </summary>
    /// <param name="status">The status filter</param>
    /// <param name="issuingStoreId">The issuing store ID filter</param>
    /// <param name="dateFrom">The start date filter</param>
    /// <param name="dateTo">The end date filter</param>
    /// <param name="dateType">The date type to filter by (default: Creation)</param>
    /// <returns>A list of filtered cash vouchers</returns>
    [HttpGet("GetFilteredCashVouchers")]
    public async Task<ActionResult<List<CashVoucherDTO>>> GetFilteredCashVouchers(
        [FromQuery] CashVoucherStatusEnum? status = null,
        [FromQuery] int? issuingStoreId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] CashVoucherDateTypeEnum dateType = CashVoucherDateTypeEnum.Creation)
    {
        var result = await _cashVoucherService.GetFilteredCashVouchersAsync(
            status, 
            issuingStoreId, 
            dateFrom, 
            dateTo, 
            dateType);
        
        return Ok(result);
    }

    /// <summary>
    /// Redeems all active cash vouchers with the specified code
    /// </summary>
    /// <param name="code">The voucher code</param>
    /// <param name="request">The redemption request data</param>
    /// <returns>A list of redeemed cash vouchers</returns>
    [HttpPut("RedeemCashVoucher/{code}")]
    public async Task<ActionResult<List<CashVoucherDTO>>> RedeemCashVoucher(
        string code,
        [FromBody] RedeemCashVoucherRequestDTO request)
    {
        var result = await _cashVoucherService.RedeemCashVoucherAsync(code, request);
        
        if (result.Count == 0)
        {
            return NotFound(new { message = "No active vouchers found with the specified code" });
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Sets the InUse flag for vouchers with the specified code
    /// When InUse is true: only updates non-redeemed, non-expired vouchers
    /// When InUse is false: updates all vouchers with the code
    /// </summary>
    /// <param name="code">The voucher code</param>
    /// <param name="request">The request data with the InUse value</param>
    /// <returns>A list of updated cash vouchers</returns>
    [HttpPost("SetCashVouchersInUse/{code}")]
    public async Task<ActionResult<List<CashVoucherDTO>>> SetCashVouchersInUse(
        string code,
        [FromBody] SetInUseRequestDTO request)
    {
        var result = await _cashVoucherService.SetCashVouchersInUseAsync(code, request.InUse);
        
        if (result.Count == 0)
        {
            return NotFound(new { message = "No vouchers found with the specified code" });
        }
            
        return Ok(result);
    }
}
