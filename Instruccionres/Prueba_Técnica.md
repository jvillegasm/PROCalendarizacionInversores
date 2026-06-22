# PRUEBA TÉCNICA DE DESARROLLO

**Contratación 2026XE-000001-0001700001**

**Caso: Calendarización de Inversores**

---

## 1. Contexto del Negocio

Como parte de su misión de atracción de inversión extranjera, PROCOMER recibe periódicamente a inversores internacionales que visitan Costa Rica con el objetivo de evaluar oportunidades de inversión. Estas visitas tienen una duración limitada y deben aprovecharse al máximo coordinando reuniones con funcionarios públicos, representantes institucionales y aliados estratégicos del ecosistema.

La complejidad operativa surge del hecho de que cada participante posee su propia agenda, trabaja desde una oficina física específica dentro del Gran Área Metropolitana, y maneja un conjunto distinto de idiomas. Adicionalmente, el inversor tiene preferencias claras respecto al idioma en que desea recibir su itinerario impreso.

Actualmente, esta coordinación se gestiona de manera manual, lo que genera errores frecuentes, traslapes de horarios y traslados imposibles de cumplir. Se requiere una solución automatizada que resuelva estas restricciones de forma eficiente y produzca el itinerario formal para entregar al visitante.

---

## 2. Problema a Resolver

El equipo deberá construir un sistema que automatice la elaboración de agendas de visita para inversores extranjeros. La solución debe ser capaz de:

- Mantener actualizado el catálogo de inversores con sus datos personales, idiomas que manejan y fechas de visita.
- Administrar la base de funcionarios y aliados que pueden recibir inversores, incluyendo su oficina de trabajo, idiomas y franjas horarias disponibles.
- Calcular automáticamente una agenda viable que respete las restricciones de tiempo, ubicación e idioma de todos los involucrados.
- Generar un documento PDF formal con el itinerario, en español (variante de Costa Rica).

La calidad del sistema se medirá tanto por la corrección del algoritmo de optimización como por la limpieza de la implementación distribuida.

---

## 3. Modalidad de Ejecución

La prueba se ejecuta bajo las siguientes condiciones:

- **Conformación del equipo:** dos personas con perfil Senior, idealmente un Desarrollador y un Analista, trabajando en conjunto.
- **Duración total:** seis (6) horas efectivas reloj a partir del inicio formal de la prueba.
- **Asistencia de IA:** se autoriza expresamente el uso de copilots de programación e IA generativa (GitHub Copilot, Claude, ChatGPT, Cursor u otros). El equipo es responsable de validar y comprender todo el código generado, ya que durante la presentación final se solicitará justificación técnica de las decisiones tomadas.
- **Recursos provistos por PROCOMER:** suscripción de Azure con Resource Group dedicado, credenciales de acceso.

---

## 4. Alcance Funcional

### 4.1 Módulo de Inversores

Este módulo concentra la información de las personas que visitan el país en condición de inversores potenciales. Debe ofrecer las operaciones típicas de mantenimiento (creación, consulta, actualización y eliminación) tanto a través del frontend como mediante endpoints REST que serán invocados por el módulo de Agendas durante el proceso de scheduling.

Información que se debe capturar por cada inversor:

- Nombre completo del visitante.
- Empresa que representa y país desde el cual viaja.
- Conjunto de idiomas que el inversor maneja con fluidez (al menos uno entre español, inglés).
- Período de la visita expresado como fecha de inicio y fecha de cierre.
- Lugar de hospedaje o punto inicial desde donde partirá cada jornada.

Validaciones aplicables sobre los datos del inversor:

- Es obligatorio asignar al menos un idioma al inversor; sin esta información el algoritmo de scheduling no puede operar.
- La fecha de cierre de la visita no puede ser anterior a la fecha de inicio.
- No se permite eliminar un inversor que tenga agendas activas asociadas; primero deben anularse esas agendas.

---

### 4.2 Módulo de Participantes, Oficinas y Traslados

