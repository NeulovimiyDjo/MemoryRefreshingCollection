#!/usr/bin/env bash

scriptdir="$(dirname "$(readlink -f "$0")")"
cd "$scriptdir"


cd ../utilities/WebScraper
echo "-----------------Running WebScraper...-----------------"
dotnet run -c Release --scrape-files
echo "-----------------All Done-----------------"

exit 0