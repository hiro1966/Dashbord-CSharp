@echo off
REM Windowsサービスをアンインストールするスクリプト
REM 管理者権限で実行する必要があります

echo ========================================
echo ダッシュボードサーバー サービスアンインストール
echo ========================================
echo.

REM 管理者権限チェック
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo エラー: このスクリプトは管理者権限で実行する必要があります。
    echo 右クリック → 「管理者として実行」してください。
    pause
    exit /b 1
)

REM サービスの存在確認
sc query DashboardService >nul 2>&1
if %errorlevel% neq 0 (
    echo サービス 'DashboardService' は存在しません。
    pause
    exit /b 0
)

echo サービスを停止しています...
sc stop DashboardService
timeout /t 3 /nobreak >nul

echo サービスを削除しています...
sc delete DashboardService

if %errorlevel% equ 0 (
    echo.
    echo ========================================
    echo サービスのアンインストールが完了しました
    echo ========================================
    echo.
) else (
    echo エラー: サービスの削除に失敗しました。
)

pause
