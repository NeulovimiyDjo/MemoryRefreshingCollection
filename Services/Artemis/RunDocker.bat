@echo off

echo -^> Running service in docker...
docker run --rm -it --name Artemis -p 61616:61616 -p 8161:8161 apache/activemq-artemis:2.39.0

@pause
