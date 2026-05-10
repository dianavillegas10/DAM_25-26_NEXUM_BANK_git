// Forms/Tickets/FrmOpcionesAyuda.cs
using NexumApp.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NexumApp.Forms.Tickets
{
    public class FrmOpcionesAyuda : Form
    {
        // ── Paleta dinámica ───────────────────────────────────
        private Color C_Bg      => AppSettings.ModoOscuro ? Color.FromArgb(10,  12,  28)  : Color.White;
        private Color C_Surface => AppSettings.ModoOscuro ? Color.FromArgb(14,  17,  38)  : Color.FromArgb(240, 242, 245);
        private Color C_Text    => AppSettings.ModoOscuro ? Color.FromArgb(241, 245, 249) : Color.FromArgb(31,  41,  55);
        private Color C_Muted   => AppSettings.ModoOscuro ? Color.FromArgb(100, 116, 139) : Color.FromArgb(100, 116, 139);

        // Accent colors (estáticos)
        private static readonly Color C_Indigo  = Color.FromArgb(99,  102, 241);
        private static readonly Color C_Green   = Color.FromArgb(16,  185, 129);
        private static readonly Color C_Orange  = Color.FromArgb(249, 115,  22);

        public FrmOpcionesAyuda()
        {
            this.Text            = "Centro de Ayuda y Soporte";
            this.Size            = new Size(550, 480);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.DoubleBuffered  = true;

            AppSettings.ConfiguracionChanged += (s, e) =>
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(new Action(() => { BuildUI(); AppSettings.AplicarTraduccionesRecursivo(this); }));
            };
        }

        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); BuildUI(); }

        private void BuildUI()
        {
            Controls.Clear();
            BackColor = C_Bg;

            var lblTitulo = new Label
            {
                Text = "🎫 Centro de Ayuda y Soporte",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = C_Indigo,
                Location = new Point(30, 25),
                AutoSize = true
            };

            var lblSubtitulo = new Label
            {
                Text = "¿En qué podemos ayudarte? Selecciona una opción:",
                Font = new Font("Segoe UI", 12),
                ForeColor = C_Muted,
                Location = new Point(30, 65),
                AutoSize = true
            };

            // Botón Crear Ticket
            var btnCrearTicket = new Button
            {
                Text = "  📝  Crear nuevo ticket",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Location = new Point(30, 110),
                Size = new Size(470, 65),
                BackColor = C_Indigo,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                ImageAlign = ContentAlignment.MiddleLeft
            };
            btnCrearTicket.FlatAppearance.BorderSize = 0;
            btnCrearTicket.Click += (s, e) =>
            {
                using (var frm = new FrmCrearTicketSimple())
                {
                    frm.ShowDialog(this);
                    this.Close();
                }
            };

            // Botón Ver Tickets
            var btnVerTickets = new Button
            {
                Text = "  📋  Ver mis tickets",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Location = new Point(30, 190),
                Size = new Size(470, 65),
                BackColor = C_Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft
            };
            btnVerTickets.FlatAppearance.BorderSize = 0;
            btnVerTickets.Click += (s, e) =>
            {
                using (var frm = new FrmMisTicketsSimple())
                {
                    frm.ShowDialog(this);
                    this.Close();
                }
            };

            // Botón Contacto Directo
            var btnContacto = new Button
            {
                Text = "  📞  Contacto directo",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Location = new Point(30, 270),
                Size = new Size(470, 65),
                BackColor = C_Orange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft
            };
            btnContacto.FlatAppearance.BorderSize = 0;
            btnContacto.Click += (s, e) =>
            {
                MessageBox.Show(
                    "📧  soporte@nexumbank.com\n☎   900 123 456\n🕒  Lunes a Viernes · 9:00 – 18:00",
                    "Contacto Directo — Nexum Bank",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            };

            // Botón Cerrar
            var btnCerrar = new Button
            {
                Text = "Cerrar",
                Location = new Point(420, 365),
                Size = new Size(100, 40),
                BackColor = C_Surface,
                ForeColor = C_Text,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Click += (s, e) => this.Close();

            // Panel decorativo inferior
            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = C_Surface
            };

            var lblFooter = new Label
            {
                Text = "Nexum Bank - Soporte 24/7",
                Font = new Font("Segoe UI", 9),
                ForeColor = C_Muted,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            pnlFooter.Controls.Add(lblFooter);

            // Agregar todos los controles
            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblSubtitulo);
            this.Controls.Add(btnCrearTicket);
            this.Controls.Add(btnVerTickets);
            this.Controls.Add(btnContacto);
            this.Controls.Add(btnCerrar);
            this.Controls.Add(pnlFooter);
        }
    }
}