using Microsoft.AspNetCore.Mvc;
using PDF.Application.UseCases;
using PDF.Domain;

namespace PDF.API.Controllers;

/// <summary>
/// Controlador REST para la generación de PDFs de itinerarios (CU-06 / AC-06).
/// Recibe los datos de la agenda y retorna el PDF en español (es-CR).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GenerarPdfController(GenerarPdfHandler handler) : ControllerBase
{
    /// <summary>
    /// Genera el PDF del itinerario de agenda.
    /// </summary>
    /// <param name="request">Datos completos de la agenda.</param>
    /// <returns>Archivo PDF application/pdf.</returns>
    [HttpPost("generar-pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Generar([FromBody] AgendaPdfRequest request)
    {
        var pdfBytes = handler.Handle(request);
        var nombreArchivo = $"Agenda_{request.Fecha:yyyy-MM-dd}_{request.InversorNombre.Replace(" ", "_")}.pdf";
        return File(pdfBytes, "application/pdf", nombreArchivo);
    }
}
