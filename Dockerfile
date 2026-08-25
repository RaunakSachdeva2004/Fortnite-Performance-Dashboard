# Multi-stage Dockerfile for ASP.NET Core 8 MVC Esports Performance Platform

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["FortniteDashboard.csproj", "./"]
RUN dotnet restore "FortniteDashboard.csproj"

# Copy source code and build
COPY . .
RUN dotnet build "FortniteDashboard.csproj" -c Release -o /app/build

# Publish application
FROM build AS publish
RUN dotnet publish "FortniteDashboard.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 80
EXPOSE 443

# Disable file watching in container environments to prevent inotify file descriptor limit exceptions
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FortniteDashboard.dll"]
