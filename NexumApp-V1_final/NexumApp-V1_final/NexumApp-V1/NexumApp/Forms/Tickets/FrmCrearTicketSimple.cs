// Forms/Tickets/FrmCrearTicketSimple.cs - VERSIÓN CON MÁRGENES CORREGIDOS
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using NexumApp.Models;
using NexumApp.Services;

namespace NexumApp.Forms.Tickets
{
    public class FrmCrearTicketSimple : Form
    {
        private TextBox txtAsunto;
        private RichTextBox txtDescripcion;
        private ComboBox cmbPrioridad;
        private Button btnEnviar;
        private Button btnCancelar;
        private TicketService _service;
        private Panel pnlHeader;
        private Panel pnlContent;

        public FrmCrearTicketSimple()
        {
            _service = new TicketService();
            InitializeComponent();
            ConfigurarFormulario();
        }

        private void InitializeComponent()
        {
            this.Text = "📝 Nuevo Ticket de Soporte";
            this.Size = new Size(600, 600);  // Ventana más ancha
            this.MinimumSize = new Size(550, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void ConfigurarFormulario()
        {
            // ========== HEADER ==========
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = Color.FromArgb(99, 102, 241)
            };

            var lblIcono = new Label
            {
                Text = "📝",
                Font = new Font("Segoe UI", 34),
                ForeColor = Color.White,
                Location = new Point(30, 25),
                AutoSize = true
            };

            var lblTitulo = new Label
            {
                Text = "Nuevo Ticket de Soporte",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(120, 22),
                AutoSize = true
            };

            var lblSubtitulo = new Label
            {
                Text = "Describe tu problema y te ayudaremos lo antes posible",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(220, 220, 255),
                Location = new Point(120, 52),
                AutoSize = true
            };

            pnlHeader.Controls.Add(lblIcono);
            pnlHeader.Controls.Add(lblTitulo);
            pnlHeader.Controls.Add(lblSubtitulo);

            // ========== CONTENIDO ==========
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30, 25, 30, 25),  // Más padding
                BackColor = Color.White
            };

            int currentY = 10;
            int marginLeft = 10;  // Margen izquierdo para todos los controles

            // Info de ayuda
            var lblInfo = new Label
            {
                Text = "💡 Cuanto más detalle nos des, más rápido podremos ayudarte",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(16, 185, 129),
                Location = new Point(marginLeft, currentY),
                Size = new Size(500, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            currentY += 35;

            // Asunto
            var lblAsunto = new Label
            {
                Text = "Asunto *",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(marginLeft, currentY),
                AutoSize = true
            };
            currentY += 25;

            txtAsunto = new TextBox
            {
                Location = new Point(marginLeft, currentY),
                Size = new Size(500, 30),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            txtAsunto.Text = "";
            txtAsunto.Enter += (s, e) =>
            {
                if (txtAsunto.Text == "Ej: Problema con transferencia, error al iniciar sesión...")
                {
                    txtAsunto.Text = "";
                    txtAsunto.ForeColor = SystemColors.WindowText;
                }
            };
            txtAsunto.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtAsunto.Text))
                {
                    txtAsunto.Text = "Ej: Problema con transferencia, error al iniciar sesión...";
                    txtAsunto.ForeColor = Color.Gray;
                }
            };
            txtAsunto.ForeColor = Color.Gray;
            currentY += 45;

            // Descripción
            var lblDescripcion = new Label
            {
                Text = "Descripción *",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(marginLeft, currentY),
                AutoSize = true
            };
            currentY += 25;

