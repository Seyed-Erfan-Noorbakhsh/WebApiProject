using Microsoft.AspNetCore.Mvc;
using Shop_ProjForWeb.Core.Application.Services;

namespace Shop_ProjForWeb.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController(ReportingService reportingService) : ControllerBase
{
    private readonly ReportingService _reportingService = reportingService;

    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var report = await _reportingService.GetSalesSummaryAsync(startDate, endDate);
            return Ok(report);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving sales summary", details = ex.Message });
        }
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventoryReport()
    {
        try
        {
            var report = await _reportingService.GetInventoryReportAsync();
            return Ok(report);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving inventory report", details = ex.Message });
        }
    }

    [HttpGet("user/{userId}/spending")]
    public async Task<IActionResult> GetUserSpendingReport(Guid userId)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                return BadRequest(new { error = "User ID is required" });
            }

            var report = await _reportingService.GetUserSpendingReportAsync(userId);
            return Ok(report);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving user spending report", details = ex.Message });
        }
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int limit = 10)
    {
        try
        {
            if (limit <= 0)
            {
                return BadRequest(new { error = "Limit must be greater than 0" });
            }

            var products = await _reportingService.GetTopProductsAsync(limit);
            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving top products", details = ex.Message });
        }
    }

    [HttpGet("users/spending")]
    public async Task<IActionResult> GetAllUsersSpendingReport()
    {
        try
        {
            var users = await _reportingService.GetAllUsersSpendingReportAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving users spending report", details = ex.Message });
        }
    }

    [HttpGet("order-status-distribution")]
    public async Task<IActionResult> GetOrderStatusDistribution()
    {
        try
        {
            var distribution = await _reportingService.GetOrderStatusDistributionAsync();
            return Ok(distribution);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving order status distribution", details = ex.Message });
        }
    }
}
