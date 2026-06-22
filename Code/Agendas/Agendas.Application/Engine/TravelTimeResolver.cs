namespace Agendas.Application.Engine;

/// <summary>
/// Resuelve el tiempo de traslado entre dos oficinas desde la matriz precargada en memoria.
/// RN-13: el intervalo entre reuniones debe ser ≥ al tiempo de traslado.
/// </summary>
public class TravelTimeResolver : ITravelTimeResolver
{
    /// <inheritdoc/>
    public int Resolver(Guid origenId, Guid destinoId, Dictionary<(Guid, Guid), int> matriz)
    {
        // Sin desplazamiento si es la misma oficina
        if (origenId == destinoId) return 0;

        // Consultar la matriz; 0 si el par no está registrado
        return matriz.TryGetValue((origenId, destinoId), out var minutos) ? minutos : 0;
    }
}
