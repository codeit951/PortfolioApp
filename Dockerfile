# ── Build ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PortfolioApp.Domain/PortfolioApp.Domain.csproj PortfolioApp.Domain/
COPY PortfolioApp.Application/PortfolioApp.Application.csproj PortfolioApp.Application/
COPY PortfolioApp.Infrastructure/PortfolioApp.Infrastructure.csproj PortfolioApp.Infrastructure/
COPY PortfolioApp.Web/PortfolioApp.Web.csproj PortfolioApp.Web/
RUN dotnet restore PortfolioApp.Web/PortfolioApp.Web.csproj

COPY . .
RUN dotnet publish PortfolioApp.Web/PortfolioApp.Web.csproj -c Release -o /app/publish --no-restore

# ── Runtime ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Everything that must survive a container restart lives under /app/data (mount a volume there).
# TLS is expected to terminate at a reverse proxy; trust its X-Forwarded-* headers.
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    ConnectionStrings__DefaultConnection="Data Source=/app/data/portfolio.db" \
    Storage__UploadsPath=/app/data/uploads \
    DataProtection__KeysPath=/app/data/keys

COPY --from=build /app/publish .
RUN mkdir -p /app/data && chown -R app:app /app/data
USER app

VOLUME /app/data
EXPOSE 8080
ENTRYPOINT ["dotnet", "PortfolioApp.Web.dll"]
