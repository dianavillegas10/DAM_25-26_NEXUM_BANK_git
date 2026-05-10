namespace NexumApp.Forms.Principal
{
    partial class FrmPerfilUsuario
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        // Controles dummy para compilar (no se usan visualmente — la UI se construye en BuildUI)
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel pnlContenido;
        private System.Windows.Forms.GroupBox gbInfoPersonal;
        private System.Windows.Forms.GroupBox gbResumen;
        private System.Windows.Forms.Label lblNombreLabel, lblNombre;
        private System.Windows.Forms.Label lblEmailLabel, lblEmail;
        private System.Windows.Forms.Label lblTelefonoLabel, lblTelefono;
        private System.Windows.Forms.Label lblEdadLabel, lblEdad;
        private System.Windows.Forms.Label lblDNILabel, lblDNI;
        private System.Windows.Forms.Label lblDireccionLabel, lblDireccion;
        private System.Windows.Forms.Label lblFechaNacimientoLabel, lblFechaNacimiento;
        private System.Windows.Forms.Label lblFechaRegistroLabel, lblFechaRegistro;
        private System.Windows.Forms.Label lblTotalCuentasLabel, lblTotalCuentas;
        private System.Windows.Forms.Label lblTotalTarjetasLabel, lblTotalTarjetas;
        private System.Windows.Forms.Label lblSaldoTotalLabel, lblSaldoTotal;
        private System.Windows.Forms.Label lblCuentaPrincipalLabel, lblCuentaPrincipal;
        private System.Windows.Forms.Label lblIBANLabel, lblIBAN;
        private System.Windows.Forms.Button btnEditar;
        private System.Windows.Forms.Button btnCerrar;

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.pnlContenido = new System.Windows.Forms.Panel();
            this.gbInfoPersonal = new System.Windows.Forms.GroupBox();
            this.gbResumen = new System.Windows.Forms.GroupBox();
            this.lblNombreLabel = new System.Windows.Forms.Label(); this.lblNombre = new System.Windows.Forms.Label();
            this.lblEmailLabel = new System.Windows.Forms.Label(); this.lblEmail = new System.Windows.Forms.Label();
            this.lblTelefonoLabel = new System.Windows.Forms.Label(); this.lblTelefono = new System.Windows.Forms.Label();
            this.lblEdadLabel = new System.Windows.Forms.Label(); this.lblEdad = new System.Windows.Forms.Label();
            this.lblDNILabel = new System.Windows.Forms.Label(); this.lblDNI = new System.Windows.Forms.Label();
            this.lblDireccionLabel = new System.Windows.Forms.Label(); this.lblDireccion = new System.Windows.Forms.Label();
            this.lblFechaNacimientoLabel = new System.Windows.Forms.Label(); this.lblFechaNacimiento = new System.Windows.Forms.Label();
            this.lblFechaRegistroLabel = new System.Windows.Forms.Label(); this.lblFechaRegistro = new System.Windows.Forms.Label();
            this.lblTotalCuentasLabel = new System.Windows.Forms.Label(); this.lblTotalCuentas = new System.Windows.Forms.Label();
            this.lblTotalTarjetasLabel = new System.Windows.Forms.Label(); this.lblTotalTarjetas = new System.Windows.Forms.Label();
            this.lblSaldoTotalLabel = new System.Windows.Forms.Label(); this.lblSaldoTotal = new System.Windows.Forms.Label();
            this.lblCuentaPrincipalLabel = new System.Windows.Forms.Label(); this.lblCuentaPrincipal = new System.Windows.Forms.Label();
            this.lblIBANLabel = new System.Windows.Forms.Label(); this.lblIBAN = new System.Windows.Forms.Label();
            this.btnEditar = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.btnEditar.Click += new System.EventHandler(this.BtnEditar_Click);
            this.btnCerrar.Click += new System.EventHandler(this.BtnCerrar_Click);
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FrmPerfilUsuario";
            this.Text = "Mi Perfil — Nexum Bank";
            this.ResumeLayout(false);
        }
    }
}
