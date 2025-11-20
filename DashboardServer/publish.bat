@echo off
REM ダッシュボードサーバー発行スクリプト
REM このスクリプトを.NET 8.0がインストールされたWindowsで実行してください

echo ========================================
echo ダッシュボードサーバー 発行スクリプト
echo ========================================
echo.

REM 発行先ディレクトリ
set PUBLISH_DIR=.\publish

REM 既存の発行ディレクトリを削除
if exist "%PUBLISH_DIR%" (
    echo 既存の発行ディレクトリを削除しています...
    rmdir /s /q "%PUBLISH_DIR%"
)

REM ビルドキャッシュをクリーンアップ（重複エラー防止）
echo.
echo ビルドキャッシュをクリーンアップしています...
if exist obj rmdir /s /q obj
if exist bin rmdir /s /q bin
dotnet clean --configuration Release --verbosity quiet

echo.
echo 発行を開始します...
echo.

REM 完全自己完結型で発行（.NET Runtimeを含む）
dotnet publish -c Release -r win-x64 --self-contained true -o "%PUBLISH_DIR%"

if %errorlevel% equ 0 (
    echo.
    echo ========================================
    echo 発行が完了しました！
    echo ========================================
    echo.
    echo 発行先: %PUBLISH_DIR%
    echo.
    echo 次の手順:
    echo 1. %PUBLISH_DIR% フォルダを配置先サーバーにコピー
    echo 2. appsettings.json を編集（Oracle接続情報など）
    echo 3. DashboardServer.exe を実行
    echo.
) else (
    echo.
    echo エラー: 発行に失敗しました。
    echo.
)

pause
