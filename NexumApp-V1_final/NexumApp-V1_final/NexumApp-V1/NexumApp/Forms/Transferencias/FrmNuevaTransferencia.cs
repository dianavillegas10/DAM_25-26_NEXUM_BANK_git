using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NexumApp.Forms.Transferencias
{
    internal partial class FrmNuevaTransferencia : Form
    {
        private readonly Color BluePrimary   = Color.FromArgb(99, 102, 241);
        private readonly Color BlueDark      = Color.FromArgb(49, 46, 129);
        private readonly Color BackgroundGray = Color.FromArgb(244, 247, 254);
        private readonly Color GrayText      = Color.FromArgb(107, 114, 128);

        private readonly CuentaService        _cuentaService        = new CuentaService();
        private readonly TransferenciaService _transferenciaService = new TransferenciaService();

        private ComboBox  cmbCuentaOrigen;
        private TextBox   txtCuenta;
        private TextBox   txtBeneficiario;
        private TextBox   txtConcepto;
        private TextBox   txtImporte;
        private Button    btnEnviar;
        private Panel     card;
        private Label     lblErrorImporte;
        private Label     lblErrorCuenta;

        public FrmNuevaTransferencia()
        {
            InitializeComponent();
            this.Load += FrmNuevaTransferencia_Load;
        }

        private void FrmNuevaTransferencia_Load(object sender, EventArgs e)
        {
            ConfigurarEstiloBase();
            DisenarInterfaz();
            CargarCuentas();
        }

        private void ConfigurarEstiloBase()
        {
            this.Text = "Enviar Dinero | Nexum Bank";
            this.Size = new Size(620, 760);
            this.BackColor = BackgroundGray;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void DisenarInterfaz()
        {
            this.Controls.Clear();

            // --- Título ---
            var lblHeader = new Label
            {
                Text = "Nueva Transferencia",
                ForeColor = BlueDark,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(40, 32),
                AutoSize = true
            };

            // --- Tarjeta principal ---
            card = new Panel
            {
                Size = new Size(540, 560),
                Location = new Point(40, 90),
                BackColor = Color.White
            };
            card.Controls.Add(new Panel
            {
                Size = new Size(540, 6),
                BackColor = BluePrimary,
                Dock = DockStyle.Top
            });

            int y = 30;

            // Cuenta origen
            CrearCampoLabel("CUENTA ORIGEN", y);
            cmbCuentaOrigen = new ComboBox
            {
                Location = new Point(40, y + 24),
                Size = new Size(460, 38),
                Font = new Font("Segoe UI", 11),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(245, 247, 250),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            card.Controls.Add(cmbCuentaOrigen);
            y += 85;

            // Cuenta destino
            CrearCampoLabel("CUENTA DESTINO (IBAN)", y);
            lblErrorCuenta = CrearLabelError(y + 18);
            txtCuenta = CrearTextBox(y + 24, 460);
            txtCuenta.CharacterCasing = CharacterCasing.Upper;
            txtCuenta.TextChanged += (s, ev) => lblErrorCuenta.Visible = false;
            y += 85;

            // Nombre beneficiario
            CrearCampoLabel("NOMBRE BENEFICIARIO (opcional)", y);
            txtBeneficiario = CrearTextBox(y + 24, 460);
            y += 85;

            // Importe
            CrearCampoLabel("IMPORTE (€)", y);
            lblErrorImporte = CrearLabelError(y + 18);
            txtImporte = CrearTextBox(y + 24, 200);
            txtImporte.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            txtImporte.ForeColor = BluePrimary;
            txtImporte.TextChanged += (s, ev) => lblErrorImporte.Visible = false;
            y += 85;

            // Concepto
            CrearCampoLabel("CONCEPTO", y);
            txtConcepto = CrearTextBox(y + 24, 460);
            y += 85;

            // Aviso
            card.Controls.Add(new Label
            {
                Text = "⚠  Las transferencias nacionales se ejecutan de forma inmediata.",
                ForeColor = GrayText,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                Location = new Point(40, y),
                Size = new Size(460, 32),
                TextAlign = ContentAlignment.MiddleCenter
            });

            // --- Botón enviar ---
            btnEnviar = new Button
            {
                Text = "CONFIRMAR Y ENVIAR",
                Size = new Size(540, 58),
                Location = new Point(40, 672),
                BackColor = BluePrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEnviar.FlatAppearance.BorderSize = 0;
            btnEnviar.Click += BtnEnviar_Click;

            var btnCancelar = new Button
            {
                Text = "VOLVER ATRÁS",
                Size = new Size(540, 40),
                Location = new Point(40, 700),
                BackColor = Color.Transparent,
                ForeColor = GrayText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (s, ev) => this.Close();

            this.Controls.Add(lblHeader);
            this.Controls.Add(card);
            this.Controls.Add(btnEnviar);
            this.Controls.Add(btnCancelar);

            BeginInvoke(new Action(() =>
            {
                Redondear(card, 16);
                Redondear(btnEnviar, 12);
            }));
        }

        private void CrearCampoLabel(string texto, int y)
        {
            card.Controls.Add(new Label
            {
                Text = texto,
                Location = new Point(40, y),
                ForeColor = GrayText,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                AutoSize = true
            });
        }

        private Label CrearLabelError(int y)
        {
            var lbl = new Label
            {
                Text = "",
                Location = new Point(40, y),
                ForeColor = Color.FromArgb(220, 38, 38),
                Font = new Font("Segoe UI", 8),
                AutoSize = true,
                Visible = false
            };
            card.Controls.Add(lbl);
            return lbl;
        }

        private TextBox CrearTextBox(int y, int ancho)
        {
            var tb = new TextBox
            {
                Location = new Point(40, y),
                Size = new Size(ancho, 38),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            card.Controls.Add(tb);
            return tb;
        }

        private void CargarCuentas()
        {
            if (SesionActual.Instancia?.Usuario == null) return;
            try
            {
                var cuentas = _cuentaService.ObtenerCuentasPorUsuario(SesionActual.Instancia.Usuario.Id)
                              ?? new List<CuentaBancaria>();
                cmbCuentaOrigen.DataSource = cuentas;
                cmbCuentaOrigen.DisplayMember = "NumeroCuenta";
                cmbCuentaOrigen.ValueMember = "Id";
                if (cuentas.Count == 0) btnEnviar.Enabled = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CargarCuentas error: " + ex.Message);
            }
        }

        private void BtnEnviar_Click(object sender, EventArgs e)
        {
            // Validaciones
            bool valido = true;

            if (cmbCuentaOrigen.SelectedItem == null)
            {
                valido = false;
            }

            if (string.IsNullOrWhiteSpace(txtCuenta.Text))
            {
                lblErrorCuenta.Text = "Introduce el IBAN de destino.";
                lblErrorCuenta.Visible = true;
                valido = false;
            }

            if (!decimal.TryParse(txtImporte.Text.Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal importe) || importe <= 0)
            {
                lblErrorImporte.Text = "Introduce un importe válido (ej: 150,50).";
                lblErrorImporte.Visible = true;
                valido = false;
            }

            if (!valido) return;

            var cuentaOrigen = (CuentaBancaria)cmbCuentaOrigen.SelectedItem;
            if (cuentaOrigen.Saldo < importe)
            {
                lblErrorImporte.Text = $"Saldo insuficiente. Disponible: {cuentaOrigen.Saldo:C2}";
                lblErrorImporte.Visible = true;
                return;
            }

            btnEnviar.Enabled = false;
            btnEnviar.Text = "PROCESANDO...";

            bool ok = _transferenciaService.RealizarTransferencia(
                cuentaOrigen.Id,
                txtCuenta.Text.Trim(),
                txtBeneficiario.Text.Trim(),
                importe,
                txtConcepto.Text.Trim(),
                out string error);

            if (ok)
            {
                MessageBox.Show(
                    $"Transferencia de {importe:C2} enviada correctamente a {txtCuenta.Text.Trim()}.",
                    "Transferencia realizada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(error ?? "Error al procesar la transferencia.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnEnviar.Enabled = true;
                btnEnviar.Text = "CONFIRMAR Y ENVIAR";
            }
        }

        private void Redondear(Control c, int r)
        {
            if (c.Width <= 0 || c.Height <= 0) return;
            var gp = new GraphicsPath();
            gp.AddArc(0, 0, r, r, 180, 90);
            gp.AddArc(c.Width - r, 0, r, r, 270, 90);
            gp.AddArc(c.Width - r, c.Height - r, r, r, 0, 90);
            gp.AddArc(0, c.Height - r, r, r, 90, 90);
            c.Region = new Region(gp);
        }
    }
}