Este módulo es responsable de tres entidades estrechamente relacionadas: los participantes (personas que se reunirán con los inversores), las oficinas (ubicaciones físicas donde ocurren las reuniones) y la matriz de tiempos de desplazamiento entre dichas oficinas.

Datos que se manejan sobre cada participante:

- Nombre completo y cargo o institución a la que pertenece.
- Oficina donde habitualmente atiende reuniones.
- Idiomas que domina (uno o varios).
- Bloques horarios disponibles para reuniones, organizados por fecha.
- Indicador de estado (activo o inactivo) para excluirlo del scheduling sin perder el registro histórico.

Datos que se manejan sobre cada oficina:

- Nombre identificable de la oficina.
- Dirección física completa.
- Coordenadas geográficas, opcionales.

Datos que conforman la matriz de traslados:

- Oficina de origen del traslado.
- Oficina de destino.
- Tiempo estimado de desplazamiento expresado en minutos.

Reglas que deben cumplirse en este módulo:

- Todo participante debe tener al menos un idioma y una oficina asignados.
- No se puede dar de baja una oficina si hay participantes activos asignados a ella.
- La matriz de traslados debe mantenerse simétrica: el tiempo entre dos oficinas debe ser igual en ambos sentidos.

---

### 4.3 Módulo de Agendas (Núcleo del Sistema)

Este es el componente más relevante de la prueba y donde se concentra la mayor complejidad técnica. Su función es recibir una solicitud de coordinación y devolver una agenda completa, factible y optimizada, lista para ser entregada al inversor.

**Información que el coordinador proporciona al solicitar una agenda:**

- Selección del inversor desde el catálogo.
- Conjunto de participantes que se desean incluir como candidatos.
- Día específico de la agenda, que debe estar comprendido dentro de la ventana de visita del inversor.
- Duración estándar para cada reunión, expresada en minutos.
- Cantidad de reuniones que se desean lograr como meta.

Al seleccionar al inversor, el frontend debe mostrar automáticamente sus datos relevantes: empresa que representa, idiomas que maneja, ventana de visita disponible. Esto permite al coordinador validar que la información esté actualizada antes de iniciar el proceso.

**Comportamiento esperado del algoritmo de scheduling:**

- Filtrar de la lista de candidatos aquellos que no comparten ningún idioma con el inversor, ya que esas reuniones no podrían realizarse.
- Recorrer la disponibilidad horaria de cada participante restante dentro de la ventana laboral establecida (de 08:00 a 17:00 horas).
- Excluir automáticamente el período de almuerzo (12:00 a 13:00 horas), durante el cual no pueden programarse reuniones.
- Verificar que entre dos reuniones consecutivas exista tiempo suficiente para que el inversor se traslade entre oficinas, consultando la matriz precargada.
- Armar una secuencia de reuniones que maximice la cantidad de reuniones realizadas dentro del día (priorizando alcanzar la meta solicitada) y, ante igualdad de reuniones, minimice el tiempo total de traslados, sin generar conflictos de tiempo o ubicación.
- En caso de no encontrar una combinación viable, retornar un mensaje explicativo claro que indique la causa: ausencia de idioma compartido, falta de disponibilidad en la fecha, traslado imposible entre oficinas consecutivas, entre otros.

**Operaciones disponibles sobre una agenda ya generada:**

- **Anulación:** se realiza mediante eliminación lógica, marcando la agenda como "Anulada". El registro y su PDF se conservan para trazabilidad histórica.

**Reglas de negocio que el sistema debe garantizar:**

- Ninguna agenda puede programarse fuera del rango de visita declarado por el inversor.
- Ninguna reunión puede iniciar antes de las 08:00 ni finalizar después de las 17:00.
- El bloque de 12:00 a 13:00 está reservado para almuerzo y no admite reuniones.
- Para cada reunión, el inversor y el participante deben compartir al menos un idioma.
- El intervalo entre dos reuniones consecutivas debe ser mayor o igual al tiempo de traslado entre las oficinas correspondientes.
- Un mismo participante no puede aparecer en dos reuniones que se solapen en el tiempo.

---

