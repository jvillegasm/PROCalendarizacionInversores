using Catalogo.Application.DTOs;
using Catalogo.Application.Interfaces;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Exceptions;

namespace Catalogo.Application.UseCases.Inversores;

/// <summary>
/// Maneja la actualización de un inversor existente (FA-01 de CU-01).
/// Aplica RN-01 y RN-02.
/// </summary>
public class ActualizarInversorHandler(
    IInversorRepository inversorRepository,
    IIdiomaRepository idiomaRepository)
{
    /// <summary>
    /// Actualiza los datos del inversor identificado por <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Id del inversor a actualizar.</param>
    /// <param name="request">Nuevos datos del inversor.</param>
    /// <returns>DTO del inversor actualizado.</returns>
    /// <exception cref="InversorNotFoundException">Si el inversor no existe.</exception>
    /// <exception cref="IdiomaRequeridoException">RN-01.</exception>
    /// <exception cref="FechaVisitaInvalidaException">RN-02.</exception>
    public async Task<InversorDto> HandleAsync(Guid id, ActualizarInversorRequest request)
    {
        var inversor = await inversorRepository.GetByIdAsync(id)
            ?? throw new InversorNotFoundException(id);

        if (!request.IdiomaIds.Any())
            throw new IdiomaRequeridoException();

        if (request.FechaFinVisita < request.FechaInicioVisita)
            throw new FechaVisitaInvalidaException();

        var idiomas = (await idiomaRepository.GetByIdsAsync(request.IdiomaIds)).ToList();

        inversor.NombreCompleto = request.NombreCompleto;
        inversor.Empresa = request.Empresa;
        inversor.PaisOrigen = request.PaisOrigen;
        inversor.FechaInicioVisita = request.FechaInicioVisita;
        inversor.FechaFinVisita = request.FechaFinVisita;
        inversor.LugarHospedaje = request.LugarHospedaje;
        inversor.InversoresIdiomas = idiomas.Select(i => new InversorIdioma
        {
            InversorId = inversor.Id,
            IdiomaId = i.Id
        }).ToList();

        await inversorRepository.UpdateAsync(inversor);

        return new InversorDto(inversor.Id, inversor.NombreCompleto, inversor.Empresa,
            inversor.PaisOrigen, inversor.FechaInicioVisita, inversor.FechaFinVisita,
            inversor.LugarHospedaje,
            idiomas.Select(i => new IdiomaDto(i.Id, i.Nombre, i.Codigo)));
    }
}
