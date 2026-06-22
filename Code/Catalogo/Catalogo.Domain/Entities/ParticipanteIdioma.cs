namespace Catalogo.Domain.Entities;

/// <summary>
/// Tabla de unión N:M entre <see cref="Participante"/> e <see cref="Idioma"/>.
/// RN-04: un participante debe tener al menos una entrada en esta tabla.
/// </summary>
public class ParticipanteIdioma
{
    /// <summary>Identificador del participante.</summary>
    public Guid ParticipanteId { get; set; }

    /// <summary>Identificador del idioma.</summary>
    public Guid IdiomaId { get; set; }

    // Navegación
    /// <summary>Participante relacionado.</summary>
    public Participante Participante { get; set; } = null!;

    /// <summary>Idioma relacionado.</summary>
    public Idioma Idioma { get; set; } = null!;
}
