// Forms/Tickets/FrmMisTicketsSimple.cs - TARJETAS CON TEXTO MOVIDO A LA DERECHA
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using NexumApp.Models;
using NexumApp.Services;

namespace NexumApp.Forms.Tickets
{
    public class FrmMisTicketsSimple : Form
    {
        private ListBox lstTickets;
        private Button btnNuevo;
        private Button btnExportar;
        private Button btnCerrar;
        private Panel pnlStats;
        private TicketService _service;
        private List<Ticket> _tickets;

        public FrmMisTicketsSimple()
        {
            _service = new TicketService();
            InitializeComponent();
            ConfigurarPanelEstadisticas();
            this.Load += async (s, e) => await CargarTickets();
        }

        private void InitializeComponent()
        {
            this.Text = "🎫 Mis Tickets de Soporte";
            this.Size = new Size(1100, 700);
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Panel Header
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = Color.FromArgb(99, 102, 241)
            };

            var lblIcono = new Label
            {
                Text = "🎫",
                Font = new Font("Segoe UI", 34),
                ForeColor = Color.White,
                Location = new Point(25, 25),
                AutoSize = true
            };

            var lblTitulo = new Label
            {
                Text = "Mis Tickets de Soporte",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(120, 22),
                AutoSize = true
            };

            var lblSubtitulo = new Label
            {
                Text = "Consulta el estado de tus solicitudes de soporte",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(220, 220, 255),
                Location = new Point(120, 52),
                AutoSize = true
            };

            pnlHeader.Controls.Add(lblIcono);
            pnlHeader.Controls.Add(lblTitulo);
            pnlHeader.Controls.Add(lblSubtitulo);

            // Panel de estadísticas
            pnlStats = new Panel
            {
                Location = new Point(40, 110),
                Size = new Size(1020, 100),
                BackColor = Color.Transparent
            };

            // ListBox para tickets
            lstTickets = new ListBox
            {
                Location = new Point(40, 225),
                Size = new Size(1020, 370),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                HorizontalScrollbar = true
            };
            lstTickets.DoubleClick += LstTickets_DoubleClick;

