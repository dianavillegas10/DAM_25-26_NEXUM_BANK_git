using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace NexumApp.Forms.Movimientos
{
    internal partial class FrmRetirarEfectivo : Form
    {
        private readonly CuentaService     _cuentaService     = new CuentaService();
        private readonly MovimientoService _movimientoService = new MovimientoService();
        private readonly CuentaBancaria    _cuentaPreseleccionada;

        private static readonly Color C_Bg     = Color.FromArgb(11,  13,  30);
        private static readonly Color C_Card   = Color.FromArgb(18,  22,  46);
        private static readonly Color C_Input  = Color.FromArgb(24,  29,  58);
        private static readonly Color C_Border = Color.FromArgb(38,  44,  80);
        private static readonly Color C_Red    = Color.FromArgb(248, 113, 113);
        private static readonly Color C_RedDk  = Color.FromArgb(220,  70,  70);
        private static readonly Color C_Green  = Color.FromArgb(52,  211, 153);
        private static readonly Color C_Text   = Color.FromArgb(241, 245, 249);
        private static readonly Color C_Muted  = Color.FromArgb(100, 116, 139);
        private static readonly CultureInfo ES = CultureInfo.CreateSpecificCulture("es-ES");

        private ComboBox _cmb;
        private TextBox  _txtMonto;
        private TextBox  _txtConcepto;
        private Button   _btnConfirmar;
        private Label    _lblSaldo;
        private Label    _lblTras;
        private Label    _lblError;
        private Panel    _pnlMontoBox;
        private Panel    _pnlConcBox;

        private static readonly decimal[] _rapidos = { 20m, 50m, 100m, 200m };

        public FrmRetirarEfectivo(CuentaBancaria cuentaPreseleccionada = null)
        {
            InitializeComponent();
            _cuentaPreseleccionada = cuentaPreseleccionada;
            DoubleBuffered = true;
        }

        private void FrmRetirarEfectivo_Load(object sender, EventArgs e)
        {
            BuildUI();
            CargarCuentas();
        }

        private void BuildUI()
        {
            Controls.Clear();

            Text = "Retirar efectivo — Nexum Bank";
            Size = new Size(480, 670);
            MinimumSize = Size; MaximumSize = Size;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = C_Bg;

            Paint += (s, ev) =>
            {
                using (var b = new LinearGradientBrush(ClientRectangle,
                    Color.FromArgb(15, 18, 40), C_Bg, LinearGradientMode.Vertical))
                    ev.Graphics.FillRectangle(b, ClientRectangle);
            };

            int icoSize = 56;
            var ico = MakeIcon(new Point((480 - icoSize) / 2, 18), icoSize, C_Red, C_RedDk, "↓", 24f);
            Controls.Add(ico);

            Controls.Add(MakeLbl("Retirar efectivo",
                new Point(0, 82), 480, 26, new Font("Segoe UI", 16, FontStyle.Bold), C_Text, ContentAlignment.MiddleCenter));
            Controls.Add(MakeLbl("Retira fondos de una de tus cuentas",
                new Point(0, 110), 480, 18, new Font("Segoe UI", 9), C_Muted, ContentAlignment.MiddleCenter));

            int cx = 24; int cw = 432;
            var card = MakeCard(new Point(cx, 136), new Size(cw, 504));
            Controls.Add(card);

            int iw = cw - 48;
            int iy = 20;

            // CUENTA
            card.Controls.Add(FieldLbl("CUENTA DE ORIGEN", 24, iy)); iy += 20;
            _cmb = new ComboBox
            {
                Location = new Point(24, iy), Size = new Size(iw, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                BackColor = C_Input, ForeColor = C_Text, FlatStyle = FlatStyle.Flat
            };
            _cmb.SelectedIndexChanged += (s, ev) => { RefrescarSaldo(); Recalcular(); };
            card.Controls.Add(_cmb); iy += 38;

            _lblSaldo = new Label
            {
                Location = new Point(24, iy), Size = new Size(iw, 16),
                ForeColor = C_Green, Font = new Font("Segoe UI", 8, FontStyle.Bold), Text = ""
            };
            card.Controls.Add(_lblSaldo); iy += 22;

            card.Controls.Add(Sep(24, iy, iw)); iy += 14;

            // IMPORTE
            card.Controls.Add(FieldLbl("IMPORTE A RETIRAR", 24, iy)); iy += 20;

            _pnlMontoBox = InputBox(new Point(24, iy), new Size(iw, 52), out _txtMonto);
            _txtMonto.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            _txtMonto.ForeColor = C_Red; _txtMonto.Text = "0,00";
            _txtMonto.Size = new Size(iw - 44, 30); _txtMonto.Location = new Point(12, 10);
            _txtMonto.GotFocus  += (s, ev) => { if (_txtMonto.Text == "0,00") _txtMonto.Clear(); BorderFocus(_pnlMontoBox, C_Red); };
            _txtMonto.LostFocus += (s, ev) => { if (string.IsNullOrWhiteSpace(_txtMonto.Text)) _txtMonto.Text = "0,00"; BorderFocus(_pnlMontoBox, C_Border); Recalcular(); };
            _txtMonto.TextChanged += (s, ev) => { _lblError.Visible = false; Recalcular(); };
            _pnlMontoBox.Controls.Add(new Label { Text = "€", ForeColor = C_Muted, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(iw - 30, 12), AutoSize = true });
            card.Controls.Add(_pnlMontoBox); iy += 58;

            // Saldo tras retiro
            _lblTras = new Label
            {
                Location = new Point(24, iy), Size = new Size(iw, 16),
                ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), Text = ""
            };
            card.Controls.Add(_lblTras); iy += 22;

            // ACCESO RÁPIDO
            card.Controls.Add(FieldLbl("RETIRAR RÁPIDO", 24, iy)); iy += 20;
            var tlp = new TableLayoutPanel
            {
                Location = new Point(24, iy), Size = new Size(iw, 34),
                ColumnCount = 4, RowCount = 1, BackColor = Color.Transparent, Padding = new Padding(0)
            };
            for (int i = 0; i < 4; i++) tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            foreach (var imp in _rapidos)
            {
                var b = new Button
                {
                    Text = imp.ToString("C0", ES), Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 0),
                    BackColor = C_Input, ForeColor = C_Red, FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
                };
                b.FlatAppearance.BorderColor = C_Border; b.FlatAppearance.BorderSize = 1;
                decimal cap = imp;
                b.Click += (s, ev) => { _txtMonto.Text = cap.ToString("N2", ES); Recalcular(); _lblError.Visible = false; };
                b.MouseEnter += (s, ev) => b.BackColor = Color.FromArgb(35, C_Red.R, C_Red.G, C_Red.B);
                b.MouseLeave += (s, ev) => b.BackColor = C_Input;
                tlp.Controls.Add(b);
            }
            card.Controls.Add(tlp); iy += 42;

            // CONCEPTO
            card.Controls.Add(FieldLbl("CONCEPTO (OPCIONAL)", 24, iy)); iy += 20;
            _pnlConcBox = InputBox(new Point(24, iy), new Size(iw, 38), out _txtConcepto);
            _txtConcepto.Font = new Font("Segoe UI", 10); _txtConcepto.ForeColor = C_Text;
            _txtConcepto.Size = new Size(iw - 24, 22); _txtConcepto.Location = new Point(12, 8);
            _txtConcepto.GotFocus  += (s, ev) => BorderFocus(_pnlConcBox, C_Red);
            _txtConcepto.LostFocus += (s, ev) => BorderFocus(_pnlConcBox, C_Border);
            card.Controls.Add(_pnlConcBox); iy += 46;

            // ERROR
            _lblError = new Label
            {
                Location = new Point(24, iy), Size = new Size(iw, 16),
                ForeColor = C_Red, Font = new Font("Segoe UI", 8, FontStyle.Bold), Visible = false
            };
            card.Controls.Add(_lblError); iy += 20;

            card.Controls.Add(Sep(24, iy, iw)); iy += 14;

            // BOTÓN CONFIRMAR
            _btnConfirmar = new Button
            {
                Text = "↓   CONFIRMAR RETIRO",
                Location = new Point(24, iy), Size = new Size(iw, 46),
                BackColor = C_Red, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand
            };
            _btnConfirmar.FlatAppearance.BorderSize = 0;
            _btnConfirmar.MouseEnter += (s, ev) => _btnConfirmar.BackColor = C_RedDk;
            _btnConfirmar.MouseLeave += (s, ev) => _btnConfirmar.BackColor = C_Red;
            _btnConfirmar.Click += BtnConfirmar_Click;
            card.Controls.Add(_btnConfirmar); iy += 52;

            // BOTÓN CANCELAR
            var btnC = new Button
            {
                Text = "Cancelar",
                Location = new Point(24, iy), Size = new Size(iw, 32),
                BackColor = Color.Transparent, ForeColor = C_Muted,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand
            };
            btnC.FlatAppearance.BorderSize = 0;
            btnC.MouseEnter += (s, ev) => btnC.ForeColor = C_Text;
            btnC.MouseLeave += (s, ev) => btnC.ForeColor = C_Muted;
            btnC.Click += BtnCancelar_Click;
            card.Controls.Add(btnC);

            BeginInvoke(new Action(() =>
            {
                Redondear(_btnConfirmar, 10);
                Redondear(card, 16);
                foreach (Control c in tlp.Controls) Redondear(c, 7);
            }));
        }

        // ── Lógica ──────────────────────────────────────────────
        private void CargarCuentas()
        {
            _cmb.Items.Clear();
            if (SesionActual.Instancia?.Usuario == null) return;
            var lista = _cuentaService.ObtenerCuentasPorUsuario(SesionActual.Instancia.Usuario.Id);
            foreach (var c in lista)
            {
                string u = c.NumeroCuenta?.Length > 4 ? c.NumeroCuenta.Substring(c.NumeroCuenta.Length - 4) : c.NumeroCuenta;
                _cmb.Items.Add(new CI { Cuenta = c, D = $"{c.TipoCuenta}  ·  •••• {u}  —  {c.Saldo.ToString("C2", ES)}" });
            }
            _cmb.DisplayMember = "D";
            if (_cuentaPreseleccionada != null)
                for (int i = 0; i < _cmb.Items.Count; i++)
                    if (((CI)_cmb.Items[i]).Cuenta.Id == _cuentaPreseleccionada.Id) { _cmb.SelectedIndex = i; break; }
            else if (_cmb.Items.Count > 0) _cmb.SelectedIndex = 0;
            RefrescarSaldo();
        }

        private void RefrescarSaldo()
        {
            if (_cmb.SelectedItem is CI item) _lblSaldo.Text = $"Saldo disponible: {item.Cuenta.Saldo.ToString("C2", ES)}";
        }

        private void Recalcular()
        {
            if (!(_cmb.SelectedItem is CI item)) return;
            string ms = _txtMonto.Text.Trim().Replace(",", ".").Replace(" ", "");
            if (!decimal.TryParse(ms, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal m) || m <= 0) { _lblTras.Text = ""; return; }
            decimal tras = item.Cuenta.Saldo - m;
            _lblTras.ForeColor = tras >= 0 ? C_Muted : C_Red;
            _lblTras.Text = tras >= 0 ? $"Saldo tras el retiro: {tras.ToString("C2", ES)}" : "⚠  Saldo insuficiente para este importe";
        }

        private void BtnConfirmar_Click(object sender, EventArgs e)
        {
            _lblError.Visible = false;
            if (_cmb.SelectedItem == null) { Err("Selecciona una cuenta de origen."); return; }
            string ms = _txtMonto.Text.Trim().Replace(",", ".").Replace(" ", "");
            if (!decimal.TryParse(ms, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal m) || m <= 0)
            { Err("Introduce un importe válido mayor que 0 €."); return; }
            var cuenta = ((CI)_cmb.SelectedItem).Cuenta;
            if (m > cuenta.Saldo) { Err("Saldo insuficiente para realizar este retiro."); return; }
            _btnConfirmar.Enabled = false; _btnConfirmar.Text = "Procesando...";
            try
            {
                if (_movimientoService.RegistrarRetiro(cuenta.Id, m, _txtConcepto.Text.Trim(), out string error))
                {
                    MessageBox.Show($"Retiro de {m.ToString("C2", ES)} completado.", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.None);
                    DialogResult = DialogResult.OK; Close();
                }
                else { Err(error ?? "No se pudo completar el retiro."); _btnConfirmar.Enabled = true; _btnConfirmar.Text = "↓   CONFIRMAR RETIRO"; }
            }
            catch (Exception ex) { Err("Error: " + ex.Message); _btnConfirmar.Enabled = true; _btnConfirmar.Text = "↓   CONFIRMAR RETIRO"; }
        }

        private void BtnCancelar_Click(object sender, EventArgs e) { DialogResult = DialogResult.Cancel; Close(); }
        private void Err(string msg) { _lblError.Text = "⚠  " + msg; _lblError.Visible = true; }

        // ── Helpers UI ──────────────────────────────────────────
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

        private Panel InputBox(Point loc, Size sz, out TextBox txt)
        {
            var p = new Panel { Location = loc, Size = sz, BackColor = C_Input };
            p.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(p.ClientRectangle, 9))
                { ev.Graphics.FillPath(new SolidBrush(C_Input), path); ev.Graphics.DrawPath(new Pen(C_Border, 1), path); }
            };
            txt = new TextBox { Location = new Point(12, (sz.Height - 22) / 2), Size = new Size(sz.Width - 30, 24), BackColor = C_Input, ForeColor = C_Text, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10) };
            p.Controls.Add(txt); return p;
        }

        private void BorderFocus(Panel p, Color col)
        {
            p.Tag = col;
            p.Paint -= DynBorder; p.Paint += DynBorder; p.Invalidate();
        }
        private void DynBorder(object s, PaintEventArgs ev)
        {
            var p = (Panel)s; var col = p.Tag is Color c ? c : C_Border;
            ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = RR(p.ClientRectangle, 9))
            { ev.Graphics.FillPath(new SolidBrush(C_Input), path); ev.Graphics.DrawPath(new Pen(col, 1.5f), path); }
        }

        private Panel MakeIcon(Point loc, int size, Color c1, Color c2, string g, float fs)
        {
            var p = new Panel { Location = loc, Size = new Size(size, size), BackColor = Color.Transparent };
            p.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(p.ClientRectangle, size / 4))
                using (var br = new LinearGradientBrush(p.ClientRectangle, c1, c2, 135f))
                    ev.Graphics.FillPath(br, path);
                using (var sh = new SolidBrush(Color.FromArgb(40, 255, 255, 255)))
                    ev.Graphics.FillEllipse(sh, -size / 4, -size / 4, size, size);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString(g, new Font("Segoe UI", fs, FontStyle.Bold), Brushes.White, p.ClientRectangle, fmt);
            };
            return p;
        }

        private Label MakeLbl(string t, Point loc, int w, int h, Font f, Color fore, ContentAlignment a)
            => new Label { Text = t, Location = loc, Size = new Size(w, h), Font = f, ForeColor = fore, TextAlign = a, BackColor = Color.Transparent };
        private Label FieldLbl(string t, int x, int y)
            => new Label { Text = t, Location = new Point(x, y), ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true };
        private Panel Sep(int x, int y, int w)
            => new Panel { Location = new Point(x, y), Size = new Size(w, 1), BackColor = C_Border };

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
