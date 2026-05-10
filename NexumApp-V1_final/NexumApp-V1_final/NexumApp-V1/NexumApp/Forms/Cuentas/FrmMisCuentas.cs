using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NexumApp.Forms.Cuentas
{
    internal partial class FrmMisCuentas : Form
    {
        private readonly CuentaService _cuentaService = new CuentaService();
        private FlowLayoutPanel panelCuentas;
        private Button btnNuevaCuenta;
        private Label lblTitulo;

        // --- NUEVA PALETA DE COLORES (AZUL & DORADO) ---
        private readonly Color BluePrimary = Color.FromArgb(26, 115, 232);  // Azul Google/Fintech
        private readonly Color BlueDark = Color.FromArgb(10, 50, 120);     // Azul profundo para contrastes
        private readonly Color GoldNexum = Color.FromArgb(212, 175, 55);    // Dorado Nexum
        private readonly Color BackgroundGray = Color.FromArgb(248, 249, 252); // Fondo claro

        public FrmMisCuentas()
        {
            InitializeComponent();
            ConfigurarUI();
            // Suscribimos al evento Load de forma segura
            this.Load += (s, e) => CargarCuentas();
        }

        private void ConfigurarUI()
        {
            this.Text = "Cuentas | Nexum Bank";
            this.BackColor = BackgroundGray;
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterParent;

            // Título Principal
            lblTitulo = new Label
            {
                Text = "Mis Cuentas",
                ForeColor = BlueDark,
                Font = new Font("Segoe UI Semibold", 26, FontStyle.Bold),
                Location = new Point(40, 30),
                AutoSize = true
            };

            // Botón de acción (Azul)
            btnNuevaCuenta = new Button
            {
                Text = "+  ABRIR NUEVA CUENTA",
                Size = new Size(240, 50),
                Location = new Point(40, 100),
                BackColor = BluePrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNuevaCuenta.FlatAppearance.BorderSize = 0;
            btnNuevaCuenta.Click += BtnNuevaCuenta_Click;

            // Panel contenedor
            panelCuentas = new FlowLayoutPanel
            {
                Location = new Point(20, 170),
                Size = new Size(this.Width - 40, this.Height - 220),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = Color.Transparent
            };

            this.Controls.Add(lblTitulo);
            this.Controls.Add(btnNuevaCuenta);
            this.Controls.Add(panelCuentas);

            // Redondeo del botón
            this.HandleCreated += (s, e) => Redondear(btnNuevaCuenta, 25);
        }

        private void CargarCuentas()
        {
            panelCuentas.Controls.Clear();
            if (SesionActual.Instancia.Usuario == null) return;

            try
            {
                var cuentas = _cuentaService.ObtenerCuentasPorUsuario(SesionActual.Instancia.Usuario.Id);

                if (cuentas == null || cuentas.Count == 0)
                {
                    MostrarEstadoVacio();
                    return;
                }

                foreach (var cuenta in cuentas)
                {
                    panelCuentas.Controls.Add(CrearCardCuenta(cuenta));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al sincronizar cuentas: {ex.Message}", "Nexum", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Panel CrearCardCuenta(CuentaBancaria cuenta)
        {
            Panel card = new Panel
            {
                Size = new Size(320, 200),
                BackColor = Color.White,
                Margin = new Padding(20),
                Cursor = Cursors.Hand
            };

            card.Click += (s, e) => {
                var frm = new FrmDetalleCuenta(cuenta);
                if (frm.ShowDialog() == DialogResult.OK) CargarCuentas();
            };

            // Franja Azul Superior
            Panel topBar = new Panel { Size = new Size(card.Width, 6), BackColor = BluePrimary, Dock = DockStyle.Top, Enabled = false };

            Label lblTipo = new Label
            {
                Text = cuenta.TipoCuenta.ToUpper(),
                ForeColor = Color.FromArgb(100, 110, 140),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Location = new Point(20, 25),
                AutoSize = true,
                Enabled = false
            };

            Label lblIban = new Label
            {
                Text = cuenta.NumeroCuenta,
                ForeColor = Color.Gray,
                Font = new Font("Consolas", 9),
                Location = new Point(20, 50),
                AutoSize = true,
                Enabled = false
            };

            Label lblSaldo = new Label
            {
                Text = cuenta.Saldo.ToString("C2"),
                ForeColor = BluePrimary,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                Location = new Point(15, 95),
                AutoSize = true,
                Enabled = false
            };

            // Badge Dorado de Estado
            Panel badge = new Panel { Size = new Size(80, 22), Location = new Point(220, 25), BackColor = GoldNexum, Enabled = false };
            Label lblStatus = new Label { Text = "ACTIVA", ForeColor = Color.Black, Font = new Font("Segoe UI", 7, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Enabled = false };
            badge.Controls.Add(lblStatus);

            card.Controls.Add(badge);
            card.Controls.Add(lblSaldo);
            card.Controls.Add(lblIban);
            card.Controls.Add(lblTipo);
            card.Controls.Add(topBar);

            // Efectos visuales de redondeo
            this.BeginInvoke(new Action(() => {
                Redondear(card, 20);
                Redondear(badge, 10);
            }));

            return card;
        }

        private void MostrarEstadoVacio()
        {
            Label lblMsg = new Label
            {
                Text = "No tienes cuentas vinculadas.\nHaz clic en el botón para comenzar.",
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(500, 100),
                Font = new Font("Segoe UI", 11, FontStyle.Italic)
            };
            panelCuentas.Controls.Add(lblMsg);
        }

        private void Redondear(Control c, int radio)
        {
            if (c.Width <= 0 || c.Height <= 0) return;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radio, radio, 180, 90);
            path.AddArc(c.Width - radio, 0, radio, radio, 270, 90);
            path.AddArc(c.Width - radio, c.Height - radio, radio, radio, 0, 90);
            path.AddArc(0, c.Height - radio, radio, radio, 90, 90);
            path.CloseAllFigures();
            c.Region = new Region(path);
        }

        private void BtnNuevaCuenta_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmAbrirCuenta())
            {
                if (frm.ShowDialog() == DialogResult.OK) CargarCuentas();
            }
        }
    }
}