### 4.4 Módulo de Generación de PDF

Una vez confirmada la agenda, el sistema debe ser capaz de producir un documento PDF profesional listo para ser entregado al inversor, ya sea de forma impresa o por correo electrónico. La generación es responsabilidad de un microservicio dedicado que recibe la agenda y devuelve el archivo.

El documento generado debe contener:

- Encabezado institucional con un logo y el nombre completo del inversor.
- Fecha de la jornada para la cual aplica la agenda.
- Tabla detallada de cada reunión: hora de inicio, hora de finalización, nombre del participante, cargo, oficina, dirección física e idioma en que se realizará.
- Indicador del tiempo estimado de traslado entre cada par de reuniones consecutivas, de modo que el visitante sepa cuánto debe anticipar su salida.
- Pie de página con numeración y fecha de generación del documento.

El documento se genera únicamente en español (variante de Costa Rica).

---

## 5. Lineamientos Técnicos

### 5.1 Estructura de la Solución

La arquitectura debe construirse explícitamente como un conjunto de microservicios desacoplados. Una arquitectura monolítica no es aceptable, independientemente de qué tan bien organizada esté internamente.

La solución se compone de tres microservicios backend y un frontend, descritos a continuación:

**Microservicio de Catálogo**

Centraliza la gestión de los datos maestros del sistema: inversores, participantes, oficinas y matriz de traslados. Debe implementarse aplicando los principios de Clean Architecture, separando claramente las capas de Dominio, Aplicación e Infraestructura.

**Microservicio de Agendas**

Es el corazón de la solución. Contiene la implementación del algoritmo de scheduling y orquesta la comunicación con los demás microservicios. Para hacer estas llamadas debe utilizar HttpClient tipado, complementado con políticas de resiliencia (reintentos, circuit breaker, timeout) implementadas con Polly o una librería equivalente. La arquitectura interna también sigue el patrón Clean Architecture.

**Microservicio de PDF**

Recibe los datos de una agenda confirmada y produce el documento PDF en el idioma indicado. Se mantiene la estructura de Clean Architecture.

**Frontend (Capa de Presentación)**

Puede desarrollarse con ASP.NET MVC. Es el punto de interacción del usuario coordinador y consume directamente cada microservicio a través de su URL pública en Azure Container Apps. Debe implementar como mínimo:

- Pantalla de mantenimiento (CRUD) de inversores.
- Pantalla de mantenimiento (CRUD) de participantes y oficinas.
- Pantalla principal de generación de agendas, con visualización dinámica de los datos del inversor al seleccionarlo.
- Vista de listado y detalle de agendas existentes, con acción de descarga del PDF.

---

### 5.2 Endpoints REST Esperados

El microservicio de Agendas debe exponer al menos los siguientes endpoints:

- `POST /agendas/generar` — Solicita la generación automática de una agenda para un inversor en una fecha determinada.
- `GET /agendas` — Lista las agendas existentes, con filtros opcionales por inversor, fecha y estado.
- `GET /agendas/{id}` — Retorna el detalle completo de una agenda específica.
- `DELETE /agendas/{id}` — Realiza la anulación lógica de la agenda.
- `GET /agendas/{id}/pdf` — Devuelve el archivo PDF correspondiente a la agenda en español.

El microservicio de catálogos debe exponer los endpoints CRUD habituales que sustenten las pantallas del frontend.

Toda la API debe estar documentada mediante Swagger, OpenAPI o Scalar, accesible públicamente para que pueda ser explorada durante la revisión de la prueba.

---

### 5.3 Despliegue en Azure

La solución no se evaluará en un entorno local. PROCOMER provisiona una suscripción de Azure y un Resource Group exclusivo para esta prueba, dentro del cual el equipo deberá realizar todos los despliegues necesarios.

Requisitos mínimos del ambiente desplegado:

- Cada microservicio se publica como un Azure Container App individual, con su propia configuración de escalado e ingress externo.
- Las imágenes Docker deben construirse y publicarse en Azure Container Registry antes de iniciar el despliegue.
- La persistencia se maneja con Azure SQL Database, instanciada dentro del mismo Resource Group.
- Al final de la prueba, el equipo entrega la URL pública del frontend funcionando contra los microservicios desplegados, junto con las URLs de cada API para verificación.

