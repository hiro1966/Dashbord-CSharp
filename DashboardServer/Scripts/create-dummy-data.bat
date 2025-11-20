@echo off
REM ダミーデータ作成スクリプト

echo ========================================
echo ダミーデータ作成
echo ========================================
echo.

REM データベースパス
set DB_PATH=..\dashboard.db

echo データベース: %DB_PATH%
echo.
echo 診療科: 内科、小児科、整形外科
echo 期間: 2025-01-01 から 2025-10-31
echo.
echo 処理を開始します...
echo.

REM スクリプト実行
dotnet run --project CreateDummyData.csproj -- %DB_PATH%

if %errorlevel% equ 0 (
    echo.
    echo ========================================
    echo ダミーデータ作成完了！
    echo ========================================
    echo.
    echo データベース: %DB_PATH%
    echo.
    echo テーブル:
    echo   - 診療科（3件）
    echo   - 入院患者（2025-01-01 から 2025-10-31）
    echo   - 外来患者（2025-01-01 から 2025-10-31）
    echo.
) else (
    echo.
    echo エラー: ダミーデータ作成に失敗しました。
    echo.
)

pause
