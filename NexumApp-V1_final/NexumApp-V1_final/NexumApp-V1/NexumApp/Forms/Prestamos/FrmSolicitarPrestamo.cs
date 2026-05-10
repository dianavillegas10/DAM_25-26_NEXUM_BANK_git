using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NexumApp.Forms.Prestamos
{
    internal class FrmSolicitarPrestamo : Form
    {
        private static readonly Color Indigo     = Color.FromArgb(99,  102, 241);
        private static readonly Color IndigoDark = Color.FromArgb(49,  46,  129);
        private static readonly Color White      = Color.White;
        private static readonly Color BgGray     = Color.FromArgb(244, 246, 252);
        private static readonly Color TextDark   = Color.FromArgb(17,  24,  39);
        private static readonly Color TextGray   = Color.FromArgb(107, 114, 128);
        private static readonly Color Border     = Color.FromArgb(229, 231, 235);
        private static readonly Color GreenOk    = Color.FromArgb(16,  185, 129);

        private readonly PrestamoService _service = new PrestamoService();
        private readonly CuentaService   _cuentaSvc = new CuentaService();

        private readonly string[] _tipos  = { "Personal", "Hipoteca", "Coche", "Estudios" };
        private readonly string[] _emojis = { "💼",       "🏠",       "🚗",    "🎓" };
        private Panel[]   _tipoCards;
        private string    _tipoSel;

        private ComboBox  _cmbCuenta;
        private TextBox   _txtMonto;
        private ComboBox  _cmbPlazo;
        private Label     _lblCuotaCalc;
        private Label     _lblResumen;
        private Button    _btnSolicitar;

        public FrmSolicitarPrestamo(string tipoInicial = "Personal", int montoInicial = 10000, int plazoInicial = 60)
        {
            _tipoSel = tipoInicial;
            ConfigurarForm();
            ConstruirUI(montoInicial, plazoInicial);
        }

        private void ConfigurarForm()
        {
            Text            = "Solicitar Préstamo — Nexum Bank";
            Size            = new Size(560, 680);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            BackColor       = BgGray;
            MaximizeBox     = false;
            Load += (s, e) =>
            {
                if (Width <= 0 || Height <= 0) return;
                var p = new GraphicsPath(); int r = 20;
                p.AddArc(0, 0, r, r, 180, 90); p.AddArc(Width - r, 0, r, r, 270, 90);
                p.AddArc(Width - r, Height - r, r, r, 0, 90); p.AddArc(0, Height - r, r, r, 90, 90);
                p.CloseFigure(); Region = new Region(p);
            };
        }

        private void ConstruirUI(int montoInicial, int plazoInicial)
        {
            // ── Header ────────────────────────────────────────────────
            var pnlHeader = new Panel { Size = new Size(560, 80), Location = Point.Empty };
            pnlHeader.Paint += (s, ev) =>
            {
                var g = ev.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new LinearGradientBrush(Point.Empty, new Point(560, 80), IndigoDark, Indigo))
                    g.FillRectangle(br, pnlHeader.ClientRectangle);
                using (var br2 = new SolidBrush(Color.FromArgb(20, 255, 255, 255)))
                { g.FillEllipse(br2, 420, -40, 200, 200); g.FillEllipse(br2, 500, 30, 100, 100); }
            };
            var btnX = new Button { Text = "✕", Size = new Size(30, 30), Location = new Point(520, 10), BackColor = Color.FromArgb(60, 255, 255, 255), ForeColor = White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnX.FlatAppearance.BorderSize = 0; btnX.Click += (s, e) => Close();
            pnlHeader.Controls.Add(new Label { Text = "💳  Solicitar Préstamo", ForeColor = White, Font = new Font("Segoe UI", 15, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(24, 12) });
            pnlHeader.Controls.Add(new Label { Text = "Nexum Bank · Condiciones personalizadas", ForeColor = Color.FromArgb(165, 180, 252), Font = new Font("Segoe UI", 9), AutoSize = true, BackColor = Color.Transparent, Location = new Point(26, 46) });
            pnlHeader.Controls.Add(btnX);

            // ── Panel scroll ──────────────────────────────────────────
            var scroll = new Panel { Location = new Point(0, 80), Size = new Size(560, 504), AutoScroll = true, BackColor = BgGray };

            int y = 12;

            // Tipo
            scroll.Controls.Add(new Label { Text = "TIPO DE PRÉSTAMO", ForeColor = TextGray, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(16, y) });
            y += 22;
            var flpTipos = new FlowLayoutPanel { Location = new Point(16, y), Size = new Size(528, 76), FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent };
            _tipoCards = new Panel[4];
            for (int i = 0; i < 4; i++) { var card = CrearTipoCard(i); _tipoCards[i] = card; flpTipos.Controls.Add(card); }
            scroll.Controls.Add(flpTipos);
            y += 88;

            // Cuenta
            scroll.Controls.Add(new Label { Text = "CUENTA DESTINO *", ForeColor = TextGray, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(16, y) });
            y += 20;
            _cmbCuenta = new ComboBox { Location = new Point(16, y), Size = new Size(528, 28), Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat, BackColor = White, DropDownStyle = ComboBoxStyle.DropDownList };
            CargarCuentas();
            scroll.Controls.Add(_cmbCuenta);
            y += 46;

            // Importe + Plazo en dos columnas
            scroll.Controls.Add(new Label { Text = "IMPORTE SOLICITADO (€) *", ForeColor = TextGray, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(16, y) });
            scroll.Controls.Add(new Label { Text = "PLAZO (MESES) *", ForeColor = TextGray, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(290, y) });
            y += 20;
            _txtMonto = new TextBox { Location = new Point(16, y), Size = new Size(258, 28), Font = new Font("Segoe UI", 11), BackColor = White, ForeColor = TextDark, BorderStyle = BorderStyle.FixedSingle, Text = montoInicial.ToString() };
            _txtMonto.TextChanged += (s, ev) => ActualizarCuota();
            _cmbPlazo = new ComboBox { Location = new Point(290, y), Size = new Size(254, 28), Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat, BackColor = White, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var p in new[] { 6, 12, 18, 24, 36, 48, 60, 84, 120, 180, 240, 300, 360 })
                _cmbPlazo.Items.Add($"{p} meses");
            _cmbPlazo.SelectedIndex = _cmbPlazo.Items.IndexOf($"{plazoInicial} meses");
            if (_cmbPlazo.SelectedIndex < 0) _cmbPlazo.SelectedIndex = 4;
            _cmbPlazo.SelectedIndexChanged += (s, ev) => ActualizarCuota();
            scroll.Controls.Add(_txtMonto);
            scroll.Controls.Add(_cmbPlazo);
            y += 48;

            // Panel cuota calculada
            var pnlCuota = new Panel { Location = new Point(16, y), Size = new Size(528, 80), BackColor = Color.FromArgb(238, 242, 255) };
            pnlCuota.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RRect(new Rectangle(0, 0, pnlCuota.Width, pnlCuota.Height), 12))
                    ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(238, 242, 255)), path);
            };
            pnlCuota.Controls.Add(new Label { Text = "Cuota mensual estimada", ForeColor = Indigo, Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(16, 10) });
            _lblCuotaCalc = new Label { Text = "—", ForeColor = Indigo, Font = new Font("Segoe UI", 22, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, Location = new Point(16, 32) };
            pnlCuota.Controls.Add(_lblCuotaCalc);
            _lblResumen = new Label { Text = "", ForeColor = TextGray, Font = new Font("Segoe UI", 8), AutoSize = true, BackColor = Color.Transparent };
            pnlCuota.Controls.Add(_lblResumen);
            Action posResumen = () => _lblResumen.Location = new Point(pnlCuota.Width - _lblResumen.Width - 14, (pnlCuota.Height - _lblResumen.Height) / 2);
            pnlCuota.Resize += (s, ev) => posResumen();
            pnlCuota.HandleCreated += (s, ev) => posResumen();
            scroll.Controls.Add(pnlCuota);
            y += 96;

            // Aviso
            scroll.Controls.Add(new Label { Text = "⚠  TAE orientativa. Las condiciones finales pueden variar según tu perfil de riesgo.", ForeColor = TextGray, Font = new Font("Segoe UI", 8), AutoSize = false, Size = new Size(528, 18), BackColor = Color.Transparent, Location = new Point(16, y) });

            // ── Botones ───────────────────────────────────────────────
            var pnlBotones = new Panel { Location = new Point(0, 584), Size = new Size(560, 96), BackColor = BgGray };
            pnlBotones.Paint += (s, ev) => { using (var pen = new Pen(Border, 1)) ev.Graphics.DrawLine(pen, 16, 0, 544, 0); };
            _btnSolicitar = new Button { Text = "SOLICITAR PRÉSTAMO", Size = new Size(528, 46), Location = new Point(16, 12), BackColor = Indigo, ForeColor = White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            _btnSolicitar.FlatAppearance.BorderSize = 0;
            _btnSolicitar.Click += BtnSolicitar_Click;
            var btnCancelar = new Button { Text = "Cancelar", Size = new Size(528, 28), Location = new Point(16, 64), BackColor = Color.Transparent, ForeColor = TextGray, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand };
            btnCancelar.FlatAppearance.BorderSize = 0; btnCancelar.Click += (s, e) => Close();
            pnlBotones.Controls.Add(_btnSolicitar);
            pnlBotones.Controls.Add(btnCancelar);

            Controls.Add(pnlHeader);
            Controls.Add(scroll);
            Controls.Add(pnlBotones);

            Load += (s, e) => ActualizarCuota();
        }

        private Panel CrearTipoCard(int idx)
        {
            var card = new Panel { Size = new Size(124, 72), Margin = new Padding(0, 0, 6, 0), BackColor = White, Cursor = Cursors.Hand };
            bool sel() => _tipoSel == _tipos[idx];
            card.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color bg  = sel() ? Color.FromArgb(238, 242, 255) : White;
                Color brd = sel() ? Indigo : Border;
                using (var path = RRect(new Rectangle(0, 0, card.Width, card.Height), 10))
                { ev.Graphics.FillPath(new SolidBrush(bg), path); ev.Graphics.DrawPath(new Pen(brd, sel() ? 2f : 1f), path); }
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var f = new Font("Segoe UI", 18)) ev.Graphics.DrawString(_emojis[idx], f, Brushes.Black, new RectangleF(0, 4, card.Width, 32), fmt);
                using (var f = new Font("Segoe UI", 8, FontStyle.Bold))
                using (var br = new SolidBrush(sel() ? Indigo : TextGray))
                    ev.Graphics.DrawString(_tipos[idx], f, br, new RectangleF(0, 46, card.Width, 14), fmt);
                using (var f = new Font("Segoe UI", 7))
                using (var br = new SolidBrush(sel() ? Indigo : TextGray))
                    ev.Graphics.DrawString($"{PrestamoService.ObtenerTasa(_tipos[idx])}% TIN", f, br, new RectangleF(0, 60, card.Width, 12), fmt);
            };
            card.Click += (s, ev) => { _tipoSel = _tipos[idx]; foreach (var c in _tipoCards) c?.Invalidate(); ActualizarCuota(); };
            return card;
        }

        private void CargarCuentas()
        {
            int uid = SesionActual.Instancia?.Usuario?.Id ?? 0;
            var cuentas = _cuentaSvc.ObtenerCuentasPorUsuario(uid) ?? new System.Collections.Generic.List<CuentaBancaria>();
            _cmbCuenta.DataSource    = cuentas;
            _cmbCuenta.DisplayMember = "NumeroCuenta";
            _cmbCuenta.ValueMember   = "Id";
        }

        private void ActualizarCuota()
        {
            if (_txtMonto == null || _cmbPlazo == null) return;
            if (!decimal.TryParse(_txtMonto.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal monto) || monto <= 0)
            { if (_lblCuotaCalc != null) _lblCuotaCalc.Text = "—"; return; }
            if (_cmbPlazo.SelectedItem == null) return;
            int plazo = int.Parse(_cmbPlazo.SelectedItem.ToString().Split(' ')[0]);
            decimal tasa  = PrestamoService.ObtenerTasa(_tipoSel);
            decimal cuota = PrestamoService.CalcularCuota(monto, plazo, tasa);
            decimal total = cuota * plazo;
            if (_lblCuotaCalc != null) _lblCuotaCalc.Text = $"{cuota:N2} €/mes";
            if (_lblResumen   != null) { _lblResumen.Text = $"Total: {total:N0} €  ·  Intereses: {(total - monto):N0} €  ·  TIN {tasa}%"; _lblResumen.Parent?.Invalidate(); }
        }

        private void BtnSolicitar_Click(object sender, EventArgs e)
        {
            if (_cmbCuenta.SelectedItem == null) { MessageBox.Show("Selecciona una cuenta destino.", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!decimal.TryParse(_txtMonto.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal monto) || monto < 500)
            { MessageBox.Show("El importe mínimo es 500 €.", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning); _txtMonto.Focus(); return; }
            if (monto > 300000) { MessageBox.Show("El importe máximo es 300.000 €.", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (_cmbPlazo.SelectedItem == null) { MessageBox.Show("Selecciona un plazo.", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            int plazo = int.Parse(_cmbPlazo.SelectedItem.ToString().Split(' ')[0]);
            decimal tasa  = PrestamoService.ObtenerTasa(_tipoSel);
            decimal cuota = PrestamoService.CalcularCuota(monto, plazo, tasa);
            var cuenta    = (CuentaBancaria)_cmbCuenta.SelectedItem;

            _btnSolicitar.Enabled = false;
            _btnSolicitar.Text    = "Procesando...";

            var prestamo = new Prestamo
            {
                UsuarioId       = SesionActual.Instancia?.Usuario?.Id ?? 0,
                CuentaId        = cuenta.Id,
                TipoPrestamo    = _tipoSel,
                MontoSolicitado = monto,
                PlazoMeses      = plazo,
                TasaInteres     = tasa,
                CuotaMensual    = cuota
            };

            var (exito, error) = _service.Solicitar(prestamo);
            if (exito)
            {
                using (var frm = new FrmPrestamoAprobado(_tipoSel, monto, cuota, plazo))
                {
                    frm.ShowDialog(this);
                }
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show($"Error al procesar el préstamo:\n{error}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _btnSolicitar.Enabled = true;
                _btnSolicitar.Text    = "SOLICITAR PRÉSTAMO";
            }
        }

        private static GraphicsPath RRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath(); int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90); path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure(); return path;
        }
    }
}
