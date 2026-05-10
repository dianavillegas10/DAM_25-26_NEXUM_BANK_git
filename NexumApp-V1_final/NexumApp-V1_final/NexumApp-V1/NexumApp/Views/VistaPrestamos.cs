using NexumApp.Forms.Prestamos;
using NexumApp.Helpers;
using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace NexumApp.Views
{
    public class VistaPrestamos : UserControl
    {
        private static readonly Color Indigo      = Color.FromArgb(99,  102, 241);
        private static readonly Color IndigoDark  = Color.FromArgb(49,  46,  129);
        private static readonly Color IndigoLight = Color.FromArgb(165, 180, 252);
        private Color BgPage   => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(10,  12,  28)  : Color.FromArgb(244, 246, 252);
        private Color White    => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(18,  22,  46)  : Color.White;
        private Color TextDark => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(241, 245, 249) : Color.FromArgb(17,  24,  39);
        private Color TextGray => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(100, 116, 139) : Color.FromArgb(107, 114, 128);
        private Color Border   => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(38,  44,  80)  : Color.FromArgb(229, 231, 235);
        private static readonly Color GreenOk     = Color.FromArgb(16,  185, 129);
        private static readonly Color Amber       = Color.FromArgb(245, 158,  11);
        private static readonly Color RedWarn     = Color.FromArgb(239,  68,  68);

        private readonly PrestamoService _service = new PrestamoService();
        private FlowLayoutPanel _pnlLista;
        private Panel           _scroll;
        private List<Prestamo>  _prestamos = new List<Prestamo>();

        public event EventHandler VolverAlInicio;
        public event EventHandler SimuladorClicked;

        public VistaPrestamos()
        {
            BackColor = BgPage;
            Dock      = DockStyle.Fill;
            Helpers.AppSettings.ConfiguracionChanged += (s, e) =>
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(new Action(() => { ConstruirUI(); CargarDatos(); Helpers.AppSettings.AplicarTraduccionesRecursivo(this); }));
            };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ConstruirUI();
            CargarDatos();
        }

        private void ConstruirUI()
        {
            Controls.Clear();
            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2,
                BackColor = Color.Transparent
            };
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlp.Controls.Add(ConstruirHeader(), 0, 0);

            _scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = BgPage, Padding = new Padding(24, 16, 24, 24) };
            _pnlLista = new FlowLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown, WrapContents = false,
                BackColor = Color.Transparent
            };
            _scroll.Controls.Add(_pnlLista);
            _scroll.SizeChanged += (s, ev) => SincronizarAnchos();
            tlp.Controls.Add(_scroll, 0, 1);
            Controls.Add(tlp);
        }

        private void SincronizarAnchos()
        {
            int w = Math.Max(400, _scroll.ClientSize.Width - _scroll.Padding.Horizontal);
            _pnlLista.Width = w;
            foreach (Control c in _pnlLista.Controls) c.Width = w;
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
                {
                    ev.Graphics.FillEllipse(br2, pnl.Width - 160, -50, 240, 240);
                    ev.Graphics.FillEllipse(br2, pnl.Width - 50,   40, 110, 110);
                }
            };

            bool hov = false;
            var btnVolver = new Panel { Location = new Point(16, 12), Size = new Size(80, 28), BackColor = Color.Transparent, Cursor = Cursors.Hand };
            btnVolver.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = btnVolver.ClientRectangle;
                if (hov) using (var path = RRect(r, 8)) ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(40, 255, 255, 255)), path);
                Color tc = hov ? Color.White : Color.FromArgb(200, 255, 255, 255);
                int cy = r.Height / 2;
                using (var pen = new Pen(tc, 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    ev.Graphics.DrawLine(pen, 18, cy, 10, cy);
                    ev.Graphics.DrawLines(pen, new[] { new PointF(14f, cy - 4f), new PointF(10f, cy), new PointF(14f, cy + 4f) });
                }
                TextRenderer.DrawText(ev.Graphics, "Inicio", new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    new Rectangle(24, 0, r.Width - 26, r.Height), tc, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            };
            btnVolver.MouseEnter += (s, ev) => { hov = true;  btnVolver.Invalidate(); };
            btnVolver.MouseLeave += (s, ev) => { hov = false; btnVolver.Invalidate(); };
            btnVolver.Click      += (s, ev) => VolverAlInicio?.Invoke(this, EventArgs.Empty);

            // Botón Simulador
            bool simHov = false;
            var btnSim = new Panel { Size = new Size(120, 30), BackColor = Color.Transparent, Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnSim.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color bg = simHov ? Color.FromArgb(60, 255, 255, 255) : Color.FromArgb(35, 255, 255, 255);
                using (var path = RRect(new Rectangle(1, 1, btnSim.Width - 2, btnSim.Height - 2), 14))
                {
                    ev.Graphics.FillPath(new SolidBrush(bg), path);
                    ev.Graphics.DrawPath(new Pen(Color.FromArgb(100, 255, 255, 255), 1), path);
                }
                TextRenderer.DrawText(ev.Graphics, "🧮  Simulador", new Font("Segoe UI", 9, FontStyle.Bold),
                    new Rectangle(0, 0, btnSim.Width, btnSim.Height), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btnSim.MouseEnter += (s, ev) => { simHov = true;  btnSim.Invalidate(); };
            btnSim.MouseLeave += (s, ev) => { simHov = false; btnSim.Invalidate(); };
            btnSim.Click      += (s, ev) => SimuladorClicked?.Invoke(this, EventArgs.Empty);
            Action posSim = () => btnSim.Location = new Point(pnl.Width - btnSim.Width - 16, 12);
            pnl.Resize += (s, ev) => posSim();
            pnl.HandleCreated += (s, ev) => posSim();

            pnl.Controls.Add(btnVolver);
            pnl.Controls.Add(btnSim);
            pnl.Controls.Add(new Label { Text = "💳  Préstamos", ForeColor = White, Font = new Font("Segoe UI", 22, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(36, 46) });
            pnl.Controls.Add(new Label { Text = "Gestiona tus préstamos Nexum Bank", ForeColor = IndigoLight, Font = new Font("Segoe UI", 11), AutoSize = true, BackColor = Color.Transparent, Location = new Point(38, 92) });
            return pnl;
        }

        private void CargarDatos()
        {
            _pnlLista.Controls.Clear();
            int uid = SesionActual.Instancia?.Usuario?.Id ?? 0;
            _prestamos = _service.ObtenerPorUsuario(uid);

            // Tarjetas resumen
            var pnlResumen = CrearPanelResumen();
            pnlResumen.Margin = new Padding(0, 0, 0, 16);
            _pnlLista.Controls.Add(pnlResumen);

            // Botón solicitar
            var btnSolicitar = new Button
            {
                Text = "+ Solicitar nuevo préstamo",
                Height = 44, BackColor = Indigo, ForeColor = White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand, Margin = new Padding(0, 0, 0, 16)
            };
            btnSolicitar.FlatAppearance.BorderSize = 0;
            btnSolicitar.Click += (s, e) => AbrirSolicitud();
            _pnlLista.Controls.Add(btnSolicitar);

            if (_prestamos.Count == 0)
            {
                _pnlLista.Controls.Add(CrearEstadoVacio());
            }
            else
            {
                var lblSec = new Label { Text = "Mis Préstamos", ForeColor = TextDark, Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Margin = new Padding(0, 4, 0, 8) };
                _pnlLista.Controls.Add(lblSec);
                foreach (var p in _prestamos)
                {
                    var card = CrearCardPrestamo(p);
                    card.Margin = new Padding(0, 0, 0, 12);
                    _pnlLista.Controls.Add(card);
                }
            }

            BeginInvoke(new Action(SincronizarAnchos));
        }

        private Panel CrearPanelResumen()
        {
            var activos = _prestamos.Where(p => p.EsAprobado).ToList();
            decimal totalDeuda    = activos.Sum(p => p.SaldoPendiente ?? 0);
            decimal cuotaTotal    = activos.Sum(p => p.CuotaMensual  ?? 0);
            DateTime? proximoPago = activos.Where(p => p.ProximoPago.HasValue).Select(p => p.ProximoPago.Value).DefaultIfEmpty().Min();
            int numActivos        = activos.Count;

            var pnl = new Panel { Height = 90, BackColor = Color.Transparent };
            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = Color.Transparent };
            for (int i = 0; i < 4; i++) tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var chips = new[]
            {
                ("Total deuda",    totalDeuda > 0 ? $"{totalDeuda:N0} €"  : "0 €",          Indigo,   Color.FromArgb(238, 242, 255)),
                ("Cuota mensual",  cuotaTotal > 0 ? $"{cuotaTotal:N2} €"  : "0 €",          GreenOk,  Color.FromArgb(236, 253, 245)),
                ("Próximo pago",   proximoPago.HasValue ? proximoPago.Value.ToString("dd/MM/yyyy") : "—", Amber, Color.FromArgb(255, 251, 235)),
                ("Préstamos activos", numActivos.ToString(),                                 RedWarn,  Color.FromArgb(254, 242, 242))
            };

            for (int i = 0; i < 4; i++)
            {
                var (label, valor, textColor, bgColor) = chips[i];
                int idx = i;
                var chip = new Panel { Margin = new Padding(idx < 3 ? 0 : 0, 0, idx < 3 ? 8 : 0, 0), BackColor = bgColor };
                chip.Paint += (s, ev) =>
                {
                    ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var path = RRect(new Rectangle(0, 0, chip.Width, chip.Height), 12))
                    {
                        ev.Graphics.FillPath(new SolidBrush(bgColor), path);
                        ev.Graphics.DrawPath(new Pen(Color.FromArgb(20, 0, 0, 0), 1), path);
                    }
                };
                chip.Controls.Add(new Label { Text = valor, ForeColor = textColor, Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(14, 14) });
                chip.Controls.Add(new Label { Text = label, ForeColor = TextGray,   Font = new Font("Segoe UI", 8),                  AutoSize = true, BackColor = Color.Transparent, Location = new Point(14, 52) });
                tlp.Controls.Add(chip, i, 0);
            }

            pnl.Controls.Add(tlp);
            return pnl;
        }

        private Panel CrearCardPrestamo(Prestamo p)
        {
            var card = new Panel { Height = 96, BackColor = White, Cursor = Cursors.Hand };
            card.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RRect(new Rectangle(0, 0, card.Width, card.Height), 14))
                {
                    ev.Graphics.FillPath(new SolidBrush(White), path);
                    ev.Graphics.DrawPath(new Pen(Border, 1), path);
                }
                Color barColor = p.EsAprobado ? Indigo : p.EsPagado ? GreenOk : Amber;
                ev.Graphics.FillRectangle(new SolidBrush(barColor), new Rectangle(0, 14, 5, card.Height - 28));

                // Barra de progreso
                if (p.EsAprobado && p.MontoAprobado > 0)
                {
                    float pct = (float)p.PorcentajePagado / 100f;
                    int barY = card.Height - 8, barH = 4;
                    int barX = 20, barW = card.Width - 40;
                    using (var pathBg = RRect(new Rectangle(barX, barY, barW, barH), 2))
                        ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(229, 231, 235)), pathBg);
                    if (pct > 0)
                        using (var pathFg = RRect(new Rectangle(barX, barY, (int)(barW * pct), barH), 2))
                            ev.Graphics.FillPath(new SolidBrush(GreenOk), pathFg);
                }
            };

            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Color.Transparent, Padding = new Padding(18, 0, 16, 10) };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Emoji tipo
            var pnlEmoji = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            pnlEmoji.Controls.Add(new Label { Text = p.EmojiTipo, Font = new Font("Segoe UI", 22), AutoSize = true, BackColor = Color.Transparent, Location = new Point(0, 18) });

            // Info central
            var pnlInfo = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            pnlInfo.Controls.Add(new Label { Text = $"Préstamo {p.TipoPrestamo}", ForeColor = TextDark, Font = new Font("Segoe UI", 11, FontStyle.Bold), AutoSize = false, Size = new Size(300, 22), BackColor = Color.Transparent, Location = new Point(0, 12) });
            pnlInfo.Controls.Add(new Label { Text = $"{p.MontoAprobado ?? p.MontoSolicitado:N0} €  ·  {p.PlazoMeses} meses  ·  {p.TasaInteres}% TIN", ForeColor = TextGray, Font = new Font("Segoe UI", 8.5f), AutoSize = true, BackColor = Color.Transparent, Location = new Point(0, 38) });
            pnlInfo.Controls.Add(new Label { Text = $"{p.PorcentajePagado:F0}% amortizado", ForeColor = GreenOk, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(0, 60) });

            // Panel derecho
            var pnlRight = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            Color badgeColor = p.EsAprobado ? Indigo : p.EsPagado ? GreenOk : Amber;
            string badgeText = p.EsAprobado ? "Aprobado" : p.EsPagado ? "Pagado" : "Pendiente";
            pnlRight.Controls.Add(new Label { Text = badgeText, ForeColor = White, BackColor = badgeColor, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, Padding = new Padding(8, 3, 8, 3), Location = new Point(0, 10) });
            pnlRight.Controls.Add(new Label { Text = $"Cuota: {p.CuotaMensual:N2} €/mes", ForeColor = TextDark, Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(0, 38) });
            if (p.ProximoPago.HasValue)
                pnlRight.Controls.Add(new Label { Text = $"Próximo: {p.ProximoPago.Value:dd/MM/yyyy}", ForeColor = TextGray, Font = new Font("Segoe UI", 8), AutoSize = true, BackColor = Color.Transparent, Location = new Point(0, 60) });

            tlp.Controls.Add(pnlEmoji, 0, 0);
            tlp.Controls.Add(pnlInfo,  1, 0);
            tlp.Controls.Add(pnlRight, 2, 0);
            card.Controls.Add(tlp);

            var prestamoCapturado = p;
            card.Click += (s, e) => AbrirDetalle(prestamoCapturado);
            foreach (Control c in tlp.Controls) { c.Cursor = Cursors.Hand; c.Click += (s, e) => AbrirDetalle(prestamoCapturado); }

            card.MouseEnter += (s, e) => { card.BackColor = Color.FromArgb(248, 249, 255); card.Invalidate(); };
            card.MouseLeave += (s, e) => { card.BackColor = White;                         card.Invalidate(); };
            return card;
        }

        private Panel CrearEstadoVacio()
        {
            var pnl = new Panel { Height = 200, BackColor = White };
            pnl.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RRect(new Rectangle(0, 0, pnl.Width, pnl.Height), 16))
                { ev.Graphics.FillPath(new SolidBrush(White), path); ev.Graphics.DrawPath(new Pen(Border, 1), path); }
            };
            pnl.Controls.Add(new Label { Text = "💳", Font = new Font("Segoe UI", 32), AutoSize = true, BackColor = Color.Transparent, Location = new Point(0, 30), Anchor = AnchorStyles.None });
            pnl.Controls.Add(new Label { Text = "No tienes préstamos activos", ForeColor = TextDark, Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(0, 95) });
            pnl.Controls.Add(new Label { Text = "Solicita tu primer préstamo o usa el simulador para calcular tu cuota.", ForeColor = TextGray, Font = new Font("Segoe UI", 9), AutoSize = false, Size = new Size(500, 18), BackColor = Color.Transparent, Location = new Point(0, 130), TextAlign = ContentAlignment.MiddleCenter });

            pnl.Resize += (s, ev) =>
            {
                foreach (Control c in pnl.Controls)
                    c.Left = (pnl.ClientSize.Width - c.Width) / 2;
            };
            return pnl;
        }

        private void AbrirSolicitud()
        {
            using (var frm = new FrmSolicitarPrestamo())
            {
                if (frm.ShowDialog(this.FindForm()) == DialogResult.OK)
                    CargarDatos();
            }
        }

        private void AbrirDetalle(Prestamo p)
        {
            using (var frm = new FrmDetallePrestamo(p))
                frm.ShowDialog(this.FindForm());
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
