using iTextSharp.text;
using iTextSharp.text.pdf;
using NexumApp.Models;
using System;
using System.IO;

namespace NexumApp.Services
{
    public class ReciboPdfService
    {
        private static readonly string RUTA_RECIBOS = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NexumBank", "Recibos");

        static ReciboPdfService()
        {
            // Crear carpeta si no existe
            if (!Directory.Exists(RUTA_RECIBOS))
                Directory.CreateDirectory(RUTA_RECIBOS);
        }

        public string GenerarReciboPago(CuentaBancaria cuenta, string servicio, string compañia, string referencia, decimal importe, DateTime fecha)
        {
            try
            {
                string nombreArchivo = $"Recibo_{servicio}_{compañia}_{fecha:yyyyMMdd_HHmmss}.pdf";
                string rutaCompleta = Path.Combine(RUTA_RECIBOS, nombreArchivo);

                using (FileStream fs = new FileStream(rutaCompleta, FileMode.Create))
                {
                    Document doc = new Document(PageSize.A4, 50, 50, 50, 50);
                    PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                    doc.Open();

                    // ===========================================
                    // 1. ENCABEZADO - Logo y título
                    // ===========================================
                    Font fontTitulo = FontFactory.GetFont("Arial", 24, Font.BOLD, BaseColor.DARK_GRAY);
                    Font fontSubtitulo = FontFactory.GetFont("Arial", 12, Font.NORMAL, BaseColor.GRAY);
                    Font fontEncabezado = FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.WHITE);
                    Font fontNormal = FontFactory.GetFont("Arial", 10, Font.NORMAL);
                    Font fontBold = FontFactory.GetFont("Arial", 10, Font.BOLD);
                    Font fontGrande = FontFactory.GetFont("Arial", 16, Font.BOLD, new BaseColor(239, 68, 68));
                    Font fontVerde = FontFactory.GetFont("Arial", 12, Font.BOLD, new BaseColor(16, 185, 129));

                    // Título principal
                    Paragraph titulo = new Paragraph("NEXUM BANK", fontTitulo);
                    titulo.Alignment = Element.ALIGN_CENTER;
                    doc.Add(titulo);

                    Paragraph subtitulo = new Paragraph("Comprobante de Pago de Servicios", fontSubtitulo);
                    subtitulo.Alignment = Element.ALIGN_CENTER;
                    doc.Add(subtitulo);

                    doc.Add(Chunk.NEWLINE);
                    doc.Add(new Paragraph($"Fecha: {fecha:dd/MM/yyyy HH:mm:ss}", fontNormal));
                    doc.Add(new Paragraph($"Nº Recibo: {fecha:yyyyMMddHHmmss}", fontNormal));
                    doc.Add(Chunk.NEWLINE);

                    // ===========================================
                    // 2. SEPARADOR
                    // ===========================================
                    PdfPTable separador = new PdfPTable(1);
                    separador.WidthPercentage = 100;
                    PdfPCell cellSep = new PdfPCell(new Phrase(""));
                    cellSep.BackgroundColor = new BaseColor(239, 68, 68);
                    cellSep.FixedHeight = 3f;
                    cellSep.Border = Rectangle.NO_BORDER;
                    separador.AddCell(cellSep);
                    doc.Add(separador);
                    doc.Add(Chunk.NEWLINE);

                    // ===========================================
                    // 3. DATOS DEL PAGO (Tabla Principal)
                    // ===========================================
                    PdfPTable tabla = new PdfPTable(2);
                    tabla.WidthPercentage = 100;
                    tabla.SetWidths(new float[] { 35f, 65f });

                    // Servicio
                    AgregarCelda(tabla, "SERVICIO:", fontEncabezado, BaseColor.DARK_GRAY);
                    AgregarCelda(tabla, $"{servicio}", fontNormal, BaseColor.WHITE);

                    // Compañía
                    AgregarCelda(tabla, "COMPAÑÍA:", fontEncabezado, BaseColor.DARK_GRAY);
                    AgregarCelda(tabla, compañia, fontNormal, BaseColor.WHITE);

                    // Referencia
                    AgregarCelda(tabla, "REFERENCIA:", fontEncabezado, BaseColor.DARK_GRAY);
                    AgregarCelda(tabla, referencia, fontNormal, BaseColor.WHITE);

                    doc.Add(tabla);
                    doc.Add(Chunk.NEWLINE);

                    // ===========================================
                    // 4. INFORMACIÓN DE LA CUENTA
                    // ===========================================
                    PdfPTable tablaCuenta = new PdfPTable(2);
                    tablaCuenta.WidthPercentage = 100;
                    tablaCuenta.SetWidths(new float[] { 35f, 65f });

                    AgregarCelda(tablaCuenta, "CUENTA DEBITADA:", fontEncabezado, BaseColor.DARK_GRAY);
                    string numeroOculto = $"••••{cuenta.NumeroCuenta?.Substring(Math.Max(0, cuenta.NumeroCuenta.Length - 4)) ?? "0000"}";
                    AgregarCelda(tablaCuenta, $"{cuenta.TipoCuenta} - {numeroOculto}", fontNormal, BaseColor.WHITE);

                    doc.Add(tablaCuenta);
                    doc.Add(Chunk.NEWLINE);

                    // ===========================================
                    // 5. MONTO PAGADO (destacado)
                    // ===========================================
                    PdfPTable tablaMonto = new PdfPTable(2);
                    tablaMonto.WidthPercentage = 100;
                    tablaMonto.SetWidths(new float[] { 35f, 65f });

                    AgregarCelda(tablaMonto, "IMPORTE:", fontEncabezado, BaseColor.DARK_GRAY);
                    PdfPCell celdaMonto = new PdfPCell(new Phrase($"{importe:N2} €", fontGrande));
                    celdaMonto.Border = Rectangle.NO_BORDER;
                    celdaMonto.BackgroundColor = new BaseColor(239, 68, 68, 50);
                    celdaMonto.Padding = 8;
                    tablaMonto.AddCell(celdaMonto);

                    doc.Add(tablaMonto);
                    doc.Add(Chunk.NEWLINE);

                    // ===========================================
                    // 6. ESTADO DEL PAGO
                    // ===========================================
                    PdfPTable tablaEstado = new PdfPTable(1);
                    tablaEstado.WidthPercentage = 100;

                    PdfPCell cellEstado = new PdfPCell(new Phrase("✔ PAGO EXITOSO", fontVerde));
                    cellEstado.Border = Rectangle.BOX;
                    cellEstado.BorderColor = new BaseColor(16, 185, 129);
                    cellEstado.BorderWidth = 2f;
                    cellEstado.HorizontalAlignment = Element.ALIGN_CENTER;
                    cellEstado.Padding = 10;
                    tablaEstado.AddCell(cellEstado);

                    doc.Add(tablaEstado);
                    doc.Add(Chunk.NEWLINE);

                    // ===========================================
                    // 7. SALDO POSTERIOR
                    // ===========================================
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

                    // ===========================================
                    // 8. PIE DE PÁGINA
                    // ===========================================
                    Paragraph pie = new Paragraph("Este documento es un comprobante válido de pago.\nGracias por confiar en Nexum Bank.",
                        FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.GRAY));
                    pie.Alignment = Element.ALIGN_CENTER;
                    doc.Add(pie);

                    doc.Close();
                }

                return rutaCompleta;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generando PDF: {ex.Message}");
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