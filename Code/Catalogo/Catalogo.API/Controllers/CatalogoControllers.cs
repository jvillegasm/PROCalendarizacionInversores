using Catalogo.Application.DTOs;
using Catalogo.Application.UseCases.Oficinas;
using Catalogo.Application.UseCases.Traslados;
using Microsoft.AspNetCore.Mvc;

namespace Catalogo.API.Controllers;

/// <summary>Controlador REST para la gestión de oficinas (CU-03).</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OficinasController(
    ConsultarOficinasHandler consultarHandler,
    RegistrarOficinaHandler registrarHandler,
    ActualizarOficinaHandler actualizarHandler,
    EliminarOficinaHandler eliminarHandler) : ControllerBase
{
    /// <summary>Lista todas las oficinas registradas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OficinaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() => Ok(await consultarHandler.GetAllAsync());

    /// <summary>Obtiene una oficina por Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OficinaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id) => Ok(await consultarHandler.GetByIdAsync(id));

    /// <summary>Registra una nueva oficina.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(OficinaDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CrearOficinaRequest request)
    {
        var result = await registrarHandler.HandleAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Actualiza los datos de una oficina.</summary>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(OficinaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CrearOficinaRequest request)
        => Ok(await actualizarHandler.HandleAsync(id, request));

    /// <summary>Elimina una oficina. RN-06: bloquea si tiene participantes activos (HTTP 409).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await eliminarHandler.HandleAsync(id);
        return Ok(new { message = "Oficina eliminada correctamente." });
    }
}

/// <summary>Controlador REST para la gestión de la matriz de traslados (CU-03).</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TrasladosController(
    ConsultarTrasladosHandler consultarHandler,
    RegistrarTrasladoHandler registrarHandler,
    ActualizarTrasladoHandler actualizarHandler,
    EliminarTrasladoHandler eliminarHandler) : ControllerBase
{
    /// <summary>Obtiene todos los pares de traslado de la matriz.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MatrizTrasladoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() => Ok(await consultarHandler.GetAllAsync());

    /// <summary>
    /// Registra un par de traslado A→B y automáticamente crea el simétrico B→A.
    /// RN-07: simetría garantizada.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(IEnumerable<MatrizTrasladoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CrearTrasladoRequest request)
    {
        var result = await registrarHandler.HandleAsync(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Actualiza el tiempo de un par de traslado y su simétrico.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MatrizTrasladoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ActualizarTrasladoRequest request)
        => Ok(await actualizarHandler.HandleAsync(id, request));

    /// <summary>Elimina un par de traslado y su simétrico.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await eliminarHandler.HandleAsync(id);
        return Ok(new { message = "Par de traslado eliminado correctamente." });
    }
}

/// <summary>Controlador REST para el catálogo de idiomas.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class IdiomasController(Catalogo.Application.Interfaces.IIdiomaRepository idiomaRepository) : ControllerBase
{
    /// <summary>Obtiene todos los idiomas disponibles en el catálogo.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<IdiomaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var idiomas = await idiomaRepository.GetAllAsync();
        return Ok(idiomas.Select(i => new IdiomaDto(i.Id, i.Nombre, i.Codigo)));
    }
}
