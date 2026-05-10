namespace NexumApp.Forms.Auth
{
    partial class FrmRegistro
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
            this.lblBeneficios = new System.Windows.Forms.Label();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.pnlNotificacion = new System.Windows.Forms.Panel();
            this.lblNotificacion = new System.Windows.Forms.Label();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblSubtitulo = new System.Windows.Forms.Label();
            this.lblNombre = new System.Windows.Forms.Label();
            this.pnlNombreContainer = new System.Windows.Forms.Panel();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.lblErrorNombre = new System.Windows.Forms.Label();
            this.lblApellidos = new System.Windows.Forms.Label();
            this.pnlApellidosContainer = new System.Windows.Forms.Panel();
            this.txtApellidos = new System.Windows.Forms.TextBox();
            this.lblErrorApellidos = new System.Windows.Forms.Label();
            this.lblEmail = new System.Windows.Forms.Label();
            this.pnlEmailContainer = new System.Windows.Forms.Panel();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblErrorEmail = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.pnlPasswordContainer = new System.Windows.Forms.Panel();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblErrorPassword = new System.Windows.Forms.Label();
            this.lblConfirmar = new System.Windows.Forms.Label();
            this.pnlConfirmarContainer = new System.Windows.Forms.Panel();
            this.txtConfirmar = new System.Windows.Forms.TextBox();
            this.lblErrorConfirmar = new System.Windows.Forms.Label();
            this.btnRegistrar = new System.Windows.Forms.Button();
            this.lnkLogin = new System.Windows.Forms.LinkLabel();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.timerNotificacion = new System.Windows.Forms.Timer(this.components);
            this.pnlContenedor.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.pnlRight.SuspendLayout();
            this.pnlNotificacion.SuspendLayout();
            this.pnlNombreContainer.SuspendLayout();
            this.pnlApellidosContainer.SuspendLayout();
            this.pnlEmailContainer.SuspendLayout();
            this.pnlPasswordContainer.SuspendLayout();
            this.pnlConfirmarContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlContenedor
            // 
            this.pnlContenedor.BackColor = System.Drawing.Color.Transparent;
            this.pnlContenedor.Controls.Add(this.pnlRight);
            this.pnlContenedor.Controls.Add(this.pnlLeft);
            this.pnlContenedor.Location = new System.Drawing.Point(50, 30);
            this.pnlContenedor.Name = "pnlContenedor";
            this.pnlContenedor.Size = new System.Drawing.Size(1000, 680);
            this.pnlContenedor.TabIndex = 0;
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.Color.FromArgb(22, 22, 26);
            this.pnlLeft.Controls.Add(this.picLogo);
            this.pnlLeft.Controls.Add(this.lblNexum);
            this.pnlLeft.Controls.Add(this.lblBank);
            this.pnlLeft.Controls.Add(this.pnlGoldLine);
            this.pnlLeft.Controls.Add(this.lblBeneficios);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(380, 680);
            this.pnlLeft.TabIndex = 0;
            // 
            // picLogo
            // 
            this.picLogo.BackColor = System.Drawing.Color.Transparent;
            this.picLogo.Location = new System.Drawing.Point(90, 40);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(200, 140);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogo.TabIndex = 0;
            this.picLogo.TabStop = false;
            // 
            // lblNexum
            // 
            this.lblNexum.AutoSize = true;
            this.lblNexum.Font = new System.Drawing.Font("Segoe UI", 32F, System.Drawing.FontStyle.Bold);
            this.lblNexum.ForeColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.lblNexum.Location = new System.Drawing.Point(85, 190);
            this.lblNexum.Name = "lblNexum";
            this.lblNexum.Size = new System.Drawing.Size(186, 59);
            this.lblNexum.TabIndex = 1;
            this.lblNexum.Text = "NEXUM";
            // 
            // lblBank
            // 
            this.lblBank.AutoSize = true;
            this.lblBank.Font = new System.Drawing.Font("Segoe UI Light", 18F);
            this.lblBank.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            this.lblBank.Location = new System.Drawing.Point(125, 250);
            this.lblBank.Name = "lblBank";
            this.lblBank.Size = new System.Drawing.Size(106, 32);
            this.lblBank.TabIndex = 2;
            this.lblBank.Text = "B A N K";
            // 
            // pnlGoldLine
            // 
            this.pnlGoldLine.BackColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.pnlGoldLine.Location = new System.Drawing.Point(120, 300);
            this.pnlGoldLine.Name = "pnlGoldLine";
            this.pnlGoldLine.Size = new System.Drawing.Size(140, 3);
            this.pnlGoldLine.TabIndex = 3;
            // 
            // lblBeneficios
            // 
            this.lblBeneficios.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblBeneficios.ForeColor = System.Drawing.Color.FromArgb(150, 150, 150);
            this.lblBeneficios.Location = new System.Drawing.Point(30, 340);
            this.lblBeneficios.Name = "lblBeneficios";
            this.lblBeneficios.Size = new System.Drawing.Size(320, 300);
            this.lblBeneficios.TabIndex = 4;
            this.lblBeneficios.Text = "✓  Cuenta 100% gratuita\r\n\r\n✓  Transferencias sin comisiones\r\n\r\n✓  App móvil y escritorio\r\n\r\n✓  Atención 24/7\r\n\r\n✓  Máxima seguridad";
            // 
            // pnlRight
            // 
            this.pnlRight.BackColor = System.Drawing.Color.FromArgb(18, 18, 22);
            this.pnlRight.Controls.Add(this.pnlNotificacion);
            this.pnlRight.Controls.Add(this.lblTitulo);
            this.pnlRight.Controls.Add(this.lblSubtitulo);
            this.pnlRight.Controls.Add(this.lblNombre);
            this.pnlRight.Controls.Add(this.pnlNombreContainer);
            this.pnlRight.Controls.Add(this.lblErrorNombre);
            this.pnlRight.Controls.Add(this.lblApellidos);
            this.pnlRight.Controls.Add(this.pnlApellidosContainer);
            this.pnlRight.Controls.Add(this.lblErrorApellidos);
            this.pnlRight.Controls.Add(this.lblEmail);
            this.pnlRight.Controls.Add(this.pnlEmailContainer);
            this.pnlRight.Controls.Add(this.lblErrorEmail);
            this.pnlRight.Controls.Add(this.lblPassword);
            this.pnlRight.Controls.Add(this.pnlPasswordContainer);
            this.pnlRight.Controls.Add(this.lblErrorPassword);
            this.pnlRight.Controls.Add(this.lblConfirmar);
            this.pnlRight.Controls.Add(this.pnlConfirmarContainer);
            this.pnlRight.Controls.Add(this.lblErrorConfirmar);
            this.pnlRight.Controls.Add(this.btnRegistrar);
            this.pnlRight.Controls.Add(this.lnkLogin);
            this.pnlRight.Controls.Add(this.btnCerrar);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(380, 0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(620, 680);
            this.pnlRight.TabIndex = 1;
            // 
            // pnlNotificacion
            // 
            this.pnlNotificacion.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            this.pnlNotificacion.Controls.Add(this.lblNotificacion);
            this.pnlNotificacion.Location = new System.Drawing.Point(40, 15);
            this.pnlNotificacion.Name = "pnlNotificacion";
            this.pnlNotificacion.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.pnlNotificacion.Size = new System.Drawing.Size(540, 0);
            this.pnlNotificacion.TabIndex = 30;
            this.pnlNotificacion.Visible = false;
            // 
            // lblNotificacion
            // 
            this.lblNotificacion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNotificacion.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblNotificacion.ForeColor = System.Drawing.Color.White;
            this.lblNotificacion.Location = new System.Drawing.Point(15, 10);
            this.lblNotificacion.Name = "lblNotificacion";
            this.lblNotificacion.Size = new System.Drawing.Size(510, 0);
            this.lblNotificacion.TabIndex = 0;
            this.lblNotificacion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(40, 35);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(230, 45);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Crear cuenta";
            // 
            // lblSubtitulo
            // 
            this.lblSubtitulo.AutoSize = true;
            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(140, 140, 140);
            this.lblSubtitulo.Location = new System.Drawing.Point(44, 85);
            this.lblSubtitulo.Name = "lblSubtitulo";
            this.lblSubtitulo.Size = new System.Drawing.Size(280, 19);
            this.lblSubtitulo.TabIndex = 1;
            this.lblSubtitulo.Text = "Completa tus datos para unirte a Nexum";
            // 
            // lblNombre
            // 
            this.lblNombre.AutoSize = true;
            this.lblNombre.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblNombre.ForeColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.lblNombre.Location = new System.Drawing.Point(44, 125);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(54, 15);
            this.lblNombre.TabIndex = 2;
            this.lblNombre.Text = "Nombre";
            // 
            // pnlNombreContainer
            // 
            this.pnlNombreContainer.BackColor = System.Drawing.Color.FromArgb(35, 35, 40);
            this.pnlNombreContainer.Controls.Add(this.txtNombre);
            this.pnlNombreContainer.Location = new System.Drawing.Point(44, 145);
            this.pnlNombreContainer.Name = "pnlNombreContainer";
            this.pnlNombreContainer.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.pnlNombreContainer.Size = new System.Drawing.Size(250, 42);
            this.pnlNombreContainer.TabIndex = 3;
            // 
            // txtNombre
            // 
            this.txtNombre.BackColor = System.Drawing.Color.FromArgb(35, 35, 40);
            this.txtNombre.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtNombre.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtNombre.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtNombre.ForeColor = System.Drawing.Color.White;
            this.txtNombre.Location = new System.Drawing.Point(12, 10);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(226, 20);
            this.txtNombre.TabIndex = 1;
            // 
            // lblErrorNombre
            // 
            this.lblErrorNombre.AutoSize = true;
            this.lblErrorNombre.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblErrorNombre.ForeColor = System.Drawing.Color.FromArgb(255, 100, 100);
            this.lblErrorNombre.Location = new System.Drawing.Point(44, 190);
            this.lblErrorNombre.Name = "lblErrorNombre";
            this.lblErrorNombre.Size = new System.Drawing.Size(0, 13);
            this.lblErrorNombre.TabIndex = 4;
            // 
            // lblApellidos
            // 
            this.lblApellidos.AutoSize = true;
            this.lblApellidos.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblApellidos.ForeColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.lblApellidos.Location = new System.Drawing.Point(310, 125);
            this.lblApellidos.Name = "lblApellidos";
            this.lblApellidos.Size = new System.Drawing.Size(57, 15);
            this.lblApellidos.TabIndex = 5;
            this.lblApellidos.Text = "Apellidos";
            // 
            // pnlApellidosContainer
            // 
            this.pnlApellidosContainer.BackColor = System.Drawing.Color.FromArgb(35, 35, 40);
            this.pnlApellidosContainer.Controls.Add(this.txtApellidos);
            this.pnlApellidosContainer.Location = new System.Drawing.Point(310, 145);
            this.pnlApellidosContainer.Name = "pnlApellidosContainer";
            this.pnlApellidosContainer.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.pnlApellidosContainer.Size = new System.Drawing.Size(270, 42);
            this.pnlApellidosContainer.TabIndex = 6;
            // 
            // txtApellidos
            // 
            this.txtApellidos.BackColor = System.Drawing.Color.FromArgb(35, 35, 40);
            this.txtApellidos.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtApellidos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtApellidos.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtApellidos.ForeColor = System.Drawing.Color.White;
            this.txtApellidos.Location = new System.Drawing.Point(12, 10);
            this.txtApellidos.Name = "txtApellidos";
            this.txtApellidos.Size = new System.Drawing.Size(246, 20);
            this.txtApellidos.TabIndex = 2;
            // 
            // lblErrorApellidos
            // 
            this.lblErrorApellidos.AutoSize = true;
            this.lblErrorApellidos.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblErrorApellidos.ForeColor = System.Drawing.Color.FromArgb(255, 100, 100);
            this.lblErrorApellidos.Location = new System.Drawing.Point(310, 190);
            this.lblErrorApellidos.Name = "lblErrorApellidos";
            this.lblErrorApellidos.Size = new System.Drawing.Size(0, 13);
            this.lblErrorApellidos.TabIndex = 7;
            // 
            // lblEmail
            // 
            this.lblEmail.AutoSize = true;
            this.lblEmail.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblEmail.ForeColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.lblEmail.Location = new System.Drawing.Point(44, 210);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(107, 15);
            this.lblEmail.TabIndex = 8;
            this.lblEmail.Text = "Correo electrónico";
            // 
            // pnlEmailContainer
            // 
            this.pnlEmailContainer.BackColor = System.Drawing.Color.FromArgb(35, 35, 40);
            this.pnlEmailContainer.Controls.Add(this.txtEmail);
            this.pnlEmailContainer.Location = new System.Drawing.Point(44, 230);
            this.pnlEmailContainer.Name = "pnlEmailContainer";
            this.pnlEmailContainer.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.pnlEmailContainer.Size = new System.Drawing.Size(536, 42);
            this.pnlEmailContainer.TabIndex = 9;
            // 
            // txtEmail
            // 
            this.txtEmail.BackColor = System.Drawing.Color.FromArgb(35, 35, 40);
            this.txtEmail.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtEmail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEmail.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtEmail.ForeColor = System.Drawing.Color.White;
            this.txtEmail.Location = new System.Drawing.Point(12, 10);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(512, 20);
            this.txtEmail.TabIndex = 3;
            // 
            // lblErrorEmail
            // 
            this.lblErrorEmail.AutoSize = true;
            this.lblErrorEmail.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblErrorEmail.ForeColor = System.Drawing.Color.FromArgb(255, 100, 100);
            this.lblErrorEmail.Location = new System.Drawing.Point(44, 275);
            this.lblErrorEmail.Name = "lblErrorEmail";
            this.lblErrorEmail.Size = new System.Drawing.Size(0, 13);
            this.lblErrorEmail.TabIndex = 10;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblPassword.ForeColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.lblPassword.Location = new System.Drawing.Point(44, 295);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(70, 15);
            this.lblPassword.TabIndex = 11;
            this.lblPassword.Text = "Contraseña";
            // 
            // pnlPasswordContainer
            // 
            this.pnlPasswordContainer.BackColor = System.Drawing.Color.FromArgb(35, 35, 40);
            this.pnlPasswordContainer.Controls.Add(this.txtPassword);
            this.pnlPasswordContainer.Location = new System.Drawing.Point(44, 315);
            this.pnlPasswordContainer.Name = "pnlPasswordContainer";
            this.pnlPasswordContainer.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.pnlPasswordContainer.Size = new System.Drawing.Size(536, 42);
            this.pnlPasswordContainer.TabIndex = 12;
            // 
            // txtPassword
            // 
            this.txtPassword.BackColor = System.Drawing.Color.FromArgb(35, 35, 40);
            this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtPassword.ForeColor = System.Drawing.Color.White;
            this.txtPassword.Location = new System.Drawing.Point(12, 10);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '●';
            this.txtPassword.Size = new System.Drawing.Size(512, 20);
            this.txtPassword.TabIndex = 4;
            // 
            // lblErrorPassword
            // 
            this.lblErrorPassword.AutoSize = true;
            this.lblErrorPassword.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblErrorPassword.ForeColor = System.Drawing.Color.FromArgb(255, 100, 100);
            this.lblErrorPassword.Location = new System.Drawing.Point(44, 360);
            this.lblErrorPassword.Name = "lblErrorPassword";
            this.lblErrorPassword.Size = new System.Drawing.Size(0, 13);
            this.lblErrorPassword.TabIndex = 13;
            // 
            // lblConfirmar
            // 
            this.lblConfirmar.AutoSize = true;
            this.lblConfirmar.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblConfirmar.ForeColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.lblConfirmar.Location = new System.Drawing.Point(44, 380);
            this.lblConfirmar.Name = "lblConfirmar";
            this.lblConfirmar.Size = new System.Drawing.Size(123, 15);
            this.lblConfirmar.TabIndex = 14;
            this.lblConfirmar.Text = "Confirmar contraseña";
            // 
            // pnlConfirmarContainer
            // 
            this.pnlConfirmarContainer.BackColor = System.Drawing.Color.FromArgb(35, 35, 40);
            this.pnlConfirmarContainer.Controls.Add(this.txtConfirmar);
            this.pnlConfirmarContainer.Location = new System.Drawing.Point(44, 400);
            this.pnlConfirmarContainer.Name = "pnlConfirmarContainer";
            this.pnlConfirmarContainer.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.pnlConfirmarContainer.Size = new System.Drawing.Size(536, 42);
            this.pnlConfirmarContainer.TabIndex = 15;
            // 
            // txtConfirmar
            // 
            this.txtConfirmar.BackColor = System.Drawing.Color.FromArgb(35, 35, 40);
            this.txtConfirmar.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtConfirmar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtConfirmar.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtConfirmar.ForeColor = System.Drawing.Color.White;
            this.txtConfirmar.Location = new System.Drawing.Point(12, 10);
            this.txtConfirmar.Name = "txtConfirmar";
            this.txtConfirmar.PasswordChar = '●';
            this.txtConfirmar.Size = new System.Drawing.Size(512, 20);
            this.txtConfirmar.TabIndex = 5;
            // 
            // lblErrorConfirmar
            // 
            this.lblErrorConfirmar.AutoSize = true;
            this.lblErrorConfirmar.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblErrorConfirmar.ForeColor = System.Drawing.Color.FromArgb(255, 100, 100);
            this.lblErrorConfirmar.Location = new System.Drawing.Point(44, 445);
            this.lblErrorConfirmar.Name = "lblErrorConfirmar";
            this.lblErrorConfirmar.Size = new System.Drawing.Size(0, 13);
            this.lblErrorConfirmar.TabIndex = 16;
            // 
            // btnRegistrar
            // 
            this.btnRegistrar.BackColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.btnRegistrar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRegistrar.FlatAppearance.BorderSize = 0;
            this.btnRegistrar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(190, 155, 45);
            this.btnRegistrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRegistrar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnRegistrar.ForeColor = System.Drawing.Color.FromArgb(22, 22, 26);
            this.btnRegistrar.Location = new System.Drawing.Point(44, 480);
            this.btnRegistrar.Name = "btnRegistrar";
            this.btnRegistrar.Size = new System.Drawing.Size(536, 55);
            this.btnRegistrar.TabIndex = 6;
            this.btnRegistrar.Text = "CREAR CUENTA";
            this.btnRegistrar.UseVisualStyleBackColor = false;
            this.btnRegistrar.Click += new System.EventHandler(this.BtnRegistrar_Click);
            // 
            // lnkLogin
            // 
            this.lnkLogin.ActiveLinkColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.lnkLogin.AutoSize = true;
            this.lnkLogin.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lnkLogin.LinkColor = System.Drawing.Color.FromArgb(140, 140, 140);
            this.lnkLogin.Location = new System.Drawing.Point(170, 555);
            this.lnkLogin.Name = "lnkLogin";
            this.lnkLogin.Size = new System.Drawing.Size(240, 19);
            this.lnkLogin.TabIndex = 7;
            this.lnkLogin.TabStop = true;
            this.lnkLogin.Text = "¿Ya tienes cuenta? Inicia sesión";
            this.lnkLogin.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkLogin_LinkClicked);
            // 
            // btnCerrar
            // 
            this.btnCerrar.BackColor = System.Drawing.Color.Transparent;
            this.btnCerrar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCerrar.FlatAppearance.BorderSize = 0;
            this.btnCerrar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(60, 60, 65);
            this.btnCerrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.btnCerrar.ForeColor = System.Drawing.Color.FromArgb(150, 150, 150);
            this.btnCerrar.Location = new System.Drawing.Point(560, 10);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(45, 35);
            this.btnCerrar.TabIndex = 20;
            this.btnCerrar.Text = "✕";
            this.btnCerrar.UseVisualStyleBackColor = false;
            this.btnCerrar.Click += new System.EventHandler(this.BtnCerrar_Click);
            // 
            // timerNotificacion
            // 
            this.timerNotificacion.Interval = 4000;
            this.timerNotificacion.Tick += new System.EventHandler(this.TimerNotificacion_Tick);
            // 
            // FrmRegistro
            // 
            this.AcceptButton = this.btnRegistrar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 35);
            this.ClientSize = new System.Drawing.Size(1100, 750);
            this.Controls.Add(this.pnlContenedor);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimumSize = new System.Drawing.Size(1000, 700);
            this.Name = "FrmRegistro";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Nexum Bank - Crear cuenta";
            this.Load += new System.EventHandler(this.FrmRegistro_Load);
            this.Resize += new System.EventHandler(this.FrmRegistro_Resize);
            this.pnlContenedor.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.pnlLeft.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.pnlRight.ResumeLayout(false);
            this.pnlRight.PerformLayout();
            this.pnlNotificacion.ResumeLayout(false);
            this.pnlNombreContainer.ResumeLayout(false);
            this.pnlNombreContainer.PerformLayout();
            this.pnlApellidosContainer.ResumeLayout(false);
            this.pnlApellidosContainer.PerformLayout();
            this.pnlEmailContainer.ResumeLayout(false);
            this.pnlEmailContainer.PerformLayout();
            this.pnlPasswordContainer.ResumeLayout(false);
            this.pnlPasswordContainer.PerformLayout();
            this.pnlConfirmarContainer.ResumeLayout(false);
            this.pnlConfirmarContainer.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlContenedor;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblNexum;
        private System.Windows.Forms.Label lblBank;
        private System.Windows.Forms.Panel pnlGoldLine;
        private System.Windows.Forms.Label lblBeneficios;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.Panel pnlNotificacion;
        private System.Windows.Forms.Label lblNotificacion;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.Panel pnlNombreContainer;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblErrorNombre;
        private System.Windows.Forms.Label lblApellidos;
        private System.Windows.Forms.Panel pnlApellidosContainer;
        private System.Windows.Forms.TextBox txtApellidos;
        private System.Windows.Forms.Label lblErrorApellidos;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.Panel pnlEmailContainer;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label lblErrorEmail;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Panel pnlPasswordContainer;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblErrorPassword;
        private System.Windows.Forms.Label lblConfirmar;
        private System.Windows.Forms.Panel pnlConfirmarContainer;
        private System.Windows.Forms.TextBox txtConfirmar;
        private System.Windows.Forms.Label lblErrorConfirmar;
        private System.Windows.Forms.Button btnRegistrar;
        private System.Windows.Forms.LinkLabel lnkLogin;
        private System.Windows.Forms.Button btnCerrar;
        private System.Windows.Forms.Timer timerNotificacion;
    }
}
