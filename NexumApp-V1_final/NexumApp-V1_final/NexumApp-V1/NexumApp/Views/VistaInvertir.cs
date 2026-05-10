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
    public class VistaInvertir : UserControl
    {
        private static readonly Color C_Red     = Color.FromArgb(236, 0, 0);
        private static readonly Color C_RedDark = Color.FromArgb(180, 0, 0);
        private Color C_BgPage => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(10, 12, 28)    : Color.FromArgb(246, 247, 249);
        private Color C_White  => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(18, 22, 46)    : Color.White;
        private Color C_Border => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(38, 44, 80)    : Color.FromArgb(220, 224, 230);
        private Color C_Text   => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(241, 245, 249) : Color.FromArgb(30, 30, 50);
        private Color C_Muted  => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(100, 116, 139) : Color.FromArgb(110, 120, 140);
        private static readonly Color C_Green   = Color.FromArgb(0, 168, 89);
        private Color C_Dark   => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(8, 10, 22)     : Color.FromArgb(18, 22, 38);
        private CultureInfo ES => Helpers.AppSettings.CultureMoneda;

        private readonly CuentaService     _cuentaService = new CuentaService();
        private readonly MovimientoService _movService    = new MovimientoService();

        private ComboBox _cmbCuenta;
        private Label    _lblSaldo, _lblError, _lblRentabilidad;
        private TextBox  _txtMonto;
        private Button   _btnInvertir;
        private string   _fondoSeleccionado = "";
        private decimal  _rentabilidad = 0;
        private Panel    _pnlFondos;

        private static readonly (string Nombre, string Tipo, string Riesgo, decimal RentAnual, Color Col)[] FONDOS = {
            ("Fondo Monetario Nexum",      "Monetario",       "Muy bajo",  1.8m,  Color.FromArgb(0, 168, 89)),
            ("Renta Fija Europa",          "Renta Fija",      "Bajo",      3.2m,  Color.FromArgb(0, 122, 204)),
            ("Mixto Conservador",          "Mixto",           "Moderado",  5.1m,  Color.FromArgb(100, 60, 200)),
            ("Índice S&P 500",             "Renta Variable",  "Alto",      9.8m,  Color.FromArgb(236, 120, 0)),
            ("Tecnología Global",          "Sectorial",       "Muy alto",  14.2m, Color.FromArgb(200, 0, 0)),
            ("Sostenible ESG Nexum",       "Mixto ESG",       "Moderado",  6.5m,  Color.FromArgb(0, 140, 80)),
        };

        public VistaInvertir()
        {
            BackColor = C_BgPage; Dock = DockStyle.Fill; DoubleBuffered = true;
            Helpers.AppSettings.ConfiguracionChanged += (s, e) =>
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(new Action(() => { BuildUI(); CargarCuentas(); Helpers.AppSettings.AplicarTraduccionesRecursivo(this); }));
            };
        }
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
            var icoHdr = MakeIconCircle(new Point(0, 6), 48, C_Red, "📈", 20f);
            main.Controls.Add(icoHdr);
            main.Controls.Add(new Label { Text = "Invertir", ForeColor = C_Text, Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(62, 8), AutoSize = true });
            main.Controls.Add(new Label { Text = "Fondos de inversión, planes de pensiones y más", ForeColor = C_Muted, Font = new Font("Segoe UI", 10), Location = new Point(62, 38), AutoSize = true }); y += 76;

            // Banner aviso legal oscuro
            var banner = new Panel { Location = new Point(0, y), Size = new Size(840, 52), BackColor = C_Dark };
            banner.Paint += (s, ev) => { ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using (var path = RR(banner.ClientRectangle, 10)) ev.Graphics.FillPath(new SolidBrush(C_Dark), path); };
            Redondear(banner, 10);
            banner.Controls.Add(new Label { Text = "⚠️  Las inversiones conllevan riesgo. La rentabilidad pasada no garantiza resultados futuros. Datos orientativos.",
                ForeColor = Color.FromArgb(200, 200, 220), Font = new Font("Segoe UI", 9, FontStyle.Italic),
                Location = new Point(16, 16), AutoSize = true });
            main.Controls.Add(banner); y += 66;

            // Card cuenta
            var cardCuenta = MakeCard(new Point(0, y), new Size(840, 86)); main.Controls.Add(cardCuenta); y += 100;
            cardCuenta.Controls.Add(FieldLbl("CUENTA DE CARGO", 20, 14));
            _cmbCuenta = new ComboBox { Location = new Point(20, 34), Size = new Size(500, 28),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10),
                BackColor = C_White, ForeColor = C_Text, FlatStyle = FlatStyle.Flat };
            _cmbCuenta.SelectedIndexChanged += (s, ev) => ActualizarSaldo();
            cardCuenta.Controls.Add(_cmbCuenta);
            _lblSaldo = new Label { Location = new Point(20, 66), AutoSize = true, ForeColor = C_Green, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            cardCuenta.Controls.Add(_lblSaldo);

            // Fondos disponibles
            main.Controls.Add(new Label { Text = "Fondos disponibles", ForeColor = C_Text, Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(0, y), AutoSize = true }); y += 30;

            _pnlFondos = new Panel { Location = new Point(0, y), Size = new Size(840, 300), BackColor = Color.Transparent };
            main.Controls.Add(_pnlFondos); y += 314;

            int fy = 0; int col2X = 430;
            for (int i = 0; i < FONDOS.Length; i++)
            {
                var (nombre, tipo, riesgo, rent, col) = FONDOS[i];
                int x = (i % 2 == 0) ? 0 : col2X;
                if (i % 2 == 0 && i > 0) fy += 104;
                string n = nombre; string t = tipo; string ri = riesgo; decimal r = rent; Color c = col;
                var card = CrearCardFondo(n, t, ri, r, c);
                card.Location = new Point(x, fy);
                EventHandler sel = (s, ev) => SeleccionarFondo(n, r, _pnlFondos);
                PropagateClick(card, sel);
                _pnlFondos.Controls.Add(card);
            }
            _pnlFondos.Height = fy + 104;
            y = y - 314 + _pnlFondos.Height + 14;

            // Card inversión
            var cardInv = MakeCard(new Point(0, y), new Size(840, 200)); main.Controls.Add(cardInv); y += 214;
            int iiy = 18;
            cardInv.Controls.Add(new Label { Text = "Realizar inversión", ForeColor = C_Text, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(20, iiy), AutoSize = true }); iiy += 34;
            cardInv.Controls.Add(new Panel { Location = new Point(20, iiy), Size = new Size(800, 1), BackColor = C_Border }); iiy += 14;

            cardInv.Controls.Add(FieldLbl("FONDO SELECCIONADO", 20, iiy));
            cardInv.Controls.Add(FieldLbl("IMPORTE A INVERTIR (€)", 360, iiy)); iiy += 20;

            var lblFondoSel = new Label { Text = "— Selecciona un fondo —", ForeColor = C_Muted, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, iiy), Size = new Size(330, 28), AutoSize = false };
            cardInv.Controls.Add(lblFondoSel);

            var pMon = InputBoxInline(cardInv, new Point(360, iiy - 4), new Size(280, 42), out _txtMonto, "0,00");
            _txtMonto.Font = new Font("Segoe UI", 14, FontStyle.Bold); _txtMonto.ForeColor = C_Red;
            _txtMonto.GotFocus  += (s, e) => { if (_txtMonto.Text == "0,00") _txtMonto.Clear(); FocusBox(pMon, C_Red); };
            _txtMonto.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(_txtMonto.Text)) _txtMonto.Text = "0,00"; FocusBox(pMon, C_Border); ActualizarRentabilidad(lblFondoSel); };
            iiy += 52;

            _lblRentabilidad = new Label { Location = new Point(360, iiy), AutoSize = true, ForeColor = C_Green, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            cardInv.Controls.Add(_lblRentabilidad);

            _lblError = new Label { Location = new Point(20, iiy), Size = new Size(820, 16), ForeColor = C_Red, Font = new Font("Segoe UI", 8, FontStyle.Bold), Visible = false };
            cardInv.Controls.Add(_lblError); iiy += 22;

            _btnInvertir = new Button { Text = "Confirmar inversión", Location = new Point(20, iiy), Size = new Size(200, 44),
                BackColor = C_Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            _btnInvertir.FlatAppearance.BorderSize = 0;
            _btnInvertir.MouseEnter += (s, e) => _btnInvertir.BackColor = C_RedDark;
            _btnInvertir.MouseLeave += (s, e) => _btnInvertir.BackColor = C_Red;
            _btnInvertir.Click += (s, e) => BtnInvertir_Click(lblFondoSel);
            cardInv.Controls.Add(_btnInvertir);
            cardInv.Height = iiy + 56;
            BeginInvoke(new Action(() => { Redondear(_btnInvertir, 8); Redondear(pMon, 8); }));
            main.Height = y + 20;
        }

        private Panel CrearCardFondo(string nombre, string tipo, string riesgo, decimal rent, Color col)
        {
            var card = new Panel { Size = new Size(410, 96), BackColor = C_White, Cursor = Cursors.Hand, Tag = nombre };
            card.Paint += (s, ev) => {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(card.ClientRectangle, 10)) {
                    ev.Graphics.FillPath(new SolidBrush(card.BackColor), path);
                    bool sel = _fondoSeleccionado == nombre;
                    ev.Graphics.DrawPath(new Pen(sel ? C_Red : C_Border, sel ? 2f : 1f), path);
                }
                // Franja lateral color
                using (var br = new SolidBrush(col))
                    ev.Graphics.FillRectangle(br, new Rectangle(0, 0, 6, card.Height));
            };
            Redondear(card, 10);
            Action hover   = () => { if (_fondoSeleccionado != nombre) { card.BackColor = Color.FromArgb(252, 252, 253); card.Invalidate(); } };
            Action unhover = () => { if (_fondoSeleccionado != nombre) { card.BackColor = C_White; card.Invalidate(); } };
            card.MouseEnter += (s, e) => hover();
            card.MouseLeave += (s, e) => unhover();

            var lblNombre = new Label { Text = nombre, ForeColor = C_Text, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(18, 12), Size = new Size(260, 20), BackColor = Color.Transparent, Cursor = Cursors.Hand };
            var lblTipo   = new Label { Text = $"{tipo}  ·  Riesgo: {riesgo}", ForeColor = C_Muted, Font = new Font("Segoe UI", 8), Location = new Point(18, 36), Size = new Size(260, 16), BackColor = Color.Transparent, Cursor = Cursors.Hand };
            var lblRent   = new Label { Text = $"+{rent:F1}% / año", ForeColor = C_Green, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(300, 16), Size = new Size(100, 26), TextAlign = ContentAlignment.MiddleRight, BackColor = Color.Transparent, Cursor = Cursors.Hand };
            var lblRef    = new Label { Text = "rentabilidad est.", ForeColor = C_Muted, Font = new Font("Segoe UI", 7), Location = new Point(300, 44), Size = new Size(100, 14), TextAlign = ContentAlignment.MiddleRight, BackColor = Color.Transparent, Cursor = Cursors.Hand };
            foreach (Label l in new[] { lblNombre, lblTipo, lblRent, lblRef })
            { l.MouseEnter += (s, e) => hover(); l.MouseLeave += (s, e) => unhover(); }

            // Barra riesgo
            int nivel = riesgo == "Muy bajo" ? 1 : riesgo == "Bajo" ? 2 : riesgo == "Moderado" ? 3 : riesgo == "Alto" ? 4 : 5;
            var pnlBar  = new Panel { Location = new Point(18, 62), Size = new Size(260, 6), BackColor = Color.FromArgb(220, 224, 230), Cursor = Cursors.Hand };
            var pnlFill = new Panel { Location = new Point(0, 0), Size = new Size(260 * nivel / 5, 6), BackColor = col, Cursor = Cursors.Hand };
            pnlBar.MouseEnter  += (s, e) => hover(); pnlBar.MouseLeave  += (s, e) => unhover();
            pnlFill.MouseEnter += (s, e) => hover(); pnlFill.MouseLeave += (s, e) => unhover();
            pnlBar.Controls.Add(pnlFill); Redondear(pnlBar, 3); Redondear(pnlFill, 3);
            card.Controls.Add(lblNombre); card.Controls.Add(lblTipo); card.Controls.Add(lblRent);
            card.Controls.Add(lblRef); card.Controls.Add(pnlBar);
            return card;
        }

        private void SeleccionarFondo(string nombre, decimal rent, Panel grid)
        {
            _fondoSeleccionado = nombre; _rentabilidad = rent;
            foreach (Panel c in grid.Controls) { c.BackColor = (string)c.Tag == nombre ? Color.FromArgb(255, 242, 242) : C_White; c.Invalidate(); }
        }

        private void ActualizarRentabilidad(Label lblFondo)
        {
            lblFondo.Text = string.IsNullOrEmpty(_fondoSeleccionado) ? "— Selecciona un fondo —" : _fondoSeleccionado;
            lblFondo.ForeColor = string.IsNullOrEmpty(_fondoSeleccionado) ? C_Muted : C_Text;
            string ms = _txtMonto.Text.Trim().Replace(",", ".").Replace(" ", "");
            if (!decimal.TryParse(ms, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out decimal m) || m <= 0 || _rentabilidad <= 0)
            { _lblRentabilidad.Text = ""; return; }
            decimal ganancia = m * _rentabilidad / 100m;
            _lblRentabilidad.Text = $"📈  Beneficio estimado en 1 año: +{ganancia.ToString("C2", ES)}";
        }

        private void BtnInvertir_Click(Label lblFondo)
        {
            _lblError.Visible = false;
            if (_cmbCuenta.SelectedItem == null) { Err("Selecciona una cuenta de cargo."); return; }
            if (string.IsNullOrEmpty(_fondoSeleccionado)) { Err("Selecciona un fondo de inversión."); return; }
            string ms = _txtMonto.Text.Trim().Replace(",", ".").Replace(" ", "");
            if (!decimal.TryParse(ms, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out decimal monto) || monto <= 0)
            { Err("Introduce un importe válido mayor que 0 €."); return; }
            var ci = (CuentaItem)_cmbCuenta.SelectedItem;
            if (monto > ci.Cuenta.Saldo) { Err("Saldo insuficiente para esta inversión."); return; }

            string ult4 = ci.Cuenta.NumeroCuenta?.Length > 4 ? ci.Cuenta.NumeroCuenta.Substring(ci.Cuenta.NumeroCuenta.Length - 4) : ci.Cuenta.NumeroCuenta;
            var res = MessageBox.Show(
                $"¿Confirmar inversión de {monto.ToString("C2", ES)} en '{_fondoSeleccionado}'?\n\nRentabilidad estimada: +{_rentabilidad:F1}% anual\nBeneficio estimado 1 año: +{(monto * _rentabilidad / 100m).ToString("C2", ES)}\nCuenta cargo: ••••{ult4}\n\n⚠ Las inversiones conllevan riesgo. La rentabilidad pasada no garantiza resultados futuros.",
                "Nexum Bank — Confirmar inversión", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (res != DialogResult.Yes) return;

            _btnInvertir.Enabled = false; _btnInvertir.Text = "Procesando...";
            bool ok = _movService.RegistrarRetiro(ci.Cuenta.Id, monto, $"Inversión – {_fondoSeleccionado}", out string err);
            if (ok)
            {
                MessageBox.Show($"✓  Inversión de {monto.ToString("C2", ES)} en '{_fondoSeleccionado}' confirmada.\n\nBeneficio estimado en 1 año: +{(monto * _rentabilidad / 100m).ToString("C2", ES)}",
                    "Nexum Bank — Inversión realizada", MessageBoxButtons.OK, MessageBoxIcon.None);
                _txtMonto.Text = "0,00"; _fondoSeleccionado = ""; _rentabilidad = 0;
                SeleccionarFondo("", 0, _pnlFondos); _lblRentabilidad.Text = "";
                CargarCuentas();
            }
            else Err(err ?? "No se pudo procesar la inversión.");
            _btnInvertir.Enabled = true; _btnInvertir.Text = "Confirmar inversión";
        }

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
        private void Err(string msg) { _lblError.Text = "⚠  " + msg; _lblError.Visible = true; }
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
