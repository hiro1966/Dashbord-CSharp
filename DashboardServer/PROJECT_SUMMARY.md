# プロジェクトサマリー

## 📌 概要

オフラインWindows10環境で動作する、Oracle DB認証機能付きダッシュボードシステムです。

## 🎯 主な機能

### 1. 認証機能
- **Oracle DB (ODBC)** による認証
- **UserMasterテーブル** からユーザー情報を取得
- **JWT Token** による安全な認証
- **StaffLevel** による権限管理

### 2. ダッシュボード
- **Chart.js** による美しいグラフ表示
- **SQLite** データベースからのデータ取得
- リアルタイム更新機能（5分間隔）
- レスポンシブデザイン

### 3. データ管理
- **日次バッチ** によるOracleからSQLiteへのデータインポート
- タスクスケジューラー対応
- 実行ログ記録機能

### 4. デプロイ
- **自己完結型** 実行ファイル生成
- **Windowsサービス** 化対応
- ワンクリック配布パッケージ作成

## 📂 プロジェクト構造

```
DashboardServer/
├── Controllers/              # API コントローラー
│   ├── AuthController.cs     # 認証API
│   └── DashboardController.cs # ダッシュボードAPI
│
├── Models/                   # データモデル
│   ├── LoginRequest.cs
│   ├── LoginResponse.cs
│   ├── UserMaster.cs
│   └── DashboardData.cs
│
├── Services/                 # ビジネスロジック
│   ├── AuthService.cs        # 認証サービス
│   └── DashboardService.cs   # データサービス
│
├── wwwroot/                  # フロントエンド
│   ├── index.html           # メインページ
│   ├── css/style.css        # スタイルシート
│   └── js/chart.min.js      # Chart.js（ローカル）
│
├── BatchImport/              # 日次バッチ
│   ├── Program.cs           # バッチメイン処理
│   ├── DataImport.csproj    # プロジェクトファイル
│   ├── appsettings.json     # 設定ファイル
│   ├── run-import.bat       # 実行スクリプト
│   └── README.md            # バッチドキュメント
│
├── appsettings.json          # サーバー設定
├── Program.cs                # エントリーポイント
│
├── publish.bat               # 発行スクリプト
├── start-server.bat          # 起動スクリプト
├── install-service.bat       # サービスインストール
├── uninstall-service.bat     # サービスアンインストール
├── create-package.bat        # パッケージ作成
│
├── README.md                 # プロジェクト説明
├── SETUP_GUIDE.md           # セットアップガイド
└── test-client.html         # APIテストツール
```

## 🔧 技術スタック

### バックエンド
- **.NET 8.0** (ASP.NET Core Web API)
- **System.Data.Odbc** (Oracle接続)
- **Microsoft.Data.Sqlite** (SQLite接続)
- **JWT Bearer Authentication** (認証)

### フロントエンド
- **HTML5 + JavaScript (Vanilla)**
- **Chart.js 4.4.0** (グラフ描画)
- **CSS3** (スタイリング)

### データベース
- **Oracle Database** (認証用)
- **SQLite** (ダッシュボードデータ用)

## 🚀 クイックスタート

### 開発環境での実行

```cmd
cd DashboardServer
dotnet run
```

ブラウザで `http://localhost:5000` にアクセス

### 本番環境での配布

```cmd
cd DashboardServer
create-package.bat
```

生成された `DashboardSystem_YYYYMMDD.zip` を配布

## 📝 設定ファイル

### appsettings.json の主要項目

```json
{
  "JwtSettings": {
    "SecretKey": "秘密鍵（32文字以上推奨）",
    "ExpirationMinutes": 480
  },
  "OracleConnection": {
    "DataSourceName": "ORD11",
    "UserId": "Oracleユーザー",
    "Password": "Oracleパスワード"
  },
  "Authentication": {
    "StaffLevels": ["Manager", "Admin", "Staff"]
  },
  "SqliteConnection": {
    "DatabasePath": "dashboard.db"
  }
}
```

## 🗄️ データベース設計

### Oracle - UserMaster テーブル

