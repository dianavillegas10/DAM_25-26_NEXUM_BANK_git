// Forms/Admin/FrmGestionTransferencias.cs
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using NexumApp.Services;

namespace NexumApp.Forms.Admin
{
    public class FrmGestionTransferencias : Form
    {
        private DataGridView dgvTransferencias;
        private Button btnActualizar;
        private Button btnCerrar;
        private Label lblEstado;

        public FrmGestionTransferencias()
        {
            this.Text = "💰 Gestión de Transferencias";
            this.Size = new Size(1100, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var lblTitulo = new Label
            {
                Text = "💰 GESTIÓN DE TRANSFERENCIAS",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(99, 102, 241),
                Location = new Point(20, 20),
                AutoSize = true
            };

            dgvTransferencias = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(1040, 440),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };

            btnActualizar = new Button
            {
                Text = "🔄 Actualizar",
                Location = new Point(20, 525),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnActualizar.Click += (s, e) => CargarTransferencias();

            btnCerrar = new Button
            {
                Text = "Cerrar",
                Location = new Point(960, 525),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCerrar.Click += (s, e) => this.Close();

            lblEstado = new Label
            {
                Text = "",
                Location = new Point(20, 560),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            this.Controls.Add(btnCerrar);
            this.Controls.Add(btnActualizar);
            this.Controls.Add(dgvTransferencias);
            this.Controls.Add(lblEstado);
            this.Controls.Add(lblTitulo);

            this.Load += (s, e) => CargarTransferencias();
        }

        private async void CargarTransferencias()
        {
            try
            {
                lblEstado.Text = "Cargando transferencias...";
                lblEstado.ForeColor = Color.FromArgb(245, 158, 11);

                var dt = new DataTable();

                using (var conn = DatabaseConfig.GetConnection())
                {
                    await conn.OpenAsync();

                    string query = @"SELECT 
                                        t.Id,
                                        DATE_FORMAT(t.Fecha, '%d/%m/%Y %H:%i') as Fecha,
                                        u.Nombre as Remitente,
                                        t.NombreBeneficiario as Beneficiario,
                                        CONCAT(t.Monto, ' €') as Monto,
                                        t.Estado,
                                        t.Concepto
                                    FROM transferencias t
                                    JOIN cuentas_bancarias cb ON t.CuentaOrigenId = cb.Id
                                    JOIN usuarios u ON cb.UsuarioId = u.Id
                                    ORDER BY t.Id DESC";

                    using (var adapter = new MySqlDataAdapter(query, conn))
                    {
                        adapter.Fill(dt);
                    }
                }

                dgvTransferencias.DataSource = dt;
                lblEstado.Text = $"✅ {dt.Rows.Count} transferencias cargadas";
                lblEstado.ForeColor = Color.FromArgb(16, 185, 129);
            }
            catch (Exception ex)
            {
                lblEstado.Text = $"❌ Error: {ex.Message}";
                lblEstado.ForeColor = Color.FromArgb(239, 68, 68);
                MessageBox.Show($"Error al cargar transferencias: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}