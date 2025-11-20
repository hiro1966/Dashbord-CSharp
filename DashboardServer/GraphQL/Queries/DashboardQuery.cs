using DashboardServer.Models;
using DashboardServer.Services;

namespace DashboardServer.GraphQL.Queries;

public class DashboardQuery
{
    /// <summary>
    /// 入院患者データ取得
    /// </summary>
    public async Task<DashboardData> GetInpatientData([Service] DashboardService service)
    {
        return await service.GetDashboardDataAsync();
    }

    /// <summary>
    /// 外来患者データ取得
    /// </summary>
    public async Task<OutpatientChartData> GetOutpatientData(
        [Service] DashboardService service,
        string department = "全科",
        string period = "日毎",
        string? startDate = null,
        string? endDate = null)
    {
        return await service.GetOutpatientChartDataAsync(department, period, startDate, endDate);
    }
}
