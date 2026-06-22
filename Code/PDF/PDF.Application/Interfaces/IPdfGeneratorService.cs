using PDF.Domain;

namespace PDF.Application.Interfaces;

/// <summary>
/// Contrato del generador de PDF del itinerario de agenda.
/// Implementado en la capa Infrastructure usando QuestPDF.
/// </summary>
public interface IPdfGeneratorService
{
    /// <summary>
    /// Genera el documento PDF del itinerario en español (variante Costa Rica).
    /// </summary>
    /// <param name="request">Datos completos de la agenda para el documento.</param>
    /// <returns>Arreglo de bytes con el PDF generado.</returns>
    byte[] Generar(AgendaPdfRequest request);
}
