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

set appDir=FileDB.Desktop\bin\Release\net9.0\win-x64\publish
set zipDir=FileDB-%version%-win-x64
set releaseDir=release\%zipDir%
set zipFilename=%zipDir%.zip

if exist "%releaseDir%" (
    echo "Release directory already exists" && exit /b 1
)

if exist "release\%zipFilename%" (
    echo "Release zip filename already exists" && exit /b 1
)

mkdir %releaseDir%

xcopy /s %appDir%\* %releaseDir%
xcopy CHANGES.txt %releaseDir%
xcopy LICENSE.txt %releaseDir%

cd release
"C:\Program Files\7-Zip\7z.exe" a -tzip %zipFilename% %zipDir%
cd ..

pause