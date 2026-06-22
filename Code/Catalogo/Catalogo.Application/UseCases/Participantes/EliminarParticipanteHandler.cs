using Catalogo.Application.Interfaces;
using Catalogo.Domain.Exceptions;

namespace Catalogo.Application.UseCases.Participantes;

/// <summary>Maneja la eliminación de un participante.</summary>
public class EliminarParticipanteHandler(IParticipanteRepository participanteRepository)
{
    /// <summary>
    /// Elimina el participante identificado por <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Id del participante.</param>
    /// <exception cref="ParticipanteNotFoundException">Si no existe.</exception>
    public async Task HandleAsync(Guid id)
    {
        var participante = await participanteRepository.GetByIdAsync(id)
            ?? throw new ParticipanteNotFoundException(id);

        await participanteRepository.DeleteAsync(participante);
    }
}
