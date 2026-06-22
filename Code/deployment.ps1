# =============================================================================
# PROCalendarizacionInversores — Despliegue en Azure Container Apps
# Archivo: deployment.ps1
# Ubicación: Code/deployment.ps1
# Ejecutar desde la carpeta Code/ (donde está PROCalendarizacionInversores.sln)
#
# Solución:  4 Container Apps
#   1. catalogo-api   (microservicio de Catálogo · acceso SQL)
#   2. pdf-api        (microservicio de PDF · sin acceso SQL)
#   3. agendas-api    (microservicio orquestador · acceso SQL + consume Catálogo + PDF)
#   4. frontend-mvc   (ASP.NET MVC · consume Catálogo y Agendas)
#
# Infraestructura Azure asumida COMO YA PROVISIONADA (el script sólo la verifica):
#   - Resource Group:               rsgr-E02-TST-EaUS
#   - Azure Container Registry:     acre02prd
#   - Container Apps Environment:   rs-cae-e02-tst-4e4d-eaus-1
#   - Azure SQL Server (logical):   rs-dbs-pte02-tst-4e4d-eaus-1.database.windows.net
#   - Database:                     PROInversores
# =============================================================================
# ⚠️  ADVERTENCIA DE SEGURIDAD: La cadena de conexión a SQL aparece en texto plano
#     por requerimiento explícito de esta prueba técnica. En un entorno productivo
#     real se debe almacenar el password en Azure Key Vault y consumirlo desde
#     Container Apps mediante secretref: (secrets + --secrets).
# =============================================================================

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# =============================================================================
# Sección 1: Variables de infraestructura
# =============================================================================
$RESOURCE_GROUP = "rsgr-E02-TST-EaUS"
$LOCATION       = "eastus"
$ACR_NAME       = "acre02prd"
$CAE_NAME       = "rs-cae-e02-tst-4e4d-eaus-1"

# Contexto de build: directorio actual (Code/, donde está el .sln)
$BUILD_CONTEXT  = "."

# Cadena de conexión compartida por catalogo-api y agendas-api
$DB_CONNECTION  = "Server=rs-dbs-pte02-tst-4e4d-eaus-1.database.windows.net;Database=PROInversores;User Id=usr_e02_tst;Password=Mj47Rw2025!;Encrypt=True;TrustServerCertificate=False;"

# Nombres lógicos de Container Apps e imágenes
$APP_CATALOGO   = "catalogo-api"
$APP_PDF        = "pdf-api"
$APP_AGENDAS    = "agendas-api"
$APP_FRONTEND   = "frontend-mvc"

# Rutas de Dockerfile (relativas al $BUILD_CONTEXT)
$DOCKERFILE_CATALOGO = "Catalogo/Catalogo.API/Dockerfile"
$DOCKERFILE_PDF      = "PDF/PDF.API/Dockerfile"
$DOCKERFILE_AGENDAS  = "Agendas/Agendas.API/Dockerfile"
$DOCKERFILE_FRONTEND = "Frontend/Frontend.MVC/Dockerfile"

# =============================================================================
# Sección 2: Verificación del Resource Group (NO se crea, sólo se valida)
# =============================================================================
Write-Host "`n>>> Verificando Resource Group '$RESOURCE_GROUP'..." -ForegroundColor Cyan
az group show --name $RESOURCE_GROUP 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Error "El Resource Group '$RESOURCE_GROUP' no existe. Debe estar provisionado por PROCOMER antes de ejecutar este script."
    exit 1
}
Write-Host "    Resource Group encontrado." -ForegroundColor Green

# =============================================================================
# Sección 3: Verificación del Azure Container Registry + credenciales
# =============================================================================
Write-Host "`n>>> Verificando Azure Container Registry '$ACR_NAME'..." -ForegroundColor Cyan
az acr show --name $ACR_NAME --resource-group $RESOURCE_GROUP 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Error "El Container Registry '$ACR_NAME' no existe en el Resource Group '$RESOURCE_GROUP'."
    exit 1
}
Write-Host "    ACR encontrado." -ForegroundColor Green

# Habilitar autenticación de administrador para que Container Apps haga pull
Write-Host "    Habilitando admin-enabled en ACR..." -ForegroundColor Yellow
az acr update --name $ACR_NAME --admin-enabled true | Out-Null

