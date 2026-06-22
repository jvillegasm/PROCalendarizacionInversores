using Agendas.Application.DTOs;

namespace Agendas.Application.Interfaces;

/// <summary>
/// Cliente HTTP tipado hacia el Catálogo Service.
/// Implementado en Infrastructure con IHttpClientFactory + políticas de resiliencia Polly.
/// AC-09: 3 reintentos con backoff exponencial + circuit breaker + timeout.
/// </summary>
public interface ICatalogoHttpClient
{
    /// <summary>Obtiene el inversor por Id desde el Catálogo Service.</summary>
    /// <param name="inversorId">Id del inversor.</param>
    Task<InversorCatalogoDto?> GetInversorAsync(Guid inversorId);

    /// <summary>Obtiene los participantes activos con disponibilidad e idiomas.</summary>
    Task<IEnumerable<ParticipanteCatalogoDto>> GetParticipantesActivosAsync();

    /// <summary>Obtiene participantes activos filtrados por Ids específicos.</summary>
    /// <param name="ids">Ids de los candidatos seleccionados.</param>
    Task<IEnumerable<ParticipanteCatalogoDto>> GetParticipantesByIdsAsync(IEnumerable<Guid> ids);

    /// <summary>Obtiene la matriz completa de traslados entre oficinas.</summary>
    Task<IEnumerable<MatrizTrasladoCatalogoDto>> GetMatrizTrasladosAsync();

    /// <summary>Obtiene todas las oficinas registradas.</summary>
    Task<IEnumerable<OficinaCatalogoDto>> GetOficinasAsync();
}

/// <summary>
/// Cliente HTTP tipado hacia el PDF Service.
/// Implementado en Infrastructure con IHttpClientFactory + políticas de resiliencia.
/// </summary>
public interface IPdfServiceHttpClient
{
    /// <summary>
    /// Solicita la generación de un PDF de agenda al PDF Service.
    /// </summary>
    /// <param name="request">Datos de la agenda para el PDF.</param>
    /// <returns>Bytes del PDF generado.</returns>
    Task<byte[]> GenerarPdfAsync(object request);
}
