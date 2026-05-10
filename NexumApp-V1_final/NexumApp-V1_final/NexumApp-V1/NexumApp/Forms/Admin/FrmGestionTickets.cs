using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using NexumApp.Services; // Asegúrate de que esta ruta sea correcta
using System.Linq;

namespace NexumApp.Forms.Admin
{
    public class FrmGestionTickets : Form
    {
        private DataGridView dgvTickets;
        private RichTextBox txtRespuesta;
        private ComboBox cmbEstado;
        private Button btnResponder;
        private Button btnCerrar;
        private Label lblTicketSeleccionado;

        // Inyectamos el servicio
        private TicketService _ticketService = new TicketService();

        public FrmGestionTickets()
        {
            InitializeComponentLayout();

            // Cargamos los datos reales al iniciar
            CargarTicketsDesdeJson();
        }

        private void InitializeComponentLayout()
        {
            this.Text = "🎫 Gestión de Tickets (Admin)";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            var lblTitulo = new Label
            {
                Text = "🎫 GESTIÓN DE TICKETS",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(99, 102, 241),
                Location = new Point(20, 20),
                AutoSize = true
            };

            // Tabla de tickets
            dgvTickets = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(940, 250),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };

            dgvTickets.Columns.Add("Id", "ID");
            dgvTickets.Columns.Add("Usuario", "Usuario");
            dgvTickets.Columns.Add("Asunto", "Asunto");
            dgvTickets.Columns.Add("Estado", "Estado");
            dgvTickets.Columns.Add("Prioridad", "Prioridad");
            dgvTickets.Columns.Add("Fecha", "Fecha");

            // ELIMINADO: Ya no hay Rows.Add manuales aquí.

            dgvTickets.SelectionChanged += (s, e) => {
                if (dgvTickets.SelectedRows.Count > 0)
                {
                    var row = dgvTickets.SelectedRows[0];
                    lblTicketSeleccionado.Text = $"Ticket seleccionado: #{row.Cells[0].Value} - {row.Cells[2].Value}";

                    // Opcional: Podrías cargar la respuesta previa si existiera
                    txtRespuesta.Text = "";
                    cmbEstado.SelectedItem = row.Cells[3].Value.ToString();
                }
            };

            // Panel de respuesta
            var pnlRespuesta = new Panel
            {
                Location = new Point(20, 340),
                Size = new Size(940, 200),
                BackColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblRespuesta = new Label
            {
                Text = "Respuesta del administrador:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(15, 15),
                AutoSize = true
            };

            txtRespuesta = new RichTextBox
            {
                Location = new Point(15, 45),
                Size = new Size(600, 100),
                Font = new Font("Segoe UI", 10)
            };

            var lblEstado = new Label
            {
                Text = "Estado:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(15, 160),
                AutoSize = true
            };

            cmbEstado = new ComboBox
            {
                Location = new Point(75, 157),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbEstado.Items.AddRange(new[] { "Pendiente", "En proceso", "Resuelto", "Cerrado" });
            cmbEstado.SelectedIndex = 0;

            btnResponder = new Button
            {
                Text = "Guardar y Responder",
                Location = new Point(750, 80),
                Size = new Size(160, 40),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnResponder.Click += BtnResponder_Click;

            lblTicketSeleccionado = new Label
            {
                Text = "Selecciona un ticket de la lista",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(15, 210),
                AutoSize = true
            };

            pnlRespuesta.Controls.Add(lblTicketSeleccionado);
            pnlRespuesta.Controls.Add(btnResponder);
            pnlRespuesta.Controls.Add(cmbEstado);
            pnlRespuesta.Controls.Add(lblEstado);
            pnlRespuesta.Controls.Add(txtRespuesta);
            pnlRespuesta.Controls.Add(lblRespuesta);

            btnCerrar = new Button
            {
                Text = "Cerrar",
                Location = new Point(860, 560),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCerrar.Click += (s, e) => this.Close();

            this.Controls.Add(btnCerrar);
            this.Controls.Add(pnlRespuesta);
            this.Controls.Add(dgvTickets);
            this.Controls.Add(lblTitulo);
        }

        // MÉTODO DE CARGA REAL
        private async void CargarTicketsDesdeJson()
        {
            try
            {
                // Asegúrate de haber implementado ObtenerTodosLosTicketsAdminAsync en tu TicketService
                var tickets = await _ticketService.ObtenerTodosLosTicketsAdminAsync();

                dgvTickets.Rows.Clear();
                foreach (var t in tickets)
                {
                    dgvTickets.Rows.Add(
                        t.Id,
                        t.UsuarioNombre ?? "Desconocido",
                        t.Asunto,
                        t.Estado,
                        t.Prioridad,
                        t.FechaCreacion.ToString("dd/MM/yyyy HH:mm")
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar tickets: {ex.Message}");
            }
        }

        // LÓGICA PARA GUARDAR LA RESPUESTA
        private void BtnResponder_Click(object sender, EventArgs e)
        {
            if (dgvTickets.SelectedRows.Count == 0)
            {
                MessageBox.Show("Por favor, selecciona un ticket de la tabla.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int ticketId = Convert.ToInt32(dgvTickets.SelectedRows[0].Cells[0].Value);
                string respuesta = txtRespuesta.Text.Trim();
                string estado = cmbEstado.SelectedItem.ToString();

                // Llamamos al servicio para que actualice el archivo JSON
                // Nota: Asegúrate de añadir el método 'ResponderTicket' a tu TicketService
                _ticketService.ResponderTicket(ticketId, respuesta, estado);

                MessageBox.Show("Respuesta guardada y estado actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Refrescamos la lista para ver los cambios
                CargarTicketsDesdeJson();
                txtRespuesta.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}