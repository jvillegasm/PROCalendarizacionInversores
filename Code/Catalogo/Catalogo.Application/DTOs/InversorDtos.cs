namespace Catalogo.Application.DTOs;

// ─── Inversores ────────────────────────────────────────────────────────────────

/// <summary>DTO de respuesta con los datos completos de un inversor.</summary>
public record InversorDto(
    Guid Id,
    string NombreCompleto,
    string Empresa,
    string PaisOrigen,
    DateTime FechaInicioVisita,
    DateTime FechaFinVisita,
    string LugarHospedaje,
    IEnumerable<IdiomaDto> Idiomas);

/// <summary>DTO para crear un nuevo inversor (POST /api/inversores).</summary>
public record CrearInversorRequest(
    string NombreCompleto,
    string Empresa,
    string PaisOrigen,
    DateTime FechaInicioVisita,
    DateTime FechaFinVisita,
    string LugarHospedaje,
    IEnumerable<Guid> IdiomaIds);

/// <summary>DTO para actualizar un inversor existente (PUT /api/inversores/{id}).</summary>
public record ActualizarInversorRequest(
    string NombreCompleto,
    string Empresa,
    string PaisOrigen,
    DateTime FechaInicioVisita,
    DateTime FechaFinVisita,
    string LugarHospedaje,
    IEnumerable<Guid> IdiomaIds);

// ─── Idiomas ────────────────────────────────────────────────────────────────────

/// <summary>DTO de respuesta para un idioma del catálogo.</summary>
public record IdiomaDto(Guid Id, string Nombre, string Codigo);
