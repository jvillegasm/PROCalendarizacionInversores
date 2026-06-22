namespace Catalogo.Application.Interfaces;

/// <summary>
/// Verifica si un inversor tiene agendas activas consultando la tabla Agendas
/// directamente en la misma base de datos, evitando dependencia HTTP circular
/// entre Catálogo Service y Agendas Service.
/// </summary>
public interface IAgendaStatusChecker
{
    /// <summary>
    /// Indica si el inversor tiene al menos una agenda en estado Activa.
    /// RN-03: condiciona la eliminación del inversor.
    /// </summary>
    /// <param name="inversorId">Id del inversor a verificar.</param>
    /// <returns>True si tiene agendas activas; false en caso contrario.</returns>
    Task<bool> TieneAgendasActivasAsync(Guid inversorId);
}
