@echo off


echo -^> Running service in docker...
call docker run --pull=never --rm -it --name AspireDashboard -p 18888:18888 -p 18889:18889 -p 18890:18890 mcr.microsoft.com/dotnet/aspire-dashboard:8.2.1
if %errorlevel% neq 0 goto :Error
goto :Success


:Error
pause
:Success
exit /b 0