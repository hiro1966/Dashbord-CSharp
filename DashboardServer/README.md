# ダッシュボードサーバー

オフラインWindows10環境で動作するダッシュボードシステムです。

## 📋 システム概要

- **サーバー**: ASP.NET Core 8.0 Web API
- **認証**: Oracle DB (ODBC接続) + JWT Token
- **データベース**: SQLite (ダッシュボードデータ用)
- **クライアント**: HTML + JavaScript + Chart.js

## 🔧 前提条件

### サーバー側
- Windows 10 64bit
- .NET 8.0 Runtime (ASP.NET Core含む)
- Oracle ODBC Driver
- Oracle DB データソース名: `ORD11` (ODBC設定済み)

### クライアント側
- Windows 10 64bit
- Microsoft Edge または Google Chrome

## 📦 インストール手順

### 1. .NET 8.0 Runtimeのインストール

1. Microsoft公式サイトから.NET 8.0 Runtime（ASP.NET Core含む）をダウンロード
   - https://dotnet.microsoft.com/download/dotnet/8.0
   - 「ASP.NET Core Runtime 8.0.x - Windows Hosting Bundle」をダウンロード
2. インストーラーを実行

### 2. Oracle ODBCドライバーのインストール

1. Oracle公式サイトからODBC Driverをダウンロード
2. インストーラーを実行
3. ODBCデータソースアドミニストレーター（64bit）で `ORD11` を設定

### 3. アプリケーションの配置

1. プロジェクトをビルドまたは発行
2. 発行したファイル一式を任意のフォルダにコピー
3. `appsettings.json` を編集

## ⚙️ 設定ファイル (appsettings.json)

```json
{
  "JwtSettings": {
    "SecretKey": "YourVeryLongSecretKeyForJWTTokenGeneration123456789",
    "Issuer": "DashboardServer",
    "Audience": "DashboardClient",
    "ExpirationMinutes": 480
  },
  "OracleConnection": {
    "DataSourceName": "ORD11",
    "UserId": "your_oracle_user",
    "Password": "your_oracle_password"
  },
  "Authentication": {
    "StaffLevels": ["Manager", "Admin", "Staff"]
  },
  "SqliteConnection": {
    "DatabasePath": "dashboard.db"
  }
}
```

### 設定項目の説明

#### JwtSettings
- **SecretKey**: JWT署名用の秘密鍵（32文字以上推奨）
- **Issuer**: トークン発行者
- **Audience**: トークン対象者
- **ExpirationMinutes**: トークン有効期限（分）

#### OracleConnection
- **DataSourceName**: ODBC データソース名（デフォルト: ORD11）
- **UserId**: Oracle DBユーザーID
- **Password**: Oracle DBパスワード

#### Authentication
- **StaffLevels**: アクセスを許可するStaffLevelのリスト

#### SqliteConnection
- **DatabasePath**: SQLiteデータベースファイルのパス

## 🚀 起動方法

### 開発環境での起動

```bash
cd DashboardServer
dotnet run
```

### 本番環境での起動

#### 方法1: コマンドラインから起動
```bash
cd DashboardServer
DashboardServer.exe
```

#### 方法2: Windows Serviceとして起動（推奨）
1. 管理者権限でコマンドプロンプトを開く
2. 以下のコマンドを実行

```cmd
sc create DashboardService binPath= "C:\Path\To\DashboardServer.exe" start= auto
sc start DashboardService
```

サービスの削除:
```cmd
sc stop DashboardService
sc delete DashboardService
```

## 🌐 アクセス方法

### サーバー起動後のURL
- **ローカルアクセス**: http://localhost:5000
- **ネットワークアクセス**: http://[サーバーIPアドレス]:5000

### クライアントからのアクセス
1. ブラウザ（Edge/Chrome）を開く
2. `http://[サーバーIPアドレス]:5000` にアクセス
3. ログイン画面が表示される

## 🔐 認証フロー

1. クライアントがユーザーID/パスワードを入力
2. サーバーがOracle DB (`ORD11`) の `UserMaster` テーブルを参照
3. 以下をチェック:
   - ID と Passwd の一致
   - StaffLevel が設定ファイルの StaffLevels に含まれているか
4. 認証成功時、JWT トークンを発行
5. 以降のAPI呼び出しにトークンを使用

## 📊 データベース構造

### Oracle DB - UserMaster テーブル
```sql
CREATE TABLE UserMaster (
    ID VARCHAR2(10) NOT NULL,
    Passwd VARCHAR2(30) NOT NULL,
    StaffLevel VARCHAR2(10) NOT NULL,
    PRIMARY KEY (ID)
);
```

### SQLite - DashboardData テーブル
```sql
CREATE TABLE DashboardData (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Category TEXT NOT NULL,
    Value INTEGER NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

## 📁 プロジェクト構成

```
DashboardServer/
├── Controllers/
│   ├── AuthController.cs          # 認証API
│   └── DashboardController.cs     # ダッシュボードデータAPI
├── Models/
│   ├── LoginRequest.cs
│   ├── LoginResponse.cs
│   ├── UserMaster.cs
│   └── DashboardData.cs
├── Services/
│   ├── AuthService.cs             # 認証ロジック
│   └── DashboardService.cs        # データ取得ロジック
├── wwwroot/
│   ├── index.html                 # メインHTML
│   ├── js/
│   │   └── chart.min.js           # Chart.js（ローカル）
│   └── css/
│       └── style.css              # スタイルシート
├── appsettings.json               # 設定ファイル
├── Program.cs                     # エントリーポイント
└── DashboardServer.csproj         # プロジェクトファイル
```

## 🔄 日次バッチでのデータ登録

SQLiteデータベース (`dashboard.db`) にデータを登録する例:

```csharp
using Microsoft.Data.Sqlite;

var connectionString = "Data Source=dashboard.db";
using var connection = new SqliteConnection(connectionString);
await connection.OpenAsync();

var insertQuery = @"
    INSERT INTO DashboardData (Category, Value) 
    VALUES (@Category, @Value)";

using var command = new SqliteCommand(insertQuery, connection);
command.Parameters.AddWithValue("@Category", "カテゴリ名");
command.Parameters.AddWithValue("@Value", 100);
await command.ExecuteNonQueryAsync();
```

## 🛠️ トラブルシューティング

### ポート5000が使用中の場合
`appsettings.json` と同階層に `Properties/launchSettings.json` を編集:

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:8080",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### ファイアウォール設定
1. Windowsファイアウォールの詳細設定を開く
2. 受信の規則で新しい規則を作成
3. ポート5000（または設定したポート）を許可

### Oracle ODBC接続エラー
1. ODBCデータソースアドミニストレーター（64bit）を開く
2. システムDSNタブで `ORD11` が存在するか確認
3. テスト接続が成功するか確認

## 📝 ライセンス

このプロジェクトは内部使用のために作成されています。

## 🆘 サポート

問題が発生した場合は、システム管理者に連絡してください。