            // Botones
            btnNuevo = new Button
            {
                Text = "+ Nuevo Ticket",
                Location = new Point(40, 610),
                Size = new Size(140, 42),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNuevo.FlatAppearance.BorderSize = 0;
            btnNuevo.Click += BtnNuevo_Click;

            btnExportar = new Button
            {
                Text = "📎 Exportar",
                Location = new Point(195, 610),
                Size = new Size(120, 42),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExportar.FlatAppearance.BorderSize = 0;
            btnExportar.Click += BtnExportar_Click;

            btnCerrar = new Button
            {
                Text = "Cerrar",
                Location = new Point(960, 610),
                Size = new Size(100, 42),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Click += (s, e) => this.Close();

            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlStats);
            this.Controls.Add(lstTickets);
            this.Controls.Add(btnNuevo);
            this.Controls.Add(btnExportar);
            this.Controls.Add(btnCerrar);
        }

        private void ConfigurarPanelEstadisticas()
        {
            pnlStats.Controls.Clear();

            // Calcular posición centrada para 4 tarjetas de 210px cada una
            int startX = 90;
            int cardWidth = 210;

            var cardTotal = CrearTarjetaStats("🎫", "TOTAL", "0", Color.FromArgb(99, 102, 241));
            var cardPendientes = CrearTarjetaStats("⏳", "PENDIENTES", "0", Color.FromArgb(245, 158, 11));
            var cardResueltos = CrearTarjetaStats("✅", "RESUELTOS", "0", Color.FromArgb(16, 185, 129));
            var cardUrgentes = CrearTarjetaStats("⚡", "URGENTES", "0", Color.FromArgb(239, 68, 68));

            cardTotal.Location = new Point(startX, 0);
            cardPendientes.Location = new Point(startX + cardWidth, 0);
            cardResueltos.Location = new Point(startX + (cardWidth * 2), 0);
            cardUrgentes.Location = new Point(startX + (cardWidth * 3), 0);

            pnlStats.Controls.Add(cardTotal);
            pnlStats.Controls.Add(cardPendientes);
            pnlStats.Controls.Add(cardResueltos);
            pnlStats.Controls.Add(cardUrgentes);
        }

        private Panel CrearTarjetaStats(string icono, string titulo, string valor, Color color)
        {
            var card = new Panel
            {
                Size = new Size(210, 90),
                BackColor = Color.White,
                Cursor = Cursors.Default
            };

            card.Paint += (s, e) =>
            {
                using (var path = CrearRoundedRect(card.ClientRectangle, 10))
                using (var pen = new Pen(Color.FromArgb(220, 220, 230), 1))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(new SolidBrush(Color.White), path);
                    e.Graphics.DrawPath(pen, path);
                }
            };

            // Icono
            var lblIcono = new Label
            {
                Text = icono,
                Font = new Font("Segoe UI", 26),
                Location = new Point(12, 28),
                AutoSize = true
            };

            // TÍTULO MOVIDO MÁS A LA DERECHA (de 65 a 90)
            var lblTitulo = new Label
            {
                Text = titulo,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(90, 18),  // ANTES: 65, AHORA: 90
                AutoSize = true
            };

            // VALOR MOVIDO MÁS A LA DERECHA (de 65 a 90)
            var lblValor = new Label
            {
                Name = $"lblValor_{titulo}",
                Text = valor,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(90, 48),  // ANTES: 65, AHORA: 90
                AutoSize = true
            };

            card.Controls.Add(lblIcono);
            card.Controls.Add(lblTitulo);
            card.Controls.Add(lblValor);
            return card;
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

        private void ActualizarEstadisticas()
        {
            if (_tickets == null) return;

            int total = _tickets.Count;
            int pendientes = _tickets.Count(t => t.Estado == "Pendiente" || t.Estado == "En proceso");
            int resueltos = _tickets.Count(t => t.Estado == "Resuelto" || t.Estado == "Cerrado");
            int urgentes = _tickets.Count(t => t.Prioridad == "Urgente" || t.Prioridad == "Alta");

            ActualizarValorTarjeta("TOTAL", total.ToString());
            ActualizarValorTarjeta("PENDIENTES", pendientes.ToString());
            ActualizarValorTarjeta("RESUELTOS", resueltos.ToString());
            ActualizarValorTarjeta("URGENTES", urgentes.ToString());
        }

        private void ActualizarValorTarjeta(string nombre, string valor)
        {
            foreach (Control control in pnlStats.Controls)
            {
                var lbl = control.Controls.Find($"lblValor_{nombre}", true);
                if (lbl.Length > 0)
                {
                    lbl[0].Text = valor;
                    return;
                }
            }
        }

        private async System.Threading.Tasks.Task CargarTickets()
        {
            try
            {
                lstTickets.Items.Clear();
                lstTickets.Items.Add("🔄 Cargando tickets...");

                _tickets = await _service.ObtenerTicketsUsuarioAsync();

                lstTickets.Items.Clear();

                if (_tickets == null || _tickets.Count == 0)
                {
                    lstTickets.Items.Add("📭 No tienes tickets aún.");
                    lstTickets.Items.Add("");
                    lstTickets.Items.Add("👉 Haz clic en '+ Nuevo Ticket' para crear tu primera incidencia");
                    ActualizarEstadisticas();
                    return;
                }

                ActualizarEstadisticas();

                foreach (var ticket in _tickets.OrderByDescending(t => t.FechaCreacion))
                {
                    string estadoIcono = ticket.Estado == "Pendiente" ? "⏳" :
                                        ticket.Estado == "En proceso" ? "🔄" :
                                        ticket.Estado == "Resuelto" ? "✅" : "📌";

                    string prioridadIcono = ticket.Prioridad == "Urgente" ? "🔴" :
                                           ticket.Prioridad == "Alta" ? "🟠" :
                                           ticket.Prioridad == "Media" ? "🟡" : "🟢";

                    string fecha = ticket.FechaCreacion.ToString("dd/MM/yyyy HH:mm");
                    string respuesta = string.IsNullOrEmpty(ticket.RespuestaAdmin) ? "⏳ Sin respuesta" : "💬 Respondido";

                    string texto = $"{estadoIcono} #{ticket.Id} | {ticket.Asunto} | {prioridadIcono} {ticket.Prioridad} | {fecha} | {respuesta}";

                    lstTickets.Items.Add(texto);
                }
            }
            catch (Exception ex)
            {
                lstTickets.Items.Clear();
                lstTickets.Items.Add($"❌ Error: {ex.Message}");
            }
        }

        private void LstTickets_DoubleClick(object sender, EventArgs e)
        {
            if (lstTickets.SelectedIndex < 0) return;
            if (_tickets == null || _tickets.Count == 0) return;

            string seleccion = lstTickets.SelectedItem.ToString();

            int ticketId = -1;
            if (seleccion.Contains("#"))
            {
                int start = seleccion.IndexOf("#") + 1;
                int end = seleccion.IndexOf("|", start);
                if (end == -1) end = seleccion.Length;
                string idStr = seleccion.Substring(start, end - start).Trim();
                int.TryParse(idStr, out ticketId);
            }

            if (ticketId > 0)
            {
                var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
                if (ticket != null)
                    MostrarDetalleTicket(ticket);
            }
        }

        private void MostrarDetalleTicket(Ticket ticket)
        {
            Form frmDetalle = new Form
            {
                Text = $"Detalle del Ticket #{ticket.Id}",
                Size = new Size(580, 520),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            var pnlHeaderDetalle = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(99, 102, 241)
            };

            var lblHeaderTitulo = new Label
            {
                Text = $"🎫 Ticket #{ticket.Id}",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 25),
                AutoSize = true
            };
            pnlHeaderDetalle.Controls.Add(lblHeaderTitulo);

            var pnlContenidoDetalle = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(25),
                BackColor = Color.White,
                AutoScroll = true
            };

            int y = 10;

            var lblAsunto = new Label
            {
                Text = $"📌 {ticket.Asunto}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(0, y),
                AutoSize = true
            };
            y += 40;

