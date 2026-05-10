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
    public class VistaBecas : UserControl
    {
        // ── Paleta ────────────────────────────────────────────────
        private static readonly Color BgPage      = Color.FromArgb(244, 246, 252);
        private static readonly Color Indigo       = Color.FromArgb(99, 102, 241);
        private static readonly Color IndigoDark   = Color.FromArgb(49, 46, 129);
        private static readonly Color IndigoLight  = Color.FromArgb(165, 180, 252);
        private static readonly Color White        = Color.White;
        private static readonly Color TextDark     = Color.FromArgb(17, 24, 39);
        private static readonly Color TextGray     = Color.FromArgb(107, 114, 128);
        private static readonly Color TextLight    = Color.FromArgb(156, 163, 175);
        private static readonly Color BorderGray   = Color.FromArgb(229, 231, 235);
        private static readonly Color GreenOk      = Color.FromArgb(16, 185, 129);
        private static readonly Color RedWarn      = Color.FromArgb(239, 68, 68);
        private static readonly Color Amber        = Color.FromArgb(245, 158, 11);

        private static readonly Color ColUniv     = Color.FromArgb(59, 130, 246);
        private static readonly Color ColPosgrado = Color.FromArgb(139, 92, 246);
        private static readonly Color ColFP       = Color.FromArgb(16, 185, 129);
        private static readonly Color ColDigital  = Color.FromArgb(245, 158, 11);
        private static readonly Color ColDep      = Color.FromArgb(239, 68, 68);
        private static readonly Color ColArte     = Color.FromArgb(236, 72, 153);

        // ── Controles dinámicos ───────────────────────────────────
        private readonly BecaService _service = new BecaService();
        private CategoriaBeca?   _filtroActual = null;
        private Panel            _pnlDestacada;    // tarjeta destacada ancho completo
        private Label            _lblContador;     // "X becas disponibles"
        private FlowLayoutPanel  _flpCards;        // grid pequeño (primeras 3)
        private Label            _lblSeccion;      // "Más becas para ti"
        private Panel            _pnlBottomCards;  // 3 últimas — horizontales ancho completo
        private Panel            _pnlScroll;       // panel raíz con AutoScroll
        private List<Panel>      _filtroButtons = new List<Panel>();

        public event EventHandler VolverAlInicio;
        public event EventHandler MisSolicitudesClicked;

        public VistaBecas()
        {
            BackColor = BgPage;
            Dock      = DockStyle.Fill;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ConstruirUI();
            AplicarFiltro();
        }

        // ═══════════════════════════════════════════════════════════
        //  ESTRUCTURA PRINCIPAL
        // ═══════════════════════════════════════════════════════════
        private void ConstruirUI()
        {
            Controls.Clear();

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3, ColumnCount = 1,
                BackColor = Color.Transparent
            };
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 162)); // header
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));  // filtros
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // cuerpo

            tlp.Controls.Add(ConstruirHeader(),   0, 0);
            tlp.Controls.Add(ConstruirFiltros(),  0, 1);
            tlp.Controls.Add(ConstruirCuerpo(),   0, 2);

            Controls.Add(tlp);
        }

        // ── Header ────────────────────────────────────────────────
        private Panel ConstruirHeader()
        {
            var pnl = new Panel { Dock = DockStyle.Fill };
            pnl.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new LinearGradientBrush(
                    new Point(0, 0), new Point(pnl.Width, pnl.Height),
                    IndigoDark, Indigo))
                    g.FillRectangle(br, pnl.ClientRectangle);
                using (var br2 = new SolidBrush(Color.FromArgb(22, 255, 255, 255)))
                {
                    g.FillEllipse(br2, pnl.Width - 170, -70, 260, 260);
                    g.FillEllipse(br2, pnl.Width - 55,  40,  130, 130);
                }
            };

            // Botón ← para volver al inicio (dentro del header índigo)
            bool volverHovered = false;
            var btnVolver = new Panel
            {
                Location  = new Point(16, 12),
                Size      = new Size(80, 28),
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand
            };
            btnVolver.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = btnVolver.ClientRectangle;
                if (volverHovered)
                    using (var path = RRect(r, 8))
                        ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(40, 255, 255, 255)), path);
                Color tc = volverHovered ? Color.White : Color.FromArgb(200, 255, 255, 255);
                int cy = r.Height / 2;
                using (var pen = new Pen(tc, 1.8f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    ev.Graphics.DrawLine(pen, 18, cy, 10, cy);
                    ev.Graphics.DrawLines(pen, new[] { new PointF(14f, cy - 4f), new PointF(10f, cy), new PointF(14f, cy + 4f) });
                }
                TextRenderer.DrawText(ev.Graphics, "Inicio",
                    new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    new Rectangle(24, 0, r.Width - 26, r.Height), tc,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            };
            btnVolver.MouseEnter += (s, ev) => { volverHovered = true;  btnVolver.Invalidate(); };
            btnVolver.MouseLeave += (s, ev) => { volverHovered = false; btnVolver.Invalidate(); };
            btnVolver.Click      += (s, ev) => VolverAlInicio?.Invoke(this, EventArgs.Empty);
            pnl.Controls.Add(btnVolver);

            var lblTit = new Label
            {
                Text = "🎓  Becas Nexum",
                ForeColor = White,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                AutoSize = true, BackColor = Color.Transparent,
                Location = new Point(36, 48)     // desplazado para no solapar con el botón
            };
            var lblSub = new Label
            {
                Text = "Invierte en tu futuro con el apoyo del banco",
                ForeColor = IndigoLight,
                Font = new Font("Segoe UI", 11),
                AutoSize = true, BackColor = Color.Transparent,
                Location = new Point(38, 88)
            };

            int abiertas = _service.ContarAbiertas();
            decimal total = _service.ImporteTotalDisponible();

            var pnlStats = new FlowLayoutPanel
            {
                BackColor = Color.Transparent, AutoSize = true,
                Location = new Point(36, 118),
                FlowDirection = FlowDirection.LeftToRight, WrapContents = false
            };
            pnlStats.Controls.Add(ChipStat($"{abiertas} becas abiertas", GreenOk));
            pnlStats.Controls.Add(ChipStat($"Hasta {total:N0} € disponibles", Amber));

            // Botón "Mis Solicitudes"
            bool misSolHov = false;
            var btnMisSol = new Panel
            {
                Size = new Size(148, 30), BackColor = Color.Transparent, Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnMisSol.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = btnMisSol.ClientRectangle;
                Color bg = misSolHov ? Color.FromArgb(60, 255, 255, 255) : Color.FromArgb(35, 255, 255, 255);
                using (var path = RRect(new Rectangle(1, 1, r.Width - 2, r.Height - 2), 14))
                {
                    ev.Graphics.FillPath(new SolidBrush(bg), path);
                    ev.Graphics.DrawPath(new Pen(Color.FromArgb(100, 255, 255, 255), 1), path);
                }
                TextRenderer.DrawText(ev.Graphics, "📋  Mis Solicitudes",
                    new Font("Segoe UI", 9, FontStyle.Bold),
                    new Rectangle(0, 0, r.Width, r.Height), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btnMisSol.MouseEnter += (s, ev) => { misSolHov = true;  btnMisSol.Invalidate(); };
            btnMisSol.MouseLeave += (s, ev) => { misSolHov = false; btnMisSol.Invalidate(); };
            btnMisSol.Click      += (s, ev) => MisSolicitudesClicked?.Invoke(this, EventArgs.Empty);

            Action posMisSol = () =>
                btnMisSol.Location = new Point(pnl.Width - btnMisSol.Width - 16, 12);
            pnl.Resize        += (s, ev) => posMisSol();
            pnl.HandleCreated += (s, ev) => posMisSol();

            pnl.Controls.Add(lblTit);
            pnl.Controls.Add(lblSub);
            pnl.Controls.Add(pnlStats);
            pnl.Controls.Add(btnMisSol);
            return pnl;
        }

        private Label ChipStat(string texto, Color color)
        {
            return new Label
            {
                Text = "  " + texto + "  ",
                ForeColor = White,
                BackColor = Color.FromArgb(50, 255, 255, 255),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(6, 3, 6, 3),
                Margin = new Padding(0, 0, 8, 0)
            };
        }

        // ── Barra de filtros ──────────────────────────────────────
        private Panel ConstruirFiltros()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = White };
            pnl.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BorderGray, 1))
                    ev.Graphics.DrawLine(pen, 0, pnl.Height - 1, pnl.Width, pnl.Height - 1);
            };

            var flp = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 0)
            };

            var cats = new (string, CategoriaBeca?)[]
            {
                ("Todas",          null),
                ("Universitarias", CategoriaBeca.Universitaria),
                ("Posgrado",       CategoriaBeca.Posgrado),
                ("FP / CFGS",      CategoriaBeca.FP),
                ("Digital",        CategoriaBeca.Digital),
                ("Deportiva",      CategoriaBeca.Deportiva),
                ("Arte",           CategoriaBeca.Arte)
            };

            _filtroButtons.Clear();
            foreach (var (texto, cat) in cats)
            {
                var btn = CrearBtnFiltro(texto, cat);
                _filtroButtons.Add(btn);
                flp.Controls.Add(btn);
            }

            // Centrar horizontalmente
            Action centrar = () =>
            {
                flp.Left = Math.Max(0, (pnl.ClientSize.Width  - flp.Width)  / 2);
                flp.Top  = Math.Max(0, (pnl.ClientSize.Height - flp.Height) / 2);
            };
            pnl.Resize      += (s, e) => centrar();
            flp.SizeChanged += (s, e) => centrar();

            pnl.Controls.Add(flp);
            return pnl;
        }

        private Panel CrearBtnFiltro(string texto, CategoriaBeca? categoria)
        {
            bool esActivo() => _filtroActual == categoria;

            var btn = new Panel
            {
                AutoSize = false, Height = 36,
                Margin = new Padding(0, 0, 4, 0),
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent,
                Tag = categoria
            };

            var lbl = new Label
            {
                Text = texto, AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(14, 6, 14, 6),
                Cursor = Cursors.Hand
            };
            lbl.ForeColor = esActivo() ? Indigo : TextGray;

            btn.Controls.Add(lbl);
            btn.Width = lbl.PreferredWidth + 28;

            btn.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                if (esActivo())
                {
                    using (var path = RRect(new Rectangle(0, 0, btn.Width, btn.Height - 2), 18))
                    using (var br = new SolidBrush(Color.FromArgb(238, 242, 255)))
                        ev.Graphics.FillPath(br, path);
                    using (var pen = new Pen(Indigo, 2))
                    using (var path = RRect(new Rectangle(1, 1, btn.Width - 2, btn.Height - 3), 17))
                        ev.Graphics.DrawPath(pen, path);
                }
                else if (btn.ClientRectangle.Contains(btn.PointToClient(Cursor.Position)))
                {
                    using (var pen = new Pen(IndigoLight, 2))
                        ev.Graphics.DrawLine(pen, 4, btn.Height - 2, btn.Width - 4, btn.Height - 2);
                }
            };

            EventHandler onClick = (s, e) =>
            {
                _filtroActual = categoria;
                foreach (var b in _filtroButtons)
                {
                    var cl = b.Controls.Count > 0 ? b.Controls[0] as Label : null;
                    if (cl != null) cl.ForeColor = (CategoriaBeca?)b.Tag == _filtroActual ? Indigo : TextGray;
                    b.Invalidate();
                }
                AplicarFiltro();
            };
            btn.Click   += onClick;
            lbl.Click   += onClick;
            btn.MouseEnter += (s, e) => btn.Invalidate();
            btn.MouseLeave += (s, e) => btn.Invalidate();
            lbl.MouseEnter += (s, e) => btn.Invalidate();
            lbl.MouseLeave += (s, e) => btn.Invalidate();
            return btn;
        }

        // ── Cuerpo scrollable ─────────────────────────────────────
        private Panel ConstruirCuerpo()
        {
            _pnlScroll = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgPage,
                AutoScroll = true
            };

            _pnlDestacada = new Panel
            {
                Location  = new Point(28, 24),
                Height    = 160,
                BackColor = Color.Transparent
            };

            _lblContador = new Label
            {
                Text      = "",
                ForeColor = TextGray,
                Font      = new Font("Segoe UI", 10),
                AutoSize  = true,
                Location  = new Point(28, 200)
            };

            _flpCards = new FlowLayoutPanel
            {
                AutoSize      = true,
                AutoSizeMode  = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = true,
                BackColor     = Color.Transparent,
                Location      = new Point(28, 228),
                Padding       = new Padding(0)
            };

            // Título sección inferior
            _lblSeccion = new Label
            {
                Text      = "Más becas para ti",
                ForeColor = TextDark,
                Font      = new Font("Segoe UI", 13, FontStyle.Bold),
                AutoSize  = true,
                Location  = new Point(28, 560)   // se recalcula en AplicarFiltro
            };

            // Panel de tarjetas horizontales (últimas 3, ancho completo)
            _pnlBottomCards = new Panel
            {
                AutoSize     = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor    = Color.Transparent,
                Location     = new Point(28, 592)  // se recalcula en AplicarFiltro
            };

            _pnlScroll.Controls.Add(_pnlDestacada);
            _pnlScroll.Controls.Add(_lblContador);
            _pnlScroll.Controls.Add(_flpCards);
            _pnlScroll.Controls.Add(_lblSeccion);
            _pnlScroll.Controls.Add(_pnlBottomCards);

            _pnlScroll.Resize += (s, e) => SincronizarAnchos();

            return _pnlScroll;
        }

        private void SincronizarAnchos()
        {
            int w = Math.Max(200, _pnlScroll.ClientSize.Width - 56);
            _pnlDestacada.Width  = w;
            _flpCards.Width      = w;
            _pnlBottomCards.Width = w;

            // Reposicionar tarjetas horizontales al cambiar ancho
            int by = 0;
            foreach (Control c in _pnlBottomCards.Controls)
            {
                c.Width    = w;
                c.Location = new Point(0, by);
                by += c.Height + 14;
            }
            _pnlBottomCards.Height = by;
        }

        // ═══════════════════════════════════════════════════════════
        //  FILTRADO Y RENDERIZADO
        // ═══════════════════════════════════════════════════════════
        private void AplicarFiltro()
        {
            List<Beca> becas = _filtroActual.HasValue
                ? _service.ObtenerPorCategoria(_filtroActual.Value)
                : _service.ObtenerTodas();

            // ── Tarjeta destacada ─────────────────────────────────
            _pnlDestacada.Controls.Clear();
            bool mostrarDestacada = !_filtroActual.HasValue;
            Beca dest = mostrarDestacada ? _service.ObtenerDestacada() : null;

            if (dest != null)
            {
                _pnlDestacada.Height  = 160;
                _lblContador.Location = new Point(28, 200);
                _flpCards.Location    = new Point(28, 228);
                _pnlDestacada.Controls.Add(CrearTarjetaDestacada(dest));
            }
            else
            {
                _pnlDestacada.Height  = 0;
                _lblContador.Location = new Point(28, 16);
                _flpCards.Location    = new Point(28, 44);
            }

            // ── Dividir becas: grid vs. horizontales ──────────────
            var lista = becas.Where(b => !(mostrarDestacada && b.Destacada)).ToList();

            // En vista "Todas" las primeras 3 van al grid y las últimas 3 horizontales.
            // En vistas filtradas todas van al grid.
            List<Beca> gridBecas   = lista;
            List<Beca> bottomBecas = new List<Beca>();

            if (mostrarDestacada && lista.Count > 3)
            {
                gridBecas   = lista.Take(3).ToList();
                bottomBecas = lista.Skip(3).ToList();
            }

            // ── Grid ──────────────────────────────────────────────
            _flpCards.Controls.Clear();
            _flpCards.SuspendLayout();

            int abiertas = becas.Count(b => b.Estado == EstadoSolicitud.Abierta);
            _lblContador.Text = $"{abiertas} beca{(abiertas != 1 ? "s" : "")} " +
                                $"disponible{(abiertas != 1 ? "s" : "")}";

            foreach (var beca in gridBecas)
                _flpCards.Controls.Add(CrearTarjeta(beca));

            _flpCards.ResumeLayout(true);

            // ── Sección inferior ──────────────────────────────────
            _pnlBottomCards.Controls.Clear();
            _lblSeccion.Visible      = bottomBecas.Count > 0;
            _pnlBottomCards.Visible  = bottomBecas.Count > 0;

            if (bottomBecas.Count > 0)
            {
                // Posición dinámica: justo debajo del grid
                // Se actualiza en SincronizarAnchos tras el layout
                _flpCards.SizeChanged += RecalcBottomPosition;
                RecalcBottomPosition(null, null);

                int by = 0;
                foreach (var beca in bottomBecas)
                {
                    var card = CrearTarjetaHorizontal(beca);
                    card.Location = new Point(0, by);
                    by += card.Height + 14;
                    _pnlBottomCards.Controls.Add(card);
                }
                _pnlBottomCards.Height = by + 40;
            }

            SincronizarAnchos();
        }

        private void RecalcBottomPosition(object sender, EventArgs e)
        {
            int gridBottom = _flpCards.Bottom + 24;
            _lblSeccion.Location     = new Point(28, gridBottom);
            _pnlBottomCards.Location = new Point(28, gridBottom + 32);
        }

        // ── Tarjeta destacada (ancho completo) ────────────────────
        private Panel CrearTarjetaDestacada(Beca beca)
        {
            var card = new Panel
            {
                Dock      = DockStyle.Fill,
                Height    = 160,
                Cursor    = Cursors.Hand
            };

            card.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RRect(card.ClientRectangle, 18))
                using (var br = new LinearGradientBrush(
                    new Point(0, 0), new Point(card.Width, card.Height),
                    IndigoDark, Color.FromArgb(110, 115, 250)))
                    g.FillPath(br, path);

                using (var br2 = new SolidBrush(Color.FromArgb(18, 255, 255, 255)))
                {
                    g.FillEllipse(br2, card.Width - 200, -80, 300, 300);
                    g.FillEllipse(br2, card.Width - 60,  50,  140, 140);
                }
            };

            // Columna izquierda
            var badge = new Label
            {
                Text = "⭐  DESTACADA",
                BackColor = Amber, ForeColor = White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                AutoSize = true, Padding = new Padding(8, 3, 8, 3),
                Location = new Point(24, 18)
            };
            var lblTit = new Label
            {
                Text = beca.Titulo,
                ForeColor = White,
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                AutoSize = true, BackColor = Color.Transparent,
                Location = new Point(24, 46)
            };
            var lblDesc = new Label
            {
                Text = beca.Descripcion,
                ForeColor = IndigoLight,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.Transparent,
                Location = new Point(24, 90),
                Size = new Size(500, 38)
            };

            // Columna derecha — importe + botón
            var pnlRight = new Panel
            {
                BackColor = Color.Transparent,
                Size      = new Size(230, 130),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };

            var lblEur = new Label
            {
                Text = $"{beca.Importe:N0} €/año",
                ForeColor = Color.FromArgb(250, 204, 21),
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                AutoSize = true, BackColor = Color.Transparent,
                Location = new Point(0, 18)
            };
            var lblDur = new Label
            {
                Text = beca.DuracionTexto,
                ForeColor = IndigoLight,
                Font = new Font("Segoe UI", 9),
                AutoSize = true, BackColor = Color.Transparent,
                Location = new Point(2, 58)
            };
            var btn = new Button
            {
                Text = "Solicitar ahora  →",
                Size = new Size(200, 38),
                Location = new Point(0, 82),
                BackColor = White, ForeColor = Indigo,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => MostrarDetalle(beca);

            pnlRight.Controls.Add(lblEur);
            pnlRight.Controls.Add(lblDur);
            pnlRight.Controls.Add(btn);

            card.Controls.Add(badge);
            card.Controls.Add(lblTit);
            card.Controls.Add(lblDesc);
            card.Controls.Add(pnlRight);

            // Posicionar pnlRight a la derecha al resize
            Action posRight = () =>
            {
                pnlRight.Location = new Point(card.Width - pnlRight.Width - 28, 14);
            };
            card.Resize  += (s, e) => posRight();
            card.HandleCreated += (s, e) => posRight();

            return card;
        }

        // ── Tarjeta normal ────────────────────────────────────────
        private Panel CrearTarjeta(Beca beca)
        {
            bool   cerrada  = beca.Estado == EstadoSolicitud.Cerrada;
            Color  catColor = GetCatColor(beca.Categoria);

            bool proxima = !cerrada && beca.FechaCierre.HasValue &&
                           (beca.FechaCierre.Value - DateTime.Today).TotalDays <= 15;
            int cardHeight = proxima ? 308 : 286;

            var card = new Panel
            {
                Size      = new Size(290, cardHeight),
                Margin    = new Padding(0, 0, 16, 16),
                BackColor = cerrada ? Color.FromArgb(250, 250, 252) : White,
                Cursor    = cerrada ? Cursors.Default : Cursors.Hand
            };

            card.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Fondo redondeado
                using (var path = RRect(new Rectangle(0, 0, card.Width, card.Height), 14))
                {
                    using (var br = new SolidBrush(card.BackColor))
                        g.FillPath(br, path);
                    using (var pen = new Pen(BorderGray, 1))
                        g.DrawPath(pen, path);
                }

                // Franja superior de color
                Color stripe = cerrada ? TextLight : catColor;
                using (var br = new SolidBrush(stripe))
                using (var top = RRect(new Rectangle(0, 0, card.Width, 5), 14))
                {
                    g.FillPath(br, top);
                    g.FillRectangle(br, new Rectangle(0, 3, card.Width, 4));
                }
            };

            int y = 20;

            // Fila: badge categoría + estado
            var pnlTop = new Panel
            {
                Location  = new Point(16, y),
                Size      = new Size(card.Width - 32, 26),
                BackColor = Color.Transparent
            };
            var lblCat = new Label
            {
                Text      = TextoCat(beca.Categoria),
                BackColor = cerrada ? Color.FromArgb(229, 231, 235) : Color.FromArgb(238, 242, 255),
                ForeColor = cerrada ? TextLight : catColor,
                Font      = new Font("Segoe UI", 8, FontStyle.Bold),
                AutoSize  = true,
                Padding   = new Padding(7, 3, 7, 3),
                Location  = new Point(0, 0)
            };
            var lblEst = new Label
            {
                Text      = cerrada ? "● Cerrada" : "● Abierta",
                ForeColor = cerrada ? TextLight : GreenOk,
                Font      = new Font("Segoe UI", 8, FontStyle.Bold),
                AutoSize  = true, BackColor = Color.Transparent,
                Location  = new Point(198, 4)
            };
            pnlTop.Controls.Add(lblCat);
            pnlTop.Controls.Add(lblEst);
            card.Controls.Add(pnlTop);
            y += 36;

            // Título
            var lblTit = new Label
            {
                Text      = beca.Titulo,
                ForeColor = cerrada ? TextLight : TextDark,
                Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                Location  = new Point(16, y),
                Size      = new Size(card.Width - 32, 44),
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblTit);
            y += 50;

            // Importe
            var lblImporte = new Label
            {
                Text      = $"{beca.Importe:N0} €",
                ForeColor = cerrada ? TextLight : Indigo,
                Font      = new Font("Segoe UI", 20, FontStyle.Bold),
                AutoSize  = true, BackColor = Color.Transparent,
                Location  = new Point(16, y)
            };
            var lblDur = new Label
            {
                Text      = beca.DuracionTexto,
                ForeColor = TextLight,
                Font      = new Font("Segoe UI", 9),
                AutoSize  = true, BackColor = Color.Transparent,
                Location  = new Point(16, y + 30)
            };
            card.Controls.Add(lblImporte);
            card.Controls.Add(lblDur);
            y += 58;

            // Separador
            card.Controls.Add(new Panel
            {
                Location  = new Point(16, y),
                Size      = new Size(card.Width - 32, 1),
                BackColor = BorderGray
            });
            y += 12;

            // Badge alerta si cierra pronto
            if (proxima)
            {
                int dias = (int)(beca.FechaCierre.Value - DateTime.Today).TotalDays;
                card.Controls.Add(new Label
                {
                    Text      = $"⚠  Cierra en {dias} día{(dias != 1 ? "s" : "")}",
                    ForeColor = Color.White,
                    BackColor = RedWarn,
                    Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    AutoSize  = true,
                    Padding   = new Padding(6, 2, 6, 2),
                    Location  = new Point(16, y)
                });
                y += 22;
            }

            // Fecha
            card.Controls.Add(new Label
            {
                Text      = beca.FechaCierre.HasValue
                            ? $"📅  Cierra {beca.FechaCierre.Value:dd/MM/yyyy}"
                            : "📅  Sin fecha límite",
                ForeColor = cerrada ? TextLight : (proxima ? RedWarn : TextGray),
                Font      = new Font("Segoe UI", 8),
                AutoSize  = true, BackColor = Color.Transparent,
                Location  = new Point(16, y)
            });
            y += 22;

            // Plazas
            card.Controls.Add(new Label
            {
                Text      = cerrada ? "🔒  Convocatoria cerrada"
                                    : $"🎯  {beca.PlazasDisponibles} plazas disponibles",
                ForeColor = cerrada ? TextLight : TextGray,
                Font      = new Font("Segoe UI", 8),
                AutoSize  = true, BackColor = Color.Transparent,
                Location  = new Point(16, y)
            });
            y += 32;

            // Botón
            if (!cerrada)
            {
                var btn = new Button
                {
                    Text      = "Ver detalles  →",
                    Size      = new Size(card.Width - 32, 36),
                    Location  = new Point(16, y),
                    BackColor = Indigo, ForeColor = White,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                    Cursor    = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, e) => MostrarDetalle(beca);
                card.Controls.Add(btn);

                card.MouseEnter += (s, e) => { card.BackColor = Color.FromArgb(248, 249, 255); card.Invalidate(); };
                card.MouseLeave += (s, e) => { card.BackColor = White; card.Invalidate(); };
            }

            return card;
        }

        // ── Tarjeta horizontal ancho completo ─────────────────────
        private Panel CrearTarjetaHorizontal(Beca beca)
        {
            bool  cerrada  = beca.Estado == EstadoSolicitud.Cerrada;
            Color catColor = GetCatColor(beca.Categoria);

            var card = new Panel
            {
                Height    = 96,
                BackColor = cerrada ? Color.FromArgb(250, 250, 252) : White,
                Cursor    = cerrada ? Cursors.Default : Cursors.Hand
            };

            card.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RRect(new Rectangle(0, 0, card.Width, card.Height), 14))
                {
                    using (var br = new SolidBrush(card.BackColor))
                        g.FillPath(br, path);
                    using (var pen = new Pen(BorderGray, 1))
                        g.DrawPath(pen, path);
                }
                // Barra lateral izquierda de color
                using (var br = new SolidBrush(cerrada ? TextLight : catColor))
                using (var lPath = RRect(new Rectangle(0, 0, 6, card.Height), 14))
                {
                    g.FillPath(br, lPath);
                    g.FillRectangle(br, new Rectangle(4, 0, 4, card.Height));
                }
            };

            // Badge categoría
            var lblCat = new Label
            {
                Text      = TextoCat(beca.Categoria),
                BackColor = cerrada ? Color.FromArgb(229, 231, 235) : Color.FromArgb(238, 242, 255),
                ForeColor = cerrada ? TextLight : catColor,
                Font      = new Font("Segoe UI", 8, FontStyle.Bold),
                AutoSize  = true,
                Padding   = new Padding(7, 2, 7, 2),
                Location  = new Point(22, 14)
            };

            // Título
            var lblTit = new Label
            {
                Text      = beca.Titulo,
                ForeColor = cerrada ? TextLight : TextDark,
                Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize  = true,
                BackColor = Color.Transparent,
                Location  = new Point(22, 38)
            };

            // Descripción corta
            var lblDesc = new Label
            {
                Text      = beca.Requisitos,
                ForeColor = TextGray,
                Font      = new Font("Segoe UI", 8),
                AutoSize  = false,
                Size      = new Size(400, 18),
                BackColor = Color.Transparent,
                Location  = new Point(22, 66)
            };

            // Columna derecha — importe + fecha + botón
            var pnlR = new Panel
            {
                BackColor = Color.Transparent,
                Size      = new Size(360, 80),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };

            var lblImporte = new Label
            {
                Text      = $"{beca.Importe:N0} €",
                ForeColor = cerrada ? TextLight : Indigo,
                Font      = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize  = true,
                BackColor = Color.Transparent,
                Location  = new Point(0, 10)
            };

            bool proxima = !cerrada && beca.FechaCierre.HasValue &&
                           (beca.FechaCierre.Value - DateTime.Today).TotalDays <= 15;
            var lblFecha = new Label
            {
                Text      = beca.FechaCierre.HasValue
                            ? $"Cierra {beca.FechaCierre.Value:dd/MM/yyyy}"
                            : "Sin fecha límite",
                ForeColor = cerrada ? TextLight : (proxima ? RedWarn : TextGray),
                Font      = new Font("Segoe UI", 8),
                AutoSize  = true,
                BackColor = Color.Transparent,
                Location  = new Point(0, 42)
            };

            pnlR.Controls.Add(lblImporte);
            pnlR.Controls.Add(lblFecha);

            if (proxima)
            {
                int dias = (int)(beca.FechaCierre.Value - DateTime.Today).TotalDays;
                pnlR.Controls.Add(new Label
                {
                    Text      = $"⚠  Cierra en {dias} día{(dias != 1 ? "s" : "")}",
                    ForeColor = Color.White,
                    BackColor = RedWarn,
                    Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    AutoSize  = true,
                    Padding   = new Padding(6, 2, 6, 2),
                    Location  = new Point(0, 62)
                });
            }

            if (!cerrada)
            {
                var btn = new Button
                {
                    Text      = "Ver detalles  →",
                    Size      = new Size(150, 34),
                    Location  = new Point(196, 30),
                    BackColor = Indigo, ForeColor = White,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                    Cursor    = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, e) => MostrarDetalle(beca);
                pnlR.Controls.Add(btn);
            }
            else
            {
                var lblClosed = new Label
                {
                    Text      = "🔒  Cerrada",
                    ForeColor = TextLight,
                    Font      = new Font("Segoe UI", 9),
                    AutoSize  = true,
                    BackColor = Color.Transparent,
                    Location  = new Point(210, 38)
                };
                pnlR.Controls.Add(lblClosed);
            }

            card.Controls.Add(lblCat);
            card.Controls.Add(lblTit);
            card.Controls.Add(lblDesc);
            card.Controls.Add(pnlR);

            // Posicionar columna derecha al resize
            Action posR = () =>
            {
                pnlR.Location = new Point(card.Width - pnlR.Width - 20, 8);
            };
            card.Resize        += (s, e) => posR();
            card.HandleCreated += (s, e) => posR();

            if (!cerrada)
            {
                card.MouseEnter += (s, e) => { card.BackColor = Color.FromArgb(248, 249, 255); card.Invalidate(); };
                card.MouseLeave += (s, e) => { card.BackColor = White; card.Invalidate(); };
            }

            return card;
        }

        // ═══════════════════════════════════════════════════════════
        //  MODAL DETALLE
        // ═══════════════════════════════════════════════════════════
        private void MostrarDetalle(Beca beca)
        {
            using (var frm = new FrmDetalleBeca(beca))
                frm.ShowDialog(this.FindForm());
        }

        // ═══════════════════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════════════════
        private static Color GetCatColor(CategoriaBeca cat)
        {
            switch (cat)
            {
                case CategoriaBeca.Universitaria: return ColUniv;
                case CategoriaBeca.Posgrado:      return ColPosgrado;
                case CategoriaBeca.FP:            return ColFP;
                case CategoriaBeca.Digital:       return ColDigital;
                case CategoriaBeca.Deportiva:     return ColDep;
                case CategoriaBeca.Arte:          return ColArte;
                default:                          return Indigo;
            }
        }

        private static string TextoCat(CategoriaBeca cat)
        {
            switch (cat)
            {
                case CategoriaBeca.Universitaria: return "Universitaria";
                case CategoriaBeca.Posgrado:      return "Posgrado";
                case CategoriaBeca.FP:            return "FP / CFGS";
                case CategoriaBeca.Digital:       return "Digital";
                case CategoriaBeca.Deportiva:     return "Deportiva";
                case CategoriaBeca.Arte:          return "Arte";
                default:                          return "General";
            }
        }

        private static GraphicsPath RRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X,          r.Y,           d, d, 180, 90);
            path.AddArc(r.Right - d,  r.Y,           d, d, 270, 90);
            path.AddArc(r.Right - d,  r.Bottom - d,  d, d,   0, 90);
            path.AddArc(r.X,          r.Bottom - d,  d, d,  90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
