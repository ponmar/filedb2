#!/usr/bin/env bash
set -euo pipefail

# Read version from first line of CHANGES.txt (strip trailing whitespace/CR)
read -r version < CHANGES.txt
version="${version%:}"   # remove trailing colon if present
version="${version%%:*}" # keep only content before first colon
echo "Detected version: $version"

case "$(uname -s)" in
  Linux*)
    runtimeId="linux-x64"
    ;;
  MINGW*|MSYS*|CYGWIN*)
    runtimeId="win-x64"
    ;;
  *)
    echo "Error: unsupported platform: $(uname -s)" >&2
    exit 1
    ;;
esac
echo "Detected runtime: $runtimeId"

if ! command -v dotnet &>/dev/null; then
    echo "Error: 'dotnet' not found. Install the .NET 10 SDK and retry." >&2
    exit 1
fi

sevenZipExe="/c/Program Files/7-Zip/7z.exe"
if command -v zip &>/dev/null; then
    archiveTool="zip"
elif [ -x "$sevenZipExe" ]; then
    archiveTool="$sevenZipExe"
elif command -v 7z &>/dev/null; then
    archiveTool="7z"
else
    echo "Error: neither 'zip' nor '7z' found. Install one and retry." >&2
    exit 1
fi

zipDir="FileDB-${version}-${runtimeId}"
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

dotnet clean FileDB.slnx -c Release

publishDir="FileDB.Desktop/bin/Release/net10.0/${runtimeId}/release-publish"
rm -rf "$publishDir"

dotnet publish FileDB.Desktop/FileDB.Desktop.csproj \
  -c Release \
  -r "$runtimeId" \
  --self-contained true \
  -o "$publishDir" \
  -p:Version="${version}.0.0"

mkdir -p "$releaseDir"

cp -r "${publishDir}/." "$releaseDir/"
cp CHANGES.txt "$releaseDir/"
cp LICENSE.txt "$releaseDir/"

(
  cd release
  if [ "$archiveTool" = "zip" ]; then
    zip -r "$zipFilename" "$zipDir"
  else
    "$archiveTool" a -tzip "$zipFilename" "$zipDir"
  fi
)

echo "Created release/${zipFilename}"
