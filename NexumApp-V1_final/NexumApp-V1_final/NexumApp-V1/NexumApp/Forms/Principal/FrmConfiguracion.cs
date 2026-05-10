using NexumApp.Helpers;
using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NexumApp.Forms.Principal
{
    public partial class FrmConfiguracion : Form
    {
        // ── Servicios ──────────────────────────────────────────
        private readonly AuthService _authService = new AuthService();

        // ── Paleta fija (el propio formulario siempre es dark) ─
        private static readonly Color C_Bg      = Color.FromArgb(10,  12,  28);
        private static readonly Color C_Surface = Color.FromArgb(14,  17,  38);
        private static readonly Color C_Card    = Color.FromArgb(18,  22,  46);
        private static readonly Color C_Input   = Color.FromArgb(24,  29,  58);
        private static readonly Color C_Border  = Color.FromArgb(38,  44,  80);
        private static readonly Color C_Blue    = Color.FromArgb(99,  102, 241);
        private static readonly Color C_Violet  = Color.FromArgb(139, 92,  246);
        private static readonly Color C_Green   = Color.FromArgb(52,  211, 153);
        private static readonly Color C_Red     = Color.FromArgb(248, 113, 113);
        private static readonly Color C_Gold    = Color.FromArgb(251, 191, 36);
        private static readonly Color C_Text    = Color.FromArgb(241, 245, 249);
        private static readonly Color C_Muted   = Color.FromArgb(100, 116, 139);

        // ── Estado UI ─────────────────────────────────────────
        private Panel   _pnlContent;
        private Panel   _activeTabBtn;
        private int     _tabActiva = 0;

        // Refs a controles de cada tab
        private ToggleSwitch _tglEmail, _tglSMS, _tglPush, _tglMarketing;
        private ToggleSwitch _tglModoOscuro, _tglAltoContraste;
        private ToggleSwitch _tgl2FA, _tglSesionSeg, _tglMostrarSaldo, _tglOrdenar, _tglConfirmar, _tglBenef;
        private ComboBox     _cmbIdioma, _cmbMoneda;
        private TrackBar     _trackFuente;
        private Label        _lblFuenteVal;
        private NumericUpDown _numSesion;
        private TextBox      _txtPassActual, _txtPassNueva, _txtPassConfirm;
        private Label        _lblPassError, _lblPassStrength;
        private Label        _lblPreviewTema;

        public FrmConfiguracion()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            BuildUI();
            CargarTab(0);
        }

        // ══════════════════════════════════════════════════════
        //  SHELL
        // ══════════════════════════════════════════════════════
        private void BuildUI()
        {
            Controls.Clear();
            Text = "Configuración — Nexum Bank";
            Size = new Size(860, 640);
            MinimumSize = Size; MaximumSize = Size;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = C_Bg;

            Paint += (s, ev) =>
            {
                using (var b = new LinearGradientBrush(ClientRectangle,
                    Color.FromArgb(12, 14, 34), C_Bg, LinearGradientMode.Vertical))
                    ev.Graphics.FillRectangle(b, ClientRectangle);
            };

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
                BackColor = Color.Transparent, Padding = new Padding(0)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlp.Controls.Add(BuildSidebar(), 0, 0);
            _pnlContent = new Panel
            {
                Dock = DockStyle.Fill, BackColor = C_Bg,
                Padding = new Padding(28, 24, 28, 24), AutoScroll = true
            };
            tlp.Controls.Add(_pnlContent, 1, 0);
            Controls.Add(tlp);
        }

        // ── Sidebar ───────────────────────────────────────────
        private Panel BuildSidebar()
        {
            var sb = new Panel { Dock = DockStyle.Fill, BackColor = C_Surface };
            sb.Paint += (s, ev) =>
            {
                using (var pen = new Pen(C_Border, 1))
                    ev.Graphics.DrawLine(pen, sb.Width - 1, 0, sb.Width - 1, sb.Height);
            };

            var flp = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, BackColor = Color.Transparent,
                Padding = new Padding(12, 16, 12, 12)
            };
            flp.SizeChanged += (s, e) =>
            {
                foreach (Control c in flp.Controls)
                    c.Width = flp.ClientSize.Width - flp.Padding.Horizontal;
            };

            // Avatar + usuario
            var usr = SesionActual.Instancia?.Usuario;
            string ini = usr != null
                ? (usr.Nombre?.Length > 0 ? "" + usr.Nombre[0] : "?")
                  + (usr.Apellidos?.Length > 0 ? "" + usr.Apellidos[0] : "")
                : "?";

            var pnlUser = new Panel { Height = 78, BackColor = Color.Transparent, Margin = new Padding(0, 0, 0, 10) };
            var av = new Panel { Size = new Size(44, 44), Location = new Point(0, 18) };
            av.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(av.ClientRectangle, 12))
                using (var b = new LinearGradientBrush(av.ClientRectangle, C_Blue, C_Violet, 135f))
                    ev.Graphics.FillPath(b, path);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString(ini.ToUpper(), new Font("Segoe UI", 14, FontStyle.Bold), Brushes.White, av.ClientRectangle, fmt);
            };
            pnlUser.Controls.Add(av);
            pnlUser.Controls.Add(new Label { Text = usr?.NombreCompleto ?? "Usuario", ForeColor = C_Text, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(52, 20), Size = new Size(130, 16), AutoEllipsis = true });
            pnlUser.Controls.Add(new Label { Text = usr?.Email ?? "", ForeColor = C_Muted, Font = new Font("Segoe UI", 8), Location = new Point(52, 38), Size = new Size(130, 14), AutoEllipsis = true });
            flp.Controls.Add(pnlUser);
            flp.Controls.Add(new Panel { Height = 1, BackColor = C_Border, Margin = new Padding(0, 0, 0, 10) });

            string[] tabs = {
                "🔔   Notificaciones",
                "🎨   Apariencia",
                "🔒   Seguridad",
                "⚙️   Preferencias",
                "ℹ️   Mi cuenta"
            };

            for (int i = 0; i < tabs.Length; i++)
            {
                var btn = MakeTabBtn(tabs[i], i);
                flp.Controls.Add(btn);
                if (i == 0) { _activeTabBtn = btn; btn.Invalidate(); }
            }

            // Separador + botón cerrar
            flp.Controls.Add(new Panel { Height = 1, BackColor = C_Border, Margin = new Padding(0, 8, 0, 8) });
            var btnCerrar = new Panel { Height = 38, BackColor = Color.Transparent, Cursor = Cursors.Hand, Margin = new Padding(0) };
            btnCerrar.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                bool hov = btnCerrar.ClientRectangle.Contains(btnCerrar.PointToClient(Cursor.Position));
                if (hov) using (var path = RR(btnCerrar.ClientRectangle, 8))
                    ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(20, C_Red.R, C_Red.G, C_Red.B)), path);
                ev.Graphics.DrawString("✕   Cerrar", new Font("Segoe UI", 10), new SolidBrush(hov ? C_Red : C_Muted),
                    new RectangleF(12, 0, btnCerrar.Width, btnCerrar.Height), new StringFormat { LineAlignment = StringAlignment.Center });
            };
            btnCerrar.MouseEnter += (s, e) => btnCerrar.Invalidate();
            btnCerrar.MouseLeave += (s, e) => btnCerrar.Invalidate();
            btnCerrar.Click += (s, e) => Close();
            flp.Controls.Add(btnCerrar);

            sb.Controls.Add(flp);
            return sb;
        }

        private Panel MakeTabBtn(string texto, int idx)
        {
            var btn = new Panel
            {
                Height = 40, BackColor = Color.Transparent,
                Cursor = Cursors.Hand, Margin = new Padding(0, 0, 0, 2), Tag = idx
            };
            btn.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                bool activo = btn == _activeTabBtn;
                bool hov = btn.ClientRectangle.Contains(btn.PointToClient(Cursor.Position));
                if (activo)
                {
                    using (var path = RR(btn.ClientRectangle, 8))
                    using (var b = new LinearGradientBrush(btn.ClientRectangle,
                        Color.FromArgb(40, C_Blue.R, C_Blue.G, C_Blue.B),
                        Color.FromArgb(15, C_Violet.R, C_Violet.G, C_Violet.B),
                        LinearGradientMode.Horizontal))
                        ev.Graphics.FillPath(b, path);
                    ev.Graphics.FillRectangle(new SolidBrush(C_Blue), new Rectangle(0, 8, 3, btn.Height - 16));
                }
                else if (hov)
                {
                    using (var path = RR(btn.ClientRectangle, 8))
                        ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(12, 255, 255, 255)), path);
                }
                ev.Graphics.DrawString(texto, new Font("Segoe UI", 10, activo ? FontStyle.Bold : FontStyle.Regular),
                    new SolidBrush(activo ? C_Text : C_Muted),
                    new RectangleF(14, 0, btn.Width - 14, btn.Height),
                    new StringFormat { LineAlignment = StringAlignment.Center });
            };
            btn.MouseEnter += (s, e) => btn.Invalidate();
            btn.MouseLeave += (s, e) => btn.Invalidate();
            btn.Click += (s, e) =>
            {
                _activeTabBtn?.Invalidate(); _activeTabBtn = btn; btn.Invalidate();
                CargarTab((int)btn.Tag);
            };
            return btn;
        }

        // ══════════════════════════════════════════════════════
        //  TABS
        // ══════════════════════════════════════════════════════
        private void CargarTab(int idx)
        {
            _tabActiva = idx; _pnlContent.Controls.Clear();
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
            Titulo("Notificaciones", "Controla cómo y cuándo te avisamos", ref y);

            var card = Card(ref y, 230);
            _tglEmail     = Toggle(card, "📧   Email",                 "Recibe alertas de actividad en tu correo",       20,  cfg.NotificacionesEmail);
            _tglSMS       = Toggle(card, "📱   SMS",                   "Mensajes de texto para movimientos importantes",  80, cfg.NotificacionesSMS);
            _tglPush      = Toggle(card, "🔔   Push",                  "Alertas instantáneas en este dispositivo",       140, cfg.NotificacionesPush);
            _tglMarketing = Toggle(card, "📣   Comunicaciones",        "Ofertas, novedades y promociones de Nexum",      200, cfg.NotificacionesMarketing);

            Titulo2("Tipo de alertas activas", ref y);
            var card2 = Card(ref y, 130);
            InfoFila(card2, "💸  Movimientos de cuenta",  "Ingresos, retiros y cargos",      20, C_Green);
            InfoFila(card2, "✈  Transferencias enviadas",  "Confirmaciones de envío",          70, C_Green);
            InfoFila(card2, "🔐  Alertas de seguridad",   "Nuevos accesos y cambios clave",  110, C_Gold);

            BtnGuardar(ref y, () =>
            {
                cfg.NotificacionesEmail    = _tglEmail.Checked;
                cfg.NotificacionesSMS      = _tglSMS.Checked;
                cfg.NotificacionesPush     = _tglPush.Checked;
                cfg.NotificacionesMarketing= _tglMarketing.Checked;
                Guardar(cfg, "Notificaciones guardadas correctamente.");
            });
        }

        // ── TAB 1: APARIENCIA ─────────────────────────────────
        private void TabApariencia()
        {
            var cfg = GetConfig(); int y = 0;
            Titulo("Apariencia", "Personaliza el aspecto visual de la aplicación", ref y);

            // TEMA
            Titulo2("Tema de color", ref y);
            var cardTema = Card(ref y, 140);
            _tglModoOscuro = Toggle(cardTema, "🌙   Modo oscuro",
                "Interfaz con fondo oscuro — activo en esta sesión", 20, AppSettings.ModoOscuro);
            _tglModoOscuro.CheckedChanged += (s, e) =>
            {
                AppSettings.AplicarTema(_tglModoOscuro.Checked);
                ActualizarPreviewTema();
            };
            _tglAltoContraste = Toggle(cardTema, "⚡   Alto contraste",
                "Aumenta el contraste de textos y bordes", 80, cfg.AltoContraste);

            // Preview de tema
            _lblPreviewTema = new Label
            {
                Location = new Point(20, 118), Size = new Size(370, 16),
                Font = new Font("Segoe UI", 8, FontStyle.Italic), AutoSize = false
            };
            cardTema.Controls.Add(_lblPreviewTema);
            ActualizarPreviewTema();

            // FUENTE
            Titulo2("Tamaño de texto", ref y);
            var cardFont = Card(ref y, 76);
            cardFont.Controls.Add(FL("TAMAÑO DE FUENTE (75% — 150%)", 20, 14));
            _trackFuente = new TrackBar
            {
                Location = new Point(20, 38), Size = new Size(300, 28),
                Minimum = 75, Maximum = 150, TickFrequency = 25,
                SmallChange = 5, LargeChange = 25,
                Value = cfg.TamanoFuente, BackColor = C_Card, TickStyle = TickStyle.None
            };
            _lblFuenteVal = new Label
            {
                Text = cfg.TamanoFuente + "%",
                ForeColor = C_Blue, Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Location = new Point(330, 34), AutoSize = true
            };
            _trackFuente.Scroll += (s, e) => _lblFuenteVal.Text = _trackFuente.Value + "%";
            cardFont.Controls.Add(_trackFuente);
            cardFont.Controls.Add(_lblFuenteVal);

            // IDIOMA + MONEDA
            Titulo2("Idioma y región", ref y);
            var cardLang = Card(ref y, 100);
            cardLang.Controls.Add(FL("IDIOMA DE LA APLICACIÓN", 20, 14));
            _cmbIdioma = Combo(cardLang, new Point(20, 36), 200);
            _cmbIdioma.Items.AddRange(new object[] { "🇪🇸  Español", "🇬🇧  Inglés", "🏴  Catalán", "🇪🇸  Gallego", "🏳️  Euskera" });
            string idSel = AppSettings.Idioma == "en" ? "🇬🇧  Inglés"
                         : AppSettings.Idioma == "ca" ? "🏴  Catalán"
                         : AppSettings.Idioma == "gl" ? "🇪🇸  Gallego"
                         : AppSettings.Idioma == "eu" ? "🏳️  Euskera"
                         : "🇪🇸  Español";
            _cmbIdioma.SelectedItem = idSel;
            cardLang.Controls.Add(FL("MONEDA", 240, 14));
            _cmbMoneda = Combo(cardLang, new Point(240, 36), 150);
            _cmbMoneda.Items.AddRange(new object[] { "€  Euro", "$  Dólar", "£  Libra" });
            _cmbMoneda.SelectedItem = cfg.MonedaPreferida == "USD" ? "$  Dólar" : cfg.MonedaPreferida == "GBP" ? "£  Libra" : "€  Euro";

            // Nota idioma
            cardLang.Controls.Add(new Label
            {
                Text = "ℹ️  El cambio de idioma aplica los textos principales al reiniciar la vista.",
                ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Italic),
                Location = new Point(20, 72), Size = new Size(370, 16)
            });

            BtnGuardar(ref y, () =>
            {
                cfg.ModoOscuro      = _tglModoOscuro.Checked;
                cfg.AltoContraste   = _tglAltoContraste.Checked;
                cfg.TamanoFuente    = _trackFuente.Value;

                string id = _cmbIdioma.SelectedItem?.ToString() ?? "";
                string codigo = id.Contains("Inglés") ? "en" : id.Contains("Catalán") ? "ca" : id.Contains("Gallego") ? "gl" : id.Contains("Euskera") ? "eu" : "es";
                cfg.Idioma = codigo;
                AppSettings.AplicarIdioma(codigo);
                AppSettings.AplicarTema(_tglModoOscuro.Checked);

                string mn = _cmbMoneda.SelectedItem?.ToString() ?? "";
                cfg.MonedaPreferida = mn.Contains("Dólar") ? "USD" : mn.Contains("Libra") ? "GBP" : "EUR";

                Guardar(cfg, "Apariencia guardada.\n\nEl modo oscuro/claro y el idioma se aplicarán en las nuevas vistas que abras.");
            });
        }

        private void ActualizarPreviewTema()
        {
            if (_lblPreviewTema == null) return;
            bool oscuro = _tglModoOscuro?.Checked ?? AppSettings.ModoOscuro;
            _lblPreviewTema.Text   = oscuro ? "✓  Modo oscuro activo — Fondo #0A0C1C" : "✓  Modo claro activo — Fondo #F4F7FE";
            _lblPreviewTema.ForeColor = oscuro ? C_Blue : C_Gold;
        }

        // ── TAB 2: SEGURIDAD ──────────────────────────────────
        private void TabSeguridad()
        {
            var cfg = GetConfig();
            var usr = SesionActual.Instancia?.Usuario;
            int y = 0;
            Titulo("Seguridad", "Protege tu cuenta y gestiona el acceso", ref y);

            // Cambio de contraseña
            Titulo2("Cambiar contraseña", ref y);
            var cardPass = Card(ref y, 248);

            cardPass.Controls.Add(FL("CONTRASEÑA ACTUAL", 20, 14));
            _txtPassActual = IBox(cardPass, new Point(20, 34), new Size(380, 36), true);
            cardPass.Controls.Add(FL("NUEVA CONTRASEÑA", 20, 82));
            _txtPassNueva = IBox(cardPass, new Point(20, 102), new Size(380, 36), true);
            _txtPassNueva.TextChanged += (s, e) => RefStrength();

            // Barra de fortaleza
            var barraFuerza = new Panel { Location = new Point(20, 142), Size = new Size(380, 6), BackColor = C_Border };
            var barraFill = new Panel { Location = new Point(0, 0), Size = new Size(0, 6), BackColor = C_Green };
            barraFuerza.Controls.Add(barraFill);
            barraFuerza.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(barraFuerza.ClientRectangle, 3))
                    ev.Graphics.FillPath(new SolidBrush(C_Border), path);
            };
            _lblPassStrength = new Label { Location = new Point(20, 152), Size = new Size(380, 14), ForeColor = C_Muted, Font = new Font("Segoe UI", 8) };
            cardPass.Controls.Add(barraFuerza); cardPass.Controls.Add(_lblPassStrength);

            _txtPassNueva.TextChanged += (s, e) =>
            {
                int strength = GetStrength(_txtPassNueva.Text);
                int w = (int)(380 * strength / 5.0);
                Color[] cols = { C_Border, C_Red, Color.FromArgb(251, 146, 60), C_Gold, C_Blue, C_Green };
                barraFill.Width = w; barraFill.BackColor = cols[strength];
            };

            cardPass.Controls.Add(FL("CONFIRMAR NUEVA CONTRASEÑA", 20, 170));
            _txtPassConfirm = IBox(cardPass, new Point(20, 190), new Size(380, 36), true);
            _lblPassError = new Label { Location = new Point(20, 230), Size = new Size(380, 16), ForeColor = C_Red, Font = new Font("Segoe UI", 8, FontStyle.Bold), Visible = false };
            cardPass.Controls.Add(_lblPassError);

            var btnCambiar = MakeBtn("🔑  Actualizar contraseña", C_Blue, new Point(0, y), new Size(220, 40));
            btnCambiar.Click += (s, e) => CambiarContrasena();
            _pnlContent.Controls.Add(btnCambiar); y += 50;

            // Sesión y 2FA
            Titulo2("Autenticación y sesión", ref y);
            var cardSec = Card(ref y, 130);
            _tgl2FA       = Toggle(cardSec, "🛡️   Verificación en dos pasos",  "Código extra al iniciar sesión (2FA)",       20, cfg.DosFactores);
            _tglSesionSeg = Toggle(cardSec, "🔐   Sesión segura extendida",    "Mantener sesión activa de forma segura",      80, cfg.SesionSegura);

            var cardTimer = Card(ref y, 70);
            cardTimer.Controls.Add(FL("EXPIRACIÓN DE SESIÓN (MINUTOS)", 20, 12));
            _numSesion = new NumericUpDown
            {
                Location = new Point(20, 34), Size = new Size(90, 30),
                Minimum = 5, Maximum = 120, Value = cfg.TiempoSesionMinutos,
                Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = C_Input, ForeColor = C_Blue, BorderStyle = BorderStyle.None
            };
            cardTimer.Controls.Add(_numSesion);
            cardTimer.Controls.Add(new Label { Text = "La sesión se cerrará automáticamente por inactividad tras este tiempo.", ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Italic), Location = new Point(120, 40), Size = new Size(260, 18) });

            // Info de cuenta
            Titulo2("Información de acceso", ref y);
            var cardInfo = Card(ref y, 70);
            Par(cardInfo, "Último acceso", usr?.UltimoAcceso?.ToString("dd/MM/yyyy  HH:mm") ?? "—", 14);
            Par(cardInfo, "Registrado el", usr?.FechaRegistro.ToString("dd MMMM yyyy") ?? "—", 42);

            BtnGuardar(ref y, () =>
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
            Titulo("Preferencias", "Ajusta el comportamiento de la aplicación", ref y);

            Titulo2("Comportamiento general", ref y);
            var card = Card(ref y, 230);
            _tglMostrarSaldo = Toggle(card, "👁️   Mostrar saldo al inicio",    "El saldo se muestra visible al entrar",         20, cfg.MostrarSaldoInicio);
            _tglOrdenar      = Toggle(card, "📊   Ordenar cuentas por saldo",  "Las cuentas con más saldo aparecen primero",    80, cfg.OrdenarCuentasPorSaldo);
            _tglConfirmar    = Toggle(card, "✅   Confirmar transferencias",    "Muestra resumen antes de enviar dinero",        140, cfg.ConfirmarTransferencias);
            _tglBenef        = Toggle(card, "📋   Guardar beneficiarios",      "Recuerda los destinatarios frecuentes",         200, cfg.GuardarBeneficiarios);

            Titulo2("Límites y alertas de saldo", ref y);
            var card2 = Card(ref y, 80);
            InfoFila(card2, "⚠️  Alerta de saldo bajo", "Aviso cuando el saldo sea inferior a 50 €", 14, C_Gold);
            InfoFila(card2, "💳  Límite diario",         "Sin límite establecido — configurable",     54, C_Muted);

            BtnGuardar(ref y, () =>
            {
                cfg.MostrarSaldoInicio      = _tglMostrarSaldo.Checked;
                cfg.OrdenarCuentasPorSaldo  = _tglOrdenar.Checked;
                cfg.ConfirmarTransferencias = _tglConfirmar.Checked;
                cfg.GuardarBeneficiarios    = _tglBenef.Checked;
                Guardar(cfg, "Preferencias guardadas correctamente.");
            });
        }

        // ── TAB 4: CUENTA ─────────────────────────────────────
        private void TabCuenta()
        {
            var usr = SesionActual.Instancia?.Usuario; int y = 0;
            Titulo("Mi cuenta", "Información y estado de tu perfil Nexum", ref y);

            var card = Card(ref y, 196);
            string ini = usr != null
                ? (usr.Nombre?.Length > 0 ? "" + usr.Nombre[0] : "?")
                  + (usr.Apellidos?.Length > 0 ? "" + usr.Apellidos[0] : "")
                : "?";

            var av = new Panel { Size = new Size(60, 60), Location = new Point(20, 20) };
            av.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(av.ClientRectangle, 15))
                using (var b = new LinearGradientBrush(av.ClientRectangle, C_Blue, C_Violet, 135f))
                    ev.Graphics.FillPath(b, path);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString(ini.ToUpper(), new Font("Segoe UI", 18, FontStyle.Bold), Brushes.White, av.ClientRectangle, fmt);
            };
            card.Controls.Add(av);
            card.Controls.Add(new Label { Text = usr?.NombreCompleto ?? "—", ForeColor = C_Text, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(96, 22), Size = new Size(300, 24) });
            card.Controls.Add(new Label { Text = usr?.Email ?? "—", ForeColor = C_Muted, Font = new Font("Segoe UI", 10), Location = new Point(96, 48), AutoSize = true });

            var badge = new Panel { Size = new Size(100, 22), Location = new Point(96, 70) };
            badge.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color col = usr?.EsAdmin == true ? C_Gold : C_Green;
                using (var path = RR(badge.ClientRectangle, 6))
                    ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(25, col.R, col.G, col.B)), path);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString(usr?.EsAdmin == true ? "● ADMINISTRADOR" : "● CLIENTE", new Font("Segoe UI", 7, FontStyle.Bold), new SolidBrush(col), badge.ClientRectangle, fmt);
            };
            card.Controls.Add(badge);

            Par(card, "ID de usuario",      "#" + (usr?.Id.ToString() ?? "—"),                              102);
            Par(card, "Estado de cuenta",   usr?.Activo == true ? "✓  Activa y operativa" : "⚠  Inactiva",   126);
            Par(card, "Miembro desde",      usr?.FechaRegistro.ToString("dd MMMM yyyy") ?? "—",              150);
            Par(card, "Último acceso",      usr?.UltimoAcceso?.ToString("dd/MM/yyyy  HH:mm") ?? "Nunca",     174);

            // Zona de peligro
            Titulo2("Zona de peligro", ref y);
            var cardD = new Panel { Location = new Point(0, y), Size = new Size(_pnlContent.ClientSize.Width - 56, 92), BackColor = C_Card };
            cardD.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(cardD.ClientRectangle, 12))
                { ev.Graphics.FillPath(new SolidBrush(C_Card), path); ev.Graphics.DrawPath(new Pen(Color.FromArgb(70, C_Red.R, C_Red.G, C_Red.B), 1), path); }
            };
            cardD.Controls.Add(new Label { Text = "⚠  Cerrar sesión en todos los dispositivos", ForeColor = C_Red, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, 14), AutoSize = true });
            cardD.Controls.Add(new Label { Text = "Invalida todos los tokens activos. Necesitarás iniciar sesión de nuevo.", ForeColor = C_Muted, Font = new Font("Segoe UI", 8), Location = new Point(20, 36), AutoSize = true });
            var btnD = MakeBtn("Cerrar todas las sesiones", C_Red, new Point(20, 58), new Size(200, 28));
            btnD.Click += (s, e) =>
            {
                if (MessageBox.Show("¿Cerrar todas las sesiones activas?", "Nexum Bank", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    MessageBox.Show("Sesiones cerradas. Inicia sesión de nuevo en tus dispositivos.", "Nexum Bank");
            };
            cardD.Controls.Add(btnD);
            _pnlContent.Controls.Add(cardD);
            BeginInvoke(new Action(() => Redondear(cardD, 12)));
        }

        // ══════════════════════════════════════════════════════
        //  LÓGICA
        // ══════════════════════════════════════════════════════
        private ConfiguracionUsuario GetConfig()
        {
            if (SesionActual.Instancia.Configuracion == null)
                SesionActual.Instancia.Configuracion = new ConfiguracionUsuario
                    { UsuarioId = SesionActual.Instancia.Usuario?.Id ?? 0 };
            return SesionActual.Instancia.Configuracion;
        }

        private void Guardar(ConfiguracionUsuario cfg, string msg)
        {
            SesionActual.Instancia.Configuracion = cfg;
            MessageBox.Show(msg, "Nexum Bank — Configuración", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        private void CambiarContrasena()
        {
            if (_txtPassActual == null) return;
            _lblPassError.Visible = false;
            string actual  = _txtPassActual.Text;
            string nueva   = _txtPassNueva.Text;
            string confirm = _txtPassConfirm.Text;

            if (string.IsNullOrWhiteSpace(actual))    { ErrPass("Introduce tu contraseña actual."); return; }
            if (string.IsNullOrWhiteSpace(nueva))     { ErrPass("Introduce la nueva contraseña."); return; }
            if (nueva.Length < 6)                     { ErrPass("La nueva contraseña debe tener al menos 6 caracteres."); return; }
            if (nueva != confirm)                     { ErrPass("Las contraseñas no coinciden."); return; }
            if (GetStrength(nueva) < 2)               { ErrPass("La contraseña es demasiado débil. Usa letras, números y símbolos."); return; }

            var usr = SesionActual.Instancia?.Usuario;
            if (usr == null) return;

            if (_authService.CambiarContraseña(usr.Id, actual, nueva, out string err))
            {
                MessageBox.Show("Contraseña actualizada correctamente.", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.None);
                _txtPassActual.Text = _txtPassNueva.Text = _txtPassConfirm.Text = "";
            }
            else ErrPass(err ?? "Error al cambiar la contraseña.");
        }

        private void ErrPass(string msg) { if (_lblPassError != null) { _lblPassError.Text = "⚠  " + msg; _lblPassError.Visible = true; } }

        private void RefStrength()
        {
            if (_lblPassStrength == null || _txtPassNueva == null) return;
            int s = GetStrength(_txtPassNueva.Text);
            string[] etiq = { "", "Muy débil", "Débil", "Aceptable", "Fuerte", "Muy fuerte" };
            Color[]  cols = { C_Muted, C_Red, Color.FromArgb(251, 146, 60), C_Gold, C_Blue, C_Green };
            _lblPassStrength.Text      = s > 0 ? "Seguridad: " + etiq[s] : "";
            _lblPassStrength.ForeColor = s > 0 ? cols[s] : C_Muted;
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

        // Handlers requeridos por el Designer
        private void BtnGuardar_Click(object sender, EventArgs e) { }
        private void BtnCancelar_Click(object sender, EventArgs e) => Close();
        private void BtnRestablecer_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Restablecer todos los ajustes por defecto?", "Nexum Bank", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var cfg = new ConfiguracionUsuario { UsuarioId = SesionActual.Instancia.Usuario?.Id ?? 0 };
                SesionActual.Instancia.Configuracion = cfg;
                AppSettings.AplicarTema(true);
                AppSettings.AplicarIdioma("es");
                CargarTab(_tabActiva);
                MessageBox.Show("Valores restablecidos por defecto.", "Nexum Bank");
            }
        }
        private void TrackTamanoFuente_Scroll(object sender, EventArgs e)
        {
            if (_lblFuenteVal != null && _trackFuente != null) _lblFuenteVal.Text = _trackFuente.Value + "%";
        }

        // ══════════════════════════════════════════════════════
        //  HELPERS UI
        // ══════════════════════════════════════════════════════
        private void Titulo(string t, string s, ref int y)
        {
            _pnlContent.Controls.Add(new Label { Text = t, ForeColor = C_Text, Font = new Font("Segoe UI", 17, FontStyle.Bold), Location = new Point(0, y), AutoSize = true }); y += 30;
            _pnlContent.Controls.Add(new Label { Text = s, ForeColor = C_Muted, Font = new Font("Segoe UI", 9), Location = new Point(0, y), AutoSize = true }); y += 26;
            _pnlContent.Controls.Add(new Panel { Location = new Point(0, y), Height = 1, Width = 1200, BackColor = C_Border }); y += 14;
        }

        private void Titulo2(string t, ref int y)
        {
            _pnlContent.Controls.Add(new Label { Text = t, ForeColor = C_Text, Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(0, y), AutoSize = true }); y += 26;
        }

        private Panel Card(ref int y, int h)
        {
            var card = new Panel { Location = new Point(0, y), Size = new Size(_pnlContent.ClientSize.Width - 56, h), BackColor = C_Card };
            card.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(card.ClientRectangle, 12))
                { ev.Graphics.FillPath(new SolidBrush(C_Card), path); ev.Graphics.DrawPath(new Pen(C_Border, 1), path); }
            };
            _pnlContent.Controls.Add(card);
            y += h + 16;
            BeginInvoke(new Action(() => Redondear(card, 12)));
            return card;
        }

        private ToggleSwitch Toggle(Panel parent, string title, string sub, int y, bool val)
        {
            parent.Controls.Add(new Label { Text = title, ForeColor = C_Text, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, y + 4), AutoSize = true });
            parent.Controls.Add(new Label { Text = sub, ForeColor = C_Muted, Font = new Font("Segoe UI", 8), Location = new Point(20, y + 24), AutoSize = true });
            var tgl = new ToggleSwitch(val) { Location = new Point(parent.Width - 70, y + 12) };
            parent.Controls.Add(tgl);
            return tgl;
        }

        private void InfoFila(Panel parent, string title, string sub, int y, Color col)
        {
            parent.Controls.Add(new Label { Text = title, ForeColor = C_Text, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(20, y), AutoSize = true });
            parent.Controls.Add(new Label { Text = sub, ForeColor = C_Muted, Font = new Font("Segoe UI", 8), Location = new Point(20, y + 18), AutoSize = true });
            var dot = new Panel { Size = new Size(8, 8), Location = new Point(parent.Width - 28, y + 6) };
            dot.Paint += (s, ev) => { ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias; ev.Graphics.FillEllipse(new SolidBrush(col), dot.ClientRectangle); };
            parent.Controls.Add(dot);
        }

        private void BtnGuardar(ref int y, Action accion)
        {
            var btn = MakeBtn("✓  Guardar cambios", C_Green, new Point(0, y), new Size(190, 40));
            btn.ForeColor = Color.FromArgb(4, 30, 20);
            btn.Click += (s, e) => accion();
            _pnlContent.Controls.Add(btn);
            BeginInvoke(new Action(() => Redondear(btn, 10)));

            var btnReset = MakeBtn("↺  Restablecer", C_Muted, new Point(200, y), new Size(150, 40));
            btnReset.BackColor = Color.Transparent;
            btnReset.ForeColor = C_Muted;
            btnReset.MouseEnter += (s, e) => btnReset.ForeColor = C_Text;
            btnReset.MouseLeave += (s, e) => btnReset.ForeColor = C_Muted;
            btnReset.Click += (s, e) => BtnRestablecer_Click(s, e);
            _pnlContent.Controls.Add(btnReset);
            y += 50;
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
            var txt = new TextBox { Location = new Point(10, 7), Size = new Size(sz.Width - 20, 22), Font = new Font("Segoe UI", 10), BackColor = C_Input, ForeColor = C_Text, BorderStyle = BorderStyle.None };
            if (password) { txt.PasswordChar = '•'; txt.UseSystemPasswordChar = false; }
            pnl.Controls.Add(txt); parent.Controls.Add(pnl);
            return txt;
        }

        private ComboBox Combo(Panel parent, Point loc, int w)
        {
            var c = new ComboBox { Location = loc, Size = new Size(w, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), BackColor = C_Input, ForeColor = C_Text, FlatStyle = FlatStyle.Flat };
            parent.Controls.Add(c); return c;
        }

        private Button MakeBtn(string text, Color col, Point loc, Size sz)
        {
            var btn = new Button { Text = text, Location = loc, Size = sz, BackColor = col, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(Math.Max(0, col.R - 25), Math.Max(0, col.G - 25), Math.Max(0, col.B - 25));
            btn.MouseLeave += (s, e) => btn.BackColor = col;
            return btn;
        }

        private void Par(Panel parent, string label, string value, int y)
        {
            parent.Controls.Add(new Label { Text = label, ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(20, y), Size = new Size(140, 18) });
            parent.Controls.Add(new Label { Text = value, ForeColor = C_Text, Font = new Font("Segoe UI", 10), Location = new Point(170, y - 2), Size = new Size(220, 20) });
        }

        private Label FL(string t, int x, int y) =>
            new Label { Text = t, Location = new Point(x, y), ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true };

        private static GraphicsPath RR(Rectangle r, int radius)
        {
            int d = radius * 2; var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseAllFigures(); return p;
        }
        private static void Redondear(Control c, int r) { if (c.Width > 0 && c.Height > 0) c.Region = new Region(RR(c.ClientRectangle, r)); }
    }

    // ══════════════════════════════════════════════════════════
    //  TOGGLE SWITCH — Control personalizado GDI+
    // ══════════════════════════════════════════════════════════
    public class ToggleSwitch : Control
    {
        private bool _checked;
        private bool _hovered;

        public event EventHandler CheckedChanged;

        public bool Checked
        {
            get => _checked;
            set { if (_checked != value) { _checked = value; Invalidate(); CheckedChanged?.Invoke(this, EventArgs.Empty); } }
        }

        public ToggleSwitch(bool initialValue = false)
        {
            _checked = initialValue;
            Size = new Size(48, 26);
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
        }

        protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnClick(EventArgs e) { Checked = !_checked; base.OnClick(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(1, 1, Width - 2, Height - 2);

            Color on  = Color.FromArgb(52, 211, 153);
            Color off = Color.FromArgb(38, 44, 80);
            Color track = _checked ? on : (_hovered ? Color.FromArgb(55, 62, 100) : off);

            using (var path = GPR(r, r.Height / 2))
                g.FillPath(new SolidBrush(track), path);

            // Thumb
            int ts = Height - 8;
            int tx = _checked ? Width - ts - 3 : 3;
            g.FillEllipse(Brushes.White, new Rectangle(tx, 4, ts, ts));

            // Ícono dentro del thumb
            if (_checked)
                g.DrawString("✓", new Font("Segoe UI", 7, FontStyle.Bold), new SolidBrush(on),
                    new RectangleF(tx, 4, ts, ts), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        private static GraphicsPath GPR(Rectangle r, int rad)
        {
            int d = rad * 2; var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseAllFigures(); return p;
        }
    }
}
