FROM registry.access.redhat.com/ubi8/dotnet-50-runtime:5.0-39 AS base
USER root
ENV ASPNETCORE_URLS=
ENV ASPNETCORE_ENVIRONMENT=Development
STOPSIGNAL SIGINT
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LC_ALL=C.UTF-8
ENV Logging__LogLevel__Default=Information
ENV Logging__LogLevel__Microsoft=Information
ENV Logging__LogLevel__Microsoft.Hosting.Lifetime=Information
ENV Logging__Console__LogLevel__Default=Information
WORKDIR /Data/TestWeb/crossplatform
