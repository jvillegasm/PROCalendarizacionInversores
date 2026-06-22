namespace Catalogo.Domain.Entities;

/// <summary>
/// Par de traslado entre dos oficinas con el tiempo estimado en minutos.
/// RN-07: la matriz debe ser simétrica; la Application garantiza que A→B = B→A
/// persistiendo ambos pares en la misma transacción.
/// </summary>
public class MatrizTraslado
{
    /// <summary>Identificador único del par de traslado (GUID).</summary>
    public Guid Id { get; set; }

    /// <summary>Oficina de origen del desplazamiento.</summary>
    public Guid OficinaOrigenId { get; set; }

    /// <summary>Oficina de destino del desplazamiento.</summary>
    public Guid OficinaDestinoId { get; set; }

    /// <summary>
    /// Tiempo estimado de desplazamiento en minutos.
    /// RN-07: debe ser igual al par inverso (DestinoId → OrigenId).
    /// </summary>
    public int TiempoMinutos { get; set; }

    // Navegación
    /// <summary>Oficina de origen.</summary>
    public Oficina OficinaOrigen { get; set; } = null!;

    /// <summary>Oficina de destino.</summary>
    public Oficina OficinaDestino { get; set; } = null!;
}
