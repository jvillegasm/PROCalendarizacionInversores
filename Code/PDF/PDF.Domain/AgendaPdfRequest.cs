namespace PDF.Domain;

/// <summary>
/// Datos de una reunión individual para incluir en el PDF del itinerario.
/// </summary>
public record ReunionPdfItem(
    int Orden,
    string HoraInicio,
    string HoraFin,
    string ParticipanteNombre,
    string ParticipanteCargo,
    string OficinaNombre,
    string OficinaDir,
    string IdiomaReunion,
    int TiempoTrasladoSiguiente);

/// <summary>
/// Solicitud de generación de PDF enviada por el Agendas Service al PDF Service.
/// Contiene todos los datos necesarios para construir el documento (AC-06 / §4.4).
/// </summary>
public class AgendaPdfRequest
{
    /// <summary>Id de la agenda.</summary>
    public Guid AgendaId { get; set; }

    /// <summary>Nombre completo del inversor visitante.</summary>
    public string InversorNombre { get; set; } = string.Empty;

    /// <summary>Empresa que representa el inversor.</summary>
    public string InversorEmpresa { get; set; } = string.Empty;

    /// <summary>Fecha de la jornada.</summary>
    public DateTime Fecha { get; set; }

    /// <summary>Estado de la agenda (Activa / Anulada).</summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>Lista de reuniones ordenadas cronológicamente.</summary>
    public List<ReunionPdfItem> Reuniones { get; set; } = new();
}
