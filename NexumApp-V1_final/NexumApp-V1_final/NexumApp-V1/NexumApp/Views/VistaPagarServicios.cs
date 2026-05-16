using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace NexumApp.Views
{
    public class VistaPagarServicios : UserControl
    {
        private static readonly Color AccentRed = Color.FromArgb(239, 68, 68);
        private static readonly Color AccentGreen = Color.FromArgb(16, 185, 129);

        private CultureInfo ES => Helpers.AppSettings.CultureMoneda;
        private Color C_BgPage => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(15, 20, 35) : Color.FromArgb(248, 250, 252);
        private Color C_White => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(30, 36, 56) : Color.White;
        private Color C_Border => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(45, 55, 80) : Color.FromArgb(226, 232, 240);
        private Color C_Text => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(241, 245, 249) : Color.FromArgb(30, 41, 59);
        private Color C_Muted => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);
        private Color C_Hover => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(45, 52, 82) : Color.FromArgb(241, 245, 249);

        private readonly CuentaService _cuentaService = new CuentaService();
        private readonly MovimientoService _movService = new MovimientoService();

        private Panel _pnlContainer;
        private Panel _pnlDetalle;
        private ComboBox _cmbCuenta;
        private Label _lblSaldo, _lblError, _lblServicioSel;
        private Button _btnPagar;
        private Button _btnGenerarPdf;
        private TextBox _txtReferencia, _txtMonto;
        private FlowLayoutPanel _gridServicios, _flpCompañias;
        private string _servicioSeleccionado = "";
        private string _compañiaSeleccionada = "";

        // Variables para guardar el último pago
        private decimal _ultimoImporte;
        private string _ultimoServicio;
        private string _ultimaCompañia;
        private string _ultimaReferencia;
        private CuentaBancaria _ultimaCuenta;

        private List<ServicioInfo> _servicios;

        public VistaPagarServicios()
        {
            Dock = DockStyle.Fill;
            DoubleBuffered = true;
            InicializarServicios();

            Helpers.AppSettings.ConfiguracionChanged += (s, e) => {
                if (IsDisposed) return;
                this.Invoke((MethodInvoker)delegate { BuildUI(); CargarCuentas(); });
            };
        }

        private void InicializarServicios()
        {
            _servicios = new List<ServicioInfo>
            {
                new ServicioInfo { Nombre = "Electricidad", Icono = "⚡", Descripcion = "Paga tu factura de luz",
                    Compañias = new List<CompañiaInfo> {
                        new CompañiaInfo { Nombre = "Iberdrola", Logo = "🔌", RefPrefijo = "IBE-", MontoSugerido = 85.50m },
                        new CompañiaInfo { Nombre = "Endesa", Logo = "🔋", RefPrefijo = "END-", MontoSugerido = 79.90m },
                        new CompañiaInfo { Nombre = "Naturgy", Logo = "💡", RefPrefijo = "NAT-", MontoSugerido = 72.30m }
                    }
                },
                new ServicioInfo { Nombre = "Agua", Icono = "💧", Descripcion = "Paga tu factura de agua",
                    Compañias = new List<CompañiaInfo> {
                        new CompañiaInfo { Nombre = "Canal Isabel II", Logo = "🚰", RefPrefijo = "CAN-", MontoSugerido = 45.20m },
                        new CompañiaInfo { Nombre = "Aigües Barcelona", Logo = "💧", RefPrefijo = "AIG-", MontoSugerido = 52.80m }
                    }
                },
                new ServicioInfo { Nombre = "Internet", Icono = "🌐", Descripcion = "Fibra y móvil",
                    Compañias = new List<CompañiaInfo> {
                        new CompañiaInfo { Nombre = "Movistar", Logo = "📡", RefPrefijo = "MOV-", MontoSugerido = 49.90m },
                        new CompañiaInfo { Nombre = "Orange", Logo = "🍊", RefPrefijo = "ORA-", MontoSugerido = 44.95m },
                        new CompañiaInfo { Nombre = "Vodafone", Logo = "📱", RefPrefijo = "VOD-", MontoSugerido = 42.90m }
                    }
                },
                new ServicioInfo { Nombre = "Streaming", Icono = "🎬", Descripcion = "Netflix, HBO, Disney+",
                    Compañias = new List<CompañiaInfo> {
                        new CompañiaInfo { Nombre = "Netflix", Logo = "📺", RefPrefijo = "NFLX-", MontoSugerido = 17.99m },
                        new CompañiaInfo { Nombre = "Disney+", Logo = "✨", RefPrefijo = "DIS-", MontoSugerido = 10.99m }
                    }
                },
                new ServicioInfo { Nombre = "Música", Icono = "🎵", Descripcion = "Spotify, Apple Music",
                    Compañias = new List<CompañiaInfo> {
                        new CompañiaInfo { Nombre = "Spotify", Logo = "🎧", RefPrefijo = "SPT-", MontoSugerido = 10.99m }
                    }
                },
                new ServicioInfo { Nombre = "Seguros", Icono = "🛡️", Descripcion = "Hogar, vida, coche",
                    Compañias = new List<CompañiaInfo> {
                        new CompañiaInfo { Nombre = "Mapfre", Logo = "🏛️", RefPrefijo = "MAP-", MontoSugerido = 120.00m }
                    }
                }
            };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            BuildUI();
            CargarCuentas();
        }

        private void BuildUI()
        {
            Controls.Clear();
            BackColor = C_BgPage;

            var rootScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            Controls.Add(rootScroll);

            _pnlContainer = new Panel
            {
                Width = 950,
                Location = new Point((Width - 950) / 2, 20),
                BackColor = Color.Transparent,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            rootScroll.Controls.Add(_pnlContainer);

            rootScroll.Resize += (s, e) => {
                _pnlContainer.Left = Math.Max(20, (rootScroll.ClientSize.Width - _pnlContainer.Width) / 2);
            };

            int y = 0;

            var pnlHeader = CrearCard(0, y, 950, 80);
            pnlHeader.Controls.Add(new Label { Text = "💳 Pagar servicios", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = C_Text, Location = new Point(25, 15), AutoSize = true });
            pnlHeader.Controls.Add(new Label { Text = "Paga tus facturas de forma rápida y segura", Font = new Font("Segoe UI", 10), ForeColor = C_Muted, Location = new Point(25, 48), AutoSize = true });
            _pnlContainer.Controls.Add(pnlHeader);
            y += 100;

            var pnlCuenta = CrearCard(0, y, 950, 100);
            pnlCuenta.Controls.Add(new Label { Text = "CUENTA DE CARGO", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = C_Muted, Location = new Point(25, 15), AutoSize = true });
            _cmbCuenta = new ComboBox { Location = new Point(25, 38), Size = new Size(400, 30), DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = C_BgPage, ForeColor = C_Text, Font = new Font("Segoe UI", 10) };
            _cmbCuenta.SelectedIndexChanged += (s, e) => ActualizarSaldo();
            _lblSaldo = new Label { Location = new Point(25, 72), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = AccentGreen };
            pnlCuenta.Controls.AddRange(new Control[] { _cmbCuenta, _lblSaldo });
            _pnlContainer.Controls.Add(pnlCuenta);
            y += 120;

            _pnlContainer.Controls.Add(new Label { Text = "Selecciona un servicio", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = C_Text, Location = new Point(0, y), AutoSize = true });
            y += 35;

            _gridServicios = new FlowLayoutPanel
            {
                Location = new Point(0, y),
                Width = 965,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            foreach (var serv in _servicios) _gridServicios.Controls.Add(CrearCardServicio(serv));
            _pnlContainer.Controls.Add(_gridServicios);

            _gridServicios.Layout += (s, e) => {
                if (_pnlDetalle != null) _pnlDetalle.Top = _gridServicios.Bottom + 20;
            };
            y = _gridServicios.Bottom + 20;

            _pnlDetalle = CrearCard(0, y, 950, 450);
            _pnlDetalle.Visible = false;
            _pnlContainer.Controls.Add(_pnlDetalle);

            ConstruirFormularioDetalle();
        }

        private Panel CrearCardServicio(ServicioInfo servicio)
        {
            var card = new Panel { Size = new Size(225, 120), BackColor = C_White, Margin = new Padding(0, 0, 15, 15), Cursor = Cursors.Hand };

            var icon = new Label { Text = servicio.Icono, Font = new Font("Segoe UI", 26), Location = new Point(20, 15), AutoSize = true };
            var name = new Label { Text = servicio.Nombre, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = C_Text, Location = new Point(20, 60), AutoSize = true };
            var desc = new Label { Text = servicio.Descripcion, Font = new Font("Segoe UI", 8), ForeColor = C_Muted, Location = new Point(20, 85), AutoSize = true };

            card.Controls.AddRange(new Control[] { icon, name, desc });

            card.MouseEnter += (s, e) => { card.BackColor = C_Hover; card.Invalidate(); };
            card.MouseLeave += (s, e) => { card.BackColor = C_White; card.Invalidate(); };

            foreach (Control c in card.Controls) c.Click += (s, e) => SeleccionarServicio(servicio);
            card.Click += (s, e) => SeleccionarServicio(servicio);

            card.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = GetRoundPath(card.ClientRectangle, 12))
                using (var pen = new Pen(C_Border, 1))
                    e.Graphics.DrawPath(pen, path);
            };

            return card;
        }

        private void ConstruirFormularioDetalle()
        {
            int iy = 25;
            _lblServicioSel = new Label { Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = C_Text, Location = new Point(25, iy), AutoSize = true };
            _pnlDetalle.Controls.Add(_lblServicioSel);
            iy += 45;

            _pnlDetalle.Controls.Add(new Label { Text = "COMPAÑÍA / PROVEEDOR", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = C_Muted, Location = new Point(25, iy), AutoSize = true });
            iy += 22;

            _flpCompañias = new FlowLayoutPanel { Location = new Point(25, iy), Size = new Size(900, 70), FlowDirection = FlowDirection.LeftToRight };
            _pnlDetalle.Controls.Add(_flpCompañias);
            iy += 85;

            // Referencia e Importe
            _pnlDetalle.Controls.Add(new Label { Text = "REFERENCIA / Nº CONTRATO", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = C_Muted, Location = new Point(25, iy), AutoSize = true });
            _pnlDetalle.Controls.Add(new Label { Text = "IMPORTE (€)", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = C_Muted, Location = new Point(400, iy), AutoSize = true });
            iy += 22;

            _txtReferencia = new TextBox { Location = new Point(25, iy), Size = new Size(350, 35), Font = new Font("Segoe UI", 12), BackColor = C_BgPage, ForeColor = C_Text, BorderStyle = BorderStyle.FixedSingle };
            _txtMonto = new TextBox { Location = new Point(400, iy), Size = new Size(180, 35), Font = new Font("Segoe UI", 14, FontStyle.Bold), BackColor = C_BgPage, ForeColor = AccentRed, BorderStyle = BorderStyle.FixedSingle, Text = "0,00" };
            _pnlDetalle.Controls.AddRange(new Control[] { _txtReferencia, _txtMonto });
            iy += 60;

            _lblError = new Label { Location = new Point(25, iy), ForeColor = AccentRed, Font = new Font("Segoe UI", 9), AutoSize = true, Visible = false };
            _pnlDetalle.Controls.Add(_lblError);
            iy += 30;

            // Panel para los botones (horizontal)
            FlowLayoutPanel pnlBotones = new FlowLayoutPanel
            {
                Location = new Point(25, iy),
                Size = new Size(900, 50),
                FlowDirection = FlowDirection.LeftToRight
            };

            _btnPagar = new Button
            {
                Text = "✅ Realizar Pago",
                Size = new Size(220, 50),
                BackColor = AccentRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 15, 0)
            };
            _btnPagar.FlatAppearance.BorderSize = 0;
            _btnPagar.Click += BtnPagar_Click;

            _btnGenerarPdf = new Button
            {
                Text = "📄 Generar Recibo (PDF)",
                Size = new Size(200, 50),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _btnGenerarPdf.FlatAppearance.BorderSize = 0;
            _btnGenerarPdf.Click += BtnGenerarPdf_Click;

            pnlBotones.Controls.AddRange(new Control[] { _btnPagar, _btnGenerarPdf });
            _pnlDetalle.Controls.Add(pnlBotones);
        }

        private void SeleccionarServicio(ServicioInfo servicio)
        {
            _servicioSeleccionado = servicio.Nombre;
            _lblServicioSel.Text = $"{servicio.Icono} Pagar {servicio.Nombre}";
            _pnlDetalle.Visible = true;
            _flpCompañias.Controls.Clear();
            _lblError.Visible = false;

            foreach (var comp in servicio.Compañias)
            {
                var btn = new Button
                {
                    Text = $"{comp.Logo} {comp.Nombre}",
                    Size = new Size(160, 50),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = C_BgPage,
                    ForeColor = C_Text,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Margin = new Padding(0, 0, 10, 0),
                    Tag = comp
                };
                btn.FlatAppearance.BorderColor = C_Border;
                btn.Click += (s, e) => {
                    var compañia = (CompañiaInfo)((Button)s).Tag;
                    _compañiaSeleccionada = compañia.Nombre;
                    _txtReferencia.Text = compañia.RefPrefijo + DateTime.Now.ToString("yyyyMMddHHmmss");
                    _txtMonto.Text = compañia.MontoSugerido.ToString("N2", ES);
                    foreach (Button b in _flpCompañias.Controls) b.BackColor = C_BgPage;
                    btn.BackColor = C_Hover;
                };
                _flpCompañias.Controls.Add(btn);
            }
        }

        private async void BtnPagar_Click(object sender, EventArgs e)
        {
            _lblError.Visible = false;

            if (string.IsNullOrEmpty(_compañiaSeleccionada))
            {
                _lblError.Text = "Selecciona una compañía.";
                _lblError.Visible = true;
                return;
            }

            if (!decimal.TryParse(_txtMonto.Text, NumberStyles.Any, ES, out decimal importe))
            {
                _lblError.Text = "Importe no válido. Use formato válido (ej: 10,99)";
                _lblError.Visible = true;
                return;
            }

            if (importe <= 0)
            {
                _lblError.Text = "El importe debe ser mayor que cero.";
                _lblError.Visible = true;
                return;
            }

            if (_cmbCuenta.SelectedItem == null)
            {
                _lblError.Text = "Selecciona una cuenta.";
                _lblError.Visible = true;
                return;
            }

            CuentaBancaria cuentaSeleccionada = (CuentaBancaria)_cmbCuenta.SelectedItem;
            int idDeLaCuenta = cuentaSeleccionada.Id;

            decimal saldoReal = _cuentaService.ObtenerSaldoCuenta(idDeLaCuenta);

            if (saldoReal < importe)
            {
                _lblError.Text = $"Saldo insuficiente. Saldo disponible: {saldoReal.ToString("C2", ES)}";
                _lblError.Visible = true;
                return;
            }

            _btnPagar.Enabled = false;
            _btnPagar.Text = "⏳ Procesando...";
            _btnGenerarPdf.Enabled = false;

            try
            {
                string errorSql;
                bool exito = _movService.RegistrarPagoServicio(idDeLaCuenta, importe, _servicioSeleccionado, _compañiaSeleccionada, out errorSql);

                if (exito)
                {
                    _ultimoImporte = importe;
                    _ultimoServicio = _servicioSeleccionado;
                    _ultimaCompañia = _compañiaSeleccionada;
                    _ultimaReferencia = _txtReferencia.Text;
                    _ultimaCuenta = cuentaSeleccionada;

                    MessageBox.Show($"¡Pago de {_servicioSeleccionado} - {_compañiaSeleccionada} realizado con éxito!\n\nImporte: {importe.ToString("C2", ES)}",
                        "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    CargarCuentas();
                    _servicioSeleccionado = "";
                    _compañiaSeleccionada = "";
                    _pnlDetalle.Visible = false;

                    foreach (Control card in _gridServicios.Controls)
                    {
                        card.BackColor = C_White;
                    }

                    _btnGenerarPdf.Enabled = true;
                    _btnGenerarPdf.BackColor = Color.FromArgb(34, 197, 94);

                    PagoRealizado?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    _lblError.Text = "❌ " + errorSql;
                    _lblError.Visible = true;
                    _btnGenerarPdf.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                _lblError.Text = "❌ Error inesperado: " + ex.Message;
                _lblError.Visible = true;
            }
            finally
            {
                _btnPagar.Enabled = true;
                _btnPagar.Text = "✅ Realizar Pago";
            }
        }

        private void BtnGenerarPdf_Click(object sender, EventArgs e)
        {
            try
            {
                if (_ultimaCuenta == null)
                {
                    MessageBox.Show("No hay un pago reciente para generar el recibo.", "Información",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var pdfService = new ReciboPdfService();
                string rutaPdf = pdfService.GenerarReciboPago(
                    _ultimaCuenta,
                    _ultimoServicio,
                    _ultimaCompañia,
                    _ultimaReferencia,
                    _ultimoImporte,
                    DateTime.Now
                );

                DialogResult result = MessageBox.Show(
                    $"Recibo generado exitosamente.\n\nUbicación: {rutaPdf}\n\n¿Desea abrirlo ahora?",
                    "PDF Generado",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = rutaPdf,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el recibo: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public event EventHandler PagoRealizado;
        protected virtual void OnPagoRealizado(EventArgs e) => PagoRealizado?.Invoke(this, e);

        private Panel CrearCard(int x, int y, int w, int h)
        {
            var p = new Panel { Location = new Point(x, y), Size = new Size(w, h), BackColor = C_White };
            p.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = GetRoundPath(p.ClientRectangle, 12))
                using (var pen = new Pen(C_Border, 1))
                    e.Graphics.DrawPath(pen, path);
            };
            return p;
        }

        private GraphicsPath GetRoundPath(Rectangle r, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void CargarCuentas()
        {
            _cmbCuenta.Items.Clear();
            if (SesionActual.Instancia?.Usuario == null) return;

            var cuentas = _cuentaService.ObtenerCuentasPorUsuario(SesionActual.Instancia.Usuario.Id);

            foreach (var c in cuentas)
            {
                _cmbCuenta.Items.Add(c);
            }

            _cmbCuenta.DisplayMember = "DisplayText";
            _cmbCuenta.ValueMember = "Id";

            if (_cmbCuenta.Items.Count > 0)
            {
                _cmbCuenta.SelectedIndex = 0;
                ActualizarSaldo();
            }
        }

        private void ActualizarSaldo()
        {
            if (_cmbCuenta.SelectedItem is CuentaBancaria cuenta)
            {
                _lblSaldo.Text = $"💰 Saldo disponible: {cuenta.Saldo.ToString("C2", ES)}";
            }
        }
    }

    public class ServicioInfo
    {
        public string Nombre { get; set; }
        public string Icono { get; set; }
        public string Descripcion { get; set; }
        public List<CompañiaInfo> Compañias { get; set; }
    }

    public class CompañiaInfo
    {
        public string Nombre { get; set; }
        public string Logo { get; set; }
        public string RefPrefijo { get; set; }
        public decimal MontoSugerido { get; set; }
    }
}