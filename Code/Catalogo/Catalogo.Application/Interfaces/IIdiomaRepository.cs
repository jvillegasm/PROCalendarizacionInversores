using Catalogo.Domain.Entities;

namespace Catalogo.Application.Interfaces;

/// <summary>
/// Contrato del repositorio de idiomas.
/// </summary>
public interface IIdiomaRepository
{
    /// <summary>Obtiene todos los idiomas disponibles en el catálogo.</summary>
    Task<IEnumerable<Idioma>> GetAllAsync();

    /// <summary>Obtiene idiomas por lista de Ids.</summary>
    /// <param name="ids">Ids de los idiomas a recuperar.</param>
    Task<IEnumerable<Idioma>> GetByIdsAsync(IEnumerable<Guid> ids);
}
