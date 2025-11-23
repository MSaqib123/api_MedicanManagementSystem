using MedicineManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_MedicanManagementSystem.Controllers;



[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
[Authorize(Policy = "Accountant")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("daily-sales/{branchId}")]
    public async Task<IActionResult> GetDailySales(Guid branchId, [FromQuery] DateTime date)
    {
        var report = await _analyticsService.GetDailySalesReportAsync(branchId, date);
        return Ok(report);
    }

    [HttpGet("stock-aging/{branchId}")]
    public async Task<IActionResult> GetStockAging(Guid branchId)
    {
        var report = await _analyticsService.GetStockAgingReportAsync(branchId);
        return Ok(report);
    }

    [HttpGet("profit-loss/{branchId}")]
    public async Task<IActionResult> GetProfitLoss(Guid branchId, [FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var report = await _analyticsService.GetProfitLossStatementAsync(branchId, start, end);
        return Ok(report);
    }

    [HttpGet("dashboard/{tenantId}")]
    public async Task<IActionResult> GetDashboard(Guid tenantId)
    {
        var dashboard = await _analyticsService.GetRealtimeDashboardAsync(tenantId);
        return Ok(dashboard);
    }

    [HttpGet("supplier/{supplierId}")]
    public async Task<IActionResult> GetSupplierAnalytics(Guid supplierId)
    {
        var analytics = await _analyticsService.GetSupplierAnalyticsAsync(supplierId);
        return Ok(analytics);
    }

    [HttpGet("medicine/{medicineId}")]
    public async Task<IActionResult> GetMedicinePerformance(Guid medicineId)
    {
        var performance = await _analyticsService.GetMedicinePerformanceAsync(medicineId);
        return Ok(performance);
    }
}
