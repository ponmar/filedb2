#!/usr/bin/env bash
set -euo pipefail

mapfile -t projects < <(
  dotnet sln FileDB.slnx list \
  | tail -n +2 \
  | sed '/^[[:space:]]*$/d' \
  | grep -E '\.(csproj|fsproj|vbproj)$'
)

if [ "${#projects[@]}" -eq 0 ]; then
  echo "No projects found in FileDB.slnx."
  exit 1
fi

for project in "${projects[@]}"; do
  dotnet package update --project "$project"
done

nuget-license -i FileDB.slnx -o Json --file-output FileDB/Resources/licenses.json
