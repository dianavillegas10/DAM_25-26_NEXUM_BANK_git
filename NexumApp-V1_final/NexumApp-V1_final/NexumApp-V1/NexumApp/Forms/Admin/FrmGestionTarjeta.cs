// Forms/Admin/FrmGestionTarjetas.cs
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using NexumApp.Services;

namespace NexumApp.Forms.Admin
{
    public class FrmGestionTarjetas : Form
    {
        private DataGridView dgvTarjetas;
        private Button btnActualizar;
        private Button btnCerrar;
        private Label lblEstado;

        public FrmGestionTarjetas()
        {
            this.Text = "💳 Gestión de Tarjetas";
            this.Size = new Size(1100, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var lblTitulo = new Label
            {
                Text = "💳 GESTIÓN DE TARJETAS",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(99, 102, 241),
                Location = new Point(20, 20),
                AutoSize = true
            };

            dgvTarjetas = new DataGridView
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
            btnActualizar.Click += (s, e) => CargarTarjetas();

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
            this.Controls.Add(dgvTarjetas);
            this.Controls.Add(lblEstado);
            this.Controls.Add(lblTitulo);

            this.Load += (s, e) => CargarTarjetas();
        }

        private async void CargarTarjetas()
        {
            try
            {
                lblEstado.Text = "Cargando tarjetas...";
                lblEstado.ForeColor = Color.FromArgb(245, 158, 11);

                var dt = new DataTable();

                using (var conn = DatabaseConfig.GetConnection())
                {
                    await conn.OpenAsync();

                    string query = @"SELECT 
                                        t.Id,
                                        u.Nombre as Usuario,
                                        t.NumeroTarjeta as Número,
                                        t.TipoTarjeta as Tipo,
                                        t.Marca,
                                        DATE_FORMAT(t.FechaCaducidad, '%m/%y') as Caducidad,
                                        CASE WHEN t.Activa = 1 AND t.Bloqueada = 0 THEN 'Activa'
                                             WHEN t.Bloqueada = 1 THEN 'Bloqueada'
                                             ELSE 'Inactiva' END as Estado
                                    FROM tarjetas t
                                    JOIN usuarios u ON t.UsuarioId = u.Id
                                    ORDER BY t.Id DESC";

                    using (var adapter = new MySqlDataAdapter(query, conn))
                    {
                        adapter.Fill(dt);
                    }
                }

                dgvTarjetas.DataSource = dt;
                lblEstado.Text = $"✅ {dt.Rows.Count} tarjetas cargadas";
                lblEstado.ForeColor = Color.FromArgb(16, 185, 129);
            }
            catch (Exception ex)
            {
                lblEstado.Text = $"❌ Error: {ex.Message}";
                lblEstado.ForeColor = Color.FromArgb(239, 68, 68);
                MessageBox.Show($"Error al cargar tarjetas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}