| カラム名 | データ型 | 説明 |
|---------|---------|------|
| ID | VARCHAR2(10) | ユーザーID（主キー） |
| Passwd | VARCHAR2(30) | パスワード（平文） |
| StaffLevel | VARCHAR2(10) | 権限レベル |

### SQLite - DashboardData テーブル

| カラム名 | データ型 | 説明 |
|---------|---------|------|
| Id | INTEGER | 主キー（自動採番） |
| Category | TEXT | カテゴリ名 |
| Value | INTEGER | 値 |
| CreatedAt | DATETIME | 作成日時 |

## 🔐 認証フロー

1. クライアント → サーバー: ID/パスワード送信
2. サーバー → Oracle: UserMasterテーブル参照
3. サーバー: ID/Passwd検証
4. サーバー: StaffLevel検証（設定ファイルと照合）
5. サーバー → クライアント: JWT Token発行
6. 以降のAPI呼び出しでトークンを使用

## 📊 API エンドポイント

### POST /api/auth/login
ログイン認証

**リクエスト:**
```json
{
  "id": "admin",
  "password": "admin123"
}
```

**レスポンス:**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "admin",
  "staffLevel": "Admin",
  "message": "ログインに成功しました。"
}
```

### GET /api/dashboard/data
ダッシュボードデータ取得（認証必須）

**ヘッダー:**
```
Authorization: Bearer {token}
```

**レスポンス:**
```json
{
  "title": "ダッシュボードデータ",
  "labels": ["1月", "2月", "3月", "4月", "5月", "6月"],
  "values": [12, 19, 3, 5, 2, 3]
}
```

## 🛠️ カスタマイズポイント

### 1. グラフの種類を変更
`wwwroot/index.html` の Chart.js設定を編集:
```javascript
chart = new Chart(ctx, {
    type: 'bar', // 'line', 'pie', 'doughnut' など
    // ...
});
```

### 2. データ取得ロジックのカスタマイズ
`Services/DashboardService.cs` の `GetDashboardDataAsync()` メソッドを編集

### 3. 認証ロジックのカスタマイズ
`Services/AuthService.cs` の `AuthenticateAsync()` メソッドを編集

### 4. バッチ処理のカスタマイズ
`BatchImport/Program.cs` の `ImportDataAsync()` メソッドを編集

## 📦 配布パッケージ構成

```
DashboardSystem_YYYYMMDD.zip
├── Server/                   # サーバーアプリケーション
│   ├── DashboardServer.exe
│   ├── appsettings.json     # 要編集
│   ├── dashboard.db         # 自動生成
│   └── ...
│
├── BatchImport/              # 日次バッチ
│   ├── DataImport.exe
│   ├── appsettings.json     # 要編集
│   └── logs/               # ログ出力先
│
├── README.md                 # プロジェクト説明
└── SETUP_GUIDE.md           # セットアップ手順
```

## 🔄 メンテナンス

### 定期作業

1. **ログクリーンアップ**（月次）
   - `BatchImport/logs` フォルダ
   - 古いログファイルを削除

2. **データベースバックアップ**（週次）
   ```cmd
   copy dashboard.db backup\dashboard_%date%.db
   ```

3. **ユーザー管理**
   - Oracle の UserMaster テーブルで管理

## 🆘 トラブルシューティング

### よくある問題

| 問題 | 原因 | 解決方法 |
|-----|------|---------|
| サーバーが起動しない | ポート5000使用中 | launchSettings.json でポート変更 |
| Oracle接続エラー | ODBC設定ミス | ODBCデータソースを確認 |
| 認証失敗 | StaffLevel不一致 | appsettings.json の StaffLevels 確認 |
| クライアント接続不可 | ファイアウォール | ポート5000を開放 |

詳細は `SETUP_GUIDE.md` を参照してください。

## 📄 ライセンス

社内使用プロジェクト

## 👥 作成者

AI Assistant

## 📅 バージョン履歴

- **v1.0.0** (2024-11-20)
  - 初期リリース
  - 基本認証機能
  - ダッシュボード表示
  - 日次バッチインポート
  - Windowsサービス化対応
