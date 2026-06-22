using Catalogo.Domain.Entities;

namespace Catalogo.Application.Interfaces;

/// <summary>
/// Contrato del repositorio de oficinas.
/// </summary>
public interface IOficinaRepository
{
    /// <summary>Obtiene todas las oficinas registradas.</summary>
    Task<IEnumerable<Oficina>> GetAllAsync();

    /// <summary>Obtiene una oficina por Id.</summary>
    Task<Oficina?> GetByIdAsync(Guid id);

    /// <summary>Persiste una nueva oficina.</summary>
    Task AddAsync(Oficina oficina);

    /// <summary>Actualiza los datos de una oficina existente.</summary>
    Task UpdateAsync(Oficina oficina);

    /// <summary>Elimina una oficina y sus pares de traslado en cascada.</summary>
    Task DeleteAsync(Oficina oficina);
}
