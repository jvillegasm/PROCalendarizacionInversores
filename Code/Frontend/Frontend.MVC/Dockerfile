# ============================================================
# Dockerfile — Frontend MVC
# Sistema de Calendarización de Inversores · PROCOMER-CALEND-2026
#
# Contexto de build: directorio Code/ (raíz de la solución)
#   docker build -f Frontend/Dockerfile -t frontend-mvc .
#
# Etapas:
#   1. restore  — recupera paquetes NuGet (cacheada por capas)
#   2. build    — compila en Release (incluye wwwroot estáticos)
#   3. publish  — publica artefactos + static assets
#   4. runtime  — imagen final mínima sin SDK
# ============================================================

# ─── 1. Restaurar dependencias (capa cacheada) ───────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src

# Copiar solo el .csproj primero para aprovechar la caché de Docker.
COPY Frontend/Frontend.MVC/Frontend.MVC.csproj Frontend/Frontend.MVC/

RUN dotnet restore Frontend/Frontend.MVC/Frontend.MVC.csproj \
    --runtime linux-x64

# ─── 2. Compilar ─────────────────────────────────────────────────────────────
FROM restore AS build

# Copiar todo el proyecto (incluye wwwroot/, Views/, Controllers/, etc.)
COPY Frontend/Frontend.MVC/ Frontend/Frontend.MVC/

RUN dotnet build Frontend/Frontend.MVC/Frontend.MVC.csproj \
    -c Release \
    --no-restore \
    -o /app/build

# ─── 3. Publicar ─────────────────────────────────────────────────────────────
FROM build AS publish

RUN dotnet publish Frontend/Frontend.MVC/Frontend.MVC.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained false \
    --no-restore \
    /p:UseAppHost=false \
    -o /app/publish

# ─── 4. Runtime — imagen final mínima ────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

LABEL org.opencontainers.image.title="Frontend MVC" \
      org.opencontainers.image.description="PROCOMER · Capa de presentación ASP.NET Core MVC — Sistema de Calendarización de Inversores." \
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
#   ServiceUrls__CatalogoService  (ej: https://catalogo.internal.azurecontainerapps.io)
#   ServiceUrls__AgendasService   (ej: https://agendas.internal.azurecontainerapps.io)

ENTRYPOINT ["dotnet", "Frontend.MVC.dll"]
