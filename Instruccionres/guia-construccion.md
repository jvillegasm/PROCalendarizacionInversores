# Guía de Construcción — Prototipo UI

**Proyecto:** Sistema de Calendarización de Inversores · PROCOMER
**ID:** PROCOMER-CALEND-2026 · **Versión:** 1.0 · Junio 2026
**Fuente de verdad:** `SPEC_Calendarizacion_Inversores.md` (Gate 1)
**Enfoque:** Bootstrap 5.3 First (90% Bootstrap · 10% CSS de marca)

> Esta guía permite a otra IA o desarrollador reconstruir `prototype.html` desde cero sin desviarse del diseño. Toda terminología (entidades, reglas RN-XX, criterios AC-XX) es consistente con el SPEC.

---

## 1. Descripción general del diseño

El prototipo es una **aplicación de una sola página (SPA simulada)** para el coordinador de PROCOMER. Resuelve el flujo completo de la prueba: mantener datos maestros (inversores, participantes, oficinas, matriz de traslados), generar la agenda diaria optimizada y consultar/anular agendas con su PDF.

Principios rectores:

- **Bootstrap puro primero.** Antes de cualquier componente se verifica si Bootstrap lo resuelve; el CSS de marca solo aporta color institucional, radios, sombras suaves y la línea de tiempo.
- **Orientación permanente.** Navbar fija + breadcrumb dinámico: el usuario siempre sabe dónde está.
- **Máximo 3 clics** a cualquier función crítica (generar agenda está a 1 clic desde el navbar y desde Inicio).
- **Un solo color de marca** (verde institucional) sobre la paleta Bootstrap.
- **Todos los estados de UI** representados: vacío, carga, éxito, error.

La pantalla de **Generar agenda** es el centro del producto y por eso ocupa el layout más rico (formulario + panel dinámico del inversor + área de resultado).

---

## 2. Wireframe y estructura de cada pantalla

Layout global (todas las pantallas):

```
┌───────────────────────────────────────────────┐
│ NAVBAR fija: marca · Inicio Inversores ...      │
├───────────────────────────────────────────────┤
│ Breadcrumb (Inicio / <página actual>)           │
│                                                 │
│ [ contenido de la página activa ]               │
│                                                 │
│ Footer institucional                            │
└───────────────────────────────────────────────┘
```

**2.1 Inicio (`#page-inicio`)**
```
H1 + botón "Generar agenda"
[stat][stat][stat][stat]   ← 4 tarjetas de conteo
[ Paso 1: datos maestros ] [ Paso 2: agenda ]
```

**2.2 Inversores (`#page-inversores`)**
```
H1 · [buscar] [ + Nuevo ]
┌ Card ────────────────────────────┐
│ Tabla: Nombre|Empresa|País|Idiomas|Visita|Acciones │
│ (empty-state oculto si hay resultados)             │
└────────────────────────────────────────────────────┘
Modal "Inversor" con formulario validado.
```

**2.3 Participantes y oficinas (`#page-participantes`)**
```
H1
[Pills: Participantes | Oficinas | Matriz de traslados]
 ├ Participantes → tabla + estado activo/inactivo
 ├ Oficinas → tabla + "participantes activos" (bloquea borrado)
 └ Matriz → tabla simétrica NxN en minutos + aviso RN-07
```

**2.4 Generar agenda (`#page-agenda`)** — layout de 2 columnas
```
┌ col-lg-5 ───────────┐ ┌ col-lg-7 ───────────────┐
│ Form:               │ │ Panel inversor (dinámico)│
│  - Inversor (select)│ │  empty → datos al elegir │
│  - Candidatos (chks)│ ├──────────────────────────┤
│  - Fecha (rango)    │ │ Área de RESULTADO:       │
│  - Duración / Meta  │ │  skeleton → éxito/timeline│
│  [ Generar agenda ] │ │            o alerta error │
└─────────────────────┘ └──────────────────────────┘
```

**2.5 Agendas (`#page-agendas`)**
```
H1 · [filtro estado] [ + Nueva ]
Tabla: ID|Inversor|Fecha|Reuniones|Estado|Acciones
Acciones: Detalle (offcanvas) · PDF · Anular (modal)
```

---

## 3. Mapa de flujos UX (pantallas y transiciones)

