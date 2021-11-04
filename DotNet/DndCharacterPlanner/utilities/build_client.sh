#!/usr/bin/env bash

scriptdir="$(dirname "$(readlink -f "$0")")"
cd "$scriptdir"


cd ../client
echo "-----------------Building Client...-----------------"
npm run build
echo "-----------------All Done-----------------"

exit 0