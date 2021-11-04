@echo off

echo -^> Running service locally...
cd Data\TestWeb\crossplatform
set ASPNETCORE_ENVIRONMENT=Development
set Logging__LogLevel__Default=Information
set Logging__LogLevel__Microsoft=Information
set Logging__LogLevel__Microsoft.Hosting.Lifetime=Information
set Logging__Console__LogLevel__Default=Information
call TestWeb.exe --urls=http://0.0.0.0:5002/