```
                 ┌──────────┐
                 │  INICIO  │
                 └────┬─────┘
        ┌─────────────┼──────────────┬───────────────┐
        ▼             ▼              ▼               ▼
  INVERSORES   PARTICIPANTES   GENERAR AGENDA     AGENDAS
   │  ▲         /OFICINAS          │                 │
   │  │ (modal)   │ (pills/modal)  │ select inversor │ filtro estado
   │  │           │                ▼                 │
   └──┘           └──┐        panel dinámico         ▼
  CRUD + validación  │        + candidatos      DETALLE (offcanvas)
                     │        + fecha rango     PDF · ANULAR (modal)
                     │             │
                     │             ▼
                     │     [Generar] → loading → éxito (timeline) ┐
                     │                         → error (alerta)   │
                     └─────────────────────────────────→ va a ───┘ AGENDAS
```

Transiciones clave:
- **Inicio → cualquier sección:** navbar o tarjetas de paso.
- **Seleccionar inversor (Generar agenda):** dispara el panel dinámico y restringe el campo fecha al rango de visita (RN-08).
- **Generar:** `loading (skeleton)` → resultado. Si el inversor solo habla un idioma sin candidato compatible (caso demo: Hans Müller) → estado de error idioma incompatible (RN-12 / HTTP 422).
- **Generar exitoso → toast → puede ir a Agendas.**

---

## 4. Componentes Bootstrap utilizados

| Componente | Uso en el prototipo |
|---|---|
| Navbar (dark, fixed-top, collapse) | Navegación principal responsive |
| Breadcrumb | Orientación de ubicación |
| Container / Row / Col | Grid de todo el layout |
| Card | Tablas, formularios, tarjetas de estadística y resultado |
| Table (`table-hover`, `table-responsive`, `table-bordered`) | Listados y matriz de traslados |
| Nav Pills + Tab panes | Sub-navegación Participantes/Oficinas/Matriz |
| Modal | Form inversor, form participante, confirmar eliminar, anular agenda |
| Offcanvas | Detalle de agenda (timeline) |
| Toast | Confirmaciones y errores (helper `showToast`) |
| Form Floating + Input Group | Campos de formulario y buscadores |
| Native validation (`needs-validation`, `was-validated`, `invalid-feedback`) | Validaciones de formularios |
| Badge (`text-bg-*-subtle`) | Idiomas, estados (Activa/Anulada, Activo/Inactivo) |
| Button / Spinner | Acciones y estado de carga del botón Generar |
| Alert | Aviso de simetría (RN-07), bloqueo de borrado, error de scheduling |
| Form Check (checkbox) | Idiomas y selección de candidatos |
| Bootstrap Icons | Iconografía exclusiva (sin SVG custom) |

---

## 5. CSS adicional (< 150 líneas documentadas)

El bloque `<style>` está **por debajo de 150 líneas** y se limita a lo permitido por el template (color de marca, radios, sombras suaves, espaciados, hover simple). Resumen de lo que define:

| Grupo | Qué hace | Justificación |
|---|---|---|
| `:root` (variables) | `--brand`, `--brand-dark`, `--brand-soft`, `--radius` | Un único color de marca centralizado |
| `body` | `padding-top` por navbar fija, fondo gris muy claro | Compensa `fixed-top` |
| `.bg-brand/.text-brand/.btn-brand/.btn-outline-brand` | Aplica el verde institucional a fondos, texto y botones | Botón de marca sobre paleta Bootstrap |
| `.nav-pills .nav-link.active` | Pill activa en color de marca | Consistencia visual |
| `.card` | Borde 0 + radio + sombra sutil | Sombra suave permitida |
| `.stat-card .icon` | Cuadro de icono en `--brand-soft` | Tarjetas de conteo en Inicio |
| `.table thead th` | Encabezados en mayúsculas y gris | Jerarquía de tabla |
| `.page / .page.active` + `@keyframes fade` | Muestra/oculta páginas con transición leve | SPA simulada |
| `.lang-chip` | Tamaño de badge de idioma | Legibilidad |
| `.timeline-line / .timeline-dot` | Línea de tiempo de la agenda | Señal visual del itinerario |
| `.skeleton` + `@keyframes sk` | Skeleton de carga | Estado loading percibido |
| `.empty-state` | Estilo de pantallas vacías | Estado vacío |
| `.min-touch` | `min 44x44px` | Accesibilidad táctil (template) |
| `footer` | Pie discreto | — |
| `@media (prefers-reduced-motion)` | Desactiva animaciones | Accesibilidad |

