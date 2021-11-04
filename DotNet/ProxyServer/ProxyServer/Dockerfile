#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src
COPY ["ProxyServer/ProxyServer.csproj", "ProxyServer/"]
RUN dotnet restore "ProxyServer/ProxyServer.csproj"
COPY . .
WORKDIR "/src/ProxyServer"
RUN dotnet build "ProxyServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProxyServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProxyServer.dll"]