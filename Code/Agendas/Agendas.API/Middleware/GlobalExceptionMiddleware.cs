using Agendas.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Agendas.API.Middleware;

/// <summary>
/// Middleware global de excepciones del Agendas Service.
/// Convierte excepciones de dominio en respuestas HTTP estructuradas.
/// Nunca expone stack traces en producción.
/// </summary>
public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    /// <summary>Invoca el siguiente middleware y captura cualquier excepción.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AgendasDomainException ex)
        {
            logger.LogWarning("Excepción de dominio (Agendas): {Message}", ex.Message);
            var statusCode = GetStatusCode(ex);
            await WriteError(context, statusCode, ex.Message, GetCodigoError(ex));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no controlado en Agendas Service");
            await WriteError(context, StatusCodes.Status500InternalServerError,
                "Ha ocurrido un error interno en el servidor.", null);
        }
    }

    private static int GetStatusCode(AgendasDomainException ex) => ex switch
    {
        AgendaNotFoundException => StatusCodes.Status404NotFound,
        AgendaYaAnuladaException => StatusCodes.Status409Conflict,
        FechaFueraDeRangoException => StatusCodes.Status422UnprocessableEntity,
        IdiomaIncompatibleException => StatusCodes.Status422UnprocessableEntity,
        SinDisponibilidadException => StatusCodes.Status422UnprocessableEntity,
        TrasladosInviablesException => StatusCodes.Status422UnprocessableEntity,
        CatalogoServiceNoDisponibleException => StatusCodes.Status503ServiceUnavailable,
        PdfServiceNoDisponibleException => StatusCodes.Status504GatewayTimeout,
        _ => StatusCodes.Status400BadRequest
    };

    private static string? GetCodigoError(AgendasDomainException ex) => ex switch
    {
        FechaFueraDeRangoException e => e.CodigoError,
        IdiomaIncompatibleException e => e.CodigoError,
        SinDisponibilidadException e => e.CodigoError,
        TrasladosInviablesException e => e.CodigoError,
        _ => null
    };

    private static async Task WriteError(HttpContext context, int statusCode, string message, string? codigoError)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = statusCode,
            title = message,
            codigoError,
            instance = context.Request.Path.Value
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