$ACR_SERVER   = "$ACR_NAME.azurecr.io"
$ACR_USERNAME = (az acr credential show --name $ACR_NAME --query "username" -o tsv)
$ACR_PASSWORD = (az acr credential show --name $ACR_NAME --query "passwords[0].value" -o tsv)
Write-Host "    Credenciales ACR capturadas (usuario: $ACR_USERNAME)." -ForegroundColor Green

# =============================================================================
# Sección 4: Build y push de imágenes (build remoto en ACR, sin Docker local)
# =============================================================================
Write-Host "`n>>> Construyendo imágenes en ACR desde '$BUILD_CONTEXT'..." -ForegroundColor Cyan

Write-Host "    [1/4] $APP_CATALOGO..." -ForegroundColor Yellow
az acr build `
    --registry $ACR_NAME `
    --resource-group $RESOURCE_GROUP `
    --image "${APP_CATALOGO}:latest" `
    --file $DOCKERFILE_CATALOGO `
    $BUILD_CONTEXT

Write-Host "    [2/4] $APP_PDF..." -ForegroundColor Yellow
az acr build `
    --registry $ACR_NAME `
    --resource-group $RESOURCE_GROUP `
    --image "${APP_PDF}:latest" `
    --file $DOCKERFILE_PDF `
    $BUILD_CONTEXT

Write-Host "    [3/4] $APP_AGENDAS..." -ForegroundColor Yellow
az acr build `
    --registry $ACR_NAME `
    --resource-group $RESOURCE_GROUP `
    --image "${APP_AGENDAS}:latest" `
    --file $DOCKERFILE_AGENDAS `
    $BUILD_CONTEXT

Write-Host "    [4/4] $APP_FRONTEND..." -ForegroundColor Yellow
az acr build `
    --registry $ACR_NAME `
    --resource-group $RESOURCE_GROUP `
    --image "${APP_FRONTEND}:latest" `
    --file $DOCKERFILE_FRONTEND `
    $BUILD_CONTEXT

# =============================================================================
# Sección 5: Verificación del Container Apps Environment (NO se crea)
# =============================================================================
Write-Host "`n>>> Verificando Container Apps Environment '$CAE_NAME'..." -ForegroundColor Cyan
az containerapp env show --name $CAE_NAME --resource-group $RESOURCE_GROUP 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Error "El Container Apps Environment '$CAE_NAME' no existe en el Resource Group '$RESOURCE_GROUP'."
    exit 1
}
Write-Host "    Environment encontrado." -ForegroundColor Green

# =============================================================================
# Sección 6: catalogo-api — Ingress EXTERNO (acceso público + acceso SQL)
# =============================================================================
Write-Host "`n>>> [1/4] Desplegando '$APP_CATALOGO' (ingress externo)..." -ForegroundColor Cyan

az containerapp show --name $APP_CATALOGO --resource-group $RESOURCE_GROUP 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    az containerapp create `
        --name $APP_CATALOGO `
        --resource-group $RESOURCE_GROUP `
        --environment $CAE_NAME `
        --image "$ACR_SERVER/${APP_CATALOGO}:latest" `
        --registry-server $ACR_SERVER `
        --registry-username $ACR_USERNAME `
        --registry-password $ACR_PASSWORD `
        --ingress external `
        --target-port 8080 `
        --min-replicas 1 `
        --max-replicas 3 `
        --env-vars `
            "ASPNETCORE_ENVIRONMENT=Production" `
            "ConnectionStrings__DefaultConnection=$DB_CONNECTION" `
            "AllowedHosts=*" `
            "Logging__LogLevel__Default=Information" `
            "Logging__LogLevel__Microsoft.AspNetCore=Warning"
} else {
    Write-Host "    '$APP_CATALOGO' ya existe, actualizando imagen..." -ForegroundColor Yellow
    az containerapp update `
        --name $APP_CATALOGO `
        --resource-group $RESOURCE_GROUP `
        --image "$ACR_SERVER/${APP_CATALOGO}:latest"
}

