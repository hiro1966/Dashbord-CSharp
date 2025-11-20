using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace DataImport;

class Program
{
    private static IConfiguration? _configuration;
    private static string _logPath = "logs";

    static async Task<int> Main(string[] args)
    {
        var startTime = DateTime.Now;
        LogMessage($"=== データインポート開始 ===");

        try
        {
            // 設定ファイル読み込み
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            _logPath = _configuration["ImportSettings:LogPath"] ?? "logs";
            
            // ログディレクトリ作成
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }

            // データインポート実行
            var importCount = await ImportDataAsync();

            var endTime = DateTime.Now;
            var duration = endTime - startTime;

            LogMessage($"インポート完了: {importCount}件");
            LogMessage($"処理時間: {duration.TotalSeconds:F2}秒");
            LogMessage($"=== データインポート終了 ===");

            return 0; // 成功
        }
        catch (Exception ex)
        {
            LogMessage($"エラー: {ex.Message}");
            LogMessage($"スタックトレース: {ex.StackTrace}");
            return 1; // エラー
        }
    }

    static async Task<int> ImportDataAsync()
    {
        if (_configuration == null)
            throw new InvalidOperationException("設定が読み込まれていません。");

        // SQLite接続情報
        var sqliteDbPath = _configuration["SqliteConnection:DatabasePath"];

        LogMessage($"SQLite DB: {sqliteDbPath}");

        var sqliteConnectionString = $"Data Source={sqliteDbPath}";
        var importCount = 0;

        // サンプルデータ（実際にはCSVファイルやその他のソースから取得）
        LogMessage("サンプルデータを準備しています...");
        var dataList = new List<(string Category, int Value)>
        {
            ("売上", new Random().Next(1000, 2000)),
            ("顧客数", new Random().Next(200, 400)),
            ("注文数", new Random().Next(300, 500)),
            ("商品数", new Random().Next(50, 100)),
            ("在庫数", new Random().Next(1000, 3000)),
            ("返品数", new Random().Next(10, 50))
        };

        LogMessage($"データ件数: {dataList.Count}件");

        // SQLiteにデータを登録
        if (dataList.Count > 0)
        {
            LogMessage("SQLiteにデータを登録しています...");

            using var sqliteConnection = new SqliteConnection(sqliteConnectionString);
            await sqliteConnection.OpenAsync();

            using var transaction = sqliteConnection.BeginTransaction();

            try
            {
                // 既存データを削除（全削除する場合）
                var deleteQuery = "DELETE FROM DashboardData";
                using var deleteCommand = new SqliteCommand(deleteQuery, sqliteConnection, transaction);
                await deleteCommand.ExecuteNonQueryAsync();
                LogMessage("既存データを削除しました。");

                // データを挿入
                var insertQuery = @"
                    INSERT INTO DashboardData (Category, Value, CreatedAt) 
                    VALUES (@Category, @Value, @CreatedAt)";

                foreach (var data in dataList)
                {
                    using var insertCommand = new SqliteCommand(insertQuery, sqliteConnection, transaction);
                    insertCommand.Parameters.AddWithValue("@Category", data.Category);
                    insertCommand.Parameters.AddWithValue("@Value", data.Value);
                    insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                    await insertCommand.ExecuteNonQueryAsync();
                    importCount++;
                }

                transaction.Commit();
                LogMessage("データの登録が完了しました。");
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        else
        {
            LogMessage("インポートするデータがありません。");
        }

        return importCount;
    }

    static void LogMessage(string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var logMessage = $"[{timestamp}] {message}";

        // コンソール出力
        Console.WriteLine(logMessage);

        // ファイル出力
        try
        {
            var logFileName = $"import_{DateTime.Now:yyyyMMdd}.log";
            var logFilePath = Path.Combine(_logPath, logFileName);
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ログファイル書き込みエラー: {ex.Message}");
        }
    }
}