---

### 5.4 Pruebas Unitarias

El microservicio de Agendas debe acompañarse de un proyecto independiente de pruebas unitarias que ejercite específicamente las reglas del algoritmo de scheduling. Se exige un mínimo de 5 pruebas, distribuidas equitativamente entre escenarios exitosos y escenarios de validación.

Como referencia, algunos casos que el equipo puede implementar:

**Escenarios positivos (3):**

- Cálculo correcto del tiempo de traslado entre dos oficinas usando la matriz precargada.
- Generación de agenda dentro de la ventana de visita del inversor sin desbordamiento.
- Anulación exitosa de una agenda.

**Escenarios negativos (2):**

- Rechazo de generación cuando ningún participante comparte idioma con el inversor.
- Rechazo de una agenda fuera del rango de fechas declaradas por el inversor.

---

## 6. Entregables Esperados

Al cierre de la prueba, el equipo debe entregar el siguiente conjunto de artefactos:

| Tema | Puntaje | Aspectos por calificar | Detalle según la prueba |
|---|---|---|---|
| Análisis del Requerimiento (Interpretación correcta del problema e identificación de casos de uso) | 10 | Presenta diagrama completo del problema (paso a paso del proceso) | Diagrama de arquitectura mostrando los tres microservicios, su comunicación, el frontend, la base de datos y los servicios de Azure utilizados. |
|  |  | Genera los casos de uso del problema | Documentación de los casos de uso principales del sistema. |
| Planificación de la solución y estrategia de desarrollo | 10 | Elaboración de prototipo y aplicación en producto finalizado | Prototipo o mockups de las pantallas implementadas. |
| Capa de presentación: compuesta por archivos ASP (MVC), JavaScript, AJAX y jQuery, Angular | 10 | No utilización de otras tecnologías | Repositorio con el código fuente completo, incluyendo proyectos de microservicios, frontend, tests y scripts de base de datos. Solución desplegada y funcionando en Azure Container Apps. URL pública del frontend, accesible para que el panel evaluador realice las pruebas. Ejemplos de PDF generados por el sistema. |
|  |  | Cumple con todos los campos indicados en el requerimiento |  |
|  |  | Cumple con las validaciones indicadas en el requerimiento |  |
| Microservicios (Container Apps) | 50 | Cumple con la arquitectura Clean (Aplicación, dominio, infraestructura, presentación) |  |
|  |  | Realiza correctamente los cálculos indicados en el requerimiento |  |
|  |  | Expone de forma correcta los APIs REST |  |
|  |  | Acceso a datos SQL Server |  |
|  |  | Utilización de LINQ / Dapper, EF |  |
| Buenas prácticas en desarrollo de Software | 5 | Realización de unit test en el proyecto |  |
|  |  | Código limpio, documentado y manejo de errores |  |
| Eficiencia y Gestión del Tiempo | 10 | Entrega < 4 h = 10 |  |
|  |  | Entrega entre 4 y 6 h = 5 |  |
|  |  | Entrega > 6 h = 2.5 |  |
| General | 5 | Cumple con todos los requerimientos indicados en la prueba técnica | URL pública del frontend, accesible para que el panel evaluador realice las pruebas. |

---

## 7. Disposiciones Finales

El trabajo debe ser elaborado exclusivamente por los dos integrantes oficialmente registrados del equipo. Se acepta el uso de IA como herramienta de productividad, pero ante cualquier indicio de colaboración externa de terceros (mensajes, llamadas, código copiado de fuentes no documentadas) la prueba será anulada sin posibilidad de apelación.

Durante la presentación final, el panel evaluador se reserva el derecho de solicitar al equipo la explicación detallada de cualquier porción de código, decisión arquitectónica o resultado del algoritmo. La incapacidad de justificar técnicamente lo entregado afectará directamente la calificación obtenida.
