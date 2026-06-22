using Catalogo.Application.DTOs;
using Catalogo.Application.UseCases.Participantes;
using Microsoft.AspNetCore.Mvc;

namespace Catalogo.API.Controllers;

/// <summary>
/// Controlador REST para la gestión de participantes (CU-02).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ParticipantesController(
    ConsultarParticipantesHandler consultarHandler,
    RegistrarParticipanteHandler registrarHandler,
    ActualizarParticipanteHandler actualizarHandler,
    EliminarParticipanteHandler eliminarHandler) : ControllerBase
{
    /// <summary>Lista todos los participantes.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ParticipanteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() =>
        Ok(await consultarHandler.GetAllAsync());

    /// <summary>Lista solo participantes activos.</summary>
    [HttpGet("activos")]
    [ProducesResponseType(typeof(IEnumerable<ParticipanteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivos() =>
        Ok(await consultarHandler.GetActivosAsync());

    /// <summary>Obtiene el detalle de un participante por Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ParticipanteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(await consultarHandler.GetByIdAsync(id));

    /// <summary>Registra un nuevo participante. Aplica RN-04 y RN-05.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ParticipanteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CrearParticipanteRequest request)
    {
        var result = await registrarHandler.HandleAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Actualiza un participante o cambia su estado (activo/inactivo).</summary>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ParticipanteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ActualizarParticipanteRequest request) =>
        Ok(await actualizarHandler.HandleAsync(id, request));

    /// <summary>Elimina un participante.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await eliminarHandler.HandleAsync(id);
        return Ok(new { message = "Participante eliminado correctamente." });
    }
}
