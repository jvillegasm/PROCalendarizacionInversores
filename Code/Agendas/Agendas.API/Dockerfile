# ============================================================
# Dockerfile — Agendas Service
# Sistema de Calendarización de Inversores · PROCOMER-CALEND-2026
#
# Contexto de build: directorio Code/ (raíz de la solución)
#   docker build -f Agendas/Dockerfile -t agendas-service .
#
# Etapas:
#   1. restore  — recupera paquetes NuGet (cacheada por capas)
#   2. build    — compila en Release
#   3. publish  — publica artefactos optimizados
#   4. runtime  — imagen final mínima sin SDK
# ============================================================

# ─── 1. Restaurar dependencias (capa cacheada) ───────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src

# Copiar solo los archivos .csproj primero para aprovechar la caché de Docker.
COPY Agendas/Agendas.Domain/Agendas.Domain.csproj             Agendas/Agendas.Domain/
COPY Agendas/Agendas.Application/Agendas.Application.csproj   Agendas/Agendas.Application/
COPY Agendas/Agendas.Infrastructure/Agendas.Infrastructure.csproj Agendas/Agendas.Infrastructure/
COPY Agendas/Agendas.API/Agendas.API.csproj                    Agendas/Agendas.API/

RUN dotnet restore Agendas/Agendas.API/Agendas.API.csproj \
    --runtime linux-x64

# ─── 2. Compilar ─────────────────────────────────────────────────────────────
FROM restore AS build

COPY Agendas/ Agendas/

RUN dotnet build Agendas/Agendas.API/Agendas.API.csproj \
    -c Release \
    --no-restore \
    -o /app/build

# ─── 3. Publicar ─────────────────────────────────────────────────────────────
FROM build AS publish

RUN dotnet publish Agendas/Agendas.API/Agendas.API.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained false \
    --no-restore \
    /p:UseAppHost=false \
    -o /app/publish

# ─── 4. Runtime — imagen final mínima ────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

LABEL org.opencontainers.image.title="Agendas Service" \
      org.opencontainers.image.description="PROCOMER · Microservicio de agendas: motor de scheduling y generación de itinerarios." \
      org.opencontainers.image.vendor="PROCOMER"

WORKDIR /app

# ── Seguridad: usuario sin privilegios (OWASP A05)
RUN adduser --disabled-password --gecos "" --uid 1001 appuser \
    && chown -R appuser:appuser /app

COPY --from=publish --chown=appuser:appuser /app/publish .

USER appuser

# Azure Container Apps termina TLS en el ingress; el contenedor escucha en HTTP.
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_ENVIRONMENT=Production

# Variables de entorno requeridas en runtime (inyectadas como secretos en Azure):
#   ConnectionStrings__DefaultConnection
#   ServiceUrls__CatalogoService
#   ServiceUrls__PdfService
#   AllowedOrigins__0 (y subsiguientes)

ENTRYPOINT ["dotnet", "Agendas.API.dll"]
