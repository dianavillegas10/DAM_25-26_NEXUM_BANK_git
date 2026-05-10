using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace NexumApp.Views
{
    public class VistaTransferencias : UserControl
    {
        // ── Servicios ──────────────────────────────────────────
        private readonly CuentaService        _cuentaService        = new CuentaService();
        private readonly TransferenciaService _transferenciaService = new TransferenciaService();

        // ── Paleta dinámica — se adapta al tema activo ────────
        private Color C_Bg      => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(10,  12,  28)  : Color.FromArgb(244, 247, 254);
        private Color C_Surface => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(14,  17,  38)  : Color.FromArgb(255, 255, 255);
        private Color C_Card    => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(18,  22,  46)  : Color.White;
        private Color C_Card2   => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(22,  27,  54)  : Color.FromArgb(248, 249, 252);
        private Color C_Input   => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(24,  29,  58)  : Color.FromArgb(245, 247, 250);
        private Color C_Border  => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(38,  44,  80)  : Color.FromArgb(220, 225, 235);
        private Color C_Text    => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(241, 245, 249) : Color.FromArgb(31,  41,  55);
        private Color C_Muted   => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(100, 116, 139) : Color.FromArgb(107, 114, 128);
        // Colores de acento — iguales en ambos temas
        private static readonly Color C_Blue   = Color.FromArgb(99,  102, 241);
        private static readonly Color C_Violet = Color.FromArgb(139, 92,  246);
        private static readonly Color C_Green  = Color.FromArgb(52,  211, 153);
        private static readonly Color C_Red    = Color.FromArgb(248, 113, 113);
        private static readonly Color C_Gold   = Color.FromArgb(251, 191, 36);
        // Moneda dinámica — sigue MonedaPreferida del usuario
        private CultureInfo ES => Helpers.AppSettings.CultureMoneda;

        // ── Controles ─────────────────────────────────────────
        private ComboBox _cmbOrigen;
        private TextBox  _txtBenef, _txtIBAN, _txtMonto, _txtConcepto;
        private Button   _btnContinuar;
        private Label    _lblSaldo, _lblTras, _lblError;
        private Panel    _pnlBenefBox, _pnlIBANBox, _pnlMontoBox, _pnlConcBox;
        private Panel    _pnlResumenCard;
        private Panel    _pnlHistPanel;
        private Panel    _pnlStatsBar;
        private bool     _modoConfirm = false;
        private static readonly decimal[] _rapidos = { 50m, 100m, 250m, 500m };

        // Evento que el Dashboard escucha para navegar al Inicio
        public event EventHandler VolverAlInicio;

        public VistaTransferencias()
        {
            BackColor = C_Bg;
            Dock = DockStyle.Fill;
            DoubleBuffered = true;
            Helpers.AppSettings.ConfiguracionChanged += (s, e) =>
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(new Action(() =>
                {
                    BuildUI();
                    Cargar();
                    Helpers.AppSettings.AplicarTraduccionesRecursivo(this);
                }));
            };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            BuildUI();
            Cargar();
            Helpers.AppSettings.AplicarTraduccionesRecursivo(this);
        }

        // ══════════════════════════════════════════════════════
        //  BUILD UI PRINCIPAL
        // ══════════════════════════════════════════════════════
        private void BuildUI()
        {
            Controls.Clear();

            // Fondo degradado vertical
            Paint += (s, ev) =>
            {
                using (var b = new LinearGradientBrush(ClientRectangle,
                    Color.FromArgb(12, 14, 34), C_Bg, LinearGradientMode.Vertical))
                    ev.Graphics.FillRectangle(b, ClientRectangle);
            };

            // Layout: 3 columnas
            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3, RowCount = 1,
                BackColor = Color.Transparent, Padding = new Padding(0)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 460)); // Formulario
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Historial
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260)); // Stats
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            tlp.Controls.Add(BuildColumnaFormulario(), 0, 0);
            tlp.Controls.Add(BuildColumnaHistorial(),  1, 0);
            tlp.Controls.Add(BuildColumnaStats(),      2, 0);
            Controls.Add(tlp);
        }

        // ══════════════════════════════════════════════════════
        //  COLUMNA 1 — FORMULARIO
        // ══════════════════════════════════════════════════════
        private Panel BuildColumnaFormulario()
        {
            var scroll = new Panel
            {
                Dock = DockStyle.Fill, AutoScroll = true,
                BackColor = C_Surface,
                Padding = new Padding(24, 22, 18, 22)
            };
            scroll.Paint += (s, ev) =>
            {
                using (var pen = new Pen(C_Border, 1))
                    ev.Graphics.DrawLine(pen, scroll.Width - 1, 0, scroll.Width - 1, scroll.Height);
            };

            var form = new Panel { Width = 412, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent };
            scroll.Controls.Add(form);
            scroll.Resize += (s, e) => form.Left = 0;

            int y = 0;

            // ── Cabecera ──────────────────────────────────────
            var pnlH = new Panel { Location = new Point(0, y), Size = new Size(412, 62), BackColor = Color.Transparent };
            var icoPanel = MakeIconPanel(new Point(0, 6), 46, C_Blue, C_Violet, "✈", 18f);
            pnlH.Controls.Add(icoPanel);
            pnlH.Controls.Add(new Label { Text = "Transferencias", ForeColor = C_Text, Font = new Font("Segoe UI", 17, FontStyle.Bold), Location = new Point(56, 4), AutoSize = true });
            pnlH.Controls.Add(new Label { Text = "Envía dinero de forma segura e inmediata", ForeColor = C_Muted, Font = new Font("Segoe UI", 9), Location = new Point(56, 34), AutoSize = true });
            form.Controls.Add(pnlH); y += 72;

            // ── Card formulario ───────────────────────────────
            var card = MakeCard(new Point(0, y), new Size(412, 0)); // altura dinámica
            form.Controls.Add(card);

            int iw = 364; int iy = 20;

            // CUENTA ORIGEN
            card.Controls.Add(FL("DESDE TU CUENTA", 20, iy)); iy += 20;
            _cmbOrigen = new ComboBox
            {
                Location = new Point(20, iy), Size = new Size(iw, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10), BackColor = C_Input,
                ForeColor = C_Text, FlatStyle = FlatStyle.Flat
            };
            _cmbOrigen.SelectedIndexChanged += (s, e) => { RefSaldo(); CalcTras(); };
            card.Controls.Add(_cmbOrigen); iy += 38;

            _lblSaldo = new Label { Location = new Point(20, iy), Size = new Size(iw, 16), ForeColor = C_Green, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            card.Controls.Add(_lblSaldo); iy += 20;

            card.Controls.Add(Sep(20, iy, iw)); iy += 12;

            // BENEFICIARIO
            card.Controls.Add(FL("NOMBRE DEL BENEFICIARIO", 20, iy)); iy += 20;
            _pnlBenefBox = IBox(card, new Point(20, iy), new Size(iw, 36), out _txtBenef, "Ej: María García López");
            _txtBenef.GotFocus  += (s, e) => BF(_pnlBenefBox, C_Blue);
            _txtBenef.LostFocus += (s, e) => BF(_pnlBenefBox, C_Border);
            iy += 44;

            // IBAN
            card.Controls.Add(FL("IBAN / CUENTA DESTINO", 20, iy)); iy += 20;
            _pnlIBANBox = IBox(card, new Point(20, iy), new Size(iw, 36), out _txtIBAN, "ES00 0000 0000 0000 0000 0000");
            _txtIBAN.CharacterCasing = CharacterCasing.Upper;
            _txtIBAN.GotFocus  += (s, e) => BF(_pnlIBANBox, C_Blue);
            _txtIBAN.LostFocus += (s, e) => BF(_pnlIBANBox, C_Border);
            _txtIBAN.TextChanged += (s, e) => { _lblError.Visible = false; RefResumen(); };
            iy += 44;

            // IMPORTE
            card.Controls.Add(FL("IMPORTE A ENVIAR", 20, iy)); iy += 20;
            _pnlMontoBox = IBox(card, new Point(20, iy), new Size(iw, 48), out _txtMonto, "0,00");
            _txtMonto.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            _txtMonto.ForeColor = C_Blue;
            _txtMonto.Size = new Size(iw - 40, 30); _txtMonto.Location = new Point(12, 8);
            _pnlMontoBox.Controls.Add(new Label { Text = Helpers.AppSettings.SimboloMoneda, ForeColor = C_Muted, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(iw - 26, 10), AutoSize = true });
            _txtMonto.GotFocus  += (s, e) => { if (_txtMonto.Text == "0,00") _txtMonto.Clear(); BF(_pnlMontoBox, C_Blue); };
            _txtMonto.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(_txtMonto.Text)) _txtMonto.Text = "0,00"; BF(_pnlMontoBox, C_Border); CalcTras(); RefResumen(); };
            _txtMonto.TextChanged += (s, e) => _lblError.Visible = false;
            iy += 56;

            // Saldo tras
            _lblTras = new Label { Location = new Point(20, iy), Size = new Size(iw, 16), ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            card.Controls.Add(_lblTras); iy += 20;

            // ACCESO RÁPIDO
            card.Controls.Add(FL("ACCESO RÁPIDO", 20, iy)); iy += 20;
            var tlpR = new TableLayoutPanel { Location = new Point(20, iy), Size = new Size(iw, 30), ColumnCount = 4, RowCount = 1, BackColor = Color.Transparent, Padding = new Padding(0) };
            for (int i = 0; i < 4; i++) tlpR.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            foreach (var imp in _rapidos)
            {
                var b = MakeQuickBtn(imp.ToString("C0", ES), C_Blue);
                decimal cap = imp;
                b.Click += (s, e) => { _txtMonto.Text = cap.ToString("N2", ES); CalcTras(); RefResumen(); _lblError.Visible = false; };
                tlpR.Controls.Add(b);
            }
            card.Controls.Add(tlpR); iy += 38;

            // CONCEPTO
            card.Controls.Add(FL("CONCEPTO", 20, iy)); iy += 20;
            _pnlConcBox = IBox(card, new Point(20, iy), new Size(iw, 36), out _txtConcepto, "Alquiler, regalo, pago compartido...");
            _txtConcepto.GotFocus  += (s, e) => BF(_pnlConcBox, C_Blue);
            _txtConcepto.LostFocus += (s, e) => BF(_pnlConcBox, C_Border);
            iy += 44;

            // ERROR
            _lblError = new Label { Location = new Point(20, iy), Size = new Size(iw, 16), ForeColor = C_Red, Font = new Font("Segoe UI", 8, FontStyle.Bold), Visible = false };
            card.Controls.Add(_lblError); iy += 20;

            card.Controls.Add(Sep(20, iy, iw)); iy += 12;

            // PANEL RESUMEN (preview)
            _pnlResumenCard = new Panel { Location = new Point(20, iy), Size = new Size(iw, 52), BackColor = Color.Transparent, Visible = false };
            _pnlResumenCard.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(_pnlResumenCard.ClientRectangle, 10))
                {
                    ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(18, C_Blue.R, C_Blue.G, C_Blue.B)), path);
                    ev.Graphics.DrawPath(new Pen(Color.FromArgb(55, C_Blue.R, C_Blue.G, C_Blue.B), 1), path);
                }
            };
            card.Controls.Add(_pnlResumenCard); iy += 60;

            // FILA BOTONES: [← Inicio] + [Continuar →]
            const int backW = 50, gap = 8;

            // Botón ← Inicio (icono flecha, va al Dashboard)
            var btnIco = new Button
            {
                Text      = "←",
                Location  = new Point(20, iy), Size = new Size(backW, 44),
                BackColor = C_Card2, ForeColor = C_Muted,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 16, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnIco.FlatAppearance.BorderColor = C_Border;
            btnIco.FlatAppearance.BorderSize  = 1;
            btnIco.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 99, 102, 241);
            btnIco.MouseEnter += (s, e) => btnIco.ForeColor = C_Text;
            btnIco.MouseLeave += (s, e) => btnIco.ForeColor = C_Muted;
            btnIco.Click      += (s, e) => VolverAlInicio?.Invoke(this, EventArgs.Empty);
            card.Controls.Add(btnIco);

            // Botón principal Continuar (ancho restante)
            _btnContinuar = new Button
            {
                Text      = "Continuar →",
                Location  = new Point(20 + backW + gap, iy), Size = new Size(iw - backW - gap, 44),
                BackColor = C_Blue, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand
            };
            _btnContinuar.FlatAppearance.BorderSize = 0;
            _btnContinuar.MouseEnter += (s, e) => _btnContinuar.BackColor = _modoConfirm ? Color.FromArgb(16, 185, 129) : C_Violet;
            _btnContinuar.MouseLeave += (s, e) => _btnContinuar.BackColor = _modoConfirm ? C_Green : C_Blue;
            _btnContinuar.Click += BtnClick;
            card.Controls.Add(_btnContinuar); iy += 50;

            // Botón volver (modo confirmación)
            var btnVolver = new Button
            {
                Text = "← Volver y editar",
                Location = new Point(20, iy), Size = new Size(iw, 28),
                BackColor = Color.Transparent, ForeColor = C_Muted,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand, Visible = false, Name = "btnVolver"
            };
            btnVolver.FlatAppearance.BorderSize = 0;
            btnVolver.MouseEnter += (s, e) => btnVolver.ForeColor = C_Text;
            btnVolver.MouseLeave += (s, e) => btnVolver.ForeColor = C_Muted;
            btnVolver.Click += (s, e) => Desconfirmar();
            card.Controls.Add(btnVolver); iy += 34;

            // Nota seguridad
            var lblSec = new Label { Text = "🔒  Cifrado SSL 256-bit · Verificación en tiempo real", ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Italic), Location = new Point(0, iy + 2), Size = new Size(412, 16), TextAlign = ContentAlignment.MiddleCenter };
            card.Controls.Add(lblSec);

            card.Height = iy + 24;
            form.Height = y + card.Height + 20;

            BeginInvoke(new Action(() =>
            {
                Redondear(_btnContinuar, 10);
                Redondear(card, 16);
                if (_pnlResumenCard.Width > 0) Redondear(_pnlResumenCard, 10);
                foreach (Control c in tlpR.Controls) Redondear(c, 7);
                // Redondear el botón ← Inicio
                foreach (Control c in card.Controls)
                    if (c is Button b && b.Size.Width < 60) Redondear(b, 10);
            }));

            return scroll;
        }

        // ══════════════════════════════════════════════════════
        //  COLUMNA 2 — HISTORIAL
        // ══════════════════════════════════════════════════════
        private Panel BuildColumnaHistorial()
        {
            var scroll = new Panel
            {
                Dock = DockStyle.Fill, AutoScroll = true,
                BackColor = C_Bg, Padding = new Padding(20, 22, 16, 22)
            };
            scroll.Paint += (s, ev) =>
            {
                using (var pen = new Pen(C_Border, 1))
                { ev.Graphics.DrawLine(pen, 0, 0, 0, scroll.Height); ev.Graphics.DrawLine(pen, scroll.Width - 1, 0, scroll.Width - 1, scroll.Height); }
            };

            _pnlHistPanel = new Panel { Width = 600, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent };
            scroll.Controls.Add(_pnlHistPanel);
            scroll.Resize += (s, e) => _pnlHistPanel.Width = Math.Max(200, scroll.ClientSize.Width - 36);

            return scroll;
        }

        // ══════════════════════════════════════════════════════
        //  COLUMNA 3 — ESTADÍSTICAS
        // ══════════════════════════════════════════════════════
        private Panel BuildColumnaStats()
        {
            var scroll = new Panel
            {
                Dock = DockStyle.Fill, AutoScroll = true,
                BackColor = C_Surface, Padding = new Padding(16, 22, 16, 22)
            };
            scroll.Paint += (s, ev) =>
            {
                using (var pen = new Pen(C_Border, 1))
                    ev.Graphics.DrawLine(pen, 0, 0, 0, scroll.Height);
            };

            _pnlStatsBar = new Panel { Width = 228, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent };
            scroll.Controls.Add(_pnlStatsBar);

            return scroll;
        }

        // ══════════════════════════════════════════════════════
        //  CARGA DE DATOS
        // ══════════════════════════════════════════════════════
        private void Cargar()
        {
            CargarCuentas();
            CargarHistorial();
            CargarStats();
        }

        private void CargarCuentas()
        {
            if (_cmbOrigen == null || SesionActual.Instancia?.Usuario == null) return;
            _cmbOrigen.Items.Clear();
            try
            {
                var lista = _cuentaService.ObtenerCuentasPorUsuario(SesionActual.Instancia.Usuario.Id);
                foreach (var c in lista)
                {
                    string u = c.NumeroCuenta?.Length > 4 ? c.NumeroCuenta.Substring(c.NumeroCuenta.Length - 4) : c.NumeroCuenta;
                    _cmbOrigen.Items.Add(new CI { Cuenta = c, D = $"{c.TipoCuenta}  ·  •••• {u}  —  {c.Saldo.ToString("C2", ES)}" });
                }
                _cmbOrigen.DisplayMember = "D";
                if (_cmbOrigen.Items.Count > 0) _cmbOrigen.SelectedIndex = 0;
                RefSaldo();
            }
            catch { }
        }

        private void CargarHistorial()
        {
            if (_pnlHistPanel == null) return;
            _pnlHistPanel.Controls.Clear();
            int y = 0;

            // Cabecera
            _pnlHistPanel.Controls.Add(new Label { Text = "Historial de transferencias", ForeColor = C_Text, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(0, y), AutoSize = true }); y += 28;
            _pnlHistPanel.Controls.Add(new Label { Text = "Tus últimas 20 operaciones enviadas", ForeColor = C_Muted, Font = new Font("Segoe UI", 9), Location = new Point(0, y), AutoSize = true }); y += 28;
            _pnlHistPanel.Controls.Add(new Panel { Location = new Point(0, y), Height = 1, BackColor = C_Border, Width = 800 }); y += 14;

            List<Transferencia> lista = new List<Transferencia>();
            try
            {
                if (SesionActual.Instancia?.Usuario != null)
                    lista = _transferenciaService.ObtenerTransferenciasPorUsuario(SesionActual.Instancia.Usuario.Id, 20);
            }
            catch { }

            if (lista.Count == 0)
            {
                _pnlHistPanel.Controls.Add(new Label { Text = "Aún no has realizado ninguna transferencia.", ForeColor = C_Muted, Font = new Font("Segoe UI", 10, FontStyle.Italic), Location = new Point(0, y), AutoSize = true });
                _pnlHistPanel.Height = y + 40; return;
            }

            // Agrupar por fecha
            var grupos = lista.GroupBy(t => t.Fecha.Date).OrderByDescending(g => g.Key);
            foreach (var grupo in grupos)
            {
                string encabezado = grupo.Key == DateTime.Today ? "Hoy"
                    : grupo.Key == DateTime.Today.AddDays(-1) ? "Ayer"
                    : grupo.Key.ToString("dd MMMM yyyy", ES);

                _pnlHistPanel.Controls.Add(new Label { Text = encabezado.ToUpper(), ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(0, y), AutoSize = true }); y += 20;

                foreach (var t in grupo.OrderByDescending(x => x.Fecha))
                {
                    var fila = FilaHistorial(t);
                    fila.Location = new Point(0, y);
                    _pnlHistPanel.Controls.Add(fila);
                    y += 64;
                }
                y += 6;
            }

            _pnlHistPanel.Controls.Add(new Panel { Location = new Point(0, y), Height = 1, BackColor = C_Border, Width = 800 }); y += 10;
            _pnlHistPanel.Controls.Add(new Label { Text = $"Total enviado: {lista.Sum(t => t.Monto).ToString("C2", ES)}", ForeColor = C_Muted, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(0, y), AutoSize = true });
            _pnlHistPanel.Height = y + 30;
        }

        private Panel FilaHistorial(Transferencia t)
        {
            bool ok = t.Estado == "Completada";
            Color col = ok ? C_Blue : C_Red;
            string ult4 = t.CuentaDestino?.Length > 4 ? "••••" + t.CuentaDestino.Substring(t.CuentaDestino.Length - 4) : (t.CuentaDestino ?? "—");
            string benef = string.IsNullOrWhiteSpace(t.NombreBeneficiario) ? ult4 : t.NombreBeneficiario;
            string concepto = string.IsNullOrWhiteSpace(t.Concepto) ? "Sin concepto" : t.Concepto;

            var fila = new Panel { Size = new Size(800, 58), BackColor = Color.Transparent, Cursor = Cursors.Hand };
            fila.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                bool hov = fila.ClientRectangle.Contains(fila.PointToClient(Cursor.Position));
                using (var path = RR(new Rectangle(0, 1, fila.Width - 1, fila.Height - 2), 10))
                {
                    ev.Graphics.FillPath(new SolidBrush(hov ? C_Card2 : C_Card), path);
                    ev.Graphics.DrawPath(new Pen(hov ? C_Blue : C_Border, 1), path);
                }
                // Barra de acento izquierda
                ev.Graphics.FillRectangle(new SolidBrush(col), new Rectangle(0, 12, 3, fila.Height - 24));
            };
            fila.MouseEnter += (s, e) => fila.Invalidate();
            fila.MouseLeave += (s, e) => fila.Invalidate();

            // Icono
            var ico = new Panel { Size = new Size(34, 34), Location = new Point(12, 12), BackColor = Color.Transparent };
            ico.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(ico.ClientRectangle, 9))
                    ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(22, col.R, col.G, col.B)), path);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString("✈", new Font("Segoe UI", 13), new SolidBrush(col), ico.ClientRectangle, fmt);
            };

            // Badge estado
            var badge = new Panel { Size = new Size(76, 17), Location = new Point(52, 6) };
            badge.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color bc = ok ? Color.FromArgb(18, C_Green.R, C_Green.G, C_Green.B) : Color.FromArgb(18, C_Red.R, C_Red.G, C_Red.B);
                using (var path = RR(badge.ClientRectangle, 5))
                    ev.Graphics.FillPath(new SolidBrush(bc), path);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString((ok ? "✓ " : "✗ ") + (t.Estado ?? "—").ToUpper(), new Font("Segoe UI", 7, FontStyle.Bold), new SolidBrush(ok ? C_Green : C_Red), badge.ClientRectangle, fmt);
            };

            fila.Controls.Add(ico); fila.Controls.Add(badge);
            fila.Controls.Add(new Label { Text = benef,    ForeColor = C_Text,  Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(52, 24), Size = new Size(220, 20), AutoEllipsis = true });
            fila.Controls.Add(new Label { Text = concepto, ForeColor = C_Muted, Font = new Font("Segoe UI", 8),  Location = new Point(52, 40), Size = new Size(220, 16), AutoEllipsis = true });
            fila.Controls.Add(new Label { Text = t.Fecha.ToString("HH:mm"),    ForeColor = C_Muted, Font = new Font("Segoe UI", 8), Location = new Point(284, 40), AutoSize = true });
            fila.Controls.Add(new Label { Text = t.CuentaDestino?.Length > 16 ? FormatIBAN(t.CuentaDestino) : ult4, ForeColor = C_Muted, Font = new Font("Consolas", 8), Location = new Point(284, 24), Size = new Size(180, 16), AutoEllipsis = true });
            fila.Controls.Add(new Label { Text = "-" + t.Monto.ToString("C2", ES), ForeColor = col, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(475, 18), Size = new Size(140, 22), TextAlign = ContentAlignment.MiddleRight });

            // Clic → rellenar formulario
            EventHandler rel = (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(t.NombreBeneficiario)) _txtBenef.Text = t.NombreBeneficiario;
                if (!string.IsNullOrWhiteSpace(t.CuentaDestino))      _txtIBAN.Text  = t.CuentaDestino;
                if (!string.IsNullOrWhiteSpace(t.Concepto))            _txtConcepto.Text = t.Concepto;
                _txtMonto.Text = t.Monto.ToString("N2", ES);
                CalcTras(); RefResumen(); _lblError.Visible = false;
            };
            fila.Click += rel; ico.Click += rel;
            foreach (Control c in fila.Controls) c.Click += rel;

            return fila;
        }

        private void CargarStats()
        {
            if (_pnlStatsBar == null) return;
            _pnlStatsBar.Controls.Clear();
            int y = 0;

            _pnlStatsBar.Controls.Add(new Label { Text = "Resumen", ForeColor = C_Text, Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(0, y), AutoSize = true }); y += 28;
            _pnlStatsBar.Controls.Add(new Label { Text = "Estadísticas de tus envíos", ForeColor = C_Muted, Font = new Font("Segoe UI", 9), Location = new Point(0, y), AutoSize = true }); y += 28;
            _pnlStatsBar.Controls.Add(new Panel { Location = new Point(0, y), Height = 1, BackColor = C_Border, Width = 300 }); y += 14;

            List<Transferencia> lista = new List<Transferencia>();
            try { if (SesionActual.Instancia?.Usuario != null) lista = _transferenciaService.ObtenerTransferenciasPorUsuario(SesionActual.Instancia.Usuario.Id, 100); } catch { }

            // Stat cards
            var mes = lista.Where(t => t.Fecha.Month == DateTime.Now.Month && t.Fecha.Year == DateTime.Now.Year).ToList();
            var hoy = lista.Where(t => t.Fecha.Date == DateTime.Today).ToList();

            StatCard(_pnlStatsBar, ref y, "ESTE MES",       mes.Count + " transferencias",           mes.Sum(t => t.Monto).ToString("C2", ES), C_Blue);
            StatCard(_pnlStatsBar, ref y, "HOY",            hoy.Count + " transferencias",            hoy.Sum(t => t.Monto).ToString("C2", ES), C_Gold);
            StatCard(_pnlStatsBar, ref y, "TOTAL HISTÓRICO", lista.Count + " transferencias",         lista.Sum(t => t.Monto).ToString("C2", ES), C_Violet);
            StatCard(_pnlStatsBar, ref y, "COMPLETADAS",    lista.Count(t => t.Estado == "Completada") + "/" + lista.Count, (lista.Count > 0 ? (lista.Count(t => t.Estado == "Completada") * 100 / lista.Count) + "%" : "—"), C_Green);

            y += 8;
            _pnlStatsBar.Controls.Add(new Panel { Location = new Point(0, y), Height = 1, BackColor = C_Border, Width = 300 }); y += 14;

            // Top destinos recientes
            _pnlStatsBar.Controls.Add(new Label { Text = "DESTINOS RECIENTES", ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(0, y), AutoSize = true }); y += 20;

            foreach (var t in lista.GroupBy(x => x.CuentaDestino).OrderByDescending(g => g.Count()).Take(4))
            {
                var fila = new Panel { Location = new Point(0, y), Size = new Size(228, 44), BackColor = Color.Transparent, Cursor = Cursors.Hand };
                fila.Paint += (s, ev) =>
                {
                    ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    bool hov = fila.ClientRectangle.Contains(fila.PointToClient(Cursor.Position));
                    using (var path = RR(new Rectangle(0, 1, fila.Width - 1, fila.Height - 2), 9))
                        ev.Graphics.FillPath(new SolidBrush(hov ? C_Card2 : C_Card), path);
                };
                fila.MouseEnter += (s, e) => fila.Invalidate();
                fila.MouseLeave += (s, e) => fila.Invalidate();

                string ult = t.Key?.Length > 4 ? "••••" + t.Key.Substring(t.Key.Length - 4) : (t.Key ?? "—");
                string nom = string.IsNullOrWhiteSpace(t.First().NombreBeneficiario) ? ult : t.First().NombreBeneficiario;
                fila.Controls.Add(new Label { Text = nom, ForeColor = C_Text, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(10, 6), Size = new Size(180, 18), AutoEllipsis = true });
                fila.Controls.Add(new Label { Text = t.Count() + " envíos · " + t.Sum(x => x.Monto).ToString("C0", ES), ForeColor = C_Muted, Font = new Font("Segoe UI", 8), Location = new Point(10, 24), AutoSize = true });

                string capK = t.Key; string capN = t.First().NombreBeneficiario;
                EventHandler rel = (s, e) => { _txtIBAN.Text = capK ?? ""; if (!string.IsNullOrWhiteSpace(capN)) _txtBenef.Text = capN; RefResumen(); };
                fila.Click += rel; foreach (Control c in fila.Controls) c.Click += rel;

                _pnlStatsBar.Controls.Add(fila); y += 50;
            }

            _pnlStatsBar.Height = y + 10;
        }

        private void StatCard(Panel parent, ref int y, string titulo, string sub, string valor, Color col)
        {
            var card = new Panel { Location = new Point(0, y), Size = new Size(228, 66), BackColor = C_Card };
            card.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(card.ClientRectangle, 10))
                { ev.Graphics.FillPath(new SolidBrush(C_Card), path); ev.Graphics.DrawPath(new Pen(C_Border, 1), path); }
                ev.Graphics.FillRectangle(new SolidBrush(col), new Rectangle(0, 0, 3, card.Height));
            };
            card.Controls.Add(new Label { Text = titulo, ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(12, 8), AutoSize = true });
            card.Controls.Add(new Label { Text = valor,  ForeColor = col,    Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(12, 24), AutoSize = true });
            card.Controls.Add(new Label { Text = sub,    ForeColor = C_Muted, Font = new Font("Segoe UI", 8), Location = new Point(12, 46), AutoSize = true });
            parent.Controls.Add(card); y += 74;
        }

        // ══════════════════════════════════════════════════════
        //  LÓGICA DE FORMULARIO
        // ══════════════════════════════════════════════════════
        private void RefSaldo()
        {
            if (_cmbOrigen?.SelectedItem is CI item)
                _lblSaldo.Text = $"✓  Saldo disponible: {item.Cuenta.Saldo.ToString("C2", ES)}";
        }

        private void CalcTras()
        {
            if (!(_cmbOrigen?.SelectedItem is CI item)) return;
            string ms = _txtMonto.Text.Trim().Replace(",", ".").Replace(" ", "");
            if (!decimal.TryParse(ms, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal m) || m <= 0) { _lblTras.Text = ""; return; }
            decimal tras = item.Cuenta.Saldo - m;
            _lblTras.ForeColor = tras >= 0 ? C_Muted : C_Red;
            _lblTras.Text = tras >= 0 ? $"Saldo restante: {tras.ToString("C2", ES)}" : "⚠  Saldo insuficiente";
        }

        private void RefResumen()
        {
            if (_pnlResumenCard == null) return;
            string ms = _txtMonto.Text.Trim().Replace(",", ".").Replace(" ", "");
            bool ok1 = decimal.TryParse(ms, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal m) && m > 0;
            bool ok2 = !string.IsNullOrWhiteSpace(_txtIBAN?.Text);
            _pnlResumenCard.Visible = ok1 && ok2;
            if (!_pnlResumenCard.Visible) return;

            foreach (Control c in _pnlResumenCard.Controls.OfType<Label>().ToList()) _pnlResumenCard.Controls.Remove(c);
            string benef = string.IsNullOrWhiteSpace(_txtBenef?.Text) ? (_txtIBAN?.Text?.Trim() ?? "—") : _txtBenef.Text.Trim();
            _pnlResumenCard.Controls.Add(new Label { Text = m.ToString("C2", ES), ForeColor = C_Blue, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(46, 6), AutoSize = true });
            _pnlResumenCard.Controls.Add(new Label { Text = "→  " + benef, ForeColor = C_Muted, Font = new Font("Segoe UI", 9), Location = new Point(46, 30), Size = new Size(300, 16), AutoEllipsis = true });
        }

        private void BtnClick(object sender, EventArgs e)
        {
            if (!_modoConfirm) { if (Validar()) Confirmar(); }
            else Enviar();
        }

        private bool Validar()
        {
            _lblError.Visible = false;
            if (_cmbOrigen?.SelectedItem == null) { E("Selecciona la cuenta de origen."); return false; }
            if (string.IsNullOrWhiteSpace(_txtIBAN?.Text)) { E("Introduce el IBAN de destino."); return false; }
            if (_txtIBAN.Text.Trim().Replace(" ", "").Length < 10) { E("El IBAN es demasiado corto. Verifica."); return false; }
            string ms = _txtMonto.Text.Trim().Replace(",", ".").Replace(" ", "");
            if (!decimal.TryParse(ms, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal m) || m <= 0) { E("Introduce un importe válido mayor que 0 €."); return false; }
            if (m > ((CI)_cmbOrigen.SelectedItem).Cuenta.Saldo) { E("Saldo insuficiente en la cuenta seleccionada."); return false; }
            return true;
        }

        private void Confirmar()
        {
            _modoConfirm = true;
            _btnContinuar.Text = "✓  CONFIRMAR Y ENVIAR"; _btnContinuar.BackColor = C_Green;
            var bv = FindControl(_btnContinuar.Parent, "btnVolver"); if (bv != null) bv.Visible = true;
            _txtBenef.ReadOnly = true; _txtIBAN.ReadOnly = true; _txtMonto.ReadOnly = true; _txtConcepto.ReadOnly = true; _cmbOrigen.Enabled = false;
            BF(_pnlIBANBox, C_Green); BF(_pnlMontoBox, C_Green);
        }

        private void Desconfirmar()
        {
            _modoConfirm = false;
            _btnContinuar.Text = "Continuar →"; _btnContinuar.BackColor = C_Blue;
            var bv = FindControl(_btnContinuar.Parent, "btnVolver"); if (bv != null) bv.Visible = false;
            _txtBenef.ReadOnly = false; _txtIBAN.ReadOnly = false; _txtMonto.ReadOnly = false; _txtConcepto.ReadOnly = false; _cmbOrigen.Enabled = true;
            BF(_pnlIBANBox, C_Border); BF(_pnlMontoBox, C_Border);
        }

        private void Enviar()
        {
            _btnContinuar.Enabled = false; _btnContinuar.Text = "Enviando...";
            string ms = _txtMonto.Text.Trim().Replace(",", ".").Replace(" ", "");
            decimal.TryParse(ms, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal monto);
            var cuenta = ((CI)_cmbOrigen.SelectedItem).Cuenta;
            try
            {
                bool ok = _transferenciaService.RealizarTransferencia(
                    cuenta.Id, _txtIBAN.Text.Trim(),
                    string.IsNullOrWhiteSpace(_txtBenef.Text) ? (SesionActual.Instancia?.Usuario?.NombreCompleto ?? "Titular") : _txtBenef.Text.Trim(),
                    monto, _txtConcepto.Text.Trim(), out string err);

                if (ok)
                {
                    MessageBox.Show($"Transferencia de {monto.ToString("C2", ES)} enviada correctamente.", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.None);
                    Limpiar(); Cargar();
                }
                else { E(err ?? "No se pudo completar la transferencia."); Desconfirmar(); }
            }
            catch (Exception ex) { E("Error: " + ex.Message); Desconfirmar(); }
            _btnContinuar.Enabled = true;
        }

        private void Limpiar()
        {
            _txtBenef.Text = ""; _txtIBAN.Text = ""; _txtMonto.Text = "0,00"; _txtConcepto.Text = "";
            _lblError.Visible = false; _lblTras.Text = ""; _pnlResumenCard.Visible = false;
            Desconfirmar();
        }

        private void E(string msg) { _lblError.Text = "⚠  " + msg; _lblError.Visible = true; }

        // ══════════════════════════════════════════════════════
        //  HELPERS UI
        // ══════════════════════════════════════════════════════
        private Panel MakeCard(Point loc, Size sz)
        {
            var p = new Panel { Location = loc, Size = sz, BackColor = C_Card };
            p.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(p.ClientRectangle, 16))
                { ev.Graphics.FillPath(new SolidBrush(C_Card), path); ev.Graphics.DrawPath(new Pen(C_Border, 1), path); }
            };
            return p;
        }

        private Panel IBox(Panel parent, Point loc, Size sz, out TextBox txt, string hint)
        {
            var p = new Panel { Location = loc, Size = sz, BackColor = C_Input };
            p.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(p.ClientRectangle, 9))
                { ev.Graphics.FillPath(new SolidBrush(C_Input), path); ev.Graphics.DrawPath(new Pen(C_Border, 1), path); }
            };
            txt = new TextBox { Location = new Point(10, (sz.Height - 22) / 2), Size = new Size(sz.Width - 20, 22), Font = new Font("Segoe UI", 10), BackColor = C_Input, ForeColor = C_Text, BorderStyle = BorderStyle.None };
            p.Controls.Add(txt); parent.Controls.Add(p); return p;
        }

        private void BF(Panel p, Color col)
        {
            p.Tag = col; p.Paint -= BD; p.Paint += BD; p.Invalidate();
        }
        private void BD(object s, PaintEventArgs ev)
        {
            var p = (Panel)s; var col = p.Tag is Color c ? c : C_Border;
            ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = RR(p.ClientRectangle, 9))
            { ev.Graphics.FillPath(new SolidBrush(C_Input), path); ev.Graphics.DrawPath(new Pen(col, 1.5f), path); }
        }

        private Panel MakeIconPanel(Point loc, int sz, Color c1, Color c2, string g, float fs)
        {
            var p = new Panel { Location = loc, Size = new Size(sz, sz), BackColor = Color.Transparent };
            p.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(p.ClientRectangle, sz / 4))
                using (var br = new LinearGradientBrush(p.ClientRectangle, c1, c2, 135f))
                    ev.Graphics.FillPath(br, path);
                using (var sh = new SolidBrush(Color.FromArgb(40, 255, 255, 255)))
                    ev.Graphics.FillEllipse(sh, -sz / 4, -sz / 4, sz, sz);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString(g, new Font("Segoe UI", fs, FontStyle.Bold), Brushes.White, p.ClientRectangle, fmt);
            };
            return p;
        }

        private Button MakeQuickBtn(string text, Color col)
        {
            var b = new Button { Text = text, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 0), BackColor = C_Input, ForeColor = col, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderColor = C_Border; b.FlatAppearance.BorderSize = 1;
            b.MouseEnter += (s, e) => b.BackColor = Color.FromArgb(28, col.R, col.G, col.B);
            b.MouseLeave += (s, e) => b.BackColor = C_Input;
            return b;
        }

        private Label FL(string t, int x, int y) => new Label { Text = t, Location = new Point(x, y), ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true };
        private Panel Sep(int x, int y, int w)    => new Panel { Location = new Point(x, y), Size = new Size(w, 1), BackColor = C_Border };

        private static Control FindControl(Control parent, string name)
        {
            if (parent == null) return null;
            foreach (Control c in parent.Controls) { if (c.Name == name) return c; }
            return null;
        }

        private static string FormatIBAN(string iban)
        {
            if (string.IsNullOrEmpty(iban)) return "—";
            var l = iban.Replace(" ", "");
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < l.Length; i++) { if (i > 0 && i % 4 == 0) sb.Append(' '); sb.Append(l[i]); }
            return sb.ToString();
        }

        private static GraphicsPath RR(Rectangle r, int radius)
        {
            int d = radius * 2; var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseAllFigures(); return p;
        }
        private static void Redondear(Control c, int r) { if (c.Width > 0 && c.Height > 0) c.Region = new Region(RR(c.ClientRectangle, r)); }

        private class CI { public CuentaBancaria Cuenta; public string D; public override string ToString() => D; }
    }
}
