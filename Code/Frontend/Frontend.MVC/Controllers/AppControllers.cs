using Frontend.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.MVC.Controllers;

/// <summary>
/// Controlador de inversores. Retorna la vista SPA; los datos se cargan
/// vía AJAX desde el cliente directamente contra el Catálogo Service.
/// </summary>
public class InversoresController : Controller
{
    public IActionResult Index() => View();
}

/// <summary>
/// Controlador de participantes, oficinas y matriz de traslados. Retorna la
/// vista SPA; los datos se cargan vía AJAX directamente contra el Catálogo Service.
/// </summary>
public class ParticipantesController : Controller
{
    public IActionResult Index() => View();
}

/// <summary>
/// Controlador de agendas: generación, listado, detalle y descarga de PDF.
/// La descarga de PDF se sirve desde el servidor (MVC → Agendas Service → PDF Service).
/// </summary>
public class AgendasController(AgendasApiService agendasService) : Controller
{
    public IActionResult Generar() => View();
    public IActionResult Index() => View();

    /// <summary>Descarga el PDF de una agenda desde el Agendas Service.</summary>
    [HttpGet]
    public async Task<IActionResult> DescargarPdf(Guid id)
    {
        var agenda = await agendasService.GetAgendaAsync(id);
        if (agenda is null) return NotFound();

        var pdfBytes = await agendasService.GetPdfAsync(id);
        if (pdfBytes is null) return StatusCode(503, "PDF Service no disponible.");

        var nombre = $"Agenda_{agenda.Fecha:yyyy-MM-dd}_{agenda.InversorNombre.Replace(" ", "_")}.pdf";
        return File(pdfBytes, "application/pdf", nombre);
    }
}


