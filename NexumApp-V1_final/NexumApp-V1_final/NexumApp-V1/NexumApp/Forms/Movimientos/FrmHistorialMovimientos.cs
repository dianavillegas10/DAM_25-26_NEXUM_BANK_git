using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace NexumApp.Forms.Movimientos
{
    internal partial class FrmHistorialMovimientos : Form
    {
        private readonly MovimientoService _movimientoService = new MovimientoService();
        private readonly CuentaService _cuentaService = new CuentaService();
        private CuentaBancaria _cuentaInicial;
        private List<Movimiento> _movimientos = new List<Movimiento>();

        // Paleta dark
        private static readonly Color BgDark     = Color.FromArgb(10,  12,  28);
        private static readonly Color BgCard     = Color.FromArgb(18,  22,  45);
        private static readonly Color BgRow      = Color.FromArgb(22,  27,  52);
        private static readonly Color AccentBlue = Color.FromArgb(99,  102, 241);
        private static readonly Color AccentGreen= Color.FromArgb(52,  211, 153);
        private static readonly Color AccentRed  = Color.FromArgb(248, 113, 113);
        private static readonly Color BorderCol  = Color.FromArgb(40,  46,  80);
        private static readonly Color TextPrimary= Color.FromArgb(241, 245, 249);
        private static readonly Color TextMuted  = Color.FromArgb(100, 116, 139);

        // Controles
        private ComboBox _cmbCuentas;
        private Panel _panelMovimientos;
        private Label _lblSaldo, _lblIngresos, _lblGastos, _lblCount;
        private TextBox _txtBuscar;
        private string _filtroBusqueda = "";

        public FrmHistorialMovimientos(CuentaBancaria cuenta = null)
        {
            _cuentaInicial = cuenta;
            DoubleBuffered = true;
            ConstruirUI();
            CargarCuentas();
        }

        private void ConstruirUI()
        {
            Size = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            Text = "Historial de movimientos — Nexum Bank";
            BackColor = BgDark;
            MinimumSize = new Size(800, 560);

            // ── Header ──
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = BgCard };
            pnlHeader.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BorderCol, 1))
                    ev.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };
            pnlHeader.Controls.Add(new Label { Text = "Historial de movimientos", ForeColor = TextPrimary, Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(24, 18), AutoSize = true });

            // Selector cuenta en header
            _cmbCuentas = new ComboBox
            {
                Size = new Size(280, 30), Location = new Point(440, 20),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(24, 28, 55), ForeColor = TextPrimary, FlatStyle = FlatStyle.Flat
            };
            _cmbCuentas.SelectedIndexChanged += (s, ev) => CargarMovimientos();
            pnlHeader.Controls.Add(_cmbCuentas);

            // Buscar
            _txtBuscar = new TextBox
            {
                Size = new Size(200, 30), Location = new Point(740, 20),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(24, 28, 55), ForeColor = TextPrimary,
                BorderStyle = BorderStyle.FixedSingle, Text = ""
            };
            _txtBuscar.TextChanged += (s, ev) => { _filtroBusqueda = _txtBuscar.Text.ToLower(); RenderMovimientos(); };
            pnlHeader.Controls.Add(_txtBuscar);
            Controls.Add(pnlHeader);

            // ── Stats bar ──
            var pnlStats = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = BgDark, Padding = new Padding(20, 10, 20, 10) };
            pnlStats.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BorderCol, 1))
                    ev.Graphics.DrawLine(pen, 0, pnlStats.Height - 1, pnlStats.Width, pnlStats.Height - 1);
            };

            var tlpStats = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = Color.Transparent };
            for (int i = 0; i < 4; i++) tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            _lblSaldo    = CrearStatLabel("SALDO ACTUAL", "—", TextPrimary);
            _lblIngresos = CrearStatLabel("INGRESOS TOTALES", "—", AccentGreen);
            _lblGastos   = CrearStatLabel("GASTOS TOTALES", "—", AccentRed);
            _lblCount    = CrearStatLabel("MOVIMIENTOS", "—", AccentBlue);

            tlpStats.Controls.Add(_lblSaldo, 0, 0);
            tlpStats.Controls.Add(_lblIngresos, 1, 0);
            tlpStats.Controls.Add(_lblGastos, 2, 0);
            tlpStats.Controls.Add(_lblCount, 3, 0);
            pnlStats.Controls.Add(tlpStats);
            Controls.Add(pnlStats);

            // ── Cabecera tabla ──
            var pnlTablaHeader = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.FromArgb(14, 16, 36) };
            pnlTablaHeader.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                ev.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(14, 16, 36)), pnlTablaHeader.ClientRectangle);
                using (var pen = new Pen(BorderCol, 1))
                    ev.Graphics.DrawLine(pen, 0, pnlTablaHeader.Height - 1, pnlTablaHeader.Width, pnlTablaHeader.Height - 1);
                // Columnas
                string[] cols = { "TIPO", "CONCEPTO", "FECHA", "SALDO ANT.", "IMPORTE" };
                int[] xs = { 60, 160, 390, 560, 700 };
                for (int i = 0; i < cols.Length; i++)
                {
                    var fmt = new StringFormat { Alignment = i == 4 ? StringAlignment.Far : StringAlignment.Near };
                    ev.Graphics.DrawString(cols[i], new Font("Segoe UI", 8, FontStyle.Bold), new SolidBrush(TextMuted), new RectangleF(xs[i], 10, 150, 20), fmt);
                }
            };
            Controls.Add(pnlTablaHeader);

            // ── Panel movimientos (scroll) ──
            _panelMovimientos = new Panel { Dock = DockStyle.Fill, BackColor = BgDark, AutoScroll = true, Padding = new Padding(0, 4, 0, 4) };
            Controls.Add(_panelMovimientos);
        }

        private Label CrearStatLabel(string titulo, string valor, Color colorValor)
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Margin = new Padding(4, 4, 4, 4) };
            pnl.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(pnl.ClientRectangle, 10))
                    ev.Graphics.FillPath(new SolidBrush(BgCard), path);
            };
            var lblT = new Label { Text = titulo, ForeColor = TextMuted, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(12, 8), AutoSize = true };
            var lblV = new Label { Text = valor, ForeColor = colorValor, Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(12, 26), AutoSize = true, Name = "val_" + titulo };
            pnl.Controls.Add(lblT);
            pnl.Controls.Add(lblV);

            // Retornamos el label del valor para actualizarlo
            return lblV;
        }

        private void CargarCuentas()
        {
            if (SesionActual.Instancia?.Usuario == null) return;
            var cuentas = _cuentaService.ObtenerCuentasPorUsuario(SesionActual.Instancia.Usuario.Id);
            _cmbCuentas.Items.Clear();
            foreach (var c in cuentas)
                _cmbCuentas.Items.Add(new CuentaItem { Cuenta = c, Display = $"{c.TipoCuenta}  ·  •••• {(c.NumeroCuenta?.Length > 4 ? c.NumeroCuenta.Substring(c.NumeroCuenta.Length - 4) : c.NumeroCuenta)}" });
            _cmbCuentas.DisplayMember = "Display";

            if (_cuentaInicial != null)
                for (int i = 0; i < _cmbCuentas.Items.Count; i++)
                    if (((CuentaItem)_cmbCuentas.Items[i]).Cuenta.Id == _cuentaInicial.Id) { _cmbCuentas.SelectedIndex = i; break; }
            else if (_cmbCuentas.Items.Count > 0) _cmbCuentas.SelectedIndex = 0;
        }

        private void CargarMovimientos()
        {
            if (_cmbCuentas.SelectedItem == null) return;
            var cuenta = ((CuentaItem)_cmbCuentas.SelectedItem).Cuenta;
            try { _movimientos = _movimientoService.ObtenerMovimientosPorCuenta(cuenta.Id, 200) ?? new List<Movimiento>(); }
            catch { _movimientos = new List<Movimiento>(); }
            ActualizarStats(cuenta);
            RenderMovimientos();
        }

        private void ActualizarStats(CuentaBancaria cuenta)
        {
            decimal ingresos = _movimientos.Where(m => m.TipoMovimiento == "Ingreso").Sum(m => m.Monto);
            decimal gastos   = _movimientos.Where(m => m.TipoMovimiento != "Ingreso").Sum(m => m.Monto);
            var fmt = CultureInfo.CreateSpecificCulture("es-ES");

            _lblSaldo.Text    = cuenta.Saldo.ToString("C2", fmt);
            _lblIngresos.Text = ingresos.ToString("C2", fmt);
            _lblGastos.Text   = gastos.ToString("C2", fmt);
            _lblCount.Text    = _movimientos.Count.ToString();
        }

        private void RenderMovimientos()
        {
            _panelMovimientos.Controls.Clear();

            var lista = string.IsNullOrEmpty(_filtroBusqueda)
                ? _movimientos
                : _movimientos.Where(m => (m.Concepto ?? "").ToLower().Contains(_filtroBusqueda) || (m.TipoMovimiento ?? "").ToLower().Contains(_filtroBusqueda)).ToList();

            if (lista.Count == 0)
            {
                _panelMovimientos.Controls.Add(new Label
                {
                    Text = string.IsNullOrEmpty(_filtroBusqueda) ? "Sin movimientos en esta cuenta." : "No se encontraron resultados.",
                    ForeColor = TextMuted, Font = new Font("Segoe UI", 11, FontStyle.Italic),
                    TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill
                });
                return;
            }

            int y = 4;
            bool altRow = false;
            foreach (var mov in lista)
            {
                var fila = CrearFilaMovimiento(mov, altRow);
                fila.Location = new Point(0, y);
                _panelMovimientos.Controls.Add(fila);
                y += 52;
                altRow = !altRow;
            }
        }

        private Panel CrearFilaMovimiento(Movimiento mov, bool altRow)
        {
            bool esIngreso = mov.TipoMovimiento?.Equals("Ingreso", StringComparison.OrdinalIgnoreCase) == true;
            Color col = esIngreso ? AccentGreen : AccentRed;
            string signo = esIngreso ? "+" : "-";
            Color bgColor = altRow ? Color.FromArgb(20, 24, 48) : BgRow;

            var fila = new Panel { Size = new Size(1200, 48), BackColor = bgColor };
            fila.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BorderCol, 1))
                    ev.Graphics.DrawLine(pen, 0, fila.Height - 1, fila.Width, fila.Height - 1);
                // Highlight hover
                if (fila.ClientRectangle.Contains(fila.PointToClient(Cursor.Position)))
                    ev.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(15, 99, 102, 241)), fila.ClientRectangle);
            };
            fila.MouseEnter += (s, ev) => fila.Invalidate();
            fila.MouseLeave += (s, ev) => fila.Invalidate();

            // Icono tipo
            var ico = new Panel { Size = new Size(28, 28), Location = new Point(20, 10), BackColor = Color.FromArgb(22, col.R, col.G, col.B) };
            ico.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(ico.ClientRectangle, 8))
                    ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(22, col.R, col.G, col.B)), path);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString(esIngreso ? "↑" : "↓", new Font("Segoe UI", 11, FontStyle.Bold), new SolidBrush(col), ico.ClientRectangle, fmt);
            };

            // Badge tipo
            var badgePanel = new Panel { Size = new Size(70, 22), Location = new Point(60, 13), BackColor = Color.FromArgb(20, col.R, col.G, col.B) };
            badgePanel.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(badgePanel.ClientRectangle, 6))
                    ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(20, col.R, col.G, col.B)), path);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString(mov.TipoMovimiento?.ToUpper() ?? "—", new Font("Segoe UI", 7, FontStyle.Bold), new SolidBrush(col), badgePanel.ClientRectangle, fmt);
            };

            // Texto concepto
            fila.Controls.Add(ico);
            fila.Controls.Add(badgePanel);
            fila.Controls.Add(new Label { Text = mov.Concepto ?? "—", ForeColor = TextPrimary, Font = new Font("Segoe UI", 10), Location = new Point(148, 15), Size = new Size(230, 22), AutoEllipsis = true });
            fila.Controls.Add(new Label { Text = mov.Fecha.ToString("dd MMM yyyy  ·  HH:mm"), ForeColor = TextMuted, Font = new Font("Segoe UI", 9), Location = new Point(388, 15), Size = new Size(170, 22) });
            fila.Controls.Add(new Label { Text = (mov.SaldoAnterior > 0 ? mov.SaldoAnterior.ToString("C2", CultureInfo.CreateSpecificCulture("es-ES")) : "—"), ForeColor = TextMuted, Font = new Font("Segoe UI", 9), Location = new Point(560, 15), Size = new Size(130, 22) });
            fila.Controls.Add(new Label { Text = signo + mov.Monto.ToString("C2", CultureInfo.CreateSpecificCulture("es-ES")), ForeColor = col, Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(700, 11), Size = new Size(180, 26), TextAlign = ContentAlignment.MiddleRight });

            return fila;
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90); path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseAllFigures(); return path;
        }

        private class CuentaItem { public CuentaBancaria Cuenta { get; set; } public string Display { get; set; } public override string ToString() => Display; }
    }
}