**Prohibiciones respetadas:** sin gradientes avanzados, glassmorphism, neumorphism, clip-path, SVG custom ni animaciones complejas.

---

## 6. Guía de colores y tipografía

**Colores** — paleta Bootstrap + un color de marca:

| Token | Hex | Uso |
|---|---|---|
| `--brand` | `#0c7c59` | Navbar, botones primarios, acentos |
| `--brand-dark` | `#095c42` | Hover de botones de marca |
| `--brand-soft` | `#e7f4ef` | Fondos de icono, línea de tiempo |
| Fondo app | `#f6f8f7` | Lienzo general |
| Estados | `success` / `secondary` (Bootstrap subtle) | Activa/Anulada, Activo/Inactivo |
| Alertas | `warning` (error de scheduling), `danger` (borrado), `info` (simetría) | Semántica nativa |

**Tipografía:** se usa la **fuente del sistema de Bootstrap 5.3** (`system-ui` stack nativo) — sin fuentes externas, para mantener el prototipo ligero y 100% offline-CDN. Escala: `h1.h3/h4`, `h2.h5/h6`, cuerpo `1rem`, apoyo `.small` (`.78–.82rem`). Pesos: títulos `600–700`, cuerpo `400`.

---

## 7. Comportamiento responsive

Mobile First sobre el grid de Bootstrap:

- **Navbar** colapsa en hamburguesa (`navbar-toggler`) bajo `lg`. El toggler cumple área táctil 44px.
- **Inicio:** stats `col-6` en móvil → `col-lg-3` en escritorio; tarjetas de paso `col-md-6`.
- **Generar agenda:** columnas `col-lg-5 / col-lg-7` se apilan a ancho completo en móvil y tablet (el formulario queda arriba, el resultado debajo).
- **Tablas:** envueltas en `.table-responsive` → scroll horizontal en pantallas estrechas sin romper el layout.
- **Offcanvas de detalle:** ancho fijo 480px en escritorio; en móvil Bootstrap lo adapta al viewport.
- **Botones de acción** mantienen `min-touch` (44x44px) en móvil.

---

## 8. Estados de UI por componente (vacío, carga, éxito, error)

| Componente | Vacío | Carga | Éxito | Error |
|---|---|---|---|---|
| Tabla Inversores | `empty-state` "No hay inversores que coincidan" | — | Filas con datos | — |
| Listado de Agendas | `empty-state` "No hay agendas que coincidan con los filtros" | Skeleton de filas al filtrar/reintentar (GET /agendas) | Filas filtradas por inversor+fecha+estado | Panel "servicio no disponible (HTTP 503)" con botón Reintentar |
| Detalle de agenda (offcanvas) | — | Skeleton mientras carga (GET /agendas/{id}) | Itinerario completo: inversor, empresa, fecha, estado, reuniones con cargo, oficina, dirección física, idioma y traslado siguiente | — |
| Anular agenda | — | — | Toast "Agenda A-00X anulada · [timestamp]. PDF disponible"; fila pasa a Anulada | Botón Anular deshabilitado con tooltip "ya se encuentra anulada (HTTP 409)" |
| Panel inversor (agenda) | `empty-state` "Seleccione un inversor" | — | Datos del inversor + ventana de visita | — |
| Botón Generar | — | Spinner + texto "Calculando…" + disabled | Vuelve a normal | Vuelve a normal |
| Área de resultado | Vacío inicial | Skeleton (3 barras) | Card con timeline "3 de 3" + toast verde | Alert warning "IDIOMA_INCOMPATIBLE / HTTP 422 / RN-12" + toast rojo |
| Eliminar registro | — | — | Toast "Registro eliminado" | Alert de bloqueo "tiene registros activos (RN-03/RN-06)" + botón deshabilitado |
| Acción PDF | — | Toast info "Generando Agenda_{Fecha}_{Inversor}.pdf (es-CR)…" | Toast "PDF descargado: Agenda_…pdf" (disponible aun anulada, RN-15) | Toast "Servicio de PDF no disponible tras 3 reintentos (HTTP 504)" |