            var lblEstado = new Label
            {
                Text = $"📊 Estado: {ticket.Estado}  |  Prioridad: {ticket.Prioridad}",
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, y),
                AutoSize = true
            };
            y += 30;

            var lblFecha = new Label
            {
                Text = $"📅 Creado: {ticket.FechaCreacion:dd/MM/yyyy HH:mm}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(0, y),
                AutoSize = true
            };
            y += 45;

            var lblDescTitulo = new Label
            {
                Text = "Tu consulta:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(0, y),
                AutoSize = true
            };
            y += 25;

            var txtDescripcion = new RichTextBox
            {
                Location = new Point(0, y),
                Size = new Size(500, 100),
                ReadOnly = true,
                Text = ticket.Descripcion,
                BackColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.FixedSingle
            };
            y += 115;

            var lblRespuestaTitulo = new Label
            {
                Text = "Respuesta del soporte:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(0, y),
                AutoSize = true
            };
            y += 25;

            var txtRespuesta = new RichTextBox
            {
                Location = new Point(0, y),
                Size = new Size(500, 100),
                ReadOnly = true,
                Text = string.IsNullOrEmpty(ticket.RespuestaAdmin)
                    ? "Aún no hay respuesta. El equipo está revisando tu consulta."
                    : ticket.RespuestaAdmin,
                BackColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.FixedSingle
            };
            y += 115;

            pnlContenidoDetalle.Controls.Add(lblAsunto);
            pnlContenidoDetalle.Controls.Add(lblEstado);
            pnlContenidoDetalle.Controls.Add(lblFecha);
            pnlContenidoDetalle.Controls.Add(lblDescTitulo);
            pnlContenidoDetalle.Controls.Add(txtDescripcion);
            pnlContenidoDetalle.Controls.Add(lblRespuestaTitulo);
            pnlContenidoDetalle.Controls.Add(txtRespuesta);

            var pnlFooterDetalle = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                BackColor = Color.FromArgb(248, 250, 252)
            };

            var btnCerrarDetalle = new Button
            {
                Text = "Cerrar",
                Location = new Point(450, 15),
                Size = new Size(100, 38),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCerrarDetalle.FlatAppearance.BorderSize = 0;
            btnCerrarDetalle.Click += (s, ev) => frmDetalle.Close();

            pnlFooterDetalle.Controls.Add(btnCerrarDetalle);

            frmDetalle.Controls.Add(pnlContenidoDetalle);
            frmDetalle.Controls.Add(pnlFooterDetalle);
            frmDetalle.Controls.Add(pnlHeaderDetalle);

            frmDetalle.ShowDialog(this);
        }

        private async void BtnNuevo_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmCrearTicketSimple())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    await CargarTickets();
                }
            }
        }

        private async void BtnExportar_Click(object sender, EventArgs e)
        {
            if (_tickets == null || _tickets.Count == 0)
            {
                MessageBox.Show("No hay tickets para exportar.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Archivo de texto|*.txt";
                sfd.FileName = $"Mis_Tickets_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                sfd.Title = "Exportar tickets";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(sfd.FileName))
                        {
                            sw.WriteLine("=".PadRight(80, '='));
                            sw.WriteLine("NEXUM BANK - MIS TICKETS");
                            sw.WriteLine($"Fecha de exportación: {DateTime.Now:dd/MM/yyyy HH:mm}");
                            sw.WriteLine("=".PadRight(80, '='));
                            sw.WriteLine();

                            int total = _tickets.Count;
                            int pendientes = _tickets.Count(t => t.Estado == "Pendiente" || t.Estado == "En proceso");
                            int resueltos = _tickets.Count(t => t.Estado == "Resuelto" || t.Estado == "Cerrado");
                            int urgentes = _tickets.Count(t => t.Prioridad == "Urgente" || t.Prioridad == "Alta");

                            sw.WriteLine($"RESUMEN:");
                            sw.WriteLine($"Total de tickets: {total}");
                            sw.WriteLine($"Pendientes: {pendientes}");
                            sw.WriteLine($"Resueltos: {resueltos}");
                            sw.WriteLine($"Urgentes: {urgentes}");
                            sw.WriteLine();
                            sw.WriteLine("=".PadRight(80, '='));
                            sw.WriteLine();

                            foreach (var ticket in _tickets.OrderByDescending(t => t.FechaCreacion))
                            {
                                sw.WriteLine($"Ticket #{ticket.Id}");
                                sw.WriteLine($"Asunto: {ticket.Asunto}");
                                sw.WriteLine($"Estado: {ticket.Estado}");
                                sw.WriteLine($"Prioridad: {ticket.Prioridad}");
                                sw.WriteLine($"Fecha: {ticket.FechaCreacion:dd/MM/yyyy HH:mm}");
                                sw.WriteLine($"Descripción: {ticket.Descripcion}");
                                sw.WriteLine($"Respuesta: {(string.IsNullOrEmpty(ticket.RespuestaAdmin) ? "Sin respuesta aún" : ticket.RespuestaAdmin)}");
                                sw.WriteLine("-".PadRight(80, '-'));
                            }
                        }

                        MessageBox.Show($"✅ Tickets exportados correctamente", "Exportación exitosa",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}