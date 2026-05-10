using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NexumApp.Forms.Prestamos
{
    internal class FrmPrestamoAprobado : Form
    {
        private static readonly Color Verde      = Color.FromArgb(16,  185, 129);
        private static readonly Color VerdeDark  = Color.FromArgb(5,   150, 105);
        private static readonly Color VerdeLight = Color.FromArgb(209, 250, 229);
        private static readonly Color Indigo     = Color.FromArgb(99,  102, 241);
        private static readonly Color White      = Color.White;
        private static readonly Color BgGray     = Color.FromArgb(244, 246, 252);
        private static readonly Color TextDark   = Color.FromArgb(17,  24,  39);
        private static readonly Color TextGray   = Color.FromArgb(107, 114, 128);
        private static readonly Color Border     = Color.FromArgb(229, 231, 235);

        private readonly decimal _monto;
        private readonly decimal _cuota;
        private readonly int     _plazo;
        private readonly string  _tipo;
        private readonly string  _referencia;

        public bool VerPrestamos { get; private set; } = false;

        public FrmPrestamoAprobado(string tipo, decimal monto, decimal cuota, int plazo)
        {
            _tipo       = tipo;
            _monto      = monto;
            _cuota      = cuota;
            _plazo      = plazo;
            _referencia = $"NXM-PRE-{DateTime.Now.Year}-{DateTime.Now:MMddHHmmss}";

            ConfigurarForm();
            ConstruirUI();
        }

        private void ConfigurarForm()
        {
            Text            = "Préstamo Concedido — Nexum Bank";
            Size            = new Size(460, 500);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            BackColor       = BgGray;
            MaximizeBox     = false;

            Load += (s, e) =>
            {
                if (Width <= 0 || Height <= 0) return;
                var p = new GraphicsPath(); int r = 20;
                p.AddArc(0,         0,          r, r, 180, 90);
                p.AddArc(Width - r, 0,          r, r, 270, 90);
                p.AddArc(Width - r, Height - r, r, r,   0, 90);
                p.AddArc(0,         Height - r, r, r,  90, 90);
                p.CloseFigure();
                Region = new Region(p);
            };
        }

        private void ConstruirUI()
        {
            // ── Header verde ──────────────────────────────────────────
            var pnlHeader = new Panel { Size = new Size(460, 140), Location = Point.Empty };
            pnlHeader.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new LinearGradientBrush(Point.Empty, new Point(460, 140), VerdeDark, Verde))
                    g.FillRectangle(br, pnlHeader.ClientRectangle);
                // Círculos decorativos
                using (var br2 = new SolidBrush(Color.FromArgb(20, 255, 255, 255)))
                {
                    g.FillEllipse(br2, 340, -40, 180, 180);
                    g.FillEllipse(br2, 400,  60,  90,  90);
                }
                // Círculo con checkmark
                var circRect = new Rectangle(160, 28, 84, 84);
                g.FillEllipse(new SolidBrush(Color.FromArgb(40, 255, 255, 255)), circRect);
                g.FillEllipse(new SolidBrush(Color.FromArgb(60, 255, 255, 255)), new Rectangle(168, 36, 68, 68));
                using (var pen = new Pen(White, 5f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
                    g.DrawLines(pen, new[] { new PointF(184f, 74f), new PointF(198f, 90f), new PointF(224f, 58f) });
            };
            Controls.Add(pnlHeader);

            int y = 148;

            // ── Título ────────────────────────────────────────────────
            Controls.Add(new Label
            {
                Text = "¡Préstamo Concedido!",
                ForeColor = Verde, Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = false, Size = new Size(428, 32),
                BackColor = Color.Transparent, Location = new Point(16, y),
                TextAlign = ContentAlignment.MiddleCenter
            });
            y += 36;

            Controls.Add(new Label
            {
                Text = $"Tu préstamo {_tipo} ha sido aprobado y está listo.",
                ForeColor = TextGray, Font = new Font("Segoe UI", 10),
                AutoSize = false, Size = new Size(428, 20),
                BackColor = Color.Transparent, Location = new Point(16, y),
                TextAlign = ContentAlignment.MiddleCenter
            });
            y += 32;

            // ── Tarjeta resumen ───────────────────────────────────────
            var pnlCard = new Panel { Location = new Point(16, y), Size = new Size(428, 168), BackColor = White };
            pnlCard.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RRect(new Rectangle(0, 0, pnlCard.Width, pnlCard.Height), 14))
                {
                    ev.Graphics.FillPath(new SolidBrush(White), path);
                    ev.Graphics.DrawPath(new Pen(Border, 1), path);
                }
                ev.Graphics.FillRectangle(new SolidBrush(Verde), new Rectangle(0, 12, 4, pnlCard.Height - 24));
            };

            // Nº Referencia
            pnlCard.Controls.Add(new Label { Text = "Nº de referencia", ForeColor = TextGray, Font = new Font("Segoe UI", 8), AutoSize = true, BackColor = Color.Transparent, Location = new Point(16, 12) });
            pnlCard.Controls.Add(new Label { Text = _referencia, ForeColor = Indigo, Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(16, 28) });

            // Separador
            pnlCard.Controls.Add(new Panel { Location = new Point(16, 50), Size = new Size(396, 1), BackColor = Border });

            // 4 métricas en 2 filas
            AgregarMetrica(pnlCard, 16,  62, "Importe concedido",  $"{_monto:N0} €",     Verde);
            AgregarMetrica(pnlCard, 220, 62, "Cuota mensual",      $"{_cuota:N2} €/mes", Indigo);
            AgregarMetrica(pnlCard, 16,  110, "Plazo",             $"{_plazo} meses",    TextDark);
            AgregarMetrica(pnlCard, 220, 110, "Primer pago",
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).ToString("dd/MM/yyyy"), TextDark);

            Controls.Add(pnlCard);
            y += 184;

            // ── Botones ───────────────────────────────────────────────
            var btnVerPrestamos = new Button
            {
                Text = "Ver mis préstamos",
                Size = new Size(428, 46), Location = new Point(16, y),
                BackColor = Verde, ForeColor = White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnVerPrestamos.FlatAppearance.BorderSize = 0;
            btnVerPrestamos.Click += (s, e) => { VerPrestamos = true; DialogResult = DialogResult.OK; Close(); };
            Controls.Add(btnVerPrestamos);
            y += 54;

            var btnCerrar = new Button
            {
                Text = "Cerrar",
                Size = new Size(428, 34), Location = new Point(16, y),
                BackColor = Color.Transparent, ForeColor = TextGray,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
            Controls.Add(btnCerrar);
        }

        private void AgregarMetrica(Panel parent, int x, int y, string label, string valor, Color colorValor)
        {
            parent.Controls.Add(new Label { Text = label, ForeColor = TextGray, Font = new Font("Segoe UI", 8), AutoSize = true, BackColor = Color.Transparent, Location = new Point(x, y) });
            parent.Controls.Add(new Label { Text = valor, ForeColor = colorValor, Font = new Font("Segoe UI", 11, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(x, y + 16) });
        }

        private static GraphicsPath RRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath(); int d = radius * 2;
            path.AddArc(r.X,         r.Y,          d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y,          d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d,   0, 90);
            path.AddArc(r.X,         r.Bottom - d, d, d,  90, 90);
            path.CloseFigure(); return path;
        }
    }
}
