@echo off
REM ダッシュボードサーバー起動スクリプト

echo ========================================
echo ダッシュボードサーバーを起動します
echo ========================================
echo.

REM 管理者権限チェック
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo 警告: 管理者権限で実行していません。
    echo ポート5000を使用するには管理者権限が必要な場合があります。
    echo.
)

REM サーバー起動
echo サーバーを起動しています...
echo.
echo アクセスURL:
echo   ローカル:    http://localhost:5000
echo   ネットワーク: http://%COMPUTERNAME%:5000
echo.
echo サーバーを停止するには Ctrl+C を押してください。
echo.

REM 自己完結型の場合
if exist "DashboardServer.exe" (
    DashboardServer.exe
) else (
    REM .NET SDKがインストールされている場合
    dotnet run
)

pause
