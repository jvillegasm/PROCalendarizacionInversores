namespace Catalogo.Domain.Entities;

/// <summary>
/// Bloque de disponibilidad horaria de un participante para una fecha específica.
/// Indica la franja en que el participante puede atender reuniones ese día.
/// </summary>
public class DisponibilidadParticipante
{
    /// <summary>Identificador único del bloque (GUID).</summary>
    public Guid Id { get; set; }

    /// <summary>Participante propietario de este bloque.</summary>
    public Guid ParticipanteId { get; set; }

    /// <summary>Fecha para la cual aplica la disponibilidad.</summary>
    public DateTime Fecha { get; set; }

    /// <summary>Hora de inicio del bloque disponible (HH:mm).</summary>
    public TimeSpan HoraInicio { get; set; }

    /// <summary>Hora de fin del bloque disponible (HH:mm).</summary>
    public TimeSpan HoraFin { get; set; }

    // Navegación
    /// <summary>Participante relacionado.</summary>
    public Participante Participante { get; set; } = null!;
}
