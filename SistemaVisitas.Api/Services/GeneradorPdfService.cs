using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaVisitas.Shared.DTOs;

namespace SistemaVisitas.Api.Services
{
    public class GeneradorPdfService
    {
        public byte[] GenerarActaPdf(ActaVisitaDto acta)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(compose => ComponerEncabezado(compose, acta));
                    page.Content().Element(compose => ComponerContenido(compose, acta));
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf(); // Retorna los bytes del PDF
        }

        private void ComponerEncabezado(IContainer container, ActaVisitaDto acta)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    // Escribimos "TECNO" y "WARE" usando bloques de texto separados para evitar errores de Span
                    col.Item().Text(text =>
                    {
                        text.Span("TECNO").FontSize(24).FontColor(Colors.Blue.Darken2).ExtraBold();
                        text.Span("WARE").FontSize(24).FontColor(Colors.Blue.Lighten1);
                    });

                    col.Item().Text("Soluciones Tecnológicas e Inventario").FontSize(10).FontColor(Colors.Grey.Medium);
                });

                row.ConstantItem(150).AlignRight().Column(col =>
                {
                    col.Item().Text("ACTA DE SERVICIO").Bold().FontSize(12);
                    col.Item().Text($"N° {acta.Consecutivo}").FontSize(14).FontColor(Colors.Grey.Darken2);
                });
            });
        }
        

        private void ComponerContenido(IContainer container, ActaVisitaDto acta)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(col =>
            {
                // Información general
                col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Información del Cliente").Bold().FontColor(Colors.Grey.Darken1);
                        c.Item().Text($"Empresa: {acta.EmpresaNombre}");
                        c.Item().Text($"Recibe: {acta.NombreQuienRecibe}");
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Detalles de la Visita").Bold().FontColor(Colors.Grey.Darken1);
                        c.Item().Text($"Técnico: {acta.TecnicoNombre}");
                        c.Item().Text($"Fecha: {acta.FechaInicio:dd/MM/yyyy}");
                    });
                });

                col.Item().PaddingTop(15).Text("Resultados de la Inspección").Bold().FontSize(14);

                // Tabla de Respuestas
                col.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Pregunta
                        columns.RelativeColumn(2); // Respuesta
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).Padding(2).Text("Ítem evaluado").SemiBold();
                        header.Cell().BorderBottom(1).Padding(2).Text("Resultado").SemiBold();
                    });

                    foreach (var resp in acta.Respuestas)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(resp.Pregunta);

                        // Si es imagen, ponemos un texto indicando que la evidencia está adjunta digitalmente
                        var textoRespuesta = (resp.TipoCampo == "Imagen" || resp.TipoCampo == "Evidencia")
                            ? "[Evidencia Fotográfica en Sistema]"
                            : resp.Respuesta;

                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(textoRespuesta);
                    }
                });

                // Firmas
                // Firmas
                col.Item().PaddingTop(50).Row(row =>
                {
                    row.RelativeItem().AlignCenter().Column(c =>
                    {
                        // Altura reservada para que la firma quepa sin desacomodar el texto
                        c.Item().Height(60).AlignBottom().AlignCenter().Element(firmaContenedor =>
                        {
                            // Verificamos si la firma existe y tiene el formato correcto de Canvas Base64
                            if (!string.IsNullOrEmpty(acta.FirmaClienteBase64) && acta.FirmaClienteBase64.Contains(","))
                            {
                                // Extraemos la data pura quitando el encabezado "data:image/png;base64,"
                                var base64Data = acta.FirmaClienteBase64.Split(',')[1];
                                byte[] imageBytes = Convert.FromBase64String(base64Data);
                                firmaContenedor.Image(imageBytes).FitHeight();
                            }
                        });

                        c.Item().LineHorizontal(1);
                        c.Item().PaddingTop(5).Text(acta.NombreQuienRecibe).Bold();
                        c.Item().Text("Aceptado a Satisfacción");
                    });

                    row.ConstantItem(50); // Espacio entre firmas

                    row.RelativeItem().AlignCenter().Column(c =>
                    {
                        c.Item().Height(60); // Mantener simetría
                        c.Item().LineHorizontal(1);
                        c.Item().PaddingTop(5).Text(acta.TecnicoNombre).Bold();
                        c.Item().Text("Soporte Técnico");
                    });
                });
            });
        }
    }
}