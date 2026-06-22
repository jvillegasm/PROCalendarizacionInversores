namespace Catalogo.Domain.Entities;

/// <summary>
/// Representa un participante (funcionario o aliado) que puede ser convocado a reuniones con inversores.
/// RN-04: debe tener al menos un idioma. RN-05: debe tener exactamente una oficina.
/// </summary>
public class Participante
{
    /// <summary>Identificador único del participante (GUID).</summary>
    public Guid Id { get; set; }

    /// <summary>Nombre completo del funcionario o aliado.</summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>Cargo o institución a la que pertenece.</summary>
    public string Cargo { get; set; } = string.Empty;

    /// <summary>
    /// Oficina donde habitualmente atiende reuniones.
    /// RN-05: debe tener exactamente una oficina asignada.
    /// </summary>
    public Guid OficinaId { get; set; }

    /// <summary>
    /// Estado del participante: true = Activo, false = Inactivo.
    /// Los participantes inactivos quedan excluidos del scheduling (CU-04).
    /// </summary>
    public bool Activo { get; set; } = true;

    // Navegación
    /// <summary>Oficina asignada.</summary>
    public Oficina Oficina { get; set; } = null!;

    /// <summary>
    /// Idiomas que domina el participante.
    /// RN-04: debe tener al menos uno.
    /// </summary>
    public ICollection<ParticipanteIdioma> ParticipantesIdiomas { get; set; } = new List<ParticipanteIdioma>();

    /// <summary>Bloques de disponibilidad horaria organizados por fecha.</summary>
    public ICollection<DisponibilidadParticipante> Disponibilidades { get; set; } = new List<DisponibilidadParticipante>();
}
