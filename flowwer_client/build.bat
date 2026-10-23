@echo off
REM Recompile Flowwer Client et met a jour l'application lancee par le raccourci.
REM A lancer apres chaque modification (ou Ctrl+Shift+B dans VS Code).
cd /d "%~dp0"

taskkill /im flowwer_client.exe /f >nul 2>&1

dotnet publish flowwer_client.csproj -c Release -r win-x64 --self-contained false -o app
if errorlevel 1 (
    echo.
    echo *** ERREUR de compilation, l'application n'a pas ete mise a jour ***
    pause
    exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$s=(New-Object -ComObject WScript.Shell).CreateShortcut('%~dp0Flowwer Client.lnk');" ^
  "$s.TargetPath='%~dp0app\flowwer_client.exe';" ^
  "$s.WorkingDirectory='%~dp0app';" ^
  "$s.IconLocation='%~dp0Resources\flowwer.ico,0';" ^
  "$s.Save()"

echo.
echo Flowwer Client est a jour ! Lance-le avec "Flowwer Client.lnk".
