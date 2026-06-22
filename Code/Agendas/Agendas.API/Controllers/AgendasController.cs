using Agendas.Application.DTOs;
using Agendas.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Agendas.API.Controllers;

/// <summary>
/// Controlador REST para el módulo de agendas (CU-04, CU-05, CU-06).
/// Expone los endpoints de generación, consulta, anulación y PDF de agendas.
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class AgendasController(
    GenerarAgendaHandler generarHandler,
    ConsultarAgendasHandler consultarListadoHandler,
    ConsultarAgendaHandler consultarDetalleHandler,
    AnularAgendaHandler anularHandler,
    Agendas.Application.Interfaces.IPdfServiceHttpClient pdfClient) : ControllerBase
{
    /// <summary>
    /// Genera automáticamente una agenda para el inversor en la fecha indicada.
    /// CU-04 — POST /agendas/generar
    /// </summary>
    /// <param name="request">Inversor, candidatos, fecha, duración y meta de reuniones.</param>
    [HttpPost("generar")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(AgendaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Generar([FromBody] GenerarAgendaRequest request)
    {
        var result = await generarHandler.HandleAsync(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Lista las agendas existentes con filtros opcionales por inversor, fecha y estado.
    /// CU-05 — GET /agendas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AgendaResumenDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? inversorId,
        [FromQuery] DateTime? fecha,
        [FromQuery] string? estado)
    {
        var filtros = new FiltrosAgendaQuery(inversorId, fecha, estado);
        return Ok(await consultarListadoHandler.HandleAsync(filtros));
    }

    /// <summary>
    /// Obtiene el detalle completo de una agenda específica.
    /// CU-05 — GET /agendas/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AgendaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(await consultarDetalleHandler.HandleAsync(id));

    /// <summary>
    /// Anulación lógica de una agenda (soft delete).
    /// CU-05 — DELETE /agendas/{id}
    /// RN-15: el registro y su PDF se conservan para trazabilidad.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Anular(Guid id)
    {
        await anularHandler.HandleAsync(id);
        return Ok(new { message = "Agenda anulada correctamente.", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Descarga el PDF del itinerario de la agenda.
    /// CU-06 — GET /agendas/{id}/pdf
    /// RN-15: disponible incluso para agendas anuladas.
    /// </summary>
    [HttpGet("{id:guid}/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetPdf(Guid id)
    {
        var agenda = await consultarDetalleHandler.HandleAsync(id);

        // Preparar la solicitud al PDF Service con todos los datos de la agenda
        // Mapear ReunionDto → estructura que espera AgendaPdfRequest/ReunionPdfItem.
        // HoraInicio/HoraFin son TimeSpan en el dominio de Agendas y string en PDF.Domain.
        var pdfRequest = new
        {
            AgendaId = agenda.Id,
            InversorNombre = agenda.InversorNombre,
            InversorEmpresa = agenda.InversorEmpresa,
            Fecha = agenda.Fecha,
            Estado = agenda.Estado,
            Reuniones = agenda.Reuniones.Select(r => new
            {
                Orden = r.Orden,
                HoraInicio = r.HoraInicio.ToString(@"hh\:mm"),
                HoraFin = r.HoraFin.ToString(@"hh\:mm"),
                ParticipanteNombre = r.ParticipanteNombre,
                ParticipanteCargo = r.ParticipanteCargo,
                OficinaNombre = r.OficinaNombre,
                OficinaDir = r.OficinaDir,
                IdiomaReunion = r.IdiomaReunion,
                TiempoTrasladoSiguiente = r.TiempoTrasladoSiguiente
            }).ToList()
        };

        var pdfBytes = await pdfClient.GenerarPdfAsync(pdfRequest);

        var nombreArchivo = $"Agenda_{agenda.Fecha:yyyy-MM-dd}_{agenda.InversorNombre.Replace(" ", "_")}.pdf";

        return File(pdfBytes, "application/pdf", nombreArchivo);
    }
}
