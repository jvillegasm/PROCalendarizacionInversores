using Catalogo.Domain.Entities;

namespace Catalogo.Application.Interfaces;

/// <summary>
/// Contrato del repositorio de inversores.
/// Implementado en la capa Infrastructure mediante Entity Framework Core 8.
/// </summary>
public interface IInversorRepository
{
    /// <summary>
    /// Obtiene todos los inversores con sus idiomas.
    /// </summary>
    /// <returns>Lista de inversores registrados.</returns>
    Task<IEnumerable<Inversor>> GetAllAsync();

    /// <summary>
    /// Obtiene un inversor por su Id incluyendo idiomas.
    /// </summary>
    /// <param name="id">Id del inversor a buscar.</param>
    /// <returns>El inversor o null si no existe.</returns>
    Task<Inversor?> GetByIdAsync(Guid id);

    /// <summary>
    /// Persiste un nuevo inversor con sus idiomas en la base de datos.
    /// </summary>
    /// <param name="inversor">Entidad a persistir.</param>
    Task AddAsync(Inversor inversor);

    /// <summary>
    /// Actualiza los datos de un inversor existente y sus relaciones de idioma.
    /// </summary>
    /// <param name="inversor">Entidad con los datos actualizados.</param>
    Task UpdateAsync(Inversor inversor);

    /// <summary>
    /// Elimina un inversor y sus relaciones de idioma de la base de datos.
    /// </summary>
    /// <param name="inversor">Entidad a eliminar.</param>
    Task DeleteAsync(Inversor inversor);
}
