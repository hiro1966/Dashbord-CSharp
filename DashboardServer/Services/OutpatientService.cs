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
        
        // 表示対象の診療科のみを取得（SEQ順）
        var query = $@"
            SELECT 
                {groupByClause} as Period,
                s.診療科名,
                s.SEQ,
                s.Color,
                SUM(o.患者数) as Total
            FROM 外来患者 o
            INNER JOIN 診療科 s ON o.診療科ID = s.診療科ID
            WHERE o.年月日 BETWEEN @StartDate AND @EndDate
              AND s.isDisplay = 1
            GROUP BY {groupByClause}, s.診療科名, s.SEQ, s.Color
            ORDER BY {groupByClause}, s.SEQ, s.診療科名";

        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@StartDate", startDate);
        command.Parameters.AddWithValue("@EndDate", endDate);

        var tempData = new Dictionary<string, Dictionary<string, int>>();
        var labels = new List<string>();
        var departments = new List<(string name, int seq, string? color)>();

        // データを一度に取得して整形
        using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var periodLabel = reader.GetString(0);
                var deptName = reader.GetString(1);
                var seq = reader.GetInt32(2);
                var color = reader.IsDBNull(3) ? null : reader.GetString(3);
                var value = reader.GetInt32(4);

                if (!labels.Contains(periodLabel))
                {
                    labels.Add(periodLabel);
                }

                if (!departments.Any(d => d.name == deptName))
                {
                    departments.Add((deptName, seq, color));
                }

                if (!tempData.ContainsKey(periodLabel))
                {
                    tempData[periodLabel] = new Dictionary<string, int>();
                }
                tempData[periodLabel][deptName] = value;
            }
        }

        // SEQ順にソート
        departments = departments.OrderBy(d => d.seq).ToList();

        // Datasetを構築（マスタから取得した色を使用）
        var datasets = new List<DatasetInfo>();
        foreach (var dept in departments)
        {
            var values = new List<int>();
            foreach (var label in labels)
            {
                values.Add(tempData.ContainsKey(label) && tempData[label].ContainsKey(dept.name) 
                    ? tempData[label][dept.name] 
                    : 0);
            }

            // Colorフィールドから色を取得、なければデフォルト
            var hexColor = dept.color ?? "#8b5cf6";
            var rgbColor = HexToRgb(hexColor);
            var borderColor = $"rgb({rgbColor.r}, {rgbColor.g}, {rgbColor.b})";
            var bgColor = $"rgba({rgbColor.r}, {rgbColor.g}, {rgbColor.b}, 0.5)";

            datasets.Add(new DatasetInfo
            {
                Label = dept.name,
                Data = values,
                BorderColor = borderColor,
                BackgroundColor = bgColor,
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

    /// <summary>
    /// 16進数カラーコードをRGBに変換
    /// </summary>
    private (int r, int g, int b) HexToRgb(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length == 6)
        {
            return (
                Convert.ToInt32(hex.Substring(0, 2), 16),
                Convert.ToInt32(hex.Substring(2, 2), 16),
                Convert.ToInt32(hex.Substring(4, 2), 16)
            );
        }
        return (139, 92, 246); // デフォルト: purple-500
    }

    private async Task<OutpatientChartData> GetOutpatientSingleDepartmentAsync(
        SqliteConnection connection, 
        string department, 
        string period, 
        string startDate, 
        string endDate)
    {
        var groupByClause = GetGroupByClause(period);
        
        // 診療科の色をマスタから取得
        var colorQuery = "SELECT Color FROM 診療科 WHERE 診療科名 = @Department";
        using var colorCmd = new SqliteCommand(colorQuery, connection);
        colorCmd.Parameters.AddWithValue("@Department", department);
        var colorResult = await colorCmd.ExecuteScalarAsync();
        var hexColor = colorResult?.ToString() ?? "#8b5cf6";
        
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

        // マスタから取得した色を使用
        var rgb = HexToRgb(hexColor);
        var borderColor = $"rgb({rgb.r}, {rgb.g}, {rgb.b})";
        var bgColor = $"rgba({rgb.r}, {rgb.g}, {rgb.b}, 0.2)";

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
                    BorderColor = borderColor,
                    BackgroundColor = bgColor,
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
