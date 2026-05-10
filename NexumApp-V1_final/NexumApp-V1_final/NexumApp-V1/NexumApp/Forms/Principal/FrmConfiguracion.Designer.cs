namespace NexumApp.Forms.Principal
{
    partial class FrmConfiguracion
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        // Controles requeridos por el .cs para compilar (no se usan visualmente)
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel pnlContenido;
        private System.Windows.Forms.GroupBox gbNotificaciones;
        private System.Windows.Forms.CheckBox chkNotificacionesEmail;
        private System.Windows.Forms.CheckBox chkNotificacionesSMS;
        private System.Windows.Forms.CheckBox chkNotificacionesPush;
        private System.Windows.Forms.CheckBox chkNotificacionesMarketing;
        private System.Windows.Forms.GroupBox gbApariencia;
        private System.Windows.Forms.CheckBox chkModoOscuro;
        private System.Windows.Forms.CheckBox chkAltoContraste;
        private System.Windows.Forms.Label lblIdioma;
        private System.Windows.Forms.ComboBox cmbIdioma;
        private System.Windows.Forms.Label lblMoneda;
        private System.Windows.Forms.ComboBox cmbMoneda;
        private System.Windows.Forms.Label lblTamanoFuente;
        private System.Windows.Forms.TrackBar trackTamanoFuente;
        private System.Windows.Forms.Label lblTamanoValor;
        private System.Windows.Forms.GroupBox gbSeguridad;
        private System.Windows.Forms.CheckBox chkDosFactores;
        private System.Windows.Forms.CheckBox chkSesionSegura;
        private System.Windows.Forms.Label lblTiempoSesion;
        private System.Windows.Forms.NumericUpDown numTiempoSesion;
        private System.Windows.Forms.GroupBox gbPreferencias;
        private System.Windows.Forms.CheckBox chkMostrarSaldo;
        private System.Windows.Forms.CheckBox chkOrdenarCuentas;
        private System.Windows.Forms.CheckBox chkConfirmarTransferencias;
        private System.Windows.Forms.CheckBox chkGuardarBeneficiarios;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnRestablecer;

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.pnlContenido = new System.Windows.Forms.Panel();
            this.gbNotificaciones = new System.Windows.Forms.GroupBox();
            this.gbApariencia = new System.Windows.Forms.GroupBox();
            this.gbSeguridad = new System.Windows.Forms.GroupBox();
            this.gbPreferencias = new System.Windows.Forms.GroupBox();
            this.chkNotificacionesEmail = new System.Windows.Forms.CheckBox();
            this.chkNotificacionesSMS = new System.Windows.Forms.CheckBox();
            this.chkNotificacionesPush = new System.Windows.Forms.CheckBox();
            this.chkNotificacionesMarketing = new System.Windows.Forms.CheckBox();
            this.chkModoOscuro = new System.Windows.Forms.CheckBox();
            this.chkAltoContraste = new System.Windows.Forms.CheckBox();
            this.lblIdioma = new System.Windows.Forms.Label();
            this.cmbIdioma = new System.Windows.Forms.ComboBox();
            this.lblMoneda = new System.Windows.Forms.Label();
            this.cmbMoneda = new System.Windows.Forms.ComboBox();
            this.lblTamanoFuente = new System.Windows.Forms.Label();
            this.trackTamanoFuente = new System.Windows.Forms.TrackBar();
            this.lblTamanoValor = new System.Windows.Forms.Label();
            this.chkDosFactores = new System.Windows.Forms.CheckBox();
            this.chkSesionSegura = new System.Windows.Forms.CheckBox();
            this.lblTiempoSesion = new System.Windows.Forms.Label();
            this.numTiempoSesion = new System.Windows.Forms.NumericUpDown();
            this.chkMostrarSaldo = new System.Windows.Forms.CheckBox();
            this.chkOrdenarCuentas = new System.Windows.Forms.CheckBox();
            this.chkConfirmarTransferencias = new System.Windows.Forms.CheckBox();
            this.chkGuardarBeneficiarios = new System.Windows.Forms.CheckBox();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnRestablecer = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackTamanoFuente)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTiempoSesion)).BeginInit();
            this.SuspendLayout();
            this.trackTamanoFuente.Minimum = 75; this.trackTamanoFuente.Maximum = 150; this.trackTamanoFuente.Value = 100;
            this.numTiempoSesion.Minimum = 5; this.numTiempoSesion.Maximum = 120; this.numTiempoSesion.Value = 30;
            this.btnGuardar.Click += new System.EventHandler(this.BtnGuardar_Click);
            this.btnCancelar.Click += new System.EventHandler(this.BtnCancelar_Click);
            this.btnRestablecer.Click += new System.EventHandler(this.BtnRestablecer_Click);
            this.trackTamanoFuente.Scroll += new System.EventHandler(this.TrackTamanoFuente_Scroll);
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FrmConfiguracion";
            this.Text = "Configuración — Nexum Bank";
            ((System.ComponentModel.ISupportInitialize)(this.trackTamanoFuente)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTiempoSesion)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
