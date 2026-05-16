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
    public class VistaHistorialMovimientos : UserControl
    {
        private readonly MovimientoService _movimientoService = new MovimientoService();
        private readonly CuentaService _cuentaService = new CuentaService();

        // Paleta de colores claros y modernos
        private static readonly Color BgPage = Color.FromArgb(248, 250, 252);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BgCardHover = Color.FromArgb(241, 245, 249);
        private static readonly Color AccentPrimary = Color.FromArgb(99, 102, 241);
        private static readonly Color AccentSuccess = Color.FromArgb(16, 185, 129);
        private static readonly Color AccentDanger = Color.FromArgb(239, 68, 68);
        private static readonly Color AccentWarning = Color.FromArgb(245, 158, 11);
        private static readonly Color TextPrimary = Color.FromArgb(30, 41, 59);
        private static readonly Color TextSecondary = Color.FromArgb(71, 85, 105);
        private static readonly Color TextMuted = Color.FromArgb(148, 163, 184);
        private static readonly Color BorderColor = Color.FromArgb(226, 232, 240);

        private ComboBox _cmbCuentas;
        private ComboBox _cmbRangoFechas;
        private DateTimePicker _dtpDesde, _dtpHasta;
        private Button _btnAplicarFiltros, _btnLimpiar;
        private TextBox _txtBuscar;
        private FlowLayoutPanel _movimientosContainer;
        private Label _lblSaldoValor, _lblIngresosValor, _lblGastosValor, _lblMovimientosValor;

        private List<Movimiento> _movimientos = new List<Movimiento>();
        private List<Movimiento> _movimientosFiltrados = new List<Movimiento>();
        private string _filtroBusqueda = "";
        private string _filtroTiempo = "TODO";
        private DateTime _fechaDesde = DateTime.Now.AddMonths(-1);
        private DateTime _fechaHasta = DateTime.Now;

        public VistaHistorialMovimientos()
        {
            BackColor = BgPage;
            Dock = DockStyle.Fill;
            DoubleBuffered = true;
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

            // Panel principal con scroll
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = BgPage };
            var main = new Panel { Width = 1200, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent };
            scroll.Controls.Add(main);
            scroll.Resize += (s, ev) => { main.Left = Math.Max(20, (scroll.ClientSize.Width - 1200) / 2); };
            Controls.Add(scroll);

            int y = 24;

            // ========== HEADER ==========
            var headerPanel = CrearCard(new Rectangle(0, y, 1160, 70));
            headerPanel.BackColor = BgCard;

            var icono = new Label { Text = "📋", Font = new Font("Segoe UI Emoji", 28), Location = new Point(20, 18), AutoSize = true };
            var titulo = new Label { Text = "Historial de movimientos", ForeColor = TextPrimary, Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(95, 12), AutoSize = true };
            var subtitulo = new Label { Text = "Consulta y filtra todas tus transacciones", ForeColor = TextSecondary, Font = new Font("Segoe UI", 11), Location = new Point(95, 44), AutoSize = true };
            headerPanel.Controls.AddRange(new Control[] { icono, titulo, subtitulo });
            main.Controls.Add(headerPanel);
            y += 85;

            // ========== SELECCIÓN DE CUENTA ==========
            var cuentaPanel = CrearCard(new Rectangle(0, y, 1160, 65));
            cuentaPanel.BackColor = BgCard;

            var lblCuenta = new Label { Text = "Cuenta:", ForeColor = TextSecondary, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, 22), AutoSize = true };
            _cmbCuentas = new ComboBox
            {
                Size = new Size(300, 36),
                Location = new Point(90, 16),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                BackColor = BgPage,
                ForeColor = TextPrimary,
                FlatStyle = FlatStyle.Flat
            };
            _cmbCuentas.SelectedIndexChanged += (s, e) => CargarMovimientos();

            cuentaPanel.Controls.AddRange(new Control[] { lblCuenta, _cmbCuentas });
            main.Controls.Add(cuentaPanel);
            y += 80;

            // ========== FILTROS ==========
            var filtrosPanel = CrearCard(new Rectangle(0, y, 1160, 100));
            filtrosPanel.BackColor = BgCard;

            // Rápido
            var lblRapido = new Label { Text = "RÁPIDO:", ForeColor = TextMuted, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };
            _cmbRangoFechas = new ComboBox
            {
                Size = new Size(160, 32),
                Location = new Point(20, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                BackColor = BgPage,
                ForeColor = TextPrimary,
                FlatStyle = FlatStyle.Flat
            };
            _cmbRangoFechas.Items.AddRange(new string[] { "Hoy", "Esta semana", "Este mes", "Este trimestre", "Este año", "Personalizado" });
            _cmbRangoFechas.SelectedIndex = 2;
            _cmbRangoFechas.SelectedIndexChanged += (s, e) => AplicarRapido();

            // Personalizado
            var lblDesde = new Label { Text = "DESDE:", ForeColor = TextMuted, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(210, 15), AutoSize = true };
            _dtpDesde = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Size = new Size(110, 28),
                Location = new Point(210, 35),
                Value = _fechaDesde,
                BackColor = BgPage,
                ForeColor = TextPrimary
            };

            var lblHasta = new Label { Text = "HASTA:", ForeColor = TextMuted, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(340, 15), AutoSize = true };
            _dtpHasta = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Size = new Size(110, 28),
                Location = new Point(340, 35),
                Value = _fechaHasta,
                BackColor = BgPage,
                ForeColor = TextPrimary
            };

            // Búsqueda
            var lblBuscar = new Label { Text = "🔍", Font = new Font("Segoe UI", 12), Location = new Point(480, 38), AutoSize = true };
            _txtBuscar = new TextBox
            {
                Size = new Size(250, 32),
                Location = new Point(505, 35),
                Font = new Font("Segoe UI", 10),
                BackColor = BgPage,
                ForeColor = TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Text = ""
            };
            _txtBuscar.TextChanged += (s, e) => AplicarFiltros();

            // Botones
            _btnAplicarFiltros = CrearBoton("Aplicar", AccentPrimary, 50, 32);
            _btnAplicarFiltros.Location = new Point(790, 34);
            _btnAplicarFiltros.Click += (s, e) => AplicarFiltrosPersonalizados();

            _btnLimpiar = CrearBoton("Limpiar", TextMuted, 50, 32);
            _btnLimpiar.Location = new Point(880, 34);
            _btnLimpiar.Click += (s, e) => LimpiarFiltros();

            filtrosPanel.Controls.AddRange(new Control[] {
                lblRapido, _cmbRangoFechas, lblDesde, _dtpDesde, lblHasta, _dtpHasta,
                lblBuscar, _txtBuscar, _btnAplicarFiltros, _btnLimpiar
            });
            main.Controls.Add(filtrosPanel);
            y += 115;

            // ========== TARJETAS ESTADÍSTICAS ==========
            var statsPanel = new Panel { Location = new Point(0, y), Size = new Size(1160, 100), BackColor = Color.Transparent };

            var cardSaldo = CrearStatCard("💰 SALDO", ref _lblSaldoValor, AccentPrimary);
            var cardIngresos = CrearStatCard("📈 INGRESOS", ref _lblIngresosValor, AccentSuccess);
            var cardGastos = CrearStatCard("📉 GASTOS", ref _lblGastosValor, AccentDanger);
            var cardMovimientos = CrearStatCard("🔄 MOVIMIENTOS", ref _lblMovimientosValor, AccentWarning);

            cardSaldo.Location = new Point(0, 0);
            cardIngresos.Location = new Point(285, 0);
            cardGastos.Location = new Point(570, 0);
            cardMovimientos.Location = new Point(855, 0);

            statsPanel.Controls.AddRange(new Control[] { cardSaldo, cardIngresos, cardGastos, cardMovimientos });
            main.Controls.Add(statsPanel);
            y += 115;

            // ========== LISTA DE MOVIMIENTOS ==========
            var listaHeader = new Label
            {
                Text = "📋 MOVIMIENTOS RECIENTES",
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Location = new Point(0, y),
                AutoSize = true
            };
            main.Controls.Add(listaHeader);
            y += 35;

            // Cabecera de columnas
            var columnasHeader = new Panel { Location = new Point(0, y), Size = new Size(1160, 36), BackColor = BgCard };
            RedondearEsquinas(columnasHeader, 8);

            var tlpHeader = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Padding = new Padding(16, 0, 16, 0) };
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));

            string[] headers = { "FECHA", "CONCEPTO", "TIPO", "IMPORTE" };
            for (int i = 0; i < headers.Length; i++)
            {
                var lbl = new Label
                {
                    Text = headers[i],
                    ForeColor = TextMuted,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    TextAlign = i == 3 ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill
                };
                tlpHeader.Controls.Add(lbl, i, 0);
            }
            columnasHeader.Controls.Add(tlpHeader);
            main.Controls.Add(columnasHeader);
            y += 36;

            // Contenedor de movimientos
            _movimientosContainer = new FlowLayoutPanel
            {
                Location = new Point(0, y),
                Size = new Size(1160, 400),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };
            main.Controls.Add(_movimientosContainer);
            y += 415;

            main.Height = y + 30;
        }

        private Panel CrearCard(Rectangle rect)
        {
            var card = new Panel { Location = rect.Location, Size = rect.Size, BackColor = BgCard };
            RedondearEsquinas(card, 12);
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(BorderColor, 1))
                using (var path = RoundedRect(card.ClientRectangle, 12))
                    e.Graphics.DrawPath(pen, path);
            };
            return card;
        }

        private Panel CrearStatCard(string titulo, ref Label valorLabel, Color color)
        {
            var card = new Panel { Size = new Size(275, 90), BackColor = BgCard };
            RedondearEsquinas(card, 10);
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(30, color.R, color.G, color.B), 1))
                using (var path = RoundedRect(card.ClientRectangle, 10))
                    e.Graphics.DrawPath(pen, path);
            };

            var lblTitulo = new Label
            {
                Text = titulo,
                ForeColor = TextSecondary,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(16, 14),
                AutoSize = true
            };

            valorLabel = new Label
            {
                Text = "€0,00",
                ForeColor = color,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(16, 42),
                AutoSize = true
            };

            card.Controls.AddRange(new Control[] { lblTitulo, valorLabel });
            return card;
        }

        private Button CrearBoton(string texto, Color color, int ancho, int alto)
        {
            var btn = new Button
            {
                Text = texto,
                Size = new Size(ancho, alto),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            RedondearEsquinas(btn, 6);
            return btn;
        }

        private void CargarCuentas()
        {
            if (!SesionActual.Instancia?.EstaLogeado ?? true) return;
            var cuentas = _cuentaService.ObtenerCuentasPorUsuario(SesionActual.Instancia.Usuario.Id);

            _cmbCuentas.Items.Clear();
            foreach (var c in cuentas)
            {
                string numeroMostrar = c.NumeroCuenta?.Length > 4
                    ? "•••• " + c.NumeroCuenta.Substring(c.NumeroCuenta.Length - 4)
                    : "•••• ••••";
                _cmbCuentas.Items.Add(new { Id = c.Id, Display = $"{c.TipoCuenta ?? "Cuenta"} • {numeroMostrar}" });
            }
            _cmbCuentas.DisplayMember = "Display";
            _cmbCuentas.ValueMember = "Id";

            if (_cmbCuentas.Items.Count > 0)
            {
                _cmbCuentas.SelectedIndex = 0;
                CargarMovimientos();
            }
        }

        private void CargarMovimientos()
        {
            if (_cmbCuentas.SelectedItem == null) return;
            var cuentaId = (int)_cmbCuentas.SelectedItem.GetType().GetProperty("Id").GetValue(_cmbCuentas.SelectedItem, null);

            try
            {
                _movimientos = _movimientoService.ObtenerMovimientosPorCuenta(cuentaId, 2000) ?? new List<Movimiento>();
                _movimientos = _movimientos.OrderByDescending(m => m.Fecha).ToList();
            }
            catch { _movimientos = new List<Movimiento>(); }

            AplicarFiltros();
        }

        private void AplicarRapido()
        {
            var ahora = DateTime.Now;
            switch (_cmbRangoFechas.SelectedIndex)
            {
                case 0: // Hoy
                    _fechaDesde = ahora.Date;
                    _fechaHasta = ahora.Date;
                    break;
                case 1: // Semana
                    _fechaDesde = ahora.AddDays(-7);
                    _fechaHasta = ahora;
                    break;
                case 2: // Mes
                    _fechaDesde = new DateTime(ahora.Year, ahora.Month, 1);
                    _fechaHasta = ahora;
                    break;
                case 3: // Trimestre
                    int trimestre = (ahora.Month - 1) / 3;
                    _fechaDesde = new DateTime(ahora.Year, trimestre * 3 + 1, 1);
                    _fechaHasta = ahora;
                    break;
                case 4: // Año
                    _fechaDesde = new DateTime(ahora.Year, 1, 1);
                    _fechaHasta = ahora;
                    break;
                case 5: // Personalizado
                    return;
            }
            _dtpDesde.Value = _fechaDesde;
            _dtpHasta.Value = _fechaHasta;
            AplicarFiltros();
        }

        private void AplicarFiltrosPersonalizados()
        {
            _fechaDesde = _dtpDesde.Value.Date;
            _fechaHasta = _dtpHasta.Value.Date;
            if (_cmbRangoFechas.SelectedIndex != 5)
                _cmbRangoFechas.SelectedIndex = 5;
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            if (_movimientos == null) return;

            // Filtro por fecha
            var filtrados = _movimientos
                .Where(m => m.Fecha.Date >= _fechaDesde.Date && m.Fecha.Date <= _fechaHasta.Date)
                .ToList();

            // Filtro por búsqueda
            if (!string.IsNullOrEmpty(_txtBuscar.Text))
            {
                var busqueda = _txtBuscar.Text.ToLower();
                filtrados = filtrados.Where(m =>
                    (m.Concepto ?? "").ToLower().Contains(busqueda) ||
                    (m.TipoMovimiento ?? "").ToLower().Contains(busqueda)).ToList();
            }

            _movimientosFiltrados = filtrados;
            ActualizarStats();
            RenderMovimientos();
        }

        private void LimpiarFiltros()
        {
            _txtBuscar.Text = "";
            _cmbRangoFechas.SelectedIndex = 2; // Este mes
            _fechaDesde = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            _fechaHasta = DateTime.Now;
            _dtpDesde.Value = _fechaDesde;
            _dtpHasta.Value = _fechaHasta;
            AplicarFiltros();
        }

        private void ActualizarStats()
        {
            decimal ingresos = _movimientosFiltrados
                .Where(m => m.TipoMovimiento == "Ingreso" || m.TipoMovimiento?.Contains("Recibida") == true)
                .Sum(m => m.Monto);
            decimal gastos = _movimientosFiltrados
                .Where(m => m.TipoMovimiento != "Ingreso" && !m.TipoMovimiento?.Contains("Recibida") == true)
                .Sum(m => m.Monto);
            decimal saldo = ingresos - gastos;

            _lblSaldoValor.Text = saldo.ToString("C2", CultureInfo.CreateSpecificCulture("es-ES"));
            _lblIngresosValor.Text = ingresos.ToString("C2", CultureInfo.CreateSpecificCulture("es-ES"));
            _lblGastosValor.Text = gastos.ToString("C2", CultureInfo.CreateSpecificCulture("es-ES"));
            _lblMovimientosValor.Text = _movimientosFiltrados.Count.ToString("N0");

            _lblSaldoValor.ForeColor = saldo >= 0 ? AccentSuccess : AccentDanger;
        }

        private void RenderMovimientos()
        {
            _movimientosContainer.Controls.Clear();

            if (_movimientosFiltrados.Count == 0)
            {
                var empty = new Label
                {
                    Text = "✨ No hay movimientos para mostrar con los filtros seleccionados.",
                    ForeColor = TextSecondary,
                    Font = new Font("Segoe UI", 12),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(1160, 100)
                };
                _movimientosContainer.Controls.Add(empty);
                return;
            }

            foreach (var mov in _movimientosFiltrados)
            {
                _movimientosContainer.Controls.Add(CrearFilaMovimiento(mov));
            }
        }

        private Panel CrearFilaMovimiento(Movimiento mov)
        {
            bool esIngreso = mov.TipoMovimiento == "Ingreso" || mov.TipoMovimiento?.Contains("Recibida") == true;
            Color colorMov = esIngreso ? AccentSuccess : AccentDanger;
            string signo = esIngreso ? "+" : "−";
            string icono = esIngreso ? "📥" : "📤";

            var fila = new Panel
            {
                Size = new Size(1160, 52),
                BackColor = BgCard,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 1)
            };

            // Hover
            fila.MouseEnter += (s, e) => fila.BackColor = BgCardHover;
            fila.MouseLeave += (s, e) => fila.BackColor = BgCard;

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(16, 0, 16, 0)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));

            // Fecha
            tlp.Controls.Add(new Label
            {
                Text = mov.Fecha.ToString("dd MMM yyyy", CultureInfo.CreateSpecificCulture("es-ES")),
                ForeColor = TextSecondary,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            // Concepto + icono
            var conceptoPanel = new Panel { Dock = DockStyle.Fill };
            var iconoLabel = new Label { Text = icono, Font = new Font("Segoe UI Emoji", 12), Location = new Point(0, 16), AutoSize = true };
            var conceptoLabel = new Label
            {
                Text = mov.Concepto ?? "—",
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 10),
                Location = new Point(28, 16),
                Size = new Size(500, 20),
                AutoEllipsis = true
            };
            conceptoPanel.Controls.AddRange(new Control[] { iconoLabel, conceptoLabel });
            tlp.Controls.Add(conceptoPanel, 1, 0);

            // Tipo
            tlp.Controls.Add(new Label
            {
                Text = mov.TipoMovimiento ?? "Movimiento",
                ForeColor = colorMov,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            }, 2, 0);

            // Importe
            tlp.Controls.Add(new Label
            {
                Text = $"{signo} {mov.Monto.ToString("C2", CultureInfo.CreateSpecificCulture("es-ES"))}",
                ForeColor = colorMov,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            }, 3, 0);

            fila.Controls.Add(tlp);
            return fila;
        }

        private static void RedondearEsquinas(Control c, int radius)
        {
            try
            {
                if (c.Width > 0 && c.Height > 0)
                    c.Region = new Region(RoundedRect(c.ClientRectangle, radius));
            }
            catch { }
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            if (r.Width < d || r.Height < d)
            {
                path.AddRectangle(r);
                return path;
            }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseAllFigures();
            return path;
        }
    }
}