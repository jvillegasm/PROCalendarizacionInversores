using Agendas.Application.DTOs;

namespace Agendas.Application.Engine;

/// <summary>
/// Slot horario disponible de un participante para una fecha específica.
/// Ya filtrado por AvailabilitySlotBuilder para respetar RN-09, RN-10 y RN-11.
/// </summary>
public record SlotDisponible(TimeSpan Inicio, TimeSpan Fin);

/// <summary>
/// Candidato a reunión con sus slots disponibles para el día de la agenda.
/// </summary>
public record CandidatoAgenda(
    ParticipanteCatalogoDto Participante,
    List<SlotDisponible> SlotsDisponibles);

/// <summary>
/// Reunión calculada por el motor de scheduling antes de persistirla.
/// </summary>
public record ReunionCalculada(
    Guid ParticipanteId,
    string ParticipanteNombre,
    string ParticipanteCargo,
    Guid OficinaId,
    string OficinaNombre,
    string OficinaDir,
    TimeSpan HoraInicio,
    TimeSpan HoraFin,
    string IdiomaReunion,
    int TiempoTrasladoSiguiente);

/// <summary>
/// Resultado del proceso de scheduling.
/// Puede ser una agenda completa o parcial según si se alcanzó la meta.
/// </summary>
public record AgendaResult(
    List<ReunionCalculada> Reuniones,
    int ReunionesLogradas,
    int MetaSolicitada,
    bool Completa);
