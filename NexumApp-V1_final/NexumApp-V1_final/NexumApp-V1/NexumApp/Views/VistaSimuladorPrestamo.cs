using NexumApp.Forms.Prestamos;
using NexumApp.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NexumApp.Views
{
    public class VistaSimuladorPrestamo : UserControl
    {
        private static readonly Color Indigo      = Color.FromArgb(99,  102, 241);
        private static readonly Color IndigoDark  = Color.FromArgb(49,  46,  129);
        private static readonly Color IndigoLight = Color.FromArgb(165, 180, 252);
        private static readonly Color BgPage      = Color.FromArgb(244, 246, 252);
        private static readonly Color White       = Color.White;
        private static readonly Color TextDark    = Color.FromArgb(17,  24,  39);
        private static readonly Color TextGray    = Color.FromArgb(107, 114, 128);
        private static readonly Color Border      = Color.FromArgb(229, 231, 235);
        private static readonly Color GreenOk     = Color.FromArgb(16,  185, 129);

        private string   _tipoSeleccionado = "Personal";
        private TrackBar _tbMonto;
        private TrackBar _tbPlazo;
        private Label    _lblMontoVal;
        private Label    _lblPlazoVal;
        private Label    _lblCuota;
        private Label    _lblTotalIntereses;
        private Label    _lblTotalPagar;
        private Label    _lblTasa;
        private Panel[]  _tipoCards;

        public event EventHandler VolverAlPrestamos;

        private readonly string[] _tipos    = { "Personal", "Hipoteca", "Coche", "Estudios" };
        private readonly string[] _emojis   = { "💼",       "🏠",       "🚗",    "🎓" };

        public string  TipoFinal  => _tipoSeleccionado;
        public decimal MontoFinal => _tbMonto != null ? _tbMonto.Value * 1000m : 10000m;
        public int     PlazoFinal => _tbPlazo != null ? _tbPlazo.Value * 6     : 60;

        public VistaSimuladorPrestamo()
        {
            BackColor = BgPage;
            Dock      = DockStyle.Fill;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ConstruirUI();
            Recalcular();
        }

        private void ConstruirUI()
        {
            Controls.Clear();
            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent };
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlp.Controls.Add(ConstruirHeader(), 0, 0);

            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = BgPage, Padding = new Padding(28, 20, 28, 28) };
            var contenido = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent };
            scroll.Controls.Add(contenido);
            scroll.SizeChanged += (s, ev) => contenido.Width = Math.Max(400, scroll.ClientSize.Width - scroll.Padding.Horizontal);
            tlp.Controls.Add(scroll, 0, 1);
            Controls.Add(tlp);

            int y = 0;

            // ── Selector tipo ─────────────────────────────────────────
            contenido.Controls.Add(new Label { Text = "Tipo de préstamo", ForeColor = TextGray, Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(0, y) });
            y += 24;

            var flpTipos = new FlowLayoutPanel { Location = new Point(0, y), Height = 80, AutoSize = false, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent };
            contenido.SizeChanged += (s, ev) => { flpTipos.Width = contenido.Width; };
            _tipoCards = new Panel[4];
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                var card = CrearTipoCard(idx);
                _tipoCards[i] = card;
                flpTipos.Controls.Add(card);
            }
            contenido.Controls.Add(flpTipos);
            y += 96;

            // ── Slider importe ────────────────────────────────────────
            contenido.Controls.Add(new Label { Text = "Importe del préstamo", ForeColor = TextGray, Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(0, y) });
            _lblMontoVal = new Label { Text = "10.000 €", ForeColor = Indigo, Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };
            contenido.Controls.Add(_lblMontoVal);
            Action posMontoVal = () => _lblMontoVal.Location = new Point(contenido.Width - _lblMontoVal.Width, y);
            contenido.SizeChanged += (s, ev) => posMontoVal();
            y += 28;

            _tbMonto = new TrackBar { Location = new Point(0, y), Height = 40, Minimum = 1, Maximum = 100, Value = 10, TickFrequency = 10, TickStyle = TickStyle.None };
            contenido.SizeChanged += (s, ev) => _tbMonto.Width = contenido.Width;
            _tbMonto.ValueChanged += (s, ev) => { _lblMontoVal.Text = $"{_tbMonto.Value * 1000:N0} €"; posMontoVal(); Recalcular(); };
            contenido.Controls.Add(_tbMonto);
            y += 50;

            contenido.Controls.Add(new Label { Text = "1.000 €", ForeColor = TextGray, Font = new Font("Segoe UI", 8), AutoSize = true, BackColor = Color.Transparent, Location = new Point(0, y) });
            var lbl100k = new Label { Text = "100.000 €", ForeColor = TextGray, Font = new Font("Segoe UI", 8), AutoSize = true, BackColor = Color.Transparent };
            contenido.Controls.Add(lbl100k);
            contenido.SizeChanged += (s, ev) => lbl100k.Location = new Point(contenido.Width - lbl100k.Width, y);
            y += 28;

            // ── Slider plazo ──────────────────────────────────────────
            contenido.Controls.Add(new Label { Text = "Plazo de devolución", ForeColor = TextGray, Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(0, y) });
            _lblPlazoVal = new Label { Text = "60 meses (5 años)", ForeColor = Indigo, Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };
            contenido.Controls.Add(_lblPlazoVal);
            Action posPlazoVal = () => _lblPlazoVal.Location = new Point(contenido.Width - _lblPlazoVal.Width, y);
            contenido.SizeChanged += (s, ev) => posPlazoVal();
            y += 28;

            _tbPlazo = new TrackBar { Location = new Point(0, y), Height = 40, Minimum = 1, Maximum = 60, Value = 10, TickFrequency = 10, TickStyle = TickStyle.None };
            contenido.SizeChanged += (s, ev) => _tbPlazo.Width = contenido.Width;
            _tbPlazo.ValueChanged += (s, ev) =>
            {
                int meses = _tbPlazo.Value * 6;
                int anios = meses / 12; int mesRest = meses % 12;
                string txt = anios > 0 ? $"{meses} meses ({anios} año{(anios > 1 ? "s" : "")}{(mesRest > 0 ? $" {mesRest}m" : "")})" : $"{meses} meses";
                _lblPlazoVal.Text = txt; posPlazoVal(); Recalcular();
            };
            contenido.Controls.Add(_tbPlazo);
            y += 50;

            contenido.Controls.Add(new Label { Text = "6 meses", ForeColor = TextGray, Font = new Font("Segoe UI", 8), AutoSize = true, BackColor = Color.Transparent, Location = new Point(0, y) });
            var lbl360 = new Label { Text = "360 meses", ForeColor = TextGray, Font = new Font("Segoe UI", 8), AutoSize = true, BackColor = Color.Transparent };
            contenido.Controls.Add(lbl360);
            contenido.SizeChanged += (s, ev) => lbl360.Location = new Point(contenido.Width - lbl360.Width, y);
            y += 36;

            // ── Panel resultados ──────────────────────────────────────
            var pnlResult = new Panel { Location = new Point(0, y), Height = 130, BackColor = White };
            pnlResult.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RRect(new Rectangle(0, 0, pnlResult.Width, pnlResult.Height), 14))
                {
                    ev.Graphics.FillPath(new SolidBrush(White), path);
                    ev.Graphics.DrawPath(new Pen(Border, 1), path);
                }
                using (var br = new LinearGradientBrush(new Rectangle(0, 0, 5, pnlResult.Height), Indigo, Color.FromArgb(139, 92, 246), 90f))
                    ev.Graphics.FillRectangle(br, new Rectangle(0, 10, 5, pnlResult.Height - 20));
            };
            contenido.SizeChanged += (s, ev) => pnlResult.Width = contenido.Width;

            var tlpResult = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Color.Transparent, Padding = new Padding(20, 0, 16, 0) };
            tlpResult.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tlpResult.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpResult.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpResult.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _lblCuota          = new Label { Text = "—", ForeColor = Indigo,   Font = new Font("Segoe UI", 28, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };
            _lblTotalIntereses  = new Label { Text = "—", ForeColor = TextDark, Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };
            _lblTotalPagar      = new Label { Text = "—", ForeColor = TextDark, Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };
            _lblTasa            = new Label { Text = "TIN 8.50%", ForeColor = TextGray, Font = new Font("Segoe UI", 8), AutoSize = true, BackColor = Color.Transparent };

            var pnlC = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            pnlC.Controls.Add(new Label { Text = "Cuota mensual", ForeColor = TextGray, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(0, 22) });
            pnlC.Controls.Add(_lblCuota);
            pnlC.Controls.Add(_lblTasa);
            _lblCuota.Location = new Point(0, 42);
            _lblTasa.Location  = new Point(0, 96);

            var pnlI = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            pnlI.Controls.Add(new Label { Text = "Total intereses", ForeColor = TextGray, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(0, 22) });
            pnlI.Controls.Add(_lblTotalIntereses);
            _lblTotalIntereses.Location = new Point(0, 46);

            var pnlT = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            pnlT.Controls.Add(new Label { Text = "Total a pagar", ForeColor = TextGray, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(0, 22) });
            pnlT.Controls.Add(_lblTotalPagar);
            _lblTotalPagar.Location = new Point(0, 46);

            tlpResult.Controls.Add(pnlC, 0, 0);
            tlpResult.Controls.Add(pnlI, 1, 0);
            tlpResult.Controls.Add(pnlT, 2, 0);
            pnlResult.Controls.Add(tlpResult);
            contenido.Controls.Add(pnlResult);
            y += 146;

            // ── Botón solicitar ───────────────────────────────────────
            var btnSolicitar = new Button
            {
                Text = "Solicitar este préstamo  →",
                Location = new Point(0, y), Height = 48,
                BackColor = Indigo, ForeColor = White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnSolicitar.FlatAppearance.BorderSize = 0;
            btnSolicitar.Click += BtnSolicitar_Click;
            contenido.SizeChanged += (s, ev) => btnSolicitar.Width = contenido.Width;
            contenido.Controls.Add(btnSolicitar);
        }

        private Panel CrearTipoCard(int idx)
        {
            var card = new Panel { Size = new Size(120, 76), Margin = new Padding(0, 0, 8, 0), BackColor = White, Cursor = Cursors.Hand };
            bool esSeleccionado() => _tipoSeleccionado == _tipos[idx];

            card.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                bool sel = esSeleccionado();
                Color bg  = sel ? Color.FromArgb(238, 242, 255) : White;
                Color brd = sel ? Indigo : Border;
                using (var path = RRect(new Rectangle(0, 0, card.Width, card.Height), 12))
                {
                    ev.Graphics.FillPath(new SolidBrush(bg), path);
                    ev.Graphics.DrawPath(new Pen(brd, sel ? 2f : 1f), path);
                }
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var f = new Font("Segoe UI", 20)) ev.Graphics.DrawString(_emojis[idx], f, Brushes.Black, new RectangleF(0, 4, card.Width, 36), fmt);
                using (var f = new Font("Segoe UI", 8, FontStyle.Bold))
                using (var br = new SolidBrush(sel ? Indigo : TextGray))
                    ev.Graphics.DrawString(_tipos[idx], f, br, new RectangleF(0, 42, card.Width, 16), fmt);
                using (var f = new Font("Segoe UI", 7.5f))
                using (var br = new SolidBrush(sel ? Indigo : TextGray))
                    ev.Graphics.DrawString($"{PrestamoService.ObtenerTasa(_tipos[idx])}% TIN", f, br, new RectangleF(0, 58, card.Width, 14), fmt);
            };

            EventHandler click = (s, ev) =>
            {
                _tipoSeleccionado = _tipos[idx];
                foreach (var c in _tipoCards) c?.Invalidate();
                _lblTasa.Text = $"TIN {PrestamoService.ObtenerTasa(_tipoSeleccionado):F2}%";
                Recalcular();
            };
            card.Click += click;
            return card;
        }

        private void Recalcular()
        {
            if (_tbMonto == null || _tbPlazo == null) return;
            decimal monto = _tbMonto.Value * 1000m;
            int     plazo = _tbPlazo.Value * 6;
            decimal tasa  = PrestamoService.ObtenerTasa(_tipoSeleccionado);
            decimal cuota = PrestamoService.CalcularCuota(monto, plazo, tasa);
            decimal total = cuota * plazo;
            decimal intereses = total - monto;

            if (_lblCuota         != null) _lblCuota.Text         = $"{cuota:N2} €";
            if (_lblTotalIntereses != null) _lblTotalIntereses.Text = $"{intereses:N2} €";
            if (_lblTotalPagar    != null) _lblTotalPagar.Text    = $"{total:N2} €";
            if (_lblTasa          != null) _lblTasa.Text          = $"TIN {tasa:F2}%";
        }

        private void BtnSolicitar_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmSolicitarPrestamo(_tipoSeleccionado, (int)MontoFinal, PlazoFinal))
            {
                if (frm.ShowDialog(this.FindForm()) == DialogResult.OK)
                    VolverAlPrestamos?.Invoke(this, EventArgs.Empty);
            }
        }

        private Panel ConstruirHeader()
        {
            var pnl = new Panel { Dock = DockStyle.Fill };
            pnl.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new LinearGradientBrush(Point.Empty, new Point(pnl.Width, pnl.Height), IndigoDark, Indigo))
                    ev.Graphics.FillRectangle(br, pnl.ClientRectangle);
                using (var br2 = new SolidBrush(Color.FromArgb(20, 255, 255, 255)))
                { ev.Graphics.FillEllipse(br2, pnl.Width - 160, -50, 240, 240); ev.Graphics.FillEllipse(br2, pnl.Width - 50, 40, 110, 110); }
            };

            bool hov = false;
            var btnV = new Panel { Location = new Point(16, 12), Size = new Size(100, 28), BackColor = Color.Transparent, Cursor = Cursors.Hand };
            btnV.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                if (hov) using (var p = RRect(btnV.ClientRectangle, 8)) ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(40, 255, 255, 255)), p);
                Color tc = hov ? Color.White : Color.FromArgb(200, 255, 255, 255);
                int cy = btnV.Height / 2;
                using (var pen = new Pen(tc, 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                { ev.Graphics.DrawLine(pen, 18, cy, 10, cy); ev.Graphics.DrawLines(pen, new[] { new PointF(14f, cy - 4f), new PointF(10f, cy), new PointF(14f, cy + 4f) }); }
                TextRenderer.DrawText(ev.Graphics, "Préstamos", new Font("Segoe UI", 9.5f, FontStyle.Bold), new Rectangle(24, 0, btnV.Width - 26, btnV.Height), tc, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            };
            btnV.MouseEnter += (s, ev) => { hov = true;  btnV.Invalidate(); };
            btnV.MouseLeave += (s, ev) => { hov = false; btnV.Invalidate(); };
            btnV.Click      += (s, ev) => VolverAlPrestamos?.Invoke(this, EventArgs.Empty);

            pnl.Controls.Add(btnV);
            pnl.Controls.Add(new Label { Text = "🧮  Simulador de Préstamos", ForeColor = White, Font = new Font("Segoe UI", 20, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(36, 46) });
            pnl.Controls.Add(new Label { Text = "Calcula tu cuota en tiempo real", ForeColor = IndigoLight, Font = new Font("Segoe UI", 11), AutoSize = true, BackColor = Color.Transparent, Location = new Point(38, 92) });
            return pnl;
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
