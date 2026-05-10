using NexumApp.Helpers;
using NexumApp.Models;
using NexumApp.Services;
using NexumApp.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static NexumApp.Helpers.DashboardLayout;

namespace NexumApp.Forms.Principal
{
    public partial class FrmDashboardUsuario : Form
    {
        private UC_Inicio _ucInicio;
        private readonly CuentaService _cuentaService = new CuentaService();
        private readonly MovimientoService _movimientoService = new MovimientoService();
        private readonly Services.ConfiguracionService _cfgSvc = new Services.ConfiguracionService();
        private Label _lblSaludo, _lblSubtitulo;
        private List<Panel> _sidebarMenuButtons = new List<Panel>();
        private List<CuentaBancaria> _cuentasActivas = new List<CuentaBancaria>();
        private int? _cuentaActivaId;
        private Panel _activeSidebarBtn = null;
        private Panel _hoveredSidebarBtn = null;
        private DatosPresupuesto _ultimoPresupuesto;

        // Timer de inactividad de sesión
        private System.Windows.Forms.Timer _timerInactividad;
        private DateTime _ultimaActividad = DateTime.Now;

        public FrmDashboardUsuario()
        {
            InitializeComponent();
        }

        private void FrmDashboardUsuario_Load(object sender, EventArgs e)
        {
            try
            {
                // Cargar y aplicar configuración guardada del usuario
                CargarConfiguracionUsuario();

                // Aplicar colores del tema al formulario completo
                bool oscuro = AppSettings.ModoOscuro;
                this.BackColor      = oscuro ? Color.FromArgb(10, 12, 28)   : Color.FromArgb(244, 247, 254);
                pnlContenido.BackColor = oscuro ? Color.FromArgb(10, 12, 28) : Color.FromArgb(244, 247, 254);

                ConstruirSidebar();
                ConstruirHeader();
                ConstruirFooter();
                MostrarVistaInicio();

                // Iniciar timer de inactividad de sesión
                IniciarTimerInactividad();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el dashboard: {ex.Message}", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarConfiguracionUsuario()
        {
            try
            {
                int uid = SesionActual.Instancia?.Usuario?.Id ?? 0;
                if (uid <= 0) return;
                var cfg = _cfgSvc.ObtenerConfiguracion(uid);
                SesionActual.Instancia.Configuracion = cfg;
                AppSettings.CargarDesdeConfiguracion(cfg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando configuración: {ex.Message}");
            }
        }

        private void IniciarTimerInactividad()
        {
            _timerInactividad?.Stop();
            _timerInactividad = new System.Windows.Forms.Timer { Interval = 60000 }; // comprueba cada minuto
            _timerInactividad.Tick += (s, e) =>
            {
                int minutos = AppSettings.TiempoSesionMinutos;
                if (minutos <= 0) return;
                if ((DateTime.Now - _ultimaActividad).TotalMinutes >= minutos)
                {
                    _timerInactividad.Stop();
                    if (this.IsDisposed || !this.IsHandleCreated) return;
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show(
                            "Tu sesión ha expirado por inactividad.\nInicia sesión de nuevo para continuar.",
                            "Nexum Bank — Sesión expirada",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        BtnCerrarSesion_Click(this, EventArgs.Empty);
                    }));
                }
            };
            _timerInactividad.Start();
        }

        // Detectar actividad del usuario para reiniciar el timer de inactividad
        protected override void WndProc(ref Message m)
        {
            const int WM_MOUSEMOVE = 0x0200;
            const int WM_KEYDOWN   = 0x0100;
            const int WM_LBUTTONDOWN = 0x0201;
            if (m.Msg == WM_MOUSEMOVE || m.Msg == WM_KEYDOWN || m.Msg == WM_LBUTTONDOWN)
                _ultimaActividad = DateTime.Now;
            base.WndProc(ref m);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _timerInactividad?.Stop();
            base.OnFormClosing(e);
        }

        private void ConstruirFooter()
        {
            pnlFooter.Controls.Clear();
            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 1 };
            tlp.Controls.Add(new Label
            {
                Text = "Tu dinero está 100% protegido  •  Banco de España  •  Fondo de Garantía €100.000  •  Cifrado SSL 256-bit",
                Font = new Font("Segoe UI", 9),
                ForeColor = TextoGris,
                AutoSize = true,
                Anchor = AnchorStyles.None,
                MaximumSize = new Size(800, 0)
            }, 0, 0);
            pnlFooter.Controls.Add(tlp);
        }

        // ═══════════════════════════════════════════════════════
        //  SIDEBAR — Colores privados
        // ═══════════════════════════════════════════════════════
        private static readonly Color _sbAccent    = Color.FromArgb(129, 140, 248); // #818CF8
        private static readonly Color _sbAccent2   = Color.FromArgb(99,  102, 241); // #6366F1
        private static readonly Color _sbIconDark  = Color.FromArgb(26,  26,  46);  // #1A1A2E
        private static readonly Color _sbText      = Color.FromArgb(203, 213, 225); // #CBD5E1
        private static readonly Color _sbTextDim   = Color.FromArgb(100, 116, 139); // #64748B
        private static readonly Color _sbHeader    = Color.FromArgb(75,  85,  99);  // #4B5563
        private static readonly Color _sbSeparator = Color.FromArgb(31,  31,  53);  // #1F1F35
        private static readonly Color _sbHoverBg   = Color.FromArgb(13,  255, 255, 255); // rgba(255,255,255,0.05)
        private static readonly Color _sbActiveBg  = Color.FromArgb(51,  99,  102, 241);  // rgba(99,102,241,0.2)

