using PDF.Application.Interfaces;
using PDF.Domain;

namespace PDF.Application.UseCases;

/// <summary>
/// Maneja la generación del PDF de una agenda confirmada.
/// Delega la construcción del documento a IPdfGeneratorService (QuestPDF).
/// </summary>
public class GenerarPdfHandler(IPdfGeneratorService pdfGenerator)
{
    /// <summary>
    /// Genera el PDF del itinerario de agenda.
    /// </summary>
    /// <param name="request">Datos de la agenda.</param>
    /// <returns>Bytes del PDF generado.</returns>
    public byte[] Handle(AgendaPdfRequest request) =>
        pdfGenerator.Generar(request);
}
