#!/usr/bin/env bash

scriptdir="$(dirname "$(readlink -f "$0")")"
cd "$scriptdir"


cd ../client
echo "-----------------Building Client...-----------------"
npm run build


cd ../utilities/WebScraper
echo "-----------------Building WebScraper...-----------------"
dotnet build -c Release
echo "-----------------Running WebScraper...-----------------"
dotnet run -c Release --scrape-files --silent

cd ../../server
echo "-----------------Building Server...-----------------"
dotnet build -c Release

echo "-----------------All Done-----------------"

exit 0