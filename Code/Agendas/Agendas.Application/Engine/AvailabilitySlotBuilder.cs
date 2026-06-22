using Agendas.Application.DTOs;

namespace Agendas.Application.Engine;

/// <summary>
/// Construye los slots de disponibilidad válidos de un participante para una fecha específica.
/// Aplica el horario laboral 08:00–17:00 y excluye el bloque de almuerzo 12:00–13:00.
/// RN-09: inicio mínimo 08:00.
/// RN-10: fin máximo 17:00.
/// RN-11: bloque 12:00–13:00 inviolable (se parte el slot si lo contiene).
/// </summary>
public class AvailabilitySlotBuilder : IAvailabilitySlotBuilder
{
    // RN-09: hora de inicio laboral mínima
    private static readonly TimeSpan HoraInicio = new(8, 0, 0);

    // RN-10: hora de fin laboral máxima
    private static readonly TimeSpan HoraFin = new(17, 0, 0);

    // RN-11: inicio del bloque de almuerzo inviolable
    private static readonly TimeSpan InicioAlmuerzo = new(12, 0, 0);

    // RN-11: fin del bloque de almuerzo inviolable
    private static readonly TimeSpan FinAlmuerzo = new(13, 0, 0);

    /// <inheritdoc/>
    public List<SlotDisponible> Construir(ParticipanteCatalogoDto participante, DateTime fecha)
    {
        // Obtener bloques del participante para la fecha exacta
        var bloquesDelDia = participante.Disponibilidades
            .Where(d => d.Fecha.Date == fecha.Date)
            .ToList();

        var slots = new List<SlotDisponible>();

        foreach (var bloque in bloquesDelDia)
        {
            // Recortar al horario laboral (RN-09, RN-10)
            var inicio = bloque.HoraInicio < HoraInicio ? HoraInicio : bloque.HoraInicio;
            var fin = bloque.HoraFin > HoraFin ? HoraFin : bloque.HoraFin;

            if (inicio >= fin) continue; // Bloque inválido tras recorte

            // RN-11: si el bloque intersecta con el almuerzo, partirlo en dos
            if (inicio < FinAlmuerzo && fin > InicioAlmuerzo)
            {
                // Parte antes del almuerzo
                if (inicio < InicioAlmuerzo)
                    slots.Add(new SlotDisponible(inicio, InicioAlmuerzo));

                // Parte después del almuerzo
                if (fin > FinAlmuerzo)
                    slots.Add(new SlotDisponible(FinAlmuerzo, fin));
            }
            else
            {
                slots.Add(new SlotDisponible(inicio, fin));
            }
        }

        return slots;
    }
}
