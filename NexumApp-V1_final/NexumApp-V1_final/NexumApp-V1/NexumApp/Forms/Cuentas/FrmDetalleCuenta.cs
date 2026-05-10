using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NexumApp.Forms.Cuentas
{
    internal partial class FrmDetalleCuenta : Form
    {
        private readonly CuentaBancaria _cuenta;
        private readonly CuentaService _cuentaService = new CuentaService();

        // Colores Fintech (Azul y Dorado)
        private readonly Color BluePrimary = Color.FromArgb(26, 115, 232);
        private readonly Color BlueDark = Color.FromArgb(10, 50, 120);
        private readonly Color BackgroundGray = Color.FromArgb(248, 249, 252);

        private Panel cardDetalle;

        public FrmDetalleCuenta(CuentaBancaria cuenta)
        {
            InitializeComponent();
            _cuenta = cuenta;
            this.Load += FrmDetalleCuenta_Load;
        }

        private void FrmDetalleCuenta_Load(object sender, EventArgs e)
        {
            if (_cuenta == null) { this.Close(); return; }
            ConfigurarEstiloBase();
            DisenarInterfazModerna();
            ActualizarDatosEnCard();
        }

        private void ConfigurarEstiloBase()
        {
            // TAMAÑO AMPLIADO: 600 de ancho por 800 de alto
            this.Size = new Size(600, 800);
            this.BackColor = BackgroundGray;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Detalle de Cuenta | Nexum Bank";
        }

        private void DisenarInterfazModerna()
        {
            this.Controls.Clear();

            // Título más grande y con más margen
            Label lblHeader = new Label
            {
                Text = "Información de Cuenta",
                ForeColor = BlueDark,
                Font = new Font("Segoe UI Semibold", 22, FontStyle.Bold),
                Location = new Point(50, 40),
                AutoSize = true
            };

            // Tarjeta central ampliada
            cardDetalle = new Panel
            {
                Size = new Size(500, 380),
                Location = new Point(50, 110),
                BackColor = Color.White
            };

            // Botón Ingresar (Más alto y ancho)
            Button btnIngresarAccion = new Button
            {
                Text = "📥 INGRESAR EFECTIVO",
                Size = new Size(500, 65),
                Location = new Point(50, 520),
                BackColor = BluePrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnIngresarAccion.FlatAppearance.BorderSize = 0;
            btnIngresarAccion.Click += (s, ev) => AbrirIngreso();

            // Botón Retirar (Más alto y ancho)
            Button btnRetirarAccion = new Button
            {
                Text = "📤 RETIRAR EFECTIVO",
                Size = new Size(500, 65),
                Location = new Point(50, 600),
                BackColor = Color.White,
                ForeColor = BluePrimary,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRetirarAccion.FlatAppearance.BorderColor = BluePrimary;
            btnRetirarAccion.FlatAppearance.BorderSize = 2;
            btnRetirarAccion.Click += (s, ev) => AbrirRetiro();

            this.Controls.Add(lblHeader);
            this.Controls.Add(cardDetalle);
            this.Controls.Add(btnIngresarAccion);
            this.Controls.Add(btnRetirarAccion);

            // Aplicar redondeos con radios proporcionales al tamaño
            this.BeginInvoke(new Action(() => {
                Redondear(cardDetalle, 30);
                Redondear(btnIngresarAccion, 20);
                Redondear(btnRetirarAccion, 20);
            }));
        }

        private void ActualizarDatosEnCard()
        {
            cardDetalle.Controls.Clear();

            // Barra superior decorativa
            Panel topBar = new Panel { Size = new Size(500, 10), BackColor = BluePrimary, Dock = DockStyle.Top };
            cardDetalle.Controls.Add(topBar);

            // Datos con mejor espaciado (startY y spacing ajustados)
            int startY = 50;
            int spacing = 80;

            DibujarDato("TIPO DE PRODUCTO", _cuenta.TipoCuenta.ToUpper(), startY);
            DibujarDato("NÚMERO DE CUENTA (IBAN)", _cuenta.NumeroCuenta, startY + spacing);
            DibujarDato("FECHA DE CREACIÓN", _cuenta.FechaApertura.ToString("dd MMMM, yyyy"), startY + (spacing * 2));

            // Sección de Saldo destacada al final de la tarjeta
            Label lblSValue = new Label
            {
                Text = _cuenta.Saldo.ToString("C2"),
                Size = new Size(500, 80),
                Location = new Point(0, 280),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = BluePrimary,
                Font = new Font("Segoe UI", 34, FontStyle.Bold)
            };

            Label lblSTitle = new Label
            {
                Text = "BALANCE TOTAL",
                Size = new Size(500, 20),
                Location = new Point(0, 260),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkGray,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            cardDetalle.Controls.Add(lblSTitle);
            cardDetalle.Controls.Add(lblSValue);
        }

        private void DibujarDato(string tit, string val, int y)
        {
            // Etiquetas más legibles
            Label lblT = new Label
            {
                Text = tit,
                Location = new Point(40, y),
                ForeColor = Color.FromArgb(160, 170, 180),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = true
            };
            Label lblV = new Label
            {
                Text = val,
                Location = new Point(40, y + 25),
                ForeColor = Color.FromArgb(60, 70, 80),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true
            };
            cardDetalle.Controls.Add(lblT);
            cardDetalle.Controls.Add(lblV);
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

        private void AbrirIngreso()
        {
            using (var frm = new Movimientos.FrmIngresarEfectivo(_cuenta))
            {
                if (frm.ShowDialog() == DialogResult.OK) Refrescar();
            }
        }

        private void AbrirRetiro()
        {
            using (var frm = new Movimientos.FrmRetirarEfectivo(_cuenta))
            {
                if (frm.ShowDialog() == DialogResult.OK) Refrescar();
            }
        }

        private void Refrescar()
        {
            var cAct = _cuentaService.ObtenerCuentaPorId(_cuenta.Id);
            if (cAct != null)
            {
                _cuenta.Saldo = cAct.Saldo;
                ActualizarDatosEnCard();
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}