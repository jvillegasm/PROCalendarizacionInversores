namespace Agendas.Domain.Exceptions;

/// <summary>Clase base para excepciones de dominio del Agendas Service.</summary>
public abstract class AgendasDomainException(string message) : Exception(message);

/// <summary>Excepción lanzada cuando no se encuentra la agenda con el Id indicado.</summary>
public class AgendaNotFoundException(Guid id)
    : AgendasDomainException($"No se encontró la agenda con Id '{id}'.");

/// <summary>
/// Excepción lanzada cuando se intenta anular una agenda que ya está anulada.
/// FE-02 de CU-05 → HTTP 409.
/// </summary>
public class AgendaYaAnuladaException()
    : AgendasDomainException("La agenda ya se encuentra anulada.");

/// <summary>
/// Excepción lanzada cuando la fecha de la agenda está fuera del rango de visita del inversor.
/// RN-08 → HTTP 422, código FECHA_FUERA_DE_RANGO.
/// </summary>
public class FechaFueraDeRangoException(DateTime fecha, DateTime inicio, DateTime fin)
    : AgendasDomainException(
        $"La fecha {fecha:dd/MM/yyyy} está fuera del período de visita del inversor " +
        $"({inicio:dd/MM/yyyy} – {fin:dd/MM/yyyy}).")
{
    /// <summary>Código de error para el cliente.</summary>
    public string CodigoError => "FECHA_FUERA_DE_RANGO";
}

/// <summary>
/// Excepción lanzada cuando ningún candidato comparte idioma con el inversor.
/// RN-12 → HTTP 422, código IDIOMA_INCOMPATIBLE.
/// </summary>
public class IdiomaIncompatibleException()
    : AgendasDomainException(
        "No existen participantes que compartan idioma con el inversor. " +
        "Verifique la configuración de idiomas de los candidatos.")
{
    /// <summary>Código de error para el cliente.</summary>
    public string CodigoError => "IDIOMA_INCOMPATIBLE";
}

/// <summary>
/// Excepción lanzada cuando ningún candidato compatible tiene disponibilidad para la fecha.
/// → HTTP 422, código SIN_DISPONIBILIDAD.
/// </summary>
public class SinDisponibilidadException()
    : AgendasDomainException(
        "Ningún participante compatible tiene disponibilidad registrada para la fecha solicitada.")
{
    /// <summary>Código de error para el cliente.</summary>
    public string CodigoError => "SIN_DISPONIBILIDAD";
}

/// <summary>
/// Excepción lanzada cuando los tiempos de traslado impiden encadenar cualquier reunión.
/// → HTTP 422, código TRASLADOS_INVIABLES.
/// </summary>
public class TrasladosInviablesException()
    : AgendasDomainException(
        "Los tiempos de traslado entre las oficinas disponibles no permiten encadenar " +
        "ninguna reunión para esa fecha. Considere reducir la duración de reunión o " +
        "seleccionar participantes en oficinas más cercanas.")
{
    /// <summary>Código de error para el cliente.</summary>
    public string CodigoError => "TRASLADOS_INVIABLES";
}

/// <summary>
/// Excepción lanzada cuando el Catálogo Service no responde tras los reintentos con Polly.
/// AC-09 → HTTP 503.
/// </summary>
public class CatalogoServiceNoDisponibleException()
    : AgendasDomainException(
        "El servicio de catálogo no está disponible en este momento. Intente nuevamente en unos segundos.");

/// <summary>Excepción lanzada cuando el PDF Service no responde o retorna error.</summary>
public class PdfServiceNoDisponibleException(string? detalle = null)
    : AgendasDomainException(
        detalle ?? "El servicio de generación de PDF no está disponible. Intente nuevamente.");
