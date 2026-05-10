// Forms/Admin/FrmReportesAdmin.cs
using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace NexumApp.Forms.Admin
{
    public class FrmReportesAdmin : Form
    {
        private Panel pnlHeader;
        private Panel pnlSidebar;
        private Panel pnlContent;
        private Panel pnlEstadisticas;
        private Label lblTitulo;
        private Button btnCerrar;
        private Button btnClose;
        private List<Ticket> _tickets;
        private AdminTicketService _ticketService;
        private Button btnVolver;

        // Controles para mostrar datos
        private DataGridView dgvTickets;
        private Label lblDetalleTitulo;
        private RichTextBox txtDetalle;
        private ComboBox cmbFiltroEstado;
        private ComboBox cmbFiltroPrioridad;
        private Button btnFiltrar;
        private Button btnLimpiarFiltros;
        private Label lblResultados;
        private Panel pnlGrafico;
        private Panel pnlAcciones;
        private Button btnResponderTicket;
        private Button btnVerDetalle;
        private Button btnExportar;

        public FrmReportesAdmin()
        {
            InitializeComponent();
            _ticketService = new AdminTicketService();
            ConfigurarFormulario();
            this.Shown += async (s, e) => await CargarDatosAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "🎫 Centro de Gestión de Incidencias";
            this.Size = new Size(1300, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.WindowState = FormWindowState.Maximized;
        }

        private void ConfigurarFormulario()
        {
            // ========== HEADER ==========
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(18, 22, 30)
            };

            lblTitulo = new Label
            {
                Text = "🎫 Centro de Gestión de Incidencias y Tickets",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 22),
                AutoSize = true
            };

            btnClose = new Button
            {
                Text = "✗ Cerrar",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 60, 70),
                Size = new Size(90, 35),
                Location = new Point(this.Width - 120, 18),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatAppearance = { BorderSize = 0 }
            };
            btnClose.Click += (s, e) => this.Close();

            pnlHeader.Controls.Add(lblTitulo);
            pnlHeader.Controls.Add(btnClose);

            // ========== SIDEBAR ==========
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 260,
                BackColor = Color.FromArgb(26, 26, 46)
            };

            // Logo en sidebar
            var lblSidebarTitulo = new Label
            {
                Text = "NEXUM BANK",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(212, 175, 55),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(18, 22, 30)
            };
            pnlSidebar.Controls.Add(lblSidebarTitulo);

            // Botones del menú
            var btnDashboard = CrearBotonSidebar("📊 Dashboard General", 70);
            var btnPendientes = CrearBotonSidebar("⏳ Tickets Pendientes", 120);
            var btnUrgentes = CrearBotonSidebar("⚡ Tickets Urgentes", 170);
            var btnResueltos = CrearBotonSidebar("✅ Tickets Resueltos", 220);
            var btnRanking = CrearBotonSidebar("👥 Ranking Usuarios", 270);
            var btnEvolucion = CrearBotonSidebar("📈 Evolución", 320);
            var btnTiempo = CrearBotonSidebar("⏱️ Tiempo Respuesta", 370);
            var btnExportar = CrearBotonSidebar("📎 Exportar Reporte", 420);

            btnDashboard.Click += (s, e) => MostrarDashboard();
            btnPendientes.Click += (s, e) => FiltrarPorEstado("Pendiente");
            btnUrgentes.Click += (s, e) => FiltrarPorPrioridad(new[] { "Urgente", "Alta" });
            btnResueltos.Click += (s, e) => FiltrarPorEstado("Resuelto");
            btnRanking.Click += (s, e) => MostrarRankingUsuarios();
            btnEvolucion.Click += (s, e) => MostrarEvolucionTickets();
            btnTiempo.Click += (s, e) => MostrarTiempoRespuesta();
            btnExportar.Click += (s, e) => ExportarReporte();

            // Botón volver al dashboard
            btnVolver = new Button
            {
                Text = "← Volver al Dashboard",
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(99, 102, 241),
                Dock = DockStyle.Bottom,
                Height = 45,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
            };
            btnVolver.Click += (s, e) => this.Close();
            pnlSidebar.Controls.Add(btnVolver);

            // ========== CONTENIDO PRINCIPAL ==========
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(240, 242, 245),
                AutoScroll = true
            };

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(pnlHeader);

            // Mostrar dashboard inicial
            MostrarDashboard();
        }

        private Button CrearBotonSidebar(string texto, int y)
        {
            var btn = new Button
            {
                Text = texto,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Size = new Size(260, 45),
                Location = new Point(0, y),
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent,
                FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(60, 60, 70) }
            };
            pnlSidebar.Controls.Add(btn);
            return btn;
        }

        private void MostrarDashboard()
        {
            pnlContent.Controls.Clear();

            // Panel de estadísticas
            var pnlStats = new Panel
            {
                Dock = DockStyle.Top,
                Height = 140,
                Margin = new Padding(0, 0, 0, 20)
            };

            var layoutStats = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            layoutStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            layoutStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            layoutStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            layoutStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            layoutStats.Controls.Add(CrearTarjetaEstadistica("🎫", "TOTAL TICKETS", "0", Color.FromArgb(99, 102, 241)), 0, 0);
            layoutStats.Controls.Add(CrearTarjetaEstadistica("⏳", "PENDIENTES", "0", Color.FromArgb(245, 158, 11)), 1, 0);
            layoutStats.Controls.Add(CrearTarjetaEstadistica("⚡", "URGENTES", "0", Color.FromArgb(239, 68, 68)), 2, 0);
            layoutStats.Controls.Add(CrearTarjetaEstadistica("✅", "RESUELTOS", "0", Color.FromArgb(16, 185, 129)), 3, 0);

            pnlStats.Controls.Add(layoutStats);
            pnlContent.Controls.Add(pnlStats);

            // Panel de filtros
            var pnlFiltros = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 20)
            };
            pnlFiltros.Paint += (s, e) => DibujarBordeBlanco(s, e);

            var lblFiltroEstado = new Label
            {
                Text = "Estado:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(15, 18),
                AutoSize = true
            };

            cmbFiltroEstado = new ComboBox
            {
                Location = new Point(70, 15),
                Size = new Size(130, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbFiltroEstado.Items.AddRange(new[] { "Todos", "Pendiente", "En proceso", "Resuelto", "Cerrado" });
            cmbFiltroEstado.SelectedIndex = 0;

            var lblFiltroPrioridad = new Label
            {
                Text = "Prioridad:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(220, 18),
                AutoSize = true
            };

            cmbFiltroPrioridad = new ComboBox
            {
                Location = new Point(290, 15),
                Size = new Size(130, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbFiltroPrioridad.Items.AddRange(new[] { "Todos", "Baja", "Media", "Alta", "Urgente" });
            cmbFiltroPrioridad.SelectedIndex = 0;

            btnFiltrar = new Button
            {
                Text = "🔍 Aplicar Filtros",
                Location = new Point(440, 12),
                Size = new Size(120, 32),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnFiltrar.FlatAppearance.BorderSize = 0;
            btnFiltrar.Click += async (s, e) => await AplicarFiltros();

            btnLimpiarFiltros = new Button
            {
                Text = "🗑️ Limpiar",
                Location = new Point(570, 12),
                Size = new Size(90, 32),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnLimpiarFiltros.FlatAppearance.BorderSize = 0;
            btnLimpiarFiltros.Click += async (s, e) => { cmbFiltroEstado.SelectedIndex = 0; cmbFiltroPrioridad.SelectedIndex = 0; await AplicarFiltros(); };

            lblResultados = new Label
            {
                Text = "Mostrando todos los tickets",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(680, 20),
                AutoSize = true
            };

            pnlFiltros.Controls.Add(lblFiltroEstado);
            pnlFiltros.Controls.Add(cmbFiltroEstado);
            pnlFiltros.Controls.Add(lblFiltroPrioridad);
            pnlFiltros.Controls.Add(cmbFiltroPrioridad);
            pnlFiltros.Controls.Add(btnFiltrar);
            pnlFiltros.Controls.Add(btnLimpiarFiltros);
            pnlFiltros.Controls.Add(lblResultados);
            pnlContent.Controls.Add(pnlFiltros);

            // DataGridView para tickets
            dgvTickets = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9),
                AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(248, 250, 252) }
            };

            dgvTickets.Columns.Add("Id", "ID");
            dgvTickets.Columns.Add("Usuario", "Usuario");
            dgvTickets.Columns.Add("Asunto", "Asunto");
            dgvTickets.Columns.Add("Estado", "Estado");
            dgvTickets.Columns.Add("Prioridad", "Prioridad");
            dgvTickets.Columns.Add("Fecha", "Fecha Creación");
            dgvTickets.Columns.Add("Respuesta", "Respuesta");

            dgvTickets.Columns["Id"].Width = 50;
            dgvTickets.Columns["Usuario"].Width = 120;
            dgvTickets.Columns["Asunto"].Width = 200;
            dgvTickets.Columns["Estado"].Width = 80;
            dgvTickets.Columns["Prioridad"].Width = 70;
            dgvTickets.Columns["Fecha"].Width = 100;
            dgvTickets.Columns["Respuesta"].Width = 150;

            dgvTickets.CellFormatting += DgvTickets_CellFormatting;
            dgvTickets.CellDoubleClick += DgvTickets_CellDoubleClick;

            pnlContent.Controls.Add(dgvTickets);

            // Panel de acciones
            var pnlAcciones = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.White,
                Margin = new Padding(0, 10, 0, 0)
            };
            pnlAcciones.Paint += (s, e) => DibujarBordeBlanco(s, e);

            btnResponderTicket = new Button
            {
                Text = "✏️ Responder Ticket",
                Location = new Point(15, 12),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnResponderTicket.FlatAppearance.BorderSize = 0;
            btnResponderTicket.Click += BtnResponderTicket_Click;

            btnVerDetalle = new Button
            {
                Text = "👁️ Ver Detalle",
                Location = new Point(165, 12),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnVerDetalle.FlatAppearance.BorderSize = 0;
            btnVerDetalle.Click += BtnVerDetalle_Click;

            btnExportar = new Button
            {
                Text = "📎 Exportar a Excel",
                Location = new Point(pnlAcciones.Width - 130, 12),
                Size = new Size(110, 35),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnExportar.FlatAppearance.BorderSize = 0;
            btnExportar.Click += (s, e) => ExportarReporte();

            pnlAcciones.Controls.Add(btnResponderTicket);
            pnlAcciones.Controls.Add(btnVerDetalle);
            pnlAcciones.Controls.Add(btnExportar);
            pnlContent.Controls.Add(pnlAcciones);

            // Cargar datos
            _ = AplicarFiltros();
        }

        private Panel CrearTarjetaEstadistica(string icono, string titulo, string valor, Color color)
        {
            var card = new Panel
            {
                BackColor = Color.White,
                Margin = new Padding(10),
                Height = 110
            };
            card.Paint += (s, e) =>
            {
                using (var path = CrearRoundedRect(card.ClientRectangle, 12))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(new SolidBrush(Color.White), path);
                    using (var pen = new Pen(Color.FromArgb(230, 230, 235), 1))
                        e.Graphics.DrawPath(pen, path);
                }
            };

            var lblIcono = new Label
            {
                Text = icono,
                Font = new Font("Segoe UI", 32),
                Location = new Point(20, 15),
                AutoSize = true
            };

            var lblTitulo = new Label
            {
                Text = titulo,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(20, 60),
                AutoSize = true
            };

            var lblValor = new Label
            {
                Name = $"lblValor_{titulo.Replace(" ", "")}",
                Text = valor,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(20, 78),
                AutoSize = true
            };

            card.Controls.Add(lblIcono);
            card.Controls.Add(lblTitulo);
            card.Controls.Add(lblValor);

            return card;
        }

        private void DibujarBordeBlanco(object sender, PaintEventArgs e)
        {
            var pnl = (Panel)sender;
            using (var pen = new Pen(Color.FromArgb(230, 230, 235), 1))
                e.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
        }

        private GraphicsPath CrearRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        private async System.Threading.Tasks.Task CargarDatosAsync()
        {
            try
            {
                _tickets = await _ticketService.ObtenerTodosLosTicketsAsync();
                await ActualizarEstadisticas();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task ActualizarEstadisticas()
        {
            if (_tickets == null) return;

            int total = _tickets.Count;
            int pendientes = _tickets.Count(t => t.Estado == "Pendiente" || t.Estado == "En proceso");
            int urgentes = _tickets.Count(t => t.Prioridad == "Urgente" || t.Prioridad == "Alta");
            int resueltos = _tickets.Count(t => t.Estado == "Resuelto" || t.Estado == "Cerrado");

            ActualizarValorEstadistica("TOTAL TICKETS", total.ToString());
            ActualizarValorEstadistica("PENDIENTES", pendientes.ToString());
            ActualizarValorEstadistica("URGENTES", urgentes.ToString());
            ActualizarValorEstadistica("RESUELTOS", resueltos.ToString());
        }

        private void ActualizarValorEstadistica(string nombre, string valor)
        {
            var control = pnlContent.Controls.Find($"lblValor_{nombre}", true).FirstOrDefault();
            if (control != null) control.Text = valor;
        }

        private async System.Threading.Tasks.Task AplicarFiltros()
        {
            if (_tickets == null) return;

            var filtrados = _tickets.AsEnumerable();

            string estado = cmbFiltroEstado?.SelectedItem?.ToString() ?? "Todos";
            if (estado != "Todos")
                filtrados = filtrados.Where(t => t.Estado == estado);

            string prioridad = cmbFiltroPrioridad?.SelectedItem?.ToString() ?? "Todos";
            if (prioridad != "Todos")
                filtrados = filtrados.Where(t => t.Prioridad == prioridad);

            var lista = filtrados.ToList();
            lblResultados.Text = $"Mostrando {lista.Count} de {_tickets.Count} tickets";

            dgvTickets.Rows.Clear();
            foreach (var ticket in lista.OrderByDescending(t => t.FechaCreacion))
            {
                dgvTickets.Rows.Add(
                    ticket.Id,
                    ticket.UsuarioNombre,
                    ticket.Asunto,
                    ticket.Estado ?? "Pendiente",
                    ticket.Prioridad ?? "Media",
                    ticket.FechaCreacion.ToString("dd/MM/yyyy HH:mm"),
                    string.IsNullOrEmpty(ticket.RespuestaAdmin) ? "Sin responder" : "Respondido"
                );
            }
        }

        private void FiltrarPorEstado(string estado)
        {
            cmbFiltroEstado.SelectedItem = estado;
            _ = AplicarFiltros();
        }

        private void FiltrarPorPrioridad(string[] prioridades)
        {
            cmbFiltroPrioridad.SelectedIndex = 0;
            if (_tickets == null) return;

            var filtrados = _tickets.Where(t => prioridades.Contains(t.Prioridad)).ToList();
            lblResultados.Text = $"Mostrando {filtrados.Count} de {_tickets.Count} tickets (Prioridad: Urgente/Alta)";

            dgvTickets.Rows.Clear();
            foreach (var ticket in filtrados.OrderByDescending(t => t.FechaCreacion))
            {
                dgvTickets.Rows.Add(
                    ticket.Id,
                    ticket.UsuarioNombre,
                    ticket.Asunto,
                    ticket.Estado ?? "Pendiente",
                    ticket.Prioridad ?? "Media",
                    ticket.FechaCreacion.ToString("dd/MM/yyyy HH:mm"),
                    string.IsNullOrEmpty(ticket.RespuestaAdmin) ? "Sin responder" : "Respondido"
                );
            }
        }

        private void DgvTickets_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 3 && e.Value != null)
            {
                string estado = e.Value.ToString();
                switch (estado)
                {
                    case "Pendiente": e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11); e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold); break;
                    case "En proceso": e.CellStyle.ForeColor = Color.FromArgb(59, 130, 246); e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold); break;
                    case "Resuelto": e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129); break;
                    case "Cerrado": e.CellStyle.ForeColor = Color.FromArgb(107, 114, 128); break;
                }
            }

            if (e.ColumnIndex == 4 && e.Value != null)
            {
                string prioridad = e.Value.ToString();
                switch (prioridad)
                {
                    case "Urgente": e.CellStyle.ForeColor = Color.FromArgb(239, 68, 68); e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold); break;
                    case "Alta": e.CellStyle.ForeColor = Color.FromArgb(249, 115, 22); break;
                    case "Media": e.CellStyle.ForeColor = Color.FromArgb(234, 179, 8); break;
                    case "Baja": e.CellStyle.ForeColor = Color.FromArgb(34, 197, 94); break;
                }
            }
        }

        private void DgvTickets_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int ticketId = Convert.ToInt32(dgvTickets.Rows[e.RowIndex].Cells[0].Value);
                var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
                if (ticket != null)
                    AbrirDetalleTicket(ticket);
            }
        }

        private void BtnResponderTicket_Click(object sender, EventArgs e)
        {
            if (dgvTickets.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona un ticket para responder.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int ticketId = Convert.ToInt32(dgvTickets.SelectedRows[0].Cells[0].Value);
            var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket != null)
                AbrirDetalleTicket(ticket);
        }

        private void BtnVerDetalle_Click(object sender, EventArgs e)
        {
            if (dgvTickets.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona un ticket para ver detalles.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int ticketId = Convert.ToInt32(dgvTickets.SelectedRows[0].Cells[0].Value);
            var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket != null)
                AbrirDetalleTicket(ticket);
        }

        private void AbrirDetalleTicket(Ticket ticket)
        {
            var frmDetalle = new Form
            {
                Text = $"Detalle del Ticket #{ticket.Id}",
                Size = new Size(600, 500),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblUsuario = new Label
            {
                Text = $"👤 Usuario: {ticket.UsuarioNombre}",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            var lblAsunto = new Label
            {
                Text = $"📌 Asunto: {ticket.Asunto}",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 60),
                AutoSize = true
            };

            var lblEstado = new Label
            {
                Text = $"📊 Estado: {ticket.Estado} | Prioridad: {ticket.Prioridad}",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 95),
                AutoSize = true
            };

            var lblFecha = new Label
            {
                Text = $"📅 Fecha: {ticket.FechaCreacion:dd/MM/yyyy HH:mm}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(20, 125),
                AutoSize = true
            };

            var lblDescTitulo = new Label
            {
                Text = "Descripción del problema:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 160),
                AutoSize = true
            };

            var txtDescripcion = new RichTextBox
            {
                Location = new Point(20, 185),
                Size = new Size(540, 100),
                ReadOnly = true,
                Text = ticket.Descripcion,
                BackColor = Color.FromArgb(248, 250, 252)
            };

            var lblRespuestaTitulo = new Label
            {
                Text = "Tu respuesta:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 300),
                AutoSize = true
            };

            var txtRespuesta = new RichTextBox
            {
                Location = new Point(20, 325),
                Size = new Size(540, 80),
                Text = ticket.RespuestaAdmin ?? ""
            };

            var cmbNuevoEstado = new ComboBox
            {
                Location = new Point(20, 420),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbNuevoEstado.Items.AddRange(new[] { "Pendiente", "En proceso", "Resuelto", "Cerrado" });
            cmbNuevoEstado.SelectedItem = ticket.Estado ?? "Pendiente";

            var btnGuardar = new Button
            {
                Text = "✓ Guardar respuesta",
                Location = new Point(190, 418),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += async (s, ev) =>
            {
                ticket.RespuestaAdmin = txtRespuesta.Text;
                ticket.Estado = cmbNuevoEstado.SelectedItem?.ToString() ?? "Pendiente";
                ticket.AdminId = SesionActual.Instancia?.Usuario?.Id;
                ticket.AdminNombre = SesionActual.Instancia?.Usuario?.Nombre;
                ticket.FechaActualizacion = DateTime.Now;

                if (await _ticketService.ActualizarTicketAsync(ticket))
                {
                    MessageBox.Show("Ticket actualizado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    frmDetalle.Close();
                    await CargarDatosAsync();
                    await AplicarFiltros();
                }
                else
                {
                    MessageBox.Show("Error al actualizar el ticket", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            var btnCerrar = new Button
            {
                Text = "✗ Cerrar",
                Location = new Point(330, 418),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCerrar.Click += (s, ev) => frmDetalle.Close();

            frmDetalle.Controls.Add(lblUsuario);
            frmDetalle.Controls.Add(lblAsunto);
            frmDetalle.Controls.Add(lblEstado);
            frmDetalle.Controls.Add(lblFecha);
            frmDetalle.Controls.Add(lblDescTitulo);
            frmDetalle.Controls.Add(txtDescripcion);
            frmDetalle.Controls.Add(lblRespuestaTitulo);
            frmDetalle.Controls.Add(txtRespuesta);
            frmDetalle.Controls.Add(cmbNuevoEstado);
            frmDetalle.Controls.Add(btnGuardar);
            frmDetalle.Controls.Add(btnCerrar);

            frmDetalle.ShowDialog(this);
        }

        private void MostrarRankingUsuarios()
        {
            pnlContent.Controls.Clear();

            var lblTitle = new Label
            {
                Text = "👥 Ranking de usuarios con más incidencias",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(10, 10, 0, 0)
            };
            pnlContent.Controls.Add(lblTitle);

            var dgvRanking = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 11)
            };

            dgvRanking.Columns.Add("Puesto", "Puesto");
            dgvRanking.Columns.Add("Usuario", "Usuario");
            dgvRanking.Columns.Add("Total", "Total Tickets");

            var ranking = _tickets.GroupBy(t => t.UsuarioNombre)
                .Select(g => new { Usuario = g.Key, Total = g.Count() })
                .OrderByDescending(g => g.Total)
                .ToList();

            int puesto = 1;
            foreach (var item in ranking)
            {
                dgvRanking.Rows.Add(puesto++, item.Usuario, item.Total);
            }

            pnlContent.Controls.Add(dgvRanking);

            var btnVolverDashboard = new Button
            {
                Text = "← Volver al Dashboard",
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnVolverDashboard.Click += (s, e) => MostrarDashboard();
            pnlContent.Controls.Add(btnVolverDashboard);
        }

        private void MostrarEvolucionTickets()
        {
            pnlContent.Controls.Clear();

            var lblTitle = new Label
            {
                Text = "📈 Evolución de tickets (últimos 30 días)",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(10, 10, 0, 0)
            };
            pnlContent.Controls.Add(lblTitle);

            var pnlGrafico = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            var lblGrafico = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10),
                TextAlign = ContentAlignment.MiddleCenter
            };

            string grafico = "📊 TICKETS POR DÍA (Últimos 30 días)\n\n";
            for (int i = 29; i >= 0; i--)
            {
                var fecha = DateTime.Today.AddDays(-i);
                int count = _tickets.Count(t => t.FechaCreacion.Date == fecha);
                int barras = Math.Min(count, 30);
                string barra = new string('█', barras);
                grafico += $"{fecha:dd/MM}: {barra} {count}\n";
            }

            int total = _tickets.Count(t => t.FechaCreacion >= DateTime.Today.AddDays(-30));
            grafico += $"\n📊 Total últimos 30 días: {total} tickets";
            grafico += $"\n📈 Media diaria: {total / 30:F1} tickets";

            lblGrafico.Text = grafico;
            pnlGrafico.Controls.Add(lblGrafico);
            pnlContent.Controls.Add(pnlGrafico);

            var btnVolverDashboard = new Button
            {
                Text = "← Volver al Dashboard",
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnVolverDashboard.Click += (s, e) => MostrarDashboard();
            pnlContent.Controls.Add(btnVolverDashboard);
        }

        private void MostrarTiempoRespuesta()
        {
            var resueltos = _tickets.Where(t => t.FechaActualizacion.HasValue).ToList();

            pnlContent.Controls.Clear();

            var lblTitle = new Label
            {
                Text = "⏱️ Métricas de tiempo de respuesta",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(10, 10, 0, 0)
            };
            pnlContent.Controls.Add(lblTitle);

            var pnlMetricas = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30)
            };

            var lblInfo = new Label
            {
                Font = new Font("Segoe UI", 12),
                Location = new Point(30, 30),
                AutoSize = true
            };

            if (resueltos.Count == 0)
            {
                lblInfo.Text = "⚠️ Aún no hay tickets resueltos para calcular el tiempo de respuesta.";
            }
            else
            {
                double promedioHoras = resueltos.Average(t => (t.FechaActualizacion.Value - t.FechaCreacion).TotalHours);
                double maxHoras = resueltos.Max(t => (t.FechaActualizacion.Value - t.FechaCreacion).TotalHours);
                double minHoras = resueltos.Min(t => (t.FechaActualizacion.Value - t.FechaCreacion).TotalHours);

                lblInfo.Text =
                    $"📊 Tickets resueltos: {resueltos.Count}\n\n" +
                    $"⏰ Tiempo promedio: {promedioHoras:F1} horas\n" +
                    $"⚡ Tiempo más rápido: {minHoras:F1} horas\n" +
                    $"🐢 Tiempo más lento: {maxHoras:F1} horas\n\n";

                if (promedioHoras < 2)
                    lblInfo.Text += "✅ Excelente tiempo de respuesta";
                else if (promedioHoras < 8)
                    lblInfo.Text += "👍 Buen tiempo de respuesta";
                else if (promedioHoras < 24)
                    lblInfo.Text += "⚠️ Tiempo de respuesta aceptable";
                else
                    lblInfo.Text += "🔴 Tiempo de respuesta por mejorar";
            }

            pnlMetricas.Controls.Add(lblInfo);
            pnlContent.Controls.Add(pnlMetricas);

            var btnVolverDashboard = new Button
            {
                Text = "← Volver al Dashboard",
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnVolverDashboard.Click += (s, e) => MostrarDashboard();
            pnlContent.Controls.Add(btnVolverDashboard);
        }

        private async void ExportarReporte()
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Title = "Exportar reporte de tickets",
                    Filter = "Archivo CSV|*.csv",
                    DefaultExt = "csv",
                    FileName = $"Reporte_Tickets_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var sw = new System.IO.StreamWriter(saveDialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        sw.WriteLine("ID;Usuario;Asunto;Estado;Prioridad;Fecha Creación;Respuesta Admin;Fecha Actualización");
                        foreach (var ticket in _tickets)
                        {
                            sw.WriteLine($"{ticket.Id};{ticket.UsuarioNombre};{ticket.Asunto};{ticket.Estado};{ticket.Prioridad};{ticket.FechaCreacion:dd/MM/yyyy HH:mm};{ticket.RespuestaAdmin?.Replace(";", ",")};{ticket.FechaActualizacion:dd/MM/yyyy HH:mm}");
                        }
                    }
                    MessageBox.Show($"✅ Reporte exportado correctamente:\n{saveDialog.FileName}", "Exportación exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}