Cobertura de criterios del SPEC reflejada en estados: **AC-04** (generación viable), agenda parcial "2 / 4" (fila A-002), **AC-05** (rechazos), **AC-07** (anulación lógica conserva PDF).

---

## 9. Validaciones de formularios y mensajes de error

Validación nativa de Bootstrap (`needs-validation` + `was-validated`) más reglas de negocio del SPEC. Mensajes en español:

| Campo / Regla | Mensaje | Regla SPEC |
|---|---|---|
| Inversor sin idioma | "El inversor debe tener al menos un idioma asignado." | RN-01 / AC-01 |
| Fecha cierre < inicio | "La fecha de cierre no puede ser anterior a la de inicio." | RN-02 / AC-01 |
| Eliminar inversor con agendas activas | Bloqueado: "No es posible eliminar… tiene registros activos." | RN-03 / AC-01 |
| Participante sin idioma (uno o varios) | "El participante debe tener al menos un idioma asignado." | RN-04 |
| Participante sin oficina | "El participante debe tener una oficina asignada (RN-05)." | RN-05 |
| Bloques de disponibilidad por fecha | Fila dinámica (fecha + hora inicio + hora fin) con requeridos; "Agregar/Quitar bloque"; empty-state si no hay ninguno | Prueba Técnica §4.2 |
| Eliminar oficina con participantes activos | Bloqueo en modal de confirmación | RN-06 / AC-03 |
| Matriz de traslados | Aviso de simetría automática A↔B | RN-07 / AC-03 |
| Fecha de agenda fuera de rango | `min`/`max` del input restringidos + hint "RN-08" | RN-08 / AC-05 |
| Sin idioma compartido | Alerta "IDIOMA_INCOMPATIBLE" (HTTP 422) | RN-12 / AC-05 |
| Selección de inversor requerida | "Seleccione un inversor para continuar." | — |

La validación de fechas usa `setCustomValidity` para encadenar la regla cruzada (cierre ≥ inicio).

---

## 10. Reglas de interacción y microinteracciones permitidas

- **Navegación SPA:** `go(page)` muestra una sola `.page`, sincroniza navbar activo y breadcrumb, y hace scroll suave al tope.
- **Transición de página:** `@keyframes fade` (0.18s) — sutil, desactivada con `prefers-reduced-motion`.
- **Botón Generar:** spinner + texto de progreso + `disabled` durante ~1.4s simulando la llamada al motor (latencia p95 ≤ 5s del SPEC).
- **Skeleton screens** mientras "carga" el resultado (rendimiento percibido > 300ms).
- **Toasts** auto-cierre a 3.2s, esquina inferior derecha, con icono semántico.
- **Hover simple** en botones de marca (cambio a `--brand-dark`). Sin animaciones complejas.
- **Confirmación obligatoria** vía modal para acciones destructivas (eliminar, anular).

---

## 11. Componentes reutilizables

| Componente | Definición | Reutilización |
|---|---|---|
| `showToast(msg, type)` | Genera y muestra un toast (`success`/`error`/`info`) | Todas las confirmaciones y errores |
| `go(page)` | Router SPA | Navbar, tarjetas, botones "Nueva/Generar" |
| `.empty-state` | Bloque de pantalla vacía con icono + texto | Tablas y panel inversor |
| `.timeline-line` + `.timeline-dot` | Itinerario cronológico | Resultado de agenda y offcanvas de detalle |
| `skeletonResultado()` | Bloque de carga | Estado loading del resultado |
| Badge de idioma (`.lang-chip`) | Chip de idioma | Inversores, participantes, reuniones |
| Modal de confirmación (`#modalEliminar`) | Borrado con bloqueo de negocio | Inversores y oficinas |
| Bloques de disponibilidad (`agregarBloque`/`eliminarBloque`) | Filas dinámicas fecha+inicio+fin con empty-state | Modal de participante (Prueba Técnica §4.2) |
| `.stat-card` | Tarjeta de conteo con icono | Dashboard de Inicio |

---

## 12. Convenciones de nombres y estructura del proyecto

**IDs de página:** `page-<seccion>` (`page-inicio`, `page-inversores`, …).
**Funciones JS:** verbos en camelCase (`generarAgenda`, `guardarInversor`, `setEliminar`, `filterTable`, `verDetalle`, `descargarPdf`).
**Clases de marca:** prefijo semántico (`btn-brand`, `text-brand`, `bg-brand`, `min-touch`, `lang-chip`).
**Entidades y reglas:** nombres idénticos al SPEC (Inversor, Participante, Oficina, MatrizTraslado, Agenda, Reunion; RN-01…RN-15; AC-01…AC-09).

