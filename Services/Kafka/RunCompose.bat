@echo off

echo -^> Removing prevoius compose ifany...
docker-compose ^
    -f "docker-compose.yml" ^
    -p kafka-compose ^
    down --volumes

echo -^> Running compose...
docker-compose ^
    -f "docker-compose.yml" ^
    -p kafka-compose ^
    up --no-build --no-recreate

echo -^> Removing compose...
docker-compose ^
    -f "docker-compose.yml" ^
    -p kafka-compose ^
    down --volumes

@pause
