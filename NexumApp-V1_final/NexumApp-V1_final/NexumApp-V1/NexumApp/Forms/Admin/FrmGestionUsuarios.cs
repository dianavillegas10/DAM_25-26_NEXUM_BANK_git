// Forms/Admin/FrmGestionUsuarios.cs
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using NexumApp.Services;

namespace NexumApp.Forms.Admin
{
    public partial class FrmGestionUsuarios : Form
    {
        private DataTable dtUsuarios;

        public FrmGestionUsuarios()
        {
            InitializeComponent();
        }

        private void FrmGestionUsuarios_Load(object sender, EventArgs e)
        {
            cmbFiltroRol.Items.Clear();
            cmbFiltroRol.Items.Add("Todos");
            cmbFiltroRol.Items.Add("Administrador");
            cmbFiltroRol.Items.Add("Usuario");
            cmbFiltroRol.SelectedIndex = 0;

            cmbFiltroEstado.Items.Clear();
            cmbFiltroEstado.Items.Add("Todos");
            cmbFiltroEstado.Items.Add("Activo");
            cmbFiltroEstado.Items.Add("Inactivo");
            cmbFiltroEstado.SelectedIndex = 0;

            CargarUsuarios();
        }

        private async void CargarUsuarios()
        {
            try
            {
                dgvUsuarios.Rows.Clear();
                lblResultados.Text = "Cargando usuarios...";
                lblResultados.ForeColor = Color.FromArgb(245, 158, 11);

                dtUsuarios = new DataTable();

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
                        adapter.Fill(dtUsuarios);
                    }
                }

                foreach (DataRow row in dtUsuarios.Rows)
                {
                    dgvUsuarios.Rows.Add(
                        row["Id"],
                        row["Nombre"],
                        row["Apellidos"],
                        row["Email"],
                        row["Rol"],
                        row["Estado"],
                        row["FechaRegistro"]
                    );
                }

                FiltrarUsuarios();
                lblResultados.Text = $"{dgvUsuarios.Rows.Count} resultados";
                lblResultados.ForeColor = Color.FromArgb(16, 185, 129);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar usuarios: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblResultados.Text = $"Error: {ex.Message}";
                lblResultados.ForeColor = Color.FromArgb(239, 68, 68);
            }
        }

        private void FiltrarUsuarios()
        {
            if (dtUsuarios == null) return;

            string busqueda = txtBuscar.Text.ToLower();
            string rolFiltro = cmbFiltroRol.SelectedItem?.ToString() ?? "Todos";
            string estadoFiltro = cmbFiltroEstado.SelectedItem?.ToString() ?? "Todos";

            int filasVisibles = 0;
            var filasFiltradas = dtUsuarios.AsEnumerable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                filasFiltradas = filasFiltradas.Where(row =>
                    row["Nombre"].ToString().ToLower().Contains(busqueda) ||
                    row["Apellidos"].ToString().ToLower().Contains(busqueda) ||
                    row["Email"].ToString().ToLower().Contains(busqueda));
            }

            if (rolFiltro != "Todos")
            {
                filasFiltradas = filasFiltradas.Where(row => row["Rol"].ToString() == rolFiltro);
            }

            if (estadoFiltro != "Todos")
            {
                filasFiltradas = filasFiltradas.Where(row => row["Estado"].ToString() == estadoFiltro);
            }

            dgvUsuarios.Rows.Clear();

            foreach (var row in filasFiltradas)
            {
                dgvUsuarios.Rows.Add(
                    row["Id"],
                    row["Nombre"],
                    row["Apellidos"],
                    row["Email"],
                    row["Rol"],
                    row["Estado"],
                    row["FechaRegistro"]
                );
                filasVisibles++;
            }

            lblResultados.Text = $"{filasVisibles} resultados";
        }

        private void LimpiarFiltros()
        {
            txtBuscar.Text = "";
            cmbFiltroRol.SelectedIndex = 0;
            cmbFiltroEstado.SelectedIndex = 0;
            FiltrarUsuarios();
        }

        private void HabilitarBotonesSegunSeleccion()
        {
            bool haySeleccion = dgvUsuarios.SelectedRows.Count > 0;
            btnEditar.Enabled = haySeleccion;
            btnBanear.Enabled = haySeleccion;
            btnDesbanear.Enabled = haySeleccion;
        }

        private void DgvUsuarios_SelectionChanged(object sender, EventArgs e)
        {
            HabilitarBotonesSegunSeleccion();
        }

        private void DgvUsuarios_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) EditarUsuario();
        }

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmNuevoUsuario())
            {
                if (frm.ShowDialog() == DialogResult.OK) CargarUsuarios();
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            EditarUsuario();
        }

        private async void EditarUsuario()
        {
            if (dgvUsuarios.SelectedRows.Count == 0) return;

            int id = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells["colId"].Value);
            string nombre = dgvUsuarios.SelectedRows[0].Cells["colNombre"].Value.ToString();
            string apellidos = dgvUsuarios.SelectedRows[0].Cells["colApellidos"].Value.ToString();
            string email = dgvUsuarios.SelectedRows[0].Cells["colEmail"].Value.ToString();
            string rol = dgvUsuarios.SelectedRows[0].Cells["colRol"].Value.ToString();

            using (var frm = new FrmEditarUsuario())
            {
                if (frm.ShowDialog() == DialogResult.OK) CargarUsuarios();
            }
        }

        private async void BtnBanear_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count == 0) return;

            int id = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells["colId"].Value);
            string nombre = dgvUsuarios.SelectedRows[0].Cells["colNombre"].Value.ToString();
            string estado = dgvUsuarios.SelectedRows[0].Cells["colEstado"].Value.ToString();

            if (estado == "Inactivo")
            {
                MessageBox.Show($"El usuario {nombre} ya está inactivo.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var resultado = MessageBox.Show($"¿Estás seguro de que deseas DESACTIVAR a {nombre}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

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
                MessageBox.Show($"Usuario {nombre} ha sido DESACTIVADO.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarUsuarios();
            }
        }

        private async void BtnDesbanear_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count == 0) return;

            int id = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells["colId"].Value);
            string nombre = dgvUsuarios.SelectedRows[0].Cells["colNombre"].Value.ToString();
            string estado = dgvUsuarios.SelectedRows[0].Cells["colEstado"].Value.ToString();

            if (estado == "Activo")
            {
                MessageBox.Show($"El usuario {nombre} ya está activo.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var resultado = MessageBox.Show($"¿Estás seguro de que deseas ACTIVAR a {nombre}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                using (var conn = DatabaseConfig.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = "UPDATE usuarios SET Activo = 1 WHERE Id = @id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                MessageBox.Show($"Usuario {nombre} ha sido ACTIVADO.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarUsuarios();
            }
        }

        private void BtnActualizar_Click(object sender, EventArgs e) => CargarUsuarios();
        private void BtnBuscar_Click(object sender, EventArgs e) => FiltrarUsuarios();
        private void BtnLimpiar_Click(object sender, EventArgs e) => LimpiarFiltros();
        private void BtnCerrar_Click(object sender, EventArgs e) => this.Close();
        private void TxtBuscar_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) FiltrarUsuarios(); }
        private void CmbFiltro_SelectedIndexChanged(object sender, EventArgs e) => FiltrarUsuarios();
    }
}