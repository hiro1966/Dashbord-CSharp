# 日次バッチデータインポート

Oracle DBからSQLiteへデータをインポートする日次バッチのサンプルです。

## 📋 概要

このバッチは以下の処理を行います：
1. Oracle DBから最新データを取得
2. SQLiteデータベースにデータを登録/更新
3. 実行ログを記録

## 📁 ファイル構成

```
BatchImport/
├── README.md              # このファイル
├── DataImport.csproj      # プロジェクトファイル
├── Program.cs             # メイン処理
├── run-import.bat         # 実行バッチファイル
└── appsettings.json       # 設定ファイル
```

## ⚙️ 設定ファイル (appsettings.json)

```json
{
  "OracleConnection": {
    "DataSourceName": "ORD11",
    "UserId": "your_oracle_user",
    "Password": "your_oracle_password"
  },
  "SqliteConnection": {
    "DatabasePath": "../dashboard.db"
  },
  "ImportSettings": {
    "SourceTableName": "YourOracleTable",
    "LogPath": "logs"
  }
}
```

## 🚀 実行方法

### 手動実行
```cmd
cd BatchImport
run-import.bat
```

### タスクスケジューラーで自動実行

1. タスクスケジューラーを開く
2. 「基本タスクの作成」をクリック
3. 以下を設定：
   - 名前: ダッシュボードデータインポート
   - トリガー: 毎日 午前2時（任意の時刻）
   - 操作: プログラムの起動
   - プログラム/スクリプト: `C:\Path\To\BatchImport\run-import.bat`
   - 開始: `C:\Path\To\BatchImport`

## 📊 データ形式

### Oracleソーステーブルの例
```sql
CREATE TABLE SourceData (
    Category VARCHAR2(50),
    Value NUMBER,
    RecordDate DATE
);
```

### SQLite ターゲットテーブル
```sql
CREATE TABLE DashboardData (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Category TEXT NOT NULL,
    Value INTEGER NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

## 🔄 カスタマイズ

`Program.cs` を編集して、独自のデータ取得・変換ロジックを実装してください。

## 📝 ログ

実行ログは `logs` ディレクトリに日付ごとに保存されます。
- ファイル名形式: `import_YYYYMMDD.log`
- 保存内容: 開始時刻、処理件数、エラー情報、終了時刻

## 🆘 トラブルシューティング

### エラー: Oracle接続失敗
- ODBCデータソース `ORD11` が正しく設定されているか確認
- appsettings.json のユーザーID/パスワードが正しいか確認

### エラー: SQLite書き込み失敗
- dashboard.db ファイルが存在するか確認
- ファイルのアクセス権限を確認
- 他のプロセスがファイルをロックしていないか確認
