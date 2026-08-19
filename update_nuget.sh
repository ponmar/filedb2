#!/usr/bin/env bash
set -euo pipefail

dotnet package update --project FileDB.slnx

nuget-license -i FileDB.slnx -o Json --file-output FileDB/Resources/licenses.json
