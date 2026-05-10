using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace NexumApp.Views
{
    public class VistaPagarServicios : UserControl
    {
        // Colores de acento — fijos en ambos temas
        private static readonly Color C_Red      = Color.FromArgb(236, 0, 0);
        private static readonly Color C_RedDark  = Color.FromArgb(180, 0, 0);
        private static readonly Color C_RedLight = Color.FromArgb(255, 235, 235);
        private static readonly Color C_Green    = Color.FromArgb(0, 168, 89);
        // Moneda dinámica — sigue MonedaPreferida del usuario
        private CultureInfo ES => Helpers.AppSettings.CultureMoneda;
        // Colores dinámicos — se adaptan al tema activo
        private Color C_BgPage => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(10,  12,  28)  : Color.FromArgb(246, 247, 249);
        private Color C_White  => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(18,  22,  46)  : Color.White;
        private Color C_Border => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(38,  44,  80)  : Color.FromArgb(220, 224, 230);
        private Color C_Text   => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(241, 245, 249) : Color.FromArgb(30,  30,  50);
        private Color C_Muted  => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(100, 116, 139) : Color.FromArgb(110, 120, 140);

        private readonly CuentaService     _cuentaService = new CuentaService();
        private readonly MovimientoService _movService    = new MovimientoService();

        private Panel    _pnlDetalle;
        private ComboBox _cmbCuenta;
        private Label    _lblSaldo, _lblError, _lblServicioSel;
        private Button   _btnPagar;
        private TextBox  _txtReferencia, _txtMonto;
        private string   _servicioSeleccionado = "";

        private static readonly (string Nombre, string Icono, string Desc, string Ref)[] SERVICIOS = {
            ("Electricidad", "⚡", "Iberdrola / Endesa",  "ELEC-"),
            ("Agua",         "💧", "Canal Isabel II",     "AGUA-"),
            ("Gas",          "🔥", "Naturgy / Repsol",    "GAS-"),
            ("Internet",     "🌐", "Movistar / Orange",   "INET-"),
            ("Netflix",      "🎬", "Netflix Suscripción", "NFLX-"),
            ("Spotify",      "🎵", "Spotify Premium",     "SPFY-"),
            ("Seguro Hogar", "🏠", "Mapfre / AXA",        "SEG-"),
            ("Comunidad",    "🏢", "IBI / Comunidad",     "COM-"),
        };

        public VistaPagarServicios()
        {
            BackColor = C_BgPage; Dock = DockStyle.Fill; DoubleBuffered = true;
            Helpers.AppSettings.ConfiguracionChanged += (s, e) =>
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(new Action(() =>
                {
                    BuildUI();
                    CargarCuentas();
                    Helpers.AppSettings.AplicarTraduccionesRecursivo(this);
                }));
            };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            BuildUI();
            CargarCuentas();
            Helpers.AppSettings.AplicarTraduccionesRecursivo(this);
        }

        private void BuildUI()
        {
            Controls.Clear();
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = C_BgPage };
            var main   = new Panel { Width = 860, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                                     BackColor = Color.Transparent };
            scroll.Controls.Add(main);
            scroll.Resize += (s, ev) => { main.Left = Math.Max(32, (scroll.ClientSize.Width - 860) / 2); };
            Controls.Add(scroll);
            int y = 28;

            // Cabecera
            var icoHdr = MakeIconCircle(new Point(0, 6), 48, C_Red, "💳", 20f);
            main.Controls.Add(icoHdr);
            main.Controls.Add(new Label { Text = "Pagar servicios", ForeColor = C_Text,
                Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(62, 8), AutoSize = true });
            main.Controls.Add(new Label { Text = "Paga tus facturas de forma rápida y segura", ForeColor = C_Muted,
                Font = new Font("Segoe UI", 10), Location = new Point(62, 38), AutoSize = true }); y += 76;

            // Card cuenta
            var cardCuenta = MakeCard(new Point(0, y), new Size(800, 86)); main.Controls.Add(cardCuenta); y += 100;
            cardCuenta.Controls.Add(FieldLbl("CUENTA DE CARGO", 20, 14));
            _cmbCuenta = new ComboBox { Location = new Point(20, 34), Size = new Size(500, 28),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10),
                BackColor = C_White, ForeColor = C_Text, FlatStyle = FlatStyle.Flat };
            _cmbCuenta.SelectedIndexChanged += (s, ev) => ActualizarSaldo();
            cardCuenta.Controls.Add(_cmbCuenta);
            _lblSaldo = new Label { Location = new Point(20, 66), AutoSize = true,
                ForeColor = C_Green, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            cardCuenta.Controls.Add(_lblSaldo);

            // Título grid
            main.Controls.Add(new Label { Text = "Selecciona el servicio", ForeColor = C_Text,
                Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(0, y), AutoSize = true }); y += 30;

            var grid = new FlowLayoutPanel { Location = new Point(0, y), Size = new Size(800, 230),
                FlowDirection = FlowDirection.LeftToRight, WrapContents = true,
                BackColor = Color.Transparent, Padding = new Padding(0) };
            main.Controls.Add(grid); y += 242;

            foreach (var (nombre, icono, desc, refPre) in SERVICIOS)
            {
                string n = nombre; string ico = icono; string d = desc; string r = refPre;
                var card = CrearCardServicio(n, ico, d);
                EventHandler sel = (s, ev) => SeleccionarServicio(n, ico, r, grid);
                PropagateClick(card, sel);
                grid.Controls.Add(card);
            }

            // Panel detalle
            _pnlDetalle = MakeCard(new Point(0, y), new Size(800, 200));
            _pnlDetalle.Visible = false;
            main.Controls.Add(_pnlDetalle); y += 214;

            BuildPanelDetalle();
            main.Height = y + 20;
        }

        private void BuildPanelDetalle()
        {
            _pnlDetalle.Controls.Clear();
            int iy = 18;

            _lblServicioSel = new Label { ForeColor = C_Text, Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, iy), AutoSize = true };
            _pnlDetalle.Controls.Add(_lblServicioSel); iy += 34;
            _pnlDetalle.Controls.Add(new Panel { Location = new Point(20, iy), Size = new Size(760, 1), BackColor = C_Border }); iy += 14;

            _pnlDetalle.Controls.Add(FieldLbl("REFERENCIA / Nº CONTRATO", 20, iy));
            _pnlDetalle.Controls.Add(FieldLbl("IMPORTE (€)", 410, iy)); iy += 20;

            var pRef = InputBoxInline(_pnlDetalle, new Point(20, iy), new Size(370, 40), out _txtReferencia, "Ej: 00123456789");
            var pMon = InputBoxInline(_pnlDetalle, new Point(410, iy), new Size(370, 40), out _txtMonto, "0,00");
            _txtMonto.Font = new Font("Segoe UI", 13, FontStyle.Bold); _txtMonto.ForeColor = C_Red;
            _txtMonto.GotFocus  += (s, e) => { if (_txtMonto.Text == "0,00") _txtMonto.Clear(); FocusBox(pMon, C_Red); };
            _txtMonto.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(_txtMonto.Text)) _txtMonto.Text = "0,00"; FocusBox(pMon, C_Border); };
            _txtReferencia.GotFocus  += (s, e) => FocusBox(pRef, C_Red);
            _txtReferencia.LostFocus += (s, e) => FocusBox(pRef, C_Border);
            iy += 52;

            _lblError = new Label { Location = new Point(20, iy), Size = new Size(760, 16),
                ForeColor = C_Red, Font = new Font("Segoe UI", 8, FontStyle.Bold), Visible = false };
            _pnlDetalle.Controls.Add(_lblError); iy += 20;

            _pnlDetalle.Controls.Add(new Label { Text = "🔒  Pago protegido · Confirmación inmediata · SSL cifrado",
                ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Italic),
                Location = new Point(220, iy + 8), AutoSize = true });

            _btnPagar = new Button { Text = "Pagar ahora", Location = new Point(20, iy), Size = new Size(180, 44),
                BackColor = C_Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            _btnPagar.FlatAppearance.BorderSize = 0;
            _btnPagar.MouseEnter += (s, e) => _btnPagar.BackColor = C_RedDark;
            _btnPagar.MouseLeave += (s, e) => _btnPagar.BackColor = C_Red;
            _btnPagar.Click += BtnPagar_Click;
            _pnlDetalle.Controls.Add(_btnPagar);

            BeginInvoke(new Action(() => { Redondear(_btnPagar, 8); Redondear(pRef, 8); Redondear(pMon, 8); }));
        }

        private void SeleccionarServicio(string nombre, string icono, string refPrefix, FlowLayoutPanel grid)
        {
            _servicioSeleccionado   = nombre;
            _lblServicioSel.Text    = $"{icono}  Pagar {nombre}";
            _txtReferencia.Text     = refPrefix;
            _txtMonto.Text          = "0,00";
            _lblError.Visible       = false;
            _pnlDetalle.Visible     = true;
            foreach (Panel c in grid.Controls) { c.BackColor = (string)c.Tag == nombre ? C_RedLight : C_White; c.Invalidate(); }
        }

        private void BtnPagar_Click(object sender, EventArgs e)
        {
            _lblError.Visible = false;
            if (_cmbCuenta.SelectedItem == null) { Err("Selecciona una cuenta de cargo."); return; }
            if (string.IsNullOrWhiteSpace(_txtReferencia.Text)) { Err("Introduce la referencia del servicio."); return; }
            string ms = _txtMonto.Text.Trim().Replace(",", ".").Replace(" ", "");
            if (!decimal.TryParse(ms, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out decimal monto) || monto <= 0)
            { Err("Introduce un importe válido mayor que 0 €."); return; }
            var ci = (CuentaItem)_cmbCuenta.SelectedItem;
            if (monto > ci.Cuenta.Saldo) { Err("Saldo insuficiente en la cuenta seleccionada."); return; }

            string ult4 = ci.Cuenta.NumeroCuenta?.Length > 4 ? ci.Cuenta.NumeroCuenta.Substring(ci.Cuenta.NumeroCuenta.Length - 4) : ci.Cuenta.NumeroCuenta;
            var res = MessageBox.Show(
                $"¿Confirmar pago de {monto.ToString("C2", ES)} para {_servicioSeleccionado}?\n\nReferencia: {_txtReferencia.Text.Trim()}\nCuenta cargo: ••••{ult4}",
                "Nexum Bank — Confirmar pago", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes) return;

            _btnPagar.Enabled = false; _btnPagar.Text = "Procesando...";
            bool ok = _movService.RegistrarRetiro(ci.Cuenta.Id, monto, $"Pago {_servicioSeleccionado} – {_txtReferencia.Text.Trim()}", out string err);
            if (ok)
            {
                MessageBox.Show($"✓  Pago de {monto.ToString("C2", ES)} realizado correctamente.\nServicio: {_servicioSeleccionado}",
                    "Nexum Bank — Pago confirmado", MessageBoxButtons.OK, MessageBoxIcon.None);
                _txtMonto.Text = "0,00"; _txtReferencia.Text = ""; _pnlDetalle.Visible = false; CargarCuentas();
            }
            else Err(err ?? "No se pudo procesar el pago.");
            _btnPagar.Enabled = true; _btnPagar.Text = "Pagar ahora";
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

        private Panel CrearCardServicio(string nombre, string icono, string desc)
        {
            var card = new Panel { Size = new Size(186, 100), BackColor = C_White, Margin = new Padding(0, 0, 12, 12), Cursor = Cursors.Hand, Tag = nombre };
            card.Paint += (s, ev) => {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(card.ClientRectangle, 12)) {
                    ev.Graphics.FillPath(new SolidBrush(card.BackColor), path);
                    Color bord = card.BackColor == C_RedLight ? C_Red : C_Border;
                    ev.Graphics.DrawPath(new Pen(bord, card.BackColor == C_RedLight ? 2f : 1f), path);
                }
            };
            Redondear(card, 12);
            Action hover   = () => { if (card.BackColor != C_RedLight) { card.BackColor = Color.FromArgb(252, 252, 253); card.Invalidate(); } };
            Action unhover = () => { if (card.BackColor != C_RedLight) { card.BackColor = C_White; card.Invalidate(); } };
            card.MouseEnter += (s, e) => hover();
            card.MouseLeave += (s, e) => unhover();

            foreach (var lbl in new[] {
                new Label { Text = icono, Font = new Font("Segoe UI", 24), Location = new Point(14, 10), AutoSize = true, BackColor = Color.Transparent, Cursor = Cursors.Hand },
                new Label { Text = nombre, ForeColor = C_Text, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(14, 52), AutoSize = true, BackColor = Color.Transparent, Cursor = Cursors.Hand },
                new Label { Text = desc, ForeColor = C_Muted, Font = new Font("Segoe UI", 7), Location = new Point(14, 72), Size = new Size(158, 22), BackColor = Color.Transparent, Cursor = Cursors.Hand },
            }) {
                lbl.MouseEnter += (s, e) => hover();
                lbl.MouseLeave += (s, e) => unhover();
                card.Controls.Add(lbl);
            }
            return card;
        }

        private Panel MakeCard(Point loc, Size sz)
        {
            var p = new Panel { Location = loc, Size = sz, BackColor = C_White };
            p.Paint += (s, ev) => {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(p.ClientRectangle, 12)) { ev.Graphics.FillPath(Brushes.White, path); ev.Graphics.DrawPath(new Pen(C_Border, 1), path); }
            };
            BeginInvoke(new Action(() => Redondear(p, 12)));
            return p;
        }

        private Panel InputBoxInline(Panel parent, Point loc, Size sz, out TextBox txt, string hint)
        {
            var p = new Panel { Location = loc, Size = sz, BackColor = C_White, Tag = C_Border };
            p.Paint += (s, ev) => {
                var col = p.Tag is Color c ? c : C_Border;
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(p.ClientRectangle, 8)) { ev.Graphics.FillPath(Brushes.White, path); ev.Graphics.DrawPath(new Pen(col, 1.5f), path); }
            };
            txt = new TextBox { Location = new Point(10, (sz.Height - 22) / 2), Size = new Size(sz.Width - 20, 24),
                Font = new Font("Segoe UI", 10), BackColor = C_White, ForeColor = C_Text, BorderStyle = BorderStyle.None };
            p.Controls.Add(txt); parent.Controls.Add(p); return p;
        }

        private void FocusBox(Panel p, Color col) { p.Tag = col; p.Invalidate(); }
        private Panel MakeIconCircle(Point loc, int size, Color col, string g, float fs)
        {
            var p = new Panel { Location = loc, Size = new Size(size, size), BackColor = Color.Transparent };
            p.Paint += (s, ev) => {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                ev.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(20, col.R, col.G, col.B)), p.ClientRectangle);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString(g, new Font("Segoe UI", fs), new SolidBrush(col), p.ClientRectangle, fmt);
            };
            return p;
        }
        private Label FieldLbl(string t, int x, int y) => new Label { Text = t, Location = new Point(x, y), ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true };
        private void Err(string msg) { _lblError.Text = "⚠  " + msg; _lblError.Visible = true; }
        private static GraphicsPath RR(Rectangle r, int rad)
        {
            int d = rad * 2; var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseAllFigures(); return p;
        }
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