Estructura del prototipo (entregable de UI, no de implementación):

```
prototype.html        ← todo en un archivo (HTML + <style> + <script>)
guia-construccion.md   ← este documento
```

Al pasar al frontend real (fuera del prototipo) la implementación objetivo es **ASP.NET MVC + jQuery/AJAX** consumiendo los microservicios por URL pública (SPEC §5.1). Pantallas → vistas MVC:
`Inversores/Index`, `Participantes/Index`, `Agendas/Generar`, `Agendas/Index` + `Agendas/Detalle`.

---

## 13. Checklist de implementación para otra IA

- [ ] Incluir Bootstrap 5.3 CSS+JS bundle y Bootstrap Icons por CDN (sin dependencias locales).
- [ ] Mantener el CSS de marca por debajo de 150 líneas y solo para color/radios/sombras/espaciado/hover.
- [ ] Navbar fija con las 5 secciones + breadcrumb sincronizado vía `go(page)`.
- [ ] **Inversores:** tabla + modal con validación RN-01 (≥1 idioma) y RN-02 (fechas), borrado bloqueado RN-03, búsqueda con empty-state.
- [ ] **Participantes:** modal con nombre, cargo/institución, oficina (RN-05), **idiomas que domina — uno o varios (RN-04)** y **bloques de disponibilidad por fecha** (fecha + hora inicio + hora fin, agregables/eliminables, con empty-state) según Prueba Técnica §4.2; estado Activo/Inactivo.
- [ ] **Oficinas:** modal propio (nombre, dirección, coordenadas opcionales); tabla muestra "participantes activos" y bloquea borrado (RN-06); matriz simétrica con aviso RN-07.
- [ ] **Generar agenda:** al seleccionar inversor, poblar panel dinámico (empresa, idiomas, ventana) y restringir el input fecha al rango (RN-08).
- [ ] Botón Generar con estado loading (spinner + disabled) y skeleton en el resultado.
- [ ] Resultado exitoso = card con timeline 08:00–17:00, almuerzo 12:00–13:00 excluido (RN-09/10/11), tiempos de traslado entre reuniones (RN-13).
- [ ] Caso de error de scheduling = alerta con código (`IDIOMA_INCOMPATIBLE`, HTTP 422, RN-12) + toast rojo.
- [ ] Mostrar agenda parcial ("2 / 4") cuando la meta no se alcanza.
- [ ] **Agendas (CU-05/CU-06):** filtros combinables por inversor, fecha y estado con estado de carga (skeleton) y panel de error de servicio (HTTP 503); listado con Id, inversor, fecha, estado y cantidad de reuniones.
- [ ] **Detalle de agenda:** offcanvas dinámico por `id` (GET /agendas/{id}) con inversor, empresa, fecha, estado y, por reunión, hora inicio–fin, participante, cargo, oficina, **dirección física**, **idioma de la reunión** y **tiempo de traslado siguiente**; acciones de PDF y Anular (esta solo si Activa).
- [ ] **Anulación lógica (RN-15):** solo agendas Activa; al confirmar, la fila pasa a Anulada, se registra fecha/hora, se conserva el PDF y el botón Anular queda deshabilitado con tooltip de HTTP 409 (FE-02); el detalle abierto se refresca.
- [ ] **PDF (CU-06):** nombre `Agenda_{Fecha}_{NombreInversor}.pdf`, descargable incluso si la agenda está Anulada (RN-15); contemplar la ruta de error HTTP 504 (PDF Service no disponible, 3 reintentos con backoff).
- [ ] Todos los estados de UI presentes: vacío, carga, éxito, error.
- [ ] Accesibilidad: labels visibles, contraste AA, área táctil 44px, foco visible, `prefers-reduced-motion` respetado.
- [ ] Acciones destructivas siempre con confirmación modal.
- [ ] Terminología 100% consistente con el SPEC (entidades, RN-XX, AC-XX).

---

*Documento generado como artefacto de UI del Gate 1 — PROCOMER-CALEND-2026. Versión 1.0 · Junio 2026.*
