using Agendas.Application.DTOs;
using Agendas.Application.Interfaces;
using Agendas.Domain.Exceptions;

namespace Agendas.Application.UseCases;

/// <summary>
/// Maneja la consulta del listado de agendas con filtros opcionales (CU-05 / AC-08).
/// </summary>
public class ConsultarAgendasHandler(
    IAgendaRepository agendaRepository,
    ICatalogoHttpClient catalogoClient)
{
    /// <summary>
    /// Obtiene el listado de agendas aplicando filtros opcionales.
    /// </summary>
    /// <param name="filtros">Filtros opcionales por inversor, fecha y estado.</param>
    public async Task<IEnumerable<AgendaResumenDto>> HandleAsync(FiltrosAgendaQuery filtros)
    {
        var agendas = await agendaRepository.GetAllAsync(filtros);

        // Obtener nombres de inversores para enriquecer el resumen
        var inversoreIds = agendas.Select(a => a.InversorId).Distinct().ToList();
        var nombresDict = new Dictionary<Guid, string>();

        foreach (var id in inversoreIds)
        {
            var inversor = await catalogoClient.GetInversorAsync(id);
            if (inversor is not null)
                nombresDict[id] = inversor.NombreCompleto;
        }

        return agendas.Select(a => new AgendaResumenDto(
            a.Id,
            a.InversorId,
            nombresDict.TryGetValue(a.InversorId, out var nombre) ? nombre : "Desconocido",
            a.Fecha,
            a.Estado.ToString(),
            a.Reuniones.Count,
            a.FechaGeneracion));
    }
}

/// <summary>
/// Maneja la consulta del detalle de una agenda específica (AC-08).
/// </summary>
public class ConsultarAgendaHandler(
    IAgendaRepository agendaRepository,
    ICatalogoHttpClient catalogoClient)
{
    /// <summary>
    /// Obtiene el detalle completo de la agenda por Id.
    /// </summary>
    /// <param name="id">Id de la agenda.</param>
    /// <exception cref="AgendaNotFoundException">Si no existe.</exception>
    public async Task<AgendaDto> HandleAsync(Guid id)
    {
        var agenda = await agendaRepository.GetByIdAsync(id)
            ?? throw new AgendaNotFoundException(id);

        var inversor = await catalogoClient.GetInversorAsync(agenda.InversorId);
        var oficinas = (await catalogoClient.GetOficinasAsync())
            .ToDictionary(o => o.Id);

        // Resolver nombres de participantes
        var participanteIds = agenda.Reuniones.Select(r => r.ParticipanteId).Distinct().ToList();
        var participantes = (await catalogoClient.GetParticipantesByIdsAsync(participanteIds))
            .ToDictionary(p => p.Id);

        return new AgendaDto(
            agenda.Id,
            agenda.InversorId,
            inversor?.NombreCompleto ?? "Desconocido",
            inversor?.Empresa ?? string.Empty,
            agenda.Fecha,
            agenda.Estado.ToString(),
            agenda.FechaGeneracion,
            agenda.FechaAnulacion,
            agenda.Reuniones.Count,
            agenda.Reuniones.Count,
            true,
            agenda.Reuniones.OrderBy(r => r.Orden).Select(r =>
            {
                participantes.TryGetValue(r.ParticipanteId, out var p);
                oficinas.TryGetValue(r.OficinaId, out var o);
                return new ReunionDto(
                    r.Id, r.Orden,
                    r.HoraInicio, r.HoraFin,
                    r.ParticipanteId,
                    p?.NombreCompleto ?? "Desconocido",
                    p?.Cargo ?? string.Empty,
                    r.OficinaId,
                    o?.Nombre ?? string.Empty,
                    o?.DireccionFisica ?? string.Empty,
                    r.IdiomaReunion,
                    r.TiempoTrasladoSiguiente);
            }));
    }
}

/// <summary>
/// Maneja la anulación lógica de una agenda (CU-05 / AC-07).
/// RN-15: el registro y su PDF se conservan para trazabilidad histórica.
/// </summary>
public class AnularAgendaHandler(IAgendaRepository agendaRepository)
{
    /// <summary>
    /// Anula lógicamente la agenda identificada por <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Id de la agenda a anular.</param>
    /// <exception cref="AgendaNotFoundException">Si no existe.</exception>
    /// <exception cref="AgendaYaAnuladaException">Si ya está anulada (HTTP 409).</exception>
    public async Task HandleAsync(Guid id)
    {
        var agenda = await agendaRepository.GetByIdAsync(id)
            ?? throw new AgendaNotFoundException(id);

        if (agenda.Estado == Domain.Enums.EstadoAgenda.Anulada)
            throw new AgendaYaAnuladaException();

        // RN-15: anulación lógica — solo cambiar estado y registrar fecha
        agenda.Estado = Domain.Enums.EstadoAgenda.Anulada;
        agenda.FechaAnulacion = DateTime.UtcNow;

        await agendaRepository.UpdateAsync(agenda);
    }
}
