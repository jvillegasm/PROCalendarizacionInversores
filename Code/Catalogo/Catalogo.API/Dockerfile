# ============================================================
# Dockerfile — Catálogo Service
# Sistema de Calendarización de Inversores · PROCOMER-CALEND-2026
#
# Contexto de build: directorio Code/ (raíz de la solución)
#   docker build -f Catalogo/Dockerfile -t catalogo-service .
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
# Si ningún .csproj cambia, 'dotnet restore' no vuelve a ejecutarse.
COPY Catalogo/Catalogo.Domain/Catalogo.Domain.csproj           Catalogo/Catalogo.Domain/
COPY Catalogo/Catalogo.Application/Catalogo.Application.csproj  Catalogo/Catalogo.Application/
COPY Catalogo/Catalogo.Infrastructure/Catalogo.Infrastructure.csproj Catalogo/Catalogo.Infrastructure/
COPY Catalogo/Catalogo.API/Catalogo.API.csproj                   Catalogo/Catalogo.API/

RUN dotnet restore Catalogo/Catalogo.API/Catalogo.API.csproj \
    --runtime linux-x64

# ─── 2. Compilar ─────────────────────────────────────────────────────────────
FROM restore AS build

# Copiar código fuente del servicio
COPY Catalogo/ Catalogo/

RUN dotnet build Catalogo/Catalogo.API/Catalogo.API.csproj \
    -c Release \
    --no-restore \
    -o /app/build

# ─── 3. Publicar ─────────────────────────────────────────────────────────────
FROM build AS publish

RUN dotnet publish Catalogo/Catalogo.API/Catalogo.API.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained false \
    --no-restore \
    /p:UseAppHost=false \
    -o /app/publish

# ─── 4. Runtime — imagen final mínima ────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

LABEL org.opencontainers.image.title="Catálogo Service" \
      org.opencontainers.image.description="PROCOMER · Microservicio de catálogo: inversores, participantes, oficinas y traslados." \
      org.opencontainers.image.vendor="PROCOMER"

WORKDIR /app

# ── Seguridad: usuario sin privilegios (OWASP A05 — Security Misconfiguration)
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

# La cadena de conexión y AllowedOrigins se inyectan como secretos
# en Azure Container Apps (no se incluyen en la imagen).

ENTRYPOINT ["dotnet", "Catalogo.API.dll"]
