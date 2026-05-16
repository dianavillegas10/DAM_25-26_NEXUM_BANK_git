using iTextSharp.text;
using iTextSharp.text.pdf;
using NexumApp.Models;
using System;
using System.IO;

namespace NexumApp.Vºº
{
    public class ReciboMovilPdfService
    {
        private static readonly string RUTA_RECIBOS = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NexumBank", "Recibos");

        static ReciboMovilPdfService()
        {
            if (!Directory.Exists(RUTA_RECIBOS))
                Directory.CreateDirectory(RUTA_RECIBOS);
        }

        public string GenerarReciboRecarga(CuentaBancaria cuenta, string telefono, string operadora, decimal importe, DateTime fecha)
        {
            try
            {
                string nombreArchivo = $"Recibo_Recarga_{telefono}_{fecha:yyyyMMdd_HHmmss}.pdf";
                string rutaCompleta = Path.Combine(RUTA_RECIBOS, nombreArchivo);

                using (FileStream fs = new FileStream(rutaCompleta, FileMode.Create))
                {
                    Document doc = new Document(PageSize.A4, 50, 50, 50, 50);
                    PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                    doc.Open();

                    // Fuentes
                    Font fontTitulo = FontFactory.GetFont("Arial", 24, Font.BOLD, BaseColor.DARK_GRAY);
                    Font fontSubtitulo = FontFactory.GetFont("Arial", 12, Font.NORMAL, BaseColor.GRAY);
                    Font fontEncabezado = FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.WHITE);
                    Font fontNormal = FontFactory.GetFont("Arial", 10, Font.NORMAL);
                    Font fontBold = FontFactory.GetFont("Arial", 10, Font.BOLD);
                    Font fontGrande = FontFactory.GetFont("Arial", 16, Font.BOLD, new BaseColor(236, 0, 0));
                    Font fontVerde = FontFactory.GetFont("Arial", 12, Font.BOLD, new BaseColor(0, 168, 89));

                    // Título
                    Paragraph titulo = new Paragraph("NEXUM BANK", fontTitulo);
                    titulo.Alignment = Element.ALIGN_CENTER;
                    doc.Add(titulo);

                    Paragraph subtitulo = new Paragraph("Comprobante de Recarga de Móvil", fontSubtitulo);
                    subtitulo.Alignment = Element.ALIGN_CENTER;
                    doc.Add(subtitulo);

                    doc.Add(Chunk.NEWLINE);
                    doc.Add(new Paragraph($"Fecha: {fecha:dd/MM/yyyy HH:mm:ss}", fontNormal));
                    doc.Add(new Paragraph($"Nº Recibo: {fecha:yyyyMMddHHmmss}", fontNormal));
                    doc.Add(Chunk.NEWLINE);

                    // Separador
                    PdfPTable separador = new PdfPTable(1);
                    separador.WidthPercentage = 100;
                    PdfPCell cellSep = new PdfPCell(new Phrase(""));
                    cellSep.BackgroundColor = new BaseColor(236, 0, 0);
                    cellSep.FixedHeight = 3f;
                    cellSep.Border = Rectangle.NO_BORDER;
                    separador.AddCell(cellSep);
                    doc.Add(separador);
                    doc.Add(Chunk.NEWLINE);

                    // Tabla principal
                    PdfPTable tabla = new PdfPTable(2);
                    tabla.WidthPercentage = 100;
                    tabla.SetWidths(new float[] { 35f, 65f });

                    AgregarCelda(tabla, "SERVICIO:", fontEncabezado, BaseColor.DARK_GRAY);
                    AgregarCelda(tabla, "Recarga de Móvil", fontNormal, BaseColor.WHITE);

                    AgregarCelda(tabla, "NÚMERO:", fontEncabezado, BaseColor.DARK_GRAY);
                    AgregarCelda(tabla, telefono, fontNormal, BaseColor.WHITE);

                    AgregarCelda(tabla, "OPERADORA:", fontEncabezado, BaseColor.DARK_GRAY);
                    AgregarCelda(tabla, operadora, fontNormal, BaseColor.WHITE);

                    doc.Add(tabla);
                    doc.Add(Chunk.NEWLINE);

                    // Cuenta
                    PdfPTable tablaCuenta = new PdfPTable(2);
                    tablaCuenta.WidthPercentage = 100;
                    tablaCuenta.SetWidths(new float[] { 35f, 65f });

                    AgregarCelda(tablaCuenta, "CUENTA DEBITADA:", fontEncabezado, BaseColor.DARK_GRAY);
                    string numeroOculto = $"••••{cuenta.NumeroCuenta?.Substring(Math.Max(0, cuenta.NumeroCuenta.Length - 4)) ?? "0000"}";
                    AgregarCelda(tablaCuenta, $"{cuenta.TipoCuenta} - {numeroOculto}", fontNormal, BaseColor.WHITE);

                    doc.Add(tablaCuenta);
                    doc.Add(Chunk.NEWLINE);

                    // Importe
                    PdfPTable tablaMonto = new PdfPTable(2);
                    tablaMonto.WidthPercentage = 100;
                    tablaMonto.SetWidths(new float[] { 35f, 65f });

                    AgregarCelda(tablaMonto, "IMPORTE RECARGADO:", fontEncabezado, BaseColor.DARK_GRAY);
                    PdfPCell celdaMonto = new PdfPCell(new Phrase($"{importe:N2} €", fontGrande));
                    celdaMonto.Border = Rectangle.NO_BORDER;
                    celdaMonto.BackgroundColor = new BaseColor(236, 0, 0, 50);
                    celdaMonto.Padding = 8;
                    tablaMonto.AddCell(celdaMonto);

                    doc.Add(tablaMonto);
                    doc.Add(Chunk.NEWLINE);

                    // Estado
                    PdfPTable tablaEstado = new PdfPTable(1);
                    tablaEstado.WidthPercentage = 100;

                    PdfPCell cellEstado = new PdfPCell(new Phrase("✔ RECARGA EXITOSA", fontVerde));
                    cellEstado.Border = Rectangle.BOX;
                    cellEstado.BorderColor = new BaseColor(0, 168, 89);
                    cellEstado.BorderWidth = 2f;
                    cellEstado.HorizontalAlignment = Element.ALIGN_CENTER;
                    cellEstado.Padding = 10;
                    tablaEstado.AddCell(cellEstado);

                    doc.Add(tablaEstado);
                    doc.Add(Chunk.NEWLINE);

                    // Saldo
                    decimal nuevoSaldo = cuenta.Saldo - importe;
                    PdfPTable tablaSaldo = new PdfPTable(2);
                    tablaSaldo.WidthPercentage = 100;
                    tablaSaldo.SetWidths(new float[] { 50f, 50f });

                    AgregarCelda(tablaSaldo, "Saldo anterior:", fontNormal, BaseColor.WHITE);
                    AgregarCelda(tablaSaldo, $"{cuenta.Saldo:N2} €", fontBold, BaseColor.WHITE);
                    AgregarCelda(tablaSaldo, "Saldo posterior:", fontNormal, BaseColor.WHITE);
                    AgregarCelda(tablaSaldo, $"{nuevoSaldo:N2} €", fontBold, BaseColor.WHITE);

                    doc.Add(tablaSaldo);
                    doc.Add(Chunk.NEWLINE);

                    // Pie
                    Paragraph pie = new Paragraph("Este documento es un comprobante válido de recarga.\nGracias por confiar en Nexum Bank.",
                        FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.GRAY));
                    pie.Alignment = Element.ALIGN_CENTER;
                    doc.Add(pie);

                    doc.Close();
                }

                return rutaCompleta;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generando PDF recarga: {ex.Message}");
                throw;
            }
        }

        private void AgregarCelda(PdfPTable tabla, string texto, Font fuente, BaseColor colorFondo)
        {
            PdfPCell cell = new PdfPCell(new Phrase(texto, fuente));
            cell.Border = Rectangle.NO_BORDER;
            cell.BackgroundColor = colorFondo;
            cell.Padding = 6;
            tabla.AddCell(cell);
        }
    }
}