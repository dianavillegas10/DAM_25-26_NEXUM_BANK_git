using NexumApp.Forms.Principal;
using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace NexumApp.Views
{
    public class VistaHuchas : UserControl
    {
        private static readonly CultureInfo ES      = CultureInfo.CreateSpecificCulture("es-ES");
        private static readonly Color       BgPage  = Color.FromArgb(246, 248, 252);
        private static readonly Color       Oscuro  = Color.FromArgb(15,  23,  42);
        private static readonly Color       Gris    = Color.FromArgb(100, 116, 139);
        private static readonly Color       Borde   = Color.FromArgb(226, 232, 240);
        private static readonly Color       Indigo  = Color.FromArgb(99,  102, 241);

        private Panel _pnlLista;   // contenedor de cards (Dock.Top, altura explícita)
        private Label          _lblResumen;
        private List<Hucha>    _huchas  = new List<Hucha>();
        private int            _cuentaId;
        private decimal        _saldoCuenta;

        private readonly HuchaService      _huchaSvc = new HuchaService();
        private readonly CuentaService     _cuentaSvc = new CuentaService();
        private readonly MovimientoService _movSvc    = new MovimientoService();

        public event EventHandler VolverAlInicio;

        public VistaHuchas()
        {
            BackColor  = BgPage;
            Dock       = DockStyle.Fill;
            AutoScroll = true;   // El UserControl gestiona el scroll directamente
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            BuildUI();
            // Dos diferidos: tras el primer layout el panel ya tiene ClientSize real (anchura/altura para scroll).
            BeginInvoke((Action)(() => BeginInvoke((Action)CargarDatos)));
        }

        // ── UI estática (se construye una sola vez) ───────────────
        private void BuildUI()
        {
            Controls.Clear();

            // ── Header gradiente ──────────────────────────────────
            var header = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = Indigo };
            header.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = header.ClientRectangle;
                using (var b = new LinearGradientBrush(r, Indigo, Color.FromArgb(139, 92, 246), 135f))
                    g.FillRectangle(b, r);
                using (var br = new SolidBrush(Color.FromArgb(15, 255, 255, 255)))
                {
                    g.FillEllipse(br, r.Width - 200, -60, 280, 280);
                    g.FillEllipse(br, -60, -40, 180, 180);
                }
            };

            // Título
            var lTit = new Label
            {
                Text      = "🐷  Mis Huchas",
                Font      = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize  = true,
                Location  = new Point(32, 22)
            };

            // Subtítulo / resumen (se actualiza con los datos)
            _lblResumen = new Label
            {
                Text      = "Cargando...",
                Font      = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(210, 255, 255, 255),
                BackColor = Color.Transparent,
                AutoSize  = true,
                Location  = new Point(34, 58)
            };

            // ── Botón ← Inicio ────────────────────────────────────
            var btnVolver = new Button
            {
                Text      = "← Inicio",
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 255, 255, 255),
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(110, 38),
                Cursor    = Cursors.Hand,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };
            btnVolver.FlatAppearance.BorderSize       = 0;
            btnVolver.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 255, 255, 255);
            EventHandler applyVolver = (s, ev) =>
            {
                try { btnVolver.Region = new Region(RR(btnVolver.ClientRectangle, 10)); } catch { }
            };
            btnVolver.HandleCreated += applyVolver;
            btnVolver.Resize        += applyVolver;
            btnVolver.Click         += (s, ev) => VolverAlInicio?.Invoke(this, EventArgs.Empty);

            // ── Botón + Nueva Hucha ───────────────────────────────
            var btnNueva = new Button
            {
                Text      = "+  Nueva Hucha",
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Indigo,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(148, 38),
                Cursor    = Cursors.Hand,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };
            btnNueva.FlatAppearance.BorderSize = 0;
            btnNueva.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 255);
            EventHandler applyR = (s, ev) =>
            {
                try { btnNueva.Region = new Region(RR(btnNueva.ClientRectangle, 10)); } catch { }
            };
            btnNueva.HandleCreated += applyR;
            btnNueva.Resize        += applyR;
            btnNueva.Click         += (s, ev) =>
            {
                using (var frm = new FrmNuevaHucha())
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                        CargarDatos();
                }
            };

            header.Controls.Add(lTit);
            header.Controls.Add(_lblResumen);
            header.Controls.Add(btnVolver);
            header.Controls.Add(btnNueva);
            header.Resize += (s, ev) =>
            {
                btnNueva.Location  = new Point(header.Width - btnNueva.Width - 28, (header.Height - btnNueva.Height) / 2);
                btnVolver.Location = new Point(btnNueva.Left - btnVolver.Width - 10, (header.Height - btnVolver.Height) / 2);
            };

            // ── Lista de cards (Dock.Top, altura explícita) ──────────
            _pnlLista = new Panel { BackColor = BgPage, Dock = DockStyle.Top, Height = 0 };

            // Footer para padding inferior
            var footer = new Panel { BackColor = BgPage, Dock = DockStyle.Top, Height = 20 };

            // ORDEN CORRECTO en WinForms DockStyle.Top:
            // el último añadido tiene mayor z-order → se coloca arriba.
            Controls.Add(footer);    // primero → irá abajo (padding)
            Controls.Add(_pnlLista); // segundo → irá en el medio
            Controls.Add(header);    // último  → irá ARRIBA ✓

            Resize += (s, e) => AjustarAnchoCards();
        }

        // ── Carga datos desde BD y redibuja las cards ─────────────
        private void CargarDatos()
        {
            if (!SesionActual.Instancia?.EstaLogeado ?? true) return;

            int uid = SesionActual.Instancia.Usuario.Id;

            // Cuenta activa (primera disponible) para abonar
            var cuentas = _cuentaSvc.ObtenerCuentasPorUsuario(uid);
            var cuenta  = cuentas?.Find(c => c.Activa) ?? cuentas?[0];
            _cuentaId    = cuenta?.Id ?? 0;
            _saldoCuenta = cuenta?.Saldo ?? 0m;

            _huchas = _huchaSvc.ObtenerPorUsuario(uid);
            // Ordenar por progreso desc → las más avanzadas aparecen primero
            _huchas.Sort((a, b) => b.Progreso.CompareTo(a.Progreso));

            ActualizarResumen();
            RenderCards();
        }

        private void ActualizarResumen()
        {
            int     total     = _huchas.Count;
            decimal ahorrado  = 0m, meta = 0m;
            foreach (var h in _huchas) { ahorrado += h.SaldoActual; meta += h.MetaObjetivo; }

            if (total == 0)
                _lblResumen.Text = "Aún no tienes huchas. ¡Crea la primera!";
            else
                _lblResumen.Text =
                    $"{total} hucha{(total != 1 ? "s" : "")}  ·  " +
                    $"Ahorrado {ahorrado.ToString("C0", ES)}  ·  " +
                    $"Meta total {meta.ToString("C0", ES)}";
        }

        private const int PAD_X = 28;
        private const int PAD_Y = 16;
        private const int GAP   = 12;

        private void RenderCards()
        {
            _pnlLista.Controls.Clear();

            int pw    = Math.Max(300, ClientSize.Width);
            int cardW = Math.Max(300, pw - PAD_X * 2);

            if (_huchas.Count == 0)
            {
                _pnlLista.Controls.Add(new Label
                {
                    Text      = "No tienes huchas activas.\nPulsa \"+ Nueva Hucha\" para crear tu primer objetivo de ahorro.",
                    Font      = new Font("Segoe UI", 12),
                    ForeColor = Gris, AutoSize = false,
                    Size      = new Size(cardW, 80),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location  = new Point(PAD_X, PAD_Y),
                    BackColor = Color.Transparent
                });
                _pnlLista.Height = PAD_Y + 80 + PAD_Y;
                AutoScrollPosition = new Point(0, 0);
                return;
            }

            int y = PAD_Y;
            foreach (var h in _huchas)
            {
                Color clr  = ParseColor(h.ColorHex);
                var   card = CrearCard(h, clr);
                card.Width    = cardW;
                card.Location = new Point(PAD_X, y);
                _pnlLista.Controls.Add(card);
                y += card.Height + GAP;
            }

            _pnlLista.Height = y + PAD_Y;
            AutoScrollPosition = new Point(0, 0);
        }

        private void AjustarAnchoCards()
        {
            if (_pnlLista == null) return;
            int cardW = Math.Max(300, ClientSize.Width - PAD_X * 2);
            foreach (Control c in _pnlLista.Controls)
            {
                c.Width = cardW;
                c.Left  = PAD_X;
            }
        }

        // ── Card individual ───────────────────────────────────────
        private Panel CrearCard(Hucha h, Color clr)
        {
            const int H   = 100;
            const int BTN = 24;
            const int GAP = 6;

            int     pct     = h.Progreso;
            decimal falta   = Math.Max(0, h.MetaObjetivo - h.SaldoActual);
            Color   bgLight = Color.FromArgb(18, clr.R, clr.G, clr.B);
            Color   clrEdit = Color.FromArgb(99,  102, 241);
            Color   clrDel  = Color.FromArgb(220, 38,  38);
            bool    hov     = false;

            // BackColor = BgPage (fondo del panel padre) para que las esquinas
            // redondeadas dibujadas en Paint se vean correctas sin necesitar Region
            var card = new Panel { Height = H, BackColor = BgPage, Cursor = Cursors.Hand };

            // Zonas de los 3 botones (derecha, centradas verticalmente)
            Rectangle ZonaAbonar(int w) => new Rectangle(w - (BTN + GAP) * 3 - 10, (H - BTN) / 2, BTN, BTN);
            Rectangle ZonaEditar(int w) => new Rectangle(w - (BTN + GAP) * 2 - 10, (H - BTN) / 2, BTN, BTN);
            Rectangle ZonaElim  (int w) => new Rectangle(w - (BTN + GAP)      - 10, (H - BTN) / 2, BTN, BTN);

            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = card.ClientRectangle;

                // Fondo: primero rellenamos el rect completo con BgPage (esquinas),
                // luego el rounded rect blanco encima
                g.FillRectangle(new SolidBrush(BgPage), r);
                using (var path = RR(r, 14))
                {
                    g.FillPath(new SolidBrush(hov ? Color.FromArgb(10, clr.R, clr.G, clr.B) : Color.White), path);
                    g.DrawPath(new Pen(hov ? Color.FromArgb(80, clr.R, clr.G, clr.B) : Borde, 1.5f), path);
                }

                // Franja lateral
                using (var path = RR(new Rectangle(0, 8, 5, r.Height - 16), 3))
                    g.FillPath(new SolidBrush(clr), path);

                // Círculo emoji
                var cR = new Rectangle(14, (H - 54) / 2, 54, 54);
                using (var path = RR(cR, 27))
                    g.FillPath(new SolidBrush(bgLight), path);

                // Zona de texto (se estrecha al hacer hover)
                int tw = hov ? r.Width - 78 - (BTN + GAP) * 3 - 18 : r.Width - 78 - 90;

                // Nombre
                TextRenderer.DrawText(g, h.Nombre,
                    new Font("Segoe UI", 12f, FontStyle.Bold),
                    new Rectangle(78, 12, tw, 24), Oscuro,
                    TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

                // Saldo · falta
                string sub = $"{h.SaldoActual.ToString("C0", ES)}  ahorrado  ·  Falta {falta.ToString("C0", ES)}  ·  Meta {h.MetaObjetivo.ToString("C0", ES)}";
                TextRenderer.DrawText(g, sub,
                    new Font("Segoe UI", 8.5f),
                    new Rectangle(78, 36, tw, 18), Gris,
                    TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

                // Barra de progreso
                int bx = 78, by = H - 22, bw = tw, bh = 8;
                using (var path = RR(new Rectangle(bx, by, bw, bh), 4))
                    g.FillPath(new SolidBrush(Color.FromArgb(229, 231, 235)), path);
                if (pct > 0)
                {
                    int fw = Math.Max(12, bw * pct / 100);
                    using (var path = RR(new Rectangle(bx, by, fw, bh), 4))
                    using (var br = new LinearGradientBrush(
                        new Rectangle(bx, by, fw + 1, bh),
                        Color.FromArgb(Math.Min(255, clr.R + 50), Math.Min(255, clr.G + 50), Math.Min(255, clr.B + 50)),
                        clr, LinearGradientMode.Horizontal))
                        g.FillPath(br, path);
                }

                if (!hov)
                {
                    // Badge %
                    string pctTxt = pct + "%";
                    var pF  = new Font("Segoe UI", 9f, FontStyle.Bold);
                    var pSz = TextRenderer.MeasureText(pctTxt, pF);
                    var pR  = new Rectangle(r.Width - pSz.Width - 20, (H - 24) / 2, pSz.Width + 14, 24);
                    using (var path = RR(pR, 9))
                        g.FillPath(new SolidBrush(bgLight), path);
                    TextRenderer.DrawText(g, pctTxt, pF, pR, clr,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                else
                {
                    // [+] Abonar
                    var zA = ZonaAbonar(r.Width);
                    using (var path = RR(zA, 7)) g.FillPath(new SolidBrush(clr), path);
                    TextRenderer.DrawText(g, "+", new Font("Segoe UI", 14, FontStyle.Bold),
                        zA, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    // [✏] Editar
                    var zE = ZonaEditar(r.Width);
                    using (var path = RR(zE, 7)) g.FillPath(new SolidBrush(clrEdit), path);
                    TextRenderer.DrawText(g, "✏", new Font("Segoe UI", 9),
                        zE, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    // [✕] Eliminar
                    var zD = ZonaElim(r.Width);
                    using (var path = RR(zD, 7)) g.FillPath(new SolidBrush(clrDel), path);
                    TextRenderer.DrawText(g, "✕", new Font("Segoe UI", 9, FontStyle.Bold),
                        zD, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            };

            // Emoji label
            var lIco = new Label
            {
                Text      = h.Emoji ?? "🐷",
                Font      = new Font("Segoe UI Emoji", 20),
                BackColor = Color.Transparent,
                AutoSize  = false,
                Size      = new Size(54, 54),
                TextAlign = ContentAlignment.MiddleCenter,
                Location  = new Point(14, (H - 54) / 2),
                Cursor    = Cursors.Hand
            };
            card.Controls.Add(lIco);

            // Hover
            card.MouseEnter += (s, e) => { hov = true;  card.Invalidate(); };
            card.MouseLeave += (s, e) => { hov = false; card.Invalidate(); };
            lIco.MouseEnter += (s, e) => { hov = true;  card.Invalidate(); };
            lIco.MouseLeave += (s, e) => { hov = false; card.Invalidate(); };

            // Clic con hit-testing
            System.Windows.Forms.MouseEventHandler onDown = (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                Point pt = (s == (object)lIco)
                    ? card.PointToClient(lIco.PointToScreen(e.Location))
                    : e.Location;
                int w = card.Width;

                if (ZonaEditar(w).Contains(pt))
                {
                    using (var frm = new FrmEditarHucha(h))
                    {
                        if (frm.ShowDialog(this) == DialogResult.OK)
                        {
                            _huchaSvc.Actualizar(frm.HuchaActualizada);
                            CargarDatos();
                        }
                    }
                }
                else if (ZonaElim(w).Contains(pt))
                {
                    var res = MessageBox.Show(
                        $"¿Seguro que quieres eliminar \"{h.Nombre}\"?\n\nEl dinero ahorrado NO se devolverá a tu cuenta.",
                        "Eliminar hucha", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (res == DialogResult.Yes)
                    {
                        _huchaSvc.Eliminar(h.Id);
                        CargarDatos();
                    }
                }
                else
                {
                    using (var frm = new FrmAbonarHucha(h, _saldoCuenta))
                    {
                        if (frm.ShowDialog(this) != DialogResult.OK) return;

                        bool ok = _huchaSvc.AñadirSaldo(h.Id, frm.MontoIngresado, _cuentaId, _movSvc, h.Nombre);
                        if (!ok) { MessageBox.Show("No se pudo abonar.", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                        // Comprobar meta alcanzada
                        decimal nuevoSaldo = h.SaldoActual + frm.MontoIngresado;
                        if (nuevoSaldo >= h.MetaObjetivo)
                        {
                            var resp = MessageBox.Show(
                                $"🎉 ¡Has alcanzado la meta \"{h.Nombre}\"!\n\n" +
                                "¿Deseas archivar esta hucha?",
                                "¡Meta alcanzada!", MessageBoxButtons.YesNo, MessageBoxIcon.None);
                            if (resp == DialogResult.Yes)
                                _huchaSvc.Eliminar(h.Id);
                        }
                        CargarDatos();
                    }
                }
            };

            card.MouseDown += onDown;
            lIco.MouseDown += onDown;

            return card;
        }

        // ── Helpers ───────────────────────────────────────────────
        private static Color ParseColor(string hex)
        {
            try { if (!string.IsNullOrWhiteSpace(hex)) return ColorTranslator.FromHtml(hex); }
            catch { }
            return Color.FromArgb(99, 102, 241);
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
