@echo off

echo -^> Removing prevoius compose ifany...
docker-compose ^
    -f "docker-compose.yml" ^
    -p redis-compose ^
    down --volumes

echo -^> Running compose...
docker-compose ^
    -f "docker-compose.yml" ^
    -p redis-compose ^
    up

echo -^> Removing compose...
docker-compose ^
    -f "docker-compose.yml" ^
    -p redis-compose ^
    down --volumes

@pause
