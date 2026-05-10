namespace NexumApp.Forms.Principal
{
    partial class FrmSeguridad
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel pnlContenido;
        private System.Windows.Forms.GroupBox gbCambiarPassword;
        private System.Windows.Forms.Label lblPasswordActual;
        private System.Windows.Forms.TextBox txtPasswordActual;
        private System.Windows.Forms.Label lblNuevaPassword;
        private System.Windows.Forms.TextBox txtNuevaPassword;
        private System.Windows.Forms.Label lblConfirmarPassword;
        private System.Windows.Forms.TextBox txtConfirmarPassword;
        private System.Windows.Forms.Button btnCambiarPassword;
        private System.Windows.Forms.GroupBox gbAutenticacion;
        private System.Windows.Forms.CheckBox chkDosFactores;
        private System.Windows.Forms.CheckBox chkSesionSegura;
        private System.Windows.Forms.Label lblTiempoSesion;
        private System.Windows.Forms.NumericUpDown numTiempoSesion;
        private System.Windows.Forms.Button btnGuardarConfiguracion;
        private System.Windows.Forms.GroupBox gbSesiones;
        private System.Windows.Forms.Panel pnlSesiones;
        private System.Windows.Forms.GroupBox gbInfoCuenta;
        private System.Windows.Forms.Label lblUltimoAccesoLabel;
        private System.Windows.Forms.Label lblUltimoAcceso;
        private System.Windows.Forms.Label lblMiembroDesdeLabel;
        private System.Windows.Forms.Label lblMiembroDesde;
        private System.Windows.Forms.Button btnCancelar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.pnlContenido = new System.Windows.Forms.Panel();
            this.gbCambiarPassword = new System.Windows.Forms.GroupBox();
            this.gbAutenticacion = new System.Windows.Forms.GroupBox();
            this.gbSesiones = new System.Windows.Forms.GroupBox();
            this.gbInfoCuenta = new System.Windows.Forms.GroupBox();

            this.chkDosFactores = new System.Windows.Forms.CheckBox();
            this.chkSesionSegura = new System.Windows.Forms.CheckBox();
            this.lblTiempoSesion = new System.Windows.Forms.Label();
            this.numTiempoSesion = new System.Windows.Forms.NumericUpDown();
            this.btnGuardarConfiguracion = new System.Windows.Forms.Button();
            this.pnlSesiones = new System.Windows.Forms.Panel();

            this.lblPasswordActual = new System.Windows.Forms.Label();
            this.txtPasswordActual = new System.Windows.Forms.TextBox();
            this.lblNuevaPassword = new System.Windows.Forms.Label();
            this.txtNuevaPassword = new System.Windows.Forms.TextBox();
            this.lblConfirmarPassword = new System.Windows.Forms.Label();
            this.txtConfirmarPassword = new System.Windows.Forms.TextBox();
            this.btnCambiarPassword = new System.Windows.Forms.Button();

            this.lblUltimoAccesoLabel = new System.Windows.Forms.Label();
            this.lblUltimoAcceso = new System.Windows.Forms.Label();
            this.lblMiembroDesdeLabel = new System.Windows.Forms.Label();
            this.lblMiembroDesde = new System.Windows.Forms.Label();

            this.btnCancelar = new System.Windows.Forms.Button();

            this.ClientSize = new System.Drawing.Size(550, 620);
            this.Text = "Seguridad - Nexum Bank";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = System.Drawing.Color.FromArgb(240, 242, 245);

            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(18, 22, 30);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 70;
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.lblTitulo.Location = new System.Drawing.Point(30, 20);
            this.lblTitulo.Text = "SEGURIDAD";
            this.pnlHeader.Controls.Add(this.lblTitulo);

            this.pnlContenido.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContenido.Padding = new System.Windows.Forms.Padding(30);
            this.pnlContenido.BackColor = System.Drawing.Color.White;

            int yPos = 30;
            int gw = 470;

            this.gbCambiarPassword.Text = "Cambiar contraseña";
            this.gbCambiarPassword.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbCambiarPassword.Location = new System.Drawing.Point(30, yPos);
            this.gbCambiarPassword.Size = new System.Drawing.Size(gw, 180);

            int py = 30;
            this.lblPasswordActual.Text = "Contraseña actual:";
            this.lblPasswordActual.Location = new System.Drawing.Point(20, py);
            this.lblPasswordActual.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtPasswordActual.Location = new System.Drawing.Point(180, py);
            this.txtPasswordActual.Size = new System.Drawing.Size(260, 25);
            this.txtPasswordActual.UseSystemPasswordChar = true;
            py += 35;

            this.lblNuevaPassword.Text = "Nueva contraseña:";
            this.lblNuevaPassword.Location = new System.Drawing.Point(20, py);
            this.lblNuevaPassword.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtNuevaPassword.Location = new System.Drawing.Point(180, py);
            this.txtNuevaPassword.Size = new System.Drawing.Size(260, 25);
            this.txtNuevaPassword.UseSystemPasswordChar = true;
            py += 35;

            this.lblConfirmarPassword.Text = "Confirmar contraseña:";
            this.lblConfirmarPassword.Location = new System.Drawing.Point(20, py);
            this.lblConfirmarPassword.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtConfirmarPassword.Location = new System.Drawing.Point(180, py);
            this.txtConfirmarPassword.Size = new System.Drawing.Size(260, 25);
            this.txtConfirmarPassword.UseSystemPasswordChar = true;
            py += 40;

            this.btnCambiarPassword.Text = "Cambiar contraseña";
            this.btnCambiarPassword.Location = new System.Drawing.Point(180, py);
            this.btnCambiarPassword.Size = new System.Drawing.Size(150, 35);
            this.btnCambiarPassword.BackColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.btnCambiarPassword.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCambiarPassword.FlatAppearance.BorderSize = 0;
            this.btnCambiarPassword.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnCambiarPassword.Click += BtnCambiarPassword_Click;

            this.gbCambiarPassword.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblPasswordActual, txtPasswordActual,
                lblNuevaPassword, txtNuevaPassword,
                lblConfirmarPassword, txtConfirmarPassword,
                btnCambiarPassword
            });

            yPos += 200;

            // Autenticación y sesión
            this.gbAutenticacion.Text = "Autenticación y sesión";
            this.gbAutenticacion.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbAutenticacion.Location = new System.Drawing.Point(30, yPos);
            this.gbAutenticacion.Size = new System.Drawing.Size(gw, 130);

            int ay = 25;
            this.chkDosFactores.Text = "Autenticación de dos factores (2FA)";
            this.chkDosFactores.Location = new System.Drawing.Point(20, ay);
            this.chkDosFactores.Font = new System.Drawing.Font("Segoe UI", 10F);
            ay += 30;
            this.chkSesionSegura.Text = "Sesión segura (cerrar al salir)";
            this.chkSesionSegura.Location = new System.Drawing.Point(20, ay);
            this.chkSesionSegura.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkSesionSegura.Checked = true;
            ay += 35;
            this.lblTiempoSesion.Text = "Tiempo de sesión (min):";
            this.lblTiempoSesion.Location = new System.Drawing.Point(20, ay);
            this.lblTiempoSesion.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.numTiempoSesion.Location = new System.Drawing.Point(200, ay - 2);
            this.numTiempoSesion.Size = new System.Drawing.Size(80, 25);
            this.numTiempoSesion.Minimum = 5;
            this.numTiempoSesion.Maximum = 120;
            this.numTiempoSesion.Value = 30;
            ay += 35;
            this.btnGuardarConfiguracion.Text = "Guardar configuración";
            this.btnGuardarConfiguracion.Location = new System.Drawing.Point(20, ay);
            this.btnGuardarConfiguracion.Size = new System.Drawing.Size(160, 30);
            this.btnGuardarConfiguracion.BackColor = System.Drawing.Color.FromArgb(100, 100, 180);
            this.btnGuardarConfiguracion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardarConfiguracion.FlatAppearance.BorderSize = 0;
            this.btnGuardarConfiguracion.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnGuardarConfiguracion.ForeColor = System.Drawing.Color.White;
            this.btnGuardarConfiguracion.Click += BtnGuardarConfiguracion_Click;

            this.gbAutenticacion.Controls.AddRange(new System.Windows.Forms.Control[] {
                chkDosFactores, chkSesionSegura, lblTiempoSesion, numTiempoSesion, btnGuardarConfiguracion
            });
            yPos += 150;

            // Sesiones activas
            this.gbSesiones.Text = "Sesiones activas";
            this.gbSesiones.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbSesiones.Location = new System.Drawing.Point(30, yPos);
            this.gbSesiones.Size = new System.Drawing.Size(gw, 120);

            this.pnlSesiones.Location = new System.Drawing.Point(10, 30);
            this.pnlSesiones.Size = new System.Drawing.Size(450, 80);
            this.pnlSesiones.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
            this.pnlSesiones.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlSesiones.AutoScroll = true;

            this.gbSesiones.Controls.Add(pnlSesiones);
            yPos += 140;

            this.gbInfoCuenta.Text = "Información de la cuenta";
            this.gbInfoCuenta.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbInfoCuenta.Location = new System.Drawing.Point(30, yPos);
            this.gbInfoCuenta.Size = new System.Drawing.Size(gw, 90);

            int iy = 30;
            this.lblUltimoAccesoLabel.Text = "Último acceso:";
            this.lblUltimoAccesoLabel.Location = new System.Drawing.Point(20, iy);
            this.lblUltimoAccesoLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblUltimoAcceso.Text = "-";
            this.lblUltimoAcceso.Location = new System.Drawing.Point(150, iy);
            this.lblUltimoAcceso.Font = new System.Drawing.Font("Segoe UI", 10F);
            iy += 30;
            this.lblMiembroDesdeLabel.Text = "Miembro desde:";
            this.lblMiembroDesdeLabel.Location = new System.Drawing.Point(20, iy);
            this.lblMiembroDesdeLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblMiembroDesde.Text = "-";
            this.lblMiembroDesde.Location = new System.Drawing.Point(150, iy);
            this.lblMiembroDesde.Font = new System.Drawing.Font("Segoe UI", 10F);

            this.gbInfoCuenta.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblUltimoAccesoLabel, lblUltimoAcceso,
                lblMiembroDesdeLabel, lblMiembroDesde
            });

            yPos += 120;

            this.btnCancelar.Text = "Cerrar";
            this.btnCancelar.Location = new System.Drawing.Point(220, yPos);
            this.btnCancelar.Size = new System.Drawing.Size(100, 40);
            this.btnCancelar.BackColor = System.Drawing.Color.FromArgb(60, 60, 70);
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.FlatAppearance.BorderSize = 0;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.Click += BtnCancelar_Click;

            this.pnlContenido.AutoScroll = true;
            this.pnlContenido.Controls.AddRange(new System.Windows.Forms.Control[] {
                gbCambiarPassword, gbAutenticacion, gbSesiones, gbInfoCuenta, btnCancelar
            });

            this.Controls.Add(this.pnlContenido);
            this.Controls.Add(this.pnlHeader);
        }
    }
}