        private void ConstruirSidebar()
        {
            pnlSidebar.Controls.Clear();
            pnlSidebar.BackColor = SidebarFondo;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(0),
                BackColor = Color.Transparent
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));    // Logo
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Menú
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 172));  // Footer

            // ── LOGO ────────────────────────────────────────────
            root.Controls.Add(ConstruirLogoSidebar(), 0, 0);

            // ── MENÚ ────────────────────────────────────────────
            var menu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(10, 8, 10, 4),
                BackColor = Color.Transparent
            };
            menu.SizeChanged += (s, e) => SincronizarAnchoItems(menu);

            _sidebarMenuButtons.Clear();
            _activeSidebarBtn = null;

            // Sección PRINCIPAL
            menu.Controls.Add(CrearHeaderSeccion(AppSettings.T("PRINCIPAL")));
            var itemsPrincipal = new (string icono, string texto, string badge, bool badgeRojo)[]
            {
                ("inicio",         AppSettings.T("Inicio"),          null,  false),
                ("cuentas",        AppSettings.T("Cuentas"),         null,  false),
                ("transferencias", AppSettings.T("Transferencias"),  "3",   true),
                ("pagos",          AppSettings.T("Pagos y Recargas"),null,  false),
            };
            foreach (var (icono, texto, badge, badgeRojo) in itemsPrincipal)
            {
                var idx = _sidebarMenuButtons.Count;
                var btn = CrearItemMenu(icono, texto, badge, badgeRojo, iconSize: 36);
                _sidebarMenuButtons.Add(btn);
                AgregarClickHandler(btn, idx);
                menu.Controls.Add(btn);
            }

            // Espaciador
            menu.Controls.Add(new Panel { Height = 8, BackColor = Color.Transparent });

            // Sección FINANZAS
            menu.Controls.Add(CrearHeaderSeccion(AppSettings.T("FINANZAS")));
            var itemsFinanzas = new (string icono, string texto, string badge, bool badgeRojo)[]
            {
                ("inversiones", AppSettings.T("Inversiones"), "+2.4%", false),
                ("tarjetas",    AppSettings.T("Tarjetas"),    null,    false),
                ("prestamos",   AppSettings.T("Préstamos"),   null,    false),
            };
            foreach (var (icono, texto, badge, badgeRojo) in itemsFinanzas)
            {
                var idx = _sidebarMenuButtons.Count;
                var btn = CrearItemMenu(icono, texto, badge, badgeRojo, iconSize: 36);
                _sidebarMenuButtons.Add(btn);
                AgregarClickHandler(btn, idx);
                menu.Controls.Add(btn);
            }

            root.Controls.Add(menu, 0, 1);

            // ── FOOTER ──────────────────────────────────────────
            root.Controls.Add(ConstruirFooterSidebar(), 0, 2);

            pnlSidebar.Controls.Add(root);

            // Activar "Inicio" por defecto
            if (_sidebarMenuButtons.Count > 0)
                SetActiveButton(_sidebarMenuButtons[0]);

            SincronizarAnchoItems(menu);
        }

        private void AgregarClickHandler(Panel btn, int idx)
        {
            EventHandler handler = (s, ev) => { SetActiveButton(btn); OnSidebarClick(idx); };
            btn.Click += handler;
            // Propagar a todos los controles hijos
            PropagateClick(btn, handler);
        }

        private void PropagateClick(Control parent, EventHandler handler)
        {
            foreach (Control c in parent.Controls)
            {
                c.Click += handler;
                PropagateClick(c, handler);
            }
        }

        private Panel ConstruirLogoSidebar()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2, RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(14, 0, 10, 0)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // ── ICONO ──────────────────────────────────────────
            var logoImg = UiHelper.CargarImagen("logo.png");
            var iconBox = new Panel { Size = new Size(46, 46), BackColor = Color.Transparent, Anchor = AnchorStyles.None };
            iconBox.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = UiHelper.CrearRoundedRect(iconBox.ClientRectangle, 12))
                {
                    if (logoImg != null)
                    {
                        ev.Graphics.SetClip(path);
                        ev.Graphics.DrawImage(logoImg, iconBox.ClientRectangle);
                        ev.Graphics.ResetClip();
                        using (var pen = new Pen(Color.FromArgb(70, 129, 140, 248), 1.5f))
                            ev.Graphics.DrawPath(pen, path);
                    }
                    else
                    {
                        using (var brush = new LinearGradientBrush(iconBox.ClientRectangle, _sbAccent2, _sbAccent, 135f))
                            ev.Graphics.FillPath(brush, path);
                        using (var shine = new SolidBrush(Color.FromArgb(40, 255, 255, 255)))
                            ev.Graphics.FillEllipse(shine, -8, -8, 32, 32);
                        var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        using (var f = new Font("Segoe UI", 18, FontStyle.Bold))
                            ev.Graphics.DrawString("N", f, Brushes.White, iconBox.ClientRectangle, fmt);
                    }
                }
            };
            var iconWrapper = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            iconWrapper.Controls.Add(iconBox);
            iconWrapper.Resize += (s, e) => iconBox.Location = new Point(0, (iconWrapper.Height - 46) / 2);
            tlp.Controls.Add(iconWrapper, 0, 0);

            // ── TEXTO (Paint manual para evitar solapamiento) ──
            var textPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            textPanel.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                g.SmoothingMode     = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                using (var fNexum = new Font("Segoe UI", 14.5f, FontStyle.Bold))
                using (var fBank  = new Font("Segoe UI", 8f,    FontStyle.Bold))
                {
                    SizeF sNexum = g.MeasureString("NEXUM", fNexum);
                    SizeF sBank  = g.MeasureString("BANK",  fBank);

                    float totalH = sNexum.Height + sBank.Height + 1f;
                    float startY = (textPanel.Height - totalH) / 2f;

                    g.DrawString("NEXUM", fNexum, Brushes.White, new PointF(1f, startY));

                    using (var ab = new SolidBrush(_sbAccent))
                        g.DrawString("BANK", fBank, ab, new PointF(2f, startY + sNexum.Height + 1f));
                }
            };
            tlp.Controls.Add(textPanel, 1, 0);

            var sep = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = _sbSeparator };
            panel.Controls.Add(tlp);
            panel.Controls.Add(sep);
            return panel;
        }

        private Panel ConstruirFooterSidebar()
        {
            var footer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(10, 0, 10, 12)
            };

            // Línea separadora superior
            var sep = new Panel { Height = 1, Dock = DockStyle.Top, BackColor = _sbSeparator };

            var flp = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 4, 0, 0)
            };
            flp.SizeChanged += (s, e) => SincronizarAnchoItems(flp);

            var btnAyuda  = CrearItemMenu("ayuda",  AppSettings.T("Ayuda / Tickets"), null, false, iconSize: 32);
            var btnConfig = CrearItemMenu("config", AppSettings.T("Configuración"),   null, false, iconSize: 32);
            btnAyuda.Click  += (s, e) => AbrirAyudaTickets();
            btnConfig.Click += (s, e) => AbrirConfiguracion();
            PropagateClick(btnAyuda,  (s, e) => AbrirAyudaTickets());
            PropagateClick(btnConfig, (s, e) => AbrirConfiguracion());

            flp.Controls.Add(btnAyuda);
            flp.Controls.Add(btnConfig);
            flp.Controls.Add(CrearItemLogout());

            footer.Controls.Add(flp);
            footer.Controls.Add(sep);
            return footer;
        }

        private Panel CrearHeaderSeccion(string titulo)
        {
            var lbl = new Label
            {
                Text = titulo,
                ForeColor = _sbHeader,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                AutoSize = false,
                Height = 26,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(4, 0, 0, 2)
            };
            // Efecto de letter-spacing simulado con espacios (WinForms no soporta kerning directo)
            lbl.Text = string.Join(" ", titulo.ToCharArray());

            var wrapper = new Panel { Height = 26, BackColor = Color.Transparent };
            wrapper.Controls.Add(lbl);
            return wrapper;
        }

        /// <summary>
        /// Crea un item del menú lateral con icono GDI+, texto y badge opcional.
        /// </summary>
        private Panel CrearItemMenu(string iconoTipo, string texto, string badge, bool badgeRojo, int iconSize)
        {
            var item = new Panel
            {
                Height = 46,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 4)
            };

            // Fondo + borde izquierdo (se pinta en Paint)
            item.Paint += (s, ev) =>
            {
                bool esActivo  = (item == _activeSidebarBtn);
                bool esHovered = (item == _hoveredSidebarBtn);

                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = item.ClientRectangle;

                if (esActivo)
                {
                    // Gradiente de izquierda a derecha: #818CF8/20% → transparente
                    using (var brush = new LinearGradientBrush(
                        new Point(0, 0), new Point(r.Width, 0),
                        _sbActiveBg, Color.Transparent))
                        ev.Graphics.FillRectangle(brush, r);

                    // Borde izquierdo 3px
                    ev.Graphics.FillRectangle(new SolidBrush(_sbAccent),
                        new Rectangle(0, 4, 3, r.Height - 8));
                }
                else if (esHovered)
                {
                    ev.Graphics.FillRectangle(new SolidBrush(_sbHoverBg), r);
                }
            };

            // Icono (caja cuadrada)
            int iconLeft = 14;
            int iconTop  = (44 - iconSize) / 2;
            var iconBox = new Panel
            {
                Size     = new Size(iconSize, iconSize),
                Location = new Point(iconLeft, iconTop),
                BackColor = Color.Transparent
            };
            iconBox.Paint += (s, ev) =>
            {
                bool esActivo = (item == _activeSidebarBtn);
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (var path = UiHelper.CrearRoundedRect(iconBox.ClientRectangle, 10))
                {
                    if (esActivo)
                    {
                        using (var brush = new LinearGradientBrush(
                            iconBox.ClientRectangle, _sbAccent2, _sbAccent, 135f))
                            ev.Graphics.FillPath(brush, path);
                    }
                    else
                    {
                        ev.Graphics.FillPath(new SolidBrush(_sbIconDark), path);
                    }
                }

                Color iconColor = (item == _activeSidebarBtn) ? Color.White : _sbText;
                DibujarIconoSidebar(ev.Graphics, iconBox.ClientRectangle, iconoTipo, iconColor);
            };

            // Texto
            int textLeft = iconLeft + iconSize + 12;
            bool tienesBadge = !string.IsNullOrEmpty(badge);
            var lblTexto = new Label
            {
                Text         = texto,
                ForeColor    = _sbText,
                Font         = new Font("Segoe UI", 10),
                AutoSize     = false,
                Height       = 46,
                Location     = new Point(textLeft, 0),
                Width        = tienesBadge ? 110 : 140,
                TextAlign    = ContentAlignment.MiddleLeft,
                BackColor    = Color.Transparent,
                AutoEllipsis = true
            };

            // Guardar referencia al label en Tag para actualizarlo desde SetActiveButton
            item.Tag = lblTexto;

            item.Controls.Add(iconBox);
            item.Controls.Add(lblTexto);

            // Badge opcional
            if (tienesBadge)
            {
                Color badgeColor = badgeRojo
                    ? Color.FromArgb(239, 68,  68)    // rojo
                    : Color.FromArgb(16,  185, 129);  // verde

                var lblBadge = new Label
                {
                    Text      = badge,
                    ForeColor = Color.White,
                    BackColor = badgeColor,
                    Font      = new Font("Segoe UI", 7, FontStyle.Bold),
                    AutoSize  = true,
                    Padding   = new Padding(5, 2, 5, 2),
                    Anchor    = AnchorStyles.Right | AnchorStyles.None,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                lblBadge.Paint += (s, ev) =>
                {
                    // Pill redondeada
                    ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var path = UiHelper.CrearRoundedRect(lblBadge.ClientRectangle, 8))
                        ev.Graphics.FillPath(new SolidBrush(badgeColor), path);
                    var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    ev.Graphics.DrawString(badge, lblBadge.Font, Brushes.White, lblBadge.ClientRectangle, fmt);
                };
                lblBadge.Location = new Point(item.Width - 46, (44 - 18) / 2);
                lblBadge.Size     = new Size(42, 18);
                lblBadge.Anchor   = AnchorStyles.Right;
                item.Controls.Add(lblBadge);
            }

            // Hover
            EventHandler enterH = (s, e) =>
            {
                _hoveredSidebarBtn = item;
                item.Invalidate();
            };
            EventHandler leaveH = (s, e) =>
            {
                if (_hoveredSidebarBtn == item) _hoveredSidebarBtn = null;
                item.Invalidate();
            };
            item.MouseEnter += enterH;
            item.MouseLeave += leaveH;
            foreach (Control c in item.Controls)
            {
                c.MouseEnter += enterH;
                c.MouseLeave += leaveH;
            }

            return item;
        }

        private Panel CrearItemLogout()
        {
            var item = new Panel
            {
                Height = 44,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 4, 0, 0)
            };
            item.Paint += (s, ev) =>
            {
                if (item == _hoveredSidebarBtn)
                    ev.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(20, 239, 68, 68)),
                        item.ClientRectangle);
            };

            var iconBox = new Panel
            {
                Size     = new Size(32, 32),
                Location = new Point(14, 6),
                BackColor = Color.Transparent
            };
            iconBox.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = UiHelper.CrearRoundedRect(iconBox.ClientRectangle, 8))
                    ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(40, 239, 68, 68)), path);
                DibujarIconoSidebar(ev.Graphics, iconBox.ClientRectangle, "salir",
                    Color.FromArgb(239, 68, 68));
            };

            var lbl = new Label
            {
                Text      = AppSettings.T("Cerrar Sesión"),
                ForeColor = Color.FromArgb(239, 68, 68),
                Font      = new Font("Segoe UI", 10),
                AutoSize  = false,
                Height    = 44,
                Location  = new Point(54, 0),
                Width     = 120,
                TextAlign = ContentAlignment.MiddleLeft
            };

            item.Controls.Add(iconBox);
            item.Controls.Add(lbl);

            EventHandler logoutHandler = (s, e) => BtnCerrarSesion_Click(s, e);
            item.Click  += logoutHandler;
            lbl.Click   += logoutHandler;
            iconBox.Click += logoutHandler;

            EventHandler enterH = (s, e) => { _hoveredSidebarBtn = item; item.Invalidate(); };
            EventHandler leaveH = (s, e) => { if (_hoveredSidebarBtn == item) { _hoveredSidebarBtn = null; item.Invalidate(); } };
            item.MouseEnter += enterH;
            item.MouseLeave += leaveH;
            lbl.MouseEnter  += enterH;
            lbl.MouseLeave  += leaveH;

            return item;
        }

        private void SincronizarAnchoItems(FlowLayoutPanel flp)
        {
            if (flp == null) return;
            int ancho = Math.Max(140, flp.ClientSize.Width - flp.Padding.Horizontal);
            foreach (Control c in flp.Controls)
                c.Width = ancho;
        }

        private void SetActiveButton(Panel btn)
        {
            _activeSidebarBtn = btn;

            // Actualizar colores de texto y redibujar todos los items
            foreach (var b in _sidebarMenuButtons)
            {
                if (b == null) continue;
                bool esActivo = (b == btn);
                if (b.Tag is Label lbl)
                {
                    lbl.ForeColor = esActivo ? Color.White : _sbText;
                    lbl.Font      = new Font("Segoe UI", 10,
                        esActivo ? FontStyle.Bold : FontStyle.Regular);
                }
                b.Invalidate(true);
            }
        }

        // ═══════════════════════════════════════════════════════
        //  ICONOS GDI+ — dibujados como líneas limpias
        // ═══════════════════════════════════════════════════════
        private static void DibujarIconoSidebar(Graphics g, Rectangle r, string tipo, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int m  = r.Width / 4;                   // margen
            float x = r.X + m, y = r.Y + m;
            float w = r.Width  - m * 2;
            float h = r.Height - m * 2;
            float cx = r.X + r.Width  / 2f;
            float cy = r.Y + r.Height / 2f;

            using (var pen = new Pen(color, 1.6f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var brush = new SolidBrush(color))
            {

            switch (tipo)
            {
                case "inicio": // Casa
                    // Techo
                    g.DrawLines(pen, new[] {
                        new PointF(x,     cy + 1),
                        new PointF(cx,    y),
                        new PointF(x + w, cy + 1)
                    });
                    // Paredes
                    g.DrawLines(pen, new[] {
                        new PointF(x + 2,     cy + 1),
                        new PointF(x + 2,     y + h),
                        new PointF(x + w - 2, y + h),
                        new PointF(x + w - 2, cy + 1)
                    });
                    // Puerta
                    g.DrawLines(pen, new[] {
                        new PointF(cx - 3, y + h),
                        new PointF(cx - 3, cy + 4),
                        new PointF(cx + 3, cy + 4),
                        new PointF(cx + 3, y + h)
                    });
                    break;

                case "cuentas": // Tres líneas (wallet/list)
                    g.DrawLine(pen, x,        cy - 5, x + w,        cy - 5);
                    g.DrawLine(pen, x,        cy,     x + w * 0.75f, cy);
                    g.DrawLine(pen, x,        cy + 5, x + w * 0.5f,  cy + 5);
                    break;

                case "transferencias": // Dos flechas opuestas ⇄
                    // Flecha → arriba
                    g.DrawLine(pen, x,            cy - 4, x + w - 2,    cy - 4);
                    g.DrawLines(pen, new[] {
                        new PointF(x + w - 6, cy - 8),
                        new PointF(x + w - 2, cy - 4),
                        new PointF(x + w - 6, cy)
                    });
                    // Flecha ← abajo
                    g.DrawLine(pen, x + 2, cy + 4, x + w, cy + 4);
                    g.DrawLines(pen, new[] {
                        new PointF(x + 6, cy),
                        new PointF(x + 2, cy + 4),
                        new PointF(x + 6, cy + 8)
                    });
                    break;

                case "pagos": // Tarjeta de pago
                    g.DrawRectangle(pen, x, y + 2, w, h - 4);
                    g.DrawLine(pen,      x, y + 7, x + w, y + 7);  // banda
                    g.DrawRectangle(pen, x + 4, cy + 1, 7, 5);      // chip
                    break;

                case "inversiones": // Gráfica ascendente ↗
                    g.DrawLines(pen, new[] {
                        new PointF(x,     y + h - 1),
                        new PointF(cx - 3, cy + 2),
                        new PointF(cx + 2, cy - 3),
                        new PointF(x + w - 1, y + 1)
                    });
                    // Punta de flecha
                    g.DrawLine(pen, x + w - 6, y + 1, x + w - 1, y + 1);
                    g.DrawLine(pen, x + w - 1, y + 1, x + w - 1, y + 6);
                    break;

                case "tarjetas": // Tarjeta con chip
                    g.DrawRectangle(pen, x, y + 3, w, h - 6);
                    g.DrawRectangle(pen, x + 4, cy - 3, 8, 6);  // chip
                    g.DrawLine(pen, cx + 2, cy + 3, x + w - 4, cy + 3);
                    break;

                case "prestamos": // Círculo con €
                    g.DrawEllipse(pen, x + 1, y + 1, w - 2, h - 2);
                    using (var f = new Font("Segoe UI", r.Width < 28 ? 7f : 8.5f, FontStyle.Bold))
                    {
                        var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString("€", f, brush, new RectangleF(r.X, r.Y, r.Width, r.Height), fmt);
                    }
                    break;

                case "ayuda": // Círculo con ?
                    g.DrawEllipse(pen, x + 1, y + 1, w - 2, h - 2);
                    using (var f = new Font("Segoe UI", r.Width < 28 ? 7f : 8.5f, FontStyle.Bold))
                    {
                        var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString("?", f, brush, new RectangleF(r.X, r.Y, r.Width, r.Height), fmt);
                    }
                    break;

                case "config": // Engranaje
                {
                    float rOuter = w / 2f - 0.5f;
                    float rInner = w / 2f - 3f;
                    g.DrawEllipse(pen, cx - rInner + 1, cy - rInner + 1, (rInner - 1) * 2, (rInner - 1) * 2);
                    for (int i = 0; i < 8; i++)
                    {
                        double ang = i * Math.PI / 4;
                        g.DrawLine(pen,
                            cx + (float)Math.Cos(ang) * rInner,
                            cy + (float)Math.Sin(ang) * rInner,
                            cx + (float)Math.Cos(ang) * rOuter,
                            cy + (float)Math.Sin(ang) * rOuter);
                    }
                    break;
                }

                case "salir": // Puerta con flecha →
                    // Marco
                    g.DrawLines(pen, new[] {
                        new PointF(cx + 1, y),
                        new PointF(x,      y),
                        new PointF(x,      y + h),
                        new PointF(cx + 1, y + h)
                    });
                    // Flecha
                    g.DrawLine(pen, cx - 1, cy, x + w, cy);
                    g.DrawLines(pen, new[] {
                        new PointF(x + w - 5, cy - 4),
                        new PointF(x + w,     cy),
                        new PointF(x + w - 5, cy + 4)
                    });
                    break;
            }
            } // cierre using pen + brush
        }

        private void OnSidebarClick(int idx)
        {
            if (idx == 0) { MostrarVistaInicio(); return; }
            if (idx == 1) { CargarVista(new VistaCuentas()); return; }
            if (idx == 2) { CargarVista(new VistaTransferencias()); return; }
            if (idx == 3) { CargarVista(new VistaPagarServicios()); return; }
            if (idx == 4) { CargarVista(new VistaInvertir()); return; }
            if (idx == 5) { CargarVista(new Views.VistaTarjetas()); return; }
            if (idx == 6) { CargarVista(new VistaPrestamos()); return; }
        }

        private void ConstruirHeader()
        {
            // Colores del header según el tema activo
            bool   oscuro     = AppSettings.ModoOscuro;
            Color  hdrBg      = oscuro ? Color.FromArgb(14, 17, 38)   : Color.White;
            Color  hdrTexto   = oscuro ? Color.FromArgb(241, 245, 249) : Color.FromArgb(31, 41, 55);
            Color  hdrMuted   = oscuro ? Color.FromArgb(100, 116, 139) : Color.FromArgb(107, 114, 128);
            Color  hdrBadgeBg = oscuro ? Color.FromArgb(24, 29, 58)   : Color.FromArgb(243, 244, 246);
            Color  hdrBorder  = oscuro ? Color.FromArgb(38, 44, 80)   : Color.FromArgb(229, 231, 235);

            pnlHeader.BackColor = hdrBg;
            pnlHeader.Controls.Clear();

            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, Padding = new Padding(Margen), BackColor = Color.Transparent };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 290));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var nombre = SesionActual.Instancia?.EstaLogeado == true && SesionActual.Instancia.Usuario != null
                ? SesionActual.Instancia.Usuario.Nombre : "Usuario";
            _lblSaludo    = new Label { Text = $"¡Hola, {nombre}! 👋", Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = hdrTexto,  AutoSize = true, MaximumSize = new Size(310, 0), BackColor = Color.Transparent };
            _lblSubtitulo = new Label { Text = AppSettings.T("Bienvenido a Nexum Bank"),  Font = new Font("Segoe UI", 10), ForeColor = hdrMuted, AutoSize = true, BackColor = Color.Transparent };
            var pnlSaludo = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Color.Transparent };
            pnlSaludo.Controls.Add(_lblSaludo);
            pnlSaludo.Controls.Add(_lblSubtitulo);
            tlp.Controls.Add(pnlSaludo, 0, 0);

            tlp.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent }, 1, 0);

            var flpRight  = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, AutoSize = false, BackColor = Color.Transparent };
            var pnlPerfil = new Panel { Width = 220, Height = 42, BackColor = hdrBadgeBg, Cursor = Cursors.Hand };
            pnlPerfil.Paint += (s, e) =>
            {
                using (var path = UiHelper.CrearRoundedRect(pnlPerfil.ClientRectangle, 12))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(new SolidBrush(hdrBadgeBg), path);
                    using (var pen = new Pen(hdrBorder, 1f))
                        e.Graphics.DrawPath(pen, path);
                }
            };

            var usuarioPerfil = SesionActual.Instancia?.Usuario;
            string iniciales  = ObtenerIniciales(usuarioPerfil?.Nombre, usuarioPerfil?.Apellidos);
            Color colorAvatar = GenerarColorAvatar((usuarioPerfil?.Nombre ?? "") + (usuarioPerfil?.Apellidos ?? ""));

            var pnlAvatar = new Panel { Size = new Size(34, 34), Location = new Point(6, 4), BackColor = Color.Transparent, Cursor = Cursors.Hand };
            pnlAvatar.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(colorAvatar))
                    ev.Graphics.FillEllipse(b, 0, 0, 33, 33);
                using (var shine = new SolidBrush(Color.FromArgb(40, 255, 255, 255)))
                    ev.Graphics.FillEllipse(shine, 2, 2, 15, 15);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var f = new Font("Segoe UI", iniciales.Length > 1 ? 11f : 13f, FontStyle.Bold))
                    ev.Graphics.DrawString(iniciales, f, Brushes.White, new RectangleF(0, 0, 34, 34), fmt);
            };

            string nombreCompleto = usuarioPerfil?.NombreCompleto ?? "Mi perfil";
            string emailUsuario   = usuarioPerfil?.Email ?? "";

            var lblPerfil = new Label
            {
                Text = nombreCompleto, ForeColor = hdrTexto,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                AutoSize = false, Width = 132, Height = 19, Location = new Point(48, 5),
                TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand, AutoEllipsis = true, BackColor = Color.Transparent
            };
            var lblEmail = new Label
            {
                Text = emailUsuario, ForeColor = hdrMuted,
                Font = new Font("Segoe UI", 7.5f),
                AutoSize = false, Width = 132, Height = 16, Location = new Point(48, 23),
                TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand, AutoEllipsis = true, BackColor = Color.Transparent
            };

            EventHandler abrirPerfil = (s, e) => { using (var f = new FrmPerfilUsuario()) { if (f.ShowDialog() == DialogResult.OK) ActualizarHeader(); } };
            pnlPerfil.Click += abrirPerfil; pnlAvatar.Click += abrirPerfil;
            lblPerfil.Click += abrirPerfil; lblEmail.Click   += abrirPerfil;
            pnlPerfil.Controls.Add(pnlAvatar);
            pnlPerfil.Controls.Add(lblPerfil);
            pnlPerfil.Controls.Add(lblEmail);
            flpRight.Controls.Add(pnlPerfil);
            tlp.Controls.Add(flpRight, 2, 0);

            var ua = SesionActual.Instancia?.Usuario?.UltimoAcceso;
            string textoAcceso = ua.HasValue
                ? "Último acceso: " + ua.Value.ToString("dd/MM/yyyy 'a las' HH:mm")
                : "Primer acceso";
            var lblUltimoAcceso = new Label { Text = textoAcceso, Font = new Font("Segoe UI", 9), ForeColor = hdrMuted, AutoSize = true, BackColor = Color.Transparent };
            tlp.Controls.Add(lblUltimoAcceso, 0, 1);
            tlp.SetColumnSpan(lblUltimoAcceso, 3);

            // Línea separadora inferior
            var sep = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = hdrBorder };
            pnlHeader.Controls.Add(sep);
            pnlHeader.Controls.Add(tlp);
        }

        private Panel CrearIconoCircular(string icono, EventHandler click)
        {
            var pnl = new Panel { Width = 36, Height = 36, Cursor = Cursors.Hand, BackColor = Color.FromArgb(243, 244, 246), Margin = new Padding(4, 0, 4, 0) };
            pnl.Controls.Add(new Label { Text = icono, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = TextoGris, AutoSize = true, Anchor = AnchorStyles.None });
            pnl.Click += click;
            foreach (Control c in pnl.Controls) c.Click += click;
            return pnl;
        }

        private Panel CrearIconoConBadge(string icono, string badge)
        {
            var pnl = new Panel { Width = 36, Height = 36, Cursor = Cursors.Hand, BackColor = Color.FromArgb(243, 244, 246), Margin = new Padding(4, 0, 4, 0) };
            pnl.Controls.Add(new Label { Text = icono, Font = new Font("Segoe UI", 14), AutoSize = true });
            pnl.Controls.Add(new Label { Text = badge, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(239, 68, 68), AutoSize = true });
            return pnl;
        }

        private void ActualizarHeader()
        {
            if (_lblSaludo != null && SesionActual.Instancia?.Usuario != null)
                _lblSaludo.Text = $"¡Hola, {SesionActual.Instancia.Usuario.Nombre}! 👋";
        }

        private void MostrarVistaInicio()
        {
            pnlContenido.Controls.Clear();
            pnlContenido.Padding = new Padding(Margen);
            _ucInicio = new UC_Inicio { Dock = DockStyle.Fill };
            pnlContenido.Controls.Add(_ucInicio);
            SuscribirEventosUCInicio();
            // Aplicar traducciones inmediatamente (sin BD, es instantáneo)
            AppSettings.AplicarTraduccionesRecursivo(_ucInicio);
            if (_sidebarMenuButtons != null && _sidebarMenuButtons.Count > 0)
                SetActiveButton(_sidebarMenuButtons[0]);
            // Cargar datos de BD en hilo de fondo → la UI no se bloquea
            var target = _ucInicio;
            Task.Run(() => CargarDatosDashboardBackground(target));
        }

        /// <summary>
        /// Ejecuta TODAS las llamadas a BD en un hilo de fondo y aplica
        /// los resultados al UC_Inicio usando BeginInvoke (nunca bloquea la UI).
        /// </summary>
        private void CargarDatosDashboardBackground(UC_Inicio target)
        {
            try
            {
                // ── Lectura de BD (hilo de fondo) ────────────────
                decimal saldo = 0;
                string textoCuenta = "";
                var movimientos   = new List<Movimiento>();
                var objetivos     = new List<ObjetivoAhorro>();
                DatosPresupuesto presupuesto = null;
                int puntos = 0;
                List<CuentaBancaria> cuentas = new List<CuentaBancaria>();
                List<Hucha>   huchas  = new List<Hucha>();
                List<Tarjeta> tarjetas = new List<Tarjeta>();
                int cuentaActivaId = 0;

                if (SesionActual.Instancia?.EstaLogeado == true && SesionActual.Instancia.Usuario != null)
                {
                    int uid = SesionActual.Instancia.Usuario.Id;

                    cuentas = _cuentaService.ObtenerCuentasPorUsuario(uid) ?? new List<CuentaBancaria>();
                    if (cuentas.Count > 0)
                    {
                        var c = cuentas.FirstOrDefault(x => x.Id == _cuentaActivaId) ?? cuentas[0];
                        cuentaActivaId  = c.Id;
                        saldo           = c.Saldo;
                        textoCuenta     = (c.TipoCuenta ?? "Cuenta") + " • " + FormatearIBAN(c.NumeroCuenta);
                        movimientos     = _movimientoService.ObtenerMovimientosPorCuenta(c.Id, 8) ?? new List<Movimiento>();
                    }
                    else
                    {
                        movimientos = _movimientoService.ObtenerMovimientosRecientesPorUsuario(uid, 8) ?? new List<Movimiento>();
                    }

                    var (ing, gas) = _movimientoService.ObtenerResumenMensual(uid);
                    decimal restante = ing - gas;
                    decimal objetivo = AppSettings.PresupuestoObjetivo > 0
                        ? AppSettings.PresupuestoObjetivo
                        : ing > 0 ? Math.Round(ing * 1.1m, 2) : 1000m;
                    presupuesto = new DatosPresupuesto
                    {
                        Ingresos       = ing,
                        Gastos         = gas,
                        Objetivo       = objetivo,
                        Restante       = restante > 0 ? restante : 0m,
                        MensajeEstado  = ing == 0 && gas == 0
                            ? "Sin movimientos este mes"
                            : restante >= 0 ? "¡Vas por buen camino! 💪" : "¡Cuidado con los gastos! ⚠"
                    };

                    try { huchas  = new HuchaService().ObtenerPorUsuario(uid); }  catch { }
                    try { tarjetas = new TarjetaService().ObtenerTarjetasPorUsuario(uid); } catch { }
                }

                objetivos = new List<ObjetivoAhorro>
                {
                    new ObjetivoAhorro { Nombre = "Viaje a Japón ✈️",       MontoActual = 1800m,  MontoObjetivo = 5000m,  Progreso = 36m, FechaObjetivo = new DateTime(2026, 12, 1) },
                    new ObjetivoAhorro { Nombre = "Entrada piso nuevo 🏡",   MontoActual = 15250m, MontoObjetivo = 30000m, Progreso = 51m, FechaObjetivo = new DateTime(2027, 6, 1)  }
                };
                if (puntos <= 0) puntos = 2450;

                // ── Aplicar al UI (hilo principal) ───────────────
                if (target.IsDisposed || !target.IsHandleCreated) return;
                target.BeginInvoke(new Action(() =>
                {
                    if (target.IsDisposed) return;

                    // Actualizar estado compartido del form
                    _cuentasActivas   = cuentas;
                    _cuentaActivaId   = cuentaActivaId > 0 ? cuentaActivaId : _cuentaActivaId;
                    _ultimoPresupuesto = presupuesto;

                    target.LoadHuchas(huchas);
                    target.LoadTarjetas(tarjetas);
                    target.LoadDashboardData(saldo, textoCuenta, movimientos, objetivos, presupuesto, puntos);
                    target.LoadCuentasActivas(_cuentasActivas, _cuentaActivaId ?? 0);
                    // Aplicar idioma a todos los controles generados tras la carga de datos
                    AppSettings.AplicarTraduccionesRecursivo(target);
                }));
            }
            catch { /* Error silencioso — la UI ya está visible, simplemente sin datos */ }
        }

        private static string FormatearIBAN(string iban)
        {
            if (string.IsNullOrEmpty(iban)) return "•••• •••• •••• ••••";
            var limpio = iban.Replace(" ", "").Trim();
            if (limpio.Length < 8) return "•••• " + limpio.PadLeft(4, '•');
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < Math.Min(limpio.Length, 24); i += 4)
            {
                if (i > 0) sb.Append(" ");
                int len = Math.Min(4, limpio.Length - i);
                sb.Append(limpio.Substring(i, len));
            }
            return sb.ToString();
        }

        private void SuscribirEventosUCInicio()
        {
            if (_ucInicio == null) return;
            _ucInicio.HuchaCreada += (s, e) =>
            {
                try
                {
                    var uid    = SesionActual.Instancia?.Usuario?.Id ?? 0;
                    var huchas = new Services.HuchaService().ObtenerPorUsuario(uid);
                    _ucInicio.LoadHuchas(huchas);
                }
                catch { var t = _ucInicio; Task.Run(() => CargarDatosDashboardBackground(t)); }
            };
            _ucInicio.AbonarHuchaRequested += (s, args) =>
            {
                try
                {
                    int cuentaId = _cuentaActivaId ?? 0;
                    if (cuentaId == 0)
                    { MessageBox.Show("No hay una cuenta activa seleccionada.", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                    var svc = new Services.HuchaService();
                    // Obtener nombre de la hucha antes de abonar para el concepto del movimiento
                    int uid      = SesionActual.Instancia?.Usuario?.Id ?? 0;
                    var huchasPre = svc.ObtenerPorUsuario(uid);
                    var huchaPre  = huchasPre.Find(h => h.Id == args.HuchaId);

                    bool ok = svc.AñadirSaldo(args.HuchaId, args.Monto, cuentaId, _movimientoService, huchaPre?.Nombre);
                    if (!ok)
                    { MessageBox.Show("No se pudo abonar el importe a la hucha.", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                    // Comprobar si se ha alcanzado la meta
                    var huchas   = svc.ObtenerPorUsuario(uid);
                    var hucha    = huchas.Find(h => h.Id == args.HuchaId);
                    bool metaAlcanzada = hucha != null && hucha.SaldoActual >= hucha.MetaObjetivo;

                    _ucInicio.LoadHuchas(huchas);
                    var t2 = _ucInicio; Task.Run(() => CargarDatosDashboardBackground(t2));

                    if (metaAlcanzada)
                    {
                        var res = MessageBox.Show(
                            $"🎉 ¡Has alcanzado tu meta de ahorro \"{hucha.Nombre}\"!\n\n" +
                            $"Meta: {hucha.MetaObjetivo.ToString("C2", AppSettings.CultureMoneda)}\n\n" +
                            "¿Deseas archivar esta hucha? (el saldo permanece en tu cuenta)",
                            "¡Meta alcanzada!", MessageBoxButtons.YesNo, MessageBoxIcon.None);

                        if (res == DialogResult.Yes)
                        {
                            svc.Eliminar(args.HuchaId);
                            var huchasActualizadas = svc.ObtenerPorUsuario(uid);
                            _ucInicio.LoadHuchas(huchasActualizadas);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al abonar: {ex.Message}", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            _ucInicio.EditarHuchaRequested += (s, huchaId) =>
            {
                try
                {
                    int uid    = SesionActual.Instancia?.Usuario?.Id ?? 0;
                    var svc    = new Services.HuchaService();
                    var hucha  = svc.ObtenerPorUsuario(uid).Find(h => h.Id == huchaId);
                    if (hucha == null) return;

                    using (var frm = new Forms.Principal.FrmEditarHucha(hucha))
                    {
                        if (frm.ShowDialog(this) != DialogResult.OK) return;
                        bool ok = svc.Actualizar(frm.HuchaActualizada);
                        if (ok)
                        {
                            _ucInicio.LoadHuchas(svc.ObtenerPorUsuario(uid));
                        }
                        else
                            MessageBox.Show("No se pudieron guardar los cambios.", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al editar: {ex.Message}", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            _ucInicio.EliminarHuchaRequested += (s, huchaId) =>
            {
                try
                {
                    int uid   = SesionActual.Instancia?.Usuario?.Id ?? 0;
                    var svc   = new Services.HuchaService();
                    var hucha = svc.ObtenerPorUsuario(uid).Find(h => h.Id == huchaId);
                    string nombre = hucha?.Nombre ?? "esta hucha";

                    var res = MessageBox.Show(
                        $"¿Seguro que quieres eliminar \"{nombre}\"?\n\nEl dinero ahorrado NO se devolverá a tu cuenta.",
                        "Eliminar hucha", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (res == DialogResult.Yes)
                    {
                        svc.Eliminar(huchaId);
                        _ucInicio.LoadHuchas(svc.ObtenerPorUsuario(uid));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar: {ex.Message}", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            _ucInicio.HuchasVerTodoClicked    += (s, e) => CargarVista(new Views.VistaHuchas());
            _ucInicio.TarjetasVerTodoClicked  += (s, e) => CargarVista(new Views.VistaTarjetas());
            _ucInicio.EnviarDineroClicked     += (s, e) => CargarVista(new VistaNuevaTransferencia());
            _ucInicio.RecibirDineroClicked += (s, e) =>
            {
                using (var frm = new Movimientos.FrmIngresarEfectivo())
                { if (frm.ShowDialog(this) == DialogResult.OK) { var t = _ucInicio; Task.Run(() => CargarDatosDashboardBackground(t)); } }
            };
            _ucInicio.VerTodosClicked += (s, e) => CargarVista(new VistaHistorialMovimientos());
            _ucInicio.AccesoRapidoClicked += (s, idx) =>
            {
                UserControl v = null;
                if (idx == 0) v = new VistaCashback();
                else if (idx == 1) v = new VistaPagarServicios();
                else if (idx == 2) v = new VistaRecargarMovil();
                else if (idx == 3) v = new VistaInvertir();
                else if (idx == 4) v = new VistaAnalizarGastos();
                else if (idx == 5) v = new VistaDividirCuenta();
                if (v != null) CargarVista(v);
            };
            _ucInicio.FondosIndexadosClicked += (s, e) => AbrirBecasBanco();
            _ucInicio.BizumClicked += (s, e) => AbrirPlanPensiones();
            _ucInicio.PresupuestoDetallesClicked += (s, e) => MostrarDialogoPresupuesto();
            _ucInicio.ObjetivosVerTodoClicked += (s, e) => CargarVista(new VistaMetasAhorro());
            _ucInicio.BeneficiosVerTodoClicked += (s, e) =>
                MessageBox.Show("Beneficios disponibles:\n- Cashback en compras\n- Descuentos en combustible\n- Promociones por puntos", "Beneficios Nexum", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _ucInicio.RetoParticiparClicked += (s, e) =>
                MessageBox.Show("Te has unido al reto de ahorro mensual. ¡Mucho éxito!", "Reto de ahorro", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _ucInicio.CuentaActivaChanged += (s, cuentaId) =>
            {
                _cuentaActivaId = cuentaId;
                var t = _ucInicio;
                Task.Run(() => CargarDatosDashboardBackground(t));
            };
        }

        private void CargarVista(UserControl vista)
        {
            pnlContenido.Controls.Clear();
            pnlContenido.Padding = new Padding(0);
            // Aplicar traducciones al idioma activo una vez construida la vista
            vista.HandleCreated += (s, e) => AppSettings.AplicarTraduccionesRecursivo(vista);

            // Vistas con su propio botón de volver integrado — no necesitan topBar
            if (vista is Views.VistaConfiguracion vcfg)
            {
                vcfg.VolverAlInicio      += (s, e) => MostrarVistaInicio();
                vcfg.TemaAplicado        += (s, e) => AplicarTemaVisual();
                vcfg.ConfiguracionGuardada += (s, e) =>
                {
                    // Reiniciar timer con el nuevo tiempo de sesión
                    IniciarTimerInactividad();
                    // Aplicar escala de fuente al formulario
                    AplicarEscalaFuente();
                    // Reconstruir sidebar/header para reflejar idioma y tema
                    RefrescarDashboard();
                };
                vista.Dock = DockStyle.Fill;
                pnlContenido.Controls.Add(vista);
                return;
            }
            if (vista is Views.VistaTarjetas vtarj)
            {
                vtarj.VolverAlInicio += (s, e) => MostrarVistaInicio();
                vista.Dock = DockStyle.Fill;
                pnlContenido.Controls.Add(vista);
                return;
            }
            if (vista is Views.VistaHuchas vh2)
            {
                vh2.VolverAlInicio += (s, e) => MostrarVistaInicio();
                vista.Dock = DockStyle.Fill;
                pnlContenido.Controls.Add(vista);
                return;
            }
            if (vista is VistaTransferencias vt)
            {
                vt.VolverAlInicio += (s, e) => MostrarVistaInicio();
                vista.Dock = DockStyle.Fill;
                pnlContenido.Controls.Add(vista);
                return;
            }
            if (vista is VistaBecas vb)
            {
                vb.VolverAlInicio        += (s, e) => MostrarVistaInicio();
                vb.MisSolicitudesClicked += (s, e) => CargarVista(new VistaMisSolicitudes());
                vista.Dock = DockStyle.Fill;
                pnlContenido.Controls.Add(vista);
                return;
            }
            if (vista is VistaMisSolicitudes vms)
            {
                vms.VolverAlInicio += (s, e) => CargarVista(new VistaBecas());
                vista.Dock = DockStyle.Fill;
                pnlContenido.Controls.Add(vista);
                return;
            }
            if (vista is VistaPrestamos vp)
            {
                vp.VolverAlInicio   += (s, e) => MostrarVistaInicio();
                vp.SimuladorClicked += (s, e) => CargarVista(new VistaSimuladorPrestamo());
                vista.Dock = DockStyle.Fill;
                pnlContenido.Controls.Add(vista);
                return;
            }
            if (vista is VistaSimuladorPrestamo vsim)
            {
                vsim.VolverAlPrestamos += (s, e) => CargarVista(new VistaPrestamos());
                vista.Dock = DockStyle.Fill;
                pnlContenido.Controls.Add(vista);
                return;
            }
            if (vista is VistaCashback vc)
            {
                vc.VolverAlInicio += (s, e) => MostrarVistaInicio();
                vista.Dock = DockStyle.Fill;
                pnlContenido.Controls.Add(vista);
                return;
            }
            if (vista is Views.VistaHuchas vh)
            {
                vh.VolverAlInicio += (s, e) => MostrarVistaInicio();
                vista.Dock = DockStyle.Fill;
                pnlContenido.Controls.Add(vista);
                return;
            }

            // Vistas sin botón Volver (navegación por el sidebar o tienen header propio)
            if (vista is VistaCuentas || vista is VistaHistorialMovimientos
                || vista is VistaAnalizarGastos || vista is VistaDividirCuenta
                || vista is VistaInvertir || vista is VistaNuevaTransferencia)
            {
                vista.Dock = DockStyle.Fill;
                pnlContenido.Controls.Add(vista);
                return;
            }

            // Para el resto de vistas: barra superior con botón Volver
            var topBar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 46,
                BackColor = vista.BackColor
            };
            topBar.Paint += (s, e) =>
            {
                bool dark = (vista.BackColor.R * 0.299 + vista.BackColor.G * 0.587 + vista.BackColor.B * 0.114) < 80;
                using (var pen = new Pen(dark ? Color.FromArgb(45, 55, 90) : Color.FromArgb(226, 232, 240)))
                    e.Graphics.DrawLine(pen, 0, topBar.Height - 1, topBar.Width, topBar.Height - 1);
            };
            topBar.Controls.Add(CrearBotonVolver(vista.BackColor));
            pnlContenido.Controls.Add(topBar);

            vista.Dock = DockStyle.Fill;
            pnlContenido.Controls.Add(vista);
        }

        private Panel CrearBotonVolver(Color bgVista)
        {
            double lum = bgVista.R * 0.299 + bgVista.G * 0.587 + bgVista.B * 0.114;
            bool dark = lum < 80;

            Color normalBg   = dark ? Color.FromArgb(28, 34, 68)    : Color.White;
            Color normalBord = dark ? Color.FromArgb(55, 65, 110)   : Color.FromArgb(209, 213, 219);
            Color normalText = dark ? Color.FromArgb(160, 175, 215) : Color.FromArgb(75, 85, 99);
            Color hoverBg    = dark ? Color.FromArgb(40, 48, 90)    : Color.FromArgb(243, 244, 246);
            Color hoverBord  = dark ? Color.FromArgb(99, 102, 241)  : Color.FromArgb(156, 163, 175);
            Color hoverText  = dark ? Color.White                   : Color.FromArgb(17, 24, 39);

            bool hovered = false;

            var btn = new Panel
            {
                Location  = new Point(16, 7),
                Size      = new Size(118, 32),
                BackColor = normalBg,
                Cursor    = Cursors.Hand,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left
            };

            EventHandler applyRegion = (s, e) =>
            {
                if (btn.Width > 4 && btn.Height > 4)
                    try { btn.Region = new Region(UiHelper.CrearRoundedRect(btn.ClientRectangle, 9)); } catch { }
            };
            btn.HandleCreated += applyRegion;
            btn.Resize        += applyRegion;

            btn.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = btn.ClientRectangle;

                Color cbg  = hovered ? hoverBg   : normalBg;
                Color cbrd = hovered ? hoverBord  : normalBord;
                Color ctxt = hovered ? hoverText  : normalText;

                using (var path = UiHelper.CrearRoundedRect(r, 9))
                {
                    g.FillPath(new SolidBrush(cbg), path);
                    g.DrawPath(new Pen(cbrd, 1f), path);
                }

                int cy = r.Height / 2;
                using (var pen = new Pen(ctxt, 2f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawLine(pen, 18, cy, 10, cy);
                    g.DrawLines(pen, new[] { new PointF(14f, cy - 5f), new PointF(10f, (float)cy), new PointF(14f, cy + 5f) });
                }

                TextRenderer.DrawText(g, "Volver",
                    new Font("Segoe UI", 10, FontStyle.Bold),
                    new Rectangle(26, 0, r.Width - 30, r.Height),
                    ctxt,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            };

            btn.MouseEnter += (s, e) => { hovered = true;  btn.BackColor = hoverBg;  btn.Invalidate(); };
            btn.MouseLeave += (s, e) => { hovered = false; btn.BackColor = normalBg; btn.Invalidate(); };
            btn.Click      += (s, e) => MostrarVistaInicio();

            return btn;
        }

        private void AbrirConfiguracion()
        {
            CargarVista(new Views.VistaConfiguracion());
        }

        private void AplicarTemaVisual()
        {
            // Reconstruir toda la interfaz con los nuevos colores del tema
            RefrescarDashboard();
        }

        private void AplicarEscalaFuente()
        {
            try
            {
                float escala = AppSettings.TamanoFuente / 100f;
                // Escalar la fuente base del formulario (WinForms la propaga a controles sin fuente propia)
                this.Font = new Font("Segoe UI", 9f * escala, FontStyle.Regular);
            }
            catch { }
        }

        private void RefrescarDashboard()
        {
            try
            {
                bool oscuro = AppSettings.ModoOscuro;

                // Fondo del área de contenido
                pnlContenido.BackColor = oscuro
                    ? Color.FromArgb(10, 12, 28)
                    : Color.FromArgb(244, 247, 254);

                // Fondo del footer
                if (pnlFooter != null)
                {
                    pnlFooter.BackColor = oscuro
                        ? Color.FromArgb(14, 17, 38)
                        : Color.FromArgb(249, 250, 251);
                    foreach (Control c in pnlFooter.Controls)
                    {
                        c.BackColor = Color.Transparent;
                        if (c is Label lbl) lbl.ForeColor = oscuro
                            ? Color.FromArgb(100, 116, 139)
                            : Color.FromArgb(107, 114, 128);
                    }
                }

                // Fondo del formulario principal
                this.BackColor = oscuro
                    ? Color.FromArgb(10, 12, 28)
                    : Color.FromArgb(244, 247, 254);

                pnlSidebar.Controls.Clear();
                pnlHeader.Controls.Clear();
                _sidebarMenuButtons.Clear();
                _activeSidebarBtn = null;
                ConstruirSidebar();
                ConstruirHeader();
                ConstruirFooter();
                MostrarVistaInicio();
            }
            catch { }
        }

        private void AbrirBecasBanco()
        {
            CargarVista(new NexumApp.Views.VistaBecas());
        }

        private void AbrirPlanPensiones()
        {
            CargarVista(new NexumApp.Views.VistaPlanPension());
        }

        private void AbrirAyudaTickets()
        {
            using (var frm = new Forms.Tickets.FrmOpcionesAyuda())
            {
                frm.ShowDialog(this);
            }
        }

        private void BtnAyuda_Click(object sender, EventArgs e)
        {
            MessageBox.Show("📧 soporte@nexumbank.com\n☎ 900 123 456\n🕒 L-V 9:00 - 18:00", "Soporte - Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string ObtenerIniciales(string nombre, string apellidos)
        {
            string ini = "";
            if (!string.IsNullOrEmpty(nombre))    ini += char.ToUpper(nombre[0]);
            if (!string.IsNullOrEmpty(apellidos)) ini += char.ToUpper(apellidos[0]);
            return ini.Length > 0 ? ini : "?";
        }

        private static Color GenerarColorAvatar(string clave)
        {
            var colores = new[]
            {
                Color.FromArgb(99,  102, 241),
                Color.FromArgb(16,  185, 129),
                Color.FromArgb(245, 158,  11),
                Color.FromArgb(239,  68,  68),
                Color.FromArgb(59,  130, 246),
                Color.FromArgb(168,  85, 247),
                Color.FromArgb(236,  72, 153),
                Color.FromArgb(20,  184, 166),
            };
            int hash = (clave ?? "").GetHashCode();
            return colores[Math.Abs(hash) % colores.Length];
        }

        // ═══════════════════════════════════════════════════════
        //  DIÁLOGO — Editar presupuesto mensual
        // ═══════════════════════════════════════════════════════
        private void MostrarDialogoPresupuesto()
        {
            var datos  = _ultimoPresupuesto;
            var es     = AppSettings.CultureMoneda;
            bool osc   = AppSettings.ModoOscuro;

            Color bgDlg  = osc ? Color.FromArgb(14, 17, 38)    : Color.White;
            Color txtMain = osc ? Color.FromArgb(241, 245, 249) : Color.FromArgb(31, 41, 55);
            Color txtMute = osc ? Color.FromArgb(100, 116, 139) : Color.FromArgb(107, 114, 128);
            Color borderC = osc ? Color.FromArgb(38, 44, 80)    : Color.FromArgb(220, 225, 235);
            Color inputBg = osc ? Color.FromArgb(24, 29, 58)    : Color.FromArgb(249, 250, 251);

            using (var dlg = new Form())
            {
                dlg.Text            = "Presupuesto del Mes — Nexum Bank";
                dlg.Size            = new Size(420, 330);
                dlg.MinimumSize     = dlg.Size;
                dlg.MaximumSize     = dlg.Size;
                dlg.FormBorderStyle = FormBorderStyle.FixedSingle;
                dlg.MaximizeBox     = false;
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.BackColor       = bgDlg;
                dlg.Font            = new Font("Segoe UI", 10);

                // ── Banda degradado superior ─────────────────────
                var band = new Panel { Dock = DockStyle.Top, Height = 68 };
                band.Paint += (s, ev) =>
                {
                    ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var br = new LinearGradientBrush(band.ClientRectangle,
                        Color.FromArgb(99, 102, 241), Color.FromArgb(139, 92, 246),
                        LinearGradientMode.Horizontal))
                        ev.Graphics.FillRectangle(br, band.ClientRectangle);
                    var fmt = new StringFormat
                        { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    using (var f = new Font("Segoe UI", 13, FontStyle.Bold))
                        ev.Graphics.DrawString("💰  Presupuesto del Mes", f, Brushes.White,
                            band.ClientRectangle, fmt);
                };
                dlg.Controls.Add(band);

                // ── Cuerpo ───────────────────────────────────────
                int y = 80;

                Label MkLbl(string t, int lx, int ly, Font f, Color c)
                {
                    var l = new Label { Text = t, Location = new Point(lx, ly), Font = f,
                        ForeColor = c, AutoSize = true, BackColor = Color.Transparent };
                    dlg.Controls.Add(l);
                    return l;
                }

                if (datos != null)
                {
                    MkLbl($"Ingresos este mes:  {datos.Ingresos.ToString("C0", es)}",
                        28, y, new Font("Segoe UI", 10), txtMute); y += 22;
                    MkLbl($"Gastos este mes:    {datos.Gastos.ToString("C0", es)}",
                        28, y, new Font("Segoe UI", 10), txtMute); y += 28;
                }

                MkLbl("Objetivo de ahorro mensual (€):", 28, y,
                    new Font("Segoe UI", 10, FontStyle.Bold), txtMain); y += 26;

                string valorInicial = AppSettings.PresupuestoObjetivo > 0
                    ? AppSettings.PresupuestoObjetivo.ToString("F2",
                        System.Globalization.CultureInfo.InvariantCulture)
                    : (datos != null && datos.Objetivo > 0
                        ? datos.Objetivo.ToString("F2",
                            System.Globalization.CultureInfo.InvariantCulture)
                        : "");

                var txtObjetivo = new TextBox
                {
                    Location    = new Point(28, y),
                    Size        = new Size(364, 28),
                    Font        = new Font("Segoe UI", 11),
                    BackColor   = inputBg,
                    ForeColor   = txtMain,
                    BorderStyle = BorderStyle.FixedSingle,
                    Text        = valorInicial
                };
                dlg.Controls.Add(txtObjetivo); y += 42;

                MkLbl("Introduce el importe mensual que quieres ahorrar.", 28, y,
                    new Font("Segoe UI", 9), txtMute); y += 36;

                // ── Botones ──────────────────────────────────────
                var btnGuardar = new Button
                {
                    Text      = "✓  Guardar",
                    Location  = new Point(28, y),
                    Size      = new Size(168, 38),
                    BackColor = Color.FromArgb(99, 102, 241),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor    = Cursors.Hand
                };
                btnGuardar.FlatAppearance.BorderSize = 0;
                btnGuardar.Click += (s, ev) =>
                {
                    string raw = txtObjetivo.Text.Replace(",", ".").Trim();
                    if (!decimal.TryParse(raw,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out decimal val) || val < 0)
                    {
                        MessageBox.Show(
                            "Introduce un importe válido (ej: 500 o 1500.00).",
                            "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    AppSettings.PresupuestoObjetivo = val;
                    try
                    {
                        int uid = SesionActual.Instancia?.Usuario?.Id ?? 0;
                        if (uid > 0)
                        {
                            var cfg = SesionActual.Instancia?.Configuracion
                                ?? new Models.ConfiguracionUsuario { UsuarioId = uid };
                            cfg.UsuarioId           = uid;
                            cfg.PresupuestoObjetivo = val;
                            SesionActual.Instancia.Configuracion = cfg;
                            _cfgSvc.GuardarConfiguracion(cfg);
                        }
                    }
                    catch { /* No bloquear la UI si falla el guardado */ }
                    dlg.DialogResult = DialogResult.OK;
                    dlg.Close();
                };

                var btnCancelar = new Button
                {
                    Text      = "Cancelar",
                    Location  = new Point(208, y),
                    Size      = new Size(184, 38),
                    BackColor = Color.Transparent,
                    ForeColor = txtMute,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 10),
                    Cursor    = Cursors.Hand
                };
                btnCancelar.FlatAppearance.BorderSize  = 1;
                btnCancelar.FlatAppearance.BorderColor = borderC;
                btnCancelar.Click += (s, ev) => dlg.Close();

                dlg.Controls.Add(btnGuardar);
                dlg.Controls.Add(btnCancelar);

                // Redibujar dashboard con el nuevo objetivo si el usuario guardó
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    var t = _ucInicio;
                    if (t != null)
                        Task.Run(() => CargarDatosDashboardBackground(t));
                }
            }
        }

        private void BtnCerrarSesion_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "¿Cerrar sesión?", "Nexum Bank",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            SesionActual.Instancia?.CerrarSesion();

            // Recuperar el FrmLogin original (está oculto tras el login)
            Auth.FrmLogin loginForm = null;
            foreach (Form f in Application.OpenForms)
                if (f is Auth.FrmLogin lf) { loginForm = lf; break; }

            if (loginForm != null)
            {
                loginForm.Show();
                loginForm.BringToFront();
            }
            else
            {
                new Auth.FrmLogin().Show();
            }

            Close();
        }
    }
}