            txtDescripcion = new RichTextBox
            {
                Location = new Point(marginLeft, currentY),
                Size = new Size(500, 130),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            currentY += 145;

            // Prioridad
            var lblPrioridad = new Label
            {
                Text = "Prioridad",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(marginLeft, currentY),
                AutoSize = true
            };
            currentY += 25;

            // Panel para prioridad con colores
            var pnlPrioridad = new Panel
            {
                Location = new Point(marginLeft, currentY),
                Size = new Size(500, 40),
                BackColor = Color.White
            };

            cmbPrioridad = new ComboBox
            {
                Location = new Point(0, 6),
                Size = new Size(200, 28),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White
            };
            cmbPrioridad.Items.AddRange(new[] { "🟢 Baja", "🟡 Media", "🟠 Alta", "🔴 Urgente" });
            cmbPrioridad.SelectedIndex = 1;
            cmbPrioridad.DrawMode = DrawMode.OwnerDrawFixed;
            cmbPrioridad.DrawItem += CmbPrioridad_DrawItem;

            pnlPrioridad.Controls.Add(cmbPrioridad);
            currentY += 50;

            // Botones - Centrados horizontalmente
            int botonesY = currentY;
            int anchoTotalBotones = 140 + 15 + 110; // btnEnviar + espacio + btnCancelar
            int inicioBotones = (pnlContent.Width - anchoTotalBotones) / 2;

            btnEnviar = new Button
            {
                Text = "✓ Enviar Ticket",
                Location = new Point(inicioBotones, botonesY),
                Size = new Size(140, 42),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEnviar.FlatAppearance.BorderSize = 0;
            btnEnviar.Click += BtnEnviar_Click;

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(inicioBotones + 140 + 15, botonesY),
                Size = new Size(110, 42),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (s, e) => this.Close();

            pnlContent.Controls.Add(lblInfo);
            pnlContent.Controls.Add(lblAsunto);
            pnlContent.Controls.Add(txtAsunto);
            pnlContent.Controls.Add(lblDescripcion);
            pnlContent.Controls.Add(txtDescripcion);
            pnlContent.Controls.Add(lblPrioridad);
            pnlContent.Controls.Add(pnlPrioridad);
            pnlContent.Controls.Add(btnEnviar);
            pnlContent.Controls.Add(btnCancelar);

            // Evento para centrar botones cuando se redimensione
            pnlContent.Resize += (s, e) =>
            {
                int nuevoInicio = (pnlContent.Width - anchoTotalBotones) / 2;
                btnEnviar.Location = new Point(nuevoInicio, botonesY);
                btnCancelar.Location = new Point(nuevoInicio + 140 + 15, botonesY);
            };

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlHeader);
        }

        private void CmbPrioridad_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            ComboBox combo = sender as ComboBox;
            if (combo == null) return;

            string texto = combo.Items[e.Index].ToString();
            Color colorTexto = Color.Black;

            if (texto.Contains("Baja")) colorTexto = Color.FromArgb(34, 197, 94);
            else if (texto.Contains("Media")) colorTexto = Color.FromArgb(234, 179, 8);
            else if (texto.Contains("Alta")) colorTexto = Color.FromArgb(249, 115, 22);
            else if (texto.Contains("Urgente")) colorTexto = Color.FromArgb(239, 68, 68);

            e.DrawBackground();
            using (var brush = new SolidBrush(colorTexto))
            {
                e.Graphics.DrawString(texto, e.Font, brush, e.Bounds);
            }
            e.DrawFocusRectangle();
        }

        private async void BtnEnviar_Click(object sender, EventArgs e)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(txtAsunto.Text) || txtAsunto.Text == "Ej: Problema con transferencia, error al iniciar sesión...")
            {
                MessageBox.Show("Por favor, escribe un asunto.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAsunto.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
            {
                MessageBox.Show("Por favor, describe tu problema.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDescripcion.Focus();
                return;
            }

            btnEnviar.Enabled = false;
            btnEnviar.Text = "Enviando...";

            try
            {
                string prioridadSeleccionada = cmbPrioridad.SelectedItem?.ToString() ?? "🟡 Media";
                string prioridad = prioridadSeleccionada.Replace("🟢 ", "").Replace("🟡 ", "").Replace("🟠 ", "").Replace("🔴 ", "");

                var request = new TicketRequest
                {
                    Asunto = txtAsunto.Text.Trim(),
                    Descripcion = txtDescripcion.Text.Trim(),
                    Prioridad = prioridad
                };

                bool ok = await _service.EnviarTicketAsync(request);

                if (ok)
                {
                    MessageBox.Show(
                        "✅ Ticket enviado correctamente\n\nEl equipo de soporte revisará tu consulta y te responderá en menos de 24 horas.",
                        "Ticket creado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("❌ Error al enviar el ticket.\n\nPor favor, intenta de nuevo más tarde.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnEnviar.Enabled = true;
                btnEnviar.Text = "✓ Enviar Ticket";
            }
        }
    }
}