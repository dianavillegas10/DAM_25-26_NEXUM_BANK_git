using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NexumApp.Views
{
    public class VistaHistorialMovimientos : UserControl
    {
        private readonly MovimientoService _movimientoService = new MovimientoService();
        private readonly CuentaService _cuentaService = new CuentaService();
        private ComboBox cmbCuentas;
        private DataGridView dgvMovimientos;
        private Label lblSaldo, lblIngresos, lblGastos;
        private List<Movimiento> movimientos = new List<Movimiento>();

        public VistaHistorialMovimientos()
        {
            BackColor = Color.FromArgb(18, 24, 48);
            Dock = DockStyle.Fill;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ConfigurarUI();
        }

        private void ConfigurarUI()
        {
            Controls.Clear();
            lblSaldo = new Label { ForeColor = Color.LimeGreen, Font = new Font("Segoe UI", 18, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true, Text = "Saldo actual: 0 €" };
            lblIngresos = new Label { ForeColor = Color.LimeGreen, Font = new Font("Segoe UI", 11), Location = new Point(350, 22), AutoSize = true, Text = "Ingresos: 0 €" };
            lblGastos = new Label { ForeColor = Color.IndianRed, Font = new Font("Segoe UI", 11), Location = new Point(520, 22), AutoSize = true, Text = "Gastos: 0 €" };
            dgvMovimientos = new DataGridView { Location = new Point(20, 60), Size = new Size(1040, 480), BackgroundColor = Color.FromArgb(18, 24, 48), BorderStyle = BorderStyle.None, RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvMovimientos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 36, 64);
            dgvMovimientos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvMovimientos.EnableHeadersVisualStyles = false;
            dgvMovimientos.DefaultCellStyle.BackColor = Color.FromArgb(30, 36, 64);
            dgvMovimientos.DefaultCellStyle.ForeColor = Color.White;
            dgvMovimientos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 90, 150);
            dgvMovimientos.Columns.Add("Fecha", "Fecha");
            dgvMovimientos.Columns.Add("Concepto", "Concepto");
            dgvMovimientos.Columns.Add("Tipo", "Tipo");
            dgvMovimientos.Columns.Add("Importe", "Importe");
            dgvMovimientos.Columns["Importe"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            cmbCuentas = new ComboBox { Location = new Point(150, 557), Width = 350, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCuentas.SelectedIndexChanged += (s, ev) => { if (cmbCuentas.SelectedValue != null && int.TryParse(cmbCuentas.SelectedValue.ToString(), out int id)) CargarMovimientos(id); };
            Controls.Add(lblSaldo);
            Controls.Add(lblIngresos);
            Controls.Add(lblGastos);
            Controls.Add(dgvMovimientos);
            Controls.Add(new Label { Text = "Cambiar cuenta:", ForeColor = Color.White, Location = new Point(20, 560), AutoSize = true });
            Controls.Add(cmbCuentas);
            Load += (s, ev) => CargarCuentas();
        }

        private void CargarCuentas()
        {
            if (!SesionActual.Instancia?.EstaLogeado ?? true) return;
            var cuentas = _cuentaService.ObtenerCuentasPorUsuario(SesionActual.Instancia.Usuario.Id);
            cmbCuentas.DataSource = cuentas ?? new List<CuentaBancaria>();
            cmbCuentas.DisplayMember = "NumeroCuenta";
            cmbCuentas.ValueMember = "Id";
            if (cuentas != null && cuentas.Count > 0)
                CargarMovimientos(cuentas[0].Id);
            else if (dgvMovimientos != null)
                dgvMovimientos.Rows.Clear();
        }

        private void CargarMovimientos(int cuentaId)
        {
            movimientos = _movimientoService.ObtenerMovimientosPorCuenta(cuentaId);
            dgvMovimientos.Rows.Clear();
            if (movimientos.Count == 0) { dgvMovimientos.Rows.Add("", "No hay movimientos", "", ""); ActualizarResumen(0, 0); return; }
            foreach (var m in movimientos) dgvMovimientos.Rows.Add(m.Fecha.ToString("dd/MM/yyyy"), m.Concepto, m.TipoMovimiento, EsIngreso(m.TipoMovimiento) ? $"+{m.Monto:N2} €" : $"-{m.Monto:N2} €");
            foreach (DataGridViewRow r in dgvMovimientos.Rows) if (r.Cells["Importe"].Value?.ToString().StartsWith("+") == true) r.Cells["Importe"].Style.ForeColor = Color.LimeGreen; else r.Cells["Importe"].Style.ForeColor = Color.IndianRed;
            decimal ing = movimientos.Where(x => EsIngreso(x.TipoMovimiento)).Sum(x => x.Monto);
            decimal gast = movimientos.Where(x => !EsIngreso(x.TipoMovimiento)).Sum(x => x.Monto);
            ActualizarResumen(ing, gast);
        }

        private static bool EsIngreso(string tipo)
        {
            if (string.IsNullOrEmpty(tipo)) return false;
            return tipo.IndexOf("Ingreso",  StringComparison.OrdinalIgnoreCase) >= 0
                || tipo.IndexOf("Recibida", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ActualizarResumen(decimal ingresos, decimal gastos)
        {
            decimal saldo = ingresos - gastos;
            lblSaldo.Text = $"Saldo actual: {saldo:N2} €";
            lblIngresos.Text = $"Ingresos: {ingresos:N2} €";
            lblGastos.Text = $"Gastos: {gastos:N2} €";
            lblSaldo.ForeColor = saldo >= 0 ? Color.LimeGreen : Color.IndianRed;
        }
    }
}
