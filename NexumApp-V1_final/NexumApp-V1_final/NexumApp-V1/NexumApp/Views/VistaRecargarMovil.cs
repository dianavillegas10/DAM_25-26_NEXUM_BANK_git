using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace NexumApp.Views
{
    public class VistaRecargarMovil : UserControl
    {
        private static readonly Color C_Red     = Color.FromArgb(236, 0, 0);
        private static readonly Color C_RedDark = Color.FromArgb(180, 0, 0);
        private static readonly Color C_BgPage  = Color.FromArgb(246, 247, 249);
        private static readonly Color C_White   = Color.White;
        private static readonly Color C_Border  = Color.FromArgb(220, 224, 230);
        private static readonly Color C_Text    = Color.FromArgb(30, 30, 50);
        private static readonly Color C_Muted   = Color.FromArgb(110, 120, 140);
        private static readonly Color C_Green   = Color.FromArgb(0, 168, 89);
        private static readonly Color C_BlueSel = Color.FromArgb(235, 240, 255);
        private static readonly Color C_Blue    = Color.FromArgb(70, 100, 220);
        private static readonly CultureInfo ES  = CultureInfo.CreateSpecificCulture("es-ES");

        private readonly CuentaService     _cuentaService = new CuentaService();
        private readonly MovimientoService _movService    = new MovimientoService();

        private ComboBox _cmbCuenta;
        private Label    _lblSaldo, _lblError;
        private TextBox  _txtTelefono;
        private Button   _btnRecargar;
        private decimal  _importeSeleccionado = 0;
        private FlowLayoutPanel _pnlImportes;

        private static readonly (decimal Importe, string Bono)[] IMPORTES = {
            (5m,  "Sin bono"),
            (10m, "50 SMS gratis"),
            (15m, "+500 MB datos"),
            (20m, "1 GB datos"),
            (30m, "2 GB datos + llamadas"),
            (50m, "5 GB + llamadas ilimitadas"),
        };

        private static readonly (string Nombre, string Icono, Color Col)[] OPERADORAS = {
            ("Movistar",  "📶", Color.FromArgb(0, 102, 204)),
            ("Orange",    "🟠", Color.FromArgb(255, 100, 0)),
            ("Vodafone",  "🔴", Color.FromArgb(200, 0, 0)),
            ("Yoigo",     "🟢", Color.FromArgb(0, 160, 80)),
            ("MásMóvil",  "🔵", Color.FromArgb(0, 80, 180)),
            ("Simyo",     "⚪", Color.FromArgb(80, 80, 80)),
        };

        private string _operadoraSeleccionada = "";
        private Panel  _pnlOperadoras;

        public VistaRecargarMovil() { BackColor = C_BgPage; Dock = DockStyle.Fill; DoubleBuffered = true; }

        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); BuildUI(); CargarCuentas(); }

        private void BuildUI()
        {
            Controls.Clear();
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = C_BgPage };
            var main   = new Panel { Width = 820, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent };
            scroll.Controls.Add(main);
            scroll.Resize += (s, ev) => { main.Left = Math.Max(32, (scroll.ClientSize.Width - 820) / 2); };
            Controls.Add(scroll);
            int y = 28;

            // Cabecera
            var icoHdr = MakeIconCircle(new Point(0, 6), 48, C_Red, "📱", 20f);
            main.Controls.Add(icoHdr);
            main.Controls.Add(new Label { Text = "Recargar móvil", ForeColor = C_Text, Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(62, 8), AutoSize = true });
            main.Controls.Add(new Label { Text = "Recarga tu línea o la de un familiar al instante", ForeColor = C_Muted, Font = new Font("Segoe UI", 10), Location = new Point(62, 38), AutoSize = true }); y += 76;

            // Card cuenta
            var cardCuenta = MakeCard(new Point(0, y), new Size(780, 86)); main.Controls.Add(cardCuenta); y += 100;
            cardCuenta.Controls.Add(FieldLbl("CUENTA DE CARGO", 20, 14));
            _cmbCuenta = new ComboBox { Location = new Point(20, 34), Size = new Size(500, 28),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10),
                BackColor = C_White, ForeColor = C_Text, FlatStyle = FlatStyle.Flat };
            _cmbCuenta.SelectedIndexChanged += (s, ev) => ActualizarSaldo();
            cardCuenta.Controls.Add(_cmbCuenta);
            _lblSaldo = new Label { Location = new Point(20, 66), AutoSize = true, ForeColor = C_Green, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            cardCuenta.Controls.Add(_lblSaldo);

            // Sección teléfono y operadora
            var cardForm = MakeCard(new Point(0, y), new Size(780, 280)); main.Controls.Add(cardForm); y += 294;
            int iy = 20;

            // Teléfono
            cardForm.Controls.Add(FieldLbl("NÚMERO DE TELÉFONO", 20, iy)); iy += 20;
            var pTel = InputBoxInline(cardForm, new Point(20, iy), new Size(300, 44), out _txtTelefono, "6XX XXX XXX");
            _txtTelefono.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            _txtTelefono.GotFocus  += (s, e) => FocusBox(pTel, C_Red);
            _txtTelefono.LostFocus += (s, e) => FocusBox(pTel, C_Border);
            iy += 56;

            // Operadora
            cardForm.Controls.Add(FieldLbl("OPERADORA", 20, iy)); iy += 20;
            _pnlOperadoras = new Panel { Location = new Point(20, iy), Size = new Size(740, 50), BackColor = Color.Transparent };
            cardForm.Controls.Add(_pnlOperadoras);
            int ox = 0;
            foreach (var (nombre, ico, col) in OPERADORAS)
            {
                string n = nombre; Color c = col;
                var btn = new Button { Text = $"{ico} {n}", Location = new Point(ox, 0), Size = new Size(112, 40),
                    BackColor = C_White, ForeColor = C_Text, FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand, Tag = n };
                btn.FlatAppearance.BorderColor = C_Border; btn.FlatAppearance.BorderSize = 1;
                btn.Click += (s, ev) => SeleccionarOperadora(n, _pnlOperadoras);
                btn.MouseEnter += (s, ev) => { if (_operadoraSeleccionada != n) btn.BackColor = Color.FromArgb(250, 250, 252); };
                btn.MouseLeave += (s, ev) => { if (_operadoraSeleccionada != n) btn.BackColor = C_White; };
                Redondear(btn, 8); _pnlOperadoras.Controls.Add(btn); ox += 118;
            }
            iy += 60;

            // Error
            _lblError = new Label { Location = new Point(20, iy), Size = new Size(740, 16),
                ForeColor = C_Red, Font = new Font("Segoe UI", 8, FontStyle.Bold), Visible = false };
            cardForm.Controls.Add(_lblError); iy += 22;

            // Nota seguridad + botón
            cardForm.Controls.Add(new Label { Text = "🔒  Recarga certificada · Confirmación por SMS · 100% segura",
                ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Italic), Location = new Point(210, iy + 10), AutoSize = true });
            _btnRecargar = new Button { Text = "Recargar ahora", Location = new Point(20, iy), Size = new Size(180, 44),
                BackColor = C_Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            _btnRecargar.FlatAppearance.BorderSize = 0;
            _btnRecargar.MouseEnter += (s, e) => _btnRecargar.BackColor = C_RedDark;
            _btnRecargar.MouseLeave += (s, e) => _btnRecargar.BackColor = C_Red;
            _btnRecargar.Click += BtnRecargar_Click;
            cardForm.Controls.Add(_btnRecargar);
            cardForm.Height = iy + 56;
            BeginInvoke(new Action(() => { Redondear(_btnRecargar, 8); Redondear(pTel, 8); }));

            // Importes
            main.Controls.Add(new Label { Text = "Selecciona el importe", ForeColor = C_Text,
                Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(0, y), AutoSize = true }); y += 30;

            var flowImportes = new FlowLayoutPanel { Location = new Point(0, y), Size = new Size(780, 80),
                FlowDirection = FlowDirection.LeftToRight, WrapContents = false,
                BackColor = Color.Transparent, Padding = new Padding(0) };
            _pnlImportes = flowImportes;
            main.Controls.Add(flowImportes); y += 92;

            foreach (var (imp, bono) in IMPORTES)
            {
                decimal i = imp; string b = bono;
                var card = CrearCardImporte(i, b);
                EventHandler selImp = (s, ev) => SeleccionarImporte(i, flowImportes);
                PropagateClick(card, selImp);
                flowImportes.Controls.Add(card);
            }

            main.Height = y + 20;
        }

        private Panel CrearCardImporte(decimal imp, string bono)
        {
            var card = new Panel { Size = new Size(118, 70), BackColor = C_White, Margin = new Padding(0, 0, 10, 0), Cursor = Cursors.Hand, Tag = imp };
            card.Paint += (s, ev) => {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(card.ClientRectangle, 10)) {
                    ev.Graphics.FillPath(new SolidBrush(card.BackColor), path);
                    bool sel = _importeSeleccionado == imp;
                    ev.Graphics.DrawPath(new Pen(sel ? C_Red : C_Border, sel ? 2f : 1f), path);
                }
            };
            Redondear(card, 10);
            Action hover   = () => { if (_importeSeleccionado != imp) { card.BackColor = C_BlueSel; card.Invalidate(); } };
            Action unhover = () => { if (_importeSeleccionado != imp) { card.BackColor = C_White; card.Invalidate(); } };
            card.MouseEnter += (s, e) => hover();
            card.MouseLeave += (s, e) => unhover();
            var lblPrecio = new Label { Text = imp.ToString("C0", ES), ForeColor = _importeSeleccionado == imp ? C_Red : C_Text,
                Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(0, 10), Size = new Size(118, 24),
                TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent, Cursor = Cursors.Hand, Tag = "precio" };
            var lblBono = new Label { Text = bono, ForeColor = C_Muted, Font = new Font("Segoe UI", 7),
                Location = new Point(0, 40), Size = new Size(118, 20),
                TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent, Cursor = Cursors.Hand };
            lblPrecio.MouseEnter += (s, e) => hover(); lblPrecio.MouseLeave += (s, e) => unhover();
            lblBono.MouseEnter   += (s, e) => hover(); lblBono.MouseLeave   += (s, e) => unhover();
            card.Controls.Add(lblPrecio); card.Controls.Add(lblBono);
            return card;
        }

        private void SeleccionarImporte(decimal imp, FlowLayoutPanel grid)
        {
            _importeSeleccionado = imp;
            foreach (Panel c in grid.Controls)
            {
                bool sel = (decimal)c.Tag == imp;
                c.BackColor = sel ? Color.FromArgb(255, 240, 240) : C_White;
                foreach (Control lbl in c.Controls) if (lbl.Tag?.ToString() == "precio") lbl.ForeColor = sel ? C_Red : C_Text;
                c.Invalidate();
            }
        }

        private void SeleccionarOperadora(string nombre, Panel grid)
        {
            _operadoraSeleccionada = nombre;
            foreach (Button b in grid.Controls)
            {
                bool sel = (string)b.Tag == nombre;
                b.BackColor = sel ? Color.FromArgb(255, 240, 240) : C_White;
                b.FlatAppearance.BorderColor = sel ? C_Red : C_Border;
                b.ForeColor = sel ? C_Red : C_Text;
            }
        }

        private void BtnRecargar_Click(object sender, EventArgs e)
        {
            _lblError.Visible = false;
            if (_cmbCuenta.SelectedItem == null) { Err("Selecciona una cuenta de cargo."); return; }
            if (string.IsNullOrWhiteSpace(_txtTelefono.Text) || _txtTelefono.Text.Trim().Length < 9) { Err("Introduce un número de teléfono válido."); return; }
            if (string.IsNullOrWhiteSpace(_operadoraSeleccionada)) { Err("Selecciona la operadora del teléfono."); return; }
            if (_importeSeleccionado <= 0) { Err("Selecciona el importe de recarga."); return; }

            var ci = (CuentaItem)_cmbCuenta.SelectedItem;
            if (_importeSeleccionado > ci.Cuenta.Saldo) { Err("Saldo insuficiente para esta recarga."); return; }

            string ult4 = ci.Cuenta.NumeroCuenta?.Length > 4 ? ci.Cuenta.NumeroCuenta.Substring(ci.Cuenta.NumeroCuenta.Length - 4) : ci.Cuenta.NumeroCuenta;
            var res = MessageBox.Show(
                $"¿Confirmar recarga de {_importeSeleccionado.ToString("C2", ES)} en el número {_txtTelefono.Text.Trim()} ({_operadoraSeleccionada})?\n\nCuenta cargo: ••••{ult4}",
                "Nexum Bank — Confirmar recarga", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes) return;

            _btnRecargar.Enabled = false; _btnRecargar.Text = "Procesando...";
            bool ok = _movService.RegistrarRetiro(ci.Cuenta.Id, _importeSeleccionado,
                $"Recarga móvil {_operadoraSeleccionada} – {_txtTelefono.Text.Trim()}", out string err);
            if (ok)
            {
                MessageBox.Show($"✓  Recarga de {_importeSeleccionado.ToString("C2", ES)} enviada correctamente al {_txtTelefono.Text.Trim()}.",
                    "Nexum Bank — Recarga confirmada", MessageBoxButtons.OK, MessageBoxIcon.None);
                _txtTelefono.Text = ""; _operadoraSeleccionada = ""; _importeSeleccionado = 0;
                SeleccionarImporte(0, _pnlImportes); SeleccionarOperadora("", _pnlOperadoras);
                CargarCuentas();
            }
            else Err(err ?? "No se pudo procesar la recarga.");
            _btnRecargar.Enabled = true; _btnRecargar.Text = "Recargar ahora";
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

        private Panel MakeCard(Point loc, Size sz) {
            var p = new Panel { Location = loc, Size = sz, BackColor = C_White };
            p.Paint += (s, ev) => { ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using (var path = RR(p.ClientRectangle, 12)) { ev.Graphics.FillPath(Brushes.White, path); ev.Graphics.DrawPath(new Pen(C_Border, 1), path); } };
            BeginInvoke(new Action(() => Redondear(p, 12))); return p;
        }
        private Panel InputBoxInline(Panel parent, Point loc, Size sz, out TextBox txt, string hint) {
            var p = new Panel { Location = loc, Size = sz, BackColor = C_White, Tag = C_Border };
            p.Paint += (s, ev) => { var col = p.Tag is Color c ? c : C_Border; ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using (var path = RR(p.ClientRectangle, 8)) { ev.Graphics.FillPath(Brushes.White, path); ev.Graphics.DrawPath(new Pen(col, 1.5f), path); } };
            txt = new TextBox { Location = new Point(10, (sz.Height - 22) / 2), Size = new Size(sz.Width - 20, 24), Font = new Font("Segoe UI", 10), BackColor = C_White, ForeColor = C_Text, BorderStyle = BorderStyle.None };
            p.Controls.Add(txt); parent.Controls.Add(p); return p;
        }
        private void FocusBox(Panel p, Color col) { p.Tag = col; p.Invalidate(); }
        private Panel MakeIconCircle(Point loc, int size, Color col, string g, float fs) {
            var p = new Panel { Location = loc, Size = new Size(size, size), BackColor = Color.Transparent };
            p.Paint += (s, ev) => { ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias; ev.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(20, col.R, col.G, col.B)), p.ClientRectangle); var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }; ev.Graphics.DrawString(g, new Font("Segoe UI", fs), new SolidBrush(col), p.ClientRectangle, fmt); };
            return p;
        }
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