# Capturar FQDN público para inyectarlo en agendas-api y frontend-mvc
$CATALOGO_FQDN = (az containerapp show `
    --name $APP_CATALOGO `
    --resource-group $RESOURCE_GROUP `
    --query "properties.configuration.ingress.fqdn" -o tsv)
Write-Host "    FQDN público $APP_CATALOGO : $CATALOGO_FQDN" -ForegroundColor Green

# =============================================================================
# Sección 7: pdf-api — Ingress EXTERNO (acceso público, sin SQL)
# =============================================================================
Write-Host "`n>>> [2/4] Desplegando '$APP_PDF' (ingress externo)..." -ForegroundColor Cyan

az containerapp show --name $APP_PDF --resource-group $RESOURCE_GROUP 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    az containerapp create `
        --name $APP_PDF `
        --resource-group $RESOURCE_GROUP `
        --environment $CAE_NAME `
        --image "$ACR_SERVER/${APP_PDF}:latest" `
        --registry-server $ACR_SERVER `
        --registry-username $ACR_USERNAME `
        --registry-password $ACR_PASSWORD `
        --ingress external `
        --target-port 8080 `
        --min-replicas 1 `
        --max-replicas 3 `
        --env-vars `
            "ASPNETCORE_ENVIRONMENT=Production" `
            "AllowedHosts=*" `
            "Logging__LogLevel__Default=Information" `
            "Logging__LogLevel__Microsoft.AspNetCore=Warning"
} else {
    Write-Host "    '$APP_PDF' ya existe, actualizando imagen..." -ForegroundColor Yellow
    az containerapp update `
        --name $APP_PDF `
        --resource-group $RESOURCE_GROUP `
        --image "$ACR_SERVER/${APP_PDF}:latest"
}

# Capturar FQDN público para inyectarlo en agendas-api
$PDF_FQDN = (az containerapp show `
    --name $APP_PDF `
    --resource-group $RESOURCE_GROUP `
    --query "properties.configuration.ingress.fqdn" -o tsv)
Write-Host "    FQDN público $APP_PDF : $PDF_FQDN" -ForegroundColor Green

# =============================================================================
# Sección 8: agendas-api — Ingress EXTERNO (orquestador · usa Catálogo + PDF)
# =============================================================================
Write-Host "`n>>> [3/4] Desplegando '$APP_AGENDAS' (ingress externo)..." -ForegroundColor Cyan

az containerapp show --name $APP_AGENDAS --resource-group $RESOURCE_GROUP 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    az containerapp create `
        --name $APP_AGENDAS `
        --resource-group $RESOURCE_GROUP `
        --environment $CAE_NAME `
        --image "$ACR_SERVER/${APP_AGENDAS}:latest" `
        --registry-server $ACR_SERVER `
        --registry-username $ACR_USERNAME `
        --registry-password $ACR_PASSWORD `
        --ingress external `
        --target-port 8080 `
        --min-replicas 1 `
        --max-replicas 3 `
        --env-vars `
            "ASPNETCORE_ENVIRONMENT=Production" `
            "ConnectionStrings__DefaultConnection=$DB_CONNECTION" `
            "ServiceUrls__CatalogoService=https://$CATALOGO_FQDN" `
            "ServiceUrls__PdfService=https://$PDF_FQDN" `
            "AllowedHosts=*" `
            "Logging__LogLevel__Default=Information" `
            "Logging__LogLevel__Microsoft.AspNetCore=Warning"
} else {
    Write-Host "    '$APP_AGENDAS' ya existe, actualizando imagen y URLs de servicios..." -ForegroundColor Yellow
    az containerapp update `
        --name $APP_AGENDAS `
        --resource-group $RESOURCE_GROUP `
        --image "$ACR_SERVER/${APP_AGENDAS}:latest" `
        --set-env-vars `
            "ServiceUrls__CatalogoService=https://$CATALOGO_FQDN" `
            "ServiceUrls__PdfService=https://$PDF_FQDN"
}

# Capturar FQDN público para inyectarlo en frontend-mvc
$AGENDAS_FQDN = (az containerapp show `
    --name $APP_AGENDAS `
    --resource-group $RESOURCE_GROUP `
    --query "properties.configuration.ingress.fqdn" -o tsv)
Write-Host "    FQDN público $APP_AGENDAS : $AGENDAS_FQDN" -ForegroundColor Green

