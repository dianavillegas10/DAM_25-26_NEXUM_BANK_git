namespace NexumApp.Forms.Admin
{
    partial class FrmGestionUsuarios
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
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.pnlBotones = new System.Windows.Forms.Panel();
            this.btnNuevo = new System.Windows.Forms.Button();
            this.btnEditar = new System.Windows.Forms.Button();
            this.btnBanear = new System.Windows.Forms.Button();
            this.btnDesbanear = new System.Windows.Forms.Button();
            this.btnActualizar = new System.Windows.Forms.Button();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.lblBuscar = new System.Windows.Forms.Label();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.lblFiltroRol = new System.Windows.Forms.Label();
            this.cmbFiltroRol = new System.Windows.Forms.ComboBox();
            this.lblFiltroEstado = new System.Windows.Forms.Label();
            this.cmbFiltroEstado = new System.Windows.Forms.ComboBox();
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.lblResultados = new System.Windows.Forms.Label();
            this.dgvUsuarios = new System.Windows.Forms.DataGridView();
            this.colId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNombre = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colApellidos = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEmail = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEstado = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlHeader.SuspendLayout();
            this.pnlBotones.SuspendLayout();
            this.pnlBuscar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(18, 22, 30);
            this.pnlHeader.Controls.Add(this.lblTitulo);
            this.pnlHeader.Controls.Add(this.btnCerrar);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(900, 50);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.lblTitulo.Location = new System.Drawing.Point(20, 12);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(182, 25);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Gestión de usuarios";
            // 
            // btnCerrar
            // 
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.BackColor = System.Drawing.Color.Transparent;
            this.btnCerrar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCerrar.FlatAppearance.BorderSize = 0;
            this.btnCerrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnCerrar.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            this.btnCerrar.Location = new System.Drawing.Point(850, 5);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(40, 40);
            this.btnCerrar.TabIndex = 1;
            this.btnCerrar.Text = "✕";
            this.btnCerrar.UseVisualStyleBackColor = false;
            this.btnCerrar.Click += new System.EventHandler(this.BtnCerrar_Click);
            // 
            // pnlBotones
            // 
            this.pnlBotones.BackColor = System.Drawing.Color.FromArgb(240, 242, 245);
            this.pnlBotones.Controls.Add(this.btnNuevo);
            this.pnlBotones.Controls.Add(this.btnEditar);
            this.pnlBotones.Controls.Add(this.btnBanear);
            this.pnlBotones.Controls.Add(this.btnDesbanear);
            this.pnlBotones.Controls.Add(this.btnActualizar);
            this.pnlBotones.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBotones.Location = new System.Drawing.Point(0, 50);
            this.pnlBotones.Name = "pnlBotones";
            this.pnlBotones.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.pnlBotones.Size = new System.Drawing.Size(900, 55);
            this.pnlBotones.TabIndex = 1;
            // 
            // btnNuevo
            // 
            this.btnNuevo.BackColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.btnNuevo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNuevo.FlatAppearance.BorderSize = 0;
            this.btnNuevo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNuevo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnNuevo.ForeColor = System.Drawing.Color.FromArgb(22, 22, 26);
            this.btnNuevo.Location = new System.Drawing.Point(20, 10);
            this.btnNuevo.Name = "btnNuevo";
            this.btnNuevo.Size = new System.Drawing.Size(160, 35);
            this.btnNuevo.TabIndex = 0;
            this.btnNuevo.Text = "➕ Nuevo usuario";
            this.btnNuevo.UseVisualStyleBackColor = false;
            this.btnNuevo.Click += new System.EventHandler(this.BtnNuevo_Click);
            // 
            // btnEditar
            // 
            this.btnEditar.BackColor = System.Drawing.Color.FromArgb(70, 130, 180);
            this.btnEditar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditar.FlatAppearance.BorderSize = 0;
            this.btnEditar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditar.ForeColor = System.Drawing.Color.White;
            this.btnEditar.Location = new System.Drawing.Point(325, 10);
            this.btnEditar.Name = "btnEditar";
            this.btnEditar.Size = new System.Drawing.Size(100, 35);
            this.btnEditar.TabIndex = 2;
            this.btnEditar.Text = "✏️ Editar";
            this.btnEditar.UseVisualStyleBackColor = false;
            this.btnEditar.Enabled = false;
            this.btnEditar.Click += new System.EventHandler(this.BtnEditar_Click);
            // 
            // btnBanear
            // 
            this.btnBanear.BackColor = System.Drawing.Color.FromArgb(200, 60, 60);
            this.btnBanear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBanear.FlatAppearance.BorderSize = 0;
            this.btnBanear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBanear.ForeColor = System.Drawing.Color.White;
            this.btnBanear.Location = new System.Drawing.Point(435, 10);
            this.btnBanear.Name = "btnBanear";
            this.btnBanear.Size = new System.Drawing.Size(100, 35);
            this.btnBanear.TabIndex = 3;
            this.btnBanear.Text = "🚫 Banear";
            this.btnBanear.UseVisualStyleBackColor = false;
            this.btnBanear.Enabled = false;
            this.btnBanear.Click += new System.EventHandler(this.BtnBanear_Click);
            // 
            // btnDesbanear
            // 
            this.btnDesbanear.BackColor = System.Drawing.Color.FromArgb(60, 140, 80);
            this.btnDesbanear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDesbanear.FlatAppearance.BorderSize = 0;
            this.btnDesbanear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesbanear.ForeColor = System.Drawing.Color.White;
            this.btnDesbanear.Location = new System.Drawing.Point(545, 10);
            this.btnDesbanear.Name = "btnDesbanear";
            this.btnDesbanear.Size = new System.Drawing.Size(110, 35);
            this.btnDesbanear.TabIndex = 4;
            this.btnDesbanear.Text = "✅ Desbanear";
            this.btnDesbanear.UseVisualStyleBackColor = false;
            this.btnDesbanear.Enabled = false;
            this.btnDesbanear.Click += new System.EventHandler(this.BtnDesbanear_Click);
            // 
            // btnActualizar
            // 
            this.btnActualizar.BackColor = System.Drawing.Color.FromArgb(100, 100, 110);
            this.btnActualizar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnActualizar.FlatAppearance.BorderSize = 0;
            this.btnActualizar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnActualizar.ForeColor = System.Drawing.Color.White;
            this.btnActualizar.Location = new System.Drawing.Point(665, 10);
            this.btnActualizar.Name = "btnActualizar";
            this.btnActualizar.Size = new System.Drawing.Size(120, 35);
            this.btnActualizar.TabIndex = 1;
            this.btnActualizar.Text = "Actualizar";
            this.btnActualizar.UseVisualStyleBackColor = false;
            this.btnActualizar.Click += new System.EventHandler(this.BtnActualizar_Click);
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.BackColor = System.Drawing.Color.FromArgb(250, 250, 252);
            this.pnlBuscar.Controls.Add(this.lblBuscar);
            this.pnlBuscar.Controls.Add(this.txtBuscar);
            this.pnlBuscar.Controls.Add(this.btnBuscar);
            this.pnlBuscar.Controls.Add(this.lblFiltroRol);
            this.pnlBuscar.Controls.Add(this.cmbFiltroRol);
            this.pnlBuscar.Controls.Add(this.lblFiltroEstado);
            this.pnlBuscar.Controls.Add(this.cmbFiltroEstado);
            this.pnlBuscar.Controls.Add(this.btnLimpiar);
            this.pnlBuscar.Controls.Add(this.lblResultados);
            this.pnlBuscar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBuscar.Location = new System.Drawing.Point(0, 105);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Padding = new System.Windows.Forms.Padding(15, 8, 15, 8);
            this.pnlBuscar.Size = new System.Drawing.Size(900, 48);
            this.pnlBuscar.TabIndex = 2;
            // 
            // lblBuscar
            // 
            this.lblBuscar.AutoSize = true;
            this.lblBuscar.ForeColor = System.Drawing.Color.FromArgb(80, 80, 90);
            this.lblBuscar.Location = new System.Drawing.Point(20, 15);
            this.lblBuscar.Name = "lblBuscar";
            this.lblBuscar.Size = new System.Drawing.Size(45, 15);
            this.lblBuscar.TabIndex = 0;
            this.lblBuscar.Text = "Buscar:";
            // 
            // txtBuscar
            // 
            this.txtBuscar.Location = new System.Drawing.Point(70, 12);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(220, 23);
            this.txtBuscar.TabIndex = 1;
            this.txtBuscar.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtBuscar_KeyDown);
            // 
            // btnBuscar
            // 
            this.btnBuscar.BackColor = System.Drawing.Color.FromArgb(212, 175, 55);
            this.btnBuscar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBuscar.FlatAppearance.BorderSize = 0;
            this.btnBuscar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBuscar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnBuscar.ForeColor = System.Drawing.Color.FromArgb(22, 22, 26);
            this.btnBuscar.Location = new System.Drawing.Point(300, 10);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(80, 27);
            this.btnBuscar.TabIndex = 2;
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.UseVisualStyleBackColor = false;
            this.btnBuscar.Click += new System.EventHandler(this.BtnBuscar_Click);
            // 
            // lblFiltroRol
            // 
            this.lblFiltroRol.AutoSize = true;
            this.lblFiltroRol.ForeColor = System.Drawing.Color.FromArgb(80, 80, 90);
            this.lblFiltroRol.Location = new System.Drawing.Point(395, 15);
            this.lblFiltroRol.Name = "lblFiltroRol";
            this.lblFiltroRol.Size = new System.Drawing.Size(26, 15);
            this.lblFiltroRol.TabIndex = 3;
            this.lblFiltroRol.Text = "Rol:";
            // 
            // cmbFiltroRol
            // 
            this.cmbFiltroRol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFiltroRol.FormattingEnabled = true;
            this.cmbFiltroRol.Location = new System.Drawing.Point(425, 12);
            this.cmbFiltroRol.Name = "cmbFiltroRol";
            this.cmbFiltroRol.Size = new System.Drawing.Size(110, 23);
            this.cmbFiltroRol.TabIndex = 4;
            this.cmbFiltroRol.SelectedIndexChanged += new System.EventHandler(this.CmbFiltro_SelectedIndexChanged);
            // 
            // lblFiltroEstado
            // 
            this.lblFiltroEstado.AutoSize = true;
            this.lblFiltroEstado.ForeColor = System.Drawing.Color.FromArgb(80, 80, 90);
            this.lblFiltroEstado.Location = new System.Drawing.Point(545, 15);
            this.lblFiltroEstado.Name = "lblFiltroEstado";
            this.lblFiltroEstado.Size = new System.Drawing.Size(45, 15);
            this.lblFiltroEstado.TabIndex = 5;
            this.lblFiltroEstado.Text = "Estado:";
            // 
            // cmbFiltroEstado
            // 
            this.cmbFiltroEstado.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFiltroEstado.FormattingEnabled = true;
            this.cmbFiltroEstado.Location = new System.Drawing.Point(595, 12);
            this.cmbFiltroEstado.Name = "cmbFiltroEstado";
            this.cmbFiltroEstado.Size = new System.Drawing.Size(90, 23);
            this.cmbFiltroEstado.TabIndex = 6;
            this.cmbFiltroEstado.SelectedIndexChanged += new System.EventHandler(this.CmbFiltro_SelectedIndexChanged);
            // 
            // btnLimpiar
            // 
            this.btnLimpiar.BackColor = System.Drawing.Color.FromArgb(150, 150, 160);
            this.btnLimpiar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLimpiar.FlatAppearance.BorderSize = 0;
            this.btnLimpiar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimpiar.ForeColor = System.Drawing.Color.White;
            this.btnLimpiar.Location = new System.Drawing.Point(695, 10);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(75, 27);
            this.btnLimpiar.TabIndex = 7;
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.UseVisualStyleBackColor = false;
            this.btnLimpiar.Click += new System.EventHandler(this.BtnLimpiar_Click);
            // 
            // lblResultados
            // 
            this.lblResultados.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblResultados.ForeColor = System.Drawing.Color.FromArgb(100, 100, 110);
            this.lblResultados.Location = new System.Drawing.Point(780, 15);
            this.lblResultados.Name = "lblResultados";
            this.lblResultados.Size = new System.Drawing.Size(105, 18);
            this.lblResultados.TabIndex = 8;
            this.lblResultados.Text = "0 resultados";
            this.lblResultados.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // dgvUsuarios
            // 
            this.dgvUsuarios.AllowUserToAddRows = false;
            this.dgvUsuarios.AllowUserToDeleteRows = false;
            this.dgvUsuarios.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvUsuarios.BackgroundColor = System.Drawing.Color.White;
            this.dgvUsuarios.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUsuarios.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colId,
            this.colNombre,
            this.colApellidos,
            this.colEmail,
            this.colRol,
            this.colEstado});
            this.dgvUsuarios.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvUsuarios.Location = new System.Drawing.Point(0, 153);
            this.dgvUsuarios.MultiSelect = false;
            this.dgvUsuarios.Name = "dgvUsuarios";
            this.dgvUsuarios.ReadOnly = true;
            this.dgvUsuarios.RowHeadersVisible = false;
            this.dgvUsuarios.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUsuarios.Size = new System.Drawing.Size(900, 347);
            this.dgvUsuarios.TabIndex = 2;
            this.dgvUsuarios.SelectionChanged += new System.EventHandler(this.DgvUsuarios_SelectionChanged);
            this.dgvUsuarios.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvUsuarios_CellDoubleClick);
            // 
            // colId
            // 
            this.colId.DataPropertyName = "Id";
            this.colId.FillWeight = 40F;
            this.colId.HeaderText = "ID";
            this.colId.Name = "colId";
            this.colId.ReadOnly = true;
            this.colId.Width = 50;
            // 
            // colNombre
            // 
            this.colNombre.FillWeight = 120F;
            this.colNombre.HeaderText = "Nombre";
            this.colNombre.Name = "colNombre";
            this.colNombre.ReadOnly = true;
            // 
            // colApellidos
            // 
            this.colApellidos.FillWeight = 120F;
            this.colApellidos.HeaderText = "Apellidos";
            this.colApellidos.Name = "colApellidos";
            this.colApellidos.ReadOnly = true;
            // 
            // colEmail
            // 
            this.colEmail.FillWeight = 180F;
            this.colEmail.HeaderText = "Email";
            this.colEmail.Name = "colEmail";
            this.colEmail.ReadOnly = true;
            // 
            // colRol
            // 
            this.colRol.FillWeight = 100F;
            this.colRol.HeaderText = "Rol";
            this.colRol.Name = "colRol";
            this.colRol.ReadOnly = true;
            // 
            // colEstado
            // 
            this.colEstado.FillWeight = 80F;
            this.colEstado.HeaderText = "Estado";
            this.colEstado.Name = "colEstado";
            this.colEstado.ReadOnly = true;
            // 
            // FrmGestionUsuarios
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(240, 242, 245);
            this.ClientSize = new System.Drawing.Size(900, 500);
            this.Controls.Add(this.dgvUsuarios);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.pnlBotones);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(700, 400);
            this.Name = "FrmGestionUsuarios";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Nexum Bank - Gestión de usuarios";
            this.Load += new System.EventHandler(this.FrmGestionUsuarios_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlBotones.ResumeLayout(false);
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Button btnCerrar;
        private System.Windows.Forms.Panel pnlBotones;
        private System.Windows.Forms.Button btnNuevo;
        private System.Windows.Forms.Button btnEditar;
        private System.Windows.Forms.Button btnBanear;
        private System.Windows.Forms.Button btnDesbanear;
        private System.Windows.Forms.Button btnActualizar;
        private System.Windows.Forms.Panel pnlBuscar;
        private System.Windows.Forms.Label lblBuscar;
        private System.Windows.Forms.TextBox txtBuscar;
        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.Label lblFiltroRol;
        private System.Windows.Forms.ComboBox cmbFiltroRol;
        private System.Windows.Forms.Label lblFiltroEstado;
        private System.Windows.Forms.ComboBox cmbFiltroEstado;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Label lblResultados;
        private System.Windows.Forms.DataGridView dgvUsuarios;
        private System.Windows.Forms.DataGridViewTextBoxColumn colId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNombre;
        private System.Windows.Forms.DataGridViewTextBoxColumn colApellidos;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEmail;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRol;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEstado;
    }
}
