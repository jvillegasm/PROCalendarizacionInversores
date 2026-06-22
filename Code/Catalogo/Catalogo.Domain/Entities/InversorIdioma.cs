namespace Catalogo.Domain.Entities;

/// <summary>
/// Tabla de unión N:M entre <see cref="Inversor"/> e <see cref="Idioma"/>.
/// RN-01: un inversor debe tener al menos una entrada en esta tabla.
/// </summary>
public class InversorIdioma
{
    /// <summary>Identificador del inversor propietario.</summary>
    public Guid InversorId { get; set; }

    /// <summary>Identificador del idioma asignado.</summary>
    public Guid IdiomaId { get; set; }

    // Navegación
    /// <summary>Inversor relacionado.</summary>
    public Inversor Inversor { get; set; } = null!;

    /// <summary>Idioma relacionado.</summary>
    public Idioma Idioma { get; set; } = null!;
}
