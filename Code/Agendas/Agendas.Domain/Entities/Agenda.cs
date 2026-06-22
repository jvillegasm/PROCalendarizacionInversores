using Agendas.Domain.Enums;

namespace Agendas.Domain.Entities;

/// <summary>
/// Agenda diaria generada para un inversor en una fecha específica.
/// Contiene la colección de reuniones programadas por el motor de scheduling.
/// RN-15: la anulación es lógica; el registro nunca se elimina físicamente.
/// </summary>
public class Agenda
{
    /// <summary>Identificador único de la agenda (GUID).</summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Id del inversor para quien se generó la agenda.
    /// Referencia cruzada a la tabla Inversores del Catálogo Service (mismo DB).
    /// </summary>
    public Guid InversorId { get; set; }

    /// <summary>
    /// Fecha de la jornada para la cual aplica la agenda.
    /// RN-08: debe estar dentro del rango [FechaInicioVisita, FechaFinVisita] del inversor.
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Estado actual de la agenda.
    /// RN-15: solo puede pasar de Activa a Anulada; la anulación es irreversible.
    /// </summary>
    public EstadoAgenda Estado { get; set; } = EstadoAgenda.Activa;

    /// <summary>Fecha y hora en que se generó la agenda.</summary>
    public DateTime FechaGeneracion { get; set; }

    /// <summary>
    /// Fecha y hora en que se anuló la agenda.
    /// Null si la agenda sigue Activa.
    /// </summary>
    public DateTime? FechaAnulacion { get; set; }

    // Navegación
    /// <summary>Colección de reuniones que conforman el itinerario del inversor.</summary>
    public ICollection<Reunion> Reuniones { get; set; } = new List<Reunion>();
}
