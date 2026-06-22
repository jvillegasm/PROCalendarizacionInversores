using Catalogo.Application.DTOs;
using Catalogo.Application.Interfaces;
using Catalogo.Domain.Exceptions;

namespace Catalogo.Application.UseCases.Inversores;

/// <summary>
/// Maneja la consulta del listado de inversores y el detalle de uno específico.
/// </summary>
public class ConsultarInversoresHandler(IInversorRepository inversorRepository)
{
    /// <summary>Obtiene todos los inversores registrados.</summary>
    /// <returns>Lista de DTOs de inversores.</returns>
    public async Task<IEnumerable<InversorDto>> GetAllAsync()
    {
        var inversores = await inversorRepository.GetAllAsync();
        return inversores.Select(i => new InversorDto(
            i.Id, i.NombreCompleto, i.Empresa, i.PaisOrigen,
            i.FechaInicioVisita, i.FechaFinVisita, i.LugarHospedaje,
            i.InversoresIdiomas.Select(ii => new IdiomaDto(
                ii.Idioma.Id, ii.Idioma.Nombre, ii.Idioma.Codigo))));
    }

    /// <summary>Obtiene el detalle de un inversor por Id.</summary>
    /// <param name="id">Id del inversor.</param>
    /// <returns>DTO del inversor.</returns>
    /// <exception cref="InversorNotFoundException">Si no existe.</exception>
    public async Task<InversorDto> GetByIdAsync(Guid id)
    {
        var inversor = await inversorRepository.GetByIdAsync(id)
            ?? throw new InversorNotFoundException(id);

        return new InversorDto(
            inversor.Id, inversor.NombreCompleto, inversor.Empresa, inversor.PaisOrigen,
            inversor.FechaInicioVisita, inversor.FechaFinVisita, inversor.LugarHospedaje,
            inversor.InversoresIdiomas.Select(ii => new IdiomaDto(
                ii.Idioma.Id, ii.Idioma.Nombre, ii.Idioma.Codigo)));
    }
}
