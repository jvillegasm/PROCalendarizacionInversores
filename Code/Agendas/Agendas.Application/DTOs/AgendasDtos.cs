namespace Agendas.Application.DTOs;

// ─── DTOs espejo del Catálogo Service ─────────────────────────────────────────

/// <summary>Datos del inversor obtenidos del Catálogo Service.</summary>
public record InversorCatalogoDto(
    Guid Id,
    string NombreCompleto,
    string Empresa,
    string PaisOrigen,
    DateTime FechaInicioVisita,
    DateTime FechaFinVisita,
    string LugarHospedaje,
    IEnumerable<IdiomaCatalogoDto> Idiomas);

/// <summary>Datos de un idioma del catálogo.</summary>
public record IdiomaCatalogoDto(Guid Id, string Nombre, string Codigo);

/// <summary>Datos de un participante activo obtenido del Catálogo Service.</summary>
public record ParticipanteCatalogoDto(
    Guid Id,
    string NombreCompleto,
    string Cargo,
    Guid OficinaId,
    string OficinaNombre,
    bool Activo,
    IEnumerable<IdiomaCatalogoDto> Idiomas,
    IEnumerable<DisponibilidadCatalogoDto> Disponibilidades);

/// <summary>Bloque de disponibilidad horaria de un participante.</summary>
public record DisponibilidadCatalogoDto(Guid Id, DateTime Fecha, TimeSpan HoraInicio, TimeSpan HoraFin);

/// <summary>Datos de una oficina del Catálogo Service.</summary>
public record OficinaCatalogoDto(Guid Id, string Nombre, string DireccionFisica, string? Coordenadas);

/// <summary>Par de traslado de la matriz.</summary>
public record MatrizTrasladoCatalogoDto(
    Guid Id,
    Guid OficinaOrigenId,
    string OficinaOrigenNombre,
    Guid OficinaDestinoId,
    string OficinaDestinoNombre,
    int TiempoMinutos);

// ─── DTOs de Request / Response de Agendas ────────────────────────────────────

/// <summary>
/// Solicitud de generación automática de una agenda (POST /agendas/generar).
/// </summary>
public record GenerarAgendaRequest(
    Guid InversorId,
    IEnumerable<Guid> CandidatoIds,
    DateTime Fecha,
    int DuracionMinutos,
    int MetaReuniones);

/// <summary>DTO de respuesta con el resultado de la agenda generada.</summary>
public record AgendaDto(
    Guid Id,
    Guid InversorId,
    string InversorNombre,
    string InversorEmpresa,
    DateTime Fecha,
    string Estado,
    DateTime FechaGeneracion,
    DateTime? FechaAnulacion,
    int ReunionesLogradas,
    int MetaSolicitada,
    bool Completa,
    IEnumerable<ReunionDto> Reuniones);

/// <summary>DTO de respuesta para una reunión dentro de la agenda.</summary>
public record ReunionDto(
    Guid Id,
    int Orden,
    TimeSpan HoraInicio,
    TimeSpan HoraFin,
    Guid ParticipanteId,
    string ParticipanteNombre,
    string ParticipanteCargo,
    Guid OficinaId,
    string OficinaNombre,
    string OficinaDir,
    string IdiomaReunion,
    int TiempoTrasladoSiguiente);

/// <summary>Filtros opcionales para GET /agendas.</summary>
public record FiltrosAgendaQuery(
    Guid? InversorId,
    DateTime? Fecha,
    string? Estado);

/// <summary>DTO simplificado para el listado de agendas.</summary>
public record AgendaResumenDto(
    Guid Id,
    Guid InversorId,
    string InversorNombre,
    DateTime Fecha,
    string Estado,
    int CantidadReuniones,
    DateTime FechaGeneracion);
