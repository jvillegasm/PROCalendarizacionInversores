using Agendas.Application.Interfaces;
using Agendas.Domain.Exceptions;
using System.Net.Http.Json;

namespace Agendas.Infrastructure.HttpClients;

/// <summary>
/// Cliente HTTP tipado hacia el PDF Service.
/// Configurado con políticas de resiliencia Polly en Program.cs.
/// </summary>
public class PdfServiceHttpClient(HttpClient httpClient) : IPdfServiceHttpClient
{
    /// <inheritdoc/>
    public async Task<byte[]> GenerarPdfAsync(object request)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync("api/GenerarPdf/generar-pdf", request);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
        {
            // El PDF Service no está accesible (timeout, red, contenedor caído)
            throw new PdfServiceNoDisponibleException();
        }

        if (!response.IsSuccessStatusCode)
        {
            // El PDF Service respondió con error (4xx/5xx); relanzar con detalle
            var body = await response.Content.ReadAsStringAsync();
            throw new PdfServiceNoDisponibleException(
                $"PDF Service respondió {(int)response.StatusCode}: {body}");
        }

        return await response.Content.ReadAsByteArrayAsync();
    }
}
