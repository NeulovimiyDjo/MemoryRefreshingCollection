#!/usr/bin/env bash

scriptdir="$(dirname "$(readlink -f "$0")")"
cd "$scriptdir"

cd ../server


dotnet ef migrations add InitialCreate --context AppointmentsContext
dotnet ef migrations add InitialCreate --context IdentityContext

dotnet ef database update --context IdentityContext
dotnet ef database update --context AppointmentsContext


exit 0