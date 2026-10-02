# ── Stage 1: build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore – done in a separate layer so restores are cached unless .csproj changes
COPY backend-api/CareFlowAI.API.csproj backend-api/
COPY ai-orchestrator/CareFlowAI.Orchestrator.csproj ai-orchestrator/
RUN dotnet restore backend-api/CareFlowAI.API.csproj

# Copy the rest of the source and publish
COPY backend-api/ backend-api/
COPY ai-orchestrator/ ai-orchestrator/
WORKDIR /src/backend-api
RUN dotnet publish CareFlowAI.API.csproj \
      --configuration Release \
      --output /app/publish \
      --no-restore

# ── Stage 2: runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=build /app/publish .

# ASP.NET Core listens on port 8080 by default in containers
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CareFlowAI.API.dll"]
