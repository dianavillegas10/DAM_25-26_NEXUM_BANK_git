using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace NexumApp.Forms.Cuentas
{
    internal partial class FrmAbrirCuenta : Form
    {
        private readonly CuentaService _cuentaService = new CuentaService();
        private readonly Random _random = new Random();

        // Paleta de Colores Nexum
        private readonly Color BluePrimary = Color.FromArgb(26, 115, 232);
        private readonly Color BlueDark = Color.FromArgb(10, 50, 120);
        private readonly Color BackgroundGray = Color.FromArgb(248, 249, 252);
        private readonly Color GoldNexum = Color.FromArgb(212, 175, 55);

        // Controles Dinámicos
        private ComboBox cmbTipoCuenta;
        private Button btnConfirmar;
        private Panel cardCentral;

        // Propiedad pública para que el formulario padre pueda obtener la cuenta creada
        public CuentaBancaria CuentaCreada { get; private set; }

        public FrmAbrirCuenta()
        {
            InitializeComponent();
            this.Load += FrmAbrirCuenta_Load;
        }

        private void FrmAbrirCuenta_Load(object sender, EventArgs e)
        {
            ConfigurarEstiloBase();
            DisenarInterfaz();
            CargarTiposCuenta();
        }

        private void ConfigurarEstiloBase()
        {
            this.Size = new Size(600, 700);
            this.BackColor = BackgroundGray;
            this.Text = "Apertura de Cuenta | Nexum Bank";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
        }

        private void DisenarInterfaz()
        {
            this.Controls.Clear();

            // Título Principal
            Label lblHeader = new Label
            {
                Text = "Nueva Cuenta",
                ForeColor = BlueDark,
                Font = new Font("Segoe UI Semibold", 22, FontStyle.Bold),
                Location = new Point(50, 40),
                AutoSize = true
            };

            // Tarjeta Central Blanca
            cardCentral = new Panel
            {
                Size = new Size(500, 350),
                Location = new Point(50, 110),
                BackColor = Color.White
            };

            // Decoración Superior
            Panel topBar = new Panel { Size = new Size(500, 10), BackColor = BluePrimary, Dock = DockStyle.Top };
            cardCentral.Controls.Add(topBar);

            // Instrucción
            Label lblInstruccion = new Label
            {
                Text = "SELECCIONA EL TIPO DE PRODUCTO",
                ForeColor = Color.DarkGray,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(40, 60),
                AutoSize = true
            };
            cardCentral.Controls.Add(lblInstruccion);

            // ComboBox Estilizado
            cmbTipoCuenta = new ComboBox
            {
                Location = new Point(40, 90),
                Width = 420,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 242, 245)
            };
            cardCentral.Controls.Add(cmbTipoCuenta);

            // Información Informativa (Simulada)
            Label lblInfo = new Label
            {
                Text = "Al abrir una cuenta en Nexum Bank, aceptas nuestras políticas de privacidad y comisiones de mantenimiento (0€ para nuevos clientes).",
                Size = new Size(420, 80),
                Location = new Point(40, 160),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                TextAlign = ContentAlignment.TopLeft
            };
            cardCentral.Controls.Add(lblInfo);

            // Botón Crear (Azul)
            btnConfirmar = new Button
            {
                Text = "CONFIRMAR APERTURA",
                Size = new Size(500, 65),
                Location = new Point(50, 490),
                BackColor = BluePrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnConfirmar.FlatAppearance.BorderSize = 0;
            btnConfirmar.Click += BtnCrear_Click;

            // Botón Cancelar (Outline)
            Button btnCancelar = new Button
            {
                Text = "CANCELAR",
                Size = new Size(500, 50),
                Location = new Point(50, 570),
                BackColor = Color.Transparent,
                ForeColor = Color.Gray,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (s, e) => this.Close();

            this.Controls.Add(lblHeader);
            this.Controls.Add(cardCentral);
            this.Controls.Add(btnConfirmar);
            this.Controls.Add(btnCancelar);

            // Aplicar redondeos
            this.BeginInvoke(new Action(() => {
                Redondear(cardCentral, 30);
                Redondear(btnConfirmar, 20);
                Redondear(cmbTipoCuenta, 10);
            }));
        }

        private void CargarTiposCuenta()
        {
            cmbTipoCuenta.Items.Clear();
            cmbTipoCuenta.Items.Add("Corriente");
            cmbTipoCuenta.Items.Add("Ahorro");
            cmbTipoCuenta.Items.Add("Nomina");
            cmbTipoCuenta.SelectedIndex = 0;
        }

        private void BtnCrear_Click(object sender, EventArgs e)
        {
            if (cmbTipoCuenta.SelectedIndex < 0) return;

            string tipo = cmbTipoCuenta.SelectedItem.ToString();
            int usuarioId = SesionActual.Instancia.Usuario.Id;

            // Generación de datos técnicos
            string numeroCuenta = "ES21" + GenerarDigitosAleatorios(10);
            string iban = "ES" + GenerarDigitosAleatorios(2) + "2100" + GenerarDigitosAleatorios(14);

            btnConfirmar.Enabled = false;
            btnConfirmar.Text = "PROCESANDO...";

            try
            {
                bool resultado = _cuentaService.CrearCuenta(usuarioId, tipo, 0, numeroCuenta, iban);

                if (resultado)
                {
                    // 🔥 Obtener la cuenta recién creada (necesitas implementar este método en CuentaService)
                    var cuentaCreada = _cuentaService.ObtenerCuentaPorNumero(numeroCuenta);

                    if (cuentaCreada != null)
                    {
                        CuentaCreada = cuentaCreada;

                        // 🔥 Generar tarjeta REAL en la base de datos
                        var tarjetaService = new TarjetaService();
                        tarjetaService.GenerarTarjetaParaCuenta(usuarioId, cuentaCreada.Id, tipo == "Crédito" ? "Credito" : "Debito");

                        MessageBox.Show("¡Enhorabuena! Tu cuenta y tarjeta han sido activadas.",
                            "Nexum Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Cuenta creada pero no se pudo obtener la información.",
                            "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se pudo procesar la solicitud.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnConfirmar.Enabled = true;
                    btnConfirmar.Text = "CONFIRMAR APERTURA";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnConfirmar.Enabled = true;
                btnConfirmar.Text = "CONFIRMAR APERTURA";
            }
        }

        private string GenerarDigitosAleatorios(int longitud)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < longitud; i++) sb.Append(_random.Next(0, 10));
            return sb.ToString();
        }

        private void Redondear(Control c, int r)
        {
            if (c.Width <= 0 || c.Height <= 0) return;
            GraphicsPath gp = new GraphicsPath();
            gp.AddArc(0, 0, r, r, 180, 90);
            gp.AddArc(c.Width - r, 0, r, r, 270, 90);
            gp.AddArc(c.Width - r, c.Height - r, r, r, 0, 90);
            gp.AddArc(0, c.Height - r, r, r, 90, 90);
            c.Region = new Region(gp);
        }
    }
}