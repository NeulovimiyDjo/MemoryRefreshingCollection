powershell -NoProfile -ExecutionPolicy Bypass -Command ^
	"$MSBuildPath = '%MSBuildPath%';"^
	"$Debug = $true;"^
	"$ErrorActionPreference = 'Stop';"^
	"Import-Module .\Scripts\HelperFunctions\BuildHelper.psm1;"^
	"BuildSolution -solutionPath '.\src\Solution.sln'"^
	2>&1