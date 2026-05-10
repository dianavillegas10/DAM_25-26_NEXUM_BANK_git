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
    public class VistaDividirCuenta : UserControl
    {
        private static readonly Color C_Red     = Color.FromArgb(236, 0, 0);
        private static readonly Color C_RedDark = Color.FromArgb(180, 0, 0);
        private static readonly Color C_BgPage  = Color.FromArgb(246, 247, 249);
        private static readonly Color C_White   = Color.White;
        private static readonly Color C_Border  = Color.FromArgb(220, 224, 230);
        private static readonly Color C_Text    = Color.FromArgb(30, 30, 50);
        private static readonly Color C_Muted   = Color.FromArgb(110, 120, 140);
        private static readonly Color C_Green   = Color.FromArgb(0, 168, 89);
        private static readonly CultureInfo ES  = CultureInfo.CreateSpecificCulture("es-ES");

        private readonly CuentaService     _cuentaService = new CuentaService();
        private readonly MovimientoService _movService    = new MovimientoService();

        private TextBox  _txtConcepto, _txtTotal;
        private NumericUpDown _nudPersonas;
        private Panel    _pnlPersonas, _pnlResultado;
        private ComboBox _cmbCuenta;
        private Label    _lblSaldo, _lblError;
        private Button   _btnCalcular, _btnPagar;
        private List<(TextBox Nombre, TextBox Iban)> _participantes = new List<(TextBox, TextBox)>();
        private decimal  _parteCalculada = 0;

        public VistaDividirCuenta() { BackColor = C_BgPage; Dock = DockStyle.Fill; DoubleBuffered = true; }
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); BuildUI(); CargarCuentas(); }

        private void BuildUI()
        {
            Controls.Clear();
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = C_BgPage };
            var main   = new Panel { Width = 860, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent };
            scroll.Controls.Add(main);
            scroll.Resize += (s, ev) => { main.Left = Math.Max(32, (scroll.ClientSize.Width - 860) / 2); };
            Controls.Add(scroll);
            int y = 28;

            // Cabecera
            var icoHdr = MakeIconCircle(new Point(0, 6), 48, C_Red, "👥", 20f);
            main.Controls.Add(icoHdr);
            main.Controls.Add(new Label { Text = "Dividir cuenta", ForeColor = C_Text, Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(62, 8), AutoSize = true });
            main.Controls.Add(new Label { Text = "Reparte gastos con amigos y familia de forma justa", ForeColor = C_Muted, Font = new Font("Segoe UI", 10), Location = new Point(62, 38), AutoSize = true }); y += 76;

            // Card cuenta pago
            var cardCuenta = MakeCard(new Point(0, y), new Size(840, 86)); main.Controls.Add(cardCuenta); y += 100;
            cardCuenta.Controls.Add(FieldLbl("CUENTA DE CARGO (tu parte)", 20, 14));
            _cmbCuenta = new ComboBox { Location = new Point(20, 34), Size = new Size(500, 28),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10),
                BackColor = C_White, ForeColor = C_Text, FlatStyle = FlatStyle.Flat };
            _cmbCuenta.SelectedIndexChanged += (s, ev) => ActualizarSaldo();
            cardCuenta.Controls.Add(_cmbCuenta);
            _lblSaldo = new Label { Location = new Point(20, 66), AutoSize = true, ForeColor = C_Green, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            cardCuenta.Controls.Add(_lblSaldo);

            // Card datos del gasto
            var cardGasto = MakeCard(new Point(0, y), new Size(840, 130)); main.Controls.Add(cardGasto); y += 144;
            cardGasto.Controls.Add(new Label { Text = "Datos del gasto", ForeColor = C_Text, Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(20, 14), AutoSize = true });
            cardGasto.Controls.Add(new Panel { Location = new Point(20, 38), Size = new Size(800, 1), BackColor = C_Border });
            cardGasto.Controls.Add(FieldLbl("CONCEPTO", 20, 50));
            cardGasto.Controls.Add(FieldLbl("IMPORTE TOTAL (€)", 370, 50));
            cardGasto.Controls.Add(FieldLbl("Nº PERSONAS", 620, 50));

            var pConc = InputBoxInline(cardGasto, new Point(20, 70), new Size(330, 40), out _txtConcepto, "Ej: Cena, Viaje, Renta...");
            var pTot  = InputBoxInline(cardGasto, new Point(370, 70), new Size(230, 40), out _txtTotal, "0,00");
            _txtTotal.Font = new Font("Segoe UI", 14, FontStyle.Bold); _txtTotal.ForeColor = C_Red;
            _txtTotal.GotFocus  += (s, e) => { if (_txtTotal.Text == "0,00") _txtTotal.Clear(); FocusBox(pTot, C_Red); };
            _txtTotal.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(_txtTotal.Text)) _txtTotal.Text = "0,00"; FocusBox(pTot, C_Border); };
            _txtConcepto.GotFocus  += (s, e) => FocusBox(pConc, C_Red);
            _txtConcepto.LostFocus += (s, e) => FocusBox(pConc, C_Border);

            _nudPersonas = new NumericUpDown { Location = new Point(620, 70), Size = new Size(100, 40),
                Minimum = 2, Maximum = 20, Value = 2, Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BackColor = C_White, ForeColor = C_Red, TextAlign = HorizontalAlignment.Center };
            _nudPersonas.ValueChanged += (s, e) => { GenerarParticipantes(); ActualizarPreviewParte(); };
            cardGasto.Controls.Add(_nudPersonas);

            // Preview "por persona" en tiempo real
            var lblPreview = new Label { Location = new Point(736, 78), AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = C_Muted };
            cardGasto.Controls.Add(lblPreview);
            void RefreshPreview()
            {
                string ms2 = _txtTotal.Text.Trim().Replace(",", ".").Replace(" ", "");
                if (decimal.TryParse(ms2, System.Globalization.NumberStyles.Any,
                    CultureInfo.InvariantCulture, out decimal tot2) && tot2 > 0)
                {
                    int n2 = (int)_nudPersonas.Value;
                    decimal parte2 = Math.Round(tot2 / n2, 2);
                    lblPreview.Text = $"= {parte2.ToString("C2", ES)}/persona";
                    lblPreview.ForeColor = C_Red;
                }
                else { lblPreview.Text = ""; }
            }
            _txtTotal.TextChanged += (s, e) => RefreshPreview();
            _nudPersonas.ValueChanged += (s, e) => RefreshPreview();
            BeginInvoke(new Action(() => { Redondear(pConc, 8); Redondear(pTot, 8); }));

            // Card participantes
            main.Controls.Add(new Label { Text = "Participantes", ForeColor = C_Text, Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(0, y), AutoSize = true }); y += 30;
            _pnlPersonas = MakeCard(new Point(0, y), new Size(840, 100)); main.Controls.Add(_pnlPersonas); y += 114;

            // Botón calcular
            _btnCalcular = new Button { Text = "Calcular división", Location = new Point(0, y), Size = new Size(200, 46),
                BackColor = C_Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            _btnCalcular.FlatAppearance.BorderSize = 0;
            _btnCalcular.MouseEnter += (s, e) => _btnCalcular.BackColor = C_RedDark;
            _btnCalcular.MouseLeave += (s, e) => _btnCalcular.BackColor = C_Red;
            _btnCalcular.Click += BtnCalcular_Click;
            Redondear(_btnCalcular, 8); main.Controls.Add(_btnCalcular); y += 60;

            // Panel resultado
            _pnlResultado = MakeCard(new Point(0, y), new Size(840, 200)); _pnlResultado.Visible = false;
            main.Controls.Add(_pnlResultado); y += 214;

            main.Height = y + 20;
            GenerarParticipantes();
        }

        private void GenerarParticipantes()
        {
            if (_pnlPersonas == null) return;
            _pnlPersonas.Controls.Clear();
            _participantes.Clear();
            int n = (int)_nudPersonas.Value;
            int pnlH = 20 + n * 52 + 16;
            _pnlPersonas.Height = pnlH;

            _pnlPersonas.Controls.Add(FieldLbl("NOMBRE DEL PARTICIPANTE", 20, 14));
            _pnlPersonas.Controls.Add(FieldLbl("IBAN / CUENTA (opcional)", 330, 14));

            // Fila del usuario actual
            int iy = 34;
            string nombreUsuario = SesionActual.Instancia?.Usuario?.NombreCompleto ?? "Tú";
            _pnlPersonas.Controls.Add(new Label { Text = $"👤  {nombreUsuario} (tú)", ForeColor = C_Text, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, iy + 6), AutoSize = true });
            _pnlPersonas.Controls.Add(new Label { Text = "Tu cuenta seleccionada", ForeColor = C_Muted, Font = new Font("Segoe UI", 9, FontStyle.Italic), Location = new Point(330, iy + 6), AutoSize = true });
            iy += 40;

            for (int i = 1; i < n; i++)
            {
                var pNom = InputBoxInline(_pnlPersonas, new Point(20, iy), new Size(300, 38), out TextBox txtNom, $"Participante {i + 1}");
                var pIba = InputBoxInline(_pnlPersonas, new Point(330, iy), new Size(490, 38), out TextBox txtIba, "ES00 0000 0000 0000 0000 0000 (opcional)");
                txtNom.GotFocus  += (s, e) => FocusBox(pNom, C_Red);
                txtNom.LostFocus += (s, e) => FocusBox(pNom, C_Border);
                txtIba.GotFocus  += (s, e) => FocusBox(pIba, C_Red);
                txtIba.LostFocus += (s, e) => FocusBox(pIba, C_Border);
                _participantes.Add((txtNom, txtIba));
                BeginInvoke(new Action(() => { Redondear(pNom, 8); Redondear(pIba, 8); }));
                iy += 48;
            }
            _pnlPersonas.Height = iy + 16;
        }

        private void BtnCalcular_Click(object sender, EventArgs e)
        {
            _pnlResultado.Controls.Clear();
            string ms = _txtTotal.Text.Trim().Replace(",", ".").Replace(" ", "");
            if (!decimal.TryParse(ms, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out decimal total) || total <= 0)
            { MostrarError("Introduce el importe total del gasto."); return; }
            if (string.IsNullOrWhiteSpace(_txtConcepto.Text)) { MostrarError("Introduce el concepto del gasto."); return; }

            int n = (int)_nudPersonas.Value;
            decimal parte = Math.Round(total / n, 2);
            decimal ajuste = total - parte * n;
            _parteCalculada = parte;

            // Construir panel resultado
            _pnlResultado.Visible = true;
            int ry = 16;
            _pnlResultado.Controls.Add(new Label { Text = "✓  División calculada", ForeColor = C_Green, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(20, ry), AutoSize = true }); ry += 32;
            _pnlResultado.Controls.Add(new Panel { Location = new Point(20, ry), Size = new Size(800, 1), BackColor = C_Border }); ry += 14;

            // KPIs resultado
            string concepto = _txtConcepto.Text.Trim();
            _pnlResultado.Controls.Add(new Label { Text = $"Concepto: {concepto}", ForeColor = C_Muted, Font = new Font("Segoe UI", 9), Location = new Point(20, ry), AutoSize = true }); ry += 22;
            _pnlResultado.Controls.Add(new Label { Text = $"Total:  {total.ToString("C2", ES)}   ÷   {n} personas", ForeColor = C_Text, Font = new Font("Segoe UI", 11), Location = new Point(20, ry), AutoSize = true }); ry += 30;
            _pnlResultado.Controls.Add(new Label { Text = $"Parte por persona:  {parte.ToString("C2", ES)}", ForeColor = C_Red, Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, ry), AutoSize = true }); ry += 36;

            if (Math.Abs(ajuste) > 0)
            {
                _pnlResultado.Controls.Add(new Label { Text = $"(Diferencia de redondeo {ajuste.ToString("C2", ES)} asignada al primer participante)", ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Italic), Location = new Point(20, ry), AutoSize = true }); ry += 22;
            }

            _pnlResultado.Controls.Add(new Panel { Location = new Point(20, ry), Size = new Size(800, 1), BackColor = C_Border }); ry += 12;

            // Botón pagar tu parte
            _lblError = new Label { Location = new Point(230, ry + 10), Size = new Size(580, 16), ForeColor = C_Red, Font = new Font("Segoe UI", 8, FontStyle.Bold), Visible = false };
            _pnlResultado.Controls.Add(_lblError);

            _btnPagar = new Button { Text = $"Pagar mi parte ({parte.ToString("C2", ES)})", Location = new Point(20, ry), Size = new Size(220, 44),
                BackColor = C_Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            _btnPagar.FlatAppearance.BorderSize = 0;
            _btnPagar.MouseEnter += (s, e2) => _btnPagar.BackColor = C_RedDark;
            _btnPagar.MouseLeave += (s, e2) => _btnPagar.BackColor = C_Red;
            _btnPagar.Click += BtnPagarParte_Click;
            _pnlResultado.Controls.Add(_btnPagar);
            Redondear(_btnPagar, 8);

            var btnCopiar = new Button { Text = "📋  Copiar resumen", Location = new Point(252, ry), Size = new Size(170, 44),
                BackColor = Color.FromArgb(243, 244, 246), ForeColor = C_Text, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCopiar.FlatAppearance.BorderSize = 0;
            btnCopiar.FlatAppearance.MouseOverBackColor = Color.FromArgb(229, 231, 235);
            btnCopiar.Click += BtnCopiarResumen_Click;
            _pnlResultado.Controls.Add(btnCopiar);
            Redondear(btnCopiar, 8);
            ry += 56;

            // Resumen por participante
            _pnlResultado.Controls.Add(new Label { Text = "Resumen de deudas pendientes:", ForeColor = C_Muted, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(20, ry), AutoSize = true }); ry += 20;
            string me = SesionActual.Instancia?.Usuario?.NombreCompleto ?? "Tú";
            _pnlResultado.Controls.Add(new Label { Text = $"  • {me} (tú)  ➜  {(parte + ajuste).ToString("C2", ES)}", ForeColor = C_Green, Font = new Font("Segoe UI", 9), Location = new Point(20, ry), AutoSize = true }); ry += 20;
            foreach (var (txtNom, txtIba) in _participantes)
            {
                string nm = string.IsNullOrWhiteSpace(txtNom.Text) ? "Participante" : txtNom.Text.Trim();
                string raw = txtIba.Text.Trim().Replace(" ", "");
                string ib = string.IsNullOrWhiteSpace(txtIba.Text) ? "(sin IBAN)" : $"••••{(raw.Length >= 4 ? raw.Substring(raw.Length - 4) : raw)}";
                _pnlResultado.Controls.Add(new Label { Text = $"  • {nm}  ➜  {parte.ToString("C2", ES)}  {ib}", ForeColor = C_Text, Font = new Font("Segoe UI", 9), Location = new Point(20, ry), AutoSize = true }); ry += 20;
            }
            _pnlResultado.Height = ry + 20;
        }

        private void BtnPagarParte_Click(object sender, EventArgs e)
        {
            if (_cmbCuenta.SelectedItem == null) { Err("Selecciona una cuenta de cargo."); return; }
            var ci = (CuentaItem)_cmbCuenta.SelectedItem;
            if (_parteCalculada > ci.Cuenta.Saldo) { Err("Saldo insuficiente para pagar tu parte."); return; }

            string ult4 = ci.Cuenta.NumeroCuenta?.Length > 4 ? ci.Cuenta.NumeroCuenta.Substring(ci.Cuenta.NumeroCuenta.Length - 4) : ci.Cuenta.NumeroCuenta;
            var res = MessageBox.Show(
                $"¿Confirmar pago de tu parte ({_parteCalculada.ToString("C2", ES)}) del gasto '{_txtConcepto.Text.Trim()}'?\nCuenta cargo: ••••{ult4}",
                "Nexum Bank — Confirmar pago", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes) return;

            _btnPagar.Enabled = false; _btnPagar.Text = "Procesando...";
            bool ok = _movService.RegistrarRetiro(ci.Cuenta.Id, _parteCalculada, $"División gasto: {_txtConcepto.Text.Trim()}", out string err);
            if (ok)
            {
                MessageBox.Show($"✓  Pago de {_parteCalculada.ToString("C2", ES)} registrado como tu parte del gasto '{_txtConcepto.Text.Trim()}'.",
                    "Nexum Bank — Pago confirmado", MessageBoxButtons.OK, MessageBoxIcon.None);
                _pnlResultado.Visible = false; _txtTotal.Text = "0,00"; _txtConcepto.Text = ""; _parteCalculada = 0;
                CargarCuentas();
            }
            else Err(err ?? "No se pudo procesar el pago.");
            _btnPagar.Enabled = true; _btnPagar.Text = $"Pagar mi parte ({_parteCalculada.ToString("C2", ES)})";
        }

        private void ActualizarPreviewParte() { /* triggered on participant count change */ }

        private void BtnCopiarResumen_Click(object sender, EventArgs e)
        {
            if (_parteCalculada <= 0) return;
            int n = (int)_nudPersonas.Value;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"📋 División de gastos — Nexum Bank");
            sb.AppendLine($"Concepto: {_txtConcepto.Text.Trim()}");
            sb.AppendLine($"Total: {(decimal.TryParse(_txtTotal.Text.Replace(",","."), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out decimal t) ? t.ToString("C2",ES) : "?")}");
            sb.AppendLine($"Personas: {n}");
            sb.AppendLine($"Parte por persona: {_parteCalculada.ToString("C2", ES)}");
            sb.AppendLine();
            sb.AppendLine($"  • {SesionActual.Instancia?.Usuario?.NombreCompleto ?? "Tú"} (ya pagado)");
            foreach (var (txtNom, txtIba) in _participantes)
            {
                string nm = string.IsNullOrWhiteSpace(txtNom.Text) ? "Participante" : txtNom.Text.Trim();
                sb.AppendLine($"  • {nm} debe {_parteCalculada.ToString("C2", ES)}");
            }
            try { Clipboard.SetText(sb.ToString()); MessageBox.Show("Resumen copiado al portapapeles.", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.None); }
            catch { }
        }

        private void MostrarError(string msg) { MessageBox.Show("⚠  " + msg, "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        private void Err(string msg) { if (_lblError != null) { _lblError.Text = "⚠  " + msg; _lblError.Visible = true; } }

        private void CargarCuentas()
        {
            _cmbCuenta.Items.Clear();
            if (SesionActual.Instancia?.Usuario == null) return;
            foreach (var c in _cuentaService.ObtenerCuentasPorUsuario(SesionActual.Instancia.Usuario.Id))
            {
                string u = c.NumeroCuenta?.Length > 4 ? c.NumeroCuenta.Substring(c.NumeroCuenta.Length - 4) : c.NumeroCuenta;
                _cmbCuenta.Items.Add(new CuentaItem { Cuenta = c, Display = $"{c.TipoCuenta}  •  ••••{u}  —  {c.Saldo.ToString("C2", ES)}" });
            }
            _cmbCuenta.DisplayMember = "Display";
            if (_cmbCuenta.Items.Count > 0) _cmbCuenta.SelectedIndex = 0;
            ActualizarSaldo();
        }

        private void ActualizarSaldo() { if (_cmbCuenta.SelectedItem is CuentaItem ci) _lblSaldo.Text = $"✓  Saldo disponible: {ci.Cuenta.Saldo.ToString("C2", ES)}"; }
        private Panel MakeCard(Point loc, Size sz) { var p = new Panel { Location = loc, Size = sz, BackColor = C_White }; p.Paint += (s, ev) => { ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using (var path = RR(p.ClientRectangle, 12)) { ev.Graphics.FillPath(Brushes.White, path); ev.Graphics.DrawPath(new Pen(C_Border, 1), path); } }; BeginInvoke(new Action(() => Redondear(p, 12))); return p; }
        private Panel InputBoxInline(Panel parent, Point loc, Size sz, out TextBox txt, string hint) { var p = new Panel { Location = loc, Size = sz, BackColor = C_White, Tag = C_Border }; p.Paint += (s, ev) => { var col = p.Tag is Color c ? c : C_Border; ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using (var path = RR(p.ClientRectangle, 8)) { ev.Graphics.FillPath(Brushes.White, path); ev.Graphics.DrawPath(new Pen(col, 1.5f), path); } }; txt = new TextBox { Location = new Point(10, (sz.Height - 22) / 2), Size = new Size(sz.Width - 20, 24), Font = new Font("Segoe UI", 10), BackColor = C_White, ForeColor = C_Text, BorderStyle = BorderStyle.None }; p.Controls.Add(txt); parent.Controls.Add(p); return p; }
        private void FocusBox(Panel p, Color col) { p.Tag = col; p.Invalidate(); }
        private Panel MakeIconCircle(Point loc, int size, Color col, string g, float fs) { var p = new Panel { Location = loc, Size = new Size(size, size), BackColor = Color.Transparent }; p.Paint += (s, ev) => { ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias; ev.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(20, col.R, col.G, col.B)), p.ClientRectangle); var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }; ev.Graphics.DrawString(g, new Font("Segoe UI", fs), new SolidBrush(col), p.ClientRectangle, fmt); }; return p; }
        private Label FieldLbl(string t, int x, int y) => new Label { Text = t, Location = new Point(x, y), ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true };
        private static GraphicsPath RR(Rectangle r, int rad) { int d = rad * 2; var p = new GraphicsPath(); p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90); p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90); p.CloseAllFigures(); return p; }
        private static void Redondear(Control c, int r) { try { if (c.Width > 0 && c.Height > 0) c.Region = new Region(RR(c.ClientRectangle, r)); } catch { } }

        // Propaga el click recursivamente a todos los controles hijo
        private static void PropagateClick(Control parent, EventHandler handler)
        {
            parent.Click += handler;
            foreach (Control c in parent.Controls)
            {
                c.Cursor = Cursors.Hand;
                PropagateClick(c, handler);
            }
        }

        private class CuentaItem { public CuentaBancaria Cuenta; public string Display; public override string ToString() => Display; }
    }
}
