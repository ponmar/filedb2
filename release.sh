#!/usr/bin/env bash
set -euo pipefail

# Read version from first line of CHANGES.txt (strip trailing whitespace/CR)
read -r version < CHANGES.txt
version="${version%:}"   # remove trailing colon if present
version="${version%%:*}" # keep only content before first colon
echo "Detected version: $version"

dotnet clean FileDB.slnx -c Release

publishDir="FileDB.Desktop/bin/Release/net10.0/release-publish"
rm -rf "$publishDir"

dotnet publish FileDB.Desktop/FileDB.Desktop.csproj \
  -c Release \
  --no-self-contained \
  -o "$publishDir" \
  -p:Version="${version}.0.0"

appDir="$publishDir"
zipDir="FileDB-${version}"
releaseDir="release/${zipDir}"
zipFilename="${zipDir}.zip"

if [ -d "$releaseDir" ]; then
    echo "Error: Release directory already exists: $releaseDir" >&2
    exit 1
fi

if [ -f "release/${zipFilename}" ]; then
    echo "Error: Release zip already exists: release/${zipFilename}" >&2
    exit 1
fi

mkdir -p "$releaseDir"

cp -r "${appDir}/." "$releaseDir/"
cp CHANGES.txt "$releaseDir/"
cp LICENSE.txt "$releaseDir/"

(
  cd release
  sevenZipExe="/c/Program Files/7-Zip/7z.exe"
  if command -v zip &>/dev/null; then
    zip -r "$zipFilename" "$zipDir"
  elif [ -x "$sevenZipExe" ]; then
    "$sevenZipExe" a -tzip "$zipFilename" "$zipDir"
  elif command -v 7z &>/dev/null; then
    7z a -tzip "$zipFilename" "$zipDir"
  else
    echo "Error: neither 'zip' nor '7z' found. Install one and retry." >&2
    exit 1
  fi
)

echo "Created release/${zipFilename}"
