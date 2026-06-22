namespace Agendas.Domain.Entities;

/// <summary>
/// Elemento de una agenda: una reunión entre el inversor y un participante.
/// Cumple con RN-09 (inicio ≥ 08:00), RN-10 (fin ≤ 17:00), RN-11 (sin solapar 12:00-13:00),
/// RN-12 (idioma compartido), RN-13 (traslado suficiente), RN-14 (sin solapamiento).
/// </summary>
public class Reunion
{
    /// <summary>Identificador único de la reunión (GUID).</summary>
    public Guid Id { get; set; }

    /// <summary>Agenda a la que pertenece esta reunión.</summary>
    public Guid AgendaId { get; set; }

    /// <summary>
    /// Id del participante convocado.
    /// Referencia cruzada a la tabla Participantes del Catálogo Service.
    /// </summary>
    public Guid ParticipanteId { get; set; }

    /// <summary>Hora de inicio de la reunión. RN-09: debe ser ≥ 08:00.</summary>
    public TimeSpan HoraInicio { get; set; }

    /// <summary>Hora de fin de la reunión. RN-10: debe ser ≤ 17:00.</summary>
    public TimeSpan HoraFin { get; set; }

    /// <summary>
    /// Oficina donde se realiza la reunión.
    /// Referencia cruzada a la tabla Oficinas del Catálogo Service.
    /// </summary>
    public Guid OficinaId { get; set; }

    /// <summary>Idioma en que se realizará la reunión (nombre del idioma, ej. "Español").</summary>
    public string IdiomaReunion { get; set; } = string.Empty;

    /// <summary>Posición secuencial de la reunión dentro de la agenda (1, 2, 3…).</summary>
    public int Orden { get; set; }

    /// <summary>
    /// Tiempo de traslado hacia la siguiente reunión en minutos.
    /// 0 si es la última reunión del día.
    /// RN-13: el inversor debe tener este tiempo entre reuniones.
    /// </summary>
    public int TiempoTrasladoSiguiente { get; set; }

    // Navegación
    /// <summary>Agenda propietaria de esta reunión.</summary>
    public Agenda Agenda { get; set; } = null!;
}
