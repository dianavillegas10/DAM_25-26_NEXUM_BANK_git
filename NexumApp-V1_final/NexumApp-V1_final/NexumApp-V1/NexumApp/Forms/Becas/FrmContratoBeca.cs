using NexumApp.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NexumApp.Forms.Becas
{
    internal class FrmContratoBeca : Form
    {
        private static readonly Color Indigo     = Color.FromArgb(99,  102, 241);
        private static readonly Color IndigoDark = Color.FromArgb(49,  46,  129);
        private static readonly Color White      = Color.White;
        private static readonly Color BgGray     = Color.FromArgb(248, 249, 252);
        private static readonly Color TextDark   = Color.FromArgb(17,  24,  39);
        private static readonly Color TextGray   = Color.FromArgb(107, 114, 128);
        private static readonly Color Border     = Color.FromArgb(229, 231, 235);
        private static readonly Color GreenOk    = Color.FromArgb(16,  185, 129);

        private readonly SolicitudBeca _sol;

        public FrmContratoBeca(SolicitudBeca sol)
        {
            _sol = sol;
            ConfigurarForm();
            ConstruirUI();
        }

        private void ConfigurarForm()
        {
            Text            = $"Contrato {_sol.NumeroContrato} — Nexum Bank";
            Size            = new Size(640, 740);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            BackColor       = BgGray;
            MaximizeBox     = false;

            Load += (s, e) =>
            {
                if (Width > 0 && Height > 0)
                {
                    var p = new GraphicsPath();
                    int r = 20;
                    p.AddArc(0,         0,          r, r, 180, 90);
                    p.AddArc(Width - r, 0,          r, r, 270, 90);
                    p.AddArc(Width - r, Height - r, r, r,   0, 90);
                    p.AddArc(0,         Height - r, r, r,  90, 90);
                    p.CloseFigure();
                    Region = new Region(p);
                }
            };
        }

        private void ConstruirUI()
        {
            // ── Barra de acción superior ──────────────────────────────
            var pnlTopBar = new Panel
            {
                Size = new Size(640, 52), Location = Point.Empty,
                BackColor = IndigoDark
            };

            var btnCerrar = new Button
            {
                Text = "✕", Size = new Size(30, 30), Location = new Point(600, 11),
                BackColor = Color.FromArgb(60, 255, 255, 255), ForeColor = White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Click += (s, e) => Close();

            var btnPdf = new Button
            {
                Text = "⬇  Guardar PDF",
                Size = new Size(150, 34), Location = new Point(436, 9),
                BackColor = Indigo, ForeColor = White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnPdf.FlatAppearance.BorderSize = 0;
            btnPdf.Click += BtnGuardarPdf_Click;

            pnlTopBar.Controls.Add(new Label
            {
                Text = "Contrato de Concesión de Beca", ForeColor = White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true, BackColor = Color.Transparent, Location = new Point(20, 15)
            });
            pnlTopBar.Controls.Add(btnPdf);
            pnlTopBar.Controls.Add(btnCerrar);

            // ── Panel documento (scroll) ──────────────────────────────
            var scroll = new Panel
            {
                Location = new Point(0, 52), Size = new Size(640, 688),
                AutoScroll = true, BackColor = BgGray
            };

            var doc = ConstruirDocumento();
            doc.Location = new Point(20, 20);
            scroll.Controls.Add(doc);
            scroll.Resize += (s, e) => doc.Width = Math.Max(400, scroll.ClientSize.Width - 40);

            Controls.Add(pnlTopBar);
            Controls.Add(scroll);
        }

        private Panel ConstruirDocumento()
        {
            var doc = new Panel
            {
                Width = 600, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = White
            };
            doc.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RRect(new Rectangle(0, 0, doc.Width, doc.Height), 12))
                {
                    ev.Graphics.FillPath(new SolidBrush(White), path);
                    ev.Graphics.DrawPath(new Pen(Border, 1), path);
                }
            };

            int y = 0;

            // ── Cabecera del documento ────────────────────────────────
            var pnlCab = new Panel { Location = new Point(0, 0), Size = new Size(600, 80) };
            pnlCab.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new LinearGradientBrush(Point.Empty, new Point(600, 80), IndigoDark, Indigo))
                using (var path = RRect(new Rectangle(0, 0, 600, 80), 12))
                {
                    ev.Graphics.FillPath(br, path);
                    ev.Graphics.FillRectangle(br, new Rectangle(0, 10, 600, 70));
                }
            };
            pnlCab.Controls.Add(new Label
            {
                Text = "NEXUM BANK", ForeColor = White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                AutoSize = true, BackColor = Color.Transparent, Location = new Point(28, 14)
            });
            pnlCab.Controls.Add(new Label
            {
                Text = "Fundación Nexum · Programa de Becas", ForeColor = Color.FromArgb(165, 180, 252),
                Font = new Font("Segoe UI", 9), AutoSize = true,
                BackColor = Color.Transparent, Location = new Point(30, 50)
            });
            doc.Controls.Add(pnlCab);
            y += 80;

            // ── Título contrato ───────────────────────────────────────
            y += 20;
            doc.Controls.Add(new Label
            {
                Text = "CONTRATO DE CONCESIÓN DE BECA",
                ForeColor = TextDark, Font = new Font("Segoe UI", 13, FontStyle.Bold),
                AutoSize = false, Size = new Size(560, 26),
                BackColor = Color.Transparent, Location = new Point(20, y),
                TextAlign = ContentAlignment.MiddleCenter
            });
            y += 30;

            // Línea separadora
            doc.Controls.Add(new Panel
            {
                Location = new Point(20, y), Size = new Size(560, 1), BackColor = Indigo
            });
            y += 12;

            // Número contrato y fecha
            doc.Controls.Add(new Label
            {
                Text = $"Nº  {_sol.NumeroContrato}    |    Fecha: {(_sol.FechaResolucion ?? DateTime.Now):dd/MM/yyyy}",
                ForeColor = TextGray, Font = new Font("Segoe UI", 9),
                AutoSize = false, Size = new Size(560, 20),
                BackColor = Color.Transparent, Location = new Point(20, y),
                TextAlign = ContentAlignment.MiddleCenter
            });
            y += 32;

            // ── Sección beneficiario ──────────────────────────────────
            y = AgregarSeccionTitulo(doc, "DATOS DEL BENEFICIARIO", y);

            string nombreUsuario = SesionActual.Instancia?.Usuario?.NombreCompleto ?? _sol.NombreUsuario ?? "—";
            string emailUsuario  = SesionActual.Instancia?.Usuario?.Email          ?? _sol.EmailUsuario  ?? "—";
            string dniUsuario    = SesionActual.Instancia?.Usuario?.DNI            ?? "—";

            y = AgregarFilaDatos(doc, "Nombre completo", nombreUsuario, y);
            y = AgregarFilaDatos(doc, "Email",           emailUsuario,  y);
            y = AgregarFilaDatos(doc, "DNI / NIE",       dniUsuario,    y);
            y += 8;

            // ── Sección beca ──────────────────────────────────────────
            y = AgregarSeccionTitulo(doc, "DATOS DE LA BECA", y);
            y = AgregarFilaDatos(doc, "Título",          _sol.BecaTitulo,                   y);
            y = AgregarFilaDatos(doc, "Entidad",         "Nexum Bank / Fundación Nexum",    y);
            y = AgregarFilaDatos(doc, "Importe",         $"{_sol.BecaImporte:N0} €",         y);
            y = AgregarFilaDatos(doc, "Estado",          "✓  Concedida",                    y, GreenOk);
            y = AgregarFilaDatos(doc, "Nº de contrato",  _sol.NumeroContrato,               y);
            y += 8;

            // ── Cláusulas ─────────────────────────────────────────────
            y = AgregarSeccionTitulo(doc, "CLÁUSULAS Y COMPROMISOS", y);

            string[] clausulas =
            {
                "1.  El beneficiario se compromete a usar la beca exclusivamente para los fines académicos declarados.",
                "2.  Nexum Bank se reserva el derecho a solicitar justificantes del uso de los fondos concedidos.",
                "3.  El incumplimiento de las bases podrá implicar la devolución total o parcial del importe.",
                "4.  La beca es personal e intransferible y no podrá cederse a terceros.",
                "5.  El beneficiario autoriza a Nexum Bank a publicar su nombre en la lista de becarios del programa."
            };

            foreach (var clausula in clausulas)
            {
                var lbl = new Label
                {
                    Text = clausula, ForeColor = TextGray,
                    Font = new Font("Segoe UI", 8.5f),
                    AutoSize = false, Size = new Size(556, 0),
                    BackColor = Color.Transparent, Location = new Point(22, y)
                };
                lbl.Height = lbl.GetPreferredSize(new Size(556, 0)).Height + 4;
                doc.Controls.Add(lbl);
                y += lbl.Height + 4;
            }
            y += 10;

            // ── Firma ─────────────────────────────────────────────────
            doc.Controls.Add(new Panel
            {
                Location = new Point(20, y), Size = new Size(560, 1), BackColor = Border
            });
            y += 16;

            var pnlFirmas = new Panel
            {
                Location = new Point(20, y), Size = new Size(560, 80), BackColor = Color.Transparent
            };

            // Firma beneficiario
            AgregarBloqueiFirma(pnlFirmas, 0,   nombreUsuario, "Beneficiario");
            // Sello Nexum
            AgregarBloqueiFirma(pnlFirmas, 280, "Nexum Bank",  "Entidad concedente");

            doc.Controls.Add(pnlFirmas);
            y += 96;

            // Pie de página
            doc.Controls.Add(new Label
            {
                Text = "Documento generado electrónicamente por Nexum Bank · Válido sin firma manuscrita",
                ForeColor = Color.FromArgb(156, 163, 175), Font = new Font("Segoe UI", 7.5f),
                AutoSize = false, Size = new Size(560, 18),
                BackColor = Color.Transparent, Location = new Point(20, y),
                TextAlign = ContentAlignment.MiddleCenter
            });
            y += 30;

            doc.Height = y;
            return doc;
        }

        private int AgregarSeccionTitulo(Panel doc, string titulo, int y)
        {
            var pnlTit = new Panel { Location = new Point(20, y), Size = new Size(560, 26), BackColor = Color.FromArgb(238, 242, 255) };
            pnlTit.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RRect(new Rectangle(0, 0, pnlTit.Width, pnlTit.Height), 6))
                    ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(238, 242, 255)), path);
            };
            pnlTit.Controls.Add(new Label
            {
                Text = titulo, ForeColor = Indigo,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                AutoSize = true, BackColor = Color.Transparent, Location = new Point(10, 5)
            });
            doc.Controls.Add(pnlTit);
            return y + 34;
        }

        private int AgregarFilaDatos(Panel doc, string etiqueta, string valor, int y, Color? colorValor = null)
        {
            doc.Controls.Add(new Label
            {
                Text = etiqueta + ":", ForeColor = TextGray,
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = false, Size = new Size(160, 22),
                BackColor = Color.Transparent, Location = new Point(24, y),
                TextAlign = ContentAlignment.MiddleLeft
            });
            doc.Controls.Add(new Label
            {
                Text = valor, ForeColor = colorValor ?? TextDark,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                AutoSize = false, Size = new Size(390, 22),
                BackColor = Color.Transparent, Location = new Point(190, y),
                TextAlign = ContentAlignment.MiddleLeft
            });
            doc.Controls.Add(new Panel
            {
                Location = new Point(24, y + 22), Size = new Size(552, 1), BackColor = Border
            });
            return y + 30;
        }

        private void AgregarBloqueiFirma(Panel parent, int x, string nombre, string rol)
        {
            parent.Controls.Add(new Panel
            {
                Location = new Point(x, 0), Size = new Size(220, 1), BackColor = TextDark
            });
            parent.Controls.Add(new Label
            {
                Text = nombre, ForeColor = TextDark,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = true, BackColor = Color.Transparent, Location = new Point(x, 10)
            });
            parent.Controls.Add(new Label
            {
                Text = rol, ForeColor = TextGray, Font = new Font("Segoe UI", 8),
                AutoSize = true, BackColor = Color.Transparent, Location = new Point(x, 30)
            });
            parent.Controls.Add(new Label
            {
                Text = $"Fecha: {DateTime.Now:dd/MM/yyyy}",
                ForeColor = TextGray, Font = new Font("Segoe UI", 8),
                AutoSize = true, BackColor = Color.Transparent, Location = new Point(x, 50)
            });
        }

        // ── Generación PDF ────────────────────────────────────────────
        private void BtnGuardarPdf_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog
            {
                Title            = "Guardar contrato PDF",
                Filter           = "PDF (*.pdf)|*.pdf",
                FileName         = $"Contrato_{_sol.NumeroContrato}.pdf",
                DefaultExt       = "pdf",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    GenerarPDF(dlg.FileName);
                    MessageBox.Show(
                        $"PDF guardado correctamente en:\n{dlg.FileName}",
                        "PDF generado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al generar el PDF:\n{ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GenerarPDF(string rutaDestino)
        {
            var pdfDoc = new PdfDocument();
            pdfDoc.Info.Title   = $"Contrato Beca {_sol.NumeroContrato}";
            pdfDoc.Info.Author  = "Nexum Bank";
            pdfDoc.Info.Subject = "Contrato de Concesión de Beca";

            var page = pdfDoc.AddPage();
            page.Width  = XUnit.FromMillimeter(210);
            page.Height = XUnit.FromMillimeter(297);

            var gfx = XGraphics.FromPdfPage(page);

            double w = page.Width.Point;

            var fTitulo    = new XFont("Arial", 20, XFontStyle.Bold);
            var fSubtitulo = new XFont("Arial", 10, XFontStyle.Regular);
            var fSeccion   = new XFont("Arial",  9, XFontStyle.Bold);
            var fEtiqueta  = new XFont("Arial",  9, XFontStyle.Regular);
            var fValor     = new XFont("Arial",  9, XFontStyle.Bold);
            var fClasula   = new XFont("Arial",  8, XFontStyle.Regular);
            var fPie       = new XFont("Arial",  7, XFontStyle.Italic);

            var cIndigo  = XColor.FromArgb(99,  102, 241);
            var cDark    = XColor.FromArgb(17,  24,  39);
            var cGray    = XColor.FromArgb(107, 114, 128);
            var cBorder  = XColor.FromArgb(229, 231, 235);
            var cBgSeccion = XColor.FromArgb(238, 242, 255);
            var cGreen   = XColor.FromArgb(16,  185, 129);

            double y = 0;

            // Cabecera azul
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(49, 46, 129)), 0, 0, w, 70);
            gfx.DrawString("NEXUM BANK", fTitulo, XBrushes.White,
                new XRect(0, 12, w, 30), XStringFormats.TopCenter);
            gfx.DrawString("Fundación Nexum · Programa de Becas", fSubtitulo,
                new XSolidBrush(XColor.FromArgb(165, 180, 252)),
                new XRect(0, 46, w, 20), XStringFormats.TopCenter);
            y = 82;

            // Título del documento
            gfx.DrawString("CONTRATO DE CONCESIÓN DE BECA",
                new XFont("Arial", 13, XFontStyle.Bold), new XSolidBrush(cDark),
                new XRect(0, y, w, 22), XStringFormats.TopCenter);
            y += 26;

            gfx.DrawLine(new XPen(cIndigo, 1), 30, y, w - 30, y);
            y += 8;

            string nombreU = SesionActual.Instancia?.Usuario?.NombreCompleto ?? _sol.NombreUsuario ?? "—";
            string emailU  = SesionActual.Instancia?.Usuario?.Email          ?? _sol.EmailUsuario  ?? "—";
            string dniU    = SesionActual.Instancia?.Usuario?.DNI            ?? "—";
            string fecha   = (_sol.FechaResolucion ?? DateTime.Now).ToString("dd/MM/yyyy");

            gfx.DrawString($"Nº {_sol.NumeroContrato}    |    Fecha: {fecha}",
                fEtiqueta, new XSolidBrush(cGray),
                new XRect(0, y, w, 16), XStringFormats.TopCenter);
            y += 24;

            // Secciones de datos
            y = DibujarSeccionPDF(gfx, "DATOS DEL BENEFICIARIO", cBgSeccion, cIndigo, fSeccion, w, y);
            y = DibujarFilaPDF(gfx, "Nombre completo", nombreU,    fEtiqueta, fValor, cGray, cDark,  w, y);
            y = DibujarFilaPDF(gfx, "Email",           emailU,     fEtiqueta, fValor, cGray, cDark,  w, y);
            y = DibujarFilaPDF(gfx, "DNI / NIE",       dniU,       fEtiqueta, fValor, cGray, cDark,  w, y);
            y += 6;

            y = DibujarSeccionPDF(gfx, "DATOS DE LA BECA", cBgSeccion, cIndigo, fSeccion, w, y);
            y = DibujarFilaPDF(gfx, "Título",         _sol.BecaTitulo,          fEtiqueta, fValor, cGray, cDark,  w, y);
            y = DibujarFilaPDF(gfx, "Entidad",        "Nexum Bank / Fundación Nexum", fEtiqueta, fValor, cGray, cDark, w, y);
            y = DibujarFilaPDF(gfx, "Importe",        $"{_sol.BecaImporte:N0} €", fEtiqueta, fValor, cGray, cDark, w, y);
            y = DibujarFilaPDF(gfx, "Estado",         "Concedida",              fEtiqueta, fValor, cGray, cGreen, w, y);
            y = DibujarFilaPDF(gfx, "Nº de contrato", _sol.NumeroContrato,      fEtiqueta, fValor, cGray, cDark,  w, y);
            y += 6;

            // Cláusulas
            y = DibujarSeccionPDF(gfx, "CLÁUSULAS Y COMPROMISOS", cBgSeccion, cIndigo, fSeccion, w, y);
            string[] clausulas =
            {
                "1.  El beneficiario se compromete a usar la beca exclusivamente para los fines académicos declarados.",
                "2.  Nexum Bank se reserva el derecho a solicitar justificantes del uso de los fondos concedidos.",
                "3.  El incumplimiento de las bases podrá implicar la devolución total o parcial del importe.",
                "4.  La beca es personal e intransferible y no podrá cederse a terceros.",
                "5.  El beneficiario autoriza a Nexum Bank a publicar su nombre en la lista de becarios."
            };
            foreach (var c in clausulas)
            {
                gfx.DrawString(c, fClasula, new XSolidBrush(cGray),
                    new XRect(30, y, w - 60, 14), XStringFormats.TopLeft);
                y += 14;
            }
            y += 10;

            // Firmas
            gfx.DrawLine(new XPen(cBorder, 0.5), 30, y, w - 30, y);
            y += 14;
            gfx.DrawLine(new XPen(cDark, 0.8), 40,      y + 30, 220, y + 30);
            gfx.DrawLine(new XPen(cDark, 0.8), w - 220, y + 30, w - 40, y + 30);
            gfx.DrawString(nombreU, fValor, new XSolidBrush(cDark), new XRect(40,      y + 34, 180, 14), XStringFormats.TopLeft);
            gfx.DrawString("Nexum Bank",  fValor, new XSolidBrush(cDark), new XRect(w - 220, y + 34, 180, 14), XStringFormats.TopLeft);
            gfx.DrawString("Beneficiario", fEtiqueta, new XSolidBrush(cGray), new XRect(40,      y + 50, 180, 12), XStringFormats.TopLeft);
            gfx.DrawString("Entidad concedente", fEtiqueta, new XSolidBrush(cGray), new XRect(w - 220, y + 50, 180, 12), XStringFormats.TopLeft);
            y += 72;

            // Pie
            gfx.DrawString(
                "Documento generado electrónicamente por Nexum Bank · Válido sin firma manuscrita",
                fPie, new XSolidBrush(XColor.FromArgb(156, 163, 175)),
                new XRect(0, y, w, 12), XStringFormats.TopCenter);

            pdfDoc.Save(rutaDestino);
        }

        private double DibujarSeccionPDF(XGraphics g, string titulo, XColor bgColor, XColor textColor,
            XFont font, double w, double y)
        {
            g.DrawRectangle(new XSolidBrush(bgColor), 30, y, w - 60, 18);
            g.DrawString(titulo, font, new XSolidBrush(textColor), new XRect(36, y + 2, w - 72, 14), XStringFormats.TopLeft);
            return y + 24;
        }

        private double DibujarFilaPDF(XGraphics g, string etiqueta, string valor,
            XFont fEt, XFont fVal, XColor cEt, XColor cVal, double w, double y)
        {
            g.DrawString(etiqueta + ":", fEt, new XSolidBrush(cEt), new XRect(36,      y, 140, 16), XStringFormats.TopLeft);
            g.DrawString(valor,          fVal, new XSolidBrush(cVal), new XRect(180,    y, w - 210, 16), XStringFormats.TopLeft);
            g.DrawLine(new XPen(XColor.FromArgb(229, 231, 235), 0.5), 36, y + 18, w - 36, y + 18);
            return y + 24;
        }

        private static GraphicsPath RRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X,         r.Y,          d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y,          d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d,   0, 90);
            path.AddArc(r.X,         r.Bottom - d, d, d,  90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
