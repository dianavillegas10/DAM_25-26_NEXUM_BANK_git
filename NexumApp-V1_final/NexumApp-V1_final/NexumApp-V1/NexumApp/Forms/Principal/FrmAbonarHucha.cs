using NexumApp.Models;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace NexumApp.Forms.Principal
{
    public class FrmAbonarHucha : Form
    {
        private static readonly CultureInfo ES     = CultureInfo.CreateSpecificCulture("es-ES");
        private static readonly Color       BgPage = Color.FromArgb(245, 247, 250);
        private static readonly Color       Oscuro = Color.FromArgb(15, 23, 42);
        private static readonly Color       Gris   = Color.FromArgb(100, 116, 139);
        private static readonly Color       Border = Color.FromArgb(214, 219, 228);

        private TextBox _txtMonto;
        private Label   _lblFalta;

        public decimal MontoIngresado { get; private set; }

        private readonly Hucha   _hucha;
        private readonly decimal _saldoDisponible;
        private readonly Color   _clr;

        public FrmAbonarHucha(Hucha hucha, decimal saldoDisponible)
        {
            _hucha           = hucha;
            _saldoDisponible = saldoDisponible;

            try { _clr = ColorTranslator.FromHtml(hucha.ColorHex ?? "#3B82F6"); }
            catch { _clr = Color.FromArgb(59, 130, 246); }

            Text            = "Abonar a hucha";
            Size            = new Size(440, 560);
            MinimumSize     = MaximumSize = new Size(440, 560);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            BackColor       = BgPage;
            Font            = new Font("Segoe UI", 10);
            BuildUI();
        }

        private void BuildUI()
        {
            int pct   = _hucha.Progreso;
            decimal falta = Math.Max(0, _hucha.MetaObjetivo - _hucha.SaldoActual);

            // ── HEADER (dibujado 100% en Paint) ───────────────────
            const int HH = 130;
            var header = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(440, HH),
                BackColor = _clr
            };
            header.Paint += (s, e) => PintarHeader(e.Graphics, header.ClientRectangle);
            Controls.Add(header);

            // ── CUERPO ────────────────────────────────────────────
            const int PX = 28;
            int y = HH + 18;

            // ── Tarjeta de estado ─────────────────────────────────
            var card = new Panel
            {
                Location  = new Point(PX, y),
                Size      = new Size(384, 90),
                BackColor = Color.White
            };
            EventHandler applyCard = (s, ev) =>
            {
                try { card.Region = new Region(RR(card.ClientRectangle, 14)); } catch { }
            };
            card.HandleCreated += applyCard;
            card.Resize        += applyCard;

            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = card.ClientRectangle;

                using (var path = RR(r, 14))
                {
                    g.FillPath(Brushes.White, path);
                    g.DrawPath(new Pen(Border, 1f), path);
                }

                // Franja lateral
                using (var path = RR(new Rectangle(0, 8, 5, r.Height - 16), 3))
                    g.FillPath(new SolidBrush(_clr), path);

                // Progreso y falta
                var fmtL = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                var fmtR = new StringFormat { Alignment = StringAlignment.Far,  LineAlignment = StringAlignment.Center };

                g.DrawString("Progreso", new Font("Segoe UI", 8.5f), new SolidBrush(Gris),
                    new RectangleF(16, 10, r.Width - 32, 18), fmtL);
                g.DrawString($"{pct}%", new Font("Segoe UI", 8.5f, FontStyle.Bold), new SolidBrush(_clr),
                    new RectangleF(16, 10, r.Width - 32, 18), fmtR);

                // Barra de progreso
                int bx = 16, by = 32, bw = r.Width - 32, bh = 10;
                using (var path = RR(new Rectangle(bx, by, bw, bh), 5))
                    g.FillPath(new SolidBrush(Color.FromArgb(229, 231, 235)), path);
                if (pct > 0)
                {
                    int fw = Math.Max(14, bw * pct / 100);
                    using (var path = RR(new Rectangle(bx, by, fw, bh), 5))
                    using (var br   = new LinearGradientBrush(
                        new Rectangle(bx, by, fw + 1, bh),
                        ControlPaint.Light(_clr, 0.4f), _clr,
                        LinearGradientMode.Horizontal))
                        g.FillPath(br, path);
                }

                // Saldo / Meta / Falta
                g.DrawString($"Ahorrado: {_hucha.SaldoActual.ToString("C0", ES)}",
                    new Font("Segoe UI", 8.5f), new SolidBrush(Gris),
                    new RectangleF(16, 50, r.Width / 2f, 20), fmtL);
                g.DrawString($"Falta: {falta.ToString("C0", ES)}",
                    new Font("Segoe UI", 8.5f, FontStyle.Bold), new SolidBrush(Oscuro),
                    new RectangleF(16, 50, r.Width - 32, 20), fmtR);
                g.DrawString($"Meta: {_hucha.MetaObjetivo.ToString("C0", ES)}",
                    new Font("Segoe UI", 8f), new SolidBrush(Gris),
                    new RectangleF(16, 66, r.Width - 32, 20), fmtR);
            };
            Controls.Add(card);
            y += 102;

            // ── Saldo disponible ──────────────────────────────────
            var pnlDisp = new Panel
            {
                Location  = new Point(PX, y),
                Size      = new Size(384, 36),
                BackColor = Color.FromArgb(239, 246, 255)
            };
            EventHandler applyDisp = (s, ev) =>
            {
                try { pnlDisp.Region = new Region(RR(pnlDisp.ClientRectangle, 10)); } catch { }
            };
            pnlDisp.HandleCreated += applyDisp;
            pnlDisp.Resize        += applyDisp;
            pnlDisp.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(pnlDisp.ClientRectangle, 10))
                    g.FillPath(new SolidBrush(Color.FromArgb(239, 246, 255)), path);
                var fmt = new StringFormat { LineAlignment = StringAlignment.Center };
                g.DrawString($"💳  Saldo disponible en cuenta:  {_saldoDisponible.ToString("C2", ES)}",
                    new Font("Segoe UI", 9f, FontStyle.Bold),
                    new SolidBrush(Color.FromArgb(59, 130, 246)),
                    new RectangleF(12, 0, pnlDisp.Width - 24, pnlDisp.Height), fmt);
            };
            Controls.Add(pnlDisp);
            y += 48;

            // ── Etiqueta importe ──────────────────────────────────
            var lblMonto = new Label
            {
                Text      = "¿Cuánto quieres ingresar?",
                Location  = new Point(PX, y),
                Size      = new Size(384, 20),
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Gris,
                BackColor = Color.Transparent
            };
            Controls.Add(lblMonto);
            y += 24;

            // ── TextBox importe ───────────────────────────────────
            var pnlInput = new Panel
            {
                Location  = new Point(PX, y),
                Size      = new Size(384, 52),
                BackColor = Color.White
            };
            EventHandler applyInput = (s, ev) =>
            {
                try { pnlInput.Region = new Region(RR(pnlInput.ClientRectangle, 12)); } catch { }
            };
            pnlInput.HandleCreated += applyInput;
            pnlInput.Resize        += applyInput;
            pnlInput.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(pnlInput.ClientRectangle, 12))
                {
                    e.Graphics.FillPath(Brushes.White, path);
                    e.Graphics.DrawPath(new Pen(_clr, 2f), path);
                }
            };

            _txtMonto = new TextBox
            {
                Location    = new Point(10, 10),
                Size        = new Size(280, 32),
                Font        = new Font("Segoe UI", 17, FontStyle.Bold),
                BorderStyle = BorderStyle.None,
                ForeColor   = _clr,
                BackColor   = Color.White,
                TextAlign   = HorizontalAlignment.Right,
                Text        = ""
            };
            _txtMonto.TextChanged += (s, e) => ActualizarFalta();

            var lblEur = new Label
            {
                Text      = "€",
                Location  = new Point(296, 12),
                Size      = new Size(30, 30),
                Font      = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = _clr,
                BackColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlInput.Controls.Add(_txtMonto);
            pnlInput.Controls.Add(lblEur);
            Controls.Add(pnlInput);
            y += 62;

            // ── Etiqueta "faltarán X €" ───────────────────────────
            _lblFalta = new Label
            {
                Location  = new Point(PX, y),
                Size      = new Size(384, 18),
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Gris,
                BackColor = Color.Transparent,
                Text      = ""
            };
            Controls.Add(_lblFalta);
            y += 28;

            // ── Botones rápidos ───────────────────────────────────
            string[] etiquetas = { "25 €", "50 €", "100 €", "Todo" };
            decimal[] valores  = { 25m, 50m, 100m, _saldoDisponible };
            int bqW = 82, bqGap = 8;
            for (int i = 0; i < etiquetas.Length; i++)
            {
                int idx = i;
                var bq = new Button
                {
                    Text      = etiquetas[i],
                    Location  = new Point(PX + idx * (bqW + bqGap), y),
                    Size      = new Size(bqW, 32),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(18, _clr.R, _clr.G, _clr.B),
                    ForeColor = _clr,
                    Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                    Cursor    = Cursors.Hand
                };
                bq.FlatAppearance.BorderColor = Color.FromArgb(60, _clr.R, _clr.G, _clr.B);
                bq.FlatAppearance.BorderSize  = 1;
                bq.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, _clr.R, _clr.G, _clr.B);
                EventHandler applyBq = (s, ev) =>
                {
                    try { bq.Region = new Region(RR(bq.ClientRectangle, 9)); } catch { }
                };
                bq.HandleCreated += applyBq;
                bq.Resize        += applyBq;

                decimal val = valores[idx];
                bq.Click += (s, ev) =>
                {
                    decimal v = Math.Min(val, _saldoDisponible);
                    _txtMonto.Text = v.ToString("F2", CultureInfo.InvariantCulture);
                };
                Controls.Add(bq);
            }
            y += 44;

            // ── Botones acción ────────────────────────────────────
            var btnOk = MakeBtn("✓  Abonar", new Point(PX, y), new Size(200, 46), _clr);
            btnOk.Click += BtnAbonar_Click;
            Controls.Add(btnOk);

            var btnNo = MakeBtn("Cancelar", new Point(PX + 210, y), new Size(120, 46),
                Color.FromArgb(241, 245, 249));
            btnNo.ForeColor = Gris;
            btnNo.FlatAppearance.MouseOverBackColor = Color.FromArgb(226, 232, 240);
            btnNo.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnNo);

            ActiveControl = _txtMonto;
        }

        // Dibuja el header completamente en GDI+ para evitar solapamientos de Labels
        private void PintarHeader(Graphics g, Rectangle r)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Gradiente de fondo
            using (var b = new LinearGradientBrush(r, _clr, ControlPaint.Dark(_clr, 0.18f), 135f))
                g.FillRectangle(b, r);

            // Burbujas decorativas
            using (var b2 = new SolidBrush(Color.FromArgb(22, 255, 255, 255)))
            {
                g.FillEllipse(b2, r.Width - 110, -50, 190, 190);
                g.FillEllipse(b2, -50, -30, 140, 140);
            }

            // Círculo del emoji
            int cx = 28, cy = (r.Height - 68) / 2, cSz = 68;
            using (var path = RR(new Rectangle(cx, cy, cSz, cSz), cSz / 2))
                g.FillPath(new SolidBrush(Color.FromArgb(50, 255, 255, 255)), path);

            // Emoji dibujado con DrawString (centrado en el círculo)
            string emoji = _hucha.Emoji ?? "🐷";
            var emojiFont = new Font("Segoe UI Emoji", 22);
            var emojiFmt  = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(emoji, emojiFont, Brushes.White,
                new RectangleF(cx, cy, cSz, cSz), emojiFmt);

            // Nombre de la hucha
            int tx = cx + cSz + 16;
            int tw = r.Width - tx - 20;
            var fmtL = new StringFormat { LineAlignment = StringAlignment.Near, Trimming = StringTrimming.EllipsisCharacter };
            g.DrawString(_hucha.Nombre,
                new Font("Segoe UI", 16, FontStyle.Bold),
                Brushes.White,
                new RectangleF(tx, cy + 4, tw, 32), fmtL);

            // Subtítulo: saldo · meta
            string sub = $"Ahorrado {_hucha.SaldoActual.ToString("C0", ES)}  ·  Meta {_hucha.MetaObjetivo.ToString("C0", ES)}";
            g.DrawString(sub,
                new Font("Segoe UI", 9f),
                new SolidBrush(Color.FromArgb(210, 255, 255, 255)),
                new RectangleF(tx, cy + 38, tw, 22), fmtL);
        }

        private void ActualizarFalta()
        {
            string raw = (_txtMonto.Text ?? "").Replace(",", ".");
            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal v) && v > 0)
            {
                decimal nuevoSaldo = _hucha.SaldoActual + v;
                decimal restante   = _hucha.MetaObjetivo - nuevoSaldo;
                if (restante <= 0)
                    _lblFalta.Text = $"¡Alcanzarías la meta de {_hucha.MetaObjetivo.ToString("C0", ES)}! 🎉";
                else
                    _lblFalta.Text = $"Tras abonar, faltarán {restante.ToString("C0", ES)} para la meta";
                _lblFalta.ForeColor = restante <= 0 ? Color.FromArgb(16, 185, 129) : Gris;
            }
            else
            {
                _lblFalta.Text = "";
            }
        }

        private void BtnAbonar_Click(object sender, EventArgs e)
        {
            string raw = (_txtMonto.Text ?? "").Replace(",", ".");
            if (!decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal monto) || monto <= 0)
            {
                MessageBox.Show("Introduce un importe válido mayor que 0.", "Nexum Bank",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtMonto.Focus();
                return;
            }
            if (monto > _saldoDisponible)
            {
                MessageBox.Show(
                    $"Saldo insuficiente.\nTienes {_saldoDisponible.ToString("C2", ES)} disponibles.",
                    "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtMonto.Focus();
                return;
            }

            MontoIngresado = monto;
            DialogResult   = DialogResult.OK;
            Close();
        }

        private static Button MakeBtn(string txt, Point loc, Size sz, Color bg)
        {
            var b = new Button
            {
                Text      = txt,
                Location  = loc,
                Size      = sz,
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(bg, 0.09f);
            EventHandler ar = (s, e) => { try { b.Region = new Region(RR(b.ClientRectangle, 12)); } catch { } };
            b.HandleCreated += ar;
            b.Resize        += ar;
            return b;
        }

        private static GraphicsPath RR(Rectangle r, int rad)
        {
            var p = new GraphicsPath();
            int d = rad * 2;
            if (r.Width < d || r.Height < d) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
