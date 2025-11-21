using Microsoft.Data.Sqlite;
using DashboardServer.Models;

namespace DashboardServer.Services;

public partial class DashboardService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IConfiguration configuration, ILogger<DashboardService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// 診療科マスタを取得（表示対象のみ、SEQ順）
    /// </summary>
    public async Task<List<Department>> GetDepartmentsAsync()
    {
        try
        {
            var dbPath = _configuration["SqliteConnection:DatabasePath"];
            var connectionString = $"Data Source={dbPath}";

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 診療科ID, 診療科名, SEQ, isDisplay, Color
                FROM 診療科
                WHERE isDisplay = 1
                ORDER BY SEQ, 診療科ID";

            using var command = new SqliteCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var departments = new List<Department>();
            while (await reader.ReadAsync())
            {
                departments.Add(new Department
                {
                    DepartmentId = reader.GetString(0),
                    DepartmentName = reader.GetString(1),
                    Seq = reader.GetInt32(2),
                    IsDisplay = reader.GetInt32(3) == 1,
                    Color = reader.IsDBNull(4) ? null : reader.GetString(4)
                });
            }

            return departments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "診療科マスタの取得中にエラーが発生しました。");
            return new List<Department>();
        }
    }

    /// <summary>
    /// 病棟マスタを取得（表示対象のみ、SEQ順）
    /// </summary>
    public async Task<List<Ward>> GetWardsAsync()
    {
        try
        {
            var dbPath = _configuration["SqliteConnection:DatabasePath"];
            var connectionString = $"Data Source={dbPath}";

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 病棟ID, 病棟名, SEQ, isDisplay, Color
                FROM 病棟
                WHERE isDisplay = 1
                ORDER BY SEQ, 病棟ID";

            using var command = new SqliteCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var wards = new List<Ward>();
            while (await reader.ReadAsync())
            {
                wards.Add(new Ward
                {
                    WardId = reader.GetString(0),
                    WardName = reader.GetString(1),
                    Seq = reader.GetInt32(2),
                    IsDisplay = reader.GetInt32(3) == 1,
                    Color = reader.IsDBNull(4) ? null : reader.GetString(4)
                });
            }

            return wards;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "病棟マスタの取得中にエラーが発生しました。");
            return new List<Ward>();
        }
    }

    public async Task<DashboardData> GetDashboardDataAsync()
    {
        try
        {
            var dbPath = _configuration["SqliteConnection:DatabasePath"];
            var connectionString = $"Data Source={dbPath}";

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            // 最新の入院患者数を診療科別に集計（表示対象のみ、SEQ順）
            var query = @"
                SELECT 
                    s.診療科名 as Category,
                    SUM(i.入院患者数) as Value
                FROM 入院患者 i
                INNER JOIN 診療科 s ON i.診療科ID = s.診療科ID
                WHERE i.年月日 = (SELECT MAX(年月日) FROM 入院患者)
                  AND s.isDisplay = 1
                GROUP BY s.診療科名, s.SEQ
                ORDER BY s.SEQ, s.診療科名";

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
        // 診療科テーブル（拡張版）
        var createDepartmentTable = @"
            CREATE TABLE IF NOT EXISTS 診療科 (
                診療科ID TEXT PRIMARY KEY,
                診療科名 TEXT NOT NULL,
                SEQ INTEGER NOT NULL DEFAULT 0,
                isDisplay INTEGER NOT NULL DEFAULT 1,
                Color TEXT
            )";

        // 病棟テーブル
        var createWardTable = @"
            CREATE TABLE IF NOT EXISTS 病棟 (
                病棟ID TEXT PRIMARY KEY,
                病棟名 TEXT NOT NULL,
                SEQ INTEGER NOT NULL DEFAULT 0,
                isDisplay INTEGER NOT NULL DEFAULT 1,
                Color TEXT
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

        using (var command = new SqliteCommand(createWardTable, connection))
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

        // インデックスの作成
        var createIndexes = @"
            CREATE INDEX IF NOT EXISTS idx_診療科_SEQ ON 診療科(SEQ);
            CREATE INDEX IF NOT EXISTS idx_診療科_isDisplay ON 診療科(isDisplay);
            CREATE INDEX IF NOT EXISTS idx_病棟_SEQ ON 病棟(SEQ);
            CREATE INDEX IF NOT EXISTS idx_病棟_isDisplay ON 病棟(isDisplay);";

        using (var command = new SqliteCommand(createIndexes, connection))
        {
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task InsertInitialDataAsync(SqliteConnection connection)
    {
        // 診療科データ挿入
        var insertDepartments = @"
            INSERT INTO 診療科 (診療科ID, 診療科名, SEQ, isDisplay, Color) VALUES 
            ('01', '内科', 1, 1, '#ef4444'),
            ('02', '小児科', 2, 1, '#3b82f6'),
            ('03', '整形外科', 3, 1, '#f59e0b')";

        using (var command = new SqliteCommand(insertDepartments, connection))
        {
            await command.ExecuteNonQueryAsync();
        }

        // 病棟データ挿入（サンプル）
        var insertWards = @"
            INSERT OR IGNORE INTO 病棟 (病棟ID, 病棟名, SEQ, isDisplay, Color) VALUES 
            ('01', '一般病棟', 1, 1, '#10b981'),
            ('02', '小児病棟', 2, 1, '#06b6d4'),
            ('03', 'ICU', 3, 1, '#8b5cf6')";

        using (var command = new SqliteCommand(insertWards, connection))
        {
            await command.ExecuteNonQueryAsync();
        }
    }
}
