using NexumApp.Helpers;
using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace NexumApp.Views
{
    public class VistaPlanPension : UserControl
    {
        // ── Paleta ────────────────────────────────────────────────────
        private static readonly Color C_Oro      = Color.FromArgb(217, 119,  6);
        private static readonly Color C_OroLight = Color.FromArgb(254, 243, 199);
        private static readonly Color C_Verde    = Color.FromArgb(  5, 150, 105);
        private static readonly Color C_Azul     = Color.FromArgb( 37, 99, 235);
        private static readonly Color C_Rojo     = Color.FromArgb(220,  38,  38);

        private Color C_Bg     => AppSettings.ModoOscuro ? Color.FromArgb(10, 12, 28)    : Color.FromArgb(246, 247, 249);
        private Color C_White  => AppSettings.ModoOscuro ? Color.FromArgb(18, 22, 46)    : Color.White;
        private Color C_Border => AppSettings.ModoOscuro ? Color.FromArgb(38, 44, 80)    : Color.FromArgb(220, 224, 230);
        private Color C_Text   => AppSettings.ModoOscuro ? Color.FromArgb(241, 245, 249) : Color.FromArgb(20, 24, 40);
        private Color C_Muted  => AppSettings.ModoOscuro ? Color.FromArgb(100, 116, 139) : Color.FromArgb(107, 114, 128);
        private CultureInfo ES => AppSettings.CultureMoneda;

        // ── Servicios ─────────────────────────────────────────────────
        private readonly CuentaService     _cuentaSvc = new CuentaService();
        private readonly MovimientoService _movSvc    = new MovimientoService();

        // ── Estado ────────────────────────────────────────────────────
        private string  _planSeleccionado = "Moderado";
        private decimal _tasaAnual        = 0.065m;   // Moderado por defecto
        private decimal _totalAportado    = 0m;

        // ── Refs UI ───────────────────────────────────────────────────
        private ComboBox _cmbCuenta;
        private TextBox  _txtAportacion, _txtEdadActual, _txtEdadJubilacion, _txtAportMensual;
        private Label    _lblSaldoCuenta, _lblErrorAport, _lblResAcum, _lblResTasa, _lblResProyec;
        private Label    _lblSimResultado, _lblTotalAportado;
        private Panel    _pnlPlanes;
        private Button   _btnAportar;

        public VistaPlanPension()
        {
            BackColor = C_Bg; Dock = DockStyle.Fill; DoubleBuffered = true;
            AppSettings.ConfiguracionChanged += (s, e) =>
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(new Action(() => { BuildUI(); CargarCuentas(); }));
            };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            BuildUI();
            CargarCuentas();
        }

        // ══════════════════════════════════════════════════════════════
        //  BUILD UI
        // ══════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            Controls.Clear();
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = C_Bg };
            var main   = new Panel { Width = 860, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent };
            scroll.Controls.Add(main);
            scroll.Resize += (s, ev) => main.Left = Math.Max(32, (scroll.ClientSize.Width - 860) / 2);
            Controls.Add(scroll);

            int y = 28;

            // ── CABECERA ─────────────────────────────────────────────
            var icoHdr = MakeIconCircle(new Point(0, 4), 52, C_Oro, "🏦", 22f);
            main.Controls.Add(icoHdr);
            main.Controls.Add(Lbl("Plan de Pensiones", new Font("Segoe UI", 20, FontStyle.Bold), C_Text, new Point(68, 6)));
            main.Controls.Add(Lbl("Ahorra hoy para vivir mejor mañana · Sin comisiones de gestión", new Font("Segoe UI", 10), C_Muted, new Point(68, 38)));
            y += 72;

            // ── BANNER AVISO ─────────────────────────────────────────
            var banner = new Panel { Location = new Point(0, y), Size = new Size(840, 44), BackColor = C_OroLight };
            Redondear(banner, 10);
            banner.Controls.Add(Lbl("⚠  Las proyecciones son estimativas. La rentabilidad real puede variar. Consulta con un asesor financiero.",
                new Font("Segoe UI", 9, FontStyle.Italic), C_Oro, new Point(16, 14)));
            main.Controls.Add(banner); y += 58;

            // ── RESUMEN 3 ESTADÍSTICAS ────────────────────────────────
            var cardResumen = MakeCard(new Point(0, y), new Size(840, 100));
            main.Controls.Add(cardResumen); y += 114;

            // Calcular total aportado consultando movimientos del usuario
            CalcularTotalAportado();

            decimal tasaMsg    = _tasaAnual * 100m;
            decimal proyectado = _totalAportado * (1 + _tasaAnual);

            // Stat 1 — Total aportado
            AgregarStat(cardResumen, new Point(30, 0), "TOTAL APORTADO",
                _totalAportado.ToString("C2", ES), C_Oro, ref _lblTotalAportado);

            // Stat 2 — Tasa estimada del plan
            Label lblTasaTemp = null;
            AgregarStat(cardResumen, new Point(310, 0), "TASA ANUAL ESTIMADA",
                $"{tasaMsg:F1}%", C_Verde, ref lblTasaTemp);
            _lblResTasa = lblTasaTemp;

            // Stat 3 — Proyección a 1 año
            Label lblProyTemp = null;
            AgregarStat(cardResumen, new Point(590, 0), "PROYECCIÓN 1 AÑO",
                proyectado.ToString("C2", ES), C_Azul, ref lblProyTemp);
            _lblResProyec = lblProyTemp;

            // Dividers
            cardResumen.Controls.Add(new Panel { Location = new Point(295, 20), Size = new Size(1, 60), BackColor = C_Border });
            cardResumen.Controls.Add(new Panel { Location = new Point(575, 20), Size = new Size(1, 60), BackColor = C_Border });

            // ── SELECTOR DE PLAN ──────────────────────────────────────
            main.Controls.Add(SectionLabel("Elige tu plan", new Point(0, y))); y += 34;

            _pnlPlanes = new Panel { Location = new Point(0, y), Size = new Size(840, 106), BackColor = Color.Transparent };
            main.Controls.Add(_pnlPlanes); y += 118;

            CrearCardPlan("Conservador", "Riesgo muy bajo · Renta fija europea", 3.5m,
                Color.FromArgb(5, 150, 105), "✔  Ideal para jubilación próxima", new Point(0, 0));
            CrearCardPlan("Moderado", "Riesgo moderado · Mixto acciones/bonos", 6.5m,
                C_Azul, "✔  Equilibrio entre seguridad y crecimiento", new Point(282, 0));
            CrearCardPlan("Arriesgado", "Riesgo alto · Renta variable global", 9.0m,
                C_Rojo, "✔  Máximo crecimiento a largo plazo", new Point(564, 0));

            SeleccionarPlan("Moderado", 0.065m);

            // ── SIMULADOR ─────────────────────────────────────────────
            main.Controls.Add(SectionLabel("Simulador de jubilación", new Point(0, y))); y += 34;

            var cardSim = MakeCard(new Point(0, y), new Size(840, 220));
            main.Controls.Add(cardSim); y += 234;

            int sx = 24, sy = 18;
            cardSim.Controls.Add(FieldLbl("EDAD ACTUAL", sx, sy));
            cardSim.Controls.Add(FieldLbl("EDAD DE JUBILACIÓN", sx + 190, sy));
            cardSim.Controls.Add(FieldLbl("APORTACIÓN MENSUAL (€)", sx + 380, sy));
            sy += 20;

            _txtEdadActual     = InputNum(cardSim, new Point(sx, sy), new Size(160, 38), "30");
            _txtEdadJubilacion = InputNum(cardSim, new Point(sx + 190, sy), new Size(160, 38), "65");
            _txtAportMensual   = InputNum(cardSim, new Point(sx + 380, sy), new Size(200, 38), "150");
            sy += 48;

            var btnSim = new Button
            {
                Text = "Calcular proyección  →",
                Location = new Point(sx + 600, sy - 48),
                Size = new Size(200, 38),
                BackColor = C_Oro, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnSim.FlatAppearance.BorderSize = 0;
            btnSim.MouseEnter += (s, e) => btnSim.BackColor = Color.FromArgb(180, 100, 4);
            btnSim.MouseLeave += (s, e) => btnSim.BackColor = C_Oro;
            btnSim.Click += (s, e) => EjecutarSimulador(cardSim);
            cardSim.Controls.Add(btnSim);
            BeginInvoke(new Action(() => { Redondear(btnSim, 8); }));

            // Separador
            cardSim.Controls.Add(new Panel { Location = new Point(sx, sy), Size = new Size(792, 1), BackColor = C_Border }); sy += 14;

            // Resultado simulación
            _lblSimResultado = Lbl("  Introduce tus datos y pulsa «Calcular proyección»",
                new Font("Segoe UI", 10, FontStyle.Italic), C_Muted, new Point(sx, sy));
            _lblSimResultado.Size = new Size(792, 110); _lblSimResultado.TextAlign = ContentAlignment.MiddleLeft;
            cardSim.Controls.Add(_lblSimResultado);
            cardSim.Height = sy + 120;

            // ── APORTAR AHORA ─────────────────────────────────────────
            main.Controls.Add(SectionLabel("Realizar aportación", new Point(0, y))); y += 34;

            var cardAport = MakeCard(new Point(0, y), new Size(840, 178));
            main.Controls.Add(cardAport); y += 192;

            int ay = 18;
            cardAport.Controls.Add(FieldLbl("CUENTA DE CARGO", 24, ay));
            cardAport.Controls.Add(FieldLbl("IMPORTE (€)", 420, ay)); ay += 20;

            _cmbCuenta = new ComboBox
            {
                Location = new Point(24, ay), Size = new Size(370, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10), BackColor = C_White, ForeColor = C_Text, FlatStyle = FlatStyle.Flat
            };
            _cmbCuenta.SelectedIndexChanged += (s, e) => ActualizarSaldoCuenta();
            cardAport.Controls.Add(_cmbCuenta);

            Panel pAport;
            pAport = InputBoxInline(cardAport, new Point(420, ay), new Size(220, 38), out _txtAportacion, "0,00");
            _txtAportacion.Font = new Font("Segoe UI", 14, FontStyle.Bold); _txtAportacion.ForeColor = C_Oro;
            _txtAportacion.GotFocus  += (s, e) => { if (_txtAportacion.Text == "0,00") _txtAportacion.Clear(); FocusBox(pAport, C_Oro); };
            _txtAportacion.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(_txtAportacion.Text)) _txtAportacion.Text = "0,00"; FocusBox(pAport, C_Border); };
            _txtAportacion.TextChanged += (s, e) => _lblErrorAport.Visible = false;
            ay += 48;

            _lblSaldoCuenta = Lbl("", new Font("Segoe UI", 8, FontStyle.Bold), C_Verde, new Point(24, ay));
            _lblSaldoCuenta.AutoSize = true;
            cardAport.Controls.Add(_lblSaldoCuenta);

            _lblErrorAport = Lbl("", new Font("Segoe UI", 8, FontStyle.Bold), C_Rojo, new Point(24, ay));
            _lblErrorAport.Size = new Size(800, 16); _lblErrorAport.Visible = false;
            cardAport.Controls.Add(_lblErrorAport);
            ay += 22;

            // Chips importes rápidos
            foreach (var imp in new[] { 50m, 100m, 200m, 500m })
            {
                decimal cap = imp;
                var chip = new Button
                {
                    Text = cap.ToString("C0", ES), Size = new Size(76, 28),
                    BackColor = C_OroLight, ForeColor = C_Oro,
                    FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
                };
                chip.FlatAppearance.BorderColor = Color.FromArgb(252, 211, 77);
                chip.FlatAppearance.BorderSize  = 1;
                chip.Location = new Point(24 + (int)((imp / 50m - 1) * 82), ay);
                chip.Click += (s, e) => { _txtAportacion.Text = cap.ToString("N2", ES); _lblErrorAport.Visible = false; };
                BeginInvoke(new Action(() => Redondear(chip, 6)));
                cardAport.Controls.Add(chip);
            }
            ay += 38;

            _btnAportar = new Button
            {
                Text = "Aportar al plan  🏦",
                Location = new Point(24, ay), Size = new Size(200, 42),
                BackColor = C_Oro, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand
            };
            _btnAportar.FlatAppearance.BorderSize = 0;
            _btnAportar.MouseEnter += (s, e) => _btnAportar.BackColor = Color.FromArgb(180, 100, 4);
            _btnAportar.MouseLeave += (s, e) => _btnAportar.BackColor = C_Oro;
            _btnAportar.Click += BtnAportar_Click;
            cardAport.Controls.Add(_btnAportar);
            BeginInvoke(new Action(() => { Redondear(_btnAportar, 8); Redondear(pAport, 8); }));
            cardAport.Height = ay + 56;

            // ── AVISO LEGAL ───────────────────────────────────────────
            var lblLegal = Lbl(
                "🔒  Nexum Bank · Plan de Pensiones regulado conforme a la normativa española de planes y fondos de pensiones (RD 304/2004).\n" +
                "     Las aportaciones están protegidas hasta el límite legal. El rescate anticipado puede estar sujeto a penalizaciones fiscales.",
                new Font("Segoe UI", 8, FontStyle.Italic), C_Muted, new Point(0, y));
            lblLegal.Size = new Size(840, 38);
            main.Controls.Add(lblLegal); y += 50;

            main.Height = y + 20;

            BeginInvoke(new Action(() =>
                main.Left = Math.Max(32, (scroll.ClientSize.Width - 860) / 2)));
        }

        // ══════════════════════════════════════════════════════════════
        //  PLANES
        // ══════════════════════════════════════════════════════════════
        private void CrearCardPlan(string nombre, string desc, decimal tasaPct, Color col, string ventaja, Point loc)
        {
            var card = new Panel { Location = loc, Size = new Size(270, 100), BackColor = C_White, Cursor = Cursors.Hand, Tag = nombre };
            card.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                bool sel = _planSeleccionado == nombre;
                using (var path = RR(card.ClientRectangle, 12))
                {
                    ev.Graphics.FillPath(new SolidBrush(sel ? Color.FromArgb(20, col.R, col.G, col.B) : C_White), path);
                    ev.Graphics.DrawPath(new Pen(sel ? col : C_Border, sel ? 2.5f : 1f), path);
                }
                using (var br = new SolidBrush(col))
                    ev.Graphics.FillRectangle(br, new Rectangle(0, 0, 6, card.Height));
            };
            Redondear(card, 12);

            var lblNombre = Lbl(nombre, new Font("Segoe UI", 11, FontStyle.Bold), C_Text, new Point(16, 10));
            var lblDesc   = Lbl(desc,   new Font("Segoe UI",  8),                 C_Muted, new Point(16, 32));
            lblDesc.Size  = new Size(240, 28);
            var lblTasa   = Lbl($"+{tasaPct:F1}% / año", new Font("Segoe UI", 13, FontStyle.Bold), col, new Point(16, 62));
            var lblVent   = Lbl(ventaja, new Font("Segoe UI", 7, FontStyle.Italic), C_Muted, new Point(16, 82));

            foreach (Control c in new Control[] { card, lblNombre, lblDesc, lblTasa, lblVent })
            {
                c.Cursor = Cursors.Hand;
                c.Click += (s, e) => SeleccionarPlan(nombre, tasaPct / 100m);
                if (c is Label lc) { lc.BackColor = Color.Transparent; card.Controls.Add(lc); }
            }
            _pnlPlanes.Controls.Add(card);
        }

        private void SeleccionarPlan(string nombre, decimal tasa)
        {
            _planSeleccionado = nombre;
            _tasaAnual        = tasa;
            foreach (Panel c in _pnlPlanes.Controls) c.Invalidate();

            if (_lblResTasa  != null) _lblResTasa.Text  = $"{tasa * 100:F1}%";
            if (_lblResProyec != null) _lblResProyec.Text = (_totalAportado * (1 + tasa)).ToString("C2", ES);
        }

        // ══════════════════════════════════════════════════════════════
        //  SIMULADOR
        // ══════════════════════════════════════════════════════════════
        private void EjecutarSimulador(Panel cardSim)
        {
            if (!int.TryParse(_txtEdadActual.Text.Trim(), out int edadAct) || edadAct < 18 || edadAct > 80)
            { _lblSimResultado.Text = "⚠  Edad actual inválida (18–80)."; _lblSimResultado.ForeColor = C_Rojo; return; }

            if (!int.TryParse(_txtEdadJubilacion.Text.Trim(), out int edadJub) || edadJub <= edadAct || edadJub > 100)
            { _lblSimResultado.Text = "⚠  Edad de jubilación debe ser mayor que la actual (máx. 100)."; _lblSimResultado.ForeColor = C_Rojo; return; }

            string ms = _txtAportMensual.Text.Trim().Replace(",", ".").Replace(" ", "");
            if (!decimal.TryParse(ms, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal aportMes) || aportMes <= 0)
            { _lblSimResultado.Text = "⚠  Introduce una aportación mensual válida mayor que 0 €."; _lblSimResultado.ForeColor = C_Rojo; return; }

            int     meses      = (edadJub - edadAct) * 12;
            decimal tasaMes    = _tasaAnual / 12m;
            decimal factor     = (decimal)Math.Pow((double)(1 + tasaMes), meses);
            decimal capitalFV  = aportMes * (factor - 1m) / tasaMes;
            decimal totalAport = aportMes * meses;
            decimal ganancia   = capitalFV - totalAport;
            decimal mensual65  = capitalFV / 240m; // estimación 20 años de renta

            _lblSimResultado.Font      = new Font("Segoe UI", 10, FontStyle.Bold);
            _lblSimResultado.ForeColor = C_Text;
            _lblSimResultado.Text =
                $"📊  Plan: {_planSeleccionado}  ·  Tasa anual: {_tasaAnual * 100:F1}%  ·  Duración: {edadJub - edadAct} años ({meses} meses)\n\n" +
                $"💰  Capital acumulado proyectado:  {capitalFV.ToString("C2", ES)}  " +
                $"(aportado: {totalAport.ToString("C2", ES)}  ·  ganancia estimada: {ganancia.ToString("C2", ES)})\n\n" +
                $"🏖  Renta mensual estimada a los {edadJub} años:  {mensual65.ToString("C2", ES)} / mes  (durante 20 años)";
        }

        // ══════════════════════════════════════════════════════════════
        //  APORTACIÓN
        // ══════════════════════════════════════════════════════════════
        private void BtnAportar_Click(object sender, EventArgs e)
        {
            _lblErrorAport.Visible = false;
            if (_cmbCuenta.SelectedItem == null) { Err("Selecciona una cuenta de cargo."); return; }

            string ms = _txtAportacion.Text.Trim().Replace(",", ".").Replace(" ", "");
            if (!decimal.TryParse(ms, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal monto) || monto <= 0)
            { Err("Introduce un importe válido mayor que 0 €."); return; }

            var ci = (CuentaItem)_cmbCuenta.SelectedItem;
            if (monto > ci.Cuenta.Saldo) { Err("Saldo insuficiente en la cuenta seleccionada."); return; }

            string ult4 = ci.Cuenta.NumeroCuenta?.Length > 4
                ? ci.Cuenta.NumeroCuenta.Substring(ci.Cuenta.NumeroCuenta.Length - 4)
                : ci.Cuenta.NumeroCuenta;

            var resp = MessageBox.Show(
                $"¿Confirmar aportación de {monto.ToString("C2", ES)} al Plan de Pensiones?\n\n" +
                $"Plan:    {_planSeleccionado}  ({_tasaAnual * 100:F1}% anual estimado)\n" +
                $"Cuenta:  ••••{ult4}\n\n" +
                "⚠  El rescate anticipado puede estar sujeto a penalizaciones fiscales.",
                "Nexum Bank — Aportación a Plan de Pensiones",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (resp != DialogResult.Yes) return;

            _btnAportar.Enabled = false;
            _btnAportar.Text    = "Procesando...";

            bool ok = _movSvc.RegistrarRetiro(ci.Cuenta.Id, monto,
                $"Aportación Plan de Pensiones – {_planSeleccionado}", out string errMsg);

            if (ok)
            {
                _totalAportado += monto;
                if (_lblTotalAportado != null) _lblTotalAportado.Text = _totalAportado.ToString("C2", ES);
                if (_lblResProyec    != null) _lblResProyec.Text     = (_totalAportado * (1 + _tasaAnual)).ToString("C2", ES);

                MessageBox.Show(
                    $"✅  Aportación de {monto.ToString("C2", ES)} realizada correctamente.\n\n" +
                    $"Tu plan '{_planSeleccionado}' sigue creciendo.",
                    "Nexum Bank — Aportación confirmada",
                    MessageBoxButtons.OK, MessageBoxIcon.None);

                _txtAportacion.Text = "0,00";
                CargarCuentas();
            }
            else Err(errMsg ?? "No se pudo procesar la aportación.");

            _btnAportar.Enabled = true;
            _btnAportar.Text    = "Aportar al plan  🏦";
        }

        // ══════════════════════════════════════════════════════════════
        //  DATOS
        // ══════════════════════════════════════════════════════════════
        private void CargarCuentas()
        {
            _cmbCuenta?.Items.Clear();
            if (SesionActual.Instancia?.Usuario == null) return;
            foreach (var c in _cuentaSvc.ObtenerCuentasPorUsuario(SesionActual.Instancia.Usuario.Id))
            {
                string u = c.NumeroCuenta?.Length > 4
                    ? c.NumeroCuenta.Substring(c.NumeroCuenta.Length - 4)
                    : c.NumeroCuenta;
                _cmbCuenta?.Items.Add(new CuentaItem { Cuenta = c, Display = $"{c.TipoCuenta}  •  ••••{u}  —  {c.Saldo.ToString("C2", ES)}" });
            }
            if (_cmbCuenta != null) _cmbCuenta.DisplayMember = "Display";
            if (_cmbCuenta?.Items.Count > 0) _cmbCuenta.SelectedIndex = 0;
            ActualizarSaldoCuenta();
        }

        private void CalcularTotalAportado()
        {
            _totalAportado = 0m;
            try
            {
                if (SesionActual.Instancia?.Usuario == null) return;
                var cuentas = _cuentaSvc.ObtenerCuentasPorUsuario(SesionActual.Instancia.Usuario.Id);
                foreach (var cuenta in cuentas)
                {
                    var movs = _movSvc.ObtenerMovimientosPorCuenta(cuenta.Id, 200);
                    if (movs == null) continue;
                    foreach (var m in movs)
                        if (m.Concepto != null && m.Concepto.Contains("Plan de Pensiones"))
                            _totalAportado += m.Monto;
                }
            }
            catch { }
        }

        private void ActualizarSaldoCuenta()
        {
            if (_cmbCuenta?.SelectedItem is CuentaItem ci && _lblSaldoCuenta != null)
                _lblSaldoCuenta.Text = $"✓  Saldo disponible: {ci.Cuenta.Saldo.ToString("C2", ES)}";
        }

        // ══════════════════════════════════════════════════════════════
        //  HELPERS UI
        // ══════════════════════════════════════════════════════════════
        private void AgregarStat(Panel parent, Point loc, string titulo, string valor, Color col, ref Label lblRef)
        {
            var pnl   = new Panel { Location = loc, Size = new Size(250, 100), BackColor = Color.Transparent };
            var lTit  = Lbl(titulo, new Font("Segoe UI", 8, FontStyle.Bold), C_Muted, new Point(0, 20));
            var lVal  = Lbl(valor,  new Font("Segoe UI", 18, FontStyle.Bold), col,    new Point(0, 40));
            lVal.AutoSize = true;
            pnl.Controls.Add(lTit);
            pnl.Controls.Add(lVal);
            parent.Controls.Add(pnl);
            lblRef = lVal;
        }

        private Panel MakeCard(Point loc, Size sz)
        {
            var p = new Panel { Location = loc, Size = sz, BackColor = C_White };
            p.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(p.ClientRectangle, 12))
                {
                    ev.Graphics.FillPath(new SolidBrush(C_White), path);
                    ev.Graphics.DrawPath(new Pen(C_Border, 1), path);
                }
            };
            BeginInvoke(new Action(() => Redondear(p, 12)));
            return p;
        }

        private Panel InputBoxInline(Panel parent, Point loc, Size sz, out TextBox txt, string hint)
        {
            var p = new Panel { Location = loc, Size = sz, BackColor = C_White, Tag = C_Border };
            p.Paint += (s, ev) =>
            {
                var col = p.Tag is Color c ? c : C_Border;
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(p.ClientRectangle, 8))
                {
                    ev.Graphics.FillPath(new SolidBrush(C_White), path);
                    ev.Graphics.DrawPath(new Pen(col, 1.5f), path);
                }
            };
            txt = new TextBox
            {
                Location    = new Point(10, (sz.Height - 22) / 2),
                Size        = new Size(sz.Width - 20, 24),
                Font        = new Font("Segoe UI", 10),
                BackColor   = C_White, ForeColor = C_Text, BorderStyle = BorderStyle.None
            };
            p.Controls.Add(txt);
            parent.Controls.Add(p);
            return p;
        }

        private TextBox InputNum(Panel parent, Point loc, Size sz, string defVal)
        {
            var pnl = new Panel { Location = loc, Size = sz, BackColor = C_White, Tag = C_Border };
            pnl.Paint += (s, ev) =>
            {
                var col = pnl.Tag is Color c ? c : C_Border;
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(pnl.ClientRectangle, 8))
                {
                    ev.Graphics.FillPath(new SolidBrush(C_White), path);
                    ev.Graphics.DrawPath(new Pen(col, 1.5f), path);
                }
            };
            var txt = new TextBox
            {
                Text        = defVal,
                Location    = new Point(10, (sz.Height - 22) / 2),
                Size        = new Size(sz.Width - 20, 24),
                Font        = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor   = C_Text, BackColor = C_White, BorderStyle = BorderStyle.None,
                TextAlign   = HorizontalAlignment.Center
            };
            txt.GotFocus  += (s, e) => FocusBox(pnl, C_Oro);
            txt.LostFocus += (s, e) => FocusBox(pnl, C_Border);
            pnl.Controls.Add(txt);
            parent.Controls.Add(pnl);
            BeginInvoke(new Action(() => Redondear(pnl, 8)));
            return txt;
        }

        private Panel MakeIconCircle(Point loc, int size, Color col, string g, float fs)
        {
            var p = new Panel { Location = loc, Size = new Size(size, size), BackColor = Color.Transparent };
            p.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new LinearGradientBrush(p.ClientRectangle,
                    Color.FromArgb(255, col.R, col.G, col.B),
                    Color.FromArgb(200, col.R, col.G, col.B), 135f))
                    ev.Graphics.FillEllipse(br, p.ClientRectangle);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString(g, new Font("Segoe UI Emoji", fs), Brushes.White, p.ClientRectangle, fmt);
            };
            return p;
        }

        private static Label Lbl(string text, Font font, Color col, Point loc)
            => new Label { Text = text, Font = font, ForeColor = col, Location = loc, AutoSize = true, BackColor = Color.Transparent };

        private Label SectionLabel(string text, Point loc)
        {
            var lbl = new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = C_Text,
                Location  = loc,
                AutoSize  = true,
                BackColor = Color.Transparent
            };
            return lbl;
        }

        private Label FieldLbl(string t, int x, int y)
            => new Label { Text = t, Location = new Point(x, y), ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };

        private void FocusBox(Panel p, Color col) { p.Tag = col; p.Invalidate(); }

        private void Err(string msg)
        {
            if (_lblErrorAport != null) { _lblErrorAport.Text = "⚠  " + msg; _lblErrorAport.Visible = true; }
        }

        private static GraphicsPath RR(Rectangle r, int rad)
        {
            int d = rad * 2;
            if (r.Width < d || r.Height < d) { var pp = new GraphicsPath(); pp.AddRectangle(r); return pp; }
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseAllFigures();
            return path;
        }

        private static void Redondear(Control c, int r)
        {
            try { if (c.Width > 0 && c.Height > 0) c.Region = new Region(RR(c.ClientRectangle, r)); } catch { }
        }

        private class CuentaItem { public CuentaBancaria Cuenta; public string Display; public override string ToString() => Display; }
    }
}
