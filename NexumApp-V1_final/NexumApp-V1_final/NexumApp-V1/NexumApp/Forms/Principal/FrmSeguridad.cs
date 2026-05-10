using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Windows.Forms;

namespace NexumApp.Forms.Principal
{
    /// <summary>
    /// Formulario de seguridad: cambiar contraseña e información de cuenta.
    /// Implementación integrada desde NexumApp-V2 (Diana). Versión simplificada para V1.
    /// </summary>
    public partial class FrmSeguridad : Form
    {
        private readonly AuthService _authService = new AuthService();
        private Usuario _usuario;

        public FrmSeguridad()
        {
            InitializeComponent();
            _usuario = SesionActual.Instancia.Usuario;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            CargarDatosSeguridad();
        }

        private void CargarDatosSeguridad()
        {
            if (_usuario == null) return;

            lblUltimoAcceso.Text = _usuario.UltimoAcceso?.ToString("dd/MM/yyyy HH:mm") ?? "Nunca";
            lblMiembroDesde.Text = _usuario.FechaRegistro.ToString("dd/MM/yyyy");

            var config = SesionActual.Instancia.Configuracion;
            if (config != null)
            {
                chkDosFactores.Checked = config.DosFactores;
                chkSesionSegura.Checked = config.SesionSegura;
                numTiempoSesion.Value = config.TiempoSesionMinutos;
            }

            MostrarSesionActual();
        }

        private void MostrarSesionActual()
        {
            pnlSesiones.Controls.Clear();
            var lbl = new System.Windows.Forms.Label
            {
                Text = "💻 Este dispositivo - Sesión actual\n\n" +
                       "La gestión de múltiples sesiones requiere la tabla 'sesiones' en la base de datos.",
                Location = new System.Drawing.Point(15, 15),
                Size = new System.Drawing.Size(450, 60),
                Font = new System.Drawing.Font("Segoe UI", 10F),
                ForeColor = System.Drawing.Color.FromArgb(80, 80, 80),
                AutoSize = false
            };
            pnlSesiones.Controls.Add(lbl);
        }

        private void BtnGuardarConfiguracion_Click(object sender, EventArgs e)
        {
            var config = SesionActual.Instancia.Configuracion;
            if (config == null)
                config = new ConfiguracionUsuario { UsuarioId = _usuario.Id };

            config.DosFactores = chkDosFactores.Checked;
            config.SesionSegura = chkSesionSegura.Checked;
            config.TiempoSesionMinutos = (int)numTiempoSesion.Value;

            SesionActual.Instancia.Configuracion = config;

            MessageBox.Show("Configuración de seguridad guardada correctamente.", "Éxito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCambiarPassword_Click(object sender, EventArgs e)
        {
            if (!ValidarPassword()) return;

            string error;
            if (_authService.CambiarContraseña(_usuario.Id, txtPasswordActual.Text, txtNuevaPassword.Text, out error))
            {
                MessageBox.Show("Contraseña cambiada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtPasswordActual.Clear();
                txtNuevaPassword.Clear();
                txtConfirmarPassword.Clear();
            }
            else
            {
                MessageBox.Show(error ?? "Error al cambiar la contraseña.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidarPassword()
        {
            if (string.IsNullOrWhiteSpace(txtPasswordActual.Text))
            {
                MessageBox.Show("Debes ingresar tu contraseña actual.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPasswordActual.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtNuevaPassword.Text))
            {
                MessageBox.Show("Debes ingresar una nueva contraseña.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNuevaPassword.Focus();
                return false;
            }
            if (txtNuevaPassword.Text.Length < 6)
            {
                MessageBox.Show("La contraseña debe tener al menos 6 caracteres.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNuevaPassword.Focus();
                return false;
            }
            if (txtNuevaPassword.Text != txtConfirmarPassword.Text)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmarPassword.Focus();
                return false;
            }
            return true;
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
