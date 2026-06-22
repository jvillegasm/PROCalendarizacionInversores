using Agendas.Application.DTOs;

namespace Agendas.Application.Engine;

/// <summary>
/// Motor principal de scheduling de agendas.
/// Implementa el algoritmo greedy descrito en SPEC §8.
/// </summary>
public interface ISchedulingEngine
{
    /// <summary>
    /// Genera la secuencia óptima de reuniones para el inversor dado el conjunto de candidatos.
    /// </summary>
    /// <param name="candidatos">Participantes compatibles en idioma con sus slots disponibles.</param>
    /// <param name="duracionMinutos">Duración de cada reunión (igual para todas).</param>
    /// <param name="metaReuniones">Cantidad deseada de reuniones.</param>
    /// <param name="ultimaOficinaId">Oficina de partida del inversor (hospedaje o última reunión).</param>
    /// <param name="matrizTraslados">Diccionario (origen, destino) → minutos.</param>
    /// <param name="oficinas">Catálogo de oficinas para resolver nombre y dirección.</param>
    /// <returns>Resultado de la agenda con reuniones calculadas.</returns>
    AgendaResult Generar(
        List<CandidatoAgenda> candidatos,
        int duracionMinutos,
        int metaReuniones,
        Guid ultimaOficinaId,
        Dictionary<(Guid, Guid), int> matrizTraslados,
        IEnumerable<OficinaCatalogoDto> oficinas);
}

/// <summary>
/// Filtra la lista de candidatos conservando solo aquellos que comparten
/// al menos un idioma con el inversor.
/// RN-12.
/// </summary>
public interface ILanguageCompatibilityFilter
{
    /// <summary>
    /// Filtra los candidatos por compatibilidad de idioma con el inversor.
    /// </summary>
    /// <param name="candidatos">Lista de todos los candidatos seleccionados por el coordinador.</param>
    /// <param name="idiomasInversor">Ids de idiomas del inversor.</param>
    /// <returns>Subconjunto de candidatos compatibles.</returns>
    List<ParticipanteCatalogoDto> Filtrar(
        IEnumerable<ParticipanteCatalogoDto> candidatos,
        IEnumerable<Guid> idiomasInversor);
}

/// <summary>
/// Construye los slots horarios válidos de un participante para una fecha,
/// aplicando los límites 08:00–17:00 y excluyendo 12:00–13:00.
/// RN-09, RN-10, RN-11.
/// </summary>
public interface IAvailabilitySlotBuilder
{
    /// <summary>
    /// Retorna los slots de disponibilidad válidos del participante para la fecha indicada.
    /// </summary>
    /// <param name="participante">Participante con sus disponibilidades.</param>
    /// <param name="fecha">Fecha para la cual se calculan los slots.</param>
    /// <returns>Lista de slots válidos dentro del horario laboral.</returns>
    List<SlotDisponible> Construir(ParticipanteCatalogoDto participante, DateTime fecha);
}

/// <summary>
/// Resuelve el tiempo de traslado entre dos oficinas desde la matriz precargada en memoria.
/// RN-13.
/// </summary>
public interface ITravelTimeResolver
{
    /// <summary>
    /// Retorna el tiempo de traslado en minutos entre dos oficinas.
    /// Retorna 0 si son la misma oficina o si el par no existe en la matriz.
    /// </summary>
    /// <param name="origenId">Id de la oficina de origen.</param>
    /// <param name="destinoId">Id de la oficina de destino.</param>
    /// <param name="matriz">Diccionario (origen, destino) → minutos.</param>
    int Resolver(Guid origenId, Guid destinoId, Dictionary<(Guid, Guid), int> matriz);
}
