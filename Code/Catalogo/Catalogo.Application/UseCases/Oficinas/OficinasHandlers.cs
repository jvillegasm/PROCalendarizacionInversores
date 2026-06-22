using Catalogo.Application.DTOs;
using Catalogo.Application.Interfaces;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Exceptions;

namespace Catalogo.Application.UseCases.Oficinas;

/// <summary>Maneja el registro de una nueva oficina (CU-03).</summary>
public class RegistrarOficinaHandler(IOficinaRepository oficinaRepository)
{
    /// <summary>
    /// Crea una nueva oficina.
    /// </summary>
    /// <param name="request">Datos de la oficina.</param>
    /// <returns>DTO de la oficina creada.</returns>
    public async Task<OficinaDto> HandleAsync(CrearOficinaRequest request)
    {
        var oficina = new Oficina
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            DireccionFisica = request.DireccionFisica,
            Coordenadas = request.Coordenadas
        };

        await oficinaRepository.AddAsync(oficina);

        return new OficinaDto(oficina.Id, oficina.Nombre, oficina.DireccionFisica, oficina.Coordenadas);
    }
}

/// <summary>Maneja la consulta de oficinas.</summary>
public class ConsultarOficinasHandler(IOficinaRepository oficinaRepository)
{
    /// <summary>Obtiene todas las oficinas registradas.</summary>
    public async Task<IEnumerable<OficinaDto>> GetAllAsync()
    {
        var oficinas = await oficinaRepository.GetAllAsync();
        return oficinas.Select(o => new OficinaDto(o.Id, o.Nombre, o.DireccionFisica, o.Coordenadas));
    }

    /// <summary>Obtiene el detalle de una oficina por su Id.</summary>
    public async Task<OficinaDto> GetByIdAsync(Guid id)
    {
        var oficina = await oficinaRepository.GetByIdAsync(id)
            ?? throw new OficinaNotFoundException(id);
        return new OficinaDto(oficina.Id, oficina.Nombre, oficina.DireccionFisica, oficina.Coordenadas);
    }
}

/// <summary>
/// Maneja la eliminación de una oficina (FA-03 de CU-03).
/// RN-06: bloquea si tiene participantes activos.
/// </summary>
public class EliminarOficinaHandler(
    IOficinaRepository oficinaRepository,
    IParticipanteRepository participanteRepository)
{
    /// <summary>
    /// Elimina la oficina si no tiene participantes activos.
    /// </summary>
    /// <param name="id">Id de la oficina.</param>
    /// <exception cref="OficinaNotFoundException">Si no existe.</exception>
    /// <exception cref="OficinaConParticipantesActivosException">RN-06.</exception>
    public async Task HandleAsync(Guid id)
    {
        var oficina = await oficinaRepository.GetByIdAsync(id)
            ?? throw new OficinaNotFoundException(id);

        // RN-06: verificar participantes activos
        var activos = await participanteRepository.CountActivosByOficinaAsync(id);
        if (activos > 0)
            throw new OficinaConParticipantesActivosException();

        await oficinaRepository.DeleteAsync(oficina);
    }
}

/// <summary>Maneja la actualización de los datos de una oficina.</summary>
public class ActualizarOficinaHandler(IOficinaRepository oficinaRepository)
{
    /// <summary>Actualiza los datos de la oficina identificada por <paramref name="id"/>.</summary>
    /// <exception cref="OficinaNotFoundException">Si la oficina no existe.</exception>
    public async Task<OficinaDto> HandleAsync(Guid id, CrearOficinaRequest request)
    {
        var oficina = await oficinaRepository.GetByIdAsync(id)
            ?? throw new OficinaNotFoundException(id);

        oficina.Nombre = request.Nombre;
        oficina.DireccionFisica = request.DireccionFisica;
        oficina.Coordenadas = request.Coordenadas;

        await oficinaRepository.UpdateAsync(oficina);

        return new OficinaDto(oficina.Id, oficina.Nombre, oficina.DireccionFisica, oficina.Coordenadas);
    }
}
