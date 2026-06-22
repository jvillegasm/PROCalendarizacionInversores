namespace Catalogo.Domain.Entities;

/// <summary>
/// Representa un inversor extranjero que visita Costa Rica.
/// Contiene los datos personales, idiomas que maneja y la ventana de visita.
/// Reglas RN-01 (idiomas), RN-02 (fechas), RN-03 (no eliminar con agendas activas).
/// </summary>
public class Inversor
{
    /// <summary>Identificador único del inversor (GUID).</summary>
    public Guid Id { get; set; }

    /// <summary>Nombre completo del visitante extranjero.</summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>Empresa que representa durante la visita.</summary>
    public string Empresa { get; set; } = string.Empty;

    /// <summary>País de origen desde donde viaja.</summary>
    public string PaisOrigen { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de inicio de la visita a Costa Rica.
    /// RN-02: debe ser menor o igual a FechaFinVisita.
    /// </summary>
    public DateTime FechaInicioVisita { get; set; }

    /// <summary>
    /// Fecha de cierre de la visita.
    /// RN-02: debe ser mayor o igual a FechaInicioVisita.
    /// </summary>
    public DateTime FechaFinVisita { get; set; }

    /// <summary>Lugar de hospedaje o punto de partida de cada jornada.</summary>
    public string LugarHospedaje { get; set; } = string.Empty;

    // Navegación
    /// <summary>
    /// Idiomas que maneja el inversor.
    /// RN-01: debe tener al menos uno asignado.
    /// </summary>
    public ICollection<InversorIdioma> InversoresIdiomas { get; set; } = new List<InversorIdioma>();
}
