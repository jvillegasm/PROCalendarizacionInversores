# Casos de Uso — Sistema de Calendarización de Inversores
 
| | |
|---|---|
| **Proyecto** | Sistema de Calendarización de Inversores |
| **ID del proyecto** | PROCOMER-CALEND-2026 |
| **Versión** | 1.0 · Junio 2026 |
| **Fecha** | Junio 2026 |
| **Stack** | .NET 8 · ASP.NET Core 8 Web API · ASP.NET Core 8 MVC · Entity Framework Core 8 · Azure SQL Database · Azure Container Apps · QuestPDF · xUnit + Moq + FluentAssertions |
| **Documentos fuente** | `SPEC_Calendarizacion_Inversores.md` v1.0 · `Prueba_Técnica.md` — Contratación 2026XE-000001-0001700001 |
| **Estado** | 🟡 Borrador — Gate 1 en curso |
 
---
 
## Tabla de Contenidos
 
1. [CU-01 — Registrar Inversor](#cu-01--registrar-inversor)
2. [CU-02 — Gestionar Participantes](#cu-02--gestionar-participantes)
3. [CU-03 — Gestionar Oficinas y Matriz de Traslados](#cu-03--gestionar-oficinas-y-matriz-de-traslados)
4. [CU-04 — Generar Agenda Automática](#cu-04--generar-agenda-automática)
5. [CU-05 — Consultar y Anular Agenda](#cu-05--consultar-y-anular-agenda)
6. [CU-06 — Descargar PDF del Itinerario](#cu-06--descargar-pdf-del-itinerario)
7. [Tabla Resumen de Reglas de Negocio por Caso de Uso](#tabla-resumen-de-reglas-de-negocio-por-caso-de-uso)
---
 
## CU-01 — Registrar Inversor
 
### 1. Identificación
 
| Campo | Valor |
|---|---|
| **ID** | CU-01 |
| **Nombre** | Registrar Inversor |
| **Componente responsable** | Catálogo Service (`src/Catalogo/`) — capas Application e Infrastructure |
| **Endpoint(s) / Punto de entrada** | `POST /api/inversores` · `PUT /api/inversores/{id}` · `DELETE /api/inversores/{id}` · `GET /api/inversores` · `GET /api/inversores/{id}` |
| **Prioridad** | Alta |
 
### 2. Actores
 
| Actor | Rol |
|---|---|
| Coordinador de agendas | Usuario principal del sistema; funcionario de PROCOMER que registra, actualiza y elimina inversores a través del frontend ASP.NET MVC. |
| Frontend (ASP.NET Core MVC) | Capa de presentación que recibe los datos del coordinador y los envía al Catálogo Service vía HTTP REST. |
| Catálogo Service | Microservicio backend propietario de la entidad `Inversor`; valida, persiste y expone los datos mediante su API. |
| Azure SQL Database | Almacén de persistencia donde se escribe y consulta la tabla `Inversores` vía Entity Framework Core 8. |
 
### 3. Precondiciones
 
| # | Condición |
|---|---|
| P-01 | El Catálogo Service está desplegado y disponible como Azure Container App con ingress externo habilitado. |
| P-02 | La Azure SQL Database está aprovisionada y la cadena de conexión está configurada en las variables de entorno del Catálogo Service (no en código fuente). |
| P-03 | El Catálogo Service tiene el catálogo de idiomas soportados cargado en la base de datos (mínimo español e inglés). |
| P-04 | El coordinador tiene acceso al frontend (no se requiere autenticación según el SPEC). |
 
### 4. Flujo Principal
 
**Escenario base: Registrar un nuevo inversor**
 
1. El coordinador navega a la pantalla de mantenimiento de inversores en el frontend ASP.NET MVC.
2. El frontend presenta el formulario de registro con los campos: nombre completo, empresa, país de origen, idiomas (selección múltiple), fecha de inicio de visita, fecha de cierre de visita y lugar de hospedaje.
3. El coordinador completa todos los campos y envía el formulario.
4. El frontend realiza validaciones de formato básicas en el cliente (campos requeridos, formato de fecha) y, si son correctas, construye una petición `POST /api/inversores` hacia el Catálogo Service.
5. El Catálogo Service recibe la petición en su capa API (Controller).
6. La capa API delega el procesamiento al caso de uso `RegistrarInversorHandler` en la capa Application.
7. La capa Application ejecuta las validaciones de negocio:
   a. Verifica que el conjunto de idiomas proporcionado contenga al menos un elemento (RN-01).
   b. Verifica que `FechaFinVisita` sea mayor o igual a `FechaInicioVisita` (RN-02).
8. Si las validaciones pasan, la capa Application invoca el repositorio `IInversorRepository` para persistir el nuevo `Inversor`.
9. La capa Infrastructure (`InversorRepository`) genera un nuevo `GUID` como identificador, asigna las relaciones con los idiomas seleccionados en la tabla `InversoresIdiomas` y persiste ambas entidades en la misma transacción a través de Entity Framework Core 8.
10. El Catálogo Service retorna `HTTP 201 Created` con el objeto `Inversor` completo (incluyendo el `Id` generado) en el cuerpo de la respuesta.
11. El frontend recibe la respuesta exitosa y actualiza el listado de inversores mostrando el nuevo registro al coordinador.
### 5. Flujos Alternos
 
#### FA-01 — Actualizar datos de un inversor existente
 
1. El coordinador selecciona un inversor existente del listado y elige la acción de edición.
2. El frontend carga los datos actuales del inversor vía `GET /api/inversores/{id}` y los presenta en el formulario.
3. El coordinador modifica los campos deseados y confirma.
4. El frontend envía `PUT /api/inversores/{id}` al Catálogo Service.
5. La capa Application ejecuta las mismas validaciones de negocio que en el flujo principal (RN-01, RN-02).
6. Si las validaciones pasan, el repositorio actualiza el registro existente y las relaciones de idioma (eliminando las anteriores e insertando las nuevas) en la misma transacción vía EF Core 8.
7. El Catálogo Service retorna `HTTP 200 OK` con el objeto actualizado.
8. El frontend refleja los cambios en el listado.
#### FA-02 — Consultar detalle de un inversor
 
1. El coordinador selecciona un inversor del listado.
2. El frontend envía `GET /api/inversores/{id}` al Catálogo Service.
3. La capa Application recupera el inversor con sus idiomas asociados mediante el repositorio.
4. El Catálogo Service retorna `HTTP 200 OK` con el objeto completo.
5. El frontend muestra el detalle del inversor, incluyendo su ventana de visita, idiomas y lugar de hospedaje.
#### FA-03 — Eliminar un inversor sin agendas activas
 
1. El coordinador selecciona un inversor y elige la acción de eliminación.
2. El frontend solicita confirmación al coordinador.
3. Confirmada la acción, el frontend envía `DELETE /api/inversores/{id}` al Catálogo Service.
4. La capa Application verifica que el inversor no tenga agendas en estado `Activa` consultando el repositorio de agendas (RN-03).
5. Al confirmarse que no hay agendas activas, el repositorio elimina el registro del inversor y sus relaciones de idioma.
6. El Catálogo Service retorna `HTTP 200 OK`.
7. El frontend remueve el inversor del listado.
### 6. Flujos de Excepción
 
#### FE-01 — Inversor sin idiomas asignados (RN-01)
 
- **Condición:** El coordinador intenta registrar o actualizar un inversor sin seleccionar ningún idioma.
- **Procesamiento:** La capa Application detecta la violación de RN-01 y lanza `IdiomaRequeridoException`.
- **Respuesta:** El Catálogo Service retorna `HTTP 400 Bad Request` con el mensaje: `"El inversor debe tener al menos un idioma asignado"`.
- **Compensación:** No se persiste ningún dato. El formulario permanece abierto con el mensaje de error visible.
#### FE-02 — Fechas de visita inválidas (RN-02)
 
- **Condición:** `FechaFinVisita` es anterior a `FechaInicioVisita`.
- **Procesamiento:** La capa Application detecta la violación de RN-02 y lanza `FechaVisitaInvalidaException`.
- **Respuesta:** El Catálogo Service retorna `HTTP 400 Bad Request` con el mensaje: `"La fecha de cierre no puede ser anterior a la fecha de inicio"`.
- **Compensación:** No se persiste ningún dato.
#### FE-03 — Eliminación bloqueada por agendas activas (RN-03)
 
- **Condición:** El inversor tiene una o más agendas en estado `Activa`.
- **Procesamiento:** La capa Application detecta la violación de RN-03 y lanza `InversorConAgendasActivasException`.
- **Respuesta:** El Catálogo Service retorna `HTTP 409 Conflict` con el mensaje: `"No es posible eliminar un inversor con agendas activas"`.
- **Compensación:** El registro del inversor permanece intacto.
#### FE-04 — Inversor no encontrado
 
- **Condición:** Se intenta consultar, actualizar o eliminar un inversor con un `Id` que no existe en la base de datos.
- **Procesamiento:** El repositorio no encuentra el registro y la capa Application lanza `InversorNotFoundException`.
- **Respuesta:** El Catálogo Service retorna `HTTP 404 Not Found` con mensaje descriptivo.
- **Compensación:** No aplica; no hay cambios en la base de datos.
#### FE-05 — Catálogo Service no disponible
 
- **Condición:** El Catálogo Service no responde (timeout, error de contenedor o fallo de red).
- **Procesamiento:** El frontend recibe un error de conexión o un `HTTP 503`.
- **Respuesta:** El frontend muestra un mensaje de indisponibilidad temporal al coordinador. El middleware global de excepciones del Catálogo Service registra el incidente en el Log Stream de Azure Container Apps.
- **Compensación:** No aplica; no se inició ninguna transacción.
### 7. Postcondiciones
 
| # | Estado del sistema |
|---|---|
| PC-01 | El registro del `Inversor` está persistido en la tabla `Inversores` de Azure SQL Database con un `Id` único (GUID). |
| PC-02 | Las relaciones del inversor con sus idiomas están persistidas en la tabla `InversoresIdiomas`. |
| PC-03 | El inversor está disponible en el listado del frontend y puede ser seleccionado en el módulo de generación de agendas (CU-04). |
| PC-04 | El Catálogo Service expone el nuevo inversor vía `GET /api/inversores/{id}` con todos sus datos, incluidos los idiomas asignados. |
 
### 8. Reglas de Negocio Aplicables
 
| ID Regla | Descripción |
|---|---|
| RN-01 | El inversor debe tener al menos un idioma asignado. |
| RN-02 | La `FechaFinVisita` debe ser mayor o igual a `FechaInicioVisita`. |
| RN-03 | No se permite eliminar un inversor con agendas en estado `Activa`. |
 
### 9. Criterio de Aceptación (Gherkin)
 
```gherkin
Feature: Gestión del catálogo de inversores
 
  Scenario: Registro exitoso de inversor con todos los campos válidos
    Given el coordinador accede al módulo de inversores en el frontend
    And el catálogo de idiomas contiene al menos español e inglés
    When registra nombre completo "María González", empresa "TechCorp", país "Alemania",
         idiomas [inglés, español], fecha inicio "2026-07-01", fecha fin "2026-07-05"
         y lugar de hospedaje "Hotel Marriott San José"
    Then el Catálogo Service retorna HTTP 201 con el objeto Inversor completo incluyendo Id generado
    And el inversor queda disponible en el listado del frontend
    And puede ser seleccionado en el formulario de generación de agendas
 
  Scenario: Rechazo por ausencia de idioma asignado (RN-01)
    Given el coordinador intenta registrar un inversor sin seleccionar ningún idioma
    When envía POST /api/inversores sin idiomas en el payload
    Then el Catálogo Service retorna HTTP 400
    And el mensaje de error es "El inversor debe tener al menos un idioma asignado"
    And no se persiste ningún registro en la tabla Inversores
 
  Scenario: Rechazo por fechas de visita inválidas (RN-02)
    Given el coordinador indica FechaFinVisita "2026-06-30" y FechaInicioVisita "2026-07-01"
    When envía POST /api/inversores con esas fechas
    Then el Catálogo Service retorna HTTP 400
    And el mensaje de error es "La fecha de cierre no puede ser anterior a la fecha de inicio"
 
  Scenario: Rechazo de eliminación con agendas activas (RN-03)
    Given existe un inversor con Id conocido que tiene una agenda en estado "Activa"
    When el coordinador envía DELETE /api/inversores/{id}
    Then el Catálogo Service retorna HTTP 409
    And el mensaje de error es "No es posible eliminar un inversor con agendas activas"
    And el registro del inversor permanece en la base de datos sin cambios
```
 
---
 
## CU-02 — Gestionar Participantes
 
### 1. Identificación
 
| Campo | Valor |
|---|---|
| **ID** | CU-02 |
| **Nombre** | Gestionar Participantes |
| **Componente responsable** | Catálogo Service (`src/Catalogo/`) — capas Application e Infrastructure |
| **Endpoint(s) / Punto de entrada** | `GET /api/participantes` · `POST /api/participantes` · `PUT /api/participantes/{id}` · `DELETE /api/participantes/{id}` |
| **Prioridad** | Alta |
 
### 2. Actores
 
| Actor | Rol |
|---|---|
| Coordinador de agendas | Funcionario de PROCOMER que registra, actualiza, desactiva y consulta participantes desde el frontend. |
| Frontend (ASP.NET Core MVC) | Capa de presentación que envía las peticiones al Catálogo Service vía HTTP REST con JavaScript/AJAX. |
| Catálogo Service | Microservicio propietario de las entidades `Participante`, `ParticipanteIdioma` y `DisponibilidadParticipante`; valida y persiste los datos. |
| Azure SQL Database | Almacén de persistencia de las tablas `Participantes`, `ParticipantesIdiomas` y `DisponibilidadParticipantes` vía EF Core 8. |
 
### 3. Precondiciones
 
| # | Condición |
|---|---|
| P-01 | El Catálogo Service está desplegado y disponible como Azure Container App. |
| P-02 | Existe al menos una `Oficina` registrada en el sistema para asignarla al participante (prerequisito de RN-05). |
| P-03 | El catálogo de idiomas soportados está disponible en la base de datos. |
| P-04 | El coordinador tiene acceso al frontend. |
 
### 4. Flujo Principal
 
**Escenario base: Registrar un nuevo participante**
 
1. El coordinador navega a la pantalla de mantenimiento de participantes en el frontend ASP.NET MVC.
2. El frontend muestra el formulario de registro con los campos: nombre completo, cargo o institución, oficina (seleccionada del catálogo de oficinas disponibles), idiomas (selección múltiple) y bloques de disponibilidad horaria por fecha.
3. El coordinador completa todos los campos y envía el formulario.
4. El frontend valida formato básico en el cliente y construye la petición `POST /api/participantes` hacia el Catálogo Service.
5. El Catálogo Service recibe la petición en su capa API (Controller).
6. La capa API delega al caso de uso `RegistrarParticipanteHandler` en la capa Application.
7. La capa Application ejecuta las validaciones de negocio:
   a. Verifica que se haya asignado al menos un idioma (RN-04).
   b. Verifica que se haya asignado exactamente una oficina existente (RN-05).
8. Si las validaciones pasan, la capa Application invoca el repositorio `IParticipanteRepository`.
9. La capa Infrastructure persiste en la misma transacción EF Core 8:
   a. El registro en la tabla `Participantes` con estado `Activo` y el `OficinaId` asignado.
   b. Las relaciones en `ParticipantesIdiomas`.
   c. Los bloques de disponibilidad en `DisponibilidadParticipantes` (si se proporcionaron).
10. El Catálogo Service retorna `HTTP 201 Created` con el objeto `Participante` completo.
11. El frontend actualiza el listado de participantes.
### 5. Flujos Alternos
 
#### FA-01 — Actualizar datos de un participante
 
1. El coordinador selecciona un participante existente del listado y elige la acción de edición.
2. El frontend carga los datos actuales vía `GET /api/participantes/{id}` y los presenta en el formulario.
3. El coordinador modifica los campos deseados (incluyendo modificar idiomas, oficina o disponibilidad) y confirma.
4. El frontend envía `PUT /api/participantes/{id}` al Catálogo Service.
5. La capa Application ejecuta las mismas validaciones de negocio (RN-04, RN-05).
6. El repositorio actualiza el registro, reemplaza las relaciones de idioma y actualiza los bloques de disponibilidad en la misma transacción EF Core 8.
7. El Catálogo Service retorna `HTTP 200 OK` con el objeto actualizado.
8. El frontend refleja los cambios.
#### FA-02 — Desactivar lógicamente a un participante
 
1. El coordinador selecciona un participante activo y elige la acción de desactivación.
2. El frontend envía `PUT /api/participantes/{id}` al Catálogo Service con el campo `Estado` establecido en `Inactivo`.
3. La capa Application actualiza únicamente el campo `Estado` del participante sin eliminar el registro.
4. El Catálogo Service retorna `HTTP 200 OK`.
5. El frontend muestra al participante como inactivo en el listado.
6. El participante queda excluido automáticamente de futuros procesos de scheduling en CU-04, ya que el motor de scheduling filtra candidatos por estado `Activo`.
#### FA-03 — Agregar bloques de disponibilidad horaria a un participante existente
 
1. El coordinador selecciona un participante existente y accede a la gestión de su disponibilidad.
2. El frontend presenta un calendario o formulario donde el coordinador puede agregar bloques horarios por fecha (hora inicio, hora fin, fecha).
3. El frontend envía `PUT /api/participantes/{id}` con los bloques nuevos o actualizados.
4. La capa Application valida que los bloques horarios no se solapan entre sí para el mismo participante.
5. El repositorio persiste los bloques en `DisponibilidadParticipantes` vía EF Core 8.
6. El Catálogo Service retorna `HTTP 200 OK`.
### 6. Flujos de Excepción
 
#### FE-01 — Participante sin idiomas asignados (RN-04)
 
- **Condición:** El coordinador intenta registrar o actualizar un participante sin seleccionar ningún idioma.
- **Procesamiento:** La capa Application lanza `IdiomaRequeridoParticipanteException`.
- **Respuesta:** El Catálogo Service retorna `HTTP 400 Bad Request` con el mensaje: `"El participante debe tener al menos un idioma asignado"`.
- **Compensación:** No se persiste ningún dato.
#### FE-02 — Participante sin oficina asignada (RN-05)
 
- **Condición:** El coordinador intenta registrar o actualizar un participante sin asignar una oficina.
- **Procesamiento:** La capa Application lanza `OficinaRequeridaException`.
- **Respuesta:** El Catálogo Service retorna `HTTP 400 Bad Request` con el mensaje: `"El participante debe tener una oficina asignada"`.
- **Compensación:** No se persiste ningún dato.
#### FE-03 — Oficina referenciada no existe
 
- **Condición:** El `OficinaId` enviado no corresponde a ningún registro en la tabla `Oficinas`.
- **Procesamiento:** El repositorio no encuentra la oficina y la capa Application lanza `OficinaNotFoundException`.
- **Respuesta:** El Catálogo Service retorna `HTTP 404 Not Found` con mensaje descriptivo indicando que la oficina no existe.
- **Compensación:** No se persiste ningún dato.
#### FE-04 — Participante no encontrado
 
- **Condición:** Se intenta consultar, actualizar o eliminar un participante con un `Id` inexistente.
- **Procesamiento:** La capa Application lanza `ParticipanteNotFoundException`.
- **Respuesta:** `HTTP 404 Not Found` con mensaje descriptivo.
- **Compensación:** No aplica.
#### FE-05 — Catálogo Service no disponible
 
- **Condición:** El Catálogo Service no responde.
- **Procesamiento:** El frontend recibe error de conexión o `HTTP 503`.
- **Respuesta:** El frontend muestra mensaje de indisponibilidad temporal al coordinador.
- **Compensación:** No aplica; ninguna transacción fue iniciada.
### 7. Postcondiciones
 
| # | Estado del sistema |
|---|---|
| PC-01 | El `Participante` está persistido en la tabla `Participantes` con estado `Activo` y el `OficinaId` asignado. |
| PC-02 | Las relaciones de idioma están persistidas en `ParticipantesIdiomas`. |
| PC-03 | Los bloques de disponibilidad horaria están persistidos en `DisponibilidadParticipantes` (si se proporcionaron). |
| PC-04 | El participante está disponible como candidato en el formulario de generación de agendas de CU-04, siempre que su estado sea `Activo`. |
 
### 8. Reglas de Negocio Aplicables
 
| ID Regla | Descripción |
|---|---|
| RN-04 | Todo participante debe tener al menos un idioma asignado. |
| RN-05 | Todo participante debe tener exactamente una oficina asignada. |
 
### 9. Criterio de Aceptación (Gherkin)
 
```gherkin
Feature: Gestión del catálogo de participantes
 
  Scenario: Registro exitoso de participante con todos los campos válidos
    Given existe la oficina "Edificio PROCOMER Torre Norte" registrada en el sistema
    And el catálogo de idiomas contiene español e inglés
    When el coordinador registra a "Juan Pérez", cargo "Director de Inversiones",
         oficina "Edificio PROCOMER Torre Norte", idiomas [español], disponibilidad para 2026-07-02
         entre 08:00 y 12:00
    Then el Catálogo Service retorna HTTP 201 con el objeto Participante completo en estado Activo
    And el participante queda disponible como candidato en el módulo de Agendas
 
  Scenario: Desactivación lógica de participante activo
    Given existe un participante con Id conocido en estado "Activo"
    When el coordinador envía PUT /api/participantes/{id} con Estado "Inactivo"
    Then el Catálogo Service retorna HTTP 200
    And el participante permanece en la base de datos con Estado "Inactivo"
    And no aparece como candidato disponible en futuros procesos de scheduling
 
  Scenario: Rechazo por ausencia de idioma (RN-04)
    Given el coordinador intenta registrar un participante sin seleccionar ningún idioma
    When envía POST /api/participantes sin idiomas en el payload
    Then el Catálogo Service retorna HTTP 400
    And el mensaje de error es "El participante debe tener al menos un idioma asignado"
 
  Scenario: Rechazo por ausencia de oficina (RN-05)
    Given el coordinador intenta registrar un participante sin asignar una oficina
    When envía POST /api/participantes sin OficinaId en el payload
    Then el Catálogo Service retorna HTTP 400
    And el mensaje de error es "El participante debe tener una oficina asignada"
```
 
---
 
## CU-03 — Gestionar Oficinas y Matriz de Traslados
 
### 1. Identificación
 
| Campo | Valor |
|---|---|
| **ID** | CU-03 |
| **Nombre** | Gestionar Oficinas y Matriz de Traslados |
| **Componente responsable** | Catálogo Service (`src/Catalogo/`) — capas Application e Infrastructure |
| **Endpoint(s) / Punto de entrada** | `GET /api/oficinas` · `POST /api/oficinas` · `DELETE /api/oficinas/{id}` · `GET /api/traslados` · `POST /api/traslados` |
| **Prioridad** | Alta |
 
### 2. Actores
 
| Actor | Rol |
|---|---|
| Coordinador de agendas | Funcionario de PROCOMER que registra y elimina oficinas, y mantiene la matriz de tiempos de traslado entre ellas. |
| Frontend (ASP.NET Core MVC) | Capa de presentación que envía las peticiones al Catálogo Service. |
| Catálogo Service | Microservicio propietario de las entidades `Oficina` y `MatrizTraslado`; garantiza la simetría de la matriz. |
| Agendas Service | Consumidor de la matriz de traslados durante el proceso de scheduling (CU-04); la consulta vía llamada HTTP al Catálogo Service usando `IHttpClientFactory` con políticas de resiliencia. |
| Azure SQL Database | Almacén de persistencia de las tablas `Oficinas` y `MatrizTraslados` vía EF Core 8. |
 
### 3. Precondiciones
 
| # | Condición |
|---|---|
| P-01 | El Catálogo Service está desplegado y disponible. |
| P-02 | El coordinador tiene acceso al frontend. |
| P-03 | Para registrar un par de traslado, ambas oficinas referenciadas deben existir previamente en la tabla `Oficinas`. |
 
### 4. Flujo Principal
 
**Escenario base: Registrar una nueva oficina**
 
1. El coordinador navega a la pantalla de mantenimiento de oficinas en el frontend ASP.NET MVC.
2. El frontend presenta el formulario de registro con los campos: nombre de la oficina, dirección física completa y coordenadas geográficas (opcionales).
3. El coordinador completa los campos obligatorios y envía el formulario.
4. El frontend construye la petición `POST /api/oficinas` hacia el Catálogo Service.
5. El Catálogo Service recibe la petición en la capa API (Controller) y delega al caso de uso `RegistrarOficinaHandler` en la capa Application.
6. La capa Application valida que el nombre de la oficina no esté vacío y que la dirección física esté presente.
7. El repositorio `IOficinaRepository` persiste el nuevo `Oficina` en la tabla `Oficinas` vía EF Core 8.
8. El Catálogo Service retorna `HTTP 201 Created` con el objeto `Oficina` completo (incluyendo el `Id` generado).
9. El frontend actualiza el listado de oficinas.
**Escenario complementario: Registrar un par de traslado**
 
1. El coordinador navega a la sección de matriz de traslados y selecciona dos oficinas del catálogo disponible (origen y destino) e ingresa el tiempo estimado de desplazamiento en minutos.
2. El frontend envía `POST /api/traslados` al Catálogo Service con `OficinaOrigenId`, `OficinaDestinoId` y `TiempoMinutos`.
3. El Catálogo Service recibe la petición y delega al caso de uso `RegistrarTrasladoHandler` en la capa Application.
4. La capa Application verifica que las dos oficinas existan en la base de datos.
5. La capa Application aplica la garantía de simetría (RN-07): persiste **dos registros** en la tabla `MatrizTraslados` dentro de la misma transacción EF Core 8:
   a. El par directo: `OficinaOrigenId → OficinaDestinoId` con el `TiempoMinutos` indicado.
   b. El par inverso: `OficinaDestinoId → OficinaOrigenId` con el mismo `TiempoMinutos`.
6. El Catálogo Service retorna `HTTP 201 Created` confirmando que ambos pares fueron persistidos.
7. El frontend actualiza la vista de la matriz mostrando ambas direcciones con el mismo tiempo.
### 5. Flujos Alternos
 
#### FA-01 — Consultar la matriz de traslados completa
 
1. El coordinador o el Agendas Service (durante CU-04) invoca `GET /api/traslados`.
2. El Catálogo Service recupera todos los pares de la tabla `MatrizTraslados` vía repositorio.
3. El Catálogo Service retorna `HTTP 200 OK` con la lista completa de pares (incluidos los simétricos) y sus tiempos.
4. El consumidor (frontend o Agendas Service) utiliza los datos para presentar la matriz o para resolver tiempos de traslado en el motor de scheduling.
#### FA-02 — Actualizar tiempo de traslado existente
 
1. El coordinador selecciona un par de traslado existente y modifica el tiempo.
2. El frontend envía una petición de actualización al Catálogo Service (vía `PUT /api/traslados/{id}` o como parte de un `POST /api/traslados` con sobreescritura).
3. La capa Application actualiza el par directo y, por la garantía de simetría (RN-07), actualiza también el par inverso en la misma transacción EF Core 8.
4. El Catálogo Service retorna `HTTP 200 OK`.
5. Ambas direcciones quedan con el mismo nuevo valor en la base de datos.
#### FA-03 — Eliminar una oficina sin participantes activos
 
1. El coordinador selecciona una oficina del listado y elige la acción de eliminación.
2. El frontend solicita confirmación.
3. Confirmado, el frontend envía `DELETE /api/oficinas/{id}` al Catálogo Service.
4. La capa Application verifica que la oficina no tenga participantes activos asignados (RN-06).
5. El repositorio elimina el registro de la oficina y, en cascada, los pares de traslado que la referencian, en la misma transacción EF Core 8.
6. El Catálogo Service retorna `HTTP 200 OK`.
7. El frontend remueve la oficina del listado.
### 6. Flujos de Excepción
 
#### FE-01 — Eliminación bloqueada por participantes activos (RN-06)
 
- **Condición:** La oficina tiene uno o más participantes en estado `Activo` asignados.
- **Procesamiento:** La capa Application lanza `OficinaConParticipantesActivosException`.
- **Respuesta:** `HTTP 409 Conflict` con el mensaje: `"No es posible eliminar una oficina con participantes activos asignados"`.
- **Compensación:** El registro de la oficina permanece intacto.
#### FE-02 — Par de traslado con oficina inexistente
 
- **Condición:** `OficinaOrigenId` o `OficinaDestinoId` no existen en la tabla `Oficinas`.
- **Procesamiento:** La capa Application lanza `OficinaNotFoundException` para la oficina no encontrada.
- **Respuesta:** `HTTP 404 Not Found` indicando cuál oficina no existe.
- **Compensación:** No se persiste ningún par de traslado; la transacción no se inicia.
#### FE-03 — Oficina no encontrada para consulta o eliminación
 
- **Condición:** Se intenta consultar o eliminar una oficina con `Id` inexistente.
- **Procesamiento:** La capa Application lanza `OficinaNotFoundException`.
- **Respuesta:** `HTTP 404 Not Found` con mensaje descriptivo.
- **Compensación:** No aplica.
#### FE-04 — Catálogo Service no disponible (impacto en CU-04)
 
- **Condición:** El Agendas Service intenta consultar la matriz de traslados durante CU-04 y el Catálogo Service no responde.
- **Procesamiento:** El `IHttpClientFactory` con política de resiliencia (Microsoft.Extensions.Http.Resilience) reintenta hasta 3 veces con backoff exponencial.
- **Respuesta:** Si el fallo persiste tras los reintentos, el Agendas Service retorna `HTTP 503 Service Unavailable` al frontend con mensaje descriptivo.
- **Compensación:** No se genera ninguna agenda; el incidente queda registrado en el Log Stream de Azure Container Apps.
### 7. Postcondiciones
 
| # | Estado del sistema |
|---|---|
| PC-01 | La `Oficina` está persistida en la tabla `Oficinas` con un `Id` único (GUID). |
| PC-02 | El par de traslado `A → B` y su simétrico `B → A` están persistidos en `MatrizTraslados` con el mismo `TiempoMinutos`. |
| PC-03 | La oficina está disponible para ser asignada a participantes en CU-02. |
| PC-04 | La matriz de traslados puede ser consultada por el Agendas Service durante el proceso de scheduling en CU-04. |
 
### 8. Reglas de Negocio Aplicables
 
| ID Regla | Descripción |
|---|---|
| RN-06 | No se puede eliminar una oficina con participantes activos asignados. |
| RN-07 | La `MatrizTraslado` debe ser simétrica: `TiempoMinutos(A→B)` = `TiempoMinutos(B→A)`; la garantía se aplica automáticamente en la capa Application del Catálogo Service. |
 
### 9. Criterio de Aceptación (Gherkin)
 
```gherkin
Feature: Gestión de oficinas y matriz de traslados
 
  Scenario: Registro de par de traslado con garantía de simetría automática (RN-07)
    Given existen las oficinas "Edificio PROCOMER" (Id: A) y "Ministerio de Economía" (Id: B)
    When el coordinador envía POST /api/traslados con OficinaOrigenId=A, OficinaDestinoId=B, TiempoMinutos=25
    Then el Catálogo Service retorna HTTP 201
    And la tabla MatrizTraslados contiene el par A→B con TiempoMinutos=25
    And la tabla MatrizTraslados contiene el par B→A con TiempoMinutos=25
    And GET /api/traslados retorna ambos pares con el mismo valor
 
  Scenario: Actualización de tiempo mantiene simetría (RN-07)
    Given existe el par de traslado A→B con TiempoMinutos=20 y su simétrico B→A=20
    When el coordinador actualiza ese tiempo a 30 minutos
    Then el par A→B queda con TiempoMinutos=30
    And el par B→A queda automáticamente con TiempoMinutos=30
 
  Scenario: Rechazo de eliminación de oficina con participantes activos (RN-06)
    Given la oficina "Edificio PROCOMER Torre Norte" tiene al menos un participante en estado Activo
    When el coordinador envía DELETE /api/oficinas/{id}
    Then el Catálogo Service retorna HTTP 409
    And el mensaje de error es "No es posible eliminar una oficina con participantes activos asignados"
    And la oficina permanece en el sistema
 
  Scenario: Fallo en consulta de traslados activa política de reintentos hacia Agendas Service
    Given el Catálogo Service no responde durante una generación de agenda
    When el Agendas Service intenta GET /api/traslados para obtener la matriz
    Then el Agendas Service reintenta la llamada hasta 3 veces con backoff exponencial
    And si el fallo persiste, retorna HTTP 503 al frontend con mensaje descriptivo
    And registra el incidente en el Log Stream de Azure Container Apps
```
 
---
 
## CU-04 — Generar Agenda Automática
 
### 1. Identificación
 
| Campo | Valor |
|---|---|
| **ID** | CU-04 |
| **Nombre** | Generar Agenda Automática |
| **Componente responsable** | Agendas Service (`src/Agendas/`) — capas Application (motor `SchedulingEngine`) e Infrastructure |
| **Endpoint(s) / Punto de entrada** | `POST /agendas/generar` |
| **Prioridad** | Alta |
 
### 2. Actores
 
| Actor | Rol |
|---|---|
| Coordinador de agendas | Funcionario de PROCOMER que selecciona el inversor, los candidatos, la fecha, la duración y la meta de reuniones desde el frontend. |
| Frontend (ASP.NET Core MVC) | Capa de presentación que presenta el formulario de generación con visualización dinámica de datos del inversor y envía la solicitud al Agendas Service. |
| Agendas Service | Microservicio propietario del motor de scheduling; orquesta la generación, valida las restricciones y persiste la agenda resultante. |
| Catálogo Service | Proveedor de datos maestros (inversores, participantes con disponibilidad, oficinas, matriz de traslados); consumido por el Agendas Service vía `IHttpClientFactory` tipado con política de resiliencia. |
| Azure SQL Database | Almacén donde el Agendas Service persiste las entidades `Agenda` y `Reunion` vía EF Core 8. |
 
### 3. Precondiciones
 
| # | Condición |
|---|---|
| P-01 | El Agendas Service y el Catálogo Service están desplegados y disponibles como Azure Container Apps. |
| P-02 | Existe al menos un `Inversor` registrado con idiomas asignados y una ventana de visita válida (CU-01). |
| P-03 | Existen participantes en estado `Activo` con idiomas, oficinas y bloques de disponibilidad registrados para la fecha solicitada (CU-02). |
| P-04 | Existen al menos dos `Oficinas` con sus tiempos de traslado cargados en la `MatrizTraslados` (CU-03). |
| P-05 | La `MatrizTraslados` incluye la ruta desde el lugar de hospedaje del inversor (tratado como punto de partida) o bien se asume tiempo cero cuando no existe el par en la matriz. |
 
### 4. Flujo Principal
 
**Escenario base: Generación exitosa de agenda que alcanza la meta**
 
1. El coordinador navega a la pantalla de generación de agendas en el frontend ASP.NET MVC.
2. El coordinador selecciona un inversor del catálogo desplegado en el formulario.
3. El frontend, mediante una llamada AJAX a `GET /api/inversores/{id}` del Catálogo Service, recupera y muestra dinámicamente los datos del inversor seleccionado: empresa que representa, idiomas que maneja y ventana de visita disponible (`FechaInicioVisita` – `FechaFinVisita`). El campo de fecha de la agenda se restringe automáticamente a ese rango.
4. El coordinador selecciona la fecha de la agenda (dentro del rango habilitado), elige los participantes candidatos del listado de participantes activos, ingresa la duración estándar de reunión en minutos y la meta de reuniones deseadas.
5. El coordinador envía el formulario; el frontend construye la petición `POST /agendas/generar` hacia el Agendas Service.
6. El Agendas Service recibe la petición en la capa API (Controller) y delega al caso de uso `GenerarAgendaHandler` en la capa Application.
7. La capa Application valida que la fecha solicitada esté dentro del rango de visita del inversor (RN-08). Si no, lanza `FechaFueraDeRangoException` (ver FE-01).
8. La capa Application invoca al `IHttpClientFactory` tipado hacia el Catálogo Service para obtener:
   a. Los datos completos del inversor y sus idiomas.
   b. Los candidatos seleccionados con sus idiomas, oficinas y bloques de disponibilidad para la fecha solicitada (solo bloques del día; ya disponibles en la respuesta del Catálogo Service).
   c. La matriz completa de traslados.
9. El `LanguageCompatibilityFilter` filtra los candidatos conservando únicamente los que comparten al menos un idioma con el inversor (RN-12). Si el resultado está vacío, lanza `IdiomaIncompatibleException` (ver FE-02).
10. El `AvailabilitySlotBuilder` construye los bloques horarios válidos de cada candidato compatible para la fecha, aplicando los límites 08:00–17:00 (RN-09, RN-10) y excluyendo el bloque 12:00–13:00 (RN-11).
11. El `SchedulingEngine` ejecuta el algoritmo greedy:
    a. Ordena los candidatos por la hora de inicio de su primer bloque disponible (ascendente).
    b. Inicializa la secuencia con `horaActual = 08:00` y `ultimaOficinaId` = punto de hospedaje del inversor.
    c. Para cada candidato (hasta alcanzar la meta o agotar la lista), intenta ubicar una reunión en el primer bloque horario donde: el intervalo entre `horaActual + tiempoTraslado` y `horaActual + tiempoTraslado + duracionMinutos` sea compatible con todos los límites (RN-09, RN-10, RN-11, RN-13, RN-14).
    d. Cuando una reunión es ubicable, determina el idioma de la reunión como el primer idioma compartido entre inversor y participante (RN-12), agrega la `Reunion` a la secuencia y avanza `horaActual` y `ultimaOficinaId`.
    e. Si se alcanza la meta de reuniones, detiene la iteración.
12. El `SchedulingEngine` retorna un `AgendaResult` con la lista ordenada de `Reunion`, la cantidad lograda y si se alcanzó la meta.
13. La capa Application persiste en la misma transacción EF Core 8:
    a. El registro `Agenda` (con `InversorId`, `Fecha`, `Estado = Activa`, `FechaGeneracion`).
    b. Los registros `Reunion` con sus atributos: `AgendaId`, `ParticipanteId`, `HoraInicio`, `HoraFin`, `OficinaId`, `IdiomaReunion`, `Orden` y `TiempoTrasladoSiguiente`.
14. El Agendas Service retorna `HTTP 201 Created` con el `AgendaResult` completo, incluyendo todas las reuniones con sus horarios, participantes, oficinas e idiomas.
15. El frontend muestra al coordinador el itinerario generado con toda la información de las reuniones.
### 5. Flujos Alternos
 
#### FA-01 — Agenda generada de forma parcial (meta no alcanzable)
 
1. El motor de scheduling agota la lista de candidatos compatibles antes de alcanzar la meta de reuniones solicitada.
2. El `SchedulingEngine` retorna `AgendaResult` con `Completa = false`, indicando las reuniones logradas y las solicitadas.
3. La capa Application persiste la agenda parcial con las reuniones que fue posible programar.
4. El Agendas Service retorna `HTTP 201 Created` con la agenda parcial e incluye en la respuesta el aviso explícito de que se lograron `N` de `M` reuniones solicitadas.
5. El frontend muestra el itinerario parcial junto con el aviso al coordinador.
#### FA-02 — Visualización dinámica del inversor al seleccionarlo (paso 3 del flujo principal detallado)
 
1. El coordinador selecciona un inversor distinto en el formulario (sin enviar aún la solicitud de generación).
2. El frontend lanza una llamada AJAX `GET /api/inversores/{id}` al Catálogo Service.
3. El Catálogo Service retorna `HTTP 200 OK` con los datos del inversor.
4. El frontend actualiza dinámicamente el panel del formulario mostrando: empresa, idiomas y ventana de visita. El campo de fecha se recalcula para restringir el rango al nuevo inversor.
### 6. Flujos de Excepción
 
#### FE-01 — Fecha fuera del rango de visita del inversor (RN-08)
 
- **Condición:** La fecha solicitada es anterior a `FechaInicioVisita` o posterior a `FechaFinVisita` del inversor.
- **Procesamiento:** La capa Application lanza `FechaFueraDeRangoException` antes de invocar el motor.
- **Respuesta:** `HTTP 422 Unprocessable Entity` con el mensaje: `"La fecha {fecha} está fuera del período de visita del inversor ({FechaInicio} – {FechaFin})"`. Código interno: `FECHA_FUERA_DE_RANGO`.
- **Compensación:** No se persiste ninguna agenda; no se invoca el motor de scheduling.
#### FE-02 — Ningún candidato comparte idioma con el inversor (RN-12)
 
- **Condición:** El `LanguageCompatibilityFilter` retorna una lista vacía tras filtrar todos los candidatos seleccionados.
- **Procesamiento:** La capa Application lanza `IdiomaIncompatibleException`.
- **Respuesta:** `HTTP 422 Unprocessable Entity` con el mensaje: `"No existen participantes que compartan idioma con el inversor. Verifique la configuración de idiomas de los candidatos."`. Código interno: `IDIOMA_INCOMPATIBLE`.
- **Compensación:** No se persiste ninguna agenda.
#### FE-03 — Ningún participante compatible tiene disponibilidad en la fecha
 
- **Condición:** Todos los candidatos compatibles en idioma no tienen bloques de disponibilidad registrados para la fecha solicitada, o todos sus bloques fueron excluidos por las restricciones de horario.
- **Procesamiento:** El `SchedulingEngine` retorna `SchedulingError(SinDisponibilidad)`.
- **Respuesta:** `HTTP 422 Unprocessable Entity` con el mensaje: `"Ningún participante compatible tiene disponibilidad registrada para la fecha solicitada."`. Código interno: `SIN_DISPONIBILIDAD`.
- **Compensación:** No se persiste ninguna agenda.
#### FE-04 — Tiempos de traslado impiden toda combinación
 
- **Condición:** Los tiempos de traslado entre las oficinas de los candidatos disponibles son superiores al tiempo libre entre reuniones para cualquier combinación posible.
- **Procesamiento:** El `SchedulingEngine` no logra ubicar ninguna reunión y retorna `SchedulingError(TrasladosInviables)`.
- **Respuesta:** `HTTP 422 Unprocessable Entity` con el mensaje: `"Los tiempos de traslado entre las oficinas disponibles no permiten encadenar ninguna reunión para esa fecha. Considere reducir la duración de reunión o seleccionar participantes en oficinas más cercanas."`. Código interno: `TRASLADOS_INVIABLES`.
- **Compensación:** No se persiste ninguna agenda.
#### FE-05 — Catálogo Service no disponible durante la generación (AC-09)
 
- **Condición:** El Catálogo Service no responde cuando el Agendas Service intenta obtener datos maestros.
- **Procesamiento:** El `IHttpClientFactory` con política de resiliencia (Microsoft.Extensions.Http.Resilience) reintenta hasta 3 veces con backoff exponencial. Si persiste el fallo, la capa Application lanza `CatalogoServiceNoDisponibleException`.
- **Respuesta:** `HTTP 503 Service Unavailable` con mensaje descriptivo. El incidente queda registrado en el Log Stream de Azure Container Apps.
- **Compensación:** No se persiste ninguna agenda; la transacción EF Core 8 no se inicia.
### 7. Postcondiciones
 
| # | Estado del sistema |
|---|---|
| PC-01 | El registro `Agenda` está persistido en la tabla `Agendas` con `Estado = Activa` y `FechaGeneracion` del momento de creación. |
| PC-02 | Los registros `Reunion` están persistidos en la tabla `Reuniones` con todos los atributos: `HoraInicio`, `HoraFin`, `ParticipanteId`, `OficinaId`, `IdiomaReunion`, `Orden` y `TiempoTrasladoSiguiente`. |
| PC-03 | La agenda puede ser consultada vía `GET /agendas/{id}` y su PDF puede ser descargado vía `GET /agendas/{id}/pdf` (CU-06). |
| PC-04 | El inversor tiene al menos una agenda activa, lo que activa la restricción RN-03 para su eliminación (CU-01). |
 
### 8. Reglas de Negocio Aplicables
 
| ID Regla | Descripción |
|---|---|
| RN-08 | La fecha de la agenda debe estar dentro del rango `[FechaInicioVisita, FechaFinVisita]` del inversor. |
| RN-09 | Ninguna reunión puede iniciar antes de las 08:00 horas. |
| RN-10 | Ninguna reunión puede finalizar después de las 17:00 horas. |
| RN-11 | El bloque 12:00–13:00 está reservado para almuerzo; ninguna reunión puede solaparse con él. |
| RN-12 | El inversor y el participante deben compartir al menos un idioma para que la reunión sea válida. |
| RN-13 | El intervalo entre el fin de una reunión y el inicio de la siguiente debe ser mayor o igual al `TiempoMinutos` de traslado entre las oficinas correspondientes. |
| RN-14 | Un mismo participante no puede tener dos reuniones con horarios solapados en la misma agenda. |
 
### 9. Criterio de Aceptación (Gherkin)
 
```gherkin
Feature: Generación automática de agenda para inversor
 
  Scenario: Generación exitosa con meta alcanzada
    Given existe un inversor con idiomas [español, inglés] y visita del 2026-07-01 al 2026-07-05
    And existen 5 participantes activos con idioma compartido y disponibilidad el 2026-07-02
    And la MatrizTraslados tiene tiempos entre todas sus oficinas
    When el coordinador envía POST /agendas/generar con
         inversorId válido, candidatosIds de los 5 participantes, fecha=2026-07-02,
         duracionMinutos=60, metaReuniones=3
    Then el Agendas Service retorna HTTP 201 con una agenda que contiene exactamente 3 reuniones
    And cada reunión especifica HoraInicio, HoraFin, nombre del participante, cargo, oficina e IdiomaReunion
    And ninguna reunión inicia antes de las 08:00 ni finaliza después de las 17:00 (RN-09, RN-10)
    And ninguna reunión solapa el bloque 12:00–13:00 (RN-11)
    And el intervalo entre reuniones consecutivas es mayor o igual al TiempoMinutos de traslado (RN-13)
    And la agenda queda persistida en la tabla Agendas con Estado "Activa"
 
  Scenario: Generación de agenda parcial cuando la meta no es alcanzable
    Given solo existen 2 participantes activos compatibles en idioma con disponibilidad en la fecha
    And la meta solicitada es 4 reuniones
    When se solicita POST /agendas/generar con metaReuniones=4
    Then el Agendas Service retorna HTTP 201 con una agenda de 2 reuniones
    And la respuesta indica explícitamente que se lograron 2 de 4 reuniones solicitadas
    And la agenda queda persistida con las 2 reuniones posibles
 
  Scenario: Rechazo por ningún idioma compartido (RN-12)
    Given el inversor tiene idioma [español] únicamente
    And todos los candidatos seleccionados tienen solo idioma [inglés]
    When se solicita POST /agendas/generar con esos candidatos
    Then el Agendas Service retorna HTTP 422 con código IDIOMA_INCOMPATIBLE
    And el mensaje es "No existen participantes que compartan idioma con el inversor..."
    And no se persiste ninguna agenda
 
  Scenario: Rechazo por fecha fuera del rango de visita (RN-08)
    Given el inversor tiene visita del 2026-07-01 al 2026-07-05
    When se solicita POST /agendas/generar con fecha=2026-07-10
    Then el Agendas Service retorna HTTP 422 con código FECHA_FUERA_DE_RANGO
    And el mensaje indica "La fecha 2026-07-10 está fuera del período de visita..."
 
  Scenario: Reintentos ante indisponibilidad del Catálogo Service
    Given el Catálogo Service no responde durante la consulta de datos maestros
    When el Agendas Service intenta obtener participantes y la matriz de traslados
    Then reintenta hasta 3 veces con backoff exponencial (Microsoft.Extensions.Http.Resilience)
    And si el fallo persiste retorna HTTP 503 al frontend con mensaje descriptivo
    And registra el incidente en el Log Stream de Azure Container Apps
```
 
---
 
## CU-05 — Consultar y Anular Agenda
 
### 1. Identificación
 
| Campo | Valor |
|---|---|
| **ID** | CU-05 |
| **Nombre** | Consultar y Anular Agenda |
| **Componente responsable** | Agendas Service (`src/Agendas/`) — capas Application e Infrastructure |
| **Endpoint(s) / Punto de entrada** | `GET /agendas` · `GET /agendas/{id}` · `DELETE /agendas/{id}` |
| **Prioridad** | Alta |
 
### 2. Actores
 
| Actor | Rol |
|---|---|
| Coordinador de agendas | Funcionario de PROCOMER que consulta el listado de agendas y anula aquellas que ya no son válidas. |
| Frontend (ASP.NET Core MVC) | Capa de presentación que muestra el listado y detalle de agendas, y envía la solicitud de anulación. |
| Agendas Service | Microservicio propietario de la entidad `Agenda`; ejecuta la consulta y la anulación lógica. |
| Azure SQL Database | Almacén de las tablas `Agendas` y `Reuniones`; las anulaciones son actualizaciones de campo (soft delete), nunca eliminaciones físicas. |
 
### 3. Precondiciones
 
| # | Condición |
|---|---|
| P-01 | El Agendas Service está desplegado y disponible como Azure Container App. |
| P-02 | Existe al menos una agenda generada y persistida en el sistema (resultado de CU-04). |
| P-03 | El coordinador tiene acceso al frontend. |
 
### 4. Flujo Principal
 
**Escenario base: Consultar el listado de agendas**
 
1. El coordinador navega a la pantalla de listado de agendas en el frontend ASP.NET MVC.
2. El frontend envía `GET /agendas` al Agendas Service, opcionalmente con parámetros de filtro: `inversorId`, `fecha` y/o `estado`.
3. El Agendas Service recibe la petición en la capa API y delega al caso de uso `ConsultarAgendasHandler` en la capa Application.
4. El repositorio `IAgendaRepository` ejecuta la consulta sobre la tabla `Agendas` (con los filtros aplicados) vía EF Core 8 y retorna la lista de agendas con sus datos resumidos: `Id`, nombre del inversor, `Fecha`, `Estado` y cantidad de reuniones.
5. El Agendas Service retorna `HTTP 200 OK` con la lista de agendas.
6. El frontend presenta el listado al coordinador, con las acciones disponibles por agenda: ver detalle, descargar PDF y anular (esta última solo activa para agendas en estado `Activa`).
**Escenario complementario: Anular una agenda activa**
 
1. Desde el listado o el detalle de una agenda, el coordinador elige la acción de anulación.
2. El frontend solicita confirmación al coordinador.
3. Confirmado, el frontend envía `DELETE /agendas/{id}` al Agendas Service.
4. El Agendas Service recibe la petición en la capa API y delega al caso de uso `AnularAgendaHandler` en la capa Application.
5. La capa Application verifica que la agenda con el `Id` indicado existe y que su `Estado` actual es `Activa`.
6. El repositorio `IAgendaRepository` actualiza únicamente los campos `Estado = Anulada` y `FechaAnulacion = [timestamp actual]` en la tabla `Agendas` vía EF Core 8 (RN-15). El registro y sus `Reuniones` asociadas permanecen físicamente en la base de datos.
7. El Agendas Service retorna `HTTP 200 OK` confirmando la anulación.
8. El frontend actualiza el estado de la agenda en el listado de `Activa` a `Anulada`.
### 5. Flujos Alternos
 
#### FA-01 — Consultar el detalle completo de una agenda
 
1. El coordinador selecciona una agenda del listado y elige la acción de ver detalle.
2. El frontend envía `GET /agendas/{id}` al Agendas Service.
3. El repositorio recupera el registro `Agenda` con todas las `Reunion` asociadas mediante una carga relacional vía EF Core 8.
4. El Agendas Service retorna `HTTP 200 OK` con el JSON completo: datos del inversor (nombre, empresa), fecha, estado, lista de reuniones (con `HoraInicio`, `HoraFin`, nombre del participante, cargo, nombre de la oficina, dirección física, idioma de la reunión y `TiempoTrasladoSiguiente`).
5. El frontend presenta el itinerario completo con todas las reuniones y los tiempos de traslado entre ellas.
#### FA-02 — Consultar agendas con filtros combinados
 
1. El coordinador aplica uno o varios filtros en la pantalla de listado: filtra por inversor específico, por fecha exacta o por estado (`Activa` / `Anulada`).
2. El frontend envía `GET /agendas?inversorId={id}&fecha={fecha}&estado={estado}` al Agendas Service.
3. La capa Application construye la consulta con los filtros activos y retorna únicamente las agendas que cumplen todos los criterios.
4. El Agendas Service retorna `HTTP 200 OK` con el subconjunto de agendas filtrado.
### 6. Flujos de Excepción
 
#### FE-01 — Agenda no encontrada
 
- **Condición:** Se intenta consultar o anular una agenda con un `Id` que no existe en la tabla `Agendas`.
- **Procesamiento:** El repositorio no encuentra el registro y la capa Application lanza `AgendaNotFoundException`.
- **Respuesta:** `HTTP 404 Not Found` con mensaje descriptivo.
- **Compensación:** No aplica.
#### FE-02 — Intento de anular agenda ya anulada
 
- **Condición:** El coordinador intenta anular una agenda cuyo `Estado` ya es `Anulada`.
- **Procesamiento:** La capa Application detecta que el estado no es `Activa` y lanza `AgendaYaAnuladaException`.
- **Respuesta:** `HTTP 409 Conflict` con el mensaje: `"La agenda ya se encuentra anulada"`.
- **Compensación:** No se realiza ninguna modificación en la base de datos.
#### FE-03 — Agendas Service no disponible
 
- **Condición:** El Agendas Service no responde a la petición de consulta o anulación.
- **Procesamiento:** El frontend recibe error de conexión o `HTTP 503`.
- **Respuesta:** El frontend muestra mensaje de indisponibilidad temporal al coordinador. El middleware global de excepciones registra el incidente en el Log Stream de Azure Container Apps.
- **Compensación:** No aplica.
### 7. Postcondiciones
 
**Tras consulta:**
 
| # | Estado del sistema |
|---|---|
| PC-01 | El sistema no fue modificado; se trata de una operación de solo lectura. |
| PC-02 | El frontend presenta la información actualizada de la agenda o el listado al coordinador. |
 
**Tras anulación:**
 
| # | Estado del sistema |
|---|---|
| PC-03 | El campo `Estado` de la agenda ha sido actualizado a `Anulada` en la tabla `Agendas`. |
| PC-04 | El campo `FechaAnulacion` contiene el timestamp exacto del momento de la anulación. |
| PC-05 | El registro de la agenda y todas sus `Reuniones` asociadas permanecen físicamente en la base de datos (soft delete) para trazabilidad histórica (RN-15). |
| PC-06 | El PDF generado originalmente sigue siendo descargable vía `GET /agendas/{id}/pdf` (CU-06). |
| PC-07 | El inversor ya no tiene agendas activas, por lo que la restricción RN-03 deja de bloquear su posible eliminación (si no hay otras agendas activas). |
 
### 8. Reglas de Negocio Aplicables
 
| ID Regla | Descripción |
|---|---|
| RN-15 | La anulación de una agenda es siempre lógica (soft delete); el registro y su PDF se conservan para trazabilidad histórica. |
 
### 9. Criterio de Aceptación (Gherkin)
 
```gherkin
Feature: Consulta y anulación de agendas
 
  Scenario: Listado de agendas con filtro por estado
    Given existen agendas en estado "Activa" y "Anulada" en el sistema
    When el coordinador envía GET /agendas?estado=Activa
    Then el Agendas Service retorna HTTP 200 con únicamente las agendas en estado "Activa"
    And cada elemento del listado incluye Id, nombre del inversor, fecha, estado y cantidad de reuniones
 
  Scenario: Consulta de detalle completo de una agenda
    Given existe una agenda con Id conocido que tiene 3 reuniones persistidas
    When el coordinador envía GET /agendas/{id}
    Then el Agendas Service retorna HTTP 200 con el JSON completo
    And la respuesta incluye para cada reunión: HoraInicio, HoraFin, nombre del participante,
        cargo, nombre de la oficina, dirección física, idioma de la reunión y TiempoTrasladoSiguiente
 
  Scenario: Anulación exitosa de agenda activa (RN-15)
    Given existe una agenda con Id conocido en estado "Activa"
    When el coordinador envía DELETE /agendas/{id}
    Then el Agendas Service retorna HTTP 200
    And el campo Estado de la agenda en la base de datos es "Anulada"
    And el campo FechaAnulacion contiene el timestamp del momento de la operación
    And el registro y sus Reuniones permanecen físicamente en la base de datos
    And GET /agendas/{id}/pdf sigue retornando el PDF generado originalmente
 
  Scenario: Rechazo de anulación de agenda ya anulada
    Given existe una agenda con Id conocido en estado "Anulada"
    When el coordinador envía DELETE /agendas/{id} nuevamente
    Then el Agendas Service retorna HTTP 409
    And el mensaje de error es "La agenda ya se encuentra anulada"
    And el estado del registro no cambia
```
 
---
 
## CU-06 — Descargar PDF del Itinerario
 
### 1. Identificación
 
| Campo | Valor |
|---|---|
| **ID** | CU-06 |
| **Nombre** | Descargar PDF del Itinerario |
| **Componente responsable** | Agendas Service (`src/Agendas/`) como orquestador; PDF Service (`src/PDF/`) como generador del documento |
| **Endpoint(s) / Punto de entrada** | `GET /agendas/{id}/pdf` (Agendas Service, expuesto al frontend) |
| **Prioridad** | Alta |
 
### 2. Actores
 
| Actor | Rol |
|---|---|
| Coordinador de agendas | Funcionario de PROCOMER que descarga el PDF para imprimir o enviar al inversor. |
| Frontend (ASP.NET Core MVC) | Capa de presentación que presenta la acción de descarga y gestiona la respuesta binaria del PDF. |
| Agendas Service | Microservicio que recibe la solicitud de descarga, obtiene los datos de la agenda de la base de datos y los envía al PDF Service para la generación. |
| PDF Service | Microservicio dedicado a la generación del documento PDF; recibe el JSON de la agenda y retorna el archivo binario en español (variante Costa Rica). |
| Azure SQL Database | Almacén del que el Agendas Service lee los datos completos de la `Agenda` y sus `Reuniones` antes de delegarlos al PDF Service. |
 
### 3. Precondiciones
 
| # | Condición |
|---|---|
| P-01 | El Agendas Service y el PDF Service están desplegados y disponibles como Azure Container Apps. |
| P-02 | Existe una agenda con el `Id` solicitado en la tabla `Agendas` (puede estar en estado `Activa` o `Anulada`). |
| P-03 | La agenda tiene al menos una `Reunion` asociada con todos sus atributos completos (resultado de CU-04). |
| P-04 | El coordinador tiene acceso al frontend y a la pantalla de detalle o listado de agendas. |
 
### 4. Flujo Principal
 
1. El coordinador localiza una agenda en el listado o en la vista de detalle (CU-05) y elige la acción de descarga del PDF.
2. El frontend construye la petición `GET /agendas/{id}/pdf` hacia el Agendas Service.
3. El Agendas Service recibe la petición en la capa API y delega al caso de uso `DescargarPdfAgendaHandler` en la capa Application.
4. La capa Application consulta el repositorio `IAgendaRepository` para recuperar el registro `Agenda` con todas las `Reunion` asociadas (incluyendo datos del participante, cargo, nombre y dirección de la oficina, idioma de la reunión y `TiempoTrasladoSiguiente`) vía EF Core 8.
5. La capa Application construye el objeto de transferencia `AgendaPdfDto` con toda la información requerida por el PDF Service.
6. El Agendas Service invoca el PDF Service mediante `IHttpClientFactory` tipado: envía una petición `POST` con el `AgendaPdfDto` en el cuerpo.
7. El PDF Service recibe el DTO en su capa API y delega al caso de uso `GenerarPdfHandler` en su capa Application.
8. La capa Application del PDF Service utiliza la biblioteca QuestPDF para construir el documento con la siguiente estructura:
   a. **Encabezado institucional** con logo y nombre completo del inversor.
   b. **Fecha de la jornada** para la cual aplica el itinerario.
   c. **Tabla de reuniones** con las columnas: hora de inicio, hora de finalización, nombre del participante, cargo, nombre de la oficina, dirección física de la oficina e idioma en que se realizará la reunión.
   d. **Indicador de tiempo de traslado** entre cada par de reuniones consecutivas (usando `TiempoTrasladoSiguiente`).
   e. **Pie de página** con numeración de página y fecha de generación del documento.
9. El PDF Service retorna el binario PDF generado al Agendas Service con `Content-Type: application/pdf`.
10. El Agendas Service recibe el binario y lo retorna al frontend con:
    - `HTTP 200 OK`.
    - `Content-Type: application/pdf`.
    - `Content-Disposition: attachment; filename="Agenda_{Fecha}_{NombreInversor}.pdf"`.
11. El navegador del coordinador descarga el archivo PDF con el nombre sugerido.
### 5. Flujos Alternos
 
#### FA-01 — Descarga de PDF de una agenda anulada
 
1. El coordinador accede al detalle de una agenda en estado `Anulada` y elige la acción de descarga.
2. El flujo es idéntico al flujo principal, dado que el Agendas Service no restringe la generación del PDF por el estado de la agenda (RN-15 establece que el PDF debe seguir disponible para trazabilidad histórica).
3. El PDF generado corresponde al itinerario original al momento de la creación de la agenda, con los datos que fueron persistidos en las tablas `Agendas` y `Reuniones`.
### 6. Flujos de Excepción
 
#### FE-01 — Agenda no encontrada
 
- **Condición:** El `Id` proporcionado no corresponde a ninguna agenda en la base de datos.
- **Procesamiento:** El repositorio no encuentra el registro y la capa Application lanza `AgendaNotFoundException`.
- **Respuesta:** `HTTP 404 Not Found` con mensaje descriptivo. El frontend muestra un mensaje de error al coordinador.
- **Compensación:** No aplica; no se invoca el PDF Service.
#### FE-02 — PDF Service no disponible
 
- **Condición:** El PDF Service no responde a la petición del Agendas Service (timeout, error de contenedor o fallo de red).
- **Procesamiento:** El `IHttpClientFactory` con política de resiliencia (Microsoft.Extensions.Http.Resilience) reintenta hasta 3 veces con backoff exponencial. Si persiste el fallo, la capa Application del Agendas Service lanza `PdfServiceNoDisponibleException`.
- **Respuesta:** `HTTP 504 Gateway Timeout` con el mensaje: `"El servicio de generación de documentos no está disponible. Por favor, intente nuevamente."`. El incidente queda registrado en el Log Stream de Azure Container Apps del Agendas Service.
- **Compensación:** No se retorna ningún archivo al frontend.
#### FE-03 — Error en la generación del PDF
 
- **Condición:** El PDF Service recibe el `AgendaPdfDto` pero QuestPDF falla durante la construcción del documento (datos incompletos, error de biblioteca).
- **Procesamiento:** El PDF Service captura la excepción en su middleware global de errores y retorna `HTTP 500` con mensaje estructurado (sin stack trace). El Agendas Service lo propaga al frontend.
- **Respuesta:** `HTTP 500 Internal Server Error` con mensaje: `"Error al generar el documento. Por favor, contacte al administrador del sistema."`.
- **Compensación:** No se retorna ningún archivo. El error queda registrado en el Log Stream de Azure Container Apps del PDF Service.
#### FE-04 — Agendas Service no disponible
 
- **Condición:** El Agendas Service no responde.
- **Procesamiento:** El frontend recibe error de conexión o `HTTP 503`.
- **Respuesta:** El frontend muestra un mensaje de indisponibilidad temporal al coordinador.
- **Compensación:** No aplica.
### 7. Postcondiciones
 
| # | Estado del sistema |
|---|---|
| PC-01 | El sistema no fue modificado; la descarga es una operación de solo lectura sobre la base de datos. |
| PC-02 | El coordinador tiene en su dispositivo el archivo PDF del itinerario en formato `Agenda_{Fecha}_{NombreInversor}.pdf`. |
| PC-03 | El documento PDF está redactado en español, variante de Costa Rica, con todos los campos definidos en la Prueba Técnica §4.4. |
 
### 8. Reglas de Negocio Aplicables
 
| ID Regla | Descripción |
|---|---|
| RN-15 | La anulación de una agenda es lógica; el registro y su PDF se conservan para trazabilidad histórica. El PDF es descargable incluso si la agenda está en estado `Anulada`. |
 
### 9. Criterio de Aceptación (Gherkin)
 
```gherkin
Feature: Generación y descarga del PDF del itinerario
 
  Scenario: Descarga exitosa del PDF de una agenda activa
    Given existe una agenda en estado "Activa" con Id conocido y al menos 2 reuniones
    When el coordinador invoca GET /agendas/{id}/pdf
    Then el Agendas Service obtiene los datos de la agenda y sus reuniones de Azure SQL
    And delega la generación al PDF Service vía IHttpClientFactory
    And el PDF Service retorna el binario del documento
    And el Agendas Service retorna HTTP 200 con Content-Type application/pdf
    And el Content-Disposition incluye el nombre "Agenda_{Fecha}_{NombreInversor}.pdf"
    And el PDF contiene encabezado institucional con nombre del inversor,
        la fecha de la jornada, tabla de reuniones con hora inicio, hora fin,
        nombre del participante, cargo, nombre de oficina, dirección física,
        idioma de la reunión, tiempos de traslado entre reuniones consecutivas,
        y pie de página con numeración y fecha de generación
 
  Scenario: PDF sigue disponible tras anulación de la agenda (RN-15)
    Given existe una agenda en estado "Anulada" con Id conocido
    When el coordinador invoca GET /agendas/{id}/pdf
    Then el Agendas Service retorna HTTP 200 con el PDF del itinerario original
    And el documento corresponde a los datos persistidos al momento de la generación
 
  Scenario: Reintentos ante indisponibilidad del PDF Service
    Given el PDF Service no responde durante la solicitud de generación
    When el Agendas Service intenta enviar el AgendaPdfDto al PDF Service
    Then reintenta hasta 3 veces con backoff exponencial (Microsoft.Extensions.Http.Resilience)
    And si el fallo persiste retorna HTTP 504 al frontend con el mensaje de reintento
    And registra el incidente en el Log Stream de Azure Container Apps
 
  Scenario: Agenda no encontrada al intentar descargar PDF
    Given no existe ninguna agenda con el Id proporcionado
    When el coordinador invoca GET /agendas/{Id_Inexistente}/pdf
    Then el Agendas Service retorna HTTP 404 con mensaje descriptivo
    And el PDF Service no es invocado
```
 
---
 
## Tabla Resumen de Reglas de Negocio por Caso de Uso
 
| ID Regla | Descripción | CU aplicables |
|---|---|---|
| RN-01 | El inversor debe tener al menos un idioma asignado. | CU-01 |
| RN-02 | La `FechaFinVisita` debe ser mayor o igual a `FechaInicioVisita`. | CU-01 |
| RN-03 | No se permite eliminar un inversor con agendas en estado `Activa`. | CU-01 |
| RN-04 | Todo participante debe tener al menos un idioma asignado. | CU-02 |
| RN-05 | Todo participante debe tener exactamente una oficina asignada. | CU-02 |
| RN-06 | No se puede eliminar una oficina con participantes activos asignados. | CU-03 |
| RN-07 | La `MatrizTraslado` debe ser simétrica: `TiempoMinutos(A→B)` = `TiempoMinutos(B→A)`; la garantía se aplica automáticamente en la capa Application del Catálogo Service. | CU-03, CU-04 |
| RN-08 | La fecha de la agenda debe estar dentro del rango `[FechaInicioVisita, FechaFinVisita]` del inversor. | CU-04 |
| RN-09 | Ninguna reunión puede iniciar antes de las 08:00 horas. | CU-04 |
| RN-10 | Ninguna reunión puede finalizar después de las 17:00 horas. | CU-04 |
| RN-11 | El bloque 12:00–13:00 está reservado para almuerzo; ninguna reunión puede solaparse con él. | CU-04 |
| RN-12 | El inversor y el participante deben compartir al menos un idioma para que la reunión sea válida. | CU-04 |
| RN-13 | El intervalo entre el fin de una reunión y el inicio de la siguiente debe ser mayor o igual al `TiempoMinutos` de traslado entre las oficinas correspondientes. | CU-04 |
| RN-14 | Un mismo participante no puede tener dos reuniones con horarios solapados en la misma agenda. | CU-04 |
| RN-15 | La anulación de una agenda es siempre lógica (soft delete); el registro y su PDF se conservan para trazabilidad histórica. | CU-05, CU-06 |
 
---
 
*Documento generado como artefacto del Gate 1 — PROCOMER-CALEND-2026. Versión 1.0 · Junio 2026.*