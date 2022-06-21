@echo off

set /p version=< CHANGES.txt
set version=%version:~0,-1%
echo Detected version: %version%
pause

dotnet clean FileDB.sln -c Release
if not %ERRORLEVEL%==0 (
    echo "Clean solution failed" && exit /b 1
)

dotnet build FileDB\FileDB.csproj -c Release
if not %ERRORLEVEL%==0 (
    echo "Build failed" && exit /b 1
)

set appDir=FileDB\bin\Release\net6.0-windows
set zipDir=FileDB-%version%
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
xcopy /s demo %releaseDir%\demo\
xcopy CHANGES.txt %releaseDir%
xcopy LICENSE.txt %releaseDir%

cd release
"C:\Program Files\7-Zip\7z.exe" a -tzip %zipFilename% %zipDir%
cd ..

pause