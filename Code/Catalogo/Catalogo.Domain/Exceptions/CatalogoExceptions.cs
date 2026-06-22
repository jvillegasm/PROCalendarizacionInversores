namespace Catalogo.Domain.Exceptions;

/// <summary>
/// Se lanza cuando se intenta registrar o actualizar un inversor sin asignar ningún idioma.
/// RN-01: el inversor debe tener al menos un idioma asignado.
/// </summary>
public class IdiomaRequeridoException() : DomainException("El inversor debe tener al menos un idioma asignado.");

/// <summary>
/// Se lanza cuando la FechaFinVisita es anterior a FechaInicioVisita.
/// RN-02: la fecha de cierre no puede ser anterior a la de inicio.
/// </summary>
public class FechaVisitaInvalidaException()
    : DomainException("La fecha de cierre no puede ser anterior a la fecha de inicio.");

/// <summary>
/// Se lanza cuando se intenta eliminar un inversor que tiene agendas en estado Activa.
/// RN-03: primero se deben anular las agendas activas.
/// </summary>
public class InversorConAgendasActivasException()
    : DomainException("No es posible eliminar un inversor con agendas activas.");

/// <summary>
/// Se lanza cuando no se encuentra un inversor con el Id proporcionado.
/// </summary>
public class InversorNotFoundException(Guid id)
    : DomainException($"No se encontró el inversor con Id '{id}'.");

/// <summary>
/// Se lanza cuando no se encuentra una oficina con el Id proporcionado.
/// </summary>
public class OficinaNotFoundException(Guid id)
    : DomainException($"No se encontró la oficina con Id '{id}'.");

/// <summary>
/// Se lanza cuando se intenta eliminar una oficina que tiene participantes activos asignados.
/// RN-06: la oficina no puede eliminarse mientras tenga participantes activos.
/// </summary>
public class OficinaConParticipantesActivosException()
    : DomainException("No es posible eliminar una oficina con participantes activos asignados.");

/// <summary>
/// Se lanza cuando no se encuentra un participante con el Id proporcionado.
/// </summary>
public class ParticipanteNotFoundException(Guid id)
    : DomainException($"No se encontró el participante con Id '{id}'.");

/// <summary>
/// Se lanza cuando se intenta registrar o actualizar un participante sin idiomas.
/// RN-04: el participante debe tener al menos un idioma asignado.
/// </summary>
public class IdiomaRequeridoParticipanteException()
    : DomainException("El participante debe tener al menos un idioma asignado.");

/// <summary>
/// Se lanza cuando se intenta registrar o actualizar un participante sin oficina.
/// RN-05: el participante debe tener exactamente una oficina asignada.
/// </summary>
public class OficinaRequeridaException()
    : DomainException("El participante debe tener una oficina asignada (RN-05).");

/// <summary>
/// Se lanza cuando no se encuentra un traslado con el Id proporcionado.
/// </summary>
public class TrasladoNotFoundException(Guid id)
    : DomainException($"No se encontró el traslado con Id '{id}'.");
