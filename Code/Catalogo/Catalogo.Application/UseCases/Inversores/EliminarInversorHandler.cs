using Catalogo.Application.Interfaces;
using Catalogo.Domain.Exceptions;

namespace Catalogo.Application.UseCases.Inversores;

/// <summary>
/// Maneja la eliminación de un inversor (FA-03 de CU-01).
/// RN-03: bloquea la eliminación si el inversor tiene agendas activas.
/// </summary>
public class EliminarInversorHandler(
    IInversorRepository inversorRepository,
    IAgendaStatusChecker agendaStatusChecker)
{
    /// <summary>
    /// Elimina el inversor identificado por <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Id del inversor a eliminar.</param>
    /// <exception cref="InversorNotFoundException">Si el inversor no existe.</exception>
    /// <exception cref="InversorConAgendasActivasException">RN-03: si tiene agendas activas.</exception>
    public async Task HandleAsync(Guid id)
    {
        var inversor = await inversorRepository.GetByIdAsync(id)
            ?? throw new InversorNotFoundException(id);

        // RN-03: verificar agendas activas antes de eliminar
        if (await agendaStatusChecker.TieneAgendasActivasAsync(id))
            throw new InversorConAgendasActivasException();

        await inversorRepository.DeleteAsync(inversor);
    }
}
