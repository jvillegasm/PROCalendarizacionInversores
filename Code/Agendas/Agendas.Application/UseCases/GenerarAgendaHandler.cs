using Agendas.Application.DTOs;
using Agendas.Application.Engine;
using Agendas.Application.Interfaces;
using Agendas.Domain.Entities;
using Agendas.Domain.Enums;
using Agendas.Domain.Exceptions;

namespace Agendas.Application.UseCases;

/// <summary>
/// Maneja el caso de uso CU-04: Generar Agenda Automática.
/// Orquesta la consulta al Catálogo Service, el motor de scheduling y la persistencia.
/// </summary>
public class GenerarAgendaHandler(
    ICatalogoHttpClient catalogoClient,
    ISchedulingEngine schedulingEngine,
    ILanguageCompatibilityFilter languageFilter,
    IAvailabilitySlotBuilder slotBuilder,
    IAgendaRepository agendaRepository)
{
    /// <summary>
    /// Genera una agenda automática para el inversor en la fecha indicada.
    /// </summary>
    /// <param name="request">Solicitud con inversorId, candidatos, fecha, duración y meta.</param>
    /// <returns>DTO de la agenda generada (completa o parcial).</returns>
    /// <exception cref="CatalogoServiceNoDisponibleException">Si el Catálogo Service no responde.</exception>
    /// <exception cref="FechaFueraDeRangoException">RN-08: fecha fuera del rango de visita.</exception>
    /// <exception cref="IdiomaIncompatibleException">RN-12: ningún candidato comparte idioma.</exception>
    /// <exception cref="SinDisponibilidadException">Si ningún candidato compatible tiene disponibilidad.</exception>
    public async Task<AgendaDto> HandleAsync(GenerarAgendaRequest request)
    {
        // Obtener datos del inversor desde Catálogo Service
        var inversor = await catalogoClient.GetInversorAsync(request.InversorId)
            ?? throw new CatalogoServiceNoDisponibleException();

        // RN-08: la fecha debe estar dentro del rango de visita
        if (request.Fecha.Date < inversor.FechaInicioVisita.Date ||
            request.Fecha.Date > inversor.FechaFinVisita.Date)
            throw new FechaFueraDeRangoException(request.Fecha, inversor.FechaInicioVisita, inversor.FechaFinVisita);

        // Obtener candidatos seleccionados por el coordinador
        var candidatos = (await catalogoClient.GetParticipantesByIdsAsync(request.CandidatoIds)).ToList();

        // RN-12: filtrar candidatos por idioma compartido con el inversor
        var idiomasInversor = inversor.Idiomas.Select(i => i.Id).ToList();
        var compatibles = languageFilter.Filtrar(candidatos, idiomasInversor);

        if (compatibles.Count == 0)
            throw new IdiomaIncompatibleException();

        // Construir slots de disponibilidad para cada candidato compatible
        var candidatosConSlots = compatibles
            .Select(c => new CandidatoAgenda(c, slotBuilder.Construir(c, request.Fecha)))
            .Where(c => c.SlotsDisponibles.Count > 0)
            .ToList();

        if (candidatosConSlots.Count == 0)
            throw new SinDisponibilidadException();

        // Obtener matriz de traslados y oficinas
        var traslados = await catalogoClient.GetMatrizTrasladosAsync();
        var oficinas = await catalogoClient.GetOficinasAsync();

        var matrizDict = traslados.ToDictionary(
            t => (t.OficinaOrigenId, t.OficinaDestinoId),
            t => t.TiempoMinutos);

        // El punto de partida del inversor (hospedaje tratado como punto inicial)
        // Si no hay una oficina que coincida con el hospedaje, se usa Guid.Empty (tiempo 0)
        var oficinaInicio = Guid.Empty;

        // Ejecutar motor de scheduling greedy
        var resultado = schedulingEngine.Generar(
            candidatosConSlots,
            request.DuracionMinutos,
            request.MetaReuniones,
            oficinaInicio,
            matrizDict,
            oficinas);

        if (resultado.ReunionesLogradas == 0)
            throw new SinDisponibilidadException();

        // Persistir la agenda y sus reuniones
        var agendaId = Guid.NewGuid();
        var agenda = new Agenda
        {
            Id = agendaId,
            InversorId = request.InversorId,
            Fecha = request.Fecha,
            Estado = EstadoAgenda.Activa,
            FechaGeneracion = DateTime.UtcNow,
            Reuniones = resultado.Reuniones
                .Select((r, idx) => new Reunion
                {
                    Id = Guid.NewGuid(),
                    AgendaId = agendaId,
                    ParticipanteId = r.ParticipanteId,
                    HoraInicio = r.HoraInicio,
                    HoraFin = r.HoraFin,
                    OficinaId = r.OficinaId,
                    IdiomaReunion = r.IdiomaReunion,
                    Orden = idx + 1,
                    TiempoTrasladoSiguiente = r.TiempoTrasladoSiguiente
                }).ToList()
        };

        await agendaRepository.AddAsync(agenda);

        return MapToDto(agenda, inversor, resultado);
    }

    /// <summary>Mapea la Agenda y el resultado al DTO de respuesta.</summary>
    private static AgendaDto MapToDto(Agenda agenda, InversorCatalogoDto inversor, AgendaResult resultado) =>
        new(agenda.Id,
            agenda.InversorId,
            inversor.NombreCompleto,
            inversor.Empresa,
            agenda.Fecha,
            agenda.Estado.ToString(),
            agenda.FechaGeneracion,
            agenda.FechaAnulacion,
            resultado.ReunionesLogradas,
            resultado.MetaSolicitada,
            resultado.Completa,
            resultado.Reuniones.Select((r, i) => new ReunionDto(
                agenda.Reuniones.ElementAt(i).Id,
                i + 1,
                r.HoraInicio, r.HoraFin,
                r.ParticipanteId, r.ParticipanteNombre, r.ParticipanteCargo,
                r.OficinaId, r.OficinaNombre, r.OficinaDir,
                r.IdiomaReunion, r.TiempoTrasladoSiguiente)));
}
