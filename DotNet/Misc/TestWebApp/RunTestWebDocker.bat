@echo off

echo -^> Running service in docker...
call docker run --pull=never --rm -it --name RunTestWebManual -p 5002:5002 -v "%cd%/Data:/Data" --entrypoint dotnet run_testweb:dev ./TestWeb.dll --urls=http://0.0.0.0:5002/
