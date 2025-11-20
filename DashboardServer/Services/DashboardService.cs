using Microsoft.Data.Sqlite;
using DashboardServer.Models;

namespace DashboardServer.Services;

public class DashboardService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IConfiguration configuration, ILogger<DashboardService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<DashboardData> GetDashboardDataAsync()
    {
        try
        {
            var dbPath = _configuration["SqliteConnection:DatabasePath"];
            var connectionString = $"Data Source={dbPath}";

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            // 最新の入院患者数を診療科別に集計
            var query = @"
                SELECT 
                    s.診療科名 as Category,
                    SUM(i.入院患者数) as Value
                FROM 入院患者 i
                INNER JOIN 診療科 s ON i.診療科ID = s.診療科ID
                WHERE i.年月日 = (SELECT MAX(年月日) FROM 入院患者)
                GROUP BY s.診療科名
                ORDER BY s.診療科名";

            using var command = new SqliteCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var data = new DashboardData
            {
                Title = "診療科別入院患者数"
            };

            while (await reader.ReadAsync())
            {
                data.Labels.Add(reader.GetString(0));
                data.Values.Add(reader.GetInt32(1));
            }

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ダッシュボードデータの取得中にエラーが発生しました。");
            
            // エラー時はサンプルデータを返す
            return new DashboardData
            {
                Title = "サンプルデータ（DBエラー）",
                Labels = new List<string> { "内科", "小児科", "整形外科" },
                Values = new List<int> { 45, 28, 33 }
            };
        }
    }

    public async Task InitializeDatabaseAsync()
    {
        try
        {
            var dbPath = _configuration["SqliteConnection:DatabasePath"];
            var connectionString = $"Data Source={dbPath}";

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            // テーブル作成
            await CreateTablesAsync(connection);

            // 診療科データが存在しない場合は初期データを挿入
            var checkQuery = "SELECT COUNT(*) FROM 診療科";
            using var checkCommand = new SqliteCommand(checkQuery, connection);
            var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

            if (count == 0)
            {
                await InsertInitialDataAsync(connection);
                _logger.LogInformation("初期データを挿入しました。");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "データベース初期化中にエラーが発生しました。");
        }
    }

    private async Task CreateTablesAsync(SqliteConnection connection)
    {
        // 診療科テーブル
        var createDepartmentTable = @"
            CREATE TABLE IF NOT EXISTS 診療科 (
                診療科ID TEXT PRIMARY KEY,
                診療科名 TEXT NOT NULL
            )";

        // 入院患者テーブル
        var createInpatientTable = @"
            CREATE TABLE IF NOT EXISTS 入院患者 (
                年月日 TEXT NOT NULL,
                診療科ID TEXT NOT NULL,
                病棟 TEXT NOT NULL,
                入院患者数 INTEGER NOT NULL,
                退院患者数 INTEGER NOT NULL,
                転入患者数 INTEGER NOT NULL,
                転出患者数 INTEGER NOT NULL,
                PRIMARY KEY (年月日, 診療科ID, 病棟),
                FOREIGN KEY (診療科ID) REFERENCES 診療科(診療科ID)
            )";

        // 外来患者テーブル
        var createOutpatientTable = @"
            CREATE TABLE IF NOT EXISTS 外来患者 (
                年月日 TEXT NOT NULL,
                診療科ID TEXT NOT NULL,
                初再診 INTEGER NOT NULL,
                患者数 INTEGER NOT NULL,
                PRIMARY KEY (年月日, 診療科ID, 初再診),
                FOREIGN KEY (診療科ID) REFERENCES 診療科(診療科ID)
            )";

        using (var command = new SqliteCommand(createDepartmentTable, connection))
        {
            await command.ExecuteNonQueryAsync();
        }

        using (var command = new SqliteCommand(createInpatientTable, connection))
        {
            await command.ExecuteNonQueryAsync();
        }

        using (var command = new SqliteCommand(createOutpatientTable, connection))
        {
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task InsertInitialDataAsync(SqliteConnection connection)
    {
        // 診療科データ挿入
        var insertDepartments = @"
            INSERT INTO 診療科 (診療科ID, 診療科名) VALUES 
            ('01', '内科'),
            ('02', '小児科'),
            ('03', '整形外科')";

        using (var command = new SqliteCommand(insertDepartments, connection))
        {
            await command.ExecuteNonQueryAsync();
        }
    }
}
