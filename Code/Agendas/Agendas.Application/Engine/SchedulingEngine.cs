using Agendas.Application.DTOs;
using Agendas.Domain.Exceptions;

namespace Agendas.Application.Engine;

/// <summary>
/// Motor principal de scheduling de agendas de visita para inversores.
/// Implementa el algoritmo greedy descrito en el SPEC §8 para maximizar
/// la cantidad de reuniones programadas dentro de las restricciones operativas
/// de horario (RN-09, RN-10), almuerzo (RN-11), idioma (RN-12) y traslados (RN-13).
/// No accede a la base de datos directamente; toda la persistencia ocurre fuera.
/// </summary>
public class SchedulingEngine(
    ITravelTimeResolver travelTimeResolver) : ISchedulingEngine
{
    // RN-11: bloque de almuerzo inviolable
    private static readonly TimeSpan InicioAlmuerzo = new(12, 0, 0);
    private static readonly TimeSpan FinAlmuerzo = new(13, 0, 0);

    // RN-10: fin máximo del día
    private static readonly TimeSpan FinDia = new(17, 0, 0);

    /// <inheritdoc/>
    public AgendaResult Generar(
        List<CandidatoAgenda> candidatos,
        int duracionMinutos,
        int metaReuniones,
        Guid ultimaOficinaId,
        Dictionary<(Guid, Guid), int> matrizTraslados,
        IEnumerable<OficinaCatalogoDto> oficinas)
    {
        var duracion = TimeSpan.FromMinutes(duracionMinutos);
        var oficinasDict = oficinas.ToDictionary(o => o.Id);
        var reunionesAgendadas = new List<ReunionCalculada>();

        // Hora actual del inversor: inicia a las 08:00
        var horaActual = new TimeSpan(8, 0, 0); // RN-09: inicio mínimo
        var oficinaActualId = ultimaOficinaId;

        // Ordenar candidatos por hora de inicio de su primer slot (ascendente)
        var candidatosOrdenados = candidatos
            .Where(c => c.SlotsDisponibles.Count > 0)
            .OrderBy(c => c.SlotsDisponibles.Min(s => s.Inicio))
            .ToList();

        foreach (var candidato in candidatosOrdenados)
        {
            if (reunionesAgendadas.Count >= metaReuniones) break; // Meta alcanzada

            // Intentar ubicar la reunión en algún slot disponible del candidato
            var reunionUbicada = IntentarUbicarReunion(
                candidato, duracion, horaActual, oficinaActualId,
                matrizTraslados, travelTimeResolver);

            if (reunionUbicada is null) continue; // Candidato no encaja; siguiente

            var (horaInicioR, horaFinR) = reunionUbicada.Value;
            var idioma = candidato.Participante.Idiomas.FirstOrDefault()?.Nombre ?? "Español";

            // Resolver datos de la oficina
            oficinasDict.TryGetValue(candidato.Participante.OficinaId, out var oficina);

            reunionesAgendadas.Add(new ReunionCalculada(
                ParticipanteId: candidato.Participante.Id,
                ParticipanteNombre: candidato.Participante.NombreCompleto,
                ParticipanteCargo: candidato.Participante.Cargo,
                OficinaId: candidato.Participante.OficinaId,
                OficinaNombre: oficina?.Nombre ?? string.Empty,
                OficinaDir: oficina?.DireccionFisica ?? string.Empty,
                HoraInicio: horaInicioR,
                HoraFin: horaFinR,
                IdiomaReunion: idioma,
                TiempoTrasladoSiguiente: 0)); // Se actualizará al final

            horaActual = horaFinR;
            oficinaActualId = candidato.Participante.OficinaId;
        }

        // Calcular tiempos de traslado entre reuniones consecutivas
        for (var i = 0; i < reunionesAgendadas.Count - 1; i++)
        {
            var trasladoAl = travelTimeResolver.Resolver(
                reunionesAgendadas[i].OficinaId,
                reunionesAgendadas[i + 1].OficinaId,
                matrizTraslados);

            reunionesAgendadas[i] = reunionesAgendadas[i] with { TiempoTrasladoSiguiente = trasladoAl };
        }

        return new AgendaResult(
            Reuniones: reunionesAgendadas,
            ReunionesLogradas: reunionesAgendadas.Count,
            MetaSolicitada: metaReuniones,
            Completa: reunionesAgendadas.Count >= metaReuniones);
    }

    /// <summary>
    /// Intenta ubicar una reunión del candidato en alguno de sus slots disponibles,
    /// considerando el tiempo de traslado desde la oficina actual y las restricciones RN-09 a RN-14.
    /// </summary>
    /// <returns>Datos de ubicación (inicio, fin) si encaja; null en caso contrario.</returns>
    private static (TimeSpan HoraInicio, TimeSpan HoraFin)? IntentarUbicarReunion(
        CandidatoAgenda candidato,
        TimeSpan duracion,
        TimeSpan horaActual,
        Guid oficinaActualId,
        Dictionary<(Guid, Guid), int> matrizTraslados,
        ITravelTimeResolver travelResolver)
    {
        var tiempoTraslado = TimeSpan.FromMinutes(
            travelResolver.Resolver(oficinaActualId, candidato.Participante.OficinaId, matrizTraslados));

        foreach (var slot in candidato.SlotsDisponibles)
        {
            // Inicio real: máximo entre (hora actual + traslado) y el inicio del slot
            var horaInicioReal = horaActual + tiempoTraslado;
            if (horaInicioReal < slot.Inicio) horaInicioReal = slot.Inicio;

            var horaFinReal = horaInicioReal + duracion;

            // RN-10: no puede finalizar después de las 17:00
            if (horaFinReal > FinDia) continue;

            // Verificar que la reunión cabe dentro del slot del participante
            if (horaFinReal > slot.Fin) continue;

            // RN-11: no puede solaparse con el bloque de almuerzo 12:00–13:00
            if (SolapaCon(horaInicioReal, horaFinReal, InicioAlmuerzo, FinAlmuerzo)) continue;

            return (horaInicioReal, horaFinReal);
        }

        return null; // No se encontró slot válido para este candidato
    }

    /// <summary>
    /// Verifica si el intervalo [inicio1, fin1) se solapa con [inicio2, fin2).
    /// </summary>
    private static bool SolapaCon(TimeSpan inicio1, TimeSpan fin1, TimeSpan inicio2, TimeSpan fin2) =>
        inicio1 < fin2 && fin1 > inicio2;
}
