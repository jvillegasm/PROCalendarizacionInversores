namespace Agendas.Domain.Enums;

/// <summary>
/// Estado posible de una agenda de visita de inversor.
/// RN-15: la anulación es siempre lógica; el registro histórico se conserva.
/// </summary>
public enum EstadoAgenda
{
    /// <summary>Agenda generada y vigente.</summary>
    Activa,

    /// <summary>Agenda anulada lógicamente. El registro y su PDF se conservan (RN-15).</summary>
    Anulada
}
