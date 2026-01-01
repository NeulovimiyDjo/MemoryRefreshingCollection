#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["MyApp/MyApp.csproj", "MyApp/"]
RUN dotnet restore "MyApp/MyApp.csproj"
COPY . .
WORKDIR "/src/MyApp"
RUN dotnet build "MyApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyApp.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:IS_DOCKER=true

FROM docker.io/library/node:20-alpine AS node_build
WORKDIR /src
COPY ["MyApp/ClientApp/package.json", "."]
COPY ["MyApp/ClientApp/package-lock.json", "."]
RUN npm install
COPY ["MyApp/ClientApp/.", "."]
RUN npm run build

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=node_build /src/build wwwroot
ENV ASPNETCORE_URLS="https://+:443;http://+:80"
ENTRYPOINT ["dotnet", "MyApp.dll"]