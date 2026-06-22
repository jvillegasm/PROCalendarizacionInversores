using Agendas.Application.DTOs;
using Agendas.Domain.Entities;

namespace Agendas.Application.Interfaces;

/// <summary>
/// Contrato del repositorio de agendas.
/// Implementado en la capa Infrastructure con Entity Framework Core.
/// </summary>
public interface IAgendaRepository
{
    /// <summary>
    /// Obtiene todas las agendas aplicando filtros opcionales.
    /// </summary>
    /// <param name="filtros">Filtros de inversor, fecha y estado.</param>
    Task<IEnumerable<Agenda>> GetAllAsync(FiltrosAgendaQuery filtros);

    /// <summary>Obtiene el detalle completo de una agenda por Id.</summary>
    Task<Agenda?> GetByIdAsync(Guid id);

    /// <summary>Persiste una nueva agenda con todas sus reuniones.</summary>
    Task AddAsync(Agenda agenda);

    /// <summary>Actualiza el estado de una agenda (anulación lógica RN-15).</summary>
    Task UpdateAsync(Agenda agenda);
}