# =============================================================================
# Sección 9: frontend-mvc — Ingress EXTERNO (consumidor de Catálogo + Agendas)
# =============================================================================
Write-Host "`n>>> [4/4] Desplegando '$APP_FRONTEND' (ingress externo)..." -ForegroundColor Cyan

az containerapp show --name $APP_FRONTEND --resource-group $RESOURCE_GROUP 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    az containerapp create `
        --name $APP_FRONTEND `
        --resource-group $RESOURCE_GROUP `
        --environment $CAE_NAME `
        --image "$ACR_SERVER/${APP_FRONTEND}:latest" `
        --registry-server $ACR_SERVER `
        --registry-username $ACR_USERNAME `
        --registry-password $ACR_PASSWORD `
        --ingress external `
        --target-port 8080 `
        --min-replicas 1 `
        --max-replicas 3 `
        --env-vars `
            "ASPNETCORE_ENVIRONMENT=Production" `
            "ServiceUrls__CatalogoService=https://$CATALOGO_FQDN" `
            "ServiceUrls__AgendasService=https://$AGENDAS_FQDN" `
            "AllowedHosts=*" `
            "Logging__LogLevel__Default=Information" `
            "Logging__LogLevel__Microsoft.AspNetCore=Warning"
} else {
    Write-Host "    '$APP_FRONTEND' ya existe, actualizando imagen y URLs de servicios..." -ForegroundColor Yellow
    az containerapp update `
        --name $APP_FRONTEND `
        --resource-group $RESOURCE_GROUP `
        --image "$ACR_SERVER/${APP_FRONTEND}:latest" `
        --set-env-vars `
            "ServiceUrls__CatalogoService=https://$CATALOGO_FQDN" `
            "ServiceUrls__AgendasService=https://$AGENDAS_FQDN"
}

# Capturar FQDN público del frontend
$FRONTEND_FQDN = (az containerapp show `
    --name $APP_FRONTEND `
    --resource-group $RESOURCE_GROUP `
    --query "properties.configuration.ingress.fqdn" -o tsv)

# =============================================================================
# Sección 9.5: Configurar CORS en las APIs con el FQDN real del frontend
# Se hace DESPUÉS de desplegar el frontend porque su FQDN no se conoce antes.
# Tanto en primer despliegue (create) como en actualizaciones (update) las APIs
# necesitan este valor para emitir el header Access-Control-Allow-Origin.
# =============================================================================
Write-Host "`n>>> Configurando CORS en APIs con FQDN del frontend..." -ForegroundColor Cyan
$FRONTEND_ORIGIN = "https://$FRONTEND_FQDN"

Write-Host "    Actualizando AllowedOrigins en '$APP_CATALOGO'..." -ForegroundColor Yellow
az containerapp update `
    --name $APP_CATALOGO `
    --resource-group $RESOURCE_GROUP `
    --set-env-vars "AllowedOrigins__0=$FRONTEND_ORIGIN" | Out-Null

Write-Host "    Actualizando AllowedOrigins en '$APP_AGENDAS'..." -ForegroundColor Yellow
az containerapp update `
    --name $APP_AGENDAS `
    --resource-group $RESOURCE_GROUP `
    --set-env-vars "AllowedOrigins__0=$FRONTEND_ORIGIN" | Out-Null

Write-Host "    CORS habilitado para: $FRONTEND_ORIGIN" -ForegroundColor Green

# =============================================================================
# Sección 10: Resumen final
# =============================================================================
Write-Host "`n=============================================================" -ForegroundColor Cyan
Write-Host "  DESPLIEGUE COMPLETADO EXITOSAMENTE" -ForegroundColor Green
Write-Host "=============================================================" -ForegroundColor Cyan
Write-Host ("  Catalogo API  : https://{0}" -f $CATALOGO_FQDN) -ForegroundColor White
Write-Host ("  Agendas API   : https://{0}" -f $AGENDAS_FQDN)  -ForegroundColor White
Write-Host ("  PDF API       : https://{0}" -f $PDF_FQDN)      -ForegroundColor White
Write-Host ("  Frontend MVC  : https://{0}" -f $FRONTEND_FQDN) -ForegroundColor White
Write-Host "=============================================================" -ForegroundColor Cyan
