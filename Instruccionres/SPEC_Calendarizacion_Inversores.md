# Sistema de Calendarización de Inversores — Spec Técnico

**Gate 1 · Discovery & Requirements — Fase 1**

| | |
|---|---|
| **Clasificación del documento** | USO INTERNO — PRUEBA TÉCNICA |
| **Proyecto** | Sistema de Calendarización de Inversores |
| **ID del proyecto** | PROCOMER-CALEND-2026 |
| **Cliente** | PROCOMER (Promotora del Comercio Exterior de Costa Rica) |
| **Vertical** | Atracción de Inversión Extranjera |
| **Contratación** | 2026XE-000001-0001700001 |
| **Propietario** | Equipo participante designado |
| **Versión** | 1.0 · Junio 2026 |
| **Estado** | 🟡 Borrador — en desarrollo activo |
| **Documento base** | Prueba_Técnica.md — Caso: Calendarización de Inversores |

---

## Tabla de Contenidos

1. [Objetivo](#1-objetivo)
2. [Alcance](#2-alcance)
3. [Criterios de aceptación](#3-criterios-de-aceptación)
4. [Requisitos no-funcionales](#4-requisitos-no-funcionales)
5. [Datos involucrados](#5-datos-involucrados)
6. [Riesgos y supuestos](#6-riesgos-y-supuestos)
7. [Stack tecnológico y herramientas](#7-stack-tecnológico-y-herramientas)
8. [Diseño del motor de scheduling](#8-diseño-del-motor-de-scheduling)
9. [Observabilidad, outputs esperados y decisiones pendientes](#9-observabilidad-outputs-esperados-y-decisiones-pendientes)
10. [Estándar de documentación de código](#10-estándar-de-documentación-de-código)
11. [Criterios de éxito](#11-criterios-de-éxito)
12. [Checklist de aprobación Gate 1](#12-checklist-de-aprobación-gate-1)

---

## 1. Objetivo

**Problema que resuelve:**
PROCOMER recibe periódicamente inversores internacionales que visitan Costa Rica para evaluar oportunidades de inversión. Estas visitas tienen una duración limitada y deben aprovecharse al máximo coordinando reuniones con funcionarios públicos, representantes institucionales y aliados estratégicos. La coordinación se gestiona hoy de forma manual, generando tres clases de fallos recurrentes:

1. **Traslapes de horarios** — un participante queda convocado a dos reuniones simultáneas.
2. **Traslados imposibles** — el tiempo físico de desplazamiento entre dos oficinas supera el intervalo disponible entre reuniones consecutivas.
3. **Incompatibilidad de idioma** — el inversor y el participante no comparten ningún idioma, haciendo inviable la reunión.

El proceso consume tiempo del equipo coordinador y produce itinerarios propensos a fallos el mismo día de la visita, dañando la imagen institucional de PROCOMER.

**Valor esperado:**
Un sistema automatizado que centralice la gestión de inversores, participantes y oficinas, calcule agendas diarias viables y optimizadas para el inversor respetando todas las restricciones operativas, y produzca un documento PDF profesional listo para entregar al visitante.

**Delimitación de la Fase 1:**
Este spec cubre un sistema completo de una sola fase: gestión de datos maestros (inversores, participantes, oficinas, matriz de traslados), motor de scheduling automático y generación de PDF. No se contemplan integraciones con calendarios externos, autenticación de usuarios ni funcionalidades de analítica.

**Stakeholders principales:**

| Rol | Nombre | Expectativa concreta |
|---|---|---|
| Coordinador de agendas (usuario principal) | Funcionarios de PROCOMER | Registrar inversores y participantes, generar agendas automáticas y descargar el PDF del itinerario desde el frontend web |
| Inversor (beneficiario del itinerario) | Visitante extranjero | Recibir un itinerario preciso, sin conflictos de traslado ni de idioma, en español de Costa Rica |
| Panel evaluador | Equipo técnico PROCOMER | Sistema funcional desplegado en Azure Container Apps, con Clean Architecture en cada microservicio, algoritmo de scheduling correcto, código completamente documentado y suite de pruebas unitarias |

---

## 2. Alcance

### Dentro del alcance (Fase 1)

**Módulo de Inversores** *(ref. Prueba Técnica §4.1)*
- CRUD completo de inversores (nombre, empresa, país, idiomas, fechas de visita, lugar de hospedaje).
- Validaciones: al menos un idioma asignado; fecha de cierre ≥ fecha de inicio; no eliminar inversor con agendas activas.
- Exposición de datos del inversor vía API REST para consumo del frontend y del Microservicio de Agendas.

**Módulo de Participantes, Oficinas y Traslados** *(ref. Prueba Técnica §4.2)*
- CRUD de participantes (nombre, cargo/institución, oficina, idiomas, disponibilidad horaria por fecha, estado activo/inactivo).
- CRUD de oficinas (nombre, dirección física, coordenadas opcionales).
- Gestión de la matriz de tiempos de traslado entre oficinas con garantía de simetría automática.
- Validaciones: al menos un idioma y una oficina por participante; no eliminar oficina con participantes activos.

**Módulo de Agendas — Motor de Scheduling** *(ref. Prueba Técnica §4.3)*
- Generación automática de agenda diaria para un inversor, dado un conjunto de candidatos, una fecha, una duración de reunión y una meta de cantidad.
- Filtrado por compatibilidad de idioma, disponibilidad horaria y tiempos de traslado.
- Optimización: maximizar cantidad de reuniones; ante igualdad, minimizar tiempo total de traslados.
- Mensajes explicativos en caso de no encontrar agenda viable.
- Anulación lógica (soft delete) de agendas con conservación del registro histórico y el PDF.
- Consulta y detalle de agendas, con filtros por inversor, fecha y estado.

**Módulo de Generación de PDF** *(ref. Prueba Técnica §4.4)*
- Microservicio dedicado que recibe el JSON de una agenda confirmada y devuelve el archivo PDF.
- El documento se genera únicamente en español (variante Costa Rica).
- Contenido: encabezado institucional, fecha de jornada, tabla de reuniones, tiempos de traslado, pie de página.

**Infraestructura y calidad**
- Despliegue de los tres microservicios backend y el frontend en Azure Container Apps.
- API documentada con Swagger/OpenAPI en cada microservicio, accesible públicamente.
- Suite mínima de 5 pruebas unitarias para el motor de scheduling.
- Todo el código completamente documentado (ver §10).

### Fuera del alcance

- Integración con calendarios externos (Google Calendar, Outlook, Microsoft Teams).
- Notificaciones por correo electrónico o mensajería instantánea.
- Autenticación y autorización de usuarios — no requerida por la prueba técnica.
- Generación del PDF en idiomas distintos al español de Costa Rica.
- Módulo de reportes históricos, dashboard ejecutivo o analítica de uso.
- Integración con CRM, ERP o sistemas externos de PROCOMER.
- Optimización de agendas multi-día (el motor opera por jornada, no por período de visita completo).
- Aplicación móvil o Progressive Web App.
- API Gateway centralizado — el frontend consume los microservicios directamente a través de sus URLs públicas en Azure Container Apps.

### Condiciones de frontera

- Si ningún participante candidato comparte idioma con el inversor, el sistema retorna un mensaje explicativo y no genera la agenda.
- Si ningún participante compatible tiene disponibilidad registrada para la fecha solicitada, el sistema retorna el motivo específico.
- Si los tiempos de traslado impiden encadenar alguna reunión pero permiten otras, el sistema genera la agenda parcial con la máxima cantidad posible e indica cuántas se lograron respecto a la meta.
- Si la fecha de la agenda está fuera del rango de visita del inversor, el sistema rechaza la solicitud con validación de negocio (no con error técnico).
- El bloque 12:00–13:00 es inviolable para reuniones; ninguna reunión puede iniciar ni estar en curso dentro de ese período.
- Una oficina con participantes activos no puede eliminarse; el intento se rechaza con mensaje descriptivo.
- Un inversor con agendas activas no puede eliminarse; el intento se rechaza con mensaje descriptivo.
- La anulación de una agenda es siempre lógica; el registro y su PDF se conservan en la base de datos.

---

## 3. Criterios de aceptación

### AC-01 — Gestión completa de inversores

```gherkin
Feature: Registro y mantenimiento de inversores

  Scenario: Registro exitoso de inversor con todos los campos válidos
    Given el coordinador accede al módulo de inversores en el frontend
    When registra nombre completo, empresa, país de origen, al menos un idioma,
         fecha de inicio y fecha de cierre de visita (cierre >= inicio), y lugar de hospedaje
    Then el sistema persiste al inversor y lo muestra en el listado con todos sus datos
    And el inversor queda disponible para ser seleccionado en el módulo de Agendas

  Scenario: Rechazo por ausencia de idioma (RN-01)
    Given el coordinador intenta registrar un inversor sin asignar ningún idioma
    When envía el formulario
    Then el sistema retorna HTTP 400 con el mensaje "El inversor debe tener al menos un idioma asignado"
    And no persiste ningún registro

  Scenario: Rechazo por fechas inválidas (RN-02)
    Given el coordinador indica una FechaFinVisita anterior a la FechaInicioVisita
    When envía el formulario
    Then el sistema retorna HTTP 400 con el mensaje "La fecha de cierre no puede ser anterior a la fecha de inicio"

  Scenario: Rechazo de eliminación con agendas activas (RN-03)
    Given existe un inversor con al menos una agenda en estado "Activa"
    When el coordinador intenta eliminar al inversor
    Then el sistema retorna HTTP 409 con el mensaje "No es posible eliminar un inversor con agendas activas"
    And el inversor permanece en el sistema con todos sus datos
```

### AC-02 — Gestión completa de participantes

```gherkin
Feature: Registro y mantenimiento de participantes

  Scenario: Registro exitoso de participante con todos los campos válidos
    Given el coordinador accede al módulo de participantes
    When registra nombre completo, cargo/institución, oficina asignada (existente),
         al menos un idioma y bloques de disponibilidad horaria para fechas futuras
    Then el sistema persiste al participante en estado "Activo"
    And el participante queda disponible como candidato en el módulo de Agendas

  Scenario: Rechazo por ausencia de idioma (RN-04)
    Given el coordinador intenta registrar un participante sin asignar ningún idioma
    When envía el formulario
    Then el sistema retorna HTTP 400 con el mensaje "El participante debe tener al menos un idioma asignado"

  Scenario: Rechazo por ausencia de oficina (RN-05)
    Given el coordinador intenta registrar un participante sin asignar una oficina
    When envía el formulario
    Then el sistema retorna HTTP 400 con el mensaje "El participante debe tener una oficina asignada"

  Scenario: Desactivación lógica de participante
    Given un participante está en estado "Activo"
    When el coordinador cambia su estado a "Inactivo"
    Then el participante queda excluido de futuros procesos de scheduling
    And su historial de reuniones anteriores se conserva íntegro
```

### AC-03 — Gestión de oficinas y matriz de traslados

```gherkin
Feature: Gestión de oficinas y tiempos de desplazamiento

  Scenario: Simetría automática en la matriz de traslados (RN-07)
    Given existen las oficinas A y B registradas en el sistema
    When el coordinador ingresa el par de traslado A → B con 25 minutos
    Then el sistema persiste automáticamente también el par B → A con 25 minutos
    And la consulta GET de traslados retorna ambas direcciones con el mismo valor

  Scenario: Rechazo de eliminación de oficina con participantes activos (RN-06)
    Given la oficina "Edificio Central PROCOMER" tiene participantes activos asignados
    When el coordinador intenta eliminar esa oficina
    Then el sistema retorna HTTP 409 con el mensaje
         "No es posible eliminar una oficina con participantes activos asignados"
    And la oficina permanece disponible en el sistema

  Scenario: Actualización de tiempo de traslado mantiene simetría
    Given existe el par de traslado A → B con 20 minutos
    When el coordinador actualiza ese tiempo a 30 minutos
    Then el par B → A se actualiza automáticamente a 30 minutos
```

### AC-04 — Generación automática de agenda viable

```gherkin
Feature: Motor de scheduling de agendas diarias

  Scenario: Generación exitosa de agenda con meta alcanzada
    Given existe un inversor con idiomas [español, inglés] y visita del 2026-07-01 al 2026-07-05
    And existen 5 participantes candidatos con idioma compartido y disponibilidad el 2026-07-02
    And la matriz de traslados tiene tiempos entre todas sus oficinas
    When el coordinador solicita POST /agendas/generar con
         inversorId, candidatosIds, fecha=2026-07-02, duracionMinutos=60, metaReuniones=3
    Then el sistema retorna HTTP 201 con una agenda que contiene exactamente 3 reuniones
    And cada reunión especifica: horaInicio, horaFin, participante, cargo, oficina, idioma de la reunión
    And ninguna reunión inicia antes de las 08:00 ni finaliza después de las 17:00 (RN-09, RN-10)
    And ninguna reunión solapa el bloque 12:00–13:00 (RN-11)
    And el intervalo entre reuniones consecutivas es ≥ al tiempo de traslado entre sus oficinas (RN-13)
    And el mismo participante no aparece en dos reuniones solapadas (RN-14)

  Scenario: Generación de agenda parcial cuando la meta no es alcanzable
    Given solo existen 2 participantes candidatos con disponibilidad en la fecha y compatibles en idioma
    And la meta solicitada es 4 reuniones
    When se solicita la generación de la agenda
    Then el sistema retorna HTTP 201 con una agenda de 2 reuniones
    And la respuesta indica explícitamente que se lograron 2 de 4 reuniones solicitadas

  Scenario: Visualización dinámica de datos del inversor al seleccionarlo
    Given el coordinador abre el formulario de generación de agenda en el frontend
    When selecciona un inversor del catálogo
    Then el frontend muestra automáticamente: empresa, idiomas del inversor y ventana de visita disponible
    And el campo de fecha se restringe al rango de visita del inversor seleccionado
```

### AC-05 — Validaciones críticas del motor de scheduling

```gherkin
Feature: Validaciones de restricciones de negocio en el scheduling

  Scenario: Rechazo por ausencia de idioma compartido (RN-12)
    Given ningún participante candidato seleccionado comparte idioma con el inversor
    When se solicita POST /agendas/generar
    Then el sistema retorna HTTP 422 con el mensaje
         "No existen participantes que compartan idioma con el inversor. Verifique la configuración de idiomas."
    And no se persiste ninguna agenda

  Scenario: Rechazo por fecha fuera del rango de visita (RN-08)
    Given el inversor tiene visita del 2026-07-01 al 2026-07-05
    When se solicita una agenda para el 2026-07-10
    Then el sistema retorna HTTP 422 con el mensaje
         "La fecha 2026-07-10 está fuera del período de visita del inversor (2026-07-01 – 2026-07-05)"

  Scenario: Rechazo por falta de disponibilidad en la fecha
    Given todos los participantes candidatos compatibles en idioma no tienen bloques horarios para la fecha
    When se solicita la agenda
    Then el sistema retorna HTTP 422 con el mensaje
         "Ningún participante compatible tiene disponibilidad registrada para la fecha solicitada"

  Scenario: Traslados imposibles entre oficinas consecutivas
    Given los únicos dos participantes disponibles están en oficinas con 90 minutos de traslado entre sí
    And la duración de reunión es 60 minutos dentro de una jornada de 8 horas
    When se solicita la agenda
    Then el sistema retorna el aviso de que no pudo encadenar reuniones consecutivas por tiempo de traslado
```

### AC-06 — Generación de PDF del itinerario

```gherkin
Feature: Generación de documento PDF formal

  Scenario: Generación exitosa del PDF de una agenda activa
    Given existe una agenda en estado "Activa" con Id conocido
    When el coordinador invoca GET /agendas/{id}/pdf
    Then el Microservicio de Agendas delega la generación al Microservicio de PDF
    And el PDF resultante contiene:
        un encabezado institucional con logo y nombre completo del inversor,
        la fecha de la jornada,
        una tabla con cada reunión (hora inicio, hora fin, nombre del participante, cargo,
        nombre de la oficina, dirección física e idioma en que se realizará la reunión),
        el tiempo estimado de traslado entre cada par de reuniones consecutivas,
        un pie de página con numeración de página y fecha de generación del documento
    And el documento está redactado en español, variante de Costa Rica
    And la respuesta HTTP incluye Content-Disposition con nombre sugerido "Agenda_AAAA-MM-DD_NombreInversor.pdf"
```

### AC-07 — Anulación lógica de agenda

```gherkin
Feature: Anulación de agendas

  Scenario: Anulación exitosa de agenda activa
    Given existe una agenda en estado "Activa" con Id conocido
    When el coordinador invoca DELETE /agendas/{id}
    Then el sistema retorna HTTP 200
    And la agenda cambia a estado "Anulada" en la base de datos (RN-15)
    And la FechaAnulacion queda registrada con el timestamp exacto
    And el registro no se elimina físicamente de la base de datos
    And el PDF generado originalmente sigue siendo descargable vía GET /agendas/{id}/pdf
```

### AC-08 — Consulta y detalle de agendas

```gherkin
Feature: Consulta de agendas existentes

  Scenario: Listado de agendas con filtros opcionales
    Given existen múltiples agendas con distintos estados, inversores y fechas
    When el coordinador invoca GET /agendas con filtros opcionales (inversorId, fecha, estado)
    Then el sistema retorna la lista de agendas que cumplen los filtros
    And cada elemento incluye: id, nombre del inversor, fecha, estado y cantidad de reuniones

  Scenario: Detalle completo de una agenda
    Given existe una agenda con reuniones persistidas
    When el coordinador invoca GET /agendas/{id}
    Then el sistema retorna el JSON completo con todas las reuniones,
         sus horarios, participantes, oficinas, idiomas y tiempos de traslado
```

### AC-09 — Resiliencia en comunicación entre microservicios

```gherkin
Feature: Comportamiento ante fallos en llamadas entre microservicios

  Scenario: Reintentos con backoff ante indisponibilidad del Catálogo Service
    Given el Microservicio de Catálogo no responde (timeout o error 5xx)
    When el Microservicio de Agendas intenta obtener datos de participantes u oficinas
    Then reintenta hasta 3 veces con backoff exponencial (política Polly)
    And si el fallo persiste luego de los reintentos, retorna HTTP 503 con mensaje descriptivo
    And registra el incidente en los logs del microservicio con timestamp y tipo de fallo
```

---

## 4. Requisitos no-funcionales

### Performance

| Requisito | Umbral | Justificación |
|---|---|---|
| Latencia de generación de agenda (p95) | ≤ 5 segundos | El algoritmo de scheduling puede iterar sobre múltiples combinaciones; se prioriza respuesta ágil para el coordinador |
| Latencia de generación de PDF (p95) | ≤ 10 segundos | El renderizado de un PDF con tabla de reuniones y estilos institucionales puede requerir procesamiento adicional |
| Latencia de endpoints CRUD (p95) | ≤ 2 segundos | Las operaciones de lectura/escritura sobre datos maestros deben ser inmediatas |
| Concurrencia mínima soportada | 10 solicitudes concurrentes | Dimensionado para el equipo coordinador de PROCOMER en Fase 1 |

### Disponibilidad y operación

| Requisito | Umbral | Notas |
|---|---|---|
| Disponibilidad esperada | Horario laboral (sin SLA formal en Fase 1) | El sistema es un asistente de coordinación, no infraestructura crítica de negocio en tiempo real |
| Comportamiento ante fallo de microservicio dependiente | 3 reintentos con backoff exponencial (Polly/Microsoft.Extensions.Http.Resilience); HTTP 503 si persiste | El frontend muestra mensaje descriptivo al coordinador; no se entrega respuesta parcial ni inventada |
| Comportamiento ante timeout de generación de PDF | Timeout de 30 segundos; HTTP 504 con mensaje de reintento | El coordinador puede solicitar el PDF nuevamente sin consecuencias |
| Gestión de contenedores | Azure Container Apps gestiona reinicios automáticos; configurar liveness y readiness probes en cada microservicio | Los probes deben responder en el endpoint GET /health de cada servicio |

### Seguridad y cumplimiento

| Requisito | Detalle |
|---|---|
| Autenticación / Autorización | Fuera del alcance de la Fase 1; no se implementa en la prueba técnica |
| Datos sensibles | No se procesan datos regulados ni datos bajo NDA de clientes externos. Los participantes son funcionarios públicos o representantes institucionales. |
| Gestión de credenciales | La cadena de conexión a Azure SQL Database se gestiona mediante variables de entorno o Azure Container Apps Secrets. Ninguna credencial en código fuente ni en el repositorio Git. |
| CORS | Cada microservicio backend debe habilitar CORS con política explícita que acepte las URLs del frontend desplegado en Container Apps |
| Validación de entrada | Todos los endpoints validan los datos de entrada con Data Annotations o FluentValidation y retornan HTTP 400 con descripción del campo inválido |
| Stack traces en producción | Nunca exponer en respuestas HTTP; el middleware global de excepciones retorna únicamente el mensaje de error estructurado |

### Infraestructura y despliegue

| Requisito | Detalle |
|---|---|
| Plataforma | Microsoft Azure — Resource Group provisto por PROCOMER para la prueba |
| Contenedorización | Dockerfile con multi-stage build por cada microservicio y el frontend; imágenes publicadas en Azure Container Registry |
| Servicio de cómputo | Azure Container Apps — una Container App por componente (Catálogo Service, Agendas Service, PDF Service, Frontend) |
| Base de datos | Azure SQL Database — ver DP-02 para estrategia de una vs. múltiples bases |
| Runtime | .NET 8 |
| Estructura del repositorio | `src/Catalogo/`, `src/Agendas/`, `src/PDF/`, `src/Frontend/`, `tests/Agendas.UnitTests/`, `scripts/database/` |
| Documentación de APIs | Swagger/OpenAPI 3.0 habilitado en los tres microservicios backend; endpoint `/swagger` accesible públicamente para el panel evaluador |
| Separación de ambientes | Un único ambiente de despliegue para la prueba técnica (producción directa) |
| Sin API Gateway | El frontend ASP.NET MVC consume los microservicios directamente a través de sus URLs públicas en Azure Container Apps; no se implementa YARP ni ningún otro reverse proxy |

### Estándar de codificación

| Requisito | Detalle |
|---|---|
| **Documentación de código** | **Todo el código generado debe estar completamente documentado.** Clases, interfaces, propiedades, métodos, parámetros, valores de retorno y cualquier bloque de lógica relevante deben incluir documentación XML (`///`) en C# y comentarios en línea donde aplique. Este requisito es no negociable y aplica a todos los proyectos de la solución. Ver §10 para el estándar completo. |
| Nomenclatura | PascalCase: clases, interfaces, métodos, propiedades. camelCase: variables locales y parámetros. Prefijo `I` en interfaces. Sufijos de capa: `Controller`, `Service`, `Repository`, `Handler`, `Configuration`. |
| Manejo de errores | Middleware global de excepciones en cada microservicio. Excepciones de dominio tipadas (ej. `InversorConAgendasActivasException`, `IdiomaIncompatibleException`, `FechaFueraDeRangoException`). Stack traces nunca en respuestas de producción. |
| Organización de Clean Architecture | Por cada microservicio backend: cuatro proyectos — `.Domain`, `.Application`, `.Infrastructure`, `.API`. El proyecto de frontend es `.Web` (MVC). Las dependencias fluyen hacia adentro (Dominio ← Aplicación ← Infraestructura ← API). |

---

## 5. Datos involucrados

### Inventario de entidades y flujos

| Entidad / Tipo de dato | Flujo (origen → destino) | Notas |
|---|---|---|
| Datos de inversores | Coordinador → Frontend → Catálogo Service → Azure SQL | Nombre, empresa, país, idiomas, fechas, hospedaje |
| Datos de participantes | Coordinador → Frontend → Catálogo Service → Azure SQL | Nombre, cargo, oficina, idiomas, bloques de disponibilidad, estado |
| Datos de oficinas | Coordinador → Frontend → Catálogo Service → Azure SQL | Nombre, dirección, coordenadas opcionales |
| Matriz de traslados | Coordinador → Frontend → Catálogo Service → Azure SQL | Pares origen-destino con tiempo en minutos; almacenados con simetría garantizada |
| Solicitud de agenda | Coordinador → Frontend → Agendas Service | inversorId, candidatosIds, fecha, duracionMinutos, metaReuniones |
| Agenda generada (JSON) | Agendas Service → Azure SQL + respuesta HTTP | Id, InversorId, Fecha, Estado, colección de Reuniones |
| Datos de reunión (elemento de agenda) | Agendas Service → Azure SQL | Id, AgendaId, ParticipanteId, HoraInicio, HoraFin, OficinaId, IdiomaReunion, Orden, TiempoTrasladoSiguiente |
| PDF del itinerario | Agendas Service → PDF Service → respuesta binaria → Coordinador | Binario PDF generado en español CR; nunca almacenado en disco del microservicio |
| Logs de operación | Cada microservicio → Azure Container Apps Log Stream | Errores, tiempos de respuesta, intentos de resiliencia |

### Modelo de dominio — entidades y microservicio owner

| Entidad | Microservicio owner | Tabla en BD | Descripción |
|---|---|---|---|
| `Inversor` | Catálogo Service | `Inversores` | Visitante extranjero con datos personales, idiomas y ventana de visita |
| `Idioma` | Catálogo Service | `Idiomas` | Catálogo de idiomas soportados (español, inglés, etc.) |
| `InversorIdioma` | Catálogo Service | `InversoresIdiomas` | Tabla de unión N:M entre Inversor e Idioma |
| `Participante` | Catálogo Service | `Participantes` | Funcionario o aliado convocable a reuniones |
| `ParticipanteIdioma` | Catálogo Service | `ParticipantesIdiomas` | Tabla de unión N:M entre Participante e Idioma |
| `DisponibilidadParticipante` | Catálogo Service | `DisponibilidadParticipantes` | Bloques horarios disponibles por participante por fecha |
| `Oficina` | Catálogo Service | `Oficinas` | Ubicación física donde se realizan las reuniones |
| `MatrizTraslado` | Catálogo Service | `MatrizTraslados` | Tiempo de desplazamiento en minutos entre dos oficinas (par simétrico) |
| `Agenda` | Agendas Service | `Agendas` | Itinerario diario para un inversor; estado Activa / Anulada |
| `Reunion` | Agendas Service | `Reuniones` | Elemento de agenda: hora, participante, oficina, idioma, orden y traslado al siguiente |

### Reglas de negocio formales

| ID | Regla | Módulo aplicable | Criterio de aceptación |
|---|---|---|---|
| RN-01 | El inversor debe tener al menos un idioma asignado | Catálogo — Inversores | AC-01 |
| RN-02 | La FechaFinVisita debe ser ≥ a FechaInicioVisita | Catálogo — Inversores | AC-01 |
| RN-03 | No se permite eliminar un inversor con agendas en estado "Activa" | Catálogo — Inversores | AC-01 |
| RN-04 | Todo participante debe tener al menos un idioma asignado | Catálogo — Participantes | AC-02 |
| RN-05 | Todo participante debe tener exactamente una oficina asignada | Catálogo — Participantes | AC-02 |
| RN-06 | No se puede eliminar una oficina con participantes activos asignados | Catálogo — Oficinas | AC-03 |
| RN-07 | La MatrizTraslado debe ser simétrica: TiempoMinutos(A→B) = TiempoMinutos(B→A) | Catálogo — Traslados | AC-03 |
| RN-08 | La fecha de la agenda debe estar dentro del rango [FechaInicioVisita, FechaFinVisita] del inversor | Agendas — Scheduling | AC-04, AC-05 |
| RN-09 | Ninguna reunión puede iniciar antes de las 08:00 horas | Agendas — Scheduling | AC-04 |
| RN-10 | Ninguna reunión puede finalizar después de las 17:00 horas | Agendas — Scheduling | AC-04 |
| RN-11 | El bloque 12:00–13:00 está reservado para almuerzo; ninguna reunión puede solaparse con él | Agendas — Scheduling | AC-04 |
| RN-12 | El inversor y el participante deben compartir al menos un idioma para que la reunión sea válida | Agendas — Scheduling | AC-04, AC-05 |
| RN-13 | El intervalo entre el fin de una reunión y el inicio de la siguiente debe ser ≥ al TiempoMinutos entre las oficinas correspondientes | Agendas — Scheduling | AC-04 |
| RN-14 | Un participante no puede tener dos reuniones con horarios solapados en la misma agenda | Agendas — Scheduling | AC-04 |
| RN-15 | La anulación de agenda es lógica (soft delete); el registro y su PDF se conservan para trazabilidad histórica | Agendas — Anulación | AC-07 |

---

## 6. Riesgos y supuestos

### Supuestos

| # | Supuesto | Owner | Estado |
|---|---|---|---|
| S-01 | La solución se desarrolla en .NET 8 con C# siguiendo el stack definido en §7 | Equipo participante | ✅ Confirmado por lineamientos de la prueba |
| S-02 | El Resource Group de Azure está aprovisionado y las credenciales están disponibles al inicio de la prueba | PROCOMER | ✅ Previsto explícitamente por la prueba técnica |
| S-03 | La duración estándar de reunión se aplica uniformemente a todas las reuniones de una agenda generada; no hay reuniones de distinta duración en la misma agenda | Equipo participante | 🟡 Asumido — la prueba define "duración estándar" como parámetro único |
| S-04 | El PDF se genera únicamente en español de Costa Rica; no existe requerimiento de internacionalización del documento | Equipo participante | ✅ Confirmado explícitamente por la prueba técnica §4.4 |
| S-05 | La autenticación y autorización están fuera del alcance; cualquier usuario con acceso al frontend puede operar el sistema | Equipo participante | ✅ No requerida por la prueba técnica |
| S-06 | No hay API Gateway; el frontend ASP.NET MVC llama directamente las URLs públicas de cada Container App | Equipo participante | ✅ Confirmado por la prueba técnica §5.1 |
| S-07 | La biblioteca de generación de PDF seleccionada (ver DP-01) funciona sin dependencias de sistema (GDI+, fuentes del SO) en Linux containers | Equipo participante | 🔴 Sin confirmar — condicional a la resolución de DP-01 |
| S-08 | Se usa una única Azure SQL Database para todos los microservicios, con esquemas separados o prefijos de tabla por dominio | Equipo participante | 🟡 Asumido — ver DP-02 |

### Riesgos

| # | Riesgo | Probabilidad | Impacto | Mitigación propuesta |
|---|---|---|---|---|
| R-01 | El algoritmo de scheduling es computacionalmente complejo; una implementación ingenua (fuerza bruta o backtracking sin poda) puede exceder el umbral de 5 segundos con candidatos numerosos | Media | Alto | Implementar primero el algoritmo greedy (ver §8). Reservar backtracking solo si el greedy no cubre los casos de prueba obligatorios. Definir un límite máximo de 200 combinaciones evaluadas antes de retornar la mejor solución encontrada. |
| R-02 | El tiempo total de 6 horas es insuficiente para implementar y desplegar todos los módulos con calidad de producción | Alta | Alto | Priorizar en orden: (1) Catálogo Service + BD, (2) Motor de Scheduling, (3) API de Agendas, (4) Frontend, (5) PDF Service. Desplegar a Azure Container Apps en paralelo desde la primera hora, no al final. |
| R-03 | La simetría de la MatrizTraslado puede romperse si se permite editar cada dirección de forma independiente | Media | Medio | Implementar la garantía de simetría en la capa de Aplicación del Catálogo Service (no en la BD): al insertar o actualizar un par, el servicio persiste automáticamente el par inverso. |
| R-04 | La biblioteca de generación de PDF puede fallar en Azure si requiere dependencias nativas no disponibles en el contenedor base Linux de Azure Container Apps | Media | Medio | Utilizar QuestPDF como primera opción (biblioteca 100% .NET, sin dependencias nativas); validar la generación de un PDF de prueba en el pipeline de CI antes de integrar con el sistema completo. |
| R-05 | El despliegue en Azure Container Apps puede demorar más de lo previsto si las imágenes Docker no están optimizadas o si la cuota del Container Registry no está disponible | Media | Medio | Preparar los Dockerfiles con multi-stage builds desde el inicio. Hacer push a ACR al inicio de la prueba con una imagen placeholder para verificar la cadena de despliegue antes de completar el desarrollo. |
| R-06 | La cadena de conexión a Azure SQL Database puede tener diferencias entre el entorno local y el entorno Azure (firewall, string format, usuario) | Media | Medio | Verificar la conexión desde una Container App de prueba al inicio. Usar variables de entorno para la cadena de conexión; nunca hardcodear. |

### Dependencias externas

| Dependencia | Tipo | Criticidad | Acción si falla |
|---|---|---|---|
| Azure Container Registry | Servicio Azure (provisto por PROCOMER) | Alta | Sin ACR no hay despliegue; verificar acceso y permisos al inicio de la prueba |
| Azure SQL Database | Servicio Azure (provisto por PROCOMER) | Alta | Sin base de datos no hay persistencia; verificar cadena de conexión al inicio |
| Azure Container Apps | Servicio Azure (provisto por PROCOMER) | Alta | Verificar cuotas del Resource Group antes del primer despliegue |
| QuestPDF / biblioteca PDF seleccionada | Paquete NuGet (open source) | Media | Si QuestPDF falla en Linux container, evaluar iTextSharp 5 (LGPL) como alternativa |
| Microsoft.Extensions.Http.Resilience | Paquete NuGet (.NET 8 nativo) | Media | Incluido en el SDK de .NET 8; sin dependencia externa adicional |

---

## 7. Stack Tecnológico y Herramientas

### Stack tecnológico de la solución

| Capa | Tecnología | Versión | Justificación |
|---|---|---|---|
| Runtime | .NET 8 (C#) | 8.0 LTS | Plataforma estándar del ecosistema .NET; LTS activo; soporte nativo en Azure Container Apps |
| Framework web — microservicios | ASP.NET Core 8 Web API | 8.0 | REST API minimalista con middleware, DI nativo, Swagger y Health Checks integrados |
| Framework web — frontend | ASP.NET Core 8 MVC + JavaScript / AJAX / jQuery | 8.0 / jQuery 3.x | Requerido explícitamente por la prueba técnica §5.1 |
| ORM | Entity Framework Core 8 | 8.x | Clean Architecture con Code First Migrations; soporte nativo para Azure SQL; LINQ integrado |
| Documentación de API | Swashbuckle.AspNetCore (Swagger/OpenAPI 3.0) | Latest | Requerido por la prueba técnica §5.2; endpoint accesible para el panel evaluador |
| Resiliencia HTTP | Microsoft.Extensions.Http.Resilience (Polly integrado) | .NET 8 nativo | Reintentos, circuit breaker y timeout para llamadas entre microservicios (AC-09) |
| HTTP Client tipado | IHttpClientFactory con HttpClient tipado | .NET 8 nativo | Gestión del ciclo de vida de conexiones; desacoplamiento por interfaz |
| Generación de PDF | QuestPDF | Latest (ver DP-01) | Biblioteca 100% .NET sin dependencias nativas; funciona en Linux containers en Azure |
| Pruebas unitarias | xUnit + Moq + FluentAssertions | Latest | Stack estándar de testing en .NET; compatible con Azure DevOps y GitHub Actions |
| Contenedorización | Docker (multi-stage build) | Latest | Imágenes optimizadas para Azure Container Registry |
| Base de datos | Azure SQL Database | SQL Server 2022 compatible | Requerido explícitamente por la prueba técnica §5.3 |
| Registro de contenedores | Azure Container Registry | — | Requerido por la prueba técnica §5.3 |
| Servicio de cómputo | Azure Container Apps | — | Requerido por la prueba técnica §5.3 |

### Mapa de microservicios y puertos

| Servicio | Proyecto | Responsabilidad | Puerto (local) | Azure Container App |
|---|---|---|---|---|
| Catálogo Service | `src/Catalogo/` | Inversores, participantes, oficinas, disponibilidad, matriz de traslados | `:5001` | `catalogo-service` |
| Agendas Service | `src/Agendas/` | Motor de scheduling, gestión de agendas, anulación, consulta | `:5002` | `agendas-service` |
| PDF Service | `src/PDF/` | Generación de PDF del itinerario | `:5003` | `pdf-service` |
| Frontend | `src/Frontend/` | Interfaz ASP.NET MVC para el coordinador | `:5004` | `frontend-web` |

### Mapa de endpoints REST obligatorios

| Método | Ruta | Servicio | Descripción | Ref. prueba |
|---|---|---|---|---|
| `GET` | `/api/inversores` | Catálogo | Listar inversores con filtros opcionales | §4.1 |
| `POST` | `/api/inversores` | Catálogo | Registrar inversor | §4.1 |
| `GET` | `/api/inversores/{id}` | Catálogo | Detalle de inversor | §4.1 |
| `PUT` | `/api/inversores/{id}` | Catálogo | Actualizar inversor | §4.1 |
| `DELETE` | `/api/inversores/{id}` | Catálogo | Eliminar inversor (con validación RN-03) | §4.1 |
| `GET` | `/api/participantes` | Catálogo | Listar participantes activos | §4.2 |
| `POST` | `/api/participantes` | Catálogo | Registrar participante | §4.2 |
| `PUT` | `/api/participantes/{id}` | Catálogo | Actualizar participante o cambiar estado | §4.2 |
| `DELETE` | `/api/participantes/{id}` | Catálogo | Eliminar participante | §4.2 |
| `GET` | `/api/oficinas` | Catálogo | Listar oficinas | §4.2 |
| `POST` | `/api/oficinas` | Catálogo | Registrar oficina | §4.2 |
| `DELETE` | `/api/oficinas/{id}` | Catálogo | Eliminar oficina (con validación RN-06) | §4.2 |
| `GET` | `/api/traslados` | Catálogo | Consultar matriz de traslados | §4.2 |
| `POST` | `/api/traslados` | Catálogo | Registrar par de traslado (crea par simétrico) | §4.2 |
| `POST` | `/agendas/generar` | Agendas | Generar agenda automática | §5.2 |
| `GET` | `/agendas` | Agendas | Listar agendas con filtros | §5.2 |
| `GET` | `/agendas/{id}` | Agendas | Detalle de agenda | §5.2 |
| `DELETE` | `/agendas/{id}` | Agendas | Anulación lógica de agenda | §5.2 |
| `GET` | `/agendas/{id}/pdf` | Agendas | Descargar PDF del itinerario | §5.2 |

### Herramientas de desarrollo autorizadas

| Herramienta | Uso en el proyecto | Restricción |
|---|---|---|
| GitHub Copilot / Claude / ChatGPT / Cursor | Asistencia en generación de código, plantillas, diagramas y documentación | El equipo es responsable de validar y comprender todo el código generado; el panel evaluador solicitará justificación técnica de cualquier decisión |
| Visual Studio 2022 / VS Code | Desarrollo, depuración y gestión de migraciones EF Core | Sin restricción |
| Azure CLI / Azure Portal | Aprovisionamiento de recursos y gestión de despliegues | Usar credenciales provistas por PROCOMER; no crear recursos fuera del Resource Group asignado |
| Docker Desktop | Construcción y prueba local de imágenes antes del push a ACR | Sin restricción |

---

## 8. Diseño del Motor de Scheduling

El motor de scheduling es el componente central del sistema y el de mayor complejidad técnica. Su diseño se documenta aquí para guiar la implementación y los casos de prueba unitaria.

### Componentes del motor

| Componente (clase/interfaz) | Capa | Responsabilidad | Restricción clave |
|---|---|---|---|
| `ISchedulingEngine` / `SchedulingEngine` | Application | Orquesta el algoritmo completo; recibe todos los datos necesarios como input ya consultado y retorna el `AgendaResult` | No consulta la base de datos directamente; toda la persistencia ocurre fuera de este componente |
| `ILanguageCompatibilityFilter` / `LanguageCompatibilityFilter` | Application | Filtra candidatos conservando solo los que comparten al menos un idioma con el inversor (RN-12) | No genera texto libre; solo aplica la regla de compatibilidad |
| `IAvailabilitySlotBuilder` / `AvailabilitySlotBuilder` | Application | Construye los bloques horarios válidos de un participante para la fecha solicitada, aplicando los límites 08:00–17:00 y excluyendo 12:00–13:00 (RN-09, RN-10, RN-11) | Recibe datos ya cargados; no accede a base de datos |
| `ITravelTimeResolver` / `TravelTimeResolver` | Application | Dado un par (OficinaOrigenId, OficinaDestinoId), retorna el tiempo de traslado en minutos desde la matriz | Solo lectura de la matriz previamente cargada en memoria para la sesión de scheduling |
| `IAgendaRepository` / `AgendaRepository` | Infrastructure | Persiste y consulta agendas y reuniones en Azure SQL Database vía EF Core 8 | Implementa la interfaz definida en Application; nunca referenciado desde Domain |

### Algoritmo de scheduling — greedy con optimización de traslados

El motor implementa un algoritmo greedy que construye la agenda de izquierda a derecha (de la primera hora disponible hacia el final del día), seleccionando en cada paso al siguiente participante que encaje sin violar ninguna restricción.

```
ENTRADA:
  inversor         → datos del inversor con su lista de idiomas y punto de hospedaje
  candidatos       → participantes ya filtrados por LanguageCompatibilityFilter,
                     con sus bloques de disponibilidad para la fecha (ya excluidos 12:00-13:00
                     y limitados a 08:00-17:00)
  duracionMinutos  → duración de cada reunión (igual para todas)
  metaReuniones    → cantidad deseada de reuniones
  matrizTraslados  → diccionario Dictionary<(Guid origen, Guid destino), int minutos>

SALIDA:
  AgendaResult con la lista ordenada de Reunion, o un SchedulingError descriptivo

PROCESO:
  1. Si candidatos está vacío → retornar SchedulingError(IdiomaIncompatible)

  2. Ordenar candidatos por hora de inicio de su primer bloque disponible (ASC).

  3. Inicializar:
       agenda          = lista vacía de Reunion
       horaActual      = 08:00
       ultimaOficinaId = punto de hospedaje del inversor (tratado como "oficina virtual" para el primer traslado)

  4. Para cada candidato en el orden del paso 2:
       Si agenda.Count == metaReuniones → break (meta alcanzada)

       Para cada bloque (bloqueInicio, bloqueFin) del candidato:
         a. Calcular tiempoTraslado = matrizTraslados[(ultimaOficinaId, candidato.OficinaId)]
                                      (0 si es el mismo lugar o si no existe el par → registrar advertencia)

         b. Calcular horaInicioReal = max(horaActual + tiempoTraslado, bloqueInicio)

         c. Calcular horaFinReal    = horaInicioReal + duracionMinutos

         d. Validar (todas deben pasar):
              - horaFinReal ≤ bloqueFin                     (cabe en el bloque del participante)
              - horaInicioReal ≥ 08:00                      (RN-09)
              - horaFinReal ≤ 17:00                         (RN-10)
              - horaInicioReal ≥ 13:00 OR horaFinReal ≤ 12:00   (RN-11: no solapar almuerzo)
              - candidato no tiene reunión activa en [horaInicioReal, horaFinReal] (RN-14)

         e. Si todas las validaciones pasan:
              - idiomaReunion = primer idioma compartido(inversor.Idiomas, candidato.Idiomas)
              - Agregar Reunion(candidato, horaInicioReal, horaFinReal, candidato.Oficina, idiomaReunion)
                a la agenda
              - Actualizar horaActual = horaFinReal
              - Actualizar ultimaOficinaId = candidato.OficinaId
              - break (pasar al siguiente candidato)

  5. Si agenda.Count == 0 → retornar SchedulingError(SinDisponibilidad)

  6. Retornar AgendaResult(reuniones=agenda, reunionesLogradas=agenda.Count,
                           metaSolicitada=metaReuniones, completa=(agenda.Count==metaReuniones))
```

### Mensajes de error del motor

| Causa | Código de error | Mensaje al coordinador |
|---|---|---|
| Ningún candidato comparte idioma con el inversor (RN-12) | `IDIOMA_INCOMPATIBLE` | "No existen participantes que compartan idioma con el inversor. Verifique la configuración de idiomas de los candidatos." |
| Todos los candidatos compatibles sin disponibilidad para la fecha | `SIN_DISPONIBILIDAD` | "Ningún participante compatible tiene disponibilidad registrada para la fecha solicitada." |
| Fecha fuera del rango de visita (RN-08) | `FECHA_FUERA_DE_RANGO` | "La fecha {fecha} está fuera del período de visita del inversor ({FechaInicio} – {FechaFin})." |
| Tiempos de traslado impiden toda combinación | `TRASLADOS_INVIABLES` | "Los tiempos de traslado entre las oficinas disponibles no permiten encadenar ninguna reunión para esa fecha. Considere reducir la duración de reunión o seleccionar participantes en oficinas más cercanas." |

### Casos de prueba unitaria requeridos (mínimo 5)

| # | Tipo | Descripción | Clase de prueba sugerida |
|---|---|---|---|
| UT-01 | ✅ Positivo | Cálculo correcto del tiempo de traslado entre dos oficinas usando la matriz precargada | `TravelTimeResolverTests` |
| UT-02 | ✅ Positivo | El motor genera una agenda con 3 reuniones dentro de la ventana de visita del inversor sin violar ninguna restricción | `SchedulingEngineTests` |
| UT-03 | ✅ Positivo | La anulación lógica de agenda cambia el estado a "Anulada" sin eliminar el registro ni su PDF | `AgendaServiceTests` |
| UT-04 | ❌ Negativo | El motor rechaza la generación cuando ningún candidato comparte idioma con el inversor (RN-12) | `SchedulingEngineTests` |
| UT-05 | ❌ Negativo | El motor rechaza una solicitud con fecha fuera del rango de visita del inversor (RN-08) | `SchedulingEngineTests` |

---

## 9. Observabilidad, Outputs Esperados y Decisiones Pendientes

### Outputs esperados de la solución

| Output | Formato | Destino | Frecuencia | Propietario |
|---|---|---|---|---|
| Agenda generada | JSON estructurado con lista de `Reunion` | Respuesta HTTP 201 del Agendas Service | Por solicitud de generación | Coordinador |
| PDF del itinerario | Binario PDF (`application/pdf`) | Respuesta HTTP del endpoint GET /agendas/{id}/pdf | Por solicitud de descarga | Coordinador / Inversor |
| Errores de validación | JSON `{ "error": { "code": "...", "message": "..." } }` | Respuesta HTTP 4xx de cualquier microservicio | Ante validación fallida | Coordinador |
| Swagger UI | Página HTML interactiva | Endpoint `/swagger` de cada microservicio backend | Siempre disponible | Panel evaluador |
| Health Check | JSON `{ "status": "Healthy" }` | Endpoint GET `/health` de cada microservicio | Siempre disponible | Azure Container Apps (liveness probe) |
| Logs de operación | Texto estructurado con nivel de log | Azure Container Apps — Log Stream | Continuo | Equipo participante / Evaluador |

### Decisiones pendientes

| # | Decisión pendiente | Owner | Fecha límite | Impacto si no se resuelve |
|---|---|---|---|---|
| DP-01 | **Biblioteca de generación de PDF**: QuestPDF (recomendado) vs. iTextSharp 5 (LGPL) vs. FastReport.Net (Community). Criterio principal: compatibilidad con Linux containers en Azure. | Equipo participante | Inicio de la implementación del PDF Service | Bloquea completamente el desarrollo del Microservicio de PDF |
| DP-02 | **Estrategia de base de datos**: una Azure SQL Database compartida (con prefijos de tabla por servicio) vs. una base de datos por microservicio. La primera es más sencilla de aprovisionar; la segunda refuerza el desacoplamiento pero requiere configurar múltiples connection strings. | Equipo participante | Antes de iniciar las migraciones EF Core | Define la estrategia de EF Core Migrations y los connection strings de cada Container App |
| DP-03 | **Logo institucional para el PDF**: confirmar si PROCOMER provee el logo en formato PNG/SVG para el encabezado institucional, o si se usa un placeholder de prueba | Equipo participante + PROCOMER | Antes de implementar el PDF Service | Sin logo el documento puede no cumplir el estándar institucional esperado por el evaluador |
| DP-04 | **Plan de Azure Container Apps**: verificar que el Resource Group provisto por PROCOMER soporta el plan Consumption (recomendado para la prueba técnica) y que la cuota de Container Apps no está agotada | Equipo participante | Antes del primer despliegue | Un plan no disponible o cuota agotada bloqueará todo el despliegue en Azure |

---

## 10. Estándar de Documentación de Código

Este estándar es de cumplimiento **obligatorio y no negociable** para toda la solución. El panel evaluador verificará la documentación durante la revisión técnica del código fuente.

### Regla fundamental

> **Todo el código generado en esta solución debe estar completamente documentado.** Esta regla aplica sin excepción a todos los proyectos: Catálogo Service, Agendas Service, PDF Service, Frontend y el proyecto de pruebas unitarias. Un método sin documentación es un incumplimiento de este SPEC.

### Requisitos por tipo de elemento en C#

| Elemento | Herramienta | Contenido mínimo requerido |
|---|---|---|
| Clase | XML Doc (`/// <summary>`) | Descripción de la responsabilidad de la clase dentro de su capa y módulo |
| Interface | XML Doc (`/// <summary>`) | Descripción del contrato que define y el componente que lo implementa |
| Método / Función | XML Doc completo | `<summary>` con comportamiento; `<param>` por cada parámetro; `<returns>` con el tipo y significado del valor retornado; `<exception>` con cada excepción tipada que puede lanzar y la condición que la activa |
| Propiedad | XML Doc (`/// <summary>`) | Descripción del dato que representa; restricciones o valores permitidos si aplica |
| Constante o valor literal | XML Doc o comentario en línea (`//`) | Significado del valor y la regla de negocio que lo origina (ej. `// RN-09: hora de inicio laboral`) |
| Bloque de lógica compleja | Comentario en línea (`//`) | Explicación del algoritmo o decisión de diseño no evidente en el código |
| Referencia a regla de negocio | Comentario en línea | Referenciar el ID de la regla de este SPEC (ej. `// RN-11: el bloque 12:00-13:00 es inviolable`) |

### Ejemplo de documentación esperada

```csharp
/// <summary>
/// Motor principal de generación de agendas de visita para inversores.
/// Implementa el algoritmo greedy descrito en el SPEC §8 para maximizar
/// la cantidad de reuniones programadas dentro de las restricciones operativas
/// de horario (RN-09, RN-10), almuerzo (RN-11), idioma (RN-12) y traslados (RN-13).
/// </summary>
public class SchedulingEngine : ISchedulingEngine
{
    /// <summary>Filtro de compatibilidad de idioma inyectado por DI.</summary>
    private readonly ILanguageCompatibilityFilter _languageFilter;

    /// <summary>Resolutor de tiempos de traslado entre oficinas.</summary>
    private readonly ITravelTimeResolver _travelResolver;

    /// <summary>
    /// Inicializa una nueva instancia del motor con sus dependencias requeridas.
    /// </summary>
    /// <param name="languageFilter">
    ///   Filtro que conserva únicamente los candidatos con idioma compartido con el inversor.
    /// </param>
    /// <param name="travelResolver">
    ///   Resolutor que consulta la MatrizTraslado para obtener el tiempo de desplazamiento
    ///   entre dos oficinas.
    /// </param>
    public SchedulingEngine(
        ILanguageCompatibilityFilter languageFilter,
        ITravelTimeResolver travelResolver)
    {
        _languageFilter = languageFilter;
        _travelResolver = travelResolver;
    }

    /// <summary>
    /// Genera la secuencia óptima de reuniones para un inversor en una fecha determinada
    /// aplicando el algoritmo greedy documentado en el SPEC §8.
    /// </summary>
    /// <param name="request">
    ///   Parámetros de entrada: inversor con idiomas, candidatos con disponibilidad,
    ///   fecha de la agenda, duración de reunión en minutos y meta de cantidad de reuniones.
    /// </param>
    /// <returns>
    ///   Un <see cref="AgendaResult"/> con la lista ordenada de <see cref="Reunion"/> generadas,
    ///   la cantidad lograda y si se alcanzó la meta solicitada.
    /// </returns>
    /// <exception cref="FechaFueraDeRangoException">
    ///   Se lanza cuando la fecha solicitada está fuera del período de visita del inversor (RN-08).
    /// </exception>
    /// <exception cref="IdiomaIncompatibleException">
    ///   Se lanza cuando ningún candidato comparte idioma con el inversor (RN-12).
    /// </exception>
    public AgendaResult GenerarAgenda(AgendaRequest request)
    {
        // RN-08: Validar que la fecha esté dentro de la ventana de visita del inversor
        if (request.Fecha < request.Inversor.FechaInicioVisita
            || request.Fecha > request.Inversor.FechaFinVisita)
        {
            throw new FechaFueraDeRangoException(
                request.Fecha,
                request.Inversor.FechaInicioVisita,
                request.Inversor.FechaFinVisita);
        }

        // RN-12: Filtrar candidatos que no comparten idioma con el inversor
        var candidatosCompatibles = _languageFilter
            .Filtrar(request.Candidatos, request.Inversor.Idiomas);

        if (!candidatosCompatibles.Any())
        {
            throw new IdiomaIncompatibleException(request.Inversor.Id);
        }

        // Aplicar el algoritmo greedy: ordenar por primer bloque disponible y construir secuencia
        return ConstruirAgendaGreedy(candidatosCompatibles, request);
    }

    /// <summary>
    /// Construye la secuencia de reuniones usando el algoritmo greedy.
    /// Itera sobre candidatos ordenados por disponibilidad temprana y asigna
    /// cada reunión al primer bloque horario válido encontrado.
    /// </summary>
    /// <param name="candidatos">
    ///   Lista de participantes ya filtrados por compatibilidad de idioma.
    /// </param>
    /// <param name="request">Parámetros de la solicitud de agenda.</param>
    /// <returns>El resultado con la agenda construida.</returns>
    private AgendaResult ConstruirAgendaGreedy(
        IEnumerable<CandidatoAgenda> candidatos,
        AgendaRequest request)
    {
        var agenda = new List<Reunion>();
        // RN-09: La jornada laboral inicia a las 08:00
        var horaActual = new TimeSpan(8, 0, 0);
        var ultimaOficinaId = request.Inversor.PuntoPartidaId;

        foreach (var candidato in candidatos.OrderBy(c => c.PrimerBloqueInicio))
        {
            if (agenda.Count == request.MetaReuniones)
                break; // Meta alcanzada, no continuar iterando

            // Intentar encajar una reunión con este candidato en alguno de sus bloques
            var reunion = IntentarAgendarReunion(
                candidato, horaActual, ultimaOficinaId, request.DuracionMinutos);

            if (reunion is not null)
            {
                agenda.Add(reunion);
                horaActual = reunion.HoraFin;
                ultimaOficinaId = candidato.OficinaId;
            }
        }

        return new AgendaResult(
            Reuniones: agenda,
            ReunionesLogradas: agenda.Count,
            MetaSolicitada: request.MetaReuniones,
            Completa: agenda.Count == request.MetaReuniones);
    }
}
```

---

## 11. Criterios de Éxito

Los criterios de éxito derivan directamente de la tabla de evaluación de la prueba técnica (ref. Prueba_Técnica.md §6). El panel evaluador los usará como referencia de calificación al cierre de la presentación.

### Indicadores de éxito

| # | Indicador | Puntaje | Umbral mínimo | Umbral objetivo | Fuente de verificación |
|---|---|---|---|---|---|
| E-01 | **Análisis del requerimiento** | 10 pts | Diagrama de arquitectura con los 3 microservicios, frontend, BD y servicios Azure + documentación básica de casos de uso | Diagrama completo + casos de uso desarrollados + este SPEC aprobado | Artefactos entregados (SPEC, diagramas, casos de uso) |
| E-02 | **Planificación y prototipo** | 10 pts | Mockups o wireframes básicos de las 4 pantallas principales | Prototipo navegable con todos los flujos del sistema | Archivos de prototipo entregados |
| E-03 | **Capa de presentación** | 10 pts | Frontend ASP.NET MVC con todos los campos y validaciones del requerimiento sin tecnologías no autorizadas | Frontend funcional con JavaScript / AJAX / jQuery, visualización dinámica del inversor al seleccionarlo, todos los estados de UI (carga, error, éxito) | URL pública del frontend desplegado en Azure |
| E-04 | **Microservicios** | 50 pts | Clean Architecture en cada microservicio, algoritmo de scheduling correcto, APIs REST expuestos, acceso a Azure SQL con EF Core | Los 3 microservicios en Container Apps con todos los endpoints según §5.2 de la prueba, Swagger accesible, resilencia Polly implementada | URLs públicas + Swagger de cada API |
| E-05 | **Buenas prácticas** | 5 pts | Al menos 5 pruebas unitarias (positivos y negativos) | Suite de 5+ tests correctamente distribuidos + código completamente documentado con XML Doc según §10 | Repositorio Git + resultado de ejecución de tests |
| E-06 | **Gestión del tiempo** | 10 pts | Entrega entre 4 y 6 horas (5 pts) | Entrega antes de 4 horas (10 pts) | Timestamp de entrega formal registrado por PROCOMER |
| E-07 | **Cumplimiento general** | 5 pts | URL pública del frontend funcionando contra microservicios desplegados | Todos los requerimientos funcionales del §4 de la prueba técnica cubiertos sin omisiones | Verificación directa del panel evaluador |

### Condición de demostración exitosa

Al cierre de la prueba, el panel evaluador debe poder realizar las siguientes acciones sobre el sistema desplegado en Azure sin asistencia del equipo participante:

1. Registrar un inversor con idiomas y fechas de visita.
2. Registrar al menos dos participantes con disponibilidad horaria y oficinas distintas.
3. Registrar las oficinas y el tiempo de traslado entre ellas (verificar simetría automática).
4. Generar una agenda automática que cumpla todas las reglas de negocio declaradas.
5. Descargar el PDF del itinerario generado y verificar que contiene todos los campos requeridos.
6. Anular la agenda y confirmar que el registro persiste con estado "Anulada" y el PDF sigue disponible.
7. Verificar que Swagger está accesible en los tres microservicios backend.

---

## 12. Checklist de Aprobación Gate 1

### Chequeos del documento SPEC

| Check | Estado |
|---|---|
| `## Objetivo` describe el problema de negocio y el valor esperado | [x] ✅ Conforme |
| `## Alcance` diferencia claramente dentro del alcance, fuera de alcance y condiciones de frontera | [x] ✅ Conforme |
| `## Criterios de aceptación` en formato Gherkin con escenarios positivos y negativos por cada caso de uso principal | [x] ✅ Conforme |
| Reglas de negocio (RN-01 a RN-15) documentadas con ID único y referencia a criterio de aceptación | [x] ✅ Conforme |
| Todas las entidades del dominio identificadas con su microservicio owner y tabla en BD | [x] ✅ Conforme |
| Decisiones pendientes (DP-01 a DP-04) declaradas con owner y fecha límite | [x] ✅ Conforme |
| Stack tecnológico completo declarado con justificación | [x] ✅ Conforme |
| Mapa de microservicios con puertos y responsabilidades | [x] ✅ Conforme |
| Todos los endpoints REST requeridos por §5.2 de la prueba técnica documentados | [x] ✅ Conforme |
| Algoritmo de scheduling documentado a nivel de pseudocódigo con condiciones de borde | [x] ✅ Conforme |
| Componentes del motor de scheduling documentados (ILanguageCompatibilityFilter, ITravelTimeResolver, etc.) | [x] ✅ Conforme |
| 5 casos de prueba unitaria identificados con tipo y descripción | [x] ✅ Conforme |
| Estándar de documentación de código declarado con ejemplos en C# | [x] ✅ Conforme |
| Criterios de éxito con puntajes del evaluador y condición de demostración | [x] ✅ Conforme |
| Riesgos y supuestos documentados con mitigaciones concretas | [x] ✅ Conforme |

### Chequeos de arquitectura — por confirmar en implementación

| Check | Estado |
|---|---|
| La solución tiene exactamente 3 microservicios backend + 1 frontend (no monolítica) | [ ] ⬜ Por confirmar en implementación |
| Cada microservicio backend implementa Clean Architecture con 4 proyectos separados (`.Domain`, `.Application`, `.Infrastructure`, `.API`) | [ ] ⬜ Por confirmar en implementación |
| El Microservicio de Agendas usa IHttpClientFactory con políticas de resiliencia para llamar al Catálogo Service | [ ] ⬜ Por confirmar en implementación |
| La simetría de la MatrizTraslado se garantiza en la capa de Aplicación del Catálogo Service | [ ] ⬜ Por confirmar en implementación |
| El PDF se genera en español de Costa Rica | [ ] ⬜ Por confirmar en implementación |
| Swagger está habilitado y accesible en los 3 microservicios backend | [ ] ⬜ Por confirmar en despliegue |
| Ninguna credencial está hardcodeada en el código fuente ni en el repositorio Git | [ ] ⬜ Por confirmar en revisión de código |
| Todo el código cumple el estándar de documentación XML Doc del §10 | [ ] ⬜ Por confirmar en revisión de código |

### Resultado del gate

| Campo | Valor |
|---|---|
| **Decisión** | 🟡 En progreso — SPEC aprobado como artefacto de análisis; implementación en curso |
| **Bloqueantes activos** | DP-01 (biblioteca PDF) y DP-02 (estrategia de BD) deben resolverse antes de iniciar el desarrollo de sus respectivos módulos. DP-04 (cuota Azure Container Apps) debe verificarse antes del primer despliegue. |
| **Notas** | El algoritmo de scheduling debe validarse con los 5 casos de prueba unitaria (UT-01 a UT-05) antes del despliegue final. El estándar de documentación de código del §10 es requisito de entrega, no opcional. |
| **Contratación** | 2026XE-000001-0001700001 |
| **Fecha** | Junio 2026 |

---

*Este SPEC es el artefacto de análisis y diseño del Sistema de Calendarización de Inversores para PROCOMER. Define la arquitectura, las reglas de negocio (RN-01 a RN-15), los criterios de aceptación en Gherkin, el diseño del motor de scheduling, los estándares de implementación y los criterios de éxito que rigen la construcción de la solución. Cualquier decisión de implementación que se desvíe de lo documentado aquí debe ser justificada técnicamente al panel evaluador durante la presentación final. Documento generado como artefacto del Gate 1 — PROCOMER-CALEND-2026. Versión 1.0 · Junio 2026.*
