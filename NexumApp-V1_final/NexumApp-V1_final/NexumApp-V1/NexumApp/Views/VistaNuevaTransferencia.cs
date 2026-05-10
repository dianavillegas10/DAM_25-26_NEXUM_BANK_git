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
    /// <summary>
    /// Quick-send: envío rápido de dinero desde el Dashboard.
    /// Light-theme, single-column, 3 pasos: ¿A quién? → ¿Cuánto? → Confirmar.
    /// Diferente a VistaTransferencias (que es el módulo completo con historial/stats).
    /// </summary>
    public class VistaNuevaTransferencia : UserControl
    {
        private readonly CuentaService        _cuentaSvc  = new CuentaService();
        private readonly TransferenciaService _transSvc   = new TransferenciaService();

        // ── Paleta light ─────────────────────────────────────────────
        private static readonly Color Fondo   = Color.FromArgb(248, 250, 252);
        private static readonly Color White   = Color.White;
        private static readonly Color Azul    = Color.FromArgb( 99, 102, 241);
        private static readonly Color AzulD   = Color.FromArgb( 67,  56, 202);
        private static readonly Color Verde   = Color.FromArgb( 16, 185, 129);
        private static readonly Color Rojo    = Color.FromArgb(239,  68,  68);
        private static readonly Color Oscuro  = Color.FromArgb( 15,  23,  42);
        private static readonly Color Gris    = Color.FromArgb(100, 116, 139);
        private static readonly Color Border  = Color.FromArgb(226, 232, 240);
        private static readonly Color InputBg = Color.FromArgb(248, 250, 252);
        private static readonly CultureInfo ES = CultureInfo.CreateSpecificCulture("es-ES");

        // ── UI refs ───────────────────────────────────────────────────
        private TextBox  _txtIBAN, _txtBenef, _txtConcepto, _txtMonto;
        private ComboBox _cmbCuenta;
        private Label    _lblSaldo, _lblSaldoTras, _lblError, _lblMonto;
        private Panel    _pnlIBANBox, _pnlMontoBox;
        private Button   _btnEnviar;
        private Panel    _pnlRecientes, _main, _scroll;

        // ── Estado ────────────────────────────────────────────────────
        private bool _confirmando = false;
        private List<Transferencia> _historial = new List<Transferencia>();

        public VistaNuevaTransferencia()
        {
            BackColor      = Fondo;
            Dock           = DockStyle.Fill;
            DoubleBuffered = true;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            BuildUI();
            CargarDatos();
        }

        // ══════════════════════════════════════════════════════════════
        //  BUILD UI
        // ══════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            Controls.Clear();

            _scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Fondo };
            _main   = new Panel { Width = 560, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent };
            _scroll.Controls.Add(_main);
            _scroll.Resize += (s, ev) => _main.Left = Math.Max(20, (_scroll.ClientSize.Width - 560) / 2);
            Controls.Add(_scroll);

            int y = 24;

            // ── HERO ─────────────────────────────────────────────────
            var hero = new Panel { Location = new Point(0, y), Size = new Size(560, 72), BackColor = Color.Transparent };
            hero.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                // Círculo gradiente con icono
                var r = new Rectangle(0, 4, 56, 56);
                using (var path = RR(r, 16))
                using (var br = new LinearGradientBrush(r, Azul, Color.FromArgb(139, 92, 246), 135f))
                    g.FillPath(br, path);
                using (var shine = new SolidBrush(Color.FromArgb(35, 255, 255, 255)))
                    g.FillEllipse(shine, -8, -8, 40, 40);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("💸", new Font("Segoe UI Emoji", 22), Brushes.White, new RectangleF(0, 4, 56, 56), fmt);
                // Textos
                TextRenderer.DrawText(g, "Enviar dinero",
                    new Font("Segoe UI", 20, FontStyle.Bold),
                    new Rectangle(72, 8, 488, 32), Oscuro, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                TextRenderer.DrawText(g, "Rápido, seguro y sin comisiones",
                    new Font("Segoe UI", 10),
                    new Rectangle(72, 40, 488, 24), Gris, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            };
            _main.Controls.Add(hero); y += 84;

            // ── PASO 1 — ¿A QUIÉN ENVÍAS? ────────────────────────────
            _main.Controls.Add(StepLabel("1", "¿A quién envías?", new Point(0, y))); y += 36;

            var card1 = Card(new Point(0, y), new Size(560, 0)); _main.Controls.Add(card1);
            int c1y = 16;

            // Recientes (chips cargados dinámicamente)
            card1.Controls.Add(FieldLabel("CONTACTOS RECIENTES", new Point(20, c1y))); c1y += 22;
            _pnlRecientes = new Panel { Location = new Point(16, c1y), Size = new Size(524, 40), BackColor = Color.Transparent };
            card1.Controls.Add(_pnlRecientes); c1y += 48;

            // IBAN
            card1.Controls.Add(Sep(16, c1y, 528)); c1y += 14;
            card1.Controls.Add(FieldLabel("IBAN / CUENTA DESTINO", new Point(20, c1y))); c1y += 22;
            _pnlIBANBox = InputBox(card1, new Point(16, c1y), new Size(528, 42), out _txtIBAN,
                "ES00 0000 0000 0000 0000 0000");
            _txtIBAN.CharacterCasing = CharacterCasing.Upper;
            _txtIBAN.GotFocus  += (s, e) => BF(_pnlIBANBox, Azul);
            _txtIBAN.LostFocus += (s, e) => BF(_pnlIBANBox, Border);
            _txtIBAN.TextChanged += (s, e) => { _lblError.Visible = false; };
            c1y += 50;

            // Beneficiario
            card1.Controls.Add(FieldLabel("NOMBRE DEL BENEFICIARIO (opcional)", new Point(20, c1y))); c1y += 22;
            var pnlBenef = InputBox(card1, new Point(16, c1y), new Size(528, 40), out _txtBenef,
                "Ej: María García");
            _txtBenef.GotFocus  += (s, e) => BF(pnlBenef, Azul);
            _txtBenef.LostFocus += (s, e) => BF(pnlBenef, Border);
            c1y += 48;

            card1.Height = c1y + 12;
            y += card1.Height + 16;

            // ── PASO 2 — ¿CUÁNTO? ────────────────────────────────────
            _main.Controls.Add(StepLabel("2", "¿Cuánto?", new Point(0, y))); y += 36;

            var card2 = Card(new Point(0, y), new Size(560, 0)); _main.Controls.Add(card2);
            int c2y = 20;

            // Importe grande centrado
            _pnlMontoBox = new Panel
            {
                Location  = new Point(16, c2y),
                Size      = new Size(528, 72),
                BackColor = Color.FromArgb(239, 246, 255),
                Cursor    = Cursors.IBeam
            };
            _pnlMontoBox.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color bg = _pnlMontoBox.Tag is Color c ? c : Color.FromArgb(239, 246, 255);
                using (var path = RR(_pnlMontoBox.ClientRectangle, 14))
                {
                    e.Graphics.FillPath(new SolidBrush(bg), path);
                    e.Graphics.DrawPath(new Pen(Azul, 1.5f), path);
                }
            };
            _txtMonto = new TextBox
            {
                Text        = "0,00",
                Font        = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor   = Azul,
                BackColor   = Color.FromArgb(239, 246, 255),   // mismo color que _pnlMontoBox
                BorderStyle = BorderStyle.None,
                TextAlign   = HorizontalAlignment.Center,
                Size        = new Size(430, 44),
                Location    = new Point(10, 14)
            };
            _txtMonto.GotFocus  += (s, e) => { if (_txtMonto.Text == "0,00") _txtMonto.Clear(); BF2(_pnlMontoBox, Color.FromArgb(224, 237, 255)); };
            _txtMonto.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(_txtMonto.Text)) _txtMonto.Text = "0,00"; BF2(_pnlMontoBox, Color.FromArgb(239, 246, 255)); RecalcSaldoTras(); };
            _txtMonto.TextChanged += (s, e) => _lblError.Visible = false;
            var lblEur = new Label { Text = "€", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Azul, BackColor = Color.Transparent, AutoSize = true, Location = new Point(450, 18) };
            _pnlMontoBox.Controls.Add(_txtMonto);
            _pnlMontoBox.Controls.Add(lblEur);
            card2.Controls.Add(_pnlMontoBox); c2y += 80;

            // Saldo tras
            _lblSaldoTras = new Label { Location = new Point(20, c2y), Size = new Size(520, 18), Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Gris, BackColor = Color.Transparent };
            card2.Controls.Add(_lblSaldoTras); c2y += 24;

            // Chips de importes rápidos
            card2.Controls.Add(FieldLabel("IMPORTES RÁPIDOS", new Point(20, c2y))); c2y += 22;
            var tlpChips = new TableLayoutPanel { Location = new Point(16, c2y), Size = new Size(528, 38), ColumnCount = 5, RowCount = 1, BackColor = Color.Transparent, Padding = new Padding(0) };
            for (int i = 0; i < 5; i++) tlpChips.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            foreach (var imp in new decimal[] { 20m, 50m, 100m, 200m, 500m })
            {
                decimal cap = imp;
                var chip = new Button
                {
                    Text      = cap.ToString("C0", ES),
                    Dock      = DockStyle.Fill,
                    Margin    = new Padding(0, 0, 6, 0),
                    BackColor = Color.FromArgb(239, 246, 255),
                    ForeColor = Azul,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor    = Cursors.Hand
                };
                chip.FlatAppearance.BorderColor = Color.FromArgb(196, 218, 255);
                chip.FlatAppearance.BorderSize  = 1;
                chip.FlatAppearance.MouseOverBackColor  = Color.FromArgb(224, 237, 255);
                chip.FlatAppearance.MouseDownBackColor  = Color.FromArgb(196, 218, 255);
                chip.Click += (s, e) => { _txtMonto.Text = cap.ToString("N2", ES); RecalcSaldoTras(); _lblError.Visible = false; };
                EventHandler applyR = (s, ev) => { if(chip.Width>4) try{chip.Region=new Region(RR(chip.ClientRectangle,8));}catch{} };
                chip.HandleCreated += applyR; chip.Resize += applyR;
                tlpChips.Controls.Add(chip);
            }
            card2.Controls.Add(tlpChips); c2y += 46;

            // Cuenta origen
            card2.Controls.Add(Sep(16, c2y, 528)); c2y += 14;
            card2.Controls.Add(FieldLabel("DESDE LA CUENTA", new Point(20, c2y))); c2y += 22;
            _cmbCuenta = new ComboBox
            {
                Location      = new Point(16, c2y), Size = new Size(528, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 10),
                BackColor     = InputBg, ForeColor = Oscuro, FlatStyle = FlatStyle.Flat
            };
            _cmbCuenta.SelectedIndexChanged += (s, e) => { ActualizarSaldo(); RecalcSaldoTras(); };
            card2.Controls.Add(_cmbCuenta); c2y += 38;

            _lblSaldo = new Label { Location = new Point(20, c2y), Size = new Size(520, 18), Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Verde, BackColor = Color.Transparent };
            card2.Controls.Add(_lblSaldo); c2y += 22;

            // Concepto
            card2.Controls.Add(Sep(16, c2y, 528)); c2y += 14;
            card2.Controls.Add(FieldLabel("CONCEPTO (opcional)", new Point(20, c2y))); c2y += 22;
            var pnlConc = InputBox(card2, new Point(16, c2y), new Size(528, 38), out _txtConcepto,
                "Alquiler, regalo, pago compartido...");
            _txtConcepto.GotFocus  += (s, e) => BF(pnlConc, Azul);
            _txtConcepto.LostFocus += (s, e) => BF(pnlConc, Border);
            c2y += 46;

            // Error label
            _lblError = new Label { Location = new Point(20, c2y), Size = new Size(520, 18), Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Rojo, BackColor = Color.Transparent, Visible = false };
            card2.Controls.Add(_lblError); c2y += 22;

            card2.Height = c2y + 12;
            y += card2.Height + 20;

            // ── BOTÓN ENVIAR ──────────────────────────────────────────
            _btnEnviar = new Button
            {
                Location  = new Point(0, y), Size = new Size(560, 54),
                Text      = "💸   Enviar dinero",
                Font      = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White, BackColor = Verde,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            _btnEnviar.FlatAppearance.BorderSize = 0;
            _btnEnviar.FlatAppearance.MouseOverBackColor = Color.FromArgb(10, 150, 100);
            _btnEnviar.FlatAppearance.MouseDownBackColor = Color.FromArgb(5, 120, 80);
            EventHandler applyBtnR = (s, e) => { if(_btnEnviar.Width>4) try{_btnEnviar.Region=new Region(RR(_btnEnviar.ClientRectangle,14));}catch{} };
            _btnEnviar.HandleCreated += applyBtnR; _btnEnviar.Resize += applyBtnR;
            _btnEnviar.Click += BtnEnviar_Click;
            _main.Controls.Add(_btnEnviar); y += 62;

            // Nota seguridad
            var nota = new Panel { Location = new Point(0, y), Size = new Size(560, 28), BackColor = Color.Transparent };
            nota.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                TextRenderer.DrawText(e.Graphics,
                    "🔒   Cifrado SSL 256-bit  ·  Verificación en tiempo real  ·  Sin comisiones",
                    new Font("Segoe UI", 9),
                    nota.ClientRectangle, Gris,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            _main.Controls.Add(nota); y += 40;
            _main.Height = y;

            BeginInvoke(new Action(() =>
                _main.Left = Math.Max(20, (_scroll.ClientSize.Width - 560) / 2)));
        }

        // ══════════════════════════════════════════════════════════════
        //  DATOS
        // ══════════════════════════════════════════════════════════════
        private void CargarDatos()
        {
            CargarCuentas();
            CargarRecientes();
        }

        private void CargarCuentas()
        {
            _cmbCuenta.Items.Clear();
            var usr = SesionActual.Instancia?.Usuario;
            if (usr == null) return;
            try
            {
                var lista = _cuentaSvc.ObtenerCuentasPorUsuario(usr.Id);
                foreach (var c in lista)
                {
                    string u = c.NumeroCuenta?.Length > 4 ? c.NumeroCuenta.Substring(c.NumeroCuenta.Length - 4) : c.NumeroCuenta;
                    _cmbCuenta.Items.Add(new CI { Cuenta = c, D = $"{c.TipoCuenta}  ·  ••••{u}  —  {c.Saldo.ToString("C2", ES)}" });
                }
                _cmbCuenta.DisplayMember = "D";
                if (_cmbCuenta.Items.Count > 0) _cmbCuenta.SelectedIndex = 0;
                ActualizarSaldo();
            }
            catch { }
        }

        private void CargarRecientes()
        {
            if (_pnlRecientes == null) return;
            _pnlRecientes.Controls.Clear();
            try
            {
                var usr = SesionActual.Instancia?.Usuario;
                if (usr == null) return;
                _historial = _transSvc.ObtenerTransferenciasPorUsuario(usr.Id, 30) ?? new List<Transferencia>();
            }
            catch { _historial = new List<Transferencia>(); }

            // Dedupe por IBAN, tomar los 5 más recientes
            var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var recientes = new List<Transferencia>();
            foreach (var t in _historial)
            {
                if (!string.IsNullOrWhiteSpace(t.CuentaDestino) && vistos.Add(t.CuentaDestino))
                {
                    recientes.Add(t); if (recientes.Count >= 5) break;
                }
            }

            if (recientes.Count == 0)
            {
                var lbl = new Label { Text = "Sin envíos recientes", Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Gris, AutoSize = true, Location = new Point(0, 10) };
                _pnlRecientes.Controls.Add(lbl);
                return;
            }

            int cx = 0;
            foreach (var t in recientes)
            {
                string nom   = string.IsNullOrWhiteSpace(t.NombreBeneficiario) ? FormatIBANCorto(t.CuentaDestino) : t.NombreBeneficiario.Split(' ')[0];
                string iban  = t.CuentaDestino;

                var chip = new Panel { Location = new Point(cx, 4), Size = new Size(88, 32), BackColor = Color.FromArgb(239, 246, 255), Cursor = Cursors.Hand };
                chip.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    bool hov = chip.ClientRectangle.Contains(chip.PointToClient(Cursor.Position));
                    Color bg = hov ? Color.FromArgb(224, 237, 255) : Color.FromArgb(239, 246, 255);
                    using (var path = RR(chip.ClientRectangle, 12)) e.Graphics.FillPath(new SolidBrush(bg), path);
                    using (var path = RR(chip.ClientRectangle, 12)) e.Graphics.DrawPath(new Pen(Color.FromArgb(196, 218, 255)), path);
                    var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    e.Graphics.DrawString(nom, new Font("Segoe UI", 8.5f, FontStyle.Bold), new SolidBrush(Azul), chip.ClientRectangle, fmt);
                };
                chip.MouseEnter += (s, e) => chip.Invalidate();
                chip.MouseLeave += (s, e) => chip.Invalidate();
                string capIBAN = iban;
                string capNom  = t.NombreBeneficiario;
                chip.Click += (s, e) =>
                {
                    _txtIBAN.Text  = capIBAN ?? "";
                    _txtBenef.Text = capNom  ?? "";
                };
                _pnlRecientes.Controls.Add(chip);
                cx += 94;
            }
        }

        private void ActualizarSaldo()
        {
            if (_cmbCuenta.SelectedItem is CI ci)
                _lblSaldo.Text = $"✓  Saldo disponible: {ci.Cuenta.Saldo.ToString("C2", ES)}";
        }

        private void RecalcSaldoTras()
        {
            if (!(_cmbCuenta.SelectedItem is CI ci)) return;
            string ms = _txtMonto.Text.Trim().Replace(",", ".").Replace(" ", "");
            if (!decimal.TryParse(ms, System.Globalization.NumberStyles.Any,
                CultureInfo.InvariantCulture, out decimal m) || m <= 0)
            { _lblSaldoTras.Text = ""; return; }
            decimal tras = ci.Cuenta.Saldo - m;
            _lblSaldoTras.ForeColor = tras >= 0 ? Gris : Rojo;
            _lblSaldoTras.Text = tras >= 0
                ? $"Saldo restante: {tras.ToString("C2", ES)}"
                : "⚠  Saldo insuficiente para este importe";
        }

        // ══════════════════════════════════════════════════════════════
        //  LÓGICA ENVÍO
        // ══════════════════════════════════════════════════════════════
        private void BtnEnviar_Click(object sender, EventArgs e)
        {
            if (!_confirmando) { if (Validar()) EntrarConfirm(); }
            else Ejecutar();
        }

        private bool Validar()
        {
            _lblError.Visible = false;
            if (_cmbCuenta.SelectedItem == null)           { Err("Selecciona la cuenta de origen."); return false; }
            if (string.IsNullOrWhiteSpace(_txtIBAN.Text))  { Err("Introduce el IBAN de destino."); return false; }
            if (_txtIBAN.Text.Trim().Replace(" ", "").Length < 10) { Err("IBAN demasiado corto, verifica."); return false; }
            string ms = _txtMonto.Text.Trim().Replace(",", ".").Replace(" ", "");
            if (!decimal.TryParse(ms, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out decimal m) || m <= 0)
            { Err("Introduce un importe válido mayor que 0 €."); return false; }
            if (m > ((CI)_cmbCuenta.SelectedItem).Cuenta.Saldo) { Err("Saldo insuficiente."); return false; }
            return true;
        }

        private void EntrarConfirm()
        {
            _confirmando = true;
            string ms = _txtMonto.Text.Trim().Replace(",", ".").Replace(" ", "");
            decimal.TryParse(ms, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out decimal m);
            string benef = string.IsNullOrWhiteSpace(_txtBenef.Text)
                ? FormatIBANCorto(_txtIBAN.Text)
                : _txtBenef.Text.Trim();

            _btnEnviar.Text      = $"✓   Confirmar envío de {m.ToString("C2", ES)} a {benef}";
            _btnEnviar.BackColor = Azul;
            _btnEnviar.FlatAppearance.MouseOverBackColor = AzulD;
            _txtIBAN.ReadOnly = _txtMonto.ReadOnly = _txtBenef.ReadOnly = _txtConcepto.ReadOnly = true;
            _cmbCuenta.Enabled = false;
            BF(_pnlIBANBox, Verde); BF2(_pnlMontoBox, Color.FromArgb(220, 252, 231));
        }

        private void SalirConfirm()
        {
            _confirmando = false;
            _btnEnviar.Text      = "💸   Enviar dinero";
            _btnEnviar.BackColor = Verde;
            _btnEnviar.FlatAppearance.MouseOverBackColor = Color.FromArgb(10, 150, 100);
            _txtIBAN.ReadOnly = _txtMonto.ReadOnly = _txtBenef.ReadOnly = _txtConcepto.ReadOnly = false;
            _cmbCuenta.Enabled = true;
            BF(_pnlIBANBox, Border); BF2(_pnlMontoBox, Color.FromArgb(239, 246, 255));
        }

        private void Ejecutar()
        {
            _btnEnviar.Enabled = false; _btnEnviar.Text = "Enviando...";
            string ms = _txtMonto.Text.Trim().Replace(",", ".").Replace(" ", "");
            decimal.TryParse(ms, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out decimal monto);
            var cuenta = ((CI)_cmbCuenta.SelectedItem).Cuenta;
            try
            {
                bool ok = _transSvc.RealizarTransferencia(cuenta.Id,
                    _txtIBAN.Text.Trim(),
                    string.IsNullOrWhiteSpace(_txtBenef.Text) ? SesionActual.Instancia?.Usuario?.NombreCompleto ?? "Titular" : _txtBenef.Text.Trim(),
                    monto, _txtConcepto.Text.Trim(), out string errMsg);

                if (ok)
                {
                    MessageBox.Show($"✅  Transferencia de {monto.ToString("C2", ES)} enviada correctamente.",
                        "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.None);
                    Limpiar();
                    CargarDatos();
                }
                else { Err(errMsg ?? "No se pudo completar."); SalirConfirm(); }
            }
            catch (Exception ex) { Err("Error: " + ex.Message); SalirConfirm(); }
            _btnEnviar.Enabled = true;
        }

        private void Limpiar()
        {
            _txtIBAN.Text = ""; _txtBenef.Text = ""; _txtMonto.Text = "0,00"; _txtConcepto.Text = "";
            _lblError.Visible = false; _lblSaldoTras.Text = "";
            SalirConfirm();
        }

        private void Err(string msg) { _lblError.Text = "⚠  " + msg; _lblError.Visible = true; }

        // ══════════════════════════════════════════════════════════════
        //  HELPERS UI
        // ══════════════════════════════════════════════════════════════
        private static Panel Card(Point loc, Size sz)
        {
            var p = new Panel { Location = loc, Size = sz, BackColor = White };
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(p.ClientRectangle, 14))
                { e.Graphics.FillPath(Brushes.White, path); e.Graphics.DrawPath(new Pen(Border), path); }
            };
            EventHandler ar = (s, e) => { if(p.Width>4&&p.Height>4) try{p.Region=new Region(RR(p.ClientRectangle,14));}catch{} };
            p.HandleCreated += ar; p.Resize += ar;
            return p;
        }

        private static Panel InputBox(Panel parent, Point loc, Size sz, out TextBox txt, string hint)
        {
            var p = new Panel { Location = loc, Size = sz, BackColor = White, Tag = Border };
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color col = p.Tag is Color c ? c : Border;
                using (var path = RR(p.ClientRectangle, 10))
                { e.Graphics.FillPath(Brushes.White, path); e.Graphics.DrawPath(new Pen(col, 1.5f), path); }
            };
            txt = new TextBox
            {
                Location    = new Point(12, (sz.Height - 22) / 2),
                Size        = new Size(sz.Width - 24, 22),
                Font        = new Font("Segoe UI", 10.5f),
                BackColor   = White, ForeColor = Oscuro, BorderStyle = BorderStyle.None
            };
            p.Controls.Add(txt); parent.Controls.Add(p); return p;
        }

        private static void BF(Panel p, Color col) { p.Tag = col; p.Invalidate(); }

        private void BF2(Panel p, Color col)
        {
            p.Tag = col;
            p.Invalidate();
            // Sincronizar BackColor del TextBox hijo (no admite Transparent)
            foreach (Control c in p.Controls)
                if (c is TextBox t) t.BackColor = col;
        }

        private static Panel StepLabel(string num, string texto, Point loc)
        {
            var p = new Panel { Location = loc, Size = new Size(560, 30), BackColor = Color.Transparent };
            p.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                var circ = new Rectangle(0, 3, 24, 24);
                using (var br = new SolidBrush(Azul)) g.FillEllipse(br, circ);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(num, new Font("Segoe UI", 9, FontStyle.Bold), Brushes.White, new RectangleF(0, 3, 24, 24), fmt);
                TextRenderer.DrawText(g, texto, new Font("Segoe UI", 12, FontStyle.Bold),
                    new Rectangle(32, 0, 528, 30), Oscuro, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            };
            return p;
        }

        private static Label FieldLabel(string txt, Point loc)
            => new Label { Text = txt, Location = loc, AutoSize = true, BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Gris };

        private static Panel Sep(int x, int y, int w)
            => new Panel { Location = new Point(x, y), Size = new Size(w, 1), BackColor = Border };

        private static string FormatIBANCorto(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban)) return "—";
            var l = iban.Replace(" ", "");
            return l.Length > 4 ? "••••" + l.Substring(l.Length - 4) : l;
        }

        private static GraphicsPath RR(Rectangle r, int rad)
        {
            var path = new GraphicsPath(); int d = rad * 2;
            if (r.Width < d || r.Height < d) { path.AddRectangle(r); return path; }
            path.AddArc(r.X, r.Y, d, d, 180, 90); path.AddArc(r.Right-d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right-d, r.Bottom-d, d, d, 0, 90); path.AddArc(r.X, r.Bottom-d, d, d, 90, 90);
            path.CloseFigure(); return path;
        }

        private class CI { public CuentaBancaria Cuenta; public string D; public override string ToString() => D; }
    }
}
