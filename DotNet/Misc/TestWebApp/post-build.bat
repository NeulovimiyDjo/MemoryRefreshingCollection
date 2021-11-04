setlocal
cd /D "%~dp0..\..\.."
set BuildConfigurationParam=%1

(mkdir "Data\TestWeb\crossplatform")>nul 2>&1
echo Publishing TestWeb %BuildConfigurationParam%...
dotnet publish "Source/TestServices/TestWeb" -c %BuildConfigurationParam% --no-build --no-self-contained --nologo -v q -o "Data/TestWeb/crossplatform/"
(del "Data\TestWeb\crossplatform\TestWeb.StaticWebAssets.xml")>nul 2>&1
