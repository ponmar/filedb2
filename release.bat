rem @echo off

set appDir=FileDB2Browser\bin\Release\net5.0-windows
set versionPrinter=

for /f "delims=" %%a in ('VersionPrinter\bin\Release\net5.0\VersionPrinter.exe') do @set version=%%a
echo Detected version: %version%

set zipDir=FileDB2-%version%
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

cd release
"C:\Program Files\7-Zip\7z.exe" a -tzip %zipFilename% %zipDir%
cd ..
