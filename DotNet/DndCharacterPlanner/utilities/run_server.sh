#!/usr/bin/env bash

scriptdir="$(dirname "$(readlink -f "$0")")"
cd "$scriptdir"

cd ../server
dotnet run -c Release

exit 0