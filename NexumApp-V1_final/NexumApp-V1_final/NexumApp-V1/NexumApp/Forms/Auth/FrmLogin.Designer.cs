namespace NexumApp.Forms.Auth
{
    partial class FrmLogin
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlContenedor = new System.Windows.Forms.Panel();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.lblNexum = new System.Windows.Forms.Label();
            this.lblBank = new System.Windows.Forms.Label();
            this.pnlGoldLine = new System.Windows.Forms.Panel();
            this.lblSlogan = new System.Windows.Forms.Label();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.pnlNotificacion = new System.Windows.Forms.Panel();
            this.lblNotificacion = new System.Windows.Forms.Label();
            this.lblAcceso = new System.Windows.Forms.Label();
            this.lblDescripcion = new System.Windows.Forms.Label();
            this.lblEmail = new System.Windows.Forms.Label();
            this.pnlEmailContainer = new System.Windows.Forms.Panel();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblErrorEmail = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.pnlPasswordContainer = new System.Windows.Forms.Panel();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblErrorPassword = new System.Windows.Forms.Label();
            this.lnkOlvido = new System.Windows.Forms.LinkLabel();
            this.btnLogin = new System.Windows.Forms.Button();
            this.lnkRegistro = new System.Windows.Forms.LinkLabel();
            this.lblVersion = new System.Windows.Forms.Label();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.timerNotificacion = new System.Windows.Forms.Timer(this.components);
            this.pnlContenedor.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.pnlRight.SuspendLayout();
            this.pnlNotificacion.SuspendLayout();
            this.pnlEmailContainer.SuspendLayout();
            this.pnlPasswordContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlContenedor
            // 
            this.pnlContenedor.BackColor = System.Drawing.Color.Transparent;
            this.pnlContenedor.Controls.Add(this.pnlRight);
            this.pnlContenedor.Controls.Add(this.pnlLeft);
            this.pnlContenedor.Location = new System.Drawing.Point(100, 50);
            this.pnlContenedor.Name = "pnlContenedor";
            this.pnlContenedor.Size = new System.Drawing.Size(950, 580);
            this.pnlContenedor.TabIndex = 0;
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.Color.FromArgb(22, 22, 26);
            this.pnlLeft.Controls.Add(this.picLogo);
            this.pnlLeft.Controls.Add(this.lblNexum);
            this.pnlLeft.Controls.Add(this.lblBank);
            this.pnlLeft.Controls.Add(this.pnlGoldLine);
            this.pnlLeft.Controls.Add(this.lblSlogan);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(420, 580);
            this.pnlLeft.TabIndex = 0;
            // 
            // picLogo
            // 
            this.picLogo.BackColor = System.Drawing.Color.Transparent;
            this.picLogo.Location = new System.Drawing.Point(85, 50);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(250, 180);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogo.TabIndex = 0;
            this.picLogo.TabStop = false;
            // 
            // lblNexum
            // 
            this.lblNexum.AutoSize = true;
            this.lblNexum.Font = new System.Drawing.Font("Segoe UI", 38F, System.Drawing.FontStyle.Bold);
            this.lblNexum.ForeColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.lblNexum.Location = new System.Drawing.Point(100, 240);
            this.lblNexum.Name = "lblNexum";
            this.lblNexum.Size = new System.Drawing.Size(220, 68);
            this.lblNexum.TabIndex = 1;
            this.lblNexum.Text = "NEXUM";
            // 
            // lblBank
            // 
            this.lblBank.AutoSize = true;
            this.lblBank.Font = new System.Drawing.Font("Segoe UI Light", 22F);
            this.lblBank.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            this.lblBank.Location = new System.Drawing.Point(145, 310);
            this.lblBank.Name = "lblBank";
            this.lblBank.Size = new System.Drawing.Size(130, 41);
            this.lblBank.TabIndex = 2;
            this.lblBank.Text = "B A N K";
            // 
            // pnlGoldLine
            // 
            this.pnlGoldLine.BackColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.pnlGoldLine.Location = new System.Drawing.Point(140, 370);
            this.pnlGoldLine.Name = "pnlGoldLine";
            this.pnlGoldLine.Size = new System.Drawing.Size(140, 3);
            this.pnlGoldLine.TabIndex = 3;
            // 
            // lblSlogan
            // 
            this.lblSlogan.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Italic);
            this.lblSlogan.ForeColor = System.Drawing.Color.FromArgb(150, 150, 150);
            this.lblSlogan.Location = new System.Drawing.Point(30, 450);
            this.lblSlogan.Name = "lblSlogan";
            this.lblSlogan.Size = new System.Drawing.Size(360, 80);
            this.lblSlogan.TabIndex = 4;
            this.lblSlogan.Text = "\"Tu seguridad financiera es nuestra prioridad.\"";
            this.lblSlogan.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlRight
            // 
            this.pnlRight.BackColor = System.Drawing.Color.FromArgb(18, 18, 22);
            this.pnlRight.Controls.Add(this.pnlNotificacion);
            this.pnlRight.Controls.Add(this.lblAcceso);
            this.pnlRight.Controls.Add(this.lblDescripcion);
            this.pnlRight.Controls.Add(this.lblEmail);
            this.pnlRight.Controls.Add(this.pnlEmailContainer);
            this.pnlRight.Controls.Add(this.lblErrorEmail);
            this.pnlRight.Controls.Add(this.lblPassword);
            this.pnlRight.Controls.Add(this.pnlPasswordContainer);
            this.pnlRight.Controls.Add(this.lblErrorPassword);
            this.pnlRight.Controls.Add(this.lnkOlvido);
            this.pnlRight.Controls.Add(this.btnLogin);
            this.pnlRight.Controls.Add(this.lnkRegistro);
            this.pnlRight.Controls.Add(this.lblVersion);
            this.pnlRight.Controls.Add(this.btnCerrar);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(420, 0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(530, 580);
            this.pnlRight.TabIndex = 1;
            // 
            // pnlNotificacion (Toast notification)
            // 
            this.pnlNotificacion.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            this.pnlNotificacion.Controls.Add(this.lblNotificacion);
            this.pnlNotificacion.Location = new System.Drawing.Point(30, 15);
            this.pnlNotificacion.Name = "pnlNotificacion";
            this.pnlNotificacion.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.pnlNotificacion.Size = new System.Drawing.Size(470, 0);
            this.pnlNotificacion.TabIndex = 20;
            this.pnlNotificacion.Visible = false;
            // 
            // lblNotificacion
            // 
            this.lblNotificacion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNotificacion.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblNotificacion.ForeColor = System.Drawing.Color.White;
            this.lblNotificacion.Location = new System.Drawing.Point(15, 10);
            this.lblNotificacion.Name = "lblNotificacion";
            this.lblNotificacion.Size = new System.Drawing.Size(440, 0);
            this.lblNotificacion.TabIndex = 0;
            this.lblNotificacion.Text = "Mensaje de notificación";
            this.lblNotificacion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAcceso
            // 
            this.lblAcceso.AutoSize = true;
            this.lblAcceso.Font = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Bold);
            this.lblAcceso.ForeColor = System.Drawing.Color.White;
            this.lblAcceso.Location = new System.Drawing.Point(65, 60);
            this.lblAcceso.Name = "lblAcceso";
            this.lblAcceso.Size = new System.Drawing.Size(237, 51);
            this.lblAcceso.TabIndex = 0;
            this.lblAcceso.Text = "Bienvenido";
            // 
            // lblDescripcion
            // 
            this.lblDescripcion.AutoSize = true;
            this.lblDescripcion.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblDescripcion.ForeColor = System.Drawing.Color.FromArgb(140, 140, 140);
            this.lblDescripcion.Location = new System.Drawing.Point(69, 120);
            this.lblDescripcion.Name = "lblDescripcion";
            this.lblDescripcion.Size = new System.Drawing.Size(290, 20);
            this.lblDescripcion.TabIndex = 7;
            this.lblDescripcion.Text = "Accede a tu cuenta de banca premium";
            // 
            // lblEmail
            // 
            this.lblEmail.AutoSize = true;
            this.lblEmail.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblEmail.ForeColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.lblEmail.Location = new System.Drawing.Point(69, 170);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(133, 19);
            this.lblEmail.TabIndex = 1;
            this.lblEmail.Text = "Correo electrónico";
            // 
            // pnlEmailContainer
            // 
            this.pnlEmailContainer.BackColor = System.Drawing.Color.FromArgb(35, 35, 40);
            this.pnlEmailContainer.Controls.Add(this.txtEmail);
            this.pnlEmailContainer.Location = new System.Drawing.Point(69, 195);
            this.pnlEmailContainer.Name = "pnlEmailContainer";
            this.pnlEmailContainer.Padding = new System.Windows.Forms.Padding(18, 15, 18, 15);
            this.pnlEmailContainer.Size = new System.Drawing.Size(390, 55);
            this.pnlEmailContainer.TabIndex = 8;
            // 
            // txtEmail
            // 
            this.txtEmail.BackColor = System.Drawing.Color.FromArgb(35, 35, 40);
            this.txtEmail.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtEmail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEmail.Font = new System.Drawing.Font("Segoe UI", 13F);
            this.txtEmail.ForeColor = System.Drawing.Color.White;
            this.txtEmail.Location = new System.Drawing.Point(18, 15);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(354, 24);
            this.txtEmail.TabIndex = 1;
            // 
            // lblErrorEmail
            // 
            this.lblErrorEmail.AutoSize = true;
            this.lblErrorEmail.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblErrorEmail.ForeColor = System.Drawing.Color.FromArgb(255, 100, 100);
            this.lblErrorEmail.Location = new System.Drawing.Point(69, 253);
            this.lblErrorEmail.Name = "lblErrorEmail";
            this.lblErrorEmail.Size = new System.Drawing.Size(0, 15);
            this.lblErrorEmail.TabIndex = 21;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblPassword.ForeColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.lblPassword.Location = new System.Drawing.Point(69, 275);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(85, 19);
            this.lblPassword.TabIndex = 3;
            this.lblPassword.Text = "Contraseña";
            // 
            // pnlPasswordContainer
            // 
            this.pnlPasswordContainer.BackColor = System.Drawing.Color.FromArgb(35, 35, 40);
            this.pnlPasswordContainer.Controls.Add(this.txtPassword);
            this.pnlPasswordContainer.Location = new System.Drawing.Point(69, 300);
            this.pnlPasswordContainer.Name = "pnlPasswordContainer";
            this.pnlPasswordContainer.Padding = new System.Windows.Forms.Padding(18, 15, 18, 15);
            this.pnlPasswordContainer.Size = new System.Drawing.Size(390, 55);
            this.pnlPasswordContainer.TabIndex = 9;
            // 
            // txtPassword
            // 
            this.txtPassword.BackColor = System.Drawing.Color.FromArgb(35, 35, 40);
            this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 13F);
            this.txtPassword.ForeColor = System.Drawing.Color.White;
            this.txtPassword.Location = new System.Drawing.Point(18, 15);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '●';
            this.txtPassword.Size = new System.Drawing.Size(354, 24);
            this.txtPassword.TabIndex = 2;
            // 
            // lblErrorPassword
            // 
            this.lblErrorPassword.AutoSize = true;
            this.lblErrorPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblErrorPassword.ForeColor = System.Drawing.Color.FromArgb(255, 100, 100);
            this.lblErrorPassword.Location = new System.Drawing.Point(69, 358);
            this.lblErrorPassword.Name = "lblErrorPassword";
            this.lblErrorPassword.Size = new System.Drawing.Size(0, 15);
            this.lblErrorPassword.TabIndex = 22;
            // 
            // lnkOlvido
            // 
            this.lnkOlvido.AutoSize = true;
            this.lnkOlvido.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lnkOlvido.LinkColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.lnkOlvido.ActiveLinkColor = System.Drawing.Color.FromArgb(255, 215, 100);
            this.lnkOlvido.Location = new System.Drawing.Point(290, 378);
            this.lnkOlvido.Name = "lnkOlvido";
            this.lnkOlvido.Size = new System.Drawing.Size(165, 15);
            this.lnkOlvido.TabIndex = 11;
            this.lnkOlvido.TabStop = true;
            this.lnkOlvido.Text = "¿Olvidaste tu contraseña?";
            this.lnkOlvido.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkOlvido_LinkClicked);
            // 
            // btnLogin
            // 
            this.btnLogin.BackColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(190, 155, 45);
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.btnLogin.ForeColor = System.Drawing.Color.FromArgb(22, 22, 26);
            this.btnLogin.Location = new System.Drawing.Point(69, 410);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(390, 60);
            this.btnLogin.TabIndex = 3;
            this.btnLogin.Text = "ACCEDER";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new System.EventHandler(this.BtnLogin_Click);
            // 
            // lnkRegistro
            // 
            this.lnkRegistro.ActiveLinkColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.lnkRegistro.AutoSize = true;
            this.lnkRegistro.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lnkRegistro.LinkColor = System.Drawing.Color.FromArgb(140, 140, 140);
            this.lnkRegistro.Location = new System.Drawing.Point(125, 490);
            this.lnkRegistro.Name = "lnkRegistro";
            this.lnkRegistro.Size = new System.Drawing.Size(268, 19);
            this.lnkRegistro.TabIndex = 4;
            this.lnkRegistro.TabStop = true;
            this.lnkRegistro.Text = "¿No tienes cuenta? Crear cuenta nueva";
            this.lnkRegistro.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkRegistro_LinkClicked);
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblVersion.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            this.lblVersion.Location = new System.Drawing.Point(210, 545);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(98, 13);
            this.lblVersion.TabIndex = 10;
            this.lblVersion.Text = "NexumApp v1.0.0";
            // 
            // btnCerrar
            // 
            this.btnCerrar.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
            this.btnCerrar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCerrar.FlatAppearance.BorderSize = 0;
            this.btnCerrar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(80, 80, 85);
            this.btnCerrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCerrar.ForeColor = System.Drawing.Color.FromArgb(180, 180, 180);
            this.btnCerrar.Location = new System.Drawing.Point(400, 540);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(120, 30);
            this.btnCerrar.TabIndex = 12;
            this.btnCerrar.Text = "Salir";
            this.btnCerrar.UseVisualStyleBackColor = false;
            this.btnCerrar.Click += new System.EventHandler(this.BtnCerrar_Click);
            // 
            // timerNotificacion
            // 
            this.timerNotificacion.Interval = 4000;
            this.timerNotificacion.Tick += new System.EventHandler(this.TimerNotificacion_Tick);
            // 
            // FrmLogin
            // 
            this.AcceptButton = this.btnLogin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 35);
            this.ClientSize = new System.Drawing.Size(1150, 680);
            this.Controls.Add(this.pnlContenedor);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1050, 700);
            this.Name = "FrmLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nexum Bank - Banca Premium";
            this.Load += new System.EventHandler(this.FrmLogin_Load);
            this.Resize += new System.EventHandler(this.FrmLogin_Resize);
            this.pnlContenedor.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.pnlLeft.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.pnlRight.ResumeLayout(false);
            this.pnlRight.PerformLayout();
            this.pnlNotificacion.ResumeLayout(false);
            this.pnlEmailContainer.ResumeLayout(false);
            this.pnlEmailContainer.PerformLayout();
            this.pnlPasswordContainer.ResumeLayout(false);
            this.pnlPasswordContainer.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlContenedor;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblNexum;
        private System.Windows.Forms.Label lblBank;
        private System.Windows.Forms.Panel pnlGoldLine;
        private System.Windows.Forms.Label lblSlogan;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.Panel pnlNotificacion;
        private System.Windows.Forms.Label lblNotificacion;
        private System.Windows.Forms.Label lblAcceso;
        private System.Windows.Forms.Label lblDescripcion;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.Panel pnlEmailContainer;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label lblErrorEmail;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Panel pnlPasswordContainer;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblErrorPassword;
        private System.Windows.Forms.LinkLabel lnkOlvido;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.LinkLabel lnkRegistro;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Button btnCerrar;
        private System.Windows.Forms.Timer timerNotificacion;
    }
}
