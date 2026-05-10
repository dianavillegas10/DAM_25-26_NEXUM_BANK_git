namespace NexumApp.Forms.Principal
{
    partial class FrmEditarPerfil
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        // Controles dummy para compilar
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitulo, lblNombre, lblApellidos, lblEmail, lblTelefono, lblDNI, lblDireccion, lblCiudad, lblCodigoPostal, lblFechaNacimiento;
        private System.Windows.Forms.TextBox txtNombre, txtApellidos, txtEmail, txtTelefono, txtDNI, txtDireccion, txtCiudad, txtCodigoPostal;
        private System.Windows.Forms.Panel pnlContenido;
        private System.Windows.Forms.DateTimePicker dtpFechaNacimiento;
        private System.Windows.Forms.Button btnGuardar, btnCancelar;

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.pnlContenido = new System.Windows.Forms.Panel();
            this.lblNombre = new System.Windows.Forms.Label(); this.txtNombre = new System.Windows.Forms.TextBox();
            this.lblApellidos = new System.Windows.Forms.Label(); this.txtApellidos = new System.Windows.Forms.TextBox();
            this.lblEmail = new System.Windows.Forms.Label(); this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblTelefono = new System.Windows.Forms.Label(); this.txtTelefono = new System.Windows.Forms.TextBox();
            this.lblDNI = new System.Windows.Forms.Label(); this.txtDNI = new System.Windows.Forms.TextBox();
            this.lblDireccion = new System.Windows.Forms.Label(); this.txtDireccion = new System.Windows.Forms.TextBox();
            this.lblCiudad = new System.Windows.Forms.Label(); this.txtCiudad = new System.Windows.Forms.TextBox();
            this.lblCodigoPostal = new System.Windows.Forms.Label(); this.txtCodigoPostal = new System.Windows.Forms.TextBox();
            this.lblFechaNacimiento = new System.Windows.Forms.Label();
            this.dtpFechaNacimiento = new System.Windows.Forms.DateTimePicker();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnGuardar.Click += new System.EventHandler(this.BtnGuardar_Click);
            this.btnCancelar.Click += new System.EventHandler(this.BtnCancelar_Click);
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FrmEditarPerfil";
            this.Text = "Editar Perfil — Nexum Bank";
            this.ResumeLayout(false);
        }
    }
}
