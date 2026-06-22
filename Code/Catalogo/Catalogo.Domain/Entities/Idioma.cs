namespace Catalogo.Domain.Entities;

/// <summary>
/// Representa un idioma soportado por el sistema.
/// Pertenece al catálogo de idiomas disponibles para inversores y participantes.
/// </summary>
public class Idioma
{
    /// <summary>Identificador único del idioma (GUID).</summary>
    public Guid Id { get; set; }

    /// <summary>Nombre completo del idioma (ej. "Español", "Inglés").</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Código ISO del idioma (ej. "es", "en").</summary>
    public string Codigo { get; set; } = string.Empty;

    // Navegación
    /// <summary>Relaciones con inversores que manejan este idioma.</summary>
    public ICollection<InversorIdioma> InversoresIdiomas { get; set; } = new List<InversorIdioma>();

    /// <summary>Relaciones con participantes que dominan este idioma.</summary>
    public ICollection<ParticipanteIdioma> ParticipantesIdiomas { get; set; } = new List<ParticipanteIdioma>();
}
