#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-alpine AS base
WORKDIR /app
COPY ["./ServerApp/Certificates/server.pfx", "/app/Certificates/"]
COPY ["./ServerApp/Certificates/pass", "/app/Certificates/"]

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src
COPY ["ServerApp/ServerApp.csproj", "ServerApp/"]
RUN dotnet restore "ServerApp/ServerApp.csproj"
COPY . .
WORKDIR "/src/ServerApp"
RUN dotnet build "ServerApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ServerApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServerApp.dll"]