using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NexumApp.Forms.Admin
{
    public partial class FrmEditarUsuario : Form
    {
        private UsuarioService _usuarioService = new UsuarioService();
        private List<Usuario> _usuariosDb;
        private Usuario _usuarioCargado;

        public FrmEditarUsuario()
        {
            InitializeComponent();
            this.Load += FrmEditarUsuario_Load;
        }

        private async void FrmEditarUsuario_Load(object sender, EventArgs e)
        {
            await CargarUsuariosALista();
        }

        private async Task CargarUsuariosALista()
        {
            _usuariosDb = await _usuarioService.ObtenerTodosAsync();

            cmbUsuario.DataSource = null;
            cmbUsuario.DataSource = _usuariosDb;
            cmbUsuario.DisplayMember = "Nombre"; // Muestra el nombre en el combo
            cmbUsuario.ValueMember = "Id";       // Guarda el ID internamente
            cmbUsuario.SelectedIndex = -1;
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            if (cmbUsuario.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione un usuario de la lista.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // El objeto seleccionado es un tipo 'Usuario' de tu lista
            _usuarioCargado = (Usuario)cmbUsuario.SelectedItem;

            // Rellenamos los campos con datos reales de la BBDD
            txtNombre.Text = _usuarioCargado.Nombre;
            txtEmail.Text = _usuarioCargado.Email;

            // Ajustamos el combo de Rol (0: Usuario, 1: Admin)
            cmbRol.SelectedIndex = _usuarioCargado.EsAdmin ? 1 : 0;

            btnGuardar.Enabled = true;
        }

        private async void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (_usuarioCargado == null) return;

            string nombre = txtNombre.Text.Trim();
            string email = txtEmail.Text.Trim();
            bool esAdmin = (cmbRol.SelectedIndex == 1);

            bool exito = await _usuarioService.ActualizarUsuarioAsync(_usuarioCargado.Id, nombre, email, esAdmin);

            if (exito)
            {
                MessageBox.Show("Usuario actualizado correctamente en la base de datos.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("No se pudo actualizar el usuario. Verifique la conexión.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}