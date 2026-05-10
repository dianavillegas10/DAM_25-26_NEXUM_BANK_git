using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NexumApp.Forms.Prestamos
{
    internal class FrmDetallePrestamo : Form
    {
        private static readonly Color Indigo     = Color.FromArgb(99,  102, 241);
        private static readonly Color IndigoDark = Color.FromArgb(49,  46,  129);
        private static readonly Color White      = Color.White;
        private static readonly Color BgGray     = Color.FromArgb(244, 246, 252);
        private static readonly Color TextDark   = Color.FromArgb(17,  24,  39);
        private static readonly Color TextGray   = Color.FromArgb(107, 114, 128);
        private static readonly Color Border     = Color.FromArgb(229, 231, 235);
        private static readonly Color GreenOk    = Color.FromArgb(16,  185, 129);
        private static readonly Color Amber      = Color.FromArgb(245, 158,  11);

        private readonly Prestamo _p;

        public FrmDetallePrestamo(Prestamo prestamo)
        {
            _p = prestamo;
            ConfigurarForm();
            ConstruirUI();
        }

        private void ConfigurarForm()
        {
            Text            = $"Préstamo {_p.TipoPrestamo} — Nexum Bank";
            Size            = new Size(560, 620);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            BackColor       = BgGray;
            MaximizeBox     = false;
            Load += (s, e) =>
            {
                if (Width <= 0 || Height <= 0) return;
                var path = new GraphicsPath(); int r = 20;
                path.AddArc(0, 0, r, r, 180, 90); path.AddArc(Width - r, 0, r, r, 270, 90);
                path.AddArc(Width - r, Height - r, r, r, 0, 90); path.AddArc(0, Height - r, r, r, 90, 90);
                path.CloseFigure(); Region = new Region(path);
            };
        }

        private void ConstruirUI()
        {
            // ── Header ────────────────────────────────────────────────
            var pnlHeader = new Panel { Size = new Size(560, 90), Location = Point.Empty };
            pnlHeader.Paint += (s, ev) =>
            {
                var g = ev.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new LinearGradientBrush(Point.Empty, new Point(560, 90), IndigoDark, Indigo))
                    g.FillRectangle(br, pnlHeader.ClientRectangle);
                using (var br2 = new SolidBrush(Color.FromArgb(20, 255, 255, 255)))
                { g.FillEllipse(br2, 420, -40, 200, 200); g.FillEllipse(br2, 500, 30, 100, 100); }
            };
            var btnX = new Button { Text = "✕", Size = new Size(30, 30), Location = new Point(520, 10), BackColor = Color.FromArgb(60, 255, 255, 255), ForeColor = White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnX.FlatAppearance.BorderSize = 0; btnX.Click += (s, e) => Close();
            pnlHeader.Controls.Add(new Label { Text = $"{_p.EmojiTipo}  Préstamo {_p.TipoPrestamo}", ForeColor = White, Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(24, 14) });
            pnlHeader.Controls.Add(new Label { Text = $"Concedido el {(_p.FechaAprobacion ?? _p.FechaSolicitud):dd/MM/yyyy}", ForeColor = Color.FromArgb(165, 180, 252), Font = new Font("Segoe UI", 10), AutoSize = true, BackColor = Color.Transparent, Location = new Point(26, 54) });
            pnlHeader.Controls.Add(btnX);

            int y = 96;

            // ── Gráfico circular (progreso amortización) ──────────────
            var pnlCirculo = new Panel { Location = new Point(16, y), Size = new Size(528, 180), BackColor = White };
            pnlCirculo.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RRect(new Rectangle(0, 0, pnlCirculo.Width, pnlCirculo.Height), 14))
                { ev.Graphics.FillPath(new SolidBrush(White), path); ev.Graphics.DrawPath(new Pen(Border, 1), path); }
            };

            // Panel del gráfico (izquierda)
            var pnlGraf = new Panel { Location = new Point(0, 0), Size = new Size(200, 180), BackColor = Color.Transparent };
            float pct = (float)_p.PorcentajePagado;
            pnlGraf.Paint += (s, ev) =>
            {
                var g = ev.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(30, 20, 140, 140);
                // Fondo gris
                using (var pen = new Pen(Color.FromArgb(229, 231, 235), 14)) g.DrawArc(pen, rect, -90, 360);
                // Arco de progreso
                if (pct > 0)
                {
                    Color arcColor = pct >= 75 ? GreenOk : pct >= 40 ? Amber : Indigo;
                    using (var pen = new Pen(arcColor, 14) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        g.DrawArc(pen, rect, -90, 360f * pct / 100f);
                }
                // Texto central
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var f = new Font("Segoe UI", 20, FontStyle.Bold))
                using (var br = new SolidBrush(Indigo))
                    g.DrawString($"{pct:F0}%", f, br, new RectangleF(30, 20, 140, 140), fmt);
                using (var f = new Font("Segoe UI", 8))
                using (var br = new SolidBrush(TextGray))
                    g.DrawString("amortizado", f, br, new RectangleF(30, 68, 140, 140), fmt);
            };

            // Métricas (derecha del gráfico)
            var pnlMet = new Panel { Location = new Point(200, 0), Size = new Size(328, 180), BackColor = Color.Transparent };
            decimal monto  = _p.MontoAprobado ?? _p.MontoSolicitado;
            decimal pagado = monto - (_p.SaldoPendiente ?? monto);
            AgregarMetrica(pnlMet, 0,   16, "Importe total",    $"{monto:N0} €",                           TextDark);
            AgregarMetrica(pnlMet, 0,   58, "Pagado",           $"{pagado:N0} €",                          GreenOk);
            AgregarMetrica(pnlMet, 164, 58, "Pendiente",        $"{(_p.SaldoPendiente ?? monto):N0} €",    Amber);
            AgregarMetrica(pnlMet, 0,  100, "Cuota mensual",    $"{_p.CuotaMensual:N2} €",                 Indigo);
            AgregarMetrica(pnlMet, 164,100, "TIN",              $"{_p.TasaInteres}%",                      TextDark);
            AgregarMetrica(pnlMet, 0,  142, "Plazo",            $"{_p.PlazoMeses} meses",                  TextDark);
            AgregarMetrica(pnlMet, 164,142, "Próximo pago",     _p.ProximoPago.HasValue ? _p.ProximoPago.Value.ToString("dd/MM/yyyy") : "—", _p.ProximoPago.HasValue && _p.ProximoPago.Value <= DateTime.Today.AddDays(10) ? Amber : TextDark);

            pnlCirculo.Controls.Add(pnlGraf);
            pnlCirculo.Controls.Add(pnlMet);
            Controls.Add(pnlHeader);
            Controls.Add(pnlCirculo);
            y += 196;

            // ── Tabla de primeras cuotas (amortización) ───────────────
            var lblSec = new Label { Text = "Primeras cuotas — Tabla de amortización", ForeColor = TextDark, Font = new Font("Segoe UI", 11, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(16, y) };
            Controls.Add(lblSec);
            y += 28;

            var pnlTabla = new Panel { Location = new Point(16, y), Size = new Size(528, 200), BackColor = White };
            pnlTabla.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RRect(new Rectangle(0, 0, pnlTabla.Width, pnlTabla.Height), 12))
                { ev.Graphics.FillPath(new SolidBrush(White), path); ev.Graphics.DrawPath(new Pen(Border, 1), path); }
            };
            DibujarTablaAmortizacion(pnlTabla);
            Controls.Add(pnlTabla);
            y += 216;

            // ── Botón cerrar ──────────────────────────────────────────
            var btnCerrar = new Button { Text = "Cerrar", Location = new Point(16, y), Size = new Size(528, 40), BackColor = Color.FromArgb(238, 242, 255), ForeColor = Indigo, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Click += (s, e) => Close();
            Controls.Add(btnCerrar);
        }

        private void AgregarMetrica(Panel parent, int x, int y, string label, string valor, Color colorValor)
        {
            parent.Controls.Add(new Label { Text = label, ForeColor = TextGray, Font = new Font("Segoe UI", 7.5f), AutoSize = true, BackColor = Color.Transparent, Location = new Point(x + 8, y) });
            parent.Controls.Add(new Label { Text = valor, ForeColor = colorValor, Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(x + 8, y + 16) });
        }

        private void DibujarTablaAmortizacion(Panel parent)
        {
            decimal monto = _p.MontoAprobado ?? _p.MontoSolicitado;
            decimal tasa  = _p.TasaInteres;
            int     plazo = _p.PlazoMeses;
            decimal cuota = _p.CuotaMensual ?? PrestamoService.CalcularCuota(monto, plazo, tasa);

            // Cabecera
            var header = new Panel { Location = new Point(0, 0), Size = new Size(528, 28), BackColor = Color.FromArgb(238, 242, 255) };
            header.Controls.Add(new Label { Text = "Cuota", ForeColor = Indigo, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(16, 7) });
            header.Controls.Add(new Label { Text = "Capital",  ForeColor = Indigo, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(148, 7) });
            header.Controls.Add(new Label { Text = "Intereses",ForeColor = Indigo, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(260, 7) });
            header.Controls.Add(new Label { Text = "Saldo",    ForeColor = Indigo, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(390, 7) });
            parent.Controls.Add(header);

            // Filas (primeras 5 cuotas)
            decimal saldo = monto;
            int maxFilas  = Math.Min(5, plazo);
            for (int i = 1; i <= maxFilas; i++)
            {
                decimal intMes  = Math.Round(saldo * tasa / 100m / 12m, 2);
                decimal capital = Math.Round(cuota - intMes, 2);
                saldo          -= capital;
                if (saldo < 0) saldo = 0;

                int rowY = 28 + (i - 1) * 32;
                Color bg = i % 2 == 0 ? Color.FromArgb(249, 250, 251) : White;
                var row = new Panel { Location = new Point(0, rowY), Size = new Size(528, 32), BackColor = bg };
                row.Controls.Add(new Label { Text = $"Mes {i}",          ForeColor = TextGray, Font = new Font("Segoe UI", 8.5f), AutoSize = true, BackColor = Color.Transparent, Location = new Point(16, 8) });
                row.Controls.Add(new Label { Text = $"{capital:N2} €",   ForeColor = TextDark, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(148, 8) });
                row.Controls.Add(new Label { Text = $"{intMes:N2} €",    ForeColor = Amber,    Font = new Font("Segoe UI", 8.5f), AutoSize = true, BackColor = Color.Transparent, Location = new Point(260, 8) });
                row.Controls.Add(new Label { Text = $"{saldo:N2} €",     ForeColor = saldo > 0 ? Indigo : GreenOk, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(390, 8) });
                parent.Controls.Add(row);
            }

            if (plazo > 5)
                parent.Controls.Add(new Label { Text = $"... y {plazo - 5} cuotas más hasta completar el préstamo", ForeColor = TextGray, Font = new Font("Segoe UI", 8), AutoSize = true, BackColor = Color.Transparent, Location = new Point(16, 28 + maxFilas * 32 + 6) });
        }

        private static GraphicsPath RRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath(); int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90); path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure(); return path;
        }
    }
}
