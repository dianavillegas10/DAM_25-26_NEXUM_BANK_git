// Forms/Admin/FrmCancelarCuentas.cs
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using NexumApp.Services;

namespace NexumApp.Forms.Admin
{
    public class FrmCancelarCuentas : Form
    {
        private DataGridView dgvUsuarios;
        private Button btnCancelar;
        private Button btnActualizar;
        private Button btnCerrar;
        private Label lblEstado;

        public FrmCancelarCuentas()
        {
            this.Text = "🚫 Cancelar Cuentas";
            this.Size = new Size(1100, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var lblTitulo = new Label
            {
                Text = "🚫 CANCELAR CUENTAS",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(239, 68, 68),
                Location = new Point(20, 20),
                AutoSize = true
            };

            var lblAdvertencia = new Label
            {
                Text = "⚠️ Esta acción desactiva la cuenta del usuario permanentemente.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(239, 68, 68),
                Location = new Point(20, 50),
                AutoSize = true
            };

            dgvUsuarios = new DataGridView
            {
                Location = new Point(20, 80),
                Size = new Size(1040, 400),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };

            btnCancelar = new Button
            {
                Text = "🚫 Cancelar Cuenta Seleccionada",
                Location = new Point(20, 500),
                Size = new Size(200, 35),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnCancelar.Click += BtnCancelar_Click;

            btnActualizar = new Button
            {
                Text = "🔄 Actualizar",
                Location = new Point(230, 500),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnActualizar.Click += (s, e) => CargarUsuarios();

            btnCerrar = new Button
            {
                Text = "Cerrar",
                Location = new Point(960, 500),
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
                Location = new Point(20, 540),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            dgvUsuarios.SelectionChanged += (s, e) => btnCancelar.Enabled = dgvUsuarios.SelectedRows.Count > 0;

            this.Controls.Add(btnCerrar);
            this.Controls.Add(btnActualizar);
            this.Controls.Add(btnCancelar);
            this.Controls.Add(dgvUsuarios);
            this.Controls.Add(lblAdvertencia);
            this.Controls.Add(lblEstado);
            this.Controls.Add(lblTitulo);

            this.Load += (s, e) => CargarUsuarios();
        }

        private async void CargarUsuarios()
        {
            try
            {
                lblEstado.Text = "Cargando usuarios...";
                lblEstado.ForeColor = Color.FromArgb(245, 158, 11);

                var dt = new DataTable();

                using (var conn = DatabaseConfig.GetConnection())
                {
                    await conn.OpenAsync();

                    string query = @"SELECT 
                                        Id, 
                                        Nombre, 
                                        Apellidos, 
                                        Email, 
                                        CASE WHEN EsAdmin = 1 THEN 'Administrador' ELSE 'Usuario' END as Rol,
                                        CASE WHEN Activo = 1 THEN 'Activo' ELSE 'Inactivo' END as Estado,
                                        DATE_FORMAT(FechaRegistro, '%d/%m/%Y') as FechaRegistro
                                    FROM usuarios 
                                    ORDER BY Id DESC";

                    using (var adapter = new MySqlDataAdapter(query, conn))
                    {
                        adapter.Fill(dt);
                    }
                }

                dgvUsuarios.DataSource = dt;
                lblEstado.Text = $"✅ {dt.Rows.Count} usuarios cargados";
                lblEstado.ForeColor = Color.FromArgb(16, 185, 129);
            }
            catch (Exception ex)
            {
                lblEstado.Text = $"❌ Error: {ex.Message}";
                lblEstado.ForeColor = Color.FromArgb(239, 68, 68);
                MessageBox.Show($"Error al cargar usuarios: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnCancelar_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count == 0) return;

            int id = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells["Id"].Value);
            string nombre = dgvUsuarios.SelectedRows[0].Cells["Nombre"].Value.ToString();

            var resultado = MessageBox.Show(
                $"⚠️ ¿Estás seguro de que deseas DESACTIVAR la cuenta de {nombre}?\n\nEsta acción se puede revertir.",
                "Confirmar Cancelación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (resultado == DialogResult.Yes)
            {
                using (var conn = DatabaseConfig.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = "UPDATE usuarios SET Activo = 0 WHERE Id = @id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                MessageBox.Show($"La cuenta de {nombre} ha sido DESACTIVADA.", "Cuenta Desactivada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarUsuarios();
            }
        }
    }
}