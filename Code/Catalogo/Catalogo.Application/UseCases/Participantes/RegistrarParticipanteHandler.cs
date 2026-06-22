using Catalogo.Application.DTOs;
using Catalogo.Application.Interfaces;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Exceptions;

namespace Catalogo.Application.UseCases.Participantes;

/// <summary>
/// Maneja el registro de un nuevo participante (CU-02).
/// Aplica RN-04 (idiomas) y RN-05 (oficina).
/// </summary>
public class RegistrarParticipanteHandler(
    IParticipanteRepository participanteRepository,
    IOficinaRepository oficinaRepository,
    IIdiomaRepository idiomaRepository)
{
    /// <summary>
    /// Registra un nuevo participante con sus idiomas y disponibilidad.
    /// </summary>
    /// <param name="request">Datos del participante.</param>
    /// <returns>DTO del participante creado.</returns>
    /// <exception cref="IdiomaRequeridoParticipanteException">RN-04.</exception>
    /// <exception cref="OficinaRequeridaException">RN-05.</exception>
    /// <exception cref="OficinaNotFoundException">Si la oficina no existe.</exception>
    public async Task<ParticipanteDto> HandleAsync(CrearParticipanteRequest request)
    {
        // RN-04
        if (!request.IdiomaIds.Any())
            throw new IdiomaRequeridoParticipanteException();

        // RN-05
        if (request.OficinaId == Guid.Empty)
            throw new OficinaRequeridaException();

        var oficina = await oficinaRepository.GetByIdAsync(request.OficinaId)
            ?? throw new OficinaNotFoundException(request.OficinaId);

        var idiomas = (await idiomaRepository.GetByIdsAsync(request.IdiomaIds)).ToList();
        var participanteId = Guid.NewGuid();

        var participante = new Participante
        {
            Id = participanteId,
            NombreCompleto = request.NombreCompleto,
            Cargo = request.Cargo,
            OficinaId = request.OficinaId,
            Activo = true,
            ParticipantesIdiomas = idiomas.Select(i => new ParticipanteIdioma
            {
                ParticipanteId = participanteId,
                IdiomaId = i.Id
            }).ToList(),
            Disponibilidades = request.Disponibilidades.Select(d => new DisponibilidadParticipante
            {
                Id = Guid.NewGuid(),
                ParticipanteId = participanteId,
                Fecha = d.Fecha,
                HoraInicio = d.HoraInicio,
                HoraFin = d.HoraFin
            }).ToList()
        };

        await participanteRepository.AddAsync(participante);

        return MapToDto(participante, oficina.Nombre, idiomas);
    }

    private static ParticipanteDto MapToDto(Participante p, string oficinaNombre, List<Domain.Entities.Idioma> idiomas) =>
        new(p.Id, p.NombreCompleto, p.Cargo, p.OficinaId, oficinaNombre, p.Activo,
            idiomas.Select(i => new IdiomaDto(i.Id, i.Nombre, i.Codigo)),
            p.Disponibilidades.Select(d => new DisponibilidadDto(d.Id, d.Fecha, d.HoraInicio, d.HoraFin)));
}
