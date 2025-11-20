@echo off
REM データインポートバッチ実行スクリプト

echo ========================================
echo データインポート開始
echo ========================================
echo 実行日時: %date% %time%
echo.

REM バッチプログラム実行
if exist "DataImport.exe" (
    REM 発行済みの実行ファイルを使用
    DataImport.exe
) else (
    REM .NET SDKでビルド＆実行
    dotnet run
)

set EXIT_CODE=%errorlevel%

echo.
if %EXIT_CODE% equ 0 (
    echo データインポートが正常に終了しました。
) else (
    echo エラー: データインポートが失敗しました。
    echo 終了コード: %EXIT_CODE%
)

REM 手動実行時は一時停止
if "%1"=="" pause

exit /b %EXIT_CODE%
