@echo off
REM 配布パッケージ作成スクリプト

echo ========================================
echo ダッシュボードシステム 配布パッケージ作成
echo ========================================
echo.

REM パッケージ名（日付付き）
set PACKAGE_NAME=DashboardSystem_%date:~0,4%%date:~5,2%%date:~8,2%

echo パッケージ名: %PACKAGE_NAME%
echo.

REM 一時ディレクトリ作成
if exist "package_temp" (
    rmdir /s /q "package_temp"
)
mkdir "package_temp\%PACKAGE_NAME%"

echo ========================================
echo 1. サーバーアプリケーションのビルド
echo ========================================
echo.

REM サーバーを発行
call publish.bat

if %errorlevel% neq 0 (
    echo エラー: サーバーのビルドに失敗しました。
    pause
    exit /b 1
)

REM サーバーファイルをコピー
echo サーバーファイルをパッケージにコピーしています...
xcopy /E /I /Y "publish" "package_temp\%PACKAGE_NAME%\Server"

echo.
echo ========================================
echo 2. バッチインポートプログラムのビルド
echo ========================================
echo.

cd BatchImport

REM バッチプログラムを発行
dotnet publish -c Release -r win-x64 --self-contained true -o "publish_batch"

if %errorlevel% neq 0 (
    echo エラー: バッチプログラムのビルドに失敗しました。
    cd ..
    pause
    exit /b 1
)

cd ..

REM バッチファイルをコピー
echo バッチファイルをパッケージにコピーしています...
xcopy /E /I /Y "BatchImport\publish_batch" "package_temp\%PACKAGE_NAME%\BatchImport"
copy "BatchImport\README.md" "package_temp\%PACKAGE_NAME%\BatchImport\"

echo.
echo ========================================
echo 3. ドキュメントのコピー
echo ========================================
echo.

copy "README.md" "package_temp\%PACKAGE_NAME%\"
copy "SETUP_GUIDE.md" "package_temp\%PACKAGE_NAME%\"

echo.
echo ========================================
echo 4. ZIPファイルの作成
echo ========================================
echo.

REM PowerShellでZIP作成
powershell -Command "Compress-Archive -Path 'package_temp\%PACKAGE_NAME%' -DestinationPath '%PACKAGE_NAME%.zip' -Force"

if %errorlevel% equ 0 (
    echo.
    echo ========================================
    echo パッケージ作成完了！
    echo ========================================
    echo.
    echo 作成されたファイル: %PACKAGE_NAME%.zip
    echo.
    echo このZIPファイルをオフライン環境に転送してください。
    echo セットアップ方法は SETUP_GUIDE.md を参照してください。
    echo.
) else (
    echo エラー: ZIPファイルの作成に失敗しました。
)

REM 一時ディレクトリを削除
rmdir /s /q "package_temp"
rmdir /s /q "BatchImport\publish_batch"

pause
