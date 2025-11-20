@echo off
REM ダッシュボードサーバー発行スクリプト
REM このスクリプトを.NET 8.0がインストールされたWindowsで実行してください

echo ========================================
echo ダッシュボードサーバー 発行スクリプト
echo ========================================
echo.

REM Visual Studioなどが開いている場合は警告
echo 注意: Visual Studio や VS Code を閉じてから実行してください
echo.
pause

REM 発行先ディレクトリ
set PUBLISH_DIR=.\publish

REM 既存の発行ディレクトリを削除
if exist "%PUBLISH_DIR%" (
    echo 既存の発行ディレクトリを削除しています...
    rmdir /s /q "%PUBLISH_DIR%"
)

echo.
echo ビルドキャッシュをクリーンアップしています...
echo.

REM 方法1: dotnet clean を使用
dotnet clean --configuration Release --verbosity quiet

REM 方法2: obj と bin を手動削除（エラーは無視）
if exist obj (
    echo obj フォルダを削除しています...
    rmdir /s /q obj 2>nul
    if exist obj (
        echo 注意: obj フォルダが使用中です。手動削除が必要な場合があります。
    )
)

if exist bin (
    echo bin フォルダを削除しています...
    rmdir /s /q bin 2>nul
    if exist bin (
        echo 注意: bin フォルダが使用中です。手動削除が必要な場合があります。
    )
)

echo.
echo 発行を開始します...
echo.

REM 完全自己完結型で発行（.NET Runtimeを含む）
REM --force オプションで強制的に再ビルド
dotnet publish -c Release -r win-x64 --self-contained true --force -o "%PUBLISH_DIR%"

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
    echo 2. Scripts フォルダ内の create-dummy-data.bat を実行してダミーデータを作成
    echo 3. appsettings.json を編集（必要に応じて）
    echo 4. DashboardServer.exe を実行
    echo.
) else (
    echo.
    echo ========================================
    echo エラー: 発行に失敗しました
    echo ========================================
    echo.
    echo 対処方法:
    echo 1. Visual Studio や VS Code を完全に閉じる
    echo 2. タスクマネージャーで dotnet.exe や MSBuild.exe を終了
    echo 3. obj と bin フォルダを手動で削除:
    echo    - D:\develop\Dashbord-CSharp\DashboardServer\obj
    echo    - D:\develop\Dashbord-CSharp\DashboardServer\bin
    echo 4. もう一度このスクリプトを実行
    echo.
)

pause
