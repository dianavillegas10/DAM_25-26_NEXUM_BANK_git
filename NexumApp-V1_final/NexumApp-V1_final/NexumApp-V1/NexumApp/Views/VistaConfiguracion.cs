using NexumApp.Forms.Principal;
using NexumApp.Helpers;
using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NexumApp.Views
{
    public class VistaConfiguracion : UserControl
    {
        // ── Servicios ──────────────────────────────────────────
        private readonly AuthService          _authSvc = new AuthService();
        private readonly ConfiguracionService _cfgSvc  = new ConfiguracionService();

        // ── Paleta dinámica ────────────────────────────────────
        private Color C_Bg      => AppSettings.ModoOscuro ? Color.FromArgb(10,  12,  28)  : Color.FromArgb(244, 247, 254);
        private Color C_Surface => AppSettings.ModoOscuro ? Color.FromArgb(14,  17,  38)  : Color.FromArgb(255, 255, 255);
        private Color C_Card    => AppSettings.ModoOscuro ? Color.FromArgb(18,  22,  46)  : Color.FromArgb(248, 249, 252);
        private Color C_Input   => AppSettings.ModoOscuro ? Color.FromArgb(24,  29,  58)  : Color.FromArgb(240, 242, 248);
        private Color C_Border  => AppSettings.ModoOscuro ? Color.FromArgb(38,  44,  80)  : Color.FromArgb(220, 225, 235);
        private Color C_Text    => AppSettings.ModoOscuro ? Color.FromArgb(241, 245, 249) : Color.FromArgb(31,  41,  55);
        private Color C_Muted   => AppSettings.ModoOscuro ? Color.FromArgb(100, 116, 139) : Color.FromArgb(107, 114, 128);
        private Color C_NavBg   => AppSettings.ModoOscuro ? Color.FromArgb(12,  15,  34)  : Color.FromArgb(248, 249, 255);

        // Accent — invariantes al tema
        private static readonly Color C_Blue   = Color.FromArgb(99,  102, 241);
        private static readonly Color C_Violet = Color.FromArgb(139,  92, 246);
        private static readonly Color C_Green  = Color.FromArgb(52,  211, 153);
        private static readonly Color C_Red    = Color.FromArgb(248, 113, 113);
        private static readonly Color C_Gold   = Color.FromArgb(251, 191,  36);
        private static readonly Color C_Cyan   = Color.FromArgb(34,  211, 238);

        // ── Estado UI ─────────────────────────────────────────
        private Panel        _pnlContent;
        private Panel        _activeTabBtn;
        private int          _tabActiva = 0;

        private ToggleSwitch _tglEmail, _tglSMS, _tglPush, _tglMarketing;
        private ToggleSwitch _tglModoOscuro, _tglAltoContraste;
        private ToggleSwitch _tgl2FA, _tglSesionSeg, _tglMostrarSaldo, _tglOrdenar, _tglConfirmar, _tglBenef;
        private ComboBox     _cmbIdioma, _cmbMoneda;
        private TrackBar     _trackFuente;
        private Label        _lblFuenteVal, _lblPreviewTema;
        private NumericUpDown _numSesion;
        private TextBox      _txtPassActual, _txtPassNueva, _txtPassConfirm;
        private Label        _lblPassError, _lblPassStrength;

        public event EventHandler VolverAlInicio;
        public event EventHandler TemaAplicado;
        public event EventHandler ConfiguracionGuardada;

        public VistaConfiguracion()
        {
            BackColor      = C_Bg;
            Dock           = DockStyle.Fill;
            DoubleBuffered = true;
            AppSettings.ConfiguracionChanged += (s, e) =>
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(new Action(() =>
                {
                    BackColor = C_Bg;
                    BuildShell();
                    CargarTab(_tabActiva);
                    AppSettings.AplicarTraduccionesRecursivo(this);
                }));
            };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            int uid = SesionActual.Instancia?.Usuario?.Id ?? 0;
            if (uid > 0)
            {
                var cfgBD = _cfgSvc.ObtenerConfiguracion(uid);
                if (SesionActual.Instancia.Configuracion == null)
                    SesionActual.Instancia.Configuracion = cfgBD;
                else
                {
                    var sess = SesionActual.Instancia.Configuracion;
                    sess.NotificacionesEmail = cfgBD.NotificacionesEmail;
                    sess.ModoOscuro          = cfgBD.ModoOscuro;
                    sess.Idioma              = cfgBD.Idioma;
                    sess.MonedaPreferida     = cfgBD.MonedaPreferida;
                    sess.DosFactores         = cfgBD.DosFactores;
                }
            }
            BuildShell();
            CargarTab(0);
        }

        // ══════════════════════════════════════════════════════
        //  SHELL — estructura principal
        // ══════════════════════════════════════════════════════
        private void BuildShell()
        {
            Controls.Clear();
            BackColor = C_Bg;

            // ── Header ────────────────────────────────────────
            var hdr = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.Transparent };
            hdr.Paint += (s, ev) =>
            {
                var g = ev.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = hdr.ClientRectangle;
                using (var b = new LinearGradientBrush(r, C_Surface, C_Bg, LinearGradientMode.Vertical))
                    g.FillRectangle(b, r);
                // Barra de acento superior azul→violeta
                using (var lb = new LinearGradientBrush(new Rectangle(0, 0, r.Width, 3), C_Blue, C_Violet, LinearGradientMode.Horizontal))
                    g.FillRectangle(lb, 0, 0, r.Width, 3);
                using (var p = new Pen(C_Border))
                    g.DrawLine(p, 0, r.Bottom - 1, r.Width, r.Bottom - 1);
            };

            // Icono en burbuja degradada
            var iconH = new Panel { Size = new Size(44, 44), Location = new Point(24, 18), BackColor = Color.Transparent };
            iconH.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(iconH.ClientRectangle, 12))
                using (var b = new LinearGradientBrush(iconH.ClientRectangle, C_Blue, C_Violet, 135f))
                    ev.Graphics.FillPath(b, path);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString("⚙", new Font("Segoe UI", 18), Brushes.White, iconH.ClientRectangle, fmt);
            };

            var lTit = new Label { Text = "Configuración", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = C_Text, BackColor = Color.Transparent, AutoSize = true, Location = new Point(78, 16) };
            var lSub = new Label { Text = "Personaliza tu experiencia Nexum Bank",  Font = new Font("Segoe UI", 8.5f), ForeColor = C_Muted, BackColor = Color.Transparent, AutoSize = true, Location = new Point(79, 42) };

            var btnVolver = new Button
            {
                Text = "← Inicio", Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = C_Muted, BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat,
                Size = new Size(92, 34), Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnVolver.FlatAppearance.BorderColor = C_Border;
            btnVolver.FlatAppearance.BorderSize  = 1;
            btnVolver.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 99, 102, 241);
            btnVolver.MouseEnter += (s, ev) => btnVolver.ForeColor = C_Blue;
            btnVolver.MouseLeave += (s, ev) => btnVolver.ForeColor = C_Muted;
            btnVolver.Click      += (s, ev) => VolverAlInicio?.Invoke(this, EventArgs.Empty);
            EventHandler rndV = (s, ev) => { try { btnVolver.Region = new Region(RR(btnVolver.ClientRectangle, 8)); } catch { } };
            btnVolver.HandleCreated += rndV; btnVolver.Resize += rndV;

            hdr.Controls.AddRange(new Control[] { iconH, lTit, lSub, btnVolver });
            hdr.Resize += (s, ev) => btnVolver.Location = new Point(hdr.Width - btnVolver.Width - 24, (hdr.Height - btnVolver.Height) / 2);
            Controls.Add(hdr);

            // ── Layout dos columnas ───────────────────────────
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
                BackColor = Color.Transparent
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 224));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            Controls.Add(layout);

            // ── Sidebar nav ──────────────────────────────────
            var nav = new Panel { Dock = DockStyle.Fill, BackColor = C_NavBg };
            nav.Paint += (s, ev) =>
            {
                using (var p = new Pen(C_Border))
                    ev.Graphics.DrawLine(p, nav.Width - 1, 0, nav.Width - 1, nav.Height);
            };

            // Mini-tarjeta de usuario
            var usr = SesionActual.Instancia?.Usuario;
            var userCard = new Panel { Location = new Point(14, 18), Size = new Size(196, 62), BackColor = Color.Transparent };
            userCard.Paint += (s, ev) =>
            {
                var g = ev.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(userCard.ClientRectangle, 12))
                { g.FillPath(new SolidBrush(C_Card), path); g.DrawPath(new Pen(C_Border, 1), path); }
                var av = new Rectangle(10, 11, 40, 40);
                using (var b = new LinearGradientBrush(av, C_Blue, C_Violet, 135f))
                    g.FillEllipse(b, av);
                string ini = (usr?.Nombre?.Length > 0 ? $"{usr.Nombre[0]}" : "?").ToUpper()
                           + (usr?.Apellidos?.Length > 0 ? $"{usr.Apellidos[0]}" : "").ToUpper();
                var fmtC = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(ini, new Font("Segoe UI", 12, FontStyle.Bold), Brushes.White, av, fmtC);
                g.DrawString(usr?.Nombre ?? "Usuario", new Font("Segoe UI", 9, FontStyle.Bold), new SolidBrush(C_Text), new RectangleF(58, 12, 130, 20), new StringFormat { LineAlignment = StringAlignment.Center });
                string em = usr?.Email ?? ""; if (em.Length > 20) em = em.Substring(0, 18) + "…";
                g.DrawString(em, new Font("Segoe UI", 7.5f), new SolidBrush(C_Muted), new RectangleF(58, 33, 130, 18), new StringFormat { LineAlignment = StringAlignment.Center });
            };
            nav.Controls.Add(userCard);

            nav.Controls.Add(new Panel { Location = new Point(14, 88), Size = new Size(196, 1), BackColor = C_Border });
            nav.Controls.Add(new Label { Text = "MENÚ", Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = C_Muted, BackColor = Color.Transparent, Location = new Point(18, 98), AutoSize = true });

            var tabDefs = new (string Ico, string Txt, Color Accent)[]
            {
                ("🔔", "Notificaciones", C_Blue),
                ("🎨", "Apariencia",     C_Violet),
                ("🛡️", "Seguridad",       C_Red),
                ("⚙️", "Preferencias",    C_Gold),
                ("👤", "Mi cuenta",       C_Green),
            };

            int ty = 116;
            for (int i = 0; i < tabDefs.Length; i++)
            {
                var btn = CrearTabBtn(tabDefs[i].Ico, tabDefs[i].Txt, tabDefs[i].Accent, i);
                btn.Location = new Point(14, ty);
                nav.Controls.Add(btn);
                if (i == 0) _activeTabBtn = btn;
                ty += 50;
            }

            nav.Controls.Add(new Label { Text = "Nexum Bank  v1.0", Font = new Font("Segoe UI", 7.5f), ForeColor = C_Muted, BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Bottom, Height = 30 });
            layout.Controls.Add(nav, 0, 0);

            // ── Área de contenido ─────────────────────────────
            var wrap = new Panel { Dock = DockStyle.Fill, BackColor = C_Bg, Padding = new Padding(36, 28, 36, 28) };
            _pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, AutoScroll = true };
            wrap.Controls.Add(_pnlContent);
            layout.Controls.Add(wrap, 1, 0);
        }

        private Panel CrearTabBtn(string ico, string txt, Color accent, int idx)
        {
            var btn = new Panel { Size = new Size(196, 46), Cursor = Cursors.Hand, BackColor = Color.Transparent, Tag = idx };
            bool hov = false;
            btn.Paint += (s, ev) =>
            {
                bool activo = btn == _activeTabBtn;
                var g = ev.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = btn.ClientRectangle;

                if (activo)
                {
                    using (var path = RR(r, 10))
                    using (var b = new LinearGradientBrush(r,
                        Color.FromArgb(AppSettings.ModoOscuro ? 30 : 22, accent.R, accent.G, accent.B),
                        Color.FromArgb(AppSettings.ModoOscuro ? 12 : 8,  accent.R, accent.G, accent.B),
                        LinearGradientMode.Horizontal))
                        g.FillPath(b, path);
                    // Barra izquierda
                    using (var path2 = RR(new Rectangle(0, 7, 4, r.Height - 14), 2))
                    using (var b2 = new LinearGradientBrush(new Rectangle(0, 7, 4, r.Height - 14), accent,
                        Color.FromArgb(160, accent.R, accent.G, accent.B), LinearGradientMode.Vertical))
                        g.FillPath(b2, path2);
                }
                else if (hov)
                {
                    using (var path = RR(r, 10))
                        g.FillPath(new SolidBrush(Color.FromArgb(AppSettings.ModoOscuro ? 10 : 6, 255, 255, 255)), path);
                }

                // Burbuja icono
                var iconR = new Rectangle(12, 7, 32, 32);
                if (activo)
                {
                    using (var path = RR(iconR, 8))
                    using (var b = new LinearGradientBrush(iconR, accent,
                        Color.FromArgb(Math.Min(255, accent.R + 30), accent.G, Math.Min(255, accent.B + 30)), 135f))
                        g.FillPath(b, path);
                    var fmt2 = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(ico, new Font("Segoe UI", 13), Brushes.White, iconR, fmt2);
                }
                else
                {
                    using (var path = RR(iconR, 8))
                        g.FillPath(new SolidBrush(Color.FromArgb(AppSettings.ModoOscuro ? 22 : 14, accent.R, accent.G, accent.B)), path);
                    var fmt2 = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(ico, new Font("Segoe UI", 13), new SolidBrush(accent), iconR, fmt2);
                }

                // Etiqueta texto
                g.DrawString(txt, new Font("Segoe UI", 9.5f, activo ? FontStyle.Bold : FontStyle.Regular),
                    new SolidBrush(activo ? C_Text : C_Muted),
                    new RectangleF(52, 0, r.Width - 56, r.Height),
                    new StringFormat { LineAlignment = StringAlignment.Center });
            };
            btn.MouseEnter += (s, e) => { hov = true;  btn.Invalidate(); };
            btn.MouseLeave += (s, e) => { hov = false; btn.Invalidate(); };
            btn.Click      += (s, e) => { _activeTabBtn?.Invalidate(); _activeTabBtn = btn; btn.Invalidate(); CargarTab(idx); };
            return btn;
        }

        // ══════════════════════════════════════════════════════
        //  TABS
        // ══════════════════════════════════════════════════════
        private void CargarTab(int idx)
        {
            _tabActiva = idx;
            _pnlContent.Controls.Clear();
            switch (idx)
            {
                case 0: TabNotificaciones(); break;
                case 1: TabApariencia();     break;
                case 2: TabSeguridad();      break;
                case 3: TabPreferencias();   break;
                case 4: TabCuenta();         break;
            }
        }

        // ── TAB 0: NOTIFICACIONES ─────────────────────────────
        private void TabNotificaciones()
        {
            var cfg = GetConfig(); int y = 0;
            PageHeader("Notificaciones", "Controla cómo y cuándo te avisamos", "🔔", C_Blue, ref y);

            SectionLabel("CANALES DE NOTIFICACIÓN", ref y);
            var card = Card(ref y, 64 * 4);
            SettingRow(card, "📧", C_Blue,   "Correo electrónico",   "Recibe alertas de actividad en tu email",         0, out _tglEmail,     cfg.NotificacionesEmail);
            SettingRow(card, "📱", C_Violet, "SMS",                  "Mensajes de texto para movimientos importantes",   1, out _tglSMS,       cfg.NotificacionesSMS);
            SettingRow(card, "🔔", C_Gold,   "Notificaciones push",  "Alertas instantáneas en este dispositivo",         2, out _tglPush,       cfg.NotificacionesPush);
            SettingRow(card, "📣", C_Green,  "Comunicaciones Nexum", "Ofertas, novedades y promociones exclusivas",      3, out _tglMarketing, cfg.NotificacionesMarketing, isLast: true);

            SectionLabel("ALERTAS INCLUIDAS", ref y);
            var card2 = Card(ref y, 58 * 3);
            InfoRow(card2, "💸", C_Green, "Movimientos de cuenta", "Ingresos, retiros y cargos en tiempo real",   0);
            InfoRow(card2, "✈", C_Blue,  "Transferencias",         "Confirmaciones de envío y recepción",          1);
            InfoRow(card2, "🔐", C_Gold,  "Alertas de seguridad",  "Nuevos accesos y cambios de contraseña",       2, isLast: true);

            SaveBar(ref y, () =>
            {
                cfg.NotificacionesEmail     = _tglEmail.Checked;
                cfg.NotificacionesSMS       = _tglSMS.Checked;
                cfg.NotificacionesPush      = _tglPush.Checked;
                cfg.NotificacionesMarketing = _tglMarketing.Checked;
                Guardar(cfg, "Notificaciones guardadas correctamente.");
            });
        }

        // ── TAB 1: APARIENCIA ─────────────────────────────────
        private void TabApariencia()
        {
            var cfg = GetConfig(); int y = 0;
            PageHeader("Apariencia", "Personaliza el aspecto visual de la aplicación", "🎨", C_Violet, ref y);

            SectionLabel("TEMA DE COLOR", ref y);
            var cardTema = Card(ref y, 64 * 2 + 26);
            SettingRow(cardTema, "🌙", C_Blue, "Modo oscuro",    "Interfaz con fondo oscuro y colores suaves",      0, out _tglModoOscuro,   AppSettings.ModoOscuro);
            SettingRow(cardTema, "⚡", C_Gold, "Alto contraste", "Aumenta el contraste para mayor legibilidad",     1, out _tglAltoContraste, cfg.AltoContraste, isLast: true);

            _lblPreviewTema = new Label
            {
                Location = new Point(20, 128 + 6), Size = new Size(cardTema.Width - 40, 18),
                Font = new Font("Segoe UI", 8, FontStyle.Italic), BackColor = Color.Transparent
            };
            cardTema.Controls.Add(_lblPreviewTema);
            ActualizarPreviewTema();

            _tglModoOscuro.CheckedChanged    += (s, e) => { AppSettings.AplicarTema(_tglModoOscuro.Checked); TemaAplicado?.Invoke(this, EventArgs.Empty); ActualizarPreviewTema(); };
            _tglAltoContraste.CheckedChanged += (s, e) => { AppSettings.AplicarAltoContraste(_tglAltoContraste.Checked); TemaAplicado?.Invoke(this, EventArgs.Empty); };

            SectionLabel("TAMAÑO DE TEXTO", ref y);
            var cardFont = Card(ref y, 102);

            var iconF = new Panel { Size = new Size(38, 38), Location = new Point(18, 12), BackColor = Color.Transparent };
            iconF.Paint += (s, ev) => DrawIconBubble(ev.Graphics, iconF.ClientRectangle, "🔡", C_Cyan);
            cardFont.Controls.Add(iconF);
            cardFont.Controls.Add(new Label { Text = "Tamaño de fuente", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = C_Text, BackColor = Color.Transparent, Location = new Point(66, 12), AutoSize = true });
            cardFont.Controls.Add(new Label { Text = "Ajusta el tamaño del texto en toda la aplicación", Font = new Font("Segoe UI", 8.5f), ForeColor = C_Muted, BackColor = Color.Transparent, Location = new Point(66, 33), AutoSize = true });
            _lblFuenteVal = new Label { Text = cfg.TamanoFuente + "%", ForeColor = C_Blue, Font = new Font("Segoe UI", 14, FontStyle.Bold), BackColor = Color.Transparent, AutoSize = true, Location = new Point(cardFont.Width - 72, 16), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            cardFont.Controls.Add(_lblFuenteVal);
            _trackFuente = new TrackBar { Location = new Point(18, 58), Size = new Size(cardFont.Width - 100, 28), Minimum = 75, Maximum = 150, TickFrequency = 25, SmallChange = 5, LargeChange = 25, Value = cfg.TamanoFuente, BackColor = C_Card, TickStyle = TickStyle.None };
            _trackFuente.Scroll += (s, e) => _lblFuenteVal.Text = _trackFuente.Value + "%";
            cardFont.Controls.Add(_trackFuente);
            cardFont.Controls.Add(new Label { Text = "75%", ForeColor = C_Muted, Font = new Font("Segoe UI", 7.5f), BackColor = Color.Transparent, Location = new Point(18, 87), AutoSize = true });
            cardFont.Controls.Add(new Label { Text = "150%", ForeColor = C_Muted, Font = new Font("Segoe UI", 7.5f), BackColor = Color.Transparent, Location = new Point(cardFont.Width - 96, 87), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right });

            SectionLabel("IDIOMA Y MONEDA", ref y);
            var cardLang = Card(ref y, 138);

            // Idioma
            var iconLng = new Panel { Size = new Size(38, 38), Location = new Point(18, 16), BackColor = Color.Transparent };
            iconLng.Paint += (s, ev) => DrawIconBubble(ev.Graphics, iconLng.ClientRectangle, "🌐", C_Blue);
            cardLang.Controls.Add(iconLng);
            cardLang.Controls.Add(new Label { Text = "Idioma", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = C_Text, BackColor = Color.Transparent, Location = new Point(66, 14), AutoSize = true });
            _cmbIdioma = new ComboBox { Location = new Point(66, 36), Size = new Size(190, 26), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9), BackColor = C_Input, ForeColor = C_Text, FlatStyle = FlatStyle.Flat };
            _cmbIdioma.Items.AddRange(new object[] { "🇪🇸  Español", "🇬🇧  Inglés", "🏴  Catalán", "🇪🇸  Gallego", "🏳️  Euskera" });
            _cmbIdioma.SelectedItem = AppSettings.Idioma == "en" ? "🇬🇧  Inglés" : AppSettings.Idioma == "ca" ? "🏴  Catalán" : AppSettings.Idioma == "gl" ? "🇪🇸  Gallego" : AppSettings.Idioma == "eu" ? "🏳️  Euskera" : "🇪🇸  Español";
            cardLang.Controls.Add(_cmbIdioma);

            cardLang.Controls.Add(new Panel { Location = new Point(66, 70), Size = new Size(cardLang.Width - 86, 1), BackColor = C_Border });

            // Moneda
            var iconMon = new Panel { Size = new Size(38, 38), Location = new Point(18, 82), BackColor = Color.Transparent };
            iconMon.Paint += (s, ev) => DrawIconBubble(ev.Graphics, iconMon.ClientRectangle, "💱", C_Green);
            cardLang.Controls.Add(iconMon);
            cardLang.Controls.Add(new Label { Text = "Moneda", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = C_Text, BackColor = Color.Transparent, Location = new Point(66, 80), AutoSize = true });
            _cmbMoneda = new ComboBox { Location = new Point(66, 102), Size = new Size(190, 26), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9), BackColor = C_Input, ForeColor = C_Text, FlatStyle = FlatStyle.Flat };
            _cmbMoneda.Items.AddRange(new object[] { "€  Euro", "$  Dólar", "£  Libra" });
            _cmbMoneda.SelectedItem = cfg.MonedaPreferida == "USD" ? "$  Dólar" : cfg.MonedaPreferida == "GBP" ? "£  Libra" : "€  Euro";
            cardLang.Controls.Add(_cmbMoneda);

            SaveBar(ref y, () =>
            {
                cfg.ModoOscuro    = _tglModoOscuro.Checked;
                cfg.AltoContraste = _tglAltoContraste.Checked;
                cfg.TamanoFuente  = _trackFuente.Value;
                string id = _cmbIdioma.SelectedItem?.ToString() ?? "";
                cfg.Idioma = id.Contains("Inglés") ? "en" : id.Contains("Catalán") ? "ca" : id.Contains("Gallego") ? "gl" : id.Contains("Euskera") ? "eu" : "es";
                AppSettings.AplicarIdioma(cfg.Idioma);
                AppSettings.AplicarTema(cfg.ModoOscuro);
                TemaAplicado?.Invoke(this, EventArgs.Empty);
                cfg.MonedaPreferida = (_cmbMoneda.SelectedItem?.ToString() ?? "").Contains("Dólar") ? "USD"
                    : (_cmbMoneda.SelectedItem?.ToString() ?? "").Contains("Libra") ? "GBP" : "EUR";
                Guardar(cfg, "Apariencia guardada correctamente.");
            });
        }

        private void ActualizarPreviewTema()
        {
            if (_lblPreviewTema == null) return;
            bool osc = _tglModoOscuro?.Checked ?? AppSettings.ModoOscuro;
            _lblPreviewTema.Text      = osc ? "✓  Modo oscuro activo" : "✓  Modo claro activo";
            _lblPreviewTema.ForeColor = osc ? C_Blue : C_Gold;
        }

        // ── TAB 2: SEGURIDAD ──────────────────────────────────
        private void TabSeguridad()
        {
            var cfg = GetConfig();
            var usr = SesionActual.Instancia?.Usuario;
            int y = 0;
            PageHeader("Seguridad", "Protege tu cuenta y gestiona el acceso", "🛡️", C_Red, ref y);

            SectionLabel("CAMBIAR CONTRASEÑA", ref y);
            var cardPass = Card(ref y, 340);
            int pw = cardPass.Width - 40;

            cardPass.Controls.Add(FL("CONTRASEÑA ACTUAL", 20, 18));
            _txtPassActual = IBox(cardPass, new Point(20, 36), new Size(pw, 40), true);

            cardPass.Controls.Add(FL("NUEVA CONTRASEÑA", 20, 90));
            _txtPassNueva = IBox(cardPass, new Point(20, 108), new Size(pw, 40), true);

            // Barra de seguridad
            var barBg   = new Panel { Location = new Point(20, 154), Size = new Size(pw, 6), BackColor = C_Border };
            barBg.Paint += (s, ev) => { ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using (var p = RR(barBg.ClientRectangle, 3)) ev.Graphics.FillPath(new SolidBrush(C_Border), p); };
            var barFill = new Panel { Location = new Point(20, 154), Size = new Size(0, 6), BackColor = C_Green };
            barFill.Paint += (s, ev) => { if (barFill.Width < 4) return; ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using (var p = RR(barFill.ClientRectangle, 3)) ev.Graphics.FillPath(new SolidBrush(barFill.BackColor), p); };
            _lblPassStrength = new Label { Location = new Point(20, 164), Size = new Size(pw, 16), ForeColor = C_Muted, Font = new Font("Segoe UI", 7.5f), BackColor = Color.Transparent };
            cardPass.Controls.AddRange(new Control[] { barBg, barFill, _lblPassStrength });

            _txtPassNueva.TextChanged += (s, e) =>
            {
                int st = GetStrength(_txtPassNueva.Text);
                barFill.Width = (int)(pw * st / 5.0); barFill.Invalidate();
                Color[] cols = { C_Border, C_Red, Color.FromArgb(251, 146, 60), C_Gold, C_Blue, C_Green };
                barFill.BackColor = cols[st];
                string[] etiq = { "", "Muy débil", "Débil", "Aceptable", "Fuerte", "Muy fuerte" };
                _lblPassStrength.Text = st > 0 ? "Seguridad: " + etiq[st] : "";
                _lblPassStrength.ForeColor = st > 0 ? cols[st] : C_Muted;
            };

            cardPass.Controls.Add(FL("CONFIRMAR NUEVA CONTRASEÑA", 20, 186));
            _txtPassConfirm = IBox(cardPass, new Point(20, 204), new Size(pw, 40), true);
            _lblPassError = new Label { Location = new Point(20, 250), Size = new Size(pw, 18), ForeColor = C_Red, Font = new Font("Segoe UI", 8, FontStyle.Bold), Visible = false, BackColor = Color.Transparent };
            cardPass.Controls.Add(_lblPassError);

            var btnCambiar = MakeBtn("🔑  Actualizar contraseña", C_Blue, new Point(20, 278), new Size(220, 40));
            cardPass.Controls.Add(btnCambiar);
            btnCambiar.Click += (s, e) => CambiarContrasena();

            SectionLabel("AUTENTICACIÓN Y SESIÓN", ref y);
            var cardSec = Card(ref y, 64 * 2);
            SettingRow(cardSec, "🛡️", C_Red,  "PIN de seguridad (2FA)",  "Solicita un PIN al iniciar sesión como extra",  0, out _tgl2FA,       cfg.DosFactores);
            SettingRow(cardSec, "🔐", C_Blue, "Sesión segura extendida",  "Mantener sesión activa de forma segura",        1, out _tglSesionSeg, cfg.SesionSegura, isLast: true);
            _tgl2FA.CheckedChanged += (s, e) => Gestionar2FA(_tgl2FA.Checked);

            SectionLabel("TIEMPO DE SESIÓN", ref y);
            var cardTimer = Card(ref y, 66);
            var iconT = new Panel { Size = new Size(38, 38), Location = new Point(18, 14), BackColor = Color.Transparent };
            iconT.Paint += (s, ev) => DrawIconBubble(ev.Graphics, iconT.ClientRectangle, "⏱", C_Gold);
            cardTimer.Controls.Add(iconT);
            cardTimer.Controls.Add(new Label { Text = "Expiración de sesión", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = C_Text, BackColor = Color.Transparent, Location = new Point(66, 12), AutoSize = true });
            cardTimer.Controls.Add(new Label { Text = "La sesión se cerrará automáticamente por inactividad", Font = new Font("Segoe UI", 8.5f), ForeColor = C_Muted, BackColor = Color.Transparent, Location = new Point(66, 34), AutoSize = true });
            _numSesion = new NumericUpDown { Location = new Point(cardTimer.Width - 118, 13), Size = new Size(88, 38), Minimum = 5, Maximum = 120, Value = cfg.TiempoSesionMinutos, Font = new Font("Segoe UI", 13, FontStyle.Bold), BackColor = C_Input, ForeColor = C_Blue, BorderStyle = BorderStyle.None };
            cardTimer.Controls.Add(_numSesion);
            cardTimer.Controls.Add(new Label { Text = "min", Font = new Font("Segoe UI", 8f), ForeColor = C_Muted, BackColor = Color.Transparent, Location = new Point(cardTimer.Width - 26, 28), AutoSize = true });

            SectionLabel("INFORMACIÓN DE ACCESO", ref y);
            var cardInfo = Card(ref y, 62);
            InfoPar(cardInfo, "🕐", C_Muted, "Último acceso",  usr?.UltimoAcceso?.ToString("dd/MM/yyyy  HH:mm") ?? "—", 14);
            InfoPar(cardInfo, "📅", C_Muted, "Registrado el",  usr?.FechaRegistro.ToString("dd MMMM yyyy") ?? "—",       38);

            SaveBar(ref y, () =>
            {
                cfg.DosFactores         = _tgl2FA.Checked;
                cfg.SesionSegura        = _tglSesionSeg.Checked;
                cfg.TiempoSesionMinutos = (int)_numSesion.Value;
                Guardar(cfg, "Configuración de seguridad guardada.");
            });
        }

        // ── TAB 3: PREFERENCIAS ───────────────────────────────
        private void TabPreferencias()
        {
            var cfg = GetConfig(); int y = 0;
            PageHeader("Preferencias", "Ajusta el comportamiento de la aplicación", "⚙️", C_Gold, ref y);

            SectionLabel("COMPORTAMIENTO GENERAL", ref y);
            var card = Card(ref y, 64 * 4);
            SettingRow(card, "👁️", C_Blue,   "Mostrar saldo al inicio",  "El saldo se muestra visible al entrar",       0, out _tglMostrarSaldo, cfg.MostrarSaldoInicio);
            SettingRow(card, "📊", C_Violet, "Ordenar por saldo",         "Las cuentas con más saldo aparecen primero",  1, out _tglOrdenar,      cfg.OrdenarCuentasPorSaldo);
            SettingRow(card, "✅", C_Green,  "Confirmar transferencias",   "Muestra resumen antes de enviar dinero",      2, out _tglConfirmar,    cfg.ConfirmarTransferencias);
            SettingRow(card, "📋", C_Gold,   "Guardar beneficiarios",     "Recuerda los destinatarios frecuentes",       3, out _tglBenef,        cfg.GuardarBeneficiarios, isLast: true);

            SectionLabel("ALERTAS DE SALDO", ref y);
            var card2 = Card(ref y, 58 * 2);
            InfoRow(card2, "⚠️", C_Gold,  "Alerta de saldo bajo", "Aviso cuando el saldo sea inferior a 50 €",  0);
            InfoRow(card2, "💳", C_Muted, "Límite diario",         "Sin límite establecido — configurable",       1, isLast: true);

            SaveBar(ref y, () =>
            {
                cfg.MostrarSaldoInicio      = _tglMostrarSaldo.Checked;
                cfg.OrdenarCuentasPorSaldo  = _tglOrdenar.Checked;
                cfg.ConfirmarTransferencias = _tglConfirmar.Checked;
                cfg.GuardarBeneficiarios    = _tglBenef.Checked;
                Guardar(cfg, "Preferencias guardadas correctamente.");
            });
        }

        // ── TAB 4: MI CUENTA ──────────────────────────────────
        private void TabCuenta()
        {
            var usr = SesionActual.Instancia?.Usuario; int y = 0;
            PageHeader("Mi cuenta", "Información y estado de tu perfil Nexum", "👤", C_Green, ref y);

            SectionLabel("PERFIL", ref y);
            var card = Card(ref y, 224);
            string ini = (usr?.Nombre?.Length > 0 ? $"{usr.Nombre[0]}" : "?").ToUpper()
                       + (usr?.Apellidos?.Length > 0 ? $"{usr.Apellidos[0]}" : "").ToUpper();

            var av = new Panel { Size = new Size(72, 72), Location = new Point(22, 22), BackColor = Color.Transparent };
            av.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var shadow = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
                    ev.Graphics.FillEllipse(shadow, 3, 5, 68, 68);
                using (var b = new LinearGradientBrush(av.ClientRectangle, C_Blue, C_Violet, 135f))
                    ev.Graphics.FillEllipse(b, 0, 0, 70, 70);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString(ini, new Font("Segoe UI", 22, FontStyle.Bold), Brushes.White, new Rectangle(0, 0, 70, 70), fmt);
            };
            card.Controls.Add(av);

            card.Controls.Add(new Label { Text = usr?.NombreCompleto ?? "—", ForeColor = C_Text, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(108, 24), Size = new Size(card.Width - 126, 26), BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = usr?.Email ?? "—", ForeColor = C_Muted, Font = new Font("Segoe UI", 9.5f), Location = new Point(108, 52), AutoSize = true, BackColor = Color.Transparent });

            Color roleCol  = usr?.EsAdmin == true ? C_Gold : C_Green;
            string roleText = usr?.EsAdmin == true ? "● ADMINISTRADOR" : "● CLIENTE";
            var badge = new Panel { Size = new Size(120, 24), Location = new Point(108, 76), BackColor = Color.Transparent };
            badge.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(badge.ClientRectangle, 6))
                { ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(30, roleCol.R, roleCol.G, roleCol.B)), path); ev.Graphics.DrawPath(new Pen(Color.FromArgb(80, roleCol.R, roleCol.G, roleCol.B), 1), path); }
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString(roleText, new Font("Segoe UI", 7.5f, FontStyle.Bold), new SolidBrush(roleCol), badge.ClientRectangle, fmt);
            };
            card.Controls.Add(badge);

            card.Controls.Add(new Panel { Location = new Point(22, 112), Size = new Size(card.Width - 44, 1), BackColor = C_Border });
            InfoPar(card, "🆔", C_Muted, "ID de usuario",  "#" + (usr?.Id.ToString() ?? "—"),                                                 118);
            InfoPar(card, "✅", C_Green,  "Estado",        usr?.Activo == true ? "✓  Activa y operativa" : "⚠  Inactiva",                      144);
            InfoPar(card, "📅", C_Muted, "Miembro desde",  usr?.FechaRegistro.ToString("dd MMMM yyyy") ?? "—",                                 170);
            InfoPar(card, "🕐", C_Muted, "Último acceso",  usr?.UltimoAcceso?.ToString("dd/MM/yyyy  HH:mm") ?? "Nunca",                        196);

            SectionLabel("ZONA DE PELIGRO", ref y);
            var cardD = new Panel { Location = new Point(0, y), Size = new Size(Math.Max(400, _pnlContent.ClientSize.Width - 4), 86), BackColor = Color.Transparent };
            cardD.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(cardD.ClientRectangle, 14))
                { ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(AppSettings.ModoOscuro ? 18 : 8, C_Red.R, C_Red.G, C_Red.B)), path); ev.Graphics.DrawPath(new Pen(Color.FromArgb(60, C_Red.R, C_Red.G, C_Red.B), 1.5f), path); }
            };
            var iconD = new Panel { Size = new Size(38, 38), Location = new Point(18, 24), BackColor = Color.Transparent };
            iconD.Paint += (s, ev) => DrawIconBubble(ev.Graphics, iconD.ClientRectangle, "⚠️", C_Red);
            cardD.Controls.Add(iconD);
            cardD.Controls.Add(new Label { Text = "Cerrar sesión en todos los dispositivos", ForeColor = C_Red, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(66, 18), AutoSize = true, BackColor = Color.Transparent });
            cardD.Controls.Add(new Label { Text = "Invalida todos los tokens activos. Deberás iniciar sesión de nuevo.", ForeColor = C_Muted, Font = new Font("Segoe UI", 8.5f), Location = new Point(66, 42), AutoSize = true, BackColor = Color.Transparent });
            var btnD = MakeBtn("Cerrar todas las sesiones", C_Red, new Point(cardD.Width - 214, 23), new Size(196, 38));
            btnD.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnD.Click += (s, e) =>
            {
                if (MessageBox.Show("¿Cerrar todas las sesiones activas?", "Nexum Bank", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    MessageBox.Show("Sesiones cerradas. Inicia sesión de nuevo en tus dispositivos.", "Nexum Bank");
            };
            cardD.Controls.Add(btnD);
            _pnlContent.Controls.Add(cardD);
        }

        // ══════════════════════════════════════════════════════
        //  LÓGICA
        // ══════════════════════════════════════════════════════
        private ConfiguracionUsuario GetConfig()
        {
            if (SesionActual.Instancia.Configuracion == null)
                SesionActual.Instancia.Configuracion = new ConfiguracionUsuario { UsuarioId = SesionActual.Instancia.Usuario?.Id ?? 0 };
            return SesionActual.Instancia.Configuracion;
        }

        private void Guardar(ConfiguracionUsuario cfg, string msg)
        {
            SesionActual.Instancia.Configuracion = cfg;
            AppSettings.CargarDesdeConfiguracion(cfg);
            int uid  = SesionActual.Instancia?.Usuario?.Id ?? 0;
            bool ok  = uid > 0 && _cfgSvc.GuardarConfiguracion(cfg);
            ConfiguracionGuardada?.Invoke(this, EventArgs.Empty);
            MostrarToast(ok ? msg : msg + "\n(Sin conexión — cambios aplicados en sesión)");
        }

        private void MostrarToast(string mensaje)
        {
            var toast = new Panel { Size = new Size(Math.Min(400, _pnlContent.Width - 20), 46), BackColor = Color.Transparent };
            toast.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(toast.ClientRectangle, 10))
                using (var b = new LinearGradientBrush(toast.ClientRectangle, C_Green, Color.FromArgb(34, 197, 130), LinearGradientMode.Horizontal))
                    ev.Graphics.FillPath(b, path);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString("✓  " + mensaje, new Font("Segoe UI", 9, FontStyle.Bold), new SolidBrush(Color.FromArgb(4, 40, 25)), toast.ClientRectangle, fmt);
            };
            toast.Location = new Point(0, 0);
            _pnlContent.Controls.Add(toast);
            toast.BringToFront();
            var timer = new System.Windows.Forms.Timer { Interval = 2800 };
            timer.Tick += (s, e) => { timer.Stop(); if (!toast.IsDisposed) toast.Dispose(); };
            timer.Start();
        }

        private void Gestionar2FA(bool activar)
        {
            var cfg = GetConfig();
            int uid = SesionActual.Instancia?.Usuario?.Id ?? 0;
            if (uid <= 0) return;

            if (activar)
            {
                var dlg = new Form
                {
                    Text = "Nexum Bank — Configurar PIN de seguridad",
                    Size = new Size(380, 260), FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterScreen, MaximizeBox = false, MinimizeBox = false,
                    BackColor = C_Card
                };
                var lTit = new Label { Text = "🛡️  Configura tu PIN de 4 dígitos", ForeColor = C_Text, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 16), AutoSize = true };
                var lSub = new Label { Text = "Se pedirá al iniciar sesión como verificación extra.", ForeColor = C_Muted, Font = new Font("Segoe UI", 9), Location = new Point(20, 42), AutoSize = true };
                var lPin = new Label { Text = "PIN (4 dígitos)", ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(20, 76), AutoSize = true };
                var txtPin = new TextBox { Location = new Point(20, 94), Size = new Size(120, 28), MaxLength = 4, PasswordChar = '●', Font = new Font("Segoe UI", 13, FontStyle.Bold), BackColor = C_Input, ForeColor = C_Text, BorderStyle = BorderStyle.None };
                var lErr   = new Label { ForeColor = C_Red, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(20, 126), Size = new Size(320, 18), Visible = false };
                var btnOk  = new Button { Text = "Activar PIN", Location = new Point(20, 152), Size = new Size(130, 36), BackColor = C_Green, ForeColor = Color.FromArgb(4, 30, 20), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
                btnOk.FlatAppearance.BorderSize = 0;
                var btnCan = new Button { Text = "Cancelar", Location = new Point(162, 152), Size = new Size(100, 36), BackColor = C_Input, ForeColor = C_Muted, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10) };
                btnCan.FlatAppearance.BorderSize = 0;
                btnOk.Click += (s, e) =>
                {
                    string pin = txtPin.Text.Trim();
                    if (pin.Length != 4 || !System.Text.RegularExpressions.Regex.IsMatch(pin, @"^\d{4}$"))
                    { lErr.Text = "El PIN debe ser exactamente 4 dígitos numéricos."; lErr.Visible = true; return; }
                    string hash = BCrypt.Net.BCrypt.HashPassword(pin);
                    if (_cfgSvc.ActualizarCodigo2FA(uid, hash))
                    { cfg.DosFactores = true; cfg.CodigoVerificacion = hash; SesionActual.Instancia.Configuracion = cfg; dlg.DialogResult = DialogResult.OK; dlg.Close(); }
                    else { lErr.Text = "No se pudo guardar el PIN. Inténtalo de nuevo."; lErr.Visible = true; }
                };
                btnCan.Click += (s, e) => { dlg.DialogResult = DialogResult.Cancel; dlg.Close(); };
                dlg.Controls.AddRange(new Control[] { lTit, lSub, lPin, txtPin, lErr, btnOk, btnCan });
                if (dlg.ShowDialog() == DialogResult.OK)
                    MostrarToast("PIN de seguridad activado correctamente.");
                else
                    _tgl2FA.Checked = false;
            }
            else
            {
                if (MessageBox.Show("¿Seguro que quieres desactivar el PIN de seguridad?", "Nexum Bank", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _cfgSvc.Desactivar2FA(uid);
                    cfg.DosFactores = false; cfg.CodigoVerificacion = null;
                    SesionActual.Instancia.Configuracion = cfg;
                    MostrarToast("PIN de seguridad desactivado.");
                }
                else
                    _tgl2FA.Checked = true;
            }
        }

        private void CambiarContrasena()
        {
            if (_txtPassActual == null) return;
            if (_lblPassError != null) _lblPassError.Visible = false;
            string actual = _txtPassActual.Text, nueva = _txtPassNueva.Text, confirm = _txtPassConfirm.Text;
            if (string.IsNullOrWhiteSpace(actual))  { ErrPass("Introduce tu contraseña actual."); return; }
            if (string.IsNullOrWhiteSpace(nueva))   { ErrPass("Introduce la nueva contraseña."); return; }
            if (nueva.Length < 6)                   { ErrPass("La nueva contraseña debe tener al menos 6 caracteres."); return; }
            if (nueva != confirm)                   { ErrPass("Las contraseñas no coinciden."); return; }
            if (GetStrength(nueva) < 2)             { ErrPass("La contraseña es demasiado débil."); return; }
            var usr = SesionActual.Instancia?.Usuario;
            if (usr == null) return;
            if (_authSvc.CambiarContraseña(usr.Id, actual, nueva, out string err))
            {
                MessageBox.Show("Contraseña actualizada correctamente.", "Nexum Bank", MessageBoxButtons.OK);
                _txtPassActual.Text = _txtPassNueva.Text = _txtPassConfirm.Text = "";
            }
            else ErrPass(err ?? "Error al cambiar la contraseña.");
        }

        private void ErrPass(string msg)
        {
            if (_lblPassError != null) { _lblPassError.Text = "⚠  " + msg; _lblPassError.Visible = true; }
        }

        private static int GetStrength(string p)
        {
            if (string.IsNullOrEmpty(p)) return 0;
            int s = 0;
            if (p.Length >= 6)  s++;
            if (p.Length >= 10) s++;
            if (System.Text.RegularExpressions.Regex.IsMatch(p, "[A-Z]")) s++;
            if (System.Text.RegularExpressions.Regex.IsMatch(p, "[0-9]")) s++;
            if (System.Text.RegularExpressions.Regex.IsMatch(p, "[^a-zA-Z0-9]")) s++;
            return Math.Min(s, 5);
        }

        // ══════════════════════════════════════════════════════
        //  HELPERS UI
        // ══════════════════════════════════════════════════════

        /// <summary>Cabecera de página con icono en burbuja degradada y línea de acento.</summary>
        private void PageHeader(string title, string sub, string ico, Color accent, ref int y)
        {
            int W = Math.Max(400, _pnlContent.ClientSize.Width - 4);

            var iconH = new Panel { Size = new Size(46, 46), Location = new Point(0, y), BackColor = Color.Transparent };
            iconH.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(iconH.ClientRectangle, 12))
                using (var b = new LinearGradientBrush(iconH.ClientRectangle, accent,
                    Color.FromArgb(Math.Min(255, accent.R + 40), Math.Min(255, accent.G + 30), Math.Min(255, accent.B + 40)), 135f))
                    ev.Graphics.FillPath(b, path);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString(ico, new Font("Segoe UI", 20), Brushes.White, iconH.ClientRectangle, fmt);
            };
            _pnlContent.Controls.Add(iconH);
            _pnlContent.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = C_Text,  BackColor = Color.Transparent, Location = new Point(58, y + 2),  AutoSize = true });
            _pnlContent.Controls.Add(new Label { Text = sub,   Font = new Font("Segoe UI", 9),                  ForeColor = C_Muted, BackColor = Color.Transparent, Location = new Point(59, y + 28), AutoSize = true });
            y += 56;

            var div = new Panel { Location = new Point(0, y), Size = new Size(W, 2), BackColor = Color.Transparent };
            div.Paint += (s, ev) =>
            {
                using (var b = new LinearGradientBrush(div.ClientRectangle, accent, Color.FromArgb(0, accent), LinearGradientMode.Horizontal))
                    ev.Graphics.FillRectangle(b, div.ClientRectangle);
            };
            _pnlContent.Controls.Add(div);
            y += 20;
        }

        /// <summary>Etiqueta de sección en mayúsculas pequeñas.</summary>
        private void SectionLabel(string txt, ref int y)
        {
            _pnlContent.Controls.Add(new Label { Text = txt, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = C_Muted, BackColor = Color.Transparent, Location = new Point(2, y), AutoSize = true });
            y += 26;
        }

        /// <summary>Tarjeta redondeada con borde.</summary>
        private Panel Card(ref int y, int h)
        {
            var card = new Panel { Location = new Point(0, y), Size = new Size(Math.Max(400, _pnlContent.ClientSize.Width - 4), h), BackColor = C_Card };
            card.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(card.ClientRectangle, 14))
                { ev.Graphics.FillPath(new SolidBrush(C_Card), path); ev.Graphics.DrawPath(new Pen(C_Border, 1), path); }
            };
            _pnlContent.Controls.Add(card);
            y += h + 18;
            BeginInvoke(new Action(() => { try { card.Region = new Region(RR(card.ClientRectangle, 14)); } catch { } }));
            return card;
        }

        /// <summary>Fila de ajuste: burbuja icono + título + descripción + toggle.</summary>
        private void SettingRow(Panel card, string ico, Color accent, string title, string sub,
                                int rowIdx, out ToggleSwitch tgl, bool val, bool isLast = false)
        {
            const int ROW_H = 64;
            int yRow = rowIdx * ROW_H;

            var iconP = new Panel { Size = new Size(38, 38), Location = new Point(18, yRow + 13), BackColor = Color.Transparent };
            iconP.Paint += (s, ev) => DrawIconBubble(ev.Graphics, iconP.ClientRectangle, ico, accent);
            card.Controls.Add(iconP);
            card.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = C_Text,  BackColor = Color.Transparent, Location = new Point(68, yRow + 13), AutoSize = true });
            card.Controls.Add(new Label { Text = sub,   Font = new Font("Segoe UI", 8.5f),              ForeColor = C_Muted, BackColor = Color.Transparent, Location = new Point(68, yRow + 35), AutoSize = true });

            tgl = new ToggleSwitch(val);
            tgl.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            tgl.Location = new Point(card.Width - tgl.Width - 20, yRow + (ROW_H - tgl.Height) / 2);
            card.Controls.Add(tgl);

            if (!isLast)
                card.Controls.Add(new Panel { Location = new Point(68, yRow + ROW_H - 1), Size = new Size(card.Width - 88, 1), BackColor = C_Border });
        }

        /// <summary>Fila informativa: burbuja icono + título + descripción + punto de color.</summary>
        private void InfoRow(Panel card, string ico, Color accent, string title, string sub,
                             int rowIdx, bool isLast = false)
        {
            const int ROW_H = 58;
            int yRow = rowIdx * ROW_H;

            var iconP = new Panel { Size = new Size(36, 36), Location = new Point(18, yRow + 11), BackColor = Color.Transparent };
            iconP.Paint += (s, ev) => DrawIconBubble(ev.Graphics, iconP.ClientRectangle, ico, accent);
            card.Controls.Add(iconP);
            card.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = C_Text,  BackColor = Color.Transparent, Location = new Point(66, yRow + 9),  AutoSize = true });
            card.Controls.Add(new Label { Text = sub,   Font = new Font("Segoe UI", 8.5f),                ForeColor = C_Muted, BackColor = Color.Transparent, Location = new Point(66, yRow + 30), AutoSize = true });

            var dot = new Panel { Size = new Size(10, 10), BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            dot.Location = new Point(card.Width - 28, yRow + (ROW_H - 10) / 2);
            dot.Paint += (s, ev) => { ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias; ev.Graphics.FillEllipse(new SolidBrush(accent), dot.ClientRectangle); };
            card.Controls.Add(dot);

            if (!isLast)
                card.Controls.Add(new Panel { Location = new Point(66, yRow + ROW_H - 1), Size = new Size(card.Width - 86, 1), BackColor = C_Border });
        }

        /// <summary>Par clave-valor con icono pequeño.</summary>
        private void InfoPar(Panel card, string ico, Color accent, string label, string value, int yPos)
        {
            card.Controls.Add(new Label { Text = ico,   Font = new Font("Segoe UI", 11),                ForeColor = accent, BackColor = Color.Transparent, Location = new Point(22, yPos),      AutoSize = true });
            card.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = C_Muted, BackColor = Color.Transparent, Location = new Point(46, yPos + 3),  Size = new Size(112, 18) });
            card.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 9),                 ForeColor = C_Text,  BackColor = Color.Transparent, Location = new Point(162, yPos + 1), Size = new Size(card.Width - 180, 20) });
        }

        /// <summary>Barra inferior con botón guardar (degradado) y botón restablecer (ghost).</summary>
        private void SaveBar(ref int y, Action action)
        {
            // Botón guardar con degradado azul→violeta
            var btnSave = new Button
            {
                Size = new Size(200, 44), Location = new Point(0, y),
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                Text = "✓  Guardar cambios", Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White, BackColor = Color.Transparent
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(btnSave.ClientRectangle, 10))
                using (var b = new LinearGradientBrush(btnSave.ClientRectangle, C_Blue, C_Violet, LinearGradientMode.Horizontal))
                    ev.Graphics.FillPath(b, path);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString(btnSave.Text, btnSave.Font, Brushes.White, btnSave.ClientRectangle, fmt);
            };
            btnSave.Click += (s, e) => action();
            EventHandler rS = (s, e) => { try { btnSave.Region = new Region(RR(btnSave.ClientRectangle, 10)); } catch { } };
            btnSave.HandleCreated += rS; btnSave.Resize += rS;
            _pnlContent.Controls.Add(btnSave);

            // Botón restablecer ghost
            var btnReset = new Button
            {
                Size = new Size(148, 44), Location = new Point(208, y),
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                Text = "↺  Restablecer", Font = new Font("Segoe UI", 9.5f),
                ForeColor = C_Muted, BackColor = Color.Transparent
            };
            btnReset.FlatAppearance.BorderColor = C_Border;
            btnReset.FlatAppearance.BorderSize  = 1;
            btnReset.MouseEnter += (s, e) => btnReset.ForeColor = C_Text;
            btnReset.MouseLeave += (s, e) => btnReset.ForeColor = C_Muted;
            EventHandler rR = (s, e) => { try { btnReset.Region = new Region(RR(btnReset.ClientRectangle, 10)); } catch { } };
            btnReset.HandleCreated += rR; btnReset.Resize += rR;
            btnReset.Click += (s, e) =>
            {
                if (MessageBox.Show("¿Restablecer todos los ajustes por defecto?", "Nexum Bank", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var cfgDef = new ConfiguracionUsuario { UsuarioId = SesionActual.Instancia.Usuario?.Id ?? 0 };
                    SesionActual.Instancia.Configuracion = cfgDef;
                    AppSettings.CargarDesdeConfiguracion(cfgDef);
                    int uid = SesionActual.Instancia?.Usuario?.Id ?? 0;
                    if (uid > 0) _cfgSvc.GuardarConfiguracion(cfgDef);
                    TemaAplicado?.Invoke(this, EventArgs.Empty);
                    ConfiguracionGuardada?.Invoke(this, EventArgs.Empty);
                    CargarTab(_tabActiva);
                    MostrarToast("Configuración restablecida por defecto.");
                }
            };
            _pnlContent.Controls.Add(btnReset);
            y += 60;
        }

        /// <summary>Dibuja una burbuja redondeada de fondo traslúcido con un icono centrado.</summary>
        private void DrawIconBubble(Graphics g, Rectangle r, string ico, Color accent)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = RR(r, r.Width / 4))
                g.FillPath(new SolidBrush(Color.FromArgb(AppSettings.ModoOscuro ? 30 : 18, accent.R, accent.G, accent.B)), path);
            var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(ico, new Font("Segoe UI", r.Width > 34 ? 15 : 12), new SolidBrush(accent), r, fmt);
        }

        private TextBox IBox(Panel parent, Point loc, Size sz, bool password)
        {
            var pnl = new Panel { Location = loc, Size = sz, BackColor = C_Input };
            pnl.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(pnl.ClientRectangle, 9))
                { ev.Graphics.FillPath(new SolidBrush(C_Input), path); ev.Graphics.DrawPath(new Pen(C_Border, 1), path); }
            };
            var txt = new TextBox { Location = new Point(12, 8), Size = new Size(sz.Width - 24, sz.Height - 16), Font = new Font("Segoe UI", 10), BackColor = C_Input, ForeColor = C_Text, BorderStyle = BorderStyle.None };
            if (password) { txt.PasswordChar = '●'; txt.UseSystemPasswordChar = false; }
            pnl.Controls.Add(txt);
            parent.Controls.Add(pnl);
            BeginInvoke(new Action(() => { try { pnl.Region = new Region(RR(pnl.ClientRectangle, 9)); } catch { } }));
            return txt;
        }

        private Button MakeBtn(string text, Color col, Point loc, Size sz)
        {
            var btn = new Button { Text = text, Location = loc, Size = sz, BackColor = col, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(Math.Max(0, col.R - 20), Math.Max(0, col.G - 20), Math.Max(0, col.B - 20));
            btn.MouseLeave += (s, e) => btn.BackColor = col;
            EventHandler rnd = (s, e) => { try { btn.Region = new Region(RR(btn.ClientRectangle, 8)); } catch { } };
            btn.HandleCreated += rnd; btn.Resize += rnd;
            return btn;
        }

        private Label FL(string t, int x, int y) =>
            new Label { Text = t, Location = new Point(x, y), ForeColor = C_Muted, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };

        private static GraphicsPath RR(Rectangle r, int radius)
        {
            int d = radius * 2; var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseAllFigures(); return p;
        }
    }
}
