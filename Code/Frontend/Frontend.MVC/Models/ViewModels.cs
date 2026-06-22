namespace Frontend.MVC.Models;

/// <summary>DTO del inversor para el frontend.</summary>
public class InversorViewModel
{
    public Guid Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Empresa { get; set; } = string.Empty;
    public string PaisOrigen { get; set; } = string.Empty;
    public DateTime FechaInicioVisita { get; set; }
    public DateTime FechaFinVisita { get; set; }
    public string LugarHospedaje { get; set; } = string.Empty;
    public IEnumerable<IdiomaViewModel> Idiomas { get; set; } = [];
}

public class IdiomaViewModel
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
}

public class ParticipanteViewModel
{
    public Guid Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public Guid OficinaId { get; set; }
    public string OficinaNombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public IEnumerable<IdiomaViewModel> Idiomas { get; set; } = [];
    public IEnumerable<DisponibilidadViewModel> Disponibilidades { get; set; } = [];
}

public class DisponibilidadViewModel
{
    public Guid Id { get; set; }
    public DateTime Fecha { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFin { get; set; }
}

public class OficinaViewModel
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string DireccionFisica { get; set; } = string.Empty;
    public string? Coordenadas { get; set; }
}

public class MatrizTrasladoViewModel
{
    public Guid Id { get; set; }
    public Guid OficinaOrigenId { get; set; }
    public string OficinaOrigenNombre { get; set; } = string.Empty;
    public Guid OficinaDestinoId { get; set; }
    public string OficinaDestinoNombre { get; set; } = string.Empty;
    public int TiempoMinutos { get; set; }
}

public class AgendaResumenViewModel
{
    public Guid Id { get; set; }
    public Guid InversorId { get; set; }
    public string InversorNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int CantidadReuniones { get; set; }
    public DateTime FechaGeneracion { get; set; }
}

public class AgendaDetalleViewModel
{
    public Guid Id { get; set; }
    public Guid InversorId { get; set; }
    public string InversorNombre { get; set; } = string.Empty;
    public string InversorEmpresa { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaGeneracion { get; set; }
    public DateTime? FechaAnulacion { get; set; }
    public int ReunionesLogradas { get; set; }
    public int MetaSolicitada { get; set; }
    public bool Completa { get; set; }
    public IEnumerable<ReunionViewModel> Reuniones { get; set; } = [];
}

public class ReunionViewModel
{
    public Guid Id { get; set; }
    public int Orden { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFin { get; set; }
    public string ParticipanteNombre { get; set; } = string.Empty;
    public string ParticipanteCargo { get; set; } = string.Empty;
    public string OficinaNombre { get; set; } = string.Empty;
    public string OficinaDir { get; set; } = string.Empty;
    public string IdiomaReunion { get; set; } = string.Empty;
    public int TiempoTrasladoSiguiente { get; set; }
}
