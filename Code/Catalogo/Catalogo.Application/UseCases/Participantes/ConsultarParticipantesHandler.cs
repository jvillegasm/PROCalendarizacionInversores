using Catalogo.Application.DTOs;
using Catalogo.Application.Interfaces;
using Catalogo.Domain.Exceptions;

namespace Catalogo.Application.UseCases.Participantes;

/// <summary>Maneja la consulta de participantes.</summary>
public class ConsultarParticipantesHandler(IParticipanteRepository participanteRepository)
{
    /// <summary>Obtiene todos los participantes con sus relaciones.</summary>
    public async Task<IEnumerable<ParticipanteDto>> GetAllAsync()
    {
        var participantes = await participanteRepository.GetAllAsync();
        return participantes.Select(ToDto);
    }

    /// <summary>Obtiene solo participantes activos.</summary>
    public async Task<IEnumerable<ParticipanteDto>> GetActivosAsync()
    {
        var participantes = await participanteRepository.GetActivosAsync();
        return participantes.Select(ToDto);
    }

    /// <summary>Obtiene el detalle de un participante.</summary>
    /// <param name="id">Id del participante.</param>
    /// <exception cref="ParticipanteNotFoundException">Si no existe.</exception>
    public async Task<ParticipanteDto> GetByIdAsync(Guid id)
    {
        var p = await participanteRepository.GetByIdAsync(id)
            ?? throw new ParticipanteNotFoundException(id);
        return ToDto(p);
    }

    private static ParticipanteDto ToDto(Domain.Entities.Participante p) =>
        new(p.Id, p.NombreCompleto, p.Cargo, p.OficinaId,
            p.Oficina?.Nombre ?? string.Empty, p.Activo,
            p.ParticipantesIdiomas.Select(pi => new IdiomaDto(pi.Idioma.Id, pi.Idioma.Nombre, pi.Idioma.Codigo)),
            p.Disponibilidades.Select(d => new DisponibilidadDto(d.Id, d.Fecha, d.HoraInicio, d.HoraFin)));
}
