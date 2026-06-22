using Catalogo.Application.DTOs;
using Catalogo.Application.UseCases.Inversores;
using Microsoft.AspNetCore.Mvc;

namespace Catalogo.API.Controllers;

/// <summary>
/// Controlador REST para la gestión de inversores.
/// Expone los endpoints CU-01 del sistema de calendarización.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InversoresController(
    ConsultarInversoresHandler consultarHandler,
    RegistrarInversorHandler registrarHandler,
    ActualizarInversorHandler actualizarHandler,
    EliminarInversorHandler eliminarHandler) : ControllerBase
{
    /// <summary>Lista todos los inversores registrados.</summary>
    /// <returns>Lista de inversores con sus idiomas y ventana de visita.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InversorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() =>
        Ok(await consultarHandler.GetAllAsync());

    /// <summary>Obtiene el detalle de un inversor por Id.</summary>
    /// <param name="id">GUID del inversor.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InversorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(await consultarHandler.GetByIdAsync(id));

    /// <summary>
    /// Registra un nuevo inversor.
    /// Aplica RN-01 (idiomas) y RN-02 (fechas).
    /// </summary>
    /// <param name="request">Datos del inversor a registrar.</param>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(InversorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CrearInversorRequest request)
    {
        var result = await registrarHandler.HandleAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Actualiza los datos de un inversor existente.
    /// Aplica RN-01 y RN-02.
    /// </summary>
    /// <param name="id">GUID del inversor a actualizar.</param>
    /// <param name="request">Nuevos datos del inversor.</param>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(InversorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ActualizarInversorRequest request) =>
        Ok(await actualizarHandler.HandleAsync(id, request));

    /// <summary>
    /// Elimina un inversor.
    /// RN-03: bloquea si tiene agendas activas (HTTP 409).
    /// </summary>
    /// <param name="id">GUID del inversor a eliminar.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await eliminarHandler.HandleAsync(id);
        return Ok(new { message = "Inversor eliminado correctamente." });
    }
}
