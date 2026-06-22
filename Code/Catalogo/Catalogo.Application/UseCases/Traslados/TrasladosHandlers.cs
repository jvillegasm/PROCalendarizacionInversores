using Catalogo.Application.DTOs;
using Catalogo.Application.Interfaces;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Exceptions;

namespace Catalogo.Application.UseCases.Traslados;

/// <summary>
/// Maneja el registro de un par de traslado entre dos oficinas (CU-03).
/// RN-07: garantiza simetría automática persitiendo el par directo e inverso.
/// </summary>
public class RegistrarTrasladoHandler(
    IMatrizTrasladoRepository trasladoRepository,
    IOficinaRepository oficinaRepository)
{
    /// <summary>
    /// Registra el par de traslado A→B y automáticamente B→A con el mismo tiempo.
    /// </summary>
    /// <param name="request">Datos del traslado (origenId, destinoId, minutos).</param>
    /// <returns>Lista con los dos pares creados.</returns>
    /// <exception cref="OficinaNotFoundException">Si alguna oficina no existe.</exception>
    public async Task<IEnumerable<MatrizTrasladoDto>> HandleAsync(CrearTrasladoRequest request)
    {
        // Verificar que ambas oficinas existen
        var origen = await oficinaRepository.GetByIdAsync(request.OficinaOrigenId)
            ?? throw new OficinaNotFoundException(request.OficinaOrigenId);

        var destino = await oficinaRepository.GetByIdAsync(request.OficinaDestinoId)
            ?? throw new OficinaNotFoundException(request.OficinaDestinoId);

        var directo = new MatrizTraslado
        {
            Id = Guid.NewGuid(),
            OficinaOrigenId = request.OficinaOrigenId,
            OficinaDestinoId = request.OficinaDestinoId,
            TiempoMinutos = request.TiempoMinutos
        };

        // RN-07: par simétrico automático
        var inverso = new MatrizTraslado
        {
            Id = Guid.NewGuid(),
            OficinaOrigenId = request.OficinaDestinoId,
            OficinaDestinoId = request.OficinaOrigenId,
            TiempoMinutos = request.TiempoMinutos
        };

        await trasladoRepository.AddParSimetricoAsync(directo, inverso);

        return new[]
        {
            new MatrizTrasladoDto(directo.Id, directo.OficinaOrigenId, origen.Nombre, directo.OficinaDestinoId, destino.Nombre, directo.TiempoMinutos),
            new MatrizTrasladoDto(inverso.Id, inverso.OficinaOrigenId, destino.Nombre, inverso.OficinaDestinoId, origen.Nombre, inverso.TiempoMinutos)
        };
    }
}

/// <summary>Maneja la consulta de la matriz de traslados (FA-01 de CU-03).</summary>
public class ConsultarTrasladosHandler(IMatrizTrasladoRepository trasladoRepository)
{
    /// <summary>Obtiene todos los pares de traslado registrados.</summary>
    public async Task<IEnumerable<MatrizTrasladoDto>> GetAllAsync()
    {
        var traslados = await trasladoRepository.GetAllAsync();
        return traslados.Select(t => new MatrizTrasladoDto(
            t.Id,
            t.OficinaOrigenId, t.OficinaOrigen?.Nombre ?? string.Empty,
            t.OficinaDestinoId, t.OficinaDestino?.Nombre ?? string.Empty,
            t.TiempoMinutos));
    }
}

/// <summary>Actualiza el tiempo de traslado de un par y su simétrico.</summary>
public class ActualizarTrasladoHandler(IMatrizTrasladoRepository trasladoRepository)
{
    /// <exception cref="TrasladoNotFoundException">Si el traslado no existe.</exception>
    public async Task<MatrizTrasladoDto> HandleAsync(Guid id, ActualizarTrasladoRequest request)
    {
        var traslado = await trasladoRepository.GetByIdAsync(id)
            ?? throw new TrasladoNotFoundException(id);

        await trasladoRepository.UpdateParSimetricoAsync(
            traslado.OficinaOrigenId, traslado.OficinaDestinoId, request.TiempoMinutos);

        return new MatrizTrasladoDto(traslado.Id, traslado.OficinaOrigenId, string.Empty,
            traslado.OficinaDestinoId, string.Empty, request.TiempoMinutos);
    }
}

/// <summary>Elimina un par de traslado y su simétrico.</summary>
public class EliminarTrasladoHandler(IMatrizTrasladoRepository trasladoRepository)
{
    /// <summary>
    /// Elimina el traslado indicado y su par simétrico.
    /// Si el traslado ya fue eliminado (p.ej. al borrar el simétrico primero), no lanza error.
    /// </summary>
    public async Task HandleAsync(Guid id)
    {
        // DeleteParAsync maneja internamente el caso null (par ya eliminado).
        await trasladoRepository.DeleteParAsync(id);
    }
}
