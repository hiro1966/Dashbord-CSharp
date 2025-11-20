@echo off
REM ビルドキャッシュを手動でクリーンアップするスクリプト

echo ========================================
echo ビルドキャッシュクリーンアップ
echo ========================================
echo.
echo このスクリプトは obj と bin フォルダを削除します
echo publish.bat でエラーが出る場合に実行してください
echo.
pause

echo.
echo クリーンアップ中...
echo.

REM dotnet clean
dotnet clean --configuration Release --verbosity quiet
dotnet clean --configuration Debug --verbosity quiet

REM obj フォルダ削除
if exist obj (
    echo obj フォルダを削除しています...
    rmdir /s /q obj
    if exist obj (
        echo 警告: obj フォルダを削除できませんでした
        echo Visual Studio や VS Code を閉じてください
    ) else (
        echo obj フォルダを削除しました
    )
)

REM bin フォルダ削除
if exist bin (
    echo bin フォルダを削除しています...
    rmdir /s /q bin
    if exist bin (
        echo 警告: bin フォルダを削除できませんでした
        echo Visual Studio や VS Code を閉じてください
    ) else (
        echo bin フォルダを削除しました
    )
)

echo.
echo クリーンアップ完了
echo 続けて publish.bat を実行してください
echo.

pause
