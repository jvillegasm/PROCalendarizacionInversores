using Catalogo.Application.DTOs;
using Catalogo.Application.Interfaces;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Exceptions;

namespace Catalogo.Application.UseCases.Inversores;

/// <summary>
/// Maneja el caso de uso CU-01: registrar un nuevo inversor.
/// Aplica RN-01 (idiomas obligatorios) y RN-02 (fechas válidas).
/// </summary>
public class RegistrarInversorHandler(
    IInversorRepository inversorRepository,
    IIdiomaRepository idiomaRepository)
{
    /// <summary>
    /// Ejecuta la lógica de registro del inversor.
    /// </summary>
    /// <param name="request">Datos del inversor a registrar.</param>
    /// <returns>DTO con el inversor creado incluyendo el Id generado.</returns>
    /// <exception cref="IdiomaRequeridoException">Si no se proporcionó ningún idioma (RN-01).</exception>
    /// <exception cref="FechaVisitaInvalidaException">Si FechaFinVisita &lt; FechaInicioVisita (RN-02).</exception>
    public async Task<InversorDto> HandleAsync(CrearInversorRequest request)
    {
        // RN-01: al menos un idioma requerido
        if (!request.IdiomaIds.Any())
            throw new IdiomaRequeridoException();

        // RN-02: fechas válidas
        if (request.FechaFinVisita < request.FechaInicioVisita)
            throw new FechaVisitaInvalidaException();

        var idiomas = (await idiomaRepository.GetByIdsAsync(request.IdiomaIds)).ToList();

        var inversor = new Inversor
        {
            Id = Guid.NewGuid(),
            NombreCompleto = request.NombreCompleto,
            Empresa = request.Empresa,
            PaisOrigen = request.PaisOrigen,
            FechaInicioVisita = request.FechaInicioVisita,
            FechaFinVisita = request.FechaFinVisita,
            LugarHospedaje = request.LugarHospedaje,
            InversoresIdiomas = idiomas.Select(i => new InversorIdioma { IdiomaId = i.Id }).ToList()
        };

        // Asignar InversorId a cada relación
        foreach (var ii in inversor.InversoresIdiomas)
            ii.InversorId = inversor.Id;

        await inversorRepository.AddAsync(inversor);

        return MapToDto(inversor, idiomas);
    }

    /// <summary>Mapea la entidad Inversor al DTO de respuesta.</summary>
    private static InversorDto MapToDto(Inversor inversor, List<Domain.Entities.Idioma> idiomas) =>
        new(inversor.Id, inversor.NombreCompleto, inversor.Empresa, inversor.PaisOrigen,
            inversor.FechaInicioVisita, inversor.FechaFinVisita, inversor.LugarHospedaje,
            idiomas.Select(i => new IdiomaDto(i.Id, i.Nombre, i.Codigo)));
}
