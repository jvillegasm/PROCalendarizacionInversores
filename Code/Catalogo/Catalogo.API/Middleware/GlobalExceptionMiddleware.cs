using Catalogo.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Catalogo.API.Middleware;

/// <summary>
/// Middleware global que intercepta excepciones no controladas y las convierte
/// en respuestas HTTP estructuradas. Nunca expone stack traces en producción.
/// </summary>
public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    /// <summary>Invoca el siguiente middleware y captura cualquier excepción.</summary>
    /// <param name="context">Contexto HTTP actual.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            logger.LogWarning("Excepción de dominio: {Message}", ex.Message);
            await WriteErrorResponse(context, GetStatusCodeForDomainException(ex), ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no controlado en Catálogo Service");
            await WriteErrorResponse(context, StatusCodes.Status500InternalServerError,
                "Ha ocurrido un error interno en el servidor.");
        }
    }

    /// <summary>Mapea las excepciones de dominio al código HTTP correspondiente.</summary>
    private static int GetStatusCodeForDomainException(DomainException ex) => ex switch
    {
        InversorNotFoundException => StatusCodes.Status404NotFound,
        OficinaNotFoundException => StatusCodes.Status404NotFound,
        ParticipanteNotFoundException => StatusCodes.Status404NotFound,
        TrasladoNotFoundException => StatusCodes.Status404NotFound,
        InversorConAgendasActivasException => StatusCodes.Status409Conflict,
        OficinaConParticipantesActivosException => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status400BadRequest
    };

    /// <summary>Escribe la respuesta de error en formato JSON estructurado.</summary>
    private static async Task WriteErrorResponse(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ProblemDetails
        {
            Status = statusCode,
            Title = message,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
