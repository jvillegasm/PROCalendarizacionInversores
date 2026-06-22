using PDF.Application.Interfaces;
using PDF.Domain;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PDF.Infrastructure.Services;

/// <summary>
/// Implementación del generador de PDF usando QuestPDF.
/// Genera el documento del itinerario en español (variante Costa Rica — es-CR).
/// Cumple con AC-06 y los requisitos del §4.4 de la Prueba Técnica:
/// - Encabezado institucional con nombre del inversor y empresa.
/// - Fecha de la jornada.
/// - Tabla de reuniones: hora, participante, cargo, oficina, dirección, idioma, traslado.
/// - Separador del bloque de almuerzo 12:00–13:00 (RN-11).
/// - Pie de página con número de página y fecha de generación.
/// </summary>
public class QuestPdfGeneratorService : IPdfGeneratorService
{
    private static readonly System.Globalization.CultureInfo CultureCR =
        System.Globalization.CultureInfo.GetCultureInfo("es-CR");

    /// <inheritdoc/>
    public byte[] Generar(AgendaPdfRequest request)
    {
        // Configurar QuestPDF en modo Community (licencia gratuita para uso comercial limitado)
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                // ─── Encabezado ────────────────────────────────────────────────
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("PROCOMER")
                                .FontSize(20).Bold()
                                .FontColor(Color.FromHex("#0c7c59"));
                            c.Item().Text("Promotora del Comercio Exterior de Costa Rica")
                                .FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                        row.ConstantItem(200).AlignRight().Column(c =>
                        {
                            c.Item().Text("ITINERARIO DE VISITA")
                                .FontSize(14).Bold();
                            c.Item().Text($"Estado: {request.Estado}")
                                .FontSize(9)
                                .FontColor(request.Estado == "Anulada"
                                    ? Color.FromHex("#dc3545")
                                    : Color.FromHex("#198754"));
                        });
                    });

                    col.Item().PaddingVertical(6).LineHorizontal(1)
                        .LineColor(Color.FromHex("#0c7c59"));

                    col.Item().PaddingTop(4).Column(c =>
                    {
                        c.Item().Text(tb =>
                        {
                            tb.Span("Inversor: ").Bold();
                            tb.Span(request.InversorNombre).FontSize(12);
                        });
                        c.Item().Text(tb =>
                        {
                            tb.Span("Empresa: ").Bold();
                            tb.Span(request.InversorEmpresa);
                        });
                        c.Item().Text(tb =>
                        {
                            tb.Span("Fecha de jornada: ").Bold();
                            tb.Span(request.Fecha.ToString("dddd, d 'de' MMMM 'de' yyyy", CultureCR));
                        });
                    });

                    col.Item().PaddingTop(8).Text("Reuniones programadas")
                        .FontSize(11).Bold().FontColor(Color.FromHex("#0c7c59"));
                });

                // ─── Contenido ─────────────────────────────────────────────────
                page.Content().PaddingTop(8).Column(col =>
                {
                    // Cabecera de la tabla
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(55);  // Hora
                            cols.RelativeColumn(2);   // Participante / Cargo
                            cols.RelativeColumn(2);   // Oficina / Dirección
                            cols.ConstantColumn(55);  // Idioma
                            cols.ConstantColumn(55);  // Traslado
                        });

                        // Cabecera
                        table.Header(header =>
                        {
                            HeaderCell(header, "Hora");
                            HeaderCell(header, "Participante");
                            HeaderCell(header, "Oficina");
                            HeaderCell(header, "Idioma");
                            HeaderCell(header, "Traslado");
                        });

                        // Filas de reuniones
                        var reuniones = request.Reuniones.OrderBy(r => r.Orden).ToList();
                        for (var i = 0; i < reuniones.Count; i++)
                        {
                            var r = reuniones[i];
                            var bgColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;

                            // Verificar si el almuerzo cae entre esta reunión y la siguiente
                            var esAntesAlmuerzo = i < reuniones.Count - 1 &&
                                TimeSpan.Parse(reuniones[i].HoraFin) <= new TimeSpan(12, 0, 0) &&
                                TimeSpan.Parse(reuniones[i + 1].HoraInicio) >= new TimeSpan(13, 0, 0);

                            // Celda: hora
                            table.Cell().Background(bgColor).Padding(4).Column(c =>
                            {
                                c.Item().Text($"{r.HoraInicio}").Bold().FontSize(9);
                                c.Item().Text($"– {r.HoraFin}").FontSize(8).FontColor(Colors.Grey.Medium);
                            });

                            // Celda: participante
                            table.Cell().Background(bgColor).Padding(4).Column(c =>
                            {
                                c.Item().Text(r.ParticipanteNombre).Bold().FontSize(9);
                                c.Item().Text(r.ParticipanteCargo).FontSize(8).FontColor(Colors.Grey.Darken1);
                            });

                            // Celda: oficina
                            table.Cell().Background(bgColor).Padding(4).Column(c =>
                            {
                                c.Item().Text(r.OficinaNombre).Bold().FontSize(9);
                                c.Item().Text(r.OficinaDir).FontSize(7).FontColor(Colors.Grey.Medium);
                            });

                            // Celda: idioma
                            table.Cell().Background(bgColor).Padding(4)
                                .Text(r.IdiomaReunion).FontSize(8).AlignCenter();

                            // Celda: traslado
                            table.Cell().Background(bgColor).Padding(4)
                                .Text(r.TiempoTrasladoSiguiente > 0
                                    ? $"{r.TiempoTrasladoSiguiente} min"
                                    : "—")
                                .FontSize(8).AlignCenter();

                            // Separador de almuerzo
                            if (esAntesAlmuerzo)
                            {
                                table.Cell().ColumnSpan(5).Background(Color.FromHex("#fff3cd"))
                                    .Padding(4)
                                    .Text("🍽  Almuerzo: 12:00 – 13:00 (bloque reservado — RN-11)")
                                    .Italic().FontSize(8).FontColor(Color.FromHex("#856404"));
                            }
                        }
                    });
                });

                // ─── Pie de página ──────────────────────────────────────────────
                page.Footer().Column(col =>
                {
                    col.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                    col.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Text(
                            "Documento generado por el Sistema de Calendarización de Inversores — PROCOMER")
                            .FontSize(7).FontColor(Colors.Grey.Medium);
                        row.ConstantItem(120).AlignRight().Text(tb =>
                        {
                            tb.Span($"Generado: {DateTime.Now.ToString("d 'de' MMMM 'de' yyyy, HH:mm", CultureCR)}")
                              .FontSize(7).FontColor(Colors.Grey.Medium);
                            tb.Span("   |   Página ").FontSize(7);
                            tb.CurrentPageNumber().FontSize(7);
                            tb.Span(" de ").FontSize(7);
                            tb.TotalPages().FontSize(7);
                        });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    /// <summary>Crea una celda de encabezado con estilo institucional.</summary>
    private static void HeaderCell(TableCellDescriptor header, string text)
    {
        header.Cell()
            .Background(Color.FromHex("#0c7c59"))
            .Padding(5)
            .Text(text)
            .FontSize(9).Bold().FontColor(Colors.White);
    }
}
