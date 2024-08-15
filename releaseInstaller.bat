@echo off

set /p version=< CHANGES.txt
set version=%version:~0,-1%
echo Detected version: %version%
pause

dotnet clean FileDB.sln -c Release
if not %ERRORLEVEL%==0 (
    echo "Clean solution failed" && exit /b 1
)

dotnet publish FileDB.Desktop\FileDB.Desktop.csproj -r win-x64 -p:Version=%version%.0.0 --self-contained true
if not %ERRORLEVEL%==0 (
    echo "Build failed" && exit /b 1
)

set installerFilename=FileDB-%version%

"C:\Program Files (x86)\Inno Setup 6\iscc.exe" Installer\InnoSetupInstaller.iss /F"%installerFilename%"
if not %ERRORLEVEL%==0 (
    echo "Installer creation failed" && exit /b 1
)

pause