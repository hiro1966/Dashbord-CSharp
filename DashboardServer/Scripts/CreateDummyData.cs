using Microsoft.Data.Sqlite;

namespace DashboardServer.Scripts;

/// <summary>
/// ダミーデータ作成スクリプト
/// </summary>
public class CreateDummyData
{
    private static readonly string[] Departments = { "01", "02", "03" };
    private static readonly string[] Wards = { "3階病棟", "4階病棟", "5階病棟", "6階病棟" };
    private static readonly Random random = new Random(123); // 固定シード

    public static async Task Main(string[] args)
    {
        var dbPath = args.Length > 0 ? args[0] : "dashboard.db";
        var connectionString = $"Data Source={dbPath}";

        Console.WriteLine("=== ダミーデータ作成開始 ===");
        Console.WriteLine($"データベース: {dbPath}");

        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        // テーブル作成
        await CreateTablesAsync(connection);

        // 既存データを削除
        await ClearDataAsync(connection);

        // 診療科データ挿入
        await InsertDepartmentsAsync(connection);

        // 入院患者データ挿入（2025-01-01 ～ 2025-10-31）
        await InsertInpatientDataAsync(connection);

        // 外来患者データ挿入（2025-01-01 ～ 2025-10-31）
        await InsertOutpatientDataAsync(connection);

        Console.WriteLine("=== ダミーデータ作成完了 ===");
    }

    private static async Task CreateTablesAsync(SqliteConnection connection)
    {
        Console.WriteLine("テーブル作成中...");

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

        Console.WriteLine("テーブル作成完了");
    }

    private static async Task ClearDataAsync(SqliteConnection connection)
    {
        Console.WriteLine("既存データを削除中...");

        var tables = new[] { "外来患者", "入院患者" };
        foreach (var table in tables)
        {
            var query = $"DELETE FROM {table}";
            using var command = new SqliteCommand(query, connection);
            await command.ExecuteNonQueryAsync();
        }

        Console.WriteLine("既存データ削除完了");
    }

    private static async Task InsertDepartmentsAsync(SqliteConnection connection)
    {
        Console.WriteLine("診療科データ挿入中...");

        var checkQuery = "SELECT COUNT(*) FROM 診療科";
        using var checkCommand = new SqliteCommand(checkQuery, connection);
        var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

        if (count == 0)
        {
            var query = @"
                INSERT INTO 診療科 (診療科ID, 診療科名) VALUES 
                ('01', '内科'),
                ('02', '小児科'),
                ('03', '整形外科')";

            using var command = new SqliteCommand(query, connection);
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("診療科データ挿入完了: 3件");
        }
        else
        {
            Console.WriteLine($"診療科データ既存: {count}件");
        }
    }

    private static async Task InsertInpatientDataAsync(SqliteConnection connection)
    {
        Console.WriteLine("入院患者データ挿入中...");

        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 10, 31);
        var insertCount = 0;

        using var transaction = connection.BeginTransaction();

        try
        {
            var query = @"
                INSERT INTO 入院患者 (年月日, 診療科ID, 病棟, 入院患者数, 退院患者数, 転入患者数, 転出患者数)
                VALUES (@年月日, @診療科ID, @病棟, @入院患者数, @退院患者数, @転入患者数, @転出患者数)";

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                foreach (var deptId in Departments)
                {
                    foreach (var ward in Wards)
                    {
                        using var command = new SqliteCommand(query, connection, transaction);
                        command.Parameters.AddWithValue("@年月日", date.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@診療科ID", deptId);
                        command.Parameters.AddWithValue("@病棟", ward);
                        command.Parameters.AddWithValue("@入院患者数", random.Next(5, 25));
                        command.Parameters.AddWithValue("@退院患者数", random.Next(0, 8));
                        command.Parameters.AddWithValue("@転入患者数", random.Next(0, 5));
                        command.Parameters.AddWithValue("@転出患者数", random.Next(0, 5));

                        await command.ExecuteNonQueryAsync();
                        insertCount++;
                    }
                }
            }

            transaction.Commit();
            Console.WriteLine($"入院患者データ挿入完了: {insertCount}件");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.WriteLine($"エラー: {ex.Message}");
            throw;
        }
    }

    private static async Task InsertOutpatientDataAsync(SqliteConnection connection)
    {
        Console.WriteLine("外来患者データ挿入中...");

        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 10, 31);
        var insertCount = 0;

        using var transaction = connection.BeginTransaction();

        try
        {
            var query = @"
                INSERT INTO 外来患者 (年月日, 診療科ID, 初再診, 患者数)
                VALUES (@年月日, @診療科ID, @初再診, @患者数)";

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                foreach (var deptId in Departments)
                {
                    // 初診
                    using (var command = new SqliteCommand(query, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@年月日", date.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@診療科ID", deptId);
                        command.Parameters.AddWithValue("@初再診", 0);
                        command.Parameters.AddWithValue("@患者数", random.Next(10, 40));

                        await command.ExecuteNonQueryAsync();
                        insertCount++;
                    }

                    // 再診
                    using (var command = new SqliteCommand(query, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@年月日", date.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@診療科ID", deptId);
                        command.Parameters.AddWithValue("@初再診", 1);
                        command.Parameters.AddWithValue("@患者数", random.Next(30, 80));

                        await command.ExecuteNonQueryAsync();
                        insertCount++;
                    }
                }
            }

            transaction.Commit();
            Console.WriteLine($"外来患者データ挿入完了: {insertCount}件");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.WriteLine($"エラー: {ex.Message}");
            throw;
        }
    }
}
