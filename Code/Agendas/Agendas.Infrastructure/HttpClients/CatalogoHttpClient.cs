using Agendas.Application.DTOs;
using Agendas.Application.Interfaces;
using Agendas.Domain.Exceptions;
using System.Net.Http.Json;

namespace Agendas.Infrastructure.HttpClients;

/// <summary>
/// Cliente HTTP tipado hacia el Catálogo Service.
/// Utiliza IHttpClientFactory con políticas de resiliencia Polly configuradas en Program.cs:
/// - 3 reintentos con backoff exponencial.
/// - Circuit breaker.
/// - Timeout de 30 segundos.
/// AC-09: comportamiento ante fallos en llamadas entre microservicios.
/// </summary>
public class CatalogoHttpClient(HttpClient httpClient) : ICatalogoHttpClient
{
    /// <inheritdoc/>
    public async Task<InversorCatalogoDto?> GetInversorAsync(Guid inversorId)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<InversorCatalogoDto>(
                $"api/inversores/{inversorId}");
        }
        catch (HttpRequestException)
        {
            throw new CatalogoServiceNoDisponibleException();
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ParticipanteCatalogoDto>> GetParticipantesActivosAsync()
    {
        try
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<ParticipanteCatalogoDto>>(
                "api/participantes/activos") ?? [];
        }
        catch (HttpRequestException)
        {
            throw new CatalogoServiceNoDisponibleException();
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ParticipanteCatalogoDto>> GetParticipantesByIdsAsync(IEnumerable<Guid> ids)
    {
        try
        {
            // Obtener todos los activos y filtrar por los Ids requeridos
            var todos = await httpClient.GetFromJsonAsync<IEnumerable<ParticipanteCatalogoDto>>(
                "api/participantes/activos") ?? [];

            var idsSet = ids.ToHashSet();
            return todos.Where(p => idsSet.Contains(p.Id));
        }
        catch (HttpRequestException)
        {
            throw new CatalogoServiceNoDisponibleException();
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MatrizTrasladoCatalogoDto>> GetMatrizTrasladosAsync()
    {
        try
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<MatrizTrasladoCatalogoDto>>(
                "api/traslados") ?? [];
        }
        catch (HttpRequestException)
        {
            throw new CatalogoServiceNoDisponibleException();
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<OficinaCatalogoDto>> GetOficinasAsync()
    {
        try
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<OficinaCatalogoDto>>(
                "api/oficinas") ?? [];
        }
        catch (HttpRequestException)
        {
            throw new CatalogoServiceNoDisponibleException();
        }
    }
}
