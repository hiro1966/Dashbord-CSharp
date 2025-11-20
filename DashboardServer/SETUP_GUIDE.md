# ダッシュボードシステム セットアップガイド

オフラインWindows10環境でのダッシュボードシステムの完全なセットアップ手順です。

## 📋 目次

1. [システム要件](#システム要件)
2. [サーバー環境セットアップ](#サーバー環境セットアップ)
3. [クライアント環境セットアップ](#クライアント環境セットアップ)
4. [Oracle DB設定](#oracle-db設定)
5. [アプリケーション配置](#アプリケーション配置)
6. [起動と動作確認](#起動と動作確認)
7. [日次バッチ設定](#日次バッチ設定)
8. [トラブルシューティング](#トラブルシューティング)

---

## システム要件

### サーバー側
- **OS**: Windows 10 64bit
- **CPU**: 2コア以上推奨
- **RAM**: 4GB以上推奨
- **ストレージ**: 500MB以上の空き容量
- **ソフトウェア**:
  - .NET 8.0 Runtime (ASP.NET Core)
  - Oracle ODBC Driver (64bit)

### クライアント側
- **OS**: Windows 10 64bit
- **ブラウザ**: Microsoft Edge または Google Chrome (最新版)

### ネットワーク
- サーバーとクライアントが同一ネットワーク内で通信可能
- ファイアウォールでポート5000が開放されていること

---

## サーバー環境セットアップ

### ステップ1: .NET 8.0 Runtimeのインストール

#### オンライン環境でのダウンロード（USBメモリなどでオフライン環境に転送）

1. オンラインPCで以下のURLにアクセス:
   ```
   https://dotnet.microsoft.com/download/dotnet/8.0
   ```

2. 「ASP.NET Core Runtime 8.0.x」セクションで以下をダウンロード:
   - **Windows Hosting Bundle** (推奨)
   - または **ASP.NET Core Runtime 8.0.x - Windows x64**

3. ダウンロードしたインストーラーをUSBメモリに保存

#### オフライン環境でのインストール

1. USBメモリからインストーラーをコピー
2. インストーラーを実行（例: `dotnet-hosting-8.0.x-win.exe`）
3. インストールウィザードに従って進める
4. インストール完了後、PCを再起動

#### インストール確認

コマンドプロンプトを開いて以下を実行:
```cmd
dotnet --version
```
バージョン番号（例: 8.0.x）が表示されればOK

---

### ステップ2: Oracle ODBC Driverのインストール

#### ダウンロード

1. Oracle公式サイトにアクセス（オンライン環境）:
   ```
   https://www.oracle.com/database/technologies/odbc-downloads.html
   ```

2. Oracle Instant Client ODBC をダウンロード:
   - **64-bit版** を選択
   - 例: `instantclient-odbc-windows.x64-19.x.x.x.zip`

3. USBメモリに保存

#### インストール

1. ZIPファイルを展開（例: `C:\oracle\instantclient_19_x`）

2. 管理者権限でコマンドプロンプトを開き、展開したフォルダに移動:
   ```cmd
   cd C:\oracle\instantclient_19_x
   odbc_install.exe
   ```

3. インストール成功メッセージを確認

---

### ステップ3: ODBCデータソースの設定

1. **ODBCデータソースアドミニストレーター（64ビット）** を開く:
   - スタートメニューで「ODBC」を検索
   - 「ODBCデータソース（64ビット）」を選択

2. **システムDSN** タブをクリック

3. **追加** ボタンをクリック

4. **Oracle in instantclient_19_x** を選択して「完了」

5. 以下の情報を入力:
   - **Data Source Name**: `ORD11`
   - **TNS Service Name**: （Oracle DBのサービス名）
   - **User ID**: （空欄のまま）

6. **Test Connection** で接続テスト（ユーザーID/パスワードを入力）

7. 接続成功を確認して「OK」

---

## Oracle DB設定

### UserMasterテーブルの作成

SQL*Plusまたは他のOracleツールで以下を実行:

```sql
-- UserMasterテーブル作成
CREATE TABLE UserMaster (
    ID VARCHAR2(10) NOT NULL,
    Passwd VARCHAR2(30) NOT NULL,
    StaffLevel VARCHAR2(10) NOT NULL,
    CONSTRAINT PK_UserMaster PRIMARY KEY (ID)
);

-- サンプルデータ挿入
INSERT INTO UserMaster (ID, Passwd, StaffLevel) 
VALUES ('admin', 'admin123', 'Admin');

INSERT INTO UserMaster (ID, Passwd, StaffLevel) 
VALUES ('manager', 'manager123', 'Manager');

INSERT INTO UserMaster (ID, Passwd, StaffLevel) 
VALUES ('staff01', 'staff123', 'Staff');

COMMIT;
```

### ソースデータテーブルの作成（バッチインポート用）

```sql
-- ダッシュボード用ソースデータテーブル
CREATE TABLE SourceData (
    Category VARCHAR2(50) NOT NULL,
    Value NUMBER NOT NULL,
    RecordDate DATE NOT NULL
);

-- サンプルデータ
INSERT INTO SourceData (Category, Value, RecordDate) 
VALUES ('売上', 1500, TRUNC(SYSDATE));

INSERT INTO SourceData (Category, Value, RecordDate) 
VALUES ('顧客数', 320, TRUNC(SYSDATE));

COMMIT;
```

---

## アプリケーション配置

### ステップ1: アプリケーションのビルド（開発環境）

開発環境で以下を実行:

```cmd
cd DashboardServer
publish.bat
```

これにより `publish` フォルダに配布用ファイルが生成されます。

### ステップ2: サーバーへのコピー

1. `publish` フォルダ全体をUSBメモリにコピー

2. サーバーの任意の場所に配置（例: `C:\DashboardServer`）

### ステップ3: 設定ファイルの編集

`C:\DashboardServer\appsettings.json` を編集:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "JwtSettings": {
    "SecretKey": "YourVeryLongSecretKeyForJWTTokenGeneration123456789",
    "Issuer": "DashboardServer",
    "Audience": "DashboardClient",
    "ExpirationMinutes": 480
  },
  "OracleConnection": {
    "DataSourceName": "ORD11",
    "UserId": "your_actual_oracle_user",
    "Password": "your_actual_oracle_password"
  },
  "Authentication": {
    "StaffLevels": ["Manager", "Admin", "Staff"]
  },
  "SqliteConnection": {
    "DatabasePath": "dashboard.db"
  }
}
```

**重要**: `OracleConnection` セクションのユーザーID/パスワードを実際の値に変更してください。

---

## 起動と動作確認

### 方法1: 手動起動（テスト用）

1. `C:\DashboardServer` フォルダを開く

2. `start-server.bat` をダブルクリック

3. コマンドプロンプトが開き、サーバーが起動します

4. 以下のようなメッセージが表示されます:
   ```
   Now listening on: http://localhost:5000
   Application started.
   ```

### 方法2: Windowsサービスとして起動（本番推奨）

1. 管理者権限でコマンドプロンプトを開く

2. `C:\DashboardServer` に移動:
   ```cmd
   cd C:\DashboardServer
   ```

3. `install-service.bat` を実行:
   ```cmd
   install-service.bat
   ```

4. サービスが自動的に起動します

### 動作確認

#### サーバーローカルでの確認

1. サーバーでブラウザを開く

2. `http://localhost:5000` にアクセス

3. ログイン画面が表示されることを確認

4. テストユーザーでログイン:
   - ユーザーID: `admin`
   - パスワード: `admin123`

5. ダッシュボード画面が表示されることを確認

#### クライアントからの確認

1. クライアントPCでブラウザを開く

2. `http://[サーバーのIPアドレス]:5000` にアクセス
   - サーバーのIPアドレス確認方法:
     ```cmd
     ipconfig
     ```
     「IPv4 アドレス」を確認

3. ログイン画面が表示されることを確認

4. ログインして動作確認

---

## 日次バッチ設定

### ステップ1: バッチプログラムの配置

1. `BatchImport` フォルダをサーバーにコピー（例: `C:\DashboardServer\BatchImport`）

2. `appsettings.json` を編集:
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
       "SourceTableName": "SourceData",
       "LogPath": "logs"
     }
   }
   ```

### ステップ2: 手動実行テスト

```cmd
cd C:\DashboardServer\BatchImport
run-import.bat
```

正常に実行されることを確認。

### ステップ3: タスクスケジューラーに登録

1. **タスクスケジューラー** を開く

2. **基本タスクの作成** をクリック

3. ウィザードに従って設定:

   **名前**: `ダッシュボードデータインポート`
   
   **トリガー**: 毎日
   
   **開始**: 2:00 AM（任意の時刻）
   
   **操作**: プログラムの起動
   
   **プログラム/スクリプト**:
   ```
   C:\DashboardServer\BatchImport\run-import.bat
   ```
   
   **開始（オプション）**:
   ```
   C:\DashboardServer\BatchImport
   ```

4. **完了** をクリック

5. 作成したタスクを右クリック → **実行** でテスト

6. `logs` フォルダにログファイルが生成されることを確認

---

## トラブルシューティング

### 1. サーバーが起動しない

#### エラー: ポート5000が使用中

**解決方法**: ポート番号を変更

1. `Properties\launchSettings.json` を作成/編集:
   ```json
   {
     "profiles": {
       "http": {
         "commandName": "Project",
         "launchBrowser": true,
         "applicationUrl": "http://localhost:8080",
         "environmentVariables": {
           "ASPNETCORE_ENVIRONMENT": "Production"
         }
       }
     }
   }
   ```

2. サーバーを再起動

#### エラー: .NET Runtimeが見つからない

**解決方法**: 

1. .NET Runtime が正しくインストールされているか確認:
   ```cmd
   dotnet --list-runtimes
   ```

2. `Microsoft.AspNetCore.App 8.0.x` が表示されることを確認

3. 表示されない場合は、.NET Runtimeを再インストール

---

### 2. Oracle接続エラー

#### エラー: ERROR [IM002] [Microsoft][ODBC Driver Manager]

**原因**: ODBCデータソースが見つからない

**解決方法**:

1. **ODBCデータソースアドミニストレーター（64ビット）** を開く

2. システムDSNタブで `ORD11` が存在するか確認

3. 存在しない場合は、[ステップ3: ODBCデータソースの設定](#ステップ3-odbcデータソースの設定) を参照

#### エラー: ORA-01017: invalid username/password

**原因**: ユーザーID/パスワードが間違っている

**解決方法**:

1. `appsettings.json` の `OracleConnection` セクションを確認

2. Oracle DBに直接接続して、ユーザーID/パスワードが正しいか確認

---

### 3. 認証エラー

#### エラー: ユーザーIDまたはパスワードが正しくありません

**原因1**: UserMasterテーブルにユーザーが登録されていない

**解決方法**: Oracle DBでユーザーデータを確認:
```sql
SELECT ID, StaffLevel FROM UserMaster;
```

**原因2**: StaffLevelが許可リストに含まれていない

**解決方法**: `appsettings.json` の `Authentication:StaffLevels` を確認

---

### 4. クライアントから接続できない

#### 原因: ファイアウォールでブロックされている

**解決方法**:

1. **Windows Defender ファイアウォール** を開く

2. **詳細設定** をクリック

3. **受信の規則** → **新しい規則**

4. **ポート** を選択 → **次へ**

5. **TCP** → **特定のローカルポート**: `5000` → **次へ**

6. **接続を許可する** → **次へ**

7. すべてのプロファイルにチェック → **次へ**

8. 名前: `DashboardServer` → **完了**

---

### 5. SQLiteデータベースエラー

#### エラー: database is locked

**原因**: 複数のプロセスがデータベースにアクセスしている

**解決方法**:

1. サーバーとバッチプログラムを停止

2. `dashboard.db` へのアクセスを確認:
   ```cmd
   handle.exe dashboard.db
   ```

3. ロックしているプロセスを終了

4. サーバーを再起動

---

### 6. ログの確認方法

#### サーバーログ

Windowsサービスとして実行している場合:

1. **イベントビューアー** を開く

2. **Windowsログ** → **アプリケーション**

3. ソース: `DashboardService` でフィルタ

#### バッチログ

```cmd
cd C:\DashboardServer\BatchImport\logs
type import_20231120.log
```

---

## 📞 サポート情報

その他の問題が発生した場合は、以下を確認してください:

1. **サーバーログ**: イベントビューアー
2. **バッチログ**: `BatchImport\logs` フォルダ
3. **ブラウザコンソール**: F12キーで開発者ツールを開く

詳細なエラーメッセージをシステム管理者に連絡してください。

---

## 📝 メンテナンス

### 定期メンテナンス項目

1. **ログファイルのクリーンアップ**（月次）
   - `BatchImport\logs` フォルダの古いログを削除

2. **SQLiteデータベースのバックアップ**（週次）
   ```cmd
   copy C:\DashboardServer\dashboard.db C:\Backup\dashboard_backup_%date:~0,4%%date:~5,2%%date:~8,2%.db
   ```

3. **ユーザー管理**
   - Oracle DB の UserMaster テーブルで管理

4. **.NET Runtimeの更新**（適宜）
   - セキュリティパッチの適用

---

以上でセットアップは完了です。お疲れさまでした！
