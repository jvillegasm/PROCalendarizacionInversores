namespace Catalogo.Domain.Entities;

/// <summary>
/// Representa una oficina física donde se realizan las reuniones con inversores.
/// RN-06: no puede eliminarse si tiene participantes activos asignados.
/// </summary>
public class Oficina
{
    /// <summary>Identificador único de la oficina (GUID).</summary>
    public Guid Id { get; set; }

    /// <summary>Nombre identificable de la oficina (ej. "PROCOMER Central").</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Dirección física completa donde está ubicada.</summary>
    public string DireccionFisica { get; set; } = string.Empty;

    /// <summary>Coordenadas geográficas opcionales (ej. "9.9355,-84.0838").</summary>
    public string? Coordenadas { get; set; }

    // Navegación
    /// <summary>Participantes asignados a esta oficina.</summary>
    public ICollection<Participante> Participantes { get; set; } = new List<Participante>();

    /// <summary>Traslados donde esta oficina es el origen.</summary>
    public ICollection<MatrizTraslado> TrasladosOrigen { get; set; } = new List<MatrizTraslado>();

    /// <summary>Traslados donde esta oficina es el destino.</summary>
    public ICollection<MatrizTraslado> TrasladosDestino { get; set; } = new List<MatrizTraslado>();
}
