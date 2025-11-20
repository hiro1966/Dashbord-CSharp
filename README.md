# オフラインWindows10向けダッシュボードシステム

## 🎯 プロジェクト概要

このプロジェクトは、**オフラインWindows10環境**で動作する、**Oracle DB認証機能付きダッシュボードシステム**です。

### 主な特徴

✅ **完全オフライン動作** - インターネット接続不要  
✅ **Oracle認証統合** - 既存のUserMasterテーブルを使用  
✅ **セキュアな認証** - JWT Token方式  
✅ **美しいダッシュボード** - Chart.jsによる可視化  
✅ **自動データ更新** - 日次バッチで最新データを反映  
✅ **簡単デプロイ** - ワンクリックでWindowsサービス化  

## 📦 システム構成

```
┌─────────────────────────────────────────────────────┐
│                  クライアント (Windows10)              │
│                Edge / Chrome ブラウザ                 │
└────────────────────┬────────────────────────────────┘
                     │ HTTP (Port 5000)
                     ↓
┌─────────────────────────────────────────────────────┐
│              サーバー (Windows10)                     │
│  ┌──────────────────────────────────────────────┐  │
│  │   ASP.NET Core 8.0 Web API                    │  │
│  │   - 認証API (JWT Token)                       │  │
│  │   - ダッシュボードAPI                         │  │
│  └──────────────────────────────────────────────┘  │
│           │                        │                 │
│           ↓                        ↓                 │
│  ┌─────────────────┐   ┌──────────────────┐        │
│  │  Oracle DB      │   │  SQLite          │        │
│  │  (ODBC接続)     │   │  dashboard.db    │        │
│  │  - UserMaster   │   │  - DashboardData │        │
│  └─────────────────┘   └──────────────────┘        │
│           ↑                        ↑                 │
│           │ 日次バッチ              │                │
│           └────────────────────────┘                │
└─────────────────────────────────────────────────────┘
```

## 🚀 クイックスタート

### 📥 必要なもの

- Windows 10 64bit
- .NET 8.0 Runtime (ASP.NET Core)
- Oracle ODBC Driver
- Oracle DB (UserMasterテーブル設定済み)

### 📝 セットアップ手順

1. **プロジェクトのクローン**
   ```cmd
   git clone <repository-url>
   cd DashboardServer
   ```

2. **設定ファイルの編集**
   `appsettings.json` を開いて以下を設定：
   ```json
   {
     "OracleConnection": {
       "DataSourceName": "ORD11",
       "UserId": "your_oracle_user",
       "Password": "your_oracle_password"
     }
   }
   ```

3. **サーバー起動**
   ```cmd
   start-server.bat
   ```

4. **ブラウザでアクセス**
   ```
   http://localhost:5000
   ```

5. **ログイン**
   - ユーザーID: `admin`
   - パスワード: `admin123`
   （Oracle DBに登録されているユーザー）

### 🎨 デモ・テスト

APIテストツール: `test-client.html` をブラウザで開く

## 📚 ドキュメント

| ドキュメント | 内容 |
|------------|------|
| [README.md](DashboardServer/README.md) | プロジェクト概要 |
| [SETUP_GUIDE.md](DashboardServer/SETUP_GUIDE.md) | 完全なセットアップ手順 |
| [PROJECT_SUMMARY.md](DashboardServer/PROJECT_SUMMARY.md) | 技術詳細・カスタマイズ方法 |
| [BatchImport/README.md](DashboardServer/BatchImport/README.md) | 日次バッチの使い方 |

## 🔧 主な機能

### 1. 認証機能
- Oracle DB (ODBC) による認証
- JWT Token発行
- StaffLevel による権限管理

### 2. ダッシュボード
- Chart.js による美しいグラフ
- リアルタイム更新（5分間隔）
- レスポンシブデザイン

### 3. 日次バッチ
- Oracle → SQLite データ転送
- タスクスケジューラー対応
- ログ記録機能

### 4. デプロイ
- 自己完結型実行ファイル
- Windowsサービス化
- ワンクリック配布パッケージ

## 📂 プロジェクト構造

```
DashboardServer/
├── Controllers/         # API Controllers
├── Models/             # Data Models
├── Services/           # Business Logic
├── wwwroot/            # Frontend (HTML/CSS/JS)
├── BatchImport/        # Daily Batch Job
├── *.bat               # Utility Scripts
└── *.md                # Documentation
```

## 🎯 使用技術

- **Backend**: .NET 8.0 (ASP.NET Core)
- **Authentication**: JWT Bearer Token
- **Database**: Oracle (認証), SQLite (データ)
- **Frontend**: HTML5, JavaScript, Chart.js 4.4.0
- **Deployment**: Self-contained, Windows Service

## 🔐 セキュリティ

- ✅ JWT Token認証
- ✅ HTTPS対応可能
- ✅ CORS設定
- ✅ StaffLevel権限管理
- ⚠️ パスワードは平文（Oracle DB側の仕様に依存）

## 📊 データベース

### Oracle - UserMaster
```sql
CREATE TABLE UserMaster (
    ID VARCHAR2(10) PRIMARY KEY,
    Passwd VARCHAR2(30) NOT NULL,
    StaffLevel VARCHAR2(10) NOT NULL
);
```

### SQLite - DashboardData
```sql
CREATE TABLE DashboardData (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Category TEXT NOT NULL,
    Value INTEGER NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

## 🛠️ 開発・配布

### 開発環境で実行
```cmd
cd DashboardServer
dotnet run
```

### 配布パッケージ作成
```cmd
cd DashboardServer
create-package.bat
```

### Windowsサービス化
```cmd
cd DashboardServer
install-service.bat
```

## 🐛 トラブルシューティング

| 問題 | 解決方法 |
|-----|---------|
| ポート5000使用中 | launchSettings.json でポート変更 |
| Oracle接続エラー | ODBC設定を確認 |
| 認証失敗 | StaffLevels設定を確認 |
| クライアント接続不可 | ファイアウォール設定を確認 |

詳細は [SETUP_GUIDE.md](DashboardServer/SETUP_GUIDE.md) を参照

## 📝 TODO / 今後の改善

- [ ] パスワードハッシュ化対応
- [ ] 複数グラフ対応
- [ ] データエクスポート機能
- [ ] ユーザー管理画面
- [ ] リアルタイムデータ更新（WebSocket）

## 📄 ライセンス

社内使用プロジェクト

## 👥 サポート

質問や問題がある場合は、システム管理者に連絡してください。

---

**作成日**: 2024-11-20  
**バージョン**: 1.0.0  
**開発環境**: .NET 8.0, ASP.NET Core, Oracle DB, SQLite
