using DashboardServer.Models;
using DashboardServer.Services;

namespace DashboardServer.GraphQL.Queries;

public class DashboardQuery
{
    /// <summary>
    /// 診療科マスタ取得
    /// </summary>
    public async Task<List<Department>> GetDepartments([Service] DashboardService service)
    {
        return await service.GetDepartmentsAsync();
    }

    /// <summary>
    /// 病棟マスタ取得
    /// </summary>
    public async Task<List<Ward>> GetWards([Service] DashboardService service)
    {
        return await service.GetWardsAsync();
    }

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
