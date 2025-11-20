using Microsoft.Data.Sqlite;
using DashboardServer.Models;

namespace DashboardServer.Services;

public partial class DashboardService
{
    public async Task<OutpatientChartData> GetOutpatientChartDataAsync(
        string department, 
        string period, 
        string? startDate, 
        string? endDate)
    {
        try
        {
            var dbPath = _configuration["SqliteConnection:DatabasePath"];
            var connectionString = $"Data Source={dbPath}";

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            // デフォルトの日付範囲を設定
            if (string.IsNullOrEmpty(startDate))
            {
                var minDateQuery = "SELECT MIN(年月日) FROM 外来患者";
                using var minCmd = new SqliteCommand(minDateQuery, connection);
                startDate = (await minCmd.ExecuteScalarAsync())?.ToString() ?? "2025-01-01";
            }

            if (string.IsNullOrEmpty(endDate))
            {
                var maxDateQuery = "SELECT MAX(年月日) FROM 外来患者";
                using var maxCmd = new SqliteCommand(maxDateQuery, connection);
                endDate = (await maxCmd.ExecuteScalarAsync())?.ToString() ?? "2025-10-31";
            }

            return department switch
            {
                "全科(色分)" => await GetOutpatientByDepartmentAsync(connection, period, startDate, endDate),
                "全科" => await GetOutpatientTotalAsync(connection, period, startDate, endDate),
                _ => await GetOutpatientSingleDepartmentAsync(connection, department, period, startDate, endDate)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "外来患者データの取得中にエラーが発生しました。");
            throw;
        }
    }

    private async Task<OutpatientChartData> GetOutpatientTotalAsync(
        SqliteConnection connection, 
        string period, 
        string startDate, 
        string endDate)
    {
        var groupByClause = GetGroupByClause(period);
        var query = $@"
            SELECT 
                {groupByClause} as Period,
                SUM(患者数) as Total
            FROM 外来患者
            WHERE 年月日 BETWEEN @StartDate AND @EndDate
            GROUP BY {groupByClause}
            ORDER BY {groupByClause}";

        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@StartDate", startDate);
        command.Parameters.AddWithValue("@EndDate", endDate);

        var labels = new List<string>();
        var values = new List<int>();

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            labels.Add(reader.GetString(0));
            values.Add(reader.GetInt32(1));
        }

