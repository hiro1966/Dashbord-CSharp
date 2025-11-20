using Microsoft.AspNetCore.Mvc;
using DashboardServer.Models;
using DashboardServer.Services;

namespace DashboardServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(DashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpGet("data")]
    public async Task<ActionResult<DashboardData>> GetData()
    {
        try
        {
            var data = await _dashboardService.GetDashboardDataAsync();
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ダッシュボードデータ取得中にエラーが発生しました。");
            return StatusCode(500, "データの取得に失敗しました。");
        }
    }

    [HttpGet("outpatient")]
    public async Task<ActionResult<OutpatientChartData>> GetOutpatientData(
        [FromQuery] string department = "全科",
        [FromQuery] string period = "日毎",
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null)
    {
        try
        {
            var data = await _dashboardService.GetOutpatientChartDataAsync(department, period, startDate, endDate);
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "外来患者データ取得中にエラーが発生しました。");
            return StatusCode(500, "データの取得に失敗しました。");
        }
    }
}
