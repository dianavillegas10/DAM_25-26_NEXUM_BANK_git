using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NexumApp.Forms.Transferencias
{
    public partial class FrmTransferencias : Form
    {
        private readonly CuentaService _cuentaService = new CuentaService();
        private readonly MovimientoService _movimientoService = new MovimientoService();

        // Paleta de Colores Nexum (Coherente con el resto de la App)
        private readonly Color BluePrimary = Color.FromArgb(26, 115, 232);
        private readonly Color BlueDark = Color.FromArgb(10, 50, 120);
        private readonly Color BackgroundGray = Color.FromArgb(248, 249, 252);
        private readonly Color GrayText = Color.FromArgb(120, 130, 140);

        private ComboBox cmbOrigen;
        private TextBox txtDestino, txtImporte, txtConcepto;
        private Button btnTransferir;
        private Panel cardPrincipal;

        public FrmTransferencias()
        {
            InitializeComponent();
            ConfigurarFormulario();
            DisenarPantalla();

            this.Load += FrmTransferencias_Load;
        }

        private void FrmTransferencias_Load(object sender, EventArgs e)
        {
            // Redondeos y carga de datos
            this.BeginInvoke(new Action(() => {
                Redondear(cardPrincipal, 30);
                Redondear(btnTransferir, 20);
                Redondear(cmbOrigen, 10);
            }));
            CargarCuentas();
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Enviar Dinero | Nexum Bank";
            this.Size = new Size(650, 820); // Un poco más alto para que respire
            this.BackColor = BackgroundGray;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void DisenarPantalla()
        {
            this.Controls.Clear();

            // --- HEADER ---
            Label lblHeader = new Label
            {
                Text = "Nueva Transferencia",
                ForeColor = BlueDark,
                Font = new Font("Segoe UI Semibold", 22, FontStyle.Bold),
                Location = new Point(45, 35),
                AutoSize = true
            };

            // --- TARJETA CENTRAL ---
            cardPrincipal = new Panel
            {
                Size = new Size(550, 580),
                Location = new Point(45, 110),
                BackColor = Color.White,
            };

            // Barra decorativa superior
            Panel topBar = new Panel { Size = new Size(550, 10), BackColor = BluePrimary, Dock = DockStyle.Top };
            cardPrincipal.Controls.Add(topBar);

            int startY = 50;
            int spacing = 100;

            // 1. Cuenta Origen
            CrearEtiqueta(cardPrincipal, "Selecciona tu cuenta origen", startY);
            cmbOrigen = new ComboBox
            {
                Location = new Point(40, startY + 25),
                Width = 470,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(245, 247, 250),
                ForeColor = Color.FromArgb(60, 70, 80),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cardPrincipal.Controls.Add(cmbOrigen);

            // 2. Cuenta Destino
            startY += spacing;
            txtDestino = CrearInputModerno(cardPrincipal, "Cuenta destino (IBAN)", startY);

            // 3. Importe
            startY += spacing;
            txtImporte = CrearInputModerno(cardPrincipal, "Importe a enviar (€)", startY);

            // 4. Concepto
            startY += spacing;
            txtConcepto = CrearInputModerno(cardPrincipal, "Concepto o mensaje", startY);

            // --- BOTÓN CONFIRMAR ---
            btnTransferir = new Button
            {
                Text = "CONFIRMAR ENVÍO",
                Size = new Size(550, 65),
                Location = new Point(45, 705),
                BackColor = BluePrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTransferir.FlatAppearance.BorderSize = 0;
            btnTransferir.Click += BtnTransferir_Click;

            this.Controls.Add(lblHeader);
            this.Controls.Add(cardPrincipal);
            this.Controls.Add(btnTransferir);
        }

        private TextBox CrearInputModerno(Panel contenedor, string placeholder, int y)
        {
            CrearEtiqueta(contenedor, placeholder, y);

            Panel lineaBase = new Panel
            {
                Size = new Size(470, 2),
                Location = new Point(40, y + 60),
                BackColor = Color.FromArgb(220, 225, 230)
            };

            TextBox txt = new TextBox
            {
                Location = new Point(40, y + 30),
                Width = 470,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 12),
                Text = placeholder
            };

            // Efectos de foco (Azul Eléctrico)
            txt.Enter += (s, e) => {
                if (txt.Text == placeholder) { txt.Text = ""; txt.ForeColor = Color.Black; }
                lineaBase.BackColor = BluePrimary;
                lineaBase.Height = 2;
            };
            txt.Leave += (s, e) => {
                if (string.IsNullOrWhiteSpace(txt.Text)) { txt.Text = placeholder; txt.ForeColor = Color.LightGray; }
                lineaBase.BackColor = Color.FromArgb(220, 225, 230);
            };

            contenedor.Controls.Add(txt);
            contenedor.Controls.Add(lineaBase);
            return txt;
        }

        private void CrearEtiqueta(Panel p, string texto, int y)
        {
            p.Controls.Add(new Label
            {
                Text = texto.ToUpper(),
                Location = new Point(40, y),
                ForeColor = GrayText,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                AutoSize = true
            });
        }

        private void Redondear(Control c, int radio)
        {
            if (c == null || c.Width <= 0 || c.Height <= 0) return;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radio, radio, 180, 90);
            path.AddArc(c.Width - radio, 0, radio, radio, 270, 90);
            path.AddArc(c.Width - radio, c.Height - radio, radio, radio, 0, 90);
            path.AddArc(0, c.Height - radio, radio, radio, 90, 90);
            c.Region = new Region(path);
        }

        private void CargarCuentas()
        {
            try
            {
                if (SesionActual.Instancia.Usuario == null) return;
                var cuentas = _cuentaService.ObtenerCuentasPorUsuario(SesionActual.Instancia.Usuario.Id);
                cmbOrigen.DataSource = cuentas;
                cmbOrigen.DisplayMember = "NumeroCuenta";
                cmbOrigen.ValueMember = "Id";
            }
            catch (Exception) { MessageBox.Show("Error al cargar cuentas."); }
        }

        private void BtnTransferir_Click(object sender, EventArgs e)
        {
            if (cmbOrigen.SelectedItem == null || txtDestino.Text == "Cuenta destino (IBAN)")
            {
                MessageBox.Show("Completa los datos de destino.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtImporte.Text, out decimal importe) || importe <= 0)
            {
                MessageBox.Show("Monto no válido.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cuentaO = (CuentaBancaria)cmbOrigen.SelectedItem;
            if (cuentaO.Saldo < importe)
            {
                MessageBox.Show("Saldo insuficiente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnTransferir.Enabled = false;
            btnTransferir.Text = "PROCESANDO...";

            bool ok = _movimientoService.RealizarTransferencia(
                cuentaO.Id,
                txtDestino.Text,
                importe,
                txtConcepto.Text == "Concepto o mensaje" ? "" : txtConcepto.Text
            );

            if (ok)
            {
                MessageBox.Show("¡Transferencia enviada con éxito!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Error en la transacción.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnTransferir.Enabled = true;
                btnTransferir.Text = "CONFIRMAR ENVÍO";
            }
        }
    }
}