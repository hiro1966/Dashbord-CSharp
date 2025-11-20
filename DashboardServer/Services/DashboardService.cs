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

            // サンプルクエリ（実際のテーブル構造に合わせて変更してください）
            var query = "SELECT Category, Value FROM DashboardData ORDER BY Category";
            using var command = new SqliteCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var data = new DashboardData
            {
                Title = "ダッシュボードデータ"
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
                Labels = new List<string> { "1月", "2月", "3月", "4月", "5月", "6月" },
                Values = new List<int> { 12, 19, 3, 5, 2, 3 }
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

            // サンプルテーブルを作成
            var createTableQuery = @"
                CREATE TABLE IF NOT EXISTS DashboardData (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Category TEXT NOT NULL,
                    Value INTEGER NOT NULL,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                )";

            using var command = new SqliteCommand(createTableQuery, connection);
            await command.ExecuteNonQueryAsync();

            // データが存在しない場合はサンプルデータを挿入
            var checkDataQuery = "SELECT COUNT(*) FROM DashboardData";
            using var checkCommand = new SqliteCommand(checkDataQuery, connection);
            var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

            if (count == 0)
            {
                var insertQuery = @"
                    INSERT INTO DashboardData (Category, Value) VALUES 
                    ('1月', 12),
                    ('2月', 19),
                    ('3月', 3),
                    ('4月', 5),
                    ('5月', 2),
                    ('6月', 3)";

                using var insertCommand = new SqliteCommand(insertQuery, connection);
                await insertCommand.ExecuteNonQueryAsync();
                
                _logger.LogInformation("サンプルデータを挿入しました。");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "データベース初期化中にエラーが発生しました。");
        }
    }
}
