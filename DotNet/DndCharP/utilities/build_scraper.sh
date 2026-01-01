#!/usr/bin/env bash

scriptdir="$(dirname "$(readlink -f "$0")")"
cd "$scriptdir"

cd ../utilities/WebScraper
echo "-----------------Building WebScraper...-----------------"
dotnet build -c Release
echo "-----------------All Done-----------------"

exit 0