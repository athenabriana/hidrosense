# ---- build: restore with cached layers, then publish ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/HidroSense.Api/HidroSense.Api.csproj src/HidroSense.Api/
RUN dotnet restore src/HidroSense.Api/HidroSense.Api.csproj

COPY src/ src/
RUN dotnet publish src/HidroSense.Api/HidroSense.Api.csproj -c Release -o /app/publish /p:UseAppHost=false --no-restore

# ---- runtime: slim ASP.NET image, non-root user ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# curl is only used by the container HEALTHCHECK.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080
USER $APP_UID

HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=5 \
    CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "HidroSense.Api.dll"]