        return new OutpatientChartData
        {
            Title = $"外来患者数（全科合計）",
            Labels = labels,
            Datasets = new List<DatasetInfo>
            {
                new DatasetInfo
                {
                    Label = "全科",
                    Data = values,
                    BorderColor = "rgb(75, 192, 192)",
                    BackgroundColor = "rgba(75, 192, 192, 0.2)",
                    Fill = false
                }
            }
        };
    }

    private async Task<OutpatientChartData> GetOutpatientByDepartmentAsync(
        SqliteConnection connection, 
        string period, 
        string startDate, 
        string endDate)
    {
        var groupByClause = GetGroupByClause(period);
        var query = $@"
            SELECT 
                {groupByClause} as Period,
                s.診療科名,
                SUM(o.患者数) as Total
            FROM 外来患者 o
            INNER JOIN 診療科 s ON o.診療科ID = s.診療科ID
            WHERE o.年月日 BETWEEN @StartDate AND @EndDate
            GROUP BY {groupByClause}, s.診療科名
            ORDER BY {groupByClause}, s.診療科名";

        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@StartDate", startDate);
        command.Parameters.AddWithValue("@EndDate", endDate);

        var dataDict = new Dictionary<string, List<int>>();
        var labels = new List<string>();
        var departments = new List<string>();

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var periodLabel = reader.GetString(0);
            var deptName = reader.GetString(1);
            var value = reader.GetInt32(2);

            if (!labels.Contains(periodLabel))
            {
                labels.Add(periodLabel);
            }

            if (!dataDict.ContainsKey(deptName))
            {
                dataDict[deptName] = new List<int>();
                departments.Add(deptName);
            }
        }

        // データを再取得して整形
        using var reader2 = await command.ExecuteReaderAsync();
        var tempData = new Dictionary<string, Dictionary<string, int>>();

        while (await reader2.ReadAsync())
        {
            var periodLabel = reader2.GetString(0);
            var deptName = reader2.GetString(1);
            var value = reader2.GetInt32(2);

            if (!tempData.ContainsKey(periodLabel))
            {
                tempData[periodLabel] = new Dictionary<string, int>();
            }
            tempData[periodLabel][deptName] = value;
        }

        // Datasetを構築
        var colors = new Dictionary<string, (string border, string bg)>
        {
            ["内科"] = ("rgb(255, 99, 132)", "rgba(255, 99, 132, 0.5)"),
            ["小児科"] = ("rgb(54, 162, 235)", "rgba(54, 162, 235, 0.5)"),
            ["整形外科"] = ("rgb(255, 206, 86)", "rgba(255, 206, 86, 0.5)")
        };

        var datasets = new List<DatasetInfo>();
        foreach (var dept in departments)
        {
            var values = new List<int>();
            foreach (var label in labels)
            {
                values.Add(tempData.ContainsKey(label) && tempData[label].ContainsKey(dept) 
                    ? tempData[label][dept] 
                    : 0);
            }

            var color = colors.ContainsKey(dept) 
                ? colors[dept] 
                : ("rgb(201, 203, 207)", "rgba(201, 203, 207, 0.5)");

            datasets.Add(new DatasetInfo
            {
                Label = dept,
                Data = values,
                BorderColor = color.Item1,
                BackgroundColor = color.Item2,
                Fill = true
            });
        }

        return new OutpatientChartData
        {
            Title = "外来患者数（診療科別）",
            Labels = labels,
            Datasets = datasets
        };
    }

    private async Task<OutpatientChartData> GetOutpatientSingleDepartmentAsync(
        SqliteConnection connection, 
        string department, 
        string period, 
        string startDate, 
        string endDate)
    {
        var groupByClause = GetGroupByClause(period);
        var query = $@"
            SELECT 
                {groupByClause} as Period,
                SUM(o.患者数) as Total
            FROM 外来患者 o
            INNER JOIN 診療科 s ON o.診療科ID = s.診療科ID
            WHERE s.診療科名 = @Department
              AND o.年月日 BETWEEN @StartDate AND @EndDate
            GROUP BY {groupByClause}
            ORDER BY {groupByClause}";

        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@Department", department);
        command.Parameters.AddWithValue("@StartDate", startDate);
        command.Parameters.AddWithValue("@EndDate", endDate);

        var labels = new List<string>();
        var values = new List<int>();

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            labels.Add(reader.GetString(0));
            values.Add(reader.GetInt32(1));
        }

        var color = department switch
        {
            "内科" => ("rgb(255, 99, 132)", "rgba(255, 99, 132, 0.2)"),
            "小児科" => ("rgb(54, 162, 235)", "rgba(54, 162, 235, 0.2)"),
            "整形外科" => ("rgb(255, 206, 86)", "rgba(255, 206, 86, 0.2)"),
            _ => ("rgb(75, 192, 192)", "rgba(75, 192, 192, 0.2)")
        };

        return new OutpatientChartData
        {
            Title = $"外来患者数（{department}）",
            Labels = labels,
            Datasets = new List<DatasetInfo>
            {
                new DatasetInfo
                {
                    Label = department,
                    Data = values,
                    BorderColor = color.Item1,
                    BackgroundColor = color.Item2,
                    Fill = false
                }
            }
        };
    }

    private string GetGroupByClause(string period)
    {
        return period switch
        {
            "年毎" => "strftime('%Y', 年月日)",
            "月毎" => "strftime('%Y-%m', 年月日)",
            _ => "年月日" // 日毎
        };
    }
}
