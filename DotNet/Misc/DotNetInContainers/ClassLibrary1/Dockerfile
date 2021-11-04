#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ClassLibrary1/ClassLibrary1.csproj", "ClassLibrary1/"]
RUN dotnet restore "ClassLibrary1/ClassLibrary1.csproj"
COPY . .
WORKDIR "/src/WorkerService1"
RUN dotnet build "WorkerService1.csproj" -c Release -o /app/build/srv
WORKDIR "/src/ClassLibrary1"
RUN dotnet build "ClassLibrary1.csproj" -c Release -o /app/build/lib

FROM build AS publish
WORKDIR /src
RUN dotnet publish "WorkerService1/WorkerService1.csproj" -c Release -o /app/publish/srv

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish/srv .
COPY --from=publish /app/build/lib .
ENTRYPOINT ["dotnet", "WorkerService1.dll"]