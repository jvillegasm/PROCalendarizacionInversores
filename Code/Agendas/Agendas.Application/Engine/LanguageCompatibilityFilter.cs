using Agendas.Application.DTOs;

namespace Agendas.Application.Engine;

/// <summary>
/// Implementación del filtro de compatibilidad de idioma.
/// RN-12: el inversor y el participante deben compartir al menos un idioma.
/// </summary>
public class LanguageCompatibilityFilter : ILanguageCompatibilityFilter
{
    /// <inheritdoc/>
    public List<ParticipanteCatalogoDto> Filtrar(
        IEnumerable<ParticipanteCatalogoDto> candidatos,
        IEnumerable<Guid> idiomasInversor)
    {
        var setIdiomas = idiomasInversor.ToHashSet();

        // RN-12: conservar solo candidatos que comparten al menos un idioma con el inversor
        return candidatos
            .Where(c => c.Idiomas.Any(i => setIdiomas.Contains(i.Id)))
            .ToList();
    }
}
