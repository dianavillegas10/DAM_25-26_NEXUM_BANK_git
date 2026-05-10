// Forms/Admin/FrmAdminDashboard.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NexumApp.Models;

namespace NexumApp.Forms.Admin
{
    public class FrmAdminDashboard : Form
    {
        private Panel pnlHeader;
        private Panel pnlContent;
        private Button btnGestionUsuarios;
        private Button btnEditarUsuarios;
        private Button btnGestionTarjetas;
        private Button btnGestionTransferencias;
        private Button btnGestionTickets;
        private Button btnCancelarCuentas;
        private Button btnCerrarSesion;
        private Label lblUser;
        private Label lblTitle;
        private ListBox lstToDo;
        private TextBox txtNuevaTarea;
        private Button btnAgregarTarea;
        private Button btnCompletarTarea;
        private Panel pnlGrafico;
        private string archivoToDo = Path.Combine(Application.StartupPath, "Data", "todolist_admin.txt");

        public FrmAdminDashboard()
        {
            this.Text = "Nexum Bank - Panel de Administración";
            this.Size = new Size(1300, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.MinimumSize = new Size(1100, 750);
            this.WindowState = FormWindowState.Maximized;
            this.Icon = null;

            ConfigurarHeader();
            ConfigurarContent();
            CargarUsuario();
            CargarToDoList();
        }

        private void ConfigurarHeader()
        {
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(30, 0, 30, 0)
            };

            var lblLogo = new Label
            {
                Text = "NEXUM BANK",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(99, 102, 241),
                Location = new Point(30, 18),
                AutoSize = true
            };

            lblUser = new Label
            {
                Text = "Cargando...",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true
            };

            btnCerrarSesion = new Button
            {
                Text = "Salir",
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(239, 68, 68),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(80, 35),
                Cursor = Cursors.Hand,
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnCerrarSesion.FlatAppearance.BorderSize = 1;
            btnCerrarSesion.FlatAppearance.BorderColor = Color.FromArgb(230, 230, 235);
            btnCerrarSesion.Click += BtnCerrarSesion_Click;

            pnlHeader.Controls.Add(btnCerrarSesion);
            pnlHeader.Controls.Add(lblUser);
            pnlHeader.Controls.Add(lblLogo);
            this.Controls.Add(pnlHeader);

            this.Resize += (s, e) =>
            {
                if (lblUser != null) lblUser.Location = new Point(this.Width - 200, 25);
                if (btnCerrarSesion != null) btnCerrarSesion.Location = new Point(this.Width - 100, 18);
            };
        }

        private void ConfigurarContent()
        {
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(40, 20, 40, 30),
                AutoScroll = true
            };

            // Título centrado
            lblTitle = new Label
            {
                Text = "Panel de Administración",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60
            };

            // ========== PANEL SUPERIOR (2 COLUMNAS) ==========
            var pnlSuperior = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 350,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 0, 20)
            };
            pnlSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            // Columna izquierda: Gráfico de actividad
            pnlGrafico = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 15, 0)
            };
            pnlGrafico.Paint += DibujarGrafico;

            var lblGraficoTitulo = new Label
            {
                Text = "📊 Actividad del Sistema (últimos 7 días)",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(15, 5, 0, 0)
            };
            pnlGrafico.Controls.Add(lblGraficoTitulo);
            pnlSuperior.Controls.Add(pnlGrafico, 0, 0);

            // Columna derecha: To-Do List
            var pnlToDo = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(15, 0, 0, 0)
            };

            var lblToDoTitulo = new Label
            {
                Text = "📋 Tareas Pendientes (Compartido entre Admins)",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(15, 5, 0, 0)
            };
            pnlToDo.Controls.Add(lblToDoTitulo);

            lstToDo = new ListBox
            {
                Location = new Point(15, 40),
                Size = new Size(pnlToDo.Width - 30, 180),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.FixedSingle
            };
            lstToDo.DoubleClick += (s, e) => CompletarTarea();

            txtNuevaTarea = new TextBox
            {
                Location = new Point(15, 230),
                Size = new Size(pnlToDo.Width - 110, 30),
                Font = new Font("Segoe UI", 10)
            };
            // Agregar texto de ayuda (placeholder manual)
            txtNuevaTarea.Text = "Nueva tarea...";
            txtNuevaTarea.ForeColor = Color.Gray;
            txtNuevaTarea.Enter += (s, e) =>
            {
                if (txtNuevaTarea.Text == "Nueva tarea...")
                {
                    txtNuevaTarea.Text = "";
                    txtNuevaTarea.ForeColor = SystemColors.WindowText;
                }
            };
            txtNuevaTarea.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtNuevaTarea.Text))
                {
                    txtNuevaTarea.Text = "Nueva tarea...";
                    txtNuevaTarea.ForeColor = Color.Gray;
                }
            };

            btnAgregarTarea = new Button
            {
                Text = "Agregar",
                Location = new Point(pnlToDo.Width - 90, 228),
                Size = new Size(80, 32),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAgregarTarea.Click += AgregarTarea;

            btnCompletarTarea = new Button
            {
                Text = "✓ Completar",
                Location = new Point(15, 270),
                Size = new Size(100, 32),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCompletarTarea.Click += (s, e) => CompletarTarea();

            pnlToDo.Controls.Add(btnCompletarTarea);
            pnlToDo.Controls.Add(btnAgregarTarea);
            pnlToDo.Controls.Add(txtNuevaTarea);
            pnlToDo.Controls.Add(lstToDo);
            pnlSuperior.Controls.Add(pnlToDo, 1, 0);

            pnlContent.Controls.Add(pnlSuperior);

            // ========== BOTONES DE ACCIÓN (GRID 3x2) ==========
            var pnlBotones = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 320,
                ColumnCount = 3,
                RowCount = 2,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 0)
            };
            pnlBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            pnlBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            pnlBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            pnlBotones.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            pnlBotones.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            var botones = new (string texto, string descripcion, Color color, Action accion, int col, int row)[]
            {
                ("Gestión de Usuarios", "Ver y administrar usuarios", Color.FromArgb(99, 102, 241), () => AbrirGestionUsuarios(), 0, 0),
                ("Editar Usuarios", "Modificar datos de usuarios", Color.FromArgb(59, 130, 246), () => AbrirEditarUsuarios(), 1, 0),
                ("Gestión de Tarjetas", "Administrar tarjetas bancarias", Color.FromArgb(139, 92, 246), () => AbrirGestionTarjetas(), 2, 0),
                ("Gestión de Transferencias", "Supervisar transferencias", Color.FromArgb(16, 185, 129), () => AbrirGestionTransferencias(), 0, 1),
                ("Gestión de Tickets", "Responder tickets de soporte", Color.FromArgb(239, 68, 68), () => AbrirGestionTickets(), 1, 1),
                ("Cancelar Cuentas", "Cancelar cuentas permanentemente", Color.FromArgb(245, 158, 11), () => AbrirCancelarCuentas(), 2, 1)
            };

            foreach (var b in botones)
            {
                var btn = CrearBotonGrande(b.texto, b.descripcion, b.color);
                btn.Click += (s, e) => b.accion();
                pnlBotones.Controls.Add(btn, b.col, b.row);
            }

            pnlContent.Controls.Add(pnlBotones);
            pnlContent.Controls.Add(lblTitle);
            this.Controls.Add(pnlContent);

            // Ajustar tamaños al redimensionar
            pnlContent.Resize += (s, e) =>
            {
                if (pnlGrafico != null) pnlGrafico.Invalidate();
                if (lstToDo != null && pnlToDo != null)
                {
                    lstToDo.Width = pnlToDo.Width - 30;
                    txtNuevaTarea.Width = pnlToDo.Width - 110;
                    btnAgregarTarea.Location = new Point(pnlToDo.Width - 90, 228);
                }
            };
        }

        private Button CrearBotonGrande(string texto, string descripcion, Color color)
        {
            var btn = new Button
            {
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                Margin = new Padding(10)
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(230, 230, 235);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(250, 250, 255);

            // Borde de color izquierdo
            var lineColor = new Panel
            {
                Size = new Size(6, 80),
                Location = new Point(0, 0),
                BackColor = color
            };

            var lblTitulo = new Label
            {
                Text = texto,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(25, 20),
                AutoSize = true
            };

            var lblDesc = new Label
            {
                Text = descripcion,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(25, 48),
                Size = new Size(200, 30)
            };

            var lblFlecha = new Label
            {
                Text = "→",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(btn.Width - 40, 35),
                AutoSize = true,
                Anchor = AnchorStyles.Right
            };

            btn.Controls.Add(lineColor);
            btn.Controls.Add(lblTitulo);
            btn.Controls.Add(lblDesc);
            btn.Controls.Add(lblFlecha);

            return btn;
        }

        private void DibujarGrafico(object sender, PaintEventArgs e)
        {
            var pnl = (Panel)sender;
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int ancho = pnl.Width - 60;
            int alto = pnl.Height - 80;
            int inicioX = 40;
            int inicioY = pnl.Height - 60;

            // Datos simulados de actividad (últimos 7 días)
            int[] datos = { 45, 62, 38, 71, 55, 68, 82 };
            string[] dias = { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
            int maxDato = datos.Max();

            // Dibujar barras
            int anchoBarra = (ancho - (datos.Length - 1) * 10) / datos.Length;
            for (int i = 0; i < datos.Length; i++)
            {
                int alturaBarra = (int)((double)datos[i] / maxDato * (alto - 30));
                int x = inicioX + i * (anchoBarra + 10);
                int y = inicioY - alturaBarra;

                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(x, y, anchoBarra, alturaBarra),
                    Color.FromArgb(99, 102, 241), Color.FromArgb(139, 92, 246),
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, x, y, anchoBarra, alturaBarra);
                }

                // Valor encima de la barra
                using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(51, 65, 85)))
                {
                    g.DrawString(datos[i].ToString(), font, brush, x + anchoBarra / 3, y - 18);
                }

                // Día debajo de la barra
                using (var font = new Font("Segoe UI", 9))
                using (var brush = new SolidBrush(Color.FromArgb(100, 116, 139)))
                {
                    g.DrawString(dias[i], font, brush, x + anchoBarra / 4, inicioY + 5);
                }
            }

            // Líneas de guía
            using (var pen = new Pen(Color.FromArgb(230, 230, 235), 1))
            {
                for (int i = 0; i <= 4; i++)
                {
                    int yLinea = inicioY - (i * alto / 4);
                    g.DrawLine(pen, inicioX - 5, yLinea, inicioX + ancho, yLinea);
                }
            }
        }

        private void CargarUsuario()
        {
            if (SesionActual.Instancia.EstaLogeado && SesionActual.Instancia.Usuario != null)
            {
                lblUser.Text = $"👤 {SesionActual.Instancia.Usuario.Nombre}";
            }
            else
            {
                lblUser.Text = "👤 Administrador";
            }
        }

        // ==================== TO-DO LIST ====================
        private void CargarToDoList()
        {
            try
            {
                string directorio = Path.GetDirectoryName(archivoToDo);
                if (!Directory.Exists(directorio)) Directory.CreateDirectory(directorio);

                if (File.Exists(archivoToDo))
                {
                    var tareas = File.ReadAllLines(archivoToDo);
                    lstToDo.Items.Clear();
                    foreach (var tarea in tareas)
                    {
                        if (!string.IsNullOrWhiteSpace(tarea))
                            lstToDo.Items.Add(tarea);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando To-Do: {ex.Message}");
            }
        }

        private void GuardarToDoList()
        {
            try
            {
                var tareas = lstToDo.Items.Cast<string>().ToArray();
                File.WriteAllLines(archivoToDo, tareas);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando To-Do: {ex.Message}");
            }
        }

        private void AgregarTarea(object sender, EventArgs e)
        {
            string nuevaTarea = txtNuevaTarea.Text.Trim();
            if (!string.IsNullOrWhiteSpace(nuevaTarea) && nuevaTarea != "Nueva tarea...")
            {
                lstToDo.Items.Add(nuevaTarea);
                GuardarToDoList();
                txtNuevaTarea.Text = "Nueva tarea...";
                txtNuevaTarea.ForeColor = Color.Gray;
                txtNuevaTarea.Focus();
            }
        }

        private void CompletarTarea()
        {
            if (lstToDo.SelectedItem != null)
            {
                lstToDo.Items.Remove(lstToDo.SelectedItem);
                GuardarToDoList();
            }
            else
            {
                MessageBox.Show("Selecciona una tarea para marcar como completada.", "Tareas",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnCerrarSesion_Click(object sender, EventArgs e)
        {
            var resultado = MessageBox.Show("¿Estás seguro que deseas cerrar sesión?", "Cerrar Sesión",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                SesionActual.Instancia.CerrarSesion();
                this.Hide();
                var login = new Auth.FrmLogin();
                login.FormClosed += (s, args) => this.Close();
                login.Show();
            }
        }

        // ==================== MÉTODOS PARA ABRIR FORMULARIOS ====================

        private void AbrirGestionUsuarios()
        {
            using (var frm = new FrmGestionUsuarios())
                frm.ShowDialog(this);
        }

        private void AbrirEditarUsuarios()
        {
            using (var frm = new FrmEditarUsuario())
                frm.ShowDialog(this);
        }

        private void AbrirGestionTarjetas()
        {
            using (var frm = new FrmGestionTarjetas())
                frm.ShowDialog(this);
        }

        private void AbrirGestionTransferencias()
        {
            using (var frm = new FrmGestionTransferencias())
                frm.ShowDialog(this);
        }

        private void AbrirGestionTickets()
        {
            using (var frm = new FrmGestionTickets())
                frm.ShowDialog(this);
        }

        private void AbrirCancelarCuentas()
        {
            var resultado = MessageBox.Show(
                "⚠️ ¿Estás seguro de que deseas acceder a la gestión de cancelación de cuentas?\n\n" +
                "Esta acción permite desactivar cuentas de usuario de forma permanente.",
                "Cancelar Cuentas",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (resultado == DialogResult.Yes)
            {
                using (var frm = new FrmCancelarCuentas())
                    frm.ShowDialog(this);
            }
        }
    }
}