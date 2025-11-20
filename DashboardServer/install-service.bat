@echo off
REM Windowsサービスとしてインストールするスクリプト
REM 管理者権限で実行する必要があります

echo ========================================
echo ダッシュボードサーバー サービスインストール
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

REM 現在のディレクトリを取得
set CURRENT_DIR=%~dp0
set EXE_PATH=%CURRENT_DIR%DashboardServer.exe

REM 実行ファイルの存在確認
if not exist "%EXE_PATH%" (
    echo エラー: DashboardServer.exe が見つかりません。
    echo publish.bat を実行してアプリケーションを発行してください。
    pause
    exit /b 1
)

echo サービス名: DashboardService
echo 実行ファイル: %EXE_PATH%
echo.

REM 既存のサービスを停止・削除
sc query DashboardService >nul 2>&1
if %errorlevel% equ 0 (
    echo 既存のサービスが見つかりました。削除します...
    sc stop DashboardService
    timeout /t 3 /nobreak >nul
    sc delete DashboardService
    timeout /t 2 /nobreak >nul
)

REM サービスを作成
echo サービスを作成しています...
sc create DashboardService binPath= "\"%EXE_PATH%\"" start= auto DisplayName= "Dashboard Server Service"

if %errorlevel% equ 0 (
    echo サービスの説明を設定しています...
    sc description DashboardService "オフラインダッシュボードWebサーバー"
    
    echo サービスを起動しています...
    sc start DashboardService
    
    if %errorlevel% equ 0 (
        echo.
        echo ========================================
        echo サービスのインストールが完了しました！
        echo ========================================
        echo.
        echo サービス名: DashboardService
        echo 表示名: Dashboard Server Service
        echo 起動種類: 自動
        echo.
        echo アクセスURL: http://localhost:5000
        echo.
        echo サービスの管理:
        echo   停止: sc stop DashboardService
        echo   開始: sc start DashboardService
        echo   削除: uninstall-service.bat を実行
        echo.
    ) else (
        echo エラー: サービスの起動に失敗しました。
    )
) else (
    echo エラー: サービスの作成に失敗しました。
)

pause
