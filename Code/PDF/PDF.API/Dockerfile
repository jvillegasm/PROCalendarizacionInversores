# ============================================================
# Dockerfile — PDF Service
# Sistema de Calendarización de Inversores · PROCOMER-CALEND-2026
#
# Contexto de build: directorio Code/ (raíz de la solución)
#   docker build -f PDF/Dockerfile -t pdf-service .
#
# NOTA IMPORTANTE — QuestPDF en Linux:
#   QuestPDF 2025.x usa SkiaSharp/HarfBuzzSharp con binarios nativos
#   incluidos en el paquete NuGet. Sin embargo, libfontconfig1 debe
#   estar presente en el host para renderizado correcto de fuentes.
#   Se instala explícitamente en la imagen runtime.
#
# Etapas:
#   1. restore  — recupera paquetes NuGet (cacheada por capas)
#   2. build    — compila en Release
#   3. publish  — publica artefactos optimizados
#   4. runtime  — imagen final con dependencias de fuentes para QuestPDF
# ============================================================

# ─── 1. Restaurar dependencias (capa cacheada) ───────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src

# Copiar solo los archivos .csproj primero para aprovechar la caché de Docker.
COPY PDF/PDF.Domain/PDF.Domain.csproj               PDF/PDF.Domain/
COPY PDF/PDF.Application/PDF.Application.csproj     PDF/PDF.Application/
COPY PDF/PDF.Infrastructure/PDF.Infrastructure.csproj PDF/PDF.Infrastructure/
COPY PDF/PDF.API/PDF.API.csproj                     PDF/PDF.API/

RUN dotnet restore PDF/PDF.API/PDF.API.csproj \
    --runtime linux-x64

# ─── 2. Compilar ─────────────────────────────────────────────────────────────
FROM restore AS build

COPY PDF/ PDF/

RUN dotnet build PDF/PDF.API/PDF.API.csproj \
    -c Release \
    --no-restore \
    -o /app/build

# ─── 3. Publicar ─────────────────────────────────────────────────────────────
FROM build AS publish

RUN dotnet publish PDF/PDF.API/PDF.API.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained false \
    --no-restore \
    /p:UseAppHost=false \
    -o /app/publish

# ─── 4. Runtime — imagen final con soporte de fuentes para QuestPDF ──────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

LABEL org.opencontainers.image.title="PDF Service" \
      org.opencontainers.image.description="PROCOMER · Microservicio de generación de PDF de itinerarios (QuestPDF)." \
      org.opencontainers.image.vendor="PROCOMER"

WORKDIR /app

# ── Dependencias nativas requeridas por QuestPDF / SkiaSharp en Debian Bookworm
#    libfontconfig1 : configuración y sustitución de fuentes
#    libfreetype6   : renderizado de fuentes TrueType/OpenType
#    fonts-liberation: fuentes Liberation (Arial/Times equivalentes) para el documento
#    Se limpian las listas de apt para reducir el tamaño de la imagen.
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
       libfontconfig1 \
       libfreetype6 \
       fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

# ── Seguridad: usuario sin privilegios (OWASP A05)
RUN adduser --disabled-password --gecos "" --uid 1001 appuser \
    && chown -R appuser:appuser /app

COPY --from=publish --chown=appuser:appuser /app/publish .

USER appuser

# Este servicio solo recibe llamadas internas del Agendas Service.
# No requiere CORS. Azure Container Apps lo expone con ingress interno.
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_ENVIRONMENT=Production \
    # QuestPDF Community License (no requiere clave de API)
    QuestPDF__LicenseType=Community

ENTRYPOINT ["dotnet", "PDF.API.dll"]
