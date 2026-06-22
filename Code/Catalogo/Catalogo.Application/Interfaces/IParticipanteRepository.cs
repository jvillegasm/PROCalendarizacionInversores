using Catalogo.Domain.Entities;

namespace Catalogo.Application.Interfaces;

/// <summary>
/// Contrato del repositorio de participantes.
/// </summary>
public interface IParticipanteRepository
{
    /// <summary>Obtiene todos los participantes con oficina e idiomas.</summary>
    Task<IEnumerable<Participante>> GetAllAsync();

    /// <summary>Obtiene participantes activos con disponibilidad e idiomas.</summary>
    Task<IEnumerable<Participante>> GetActivosAsync();

    /// <summary>Obtiene un participante por Id con todas sus relaciones.</summary>
    /// <param name="id">Id del participante.</param>
    Task<Participante?> GetByIdAsync(Guid id);

    /// <summary>Persiste un nuevo participante.</summary>
    Task AddAsync(Participante participante);

    /// <summary>Actualiza un participante y sus relaciones.</summary>
    Task UpdateAsync(Participante participante);

    /// <summary>Elimina un participante.</summary>
    Task DeleteAsync(Participante participante);

    /// <summary>Cuenta los participantes activos asignados a una oficina.</summary>
    /// <param name="oficinaId">Id de la oficina.</param>
    /// <returns>Cantidad de participantes activos en la oficina.</returns>
    Task<int> CountActivosByOficinaAsync(Guid oficinaId);
}
