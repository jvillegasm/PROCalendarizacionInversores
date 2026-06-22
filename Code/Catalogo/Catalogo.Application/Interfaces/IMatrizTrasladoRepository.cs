using Catalogo.Domain.Entities;

namespace Catalogo.Application.Interfaces;

/// <summary>
/// Contrato del repositorio de la matriz de traslados.
/// RN-07: garantiza simetría A→B = B→A.
/// </summary>
public interface IMatrizTrasladoRepository
{
    /// <summary>Obtiene todos los pares de traslado registrados.</summary>
    Task<IEnumerable<MatrizTraslado>> GetAllAsync();

    /// <summary>Obtiene un traslado por su Id.</summary>
    Task<MatrizTraslado?> GetByIdAsync(Guid id);

    /// <summary>Verifica si ya existe un par de traslado entre dos oficinas.</summary>
    Task<bool> ExisteParAsync(Guid origenId, Guid destinoId);

    /// <summary>
    /// Persiste los dos pares simétricos (A→B y B→A) en la misma transacción.
    /// RN-07: garantía de simetría aplicada en esta capa.
    /// </summary>
    Task AddParSimetricoAsync(MatrizTraslado directo, MatrizTraslado inverso);

    /// <summary>
    /// Actualiza el tiempo de traslado de un par y su simétrico.
    /// </summary>
    Task UpdateParSimetricoAsync(Guid origenId, Guid destinoId, int nuevoTiempoMinutos);

    /// <summary>
    /// Elimina el traslado identificado por <paramref name="id"/> y su par simétrico.
    /// </summary>
    Task DeleteParAsync(Guid id);
}
