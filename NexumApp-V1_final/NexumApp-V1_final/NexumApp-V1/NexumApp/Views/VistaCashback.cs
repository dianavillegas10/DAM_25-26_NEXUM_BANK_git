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
    public class VistaCashback : UserControl
    {
        // ── Estado sesión ─────────────────────────────────────────────
        private static readonly HashSet<int>            _activados = new HashSet<int>();
        private static readonly Dictionary<int, decimal> _acumulado = new Dictionary<int, decimal>();

        // ── Servicios ─────────────────────────────────────────────────
        private readonly MovimientoService _movSvc    = new MovimientoService();
        private readonly CuentaService     _cuentaSvc = new CuentaService();

        // ── Categorías  (ico · nombre · % · bgColor · accentColor) ───
        private static readonly (string Ico, string Nm, double Pct, Color Bg, Color Ac)[] Cats =
        {
            ("🛒","Supermercados",3.0, Color.FromArgb(220,252,231), Color.FromArgb( 16,185,129)),
            ("⛽","Gasolineras",  4.0, Color.FromArgb(255,237,213), Color.FromArgb(249,115, 22)),
            ("🍔","Restaurantes", 2.0, Color.FromArgb(254,226,226), Color.FromArgb(239, 68, 68)),
            ("🛍️","Online",       3.0, Color.FromArgb(219,234,254), Color.FromArgb( 59,130,246)),
            ("✈️","Viajes",       5.0, Color.FromArgb(237,233,254), Color.FromArgb(139, 92,246)),
            ("🏥","Farmacia",     2.5, Color.FromArgb(204,251,241), Color.FromArgb( 20,184,166)),
            ("🎬","Ocio",         1.5, Color.FromArgb(252,231,243), Color.FromArgb(236, 72,153)),
            ("💡","Servicios",    1.0, Color.FromArgb(241,245,249), Color.FromArgb(100,116,139)),
        };

        // ── Niveles ───────────────────────────────────────────────────
        private static readonly (string Ico, string Nm, decimal Min, decimal Max, Color Clr)[] Nivs =
        {
            ("🥉","Bronze",   0m,  200m, Color.FromArgb(180,120, 60)),
            ("🥈","Silver",  200m, 500m, Color.FromArgb(120,128,140)),
            ("🥇","Gold",    500m,1000m, Color.FromArgb(200,160,  0)),
            ("💎","Platinum",1000m,9999m,Color.FromArgb( 99,102,241)),
        };

        // ── Paleta ────────────────────────────────────────────────────
        private static readonly Color A1     = Color.FromArgb(245,158, 11);  // amber
        private static readonly Color A2     = Color.FromArgb(146, 86,  0);  // amber dark
        private static readonly Color Verde  = Color.FromArgb( 16,185,129);
        private static readonly Color VerdD  = Color.FromArgb( 10,140, 96);
        private static readonly Color Oscuro = Color.FromArgb( 15, 23, 42);
        private static readonly Color Gris   = Color.FromArgb(100,116,139);
        private static readonly Color Fondo  = Color.FromArgb(248,250,252);
        private static readonly Color Border = Color.FromArgb(226,232,240);

        // ── Layout ────────────────────────────────────────────────────
        private const int W    = 820;   // ancho del contenido
        private const int GAP  = 12;    // gap entre cards
        private const int VGAP = 20;    // gap vertical entre secciones

        private Panel    _main, _scroll, _pnlHist;
        private Label    _lblSimResult;
        private ComboBox _cmbCat;
        private TextBox  _txtImp;

        private static readonly CultureInfo ES = CultureInfo.CreateSpecificCulture("es-ES");

        public event EventHandler VolverAlInicio;

        public VistaCashback()
        {
            BackColor      = Fondo;
            Dock           = DockStyle.Fill;
            DoubleBuffered = true;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            int uid = SesionActual.Instancia?.Usuario?.Id ?? 0;
            if (_activados.Contains(uid)) BuildDashboard();
            else                          BuildActivacion();
        }

        // ── Contenedor base ───────────────────────────────────────────
        private void InitBase()
        {
            Controls.Clear();
            _scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Fondo };
            _main   = new Panel { Width = W, AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent };
            _scroll.Controls.Add(_main);
            _scroll.Resize += (s, ev) =>
                _main.Left = Math.Max(20, (_scroll.ClientSize.Width - W) / 2);
            Controls.Add(_scroll);
        }

        // ══════════════════════════════════════════════════════════════
        //  ACTIVACIÓN
        // ══════════════════════════════════════════════════════════════
        private void BuildActivacion()
        {
            InitBase();
            int y = 0;

            // ── 1. HEADER (160 px) ────────────────────────────────────
            var hdr = Hdr(y, 160, false, 0m); _main.Controls.Add(hdr); y += 172;

            // ── 2. BADGE BIENVENIDA (60 px) ───────────────────────────
            var badge = new Panel { Location = new Point(0, y), Size = new Size(W, 60) };
            badge.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(badge.ClientRectangle, 14))
                using (var b = new LinearGradientBrush(badge.ClientRectangle,
                    Color.FromArgb(254,243,199), Color.FromArgb(253,230,138),
                    LinearGradientMode.Horizontal))
                    g.FillPath(b, path);
                TextRenderer.DrawText(g,
                    "🎉   Activa hoy y recibe 5 € de bienvenida en tu cuenta",
                    new Font("Segoe UI Emoji", 11, FontStyle.Bold),
                    badge.ClientRectangle, Color.FromArgb(146,64,14),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            _main.Controls.Add(badge); y += 72;

            // ── 3. CATEGORÍAS ─────────────────────────────────────────
            _main.Controls.Add(SectionLabel("Tu cashback por categoría", new Point(0, y))); y += 36;
            var gCat = CatGrid(new Point(0, y), false); _main.Controls.Add(gCat); y += gCat.Height + VGAP;

            // ── 4. NIVELES ────────────────────────────────────────────
            _main.Controls.Add(SectionLabel("Cuanto más uses Nexum, más ganas", new Point(0, y))); y += 36;
            var nivRow = NivRow(new Point(0, y), -1); _main.Controls.Add(nivRow); y += nivRow.Height + VGAP + 4;

            // ── 5. TÉRMINOS ───────────────────────────────────────────
            var chk = new CheckBox
            {
                Location = new Point(0, y),
                Text     = "  He leído y acepto los Términos y Condiciones del programa Cashback",
                Font     = new Font("Segoe UI", 10.5f),
                ForeColor = Oscuro, AutoSize = true, Cursor = Cursors.Hand
            };
            _main.Controls.Add(chk); y += 44;

            // ── 6. BOTÓN ACTIVAR ──────────────────────────────────────
            var btn = MakeBtn("🎁   Activar Cashback — 5 € de regalo",
                Color.FromArgb(156,163,175), Color.FromArgb(130,140,150),
                new Point(0, y), new Size(W, 56));
            btn.Enabled = false;
            chk.CheckedChanged += (s, e2) =>
            {
                btn.Enabled   = chk.Checked;
                btn.BackColor = chk.Checked ? Verde : Color.FromArgb(156,163,175);
                btn.FlatAppearance.MouseOverBackColor = chk.Checked ? VerdD : Color.FromArgb(130,140,150);
            };
            btn.Click += (s, e2) => EjecutarActivacion();
            _main.Controls.Add(btn); y += 72;
            _main.Height = y + 24;

            BeginInvoke(new Action(() =>
                _main.Left = Math.Max(20, (_scroll.ClientSize.Width - W) / 2)));
        }

        private void EjecutarActivacion()
        {
            var usr = SesionActual.Instancia?.Usuario;
            if (usr == null) return;
            var cc = _cuentaSvc.ObtenerCuentasPorUsuario(usr.Id);
            if (cc == null || cc.Count == 0)
            { MessageBox.Show("Necesitas al menos una cuenta activa.", "Nexum Cashback", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (_movSvc.RegistrarIngreso(cc[0].Id, 5m, "Cashback — Bono bienvenida Nexum Rewards"))
            {
                _activados.Add(usr.Id);
                _acumulado[usr.Id] = 5m;
                MessageBox.Show("✅  ¡Cashback activado!\n\nSe han abonado 5,00 € en tu cuenta como regalo de bienvenida.",
                    "Nexum Cashback", MessageBoxButtons.OK, MessageBoxIcon.None);
                BuildDashboard();
            }
            else
                MessageBox.Show("No se pudo activar. Inténtalo de nuevo.", "Nexum Cashback", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // ══════════════════════════════════════════════════════════════
        //  DASHBOARD
        // ══════════════════════════════════════════════════════════════
        private void BuildDashboard()
        {
            InitBase();
            int uid  = SesionActual.Instancia?.Usuario?.Id ?? 0;
            decimal ac = _acumulado.TryGetValue(uid, out var v) ? v : 5m;
            int nivelIdx = NivelIdx(ac);
            int y = 0;

            // ── HEADER ────────────────────────────────────────────────
            var hdr = Hdr(y, 180, true, ac); _main.Controls.Add(hdr); y += 192;

            // ── STATS ROW ─────────────────────────────────────────────
            int cw3 = (W - 2 * GAP) / 3;
            _main.Controls.Add(StatCard("💰","Bono bienvenida","5,00 €",  A1,
                new Point(0, y), cw3));
            _main.Controls.Add(StatCard("📈","Cashback extra",
                (ac > 5m ? (ac-5m).ToString("C2",ES) : "0,00 €"), Verde,
                new Point(cw3+GAP, y), cw3));
            _main.Controls.Add(StatCard(Nivs[nivelIdx].Ico,"Tu nivel",
                Nivs[nivelIdx].Nm, Nivs[nivelIdx].Clr,
                new Point(2*(cw3+GAP), y), cw3));
            y += 96 + VGAP;

            // ── CATEGORÍAS ────────────────────────────────────────────
            _main.Controls.Add(SectionLabel("Tus porcentajes activos", new Point(0, y))); y += 36;
            var gCat = CatGrid(new Point(0, y), true); _main.Controls.Add(gCat); y += gCat.Height + VGAP;

            // ── SIMULADOR ─────────────────────────────────────────────
            _main.Controls.Add(SectionLabel("🧮  Simulador de Cashback", new Point(0, y))); y += 36;
            var sim = BuildSimulador(uid, new Point(0, y)); _main.Controls.Add(sim); y += sim.Height + VGAP;

            // ── NIVELES ───────────────────────────────────────────────
            _main.Controls.Add(SectionLabel("🏆  Progresión de nivel", new Point(0, y))); y += 36;
            var niv = NivRow(new Point(0, y), nivelIdx); _main.Controls.Add(niv); y += niv.Height + VGAP;

            // ── HISTORIAL ─────────────────────────────────────────────
            _main.Controls.Add(SectionLabel("📋  Historial de Cashback", new Point(0, y))); y += 36;
            _pnlHist = new Panel { Location = new Point(0, y), Width = W, BackColor = Color.Transparent };
            _main.Controls.Add(_pnlHist);
            LoadHistorial(uid, ref y);
            _main.Height = y + 40;

            BeginInvoke(new Action(() =>
                _main.Left = Math.Max(20, (_scroll.ClientSize.Width - W) / 2)));
        }

        // ══════════════════════════════════════════════════════════════
        //  COMPONENTES REUTILIZABLES
        // ══════════════════════════════════════════════════════════════

        /// Header amber: si dashboard=true, muestra saldo; si false, muestra slogan
        private Panel Hdr(int y, int h, bool dashboard, decimal acum)
        {
            var p = new Panel { Location = new Point(0, y), Size = new Size(W, h), BackColor = A1 };

            // Fondo
            p.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = p.ClientRectangle;
                using (var b = new LinearGradientBrush(r, A1, A2, 145f))
                    g.FillRectangle(b, r);
                using (var b2 = new LinearGradientBrush(new Rectangle(0,0,r.Width,r.Height/2),
                    Color.FromArgb(50,255,255,255), Color.FromArgb(0,255,255,255),
                    LinearGradientMode.Vertical))
                    g.FillRectangle(b2, 0, 0, r.Width, r.Height/2);
                // Círculos decorativos
                using (var br = new SolidBrush(Color.FromArgb(15,255,255,255)))
                { g.FillEllipse(br,-50,-50,230,230); g.FillEllipse(br,r.Width-110,-60,240,240); }
            };

            // ── BOTÓN ← INICIO (presente en ambos modos) ──────────────
            bool hov = false;
            var btnBack = new Panel { Location = new Point(12, 8), Size = new Size(80, 26),
                BackColor = Color.Transparent, Cursor = Cursors.Hand };
            btnBack.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = btnBack.ClientRectangle;
                if (hov)
                    using (var path = RR(r, 8))
                        g.FillPath(new SolidBrush(Color.FromArgb(40, 255, 255, 255)), path);
                Color tc = hov ? Color.White : Color.FromArgb(200, 255, 255, 255);
                int cy = r.Height / 2;
                using (var pen = new Pen(tc, 1.8f) { LineJoin = LineJoin.Round,
                    StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawLine(pen, 16, cy, 8, cy);
                    g.DrawLines(pen, new[] { new PointF(12f, cy-4f), new PointF(8f, cy), new PointF(12f, cy+4f) });
                }
                TextRenderer.DrawText(g, "Inicio",
                    new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    new Rectangle(22, 0, r.Width-24, r.Height), tc,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            };
            btnBack.MouseEnter += (s, e) => { hov = true;  btnBack.Invalidate(); };
            btnBack.MouseLeave += (s, e) => { hov = false; btnBack.Invalidate(); };
            btnBack.Click      += (s, e) => VolverAlInicio?.Invoke(this, EventArgs.Empty);
            p.Controls.Add(btnBack);

            if (!dashboard)
            {
                // ── ACTIVACIÓN: título desplazado para no solapar el botón
                var pTit = new Panel { BackColor = Color.Transparent,
                    Location = new Point(0, 46), Size = new Size(W, 52) };
                pTit.Paint += (s, e) =>
                    TextRenderer.DrawText(e.Graphics, "NEXUM CASHBACK",
                        new Font("Segoe UI", 26, FontStyle.Bold),
                        pTit.ClientRectangle, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                var pSub = new Panel { BackColor = Color.Transparent,
                    Location = new Point(20, 100), Size = new Size(W-40, 36) };
                pSub.Paint += (s, e) =>
                    TextRenderer.DrawText(e.Graphics,
                        "Gana dinero real con cada compra. Gratis y automático.",
                        new Font("Segoe UI", 11),
                        pSub.ClientRectangle, Color.FromArgb(240,255,255,255),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                p.Controls.Add(pTit);
                p.Controls.Add(pSub);
            }
            else
            {
                // ── DASHBOARD: nivel + saldo, desplazados para no solapar el botón
                int nivelIdx = NivelIdx(acum);
                var nivel = Nivs[nivelIdx];

                var pNiv = new Panel { BackColor = Color.Transparent,
                    Location = new Point(0, 36), Size = new Size(W, 28) };
                pNiv.Paint += (s, e) =>
                    TextRenderer.DrawText(e.Graphics,
                        $"{nivel.Ico}  Nivel {nivel.Nm}  ·  Cashback ACTIVO ✓",
                        new Font("Segoe UI Emoji", 11, FontStyle.Bold),
                        pNiv.ClientRectangle, Color.FromArgb(235,255,255,255),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                var pSaldo = new Panel { BackColor = Color.Transparent,
                    Location = new Point(0, 66), Size = new Size(W, 70) };
                pSaldo.Paint += (s, e) =>
                    TextRenderer.DrawText(e.Graphics, acum.ToString("C2", ES),
                        new Font("Segoe UI", 36, FontStyle.Bold),
                        pSaldo.ClientRectangle, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                var pSub = new Panel { BackColor = Color.Transparent,
                    Location = new Point(0, 142), Size = new Size(W, 26) };
                pSub.Paint += (s, e) =>
                    TextRenderer.DrawText(e.Graphics, "Cashback acumulado en esta sesión",
                        new Font("Segoe UI", 10),
                        pSub.ClientRectangle, Color.FromArgb(220,255,255,255),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                p.Controls.Add(pNiv);
                p.Controls.Add(pSaldo);
                p.Controls.Add(pSub);
            }
            return p;
        }

        private Panel CatGrid(Point loc, bool activo)
        {
            int cols = 4;
            int cw   = (W - (cols-1)*GAP) / cols;  // 196px cada una
            int ch   = 104;
            int rows = (int)Math.Ceiling(Cats.Length / (double)cols);
            var grid = new Panel { Location = loc, Size = new Size(W, rows*(ch+GAP)-GAP),
                BackColor = Color.Transparent };

            for (int i = 0; i < Cats.Length; i++)
            {
                var (ico, nm, pct, bg, ac) = Cats[i];
                int col = i % cols, row = i / cols;
                int cx  = col * (cw + GAP);
                int cy  = row * (ch + GAP);

                var card = new Panel { Location = new Point(cx, cy), Size = new Size(cw, ch), BackColor = bg };
                card.Paint += (s, e) =>
                {
                    var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var path = RR(card.ClientRectangle, 14))
                    {
                        g.FillPath(new SolidBrush(bg), path);
                        g.DrawPath(new Pen(activo ? ac : Color.FromArgb(40, ac.R, ac.G, ac.B), activo ? 1.8f : 1f), path);
                    }
                    // Badge % — esquina superior derecha
                    string pctTxt = pct.ToString("0.#") + "%";
                    var badgeFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                    var badgeSz   = TextRenderer.MeasureText(pctTxt, badgeFont);
                    var badgeRect = new Rectangle(cw - badgeSz.Width - 16, 8, badgeSz.Width + 12, 22);
                    using (var path = RR(badgeRect, 8))
                        g.FillPath(new SolidBrush(activo ? Verde : ac), path);
                    TextRenderer.DrawText(g, pctTxt, badgeFont, badgeRect, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    // Nombre (abajo)
                    var nmRect = new Rectangle(8, ch - 26, cw - 16, 20);
                    TextRenderer.DrawText(g, nm, new Font("Segoe UI", 9f, FontStyle.Bold),
                        nmRect, Oscuro, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                };

                // Emoji via Label con Segoe UI Emoji (centrado arriba)
                var lIco = new Label
                {
                    Text = ico,
                    Font = new Font("Segoe UI Emoji", 26),
                    ForeColor = ac,
                    BackColor = Color.Transparent,
                    AutoSize  = false,
                    Size      = new Size(cw, 62),
                    Location  = new Point(0, 10),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                card.Controls.Add(lIco);
                grid.Controls.Add(card);
            }
            return grid;
        }

        private Panel NivRow(Point loc, int activeIdx)
        {
            int cnt = Nivs.Length;
            int cw  = (W - (cnt-1)*GAP) / cnt;  // 196px
            int ch  = 76;
            var row = new Panel { Location = loc, Size = new Size(W, ch), BackColor = Color.Transparent };

            for (int i = 0; i < cnt; i++)
            {
                var (ico, nm, min, max, clr) = Nivs[i];
                bool act = (i == activeIdx);
                int cx = i * (cw + GAP);

                var c = new Panel { Location = new Point(cx, 0), Size = new Size(cw, ch) };
                c.Paint += (s, e) =>
                {
                    var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                    Color bg = act ? clr : Color.White;
                    using (var path = RR(c.ClientRectangle, 14))
                    {
                        g.FillPath(new SolidBrush(bg), path);
                        g.DrawPath(new Pen(clr, act ? 2f : 1f), path);
                    }
                    if (act)
                    {
                        // Brillo top
                        using (var shine = new LinearGradientBrush(
                            new Rectangle(0,0,c.Width,30), Color.FromArgb(40,255,255,255),
                            Color.FromArgb(0,255,255,255), LinearGradientMode.Vertical))
                        using (var path = RR(c.ClientRectangle, 14))
                            g.FillPath(shine, path);
                    }
                    Color tc    = act ? Color.White : clr;
                    Color subTc = act ? Color.FromArgb(210,255,255,255) : Gris;

                    // Emoji — línea superior centrada
                    var rectIco = new Rectangle(0, 8, cw, 30);
                    TextRenderer.DrawText(g, ico, new Font("Segoe UI Emoji", 18),
                        rectIco, tc, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    // Nombre — línea media
                    var rectNm = new Rectangle(0, 38, cw, 20);
                    TextRenderer.DrawText(g, nm, new Font("Segoe UI", 10, FontStyle.Bold),
                        rectNm, tc, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    // Rango — línea inferior
                    string rng = min == 0 ? $"< {max:C0}" : $"{min:C0}–{max:C0}";
                    var rectRng = new Rectangle(0, 56, cw, 18);
                    TextRenderer.DrawText(g, rng, new Font("Segoe UI", 8f),
                        rectRng, subTc, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                };
                row.Controls.Add(c);
            }
            return row;
        }

        private Panel StatCard(string ico, string tit, string val, Color ac, Point loc, int cw)
        {
            var c = new Panel { Location = loc, Size = new Size(cw, 88), BackColor = Color.White };
            c.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(c.ClientRectangle, 14))
                { g.FillPath(Brushes.White, path); g.DrawPath(new Pen(Border), path); }
                // Franja de color (5px top)
                g.FillRectangle(new SolidBrush(ac), 1, 1, cw-2, 5);
                // Emoji
                var rIco = new Rectangle(14, 14, 34, 30);
                TextRenderer.DrawText(g, ico, new Font("Segoe UI Emoji", 16), rIco, ac,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                // Título
                TextRenderer.DrawText(g, tit, new Font("Segoe UI", 8.5f),
                    new Rectangle(52, 14, cw-60, 18), Gris, TextFormatFlags.VerticalCenter);
                // Valor
                TextRenderer.DrawText(g, val, new Font("Segoe UI", 13, FontStyle.Bold),
                    new Rectangle(52, 34, cw-60, 34), Oscuro, TextFormatFlags.VerticalCenter);
            };
            ApplyRegion(c, 14);
            return c;
        }

        private Panel BuildSimulador(int uid, Point loc)
        {
            var card = Card(loc, new Size(W, 196));

            card.Controls.Add(SubLabel("Elige la categoría de compra y el importe para ver cuánto ganas:",
                new Point(20, 16), W - 40));

            // Fila: etiquetas
            card.Controls.Add(FieldLbl("Categoría", new Point(20, 52)));
            card.Controls.Add(FieldLbl("Importe del pago (€)", new Point(440, 52)));

            // Combo
            _cmbCat = new ComboBox { Location = new Point(20, 72), Width = 396,
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5f),
                FlatStyle = FlatStyle.Flat, BackColor = Color.White };
            foreach (var (ico, nm, pct, _, __) in Cats)
                _cmbCat.Items.Add($"{ico}  {nm}  ({pct}%)");
            _cmbCat.SelectedIndex = 0;
            card.Controls.Add(_cmbCat);

            // Importe
            _txtImp = new TextBox { Location = new Point(440, 72), Width = 170,
                Font = new Font("Segoe UI", 13, FontStyle.Bold), BorderStyle = BorderStyle.FixedSingle,
                Text = "100", ForeColor = Oscuro };
            card.Controls.Add(_txtImp);

            // Botón calcular
            var btn = MakeBtn("▶  Calcular", Verde, VerdD, new Point(630, 68), new Size(170, 42));
            card.Controls.Add(btn);

            // Separador
            card.Controls.Add(new Panel { Location = new Point(20, 126), Size = new Size(W-40, 1), BackColor = Border });

            // Resultado
            _lblSimResult = new Label { Location = new Point(20, 136), Size = new Size(W-40, 30),
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold), ForeColor = Verde,
                BackColor = Color.Transparent, AutoSize = false };
            card.Controls.Add(_lblSimResult);

            // Nota
            card.Controls.Add(SubLabel("ℹ️  El cashback se acredita automáticamente en tu cuenta Nexum en 24-48h.",
                new Point(20, 170), W-40));

            btn.Click += (s, e) =>
            {
                string raw = _txtImp.Text.Trim().Replace(",", ".");
                if (!decimal.TryParse(raw, System.Globalization.NumberStyles.Any,
                    CultureInfo.InvariantCulture, out decimal imp) || imp <= 0)
                {
                    _lblSimResult.ForeColor = Color.FromArgb(220, 38, 38);
                    _lblSimResult.Text = "⚠  Introduce un importe válido — ejemplo: 150";
                    return;
                }
                int idx   = Math.Max(0, _cmbCat.SelectedIndex);
                double pct = Cats[idx].Pct;
                decimal cb  = Math.Round(imp * (decimal)(pct / 100), 2);
                _lblSimResult.ForeColor = Verde;
                _lblSimResult.Text = $"💰  Recibirías  {cb.ToString("C2", ES)}  de cashback  ({pct}%)  por este pago";

                if (MessageBox.Show(
                    $"¿Simular y acreditar {cb.ToString("C2", ES)} en tu cuenta?\n\n" +
                    $"Categoría: {Cats[idx].Nm}\nImporte: {imp.ToString("C2", ES)}",
                    "Nexum Cashback", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    DoAcreditar(uid, cb, $"Cashback {Cats[idx].Nm} ({pct}%) — pago {imp.ToString("C2", ES)}");
            };
            return card;
        }

        private void LoadHistorial(int uid, ref int y)
        {
            if (_pnlHist == null) return;
            _pnlHist.Controls.Clear();

            var movs = _movSvc.ObtenerMovimientosRecientesPorUsuario(uid, 50);
            var cbs  = new List<Movimiento>();
            foreach (var m in movs)
                if (m.Concepto != null && m.Concepto.IndexOf("Cashback", StringComparison.OrdinalIgnoreCase) >= 0)
                    cbs.Add(m);

            if (cbs.Count == 0)
            {
                _pnlHist.Controls.Add(new Label
                {
                    Text = "Aún no hay movimientos de cashback. ¡Usa el simulador para probarlo!",
                    Font = new Font("Segoe UI", 10), ForeColor = Gris,
                    AutoSize = true, Location = Point.Empty, BackColor = Color.Transparent
                });
                _pnlHist.Height = 32; y += 32; return;
            }

            int hy = 0;
            foreach (var m in cbs)
            {
                var row = HistRow(m, hy); _pnlHist.Controls.Add(row); hy += 64;
            }
            _pnlHist.Height = hy; y += hy;
        }

        private Panel HistRow(Movimiento m, int top)
        {
            var p = Card(new Point(0, top), new Size(W, 56));
            p.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                // Círculo verde con emoji
                g.FillEllipse(new SolidBrush(Color.FromArgb(220,252,231)), 14, 10, 36, 36);
                TextRenderer.DrawText(g, "💰", new Font("Segoe UI Emoji", 16),
                    new Rectangle(14, 10, 36, 36), Verde,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                // Concepto
                TextRenderer.DrawText(g, m.Concepto ?? "Cashback",
                    new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    new Rectangle(60, 6, W-160, 22), Oscuro, TextFormatFlags.EndEllipsis);
                // Fecha
                TextRenderer.DrawText(g, m.Fecha.ToString("dd MMM yyyy  ·  HH:mm", ES),
                    new Font("Segoe UI", 8.5f),
                    new Rectangle(60, 28, W-160, 18), Gris, TextFormatFlags.Default);
                // Monto
                string montoTxt = "+ " + m.Monto.ToString("C2", ES);
                TextRenderer.DrawText(g, montoTxt, new Font("Segoe UI", 12, FontStyle.Bold),
                    new Rectangle(W-160, 14, 140, 28), Verde,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            };
            return p;
        }

        private void DoAcreditar(int uid, decimal monto, string concepto)
        {
            var cc = _cuentaSvc.ObtenerCuentasPorUsuario(SesionActual.Instancia?.Usuario?.Id ?? 0);
            if (cc == null || cc.Count == 0) return;
            if (_movSvc.RegistrarIngreso(cc[0].Id, monto, concepto))
            {
                _acumulado[uid] = (_acumulado.TryGetValue(uid, out var p) ? p : 0m) + monto;
                if (_lblSimResult != null)
                    _lblSimResult.Text = $"✅  {monto.ToString("C2", ES)} acreditados en tu cuenta";
                int y2 = 0;
                if (_pnlHist != null) { _pnlHist.Controls.Clear(); LoadHistorial(uid, ref y2); }
                MessageBox.Show($"✅  {monto.ToString("C2", ES)} de cashback abonados en tu cuenta.",
                    "Nexum Cashback", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  HELPERS
        // ══════════════════════════════════════════════════════════════

        private static int NivelIdx(decimal v)
        {
            for (int i = 0; i < Nivs.Length; i++)
                if (v >= Nivs[i].Min && v < Nivs[i].Max) return i;
            return Nivs.Length - 1;
        }

        private static Panel Card(Point loc, Size sz)
        {
            var p = new Panel { Location = loc, Size = sz, BackColor = Color.White };
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(p.ClientRectangle, 14))
                { e.Graphics.FillPath(Brushes.White, path); e.Graphics.DrawPath(new Pen(Border), path); }
            };
            ApplyRegion(p, 14);
            return p;
        }

        private static Button MakeBtn(string txt, Color bg, Color hover, Point loc, Size sz)
        {
            var b = new Button
            {
                Text = txt, Location = loc, Size = sz,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White, BackColor = bg,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor  = hover;
            b.FlatAppearance.MouseDownBackColor  = ControlPaint.Dark(bg, 0.15f);
            ApplyRegion(b, 12);
            return b;
        }

        private static void ApplyRegion(Control c, int r)
        {
            EventHandler apply = (s, e) =>
            {
                if (c.Width > 4 && c.Height > 4)
                    try { c.Region = new Region(RR(c.ClientRectangle, r)); } catch { }
            };
            c.HandleCreated += apply;
            c.Resize        += apply;
        }

        private static Label SectionLabel(string txt, Point loc)
            => new Label { Text = txt, Location = loc, AutoSize = true,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Oscuro, BackColor = Color.Transparent };

        private static Label SubLabel(string txt, Point loc, int maxW)
            => new Label { Text = txt, Location = loc, MaximumSize = new Size(maxW, 0),
                AutoSize = true, Font = new Font("Segoe UI", 9.5f),
                ForeColor = Gris, BackColor = Color.Transparent };

        private static Label FieldLbl(string txt, Point loc)
            => new Label { Text = txt, Location = loc, AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Gris, BackColor = Color.Transparent };

        private static GraphicsPath RR(Rectangle r, int rad)
        {
            var path = new GraphicsPath(); int d = rad * 2;
            if (r.Width < d || r.Height < d) { path.AddRectangle(r); return path; }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right-d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right-d, r.Bottom-d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom-d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
