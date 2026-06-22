using Catalogo.Application.DTOs;
using Catalogo.Application.Interfaces;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Exceptions;

namespace Catalogo.Application.UseCases.Participantes;

/// <summary>
/// Maneja la actualización de un participante existente (FA-01 y FA-02 de CU-02).
/// </summary>
public class ActualizarParticipanteHandler(
    IParticipanteRepository participanteRepository,
    IOficinaRepository oficinaRepository,
    IIdiomaRepository idiomaRepository)
{
    /// <summary>
    /// Actualiza los datos del participante, incluyendo idiomas y disponibilidad.
    /// </summary>
    /// <param name="id">Id del participante.</param>
    /// <param name="request">Nuevos datos.</param>
    /// <returns>DTO del participante actualizado.</returns>
    public async Task<ParticipanteDto> HandleAsync(Guid id, ActualizarParticipanteRequest request)
    {
        var participante = await participanteRepository.GetByIdAsync(id)
            ?? throw new ParticipanteNotFoundException(id);

        if (!request.IdiomaIds.Any())
            throw new IdiomaRequeridoParticipanteException();

        if (request.OficinaId == Guid.Empty)
            throw new OficinaRequeridaException();

        var oficina = await oficinaRepository.GetByIdAsync(request.OficinaId)
            ?? throw new OficinaNotFoundException(request.OficinaId);

        var idiomas = (await idiomaRepository.GetByIdsAsync(request.IdiomaIds)).ToList();

        participante.NombreCompleto = request.NombreCompleto;
        participante.Cargo = request.Cargo;
        participante.OficinaId = request.OficinaId;
        participante.Activo = request.Activo;
        participante.ParticipantesIdiomas = idiomas.Select(i => new ParticipanteIdioma
        {
            ParticipanteId = participante.Id,
            IdiomaId = i.Id
        }).ToList();
        participante.Disponibilidades = request.Disponibilidades.Select(d => new DisponibilidadParticipante
        {
            Id = Guid.NewGuid(),
            ParticipanteId = participante.Id,
            Fecha = d.Fecha,
            HoraInicio = d.HoraInicio,
            HoraFin = d.HoraFin
        }).ToList();

        await participanteRepository.UpdateAsync(participante);

        return new ParticipanteDto(participante.Id, participante.NombreCompleto, participante.Cargo,
            participante.OficinaId, oficina.Nombre, participante.Activo,
            idiomas.Select(i => new IdiomaDto(i.Id, i.Nombre, i.Codigo)),
            participante.Disponibilidades.Select(d => new DisponibilidadDto(d.Id, d.Fecha, d.HoraInicio, d.HoraFin)));
    }
}
