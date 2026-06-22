namespace Catalogo.Application.DTOs;

// ─── Participantes ─────────────────────────────────────────────────────────────

/// <summary>DTO de respuesta con los datos completos de un participante.</summary>
public record ParticipanteDto(
    Guid Id,
    string NombreCompleto,
    string Cargo,
    Guid OficinaId,
    string OficinaNombre,
    bool Activo,
    IEnumerable<IdiomaDto> Idiomas,
    IEnumerable<DisponibilidadDto> Disponibilidades);

/// <summary>DTO para crear un nuevo participante.</summary>
public record CrearParticipanteRequest(
    string NombreCompleto,
    string Cargo,
    Guid OficinaId,
    IEnumerable<Guid> IdiomaIds,
    IEnumerable<CrearDisponibilidadRequest> Disponibilidades);

/// <summary>DTO para actualizar un participante existente.</summary>
public record ActualizarParticipanteRequest(
    string NombreCompleto,
    string Cargo,
    Guid OficinaId,
    bool Activo,
    IEnumerable<Guid> IdiomaIds,
    IEnumerable<CrearDisponibilidadRequest> Disponibilidades);

/// <summary>DTO para un bloque de disponibilidad horaria.</summary>
public record DisponibilidadDto(Guid Id, DateTime Fecha, TimeSpan HoraInicio, TimeSpan HoraFin);

/// <summary>DTO para crear un bloque de disponibilidad.</summary>
public record CrearDisponibilidadRequest(DateTime Fecha, TimeSpan HoraInicio, TimeSpan HoraFin);

// ─── Oficinas ──────────────────────────────────────────────────────────────────

/// <summary>DTO de respuesta para una oficina.</summary>
public record OficinaDto(Guid Id, string Nombre, string DireccionFisica, string? Coordenadas);

/// <summary>DTO para crear una nueva oficina.</summary>
public record CrearOficinaRequest(string Nombre, string DireccionFisica, string? Coordenadas);

// ─── Traslados ─────────────────────────────────────────────────────────────────

/// <summary>DTO de respuesta para un par de traslado.</summary>
public record MatrizTrasladoDto(
    Guid Id,
    Guid OficinaOrigenId,
    string OficinaOrigenNombre,
    Guid OficinaDestinoId,
    string OficinaDestinoNombre,
    int TiempoMinutos);

/// <summary>DTO para registrar un par de traslado (crea automáticamente el par simétrico).</summary>
public record CrearTrasladoRequest(
    Guid OficinaOrigenId,
    Guid OficinaDestinoId,
    int TiempoMinutos);

/// <summary>DTO para actualizar el tiempo de un par de traslado (y su simétrico).</summary>
public record ActualizarTrasladoRequest(int TiempoMinutos);
