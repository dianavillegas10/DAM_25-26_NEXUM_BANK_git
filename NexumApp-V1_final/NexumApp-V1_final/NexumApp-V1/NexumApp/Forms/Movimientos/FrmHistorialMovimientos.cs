using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace NexumApp.Forms.Movimientos
{
    internal partial class FrmHistorialMovimientos : Form
    {
        private readonly MovimientoService _movimientoService = new MovimientoService();
        private readonly CuentaService _cuentaService = new CuentaService();
        private CuentaBancaria _cuentaInicial;
        private List<Movimiento> _movimientos = new List<Movimiento>();
        private List<Movimiento> _movimientosFiltrados = new List<Movimiento>();

        // Paleta moderna - estilo Neo-brutalist / Glassmorphism
        private static readonly Color BgMain = Color.FromArgb(15, 20, 35);
        private static readonly Color BgCard = Color.FromArgb(25, 32, 50);
        private static readonly Color BgCardHover = Color.FromArgb(35, 42, 65);
        private static readonly Color AccentPrimary = Color.FromArgb(100, 108, 255);
        private static readonly Color AccentSuccess = Color.FromArgb(16, 185, 129);
        private static readonly Color AccentDanger = Color.FromArgb(239, 68, 68);
        private static readonly Color AccentWarning = Color.FromArgb(245, 158, 11);
        private static readonly Color TextLight = Color.FromArgb(240, 245, 255);
        private static readonly Color TextGray = Color.FromArgb(150, 160, 185);
        private static readonly Color TextDarkGray = Color.FromArgb(100, 110, 135);
        private static readonly Color BorderColor = Color.FromArgb(45, 55, 80);

        // Controles
        private Panel _contentPanel;
        private Panel _sidebar;
        private Panel _mainArea;

        private ComboBox _cmbCuentas;
        private Panel _filtroHoy, _filtroSemana, _filtroMes, _filtroTrimestre, _filtroAnio, _filtroTodo;
        private TextBox _txtBuscar;

        private Label _lblSaldoValor, _lblIngresosValor, _lblGastosValor, _lblMovimientosValor;
        private FlowLayoutPanel _movimientosContainer;

        private string _filtroActivo = "TODO";
        private string _filtroBusqueda = "";
        private Button _btnLimpiar;

        public FrmHistorialMovimientos(CuentaBancaria cuenta = null)
        {
            _cuentaInicial = cuenta;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            BackColor = BgMain;

            ConstruirUI();
            CargarCuentas();
        }

        private void ConstruirUI()
        {
            // Panel superior para cerrar/minimizar (barra personalizada)
            var titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = BgCard
            };

            var lblTitle = new Label
            {
                Text = "nexum  |  Historial de movimientos",
                ForeColor = TextGray,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                Location = new Point(20, 12),
                AutoSize = true
            };

            var btnClose = new Button
            {
                Text = "✕",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = TextGray,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(40, 45),
                Location = new Point(Width - 50, 0),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();

            var btnMinimize = new Button
            {
                Text = "─",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = TextGray,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(40, 45),
                Location = new Point(Width - 90, 0),
                Cursor = Cursors.Hand
            };
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.Click += (s, e) => WindowState = FormWindowState.Minimized;

            titleBar.Controls.Add(lblTitle);
            titleBar.Controls.Add(btnClose);
            titleBar.Controls.Add(btnMinimize);

            // Layout principal
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Sidebar izquierdo
            _sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 280,
                BackColor = Color.Transparent
            };

            // Área principal derecha
            _mainArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(20, 0, 0, 0)
            };

            _contentPanel.Controls.Add(_mainArea);
            _contentPanel.Controls.Add(_sidebar);

            Controls.Add(_contentPanel);
            Controls.Add(titleBar);

            ConstruirSidebar();
            ConstruirMainArea();
        }

        private void ConstruirSidebar()
        {
            // Logo / Header
            var logoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.Transparent
            };

            var logo = new Label
            {
                Text = "💳",
                Font = new Font("Segoe UI Emoji", 32),
                ForeColor = AccentPrimary,
                Location = new Point(20, 15),
                AutoSize = true
            };

            var logoText = new Label
            {
                Text = "Nexum",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = TextLight,
                Location = new Point(70, 20),
                AutoSize = true
            };

            var logoSlogan = new Label
            {
                Text = "Historial de transacciones",
                Font = new Font("Segoe UI", 9),
                ForeColor = TextGray,
                Location = new Point(70, 48),
                AutoSize = true
            };

            logoPanel.Controls.AddRange(new Control[] { logo, logoText, logoSlogan });

            // Selector de cuenta
            var cuentaLabel = new Label
            {
                Text = "CUENTA ACTIVA",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = TextDarkGray,
                Location = new Point(20, 100),
                AutoSize = true
            };

            _cmbCuentas = new ComboBox
            {
                Size = new Size(240, 40),
                Location = new Point(20, 125),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                BackColor = BgCard,
                ForeColor = TextLight,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 35
            };
            _cmbCuentas.DrawItem += (s, e) =>
            {
                e.DrawBackground();
                if (e.Index >= 0)
                {
                    var rect = new Rectangle(e.Bounds.X + 10, e.Bounds.Y, e.Bounds.Width - 20, e.Bounds.Height);
                    using (var brush = new SolidBrush(e.State.HasFlag(DrawItemState.Selected) ? AccentPrimary : TextLight))
                        e.Graphics.DrawString(_cmbCuentas.Items[e.Index].ToString(), _cmbCuentas.Font, brush, rect);
                }
                e.DrawFocusRectangle();
            };
            _cmbCuentas.SelectedIndexChanged += (s, e) => CargarMovimientos();

            // Filtros de tiempo
            var filtrosLabel = new Label
            {
                Text = "PERÍODO",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = TextDarkGray,
                Location = new Point(20, 185),
                AutoSize = true
            };

            var filtrosPanel = new Panel
            {
                Location = new Point(20, 205),
                Size = new Size(240, 220),
                BackColor = Color.Transparent
            };

            _filtroHoy = CrearBotonFiltro("📆  Hoy", "HOY", 0);
            _filtroSemana = CrearBotonFiltro("📅  Esta semana", "SEMANA", 45);
            _filtroMes = CrearBotonFiltro("📆  Este mes", "MES", 90);
            _filtroTrimestre = CrearBotonFiltro("🗓️  Este trimestre", "TRIMESTRE", 135);
            _filtroAnio = CrearBotonFiltro("📅  Este año", "AÑO", 180);
            _filtroTodo = CrearBotonFiltro("📜  Todo el historial", "TODO", 225);

            filtrosPanel.Controls.AddRange(new Control[] { _filtroHoy, _filtroSemana, _filtroMes, _filtroTrimestre, _filtroAnio, _filtroTodo });

            // Búsqueda
            var buscarLabel = new Label
            {
                Text = "BUSCAR",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = TextDarkGray,
                Location = new Point(20, 440),
                AutoSize = true
            };

            _txtBuscar = new TextBox
            {
                Size = new Size(240, 38),
                Location = new Point(20, 460),
                Font = new Font("Segoe UI", 10),
                BackColor = BgCard,
                ForeColor = TextLight,
                BorderStyle = BorderStyle.FixedSingle,
                Text = ""
            };
            _txtBuscar.TextChanged += (s, e) => AplicarFiltros();

            // Botón limpiar
            _btnLimpiar = new Button
            {
                Text = "⟳  Limpiar filtros",
                Size = new Size(240, 38),
                Location = new Point(20, 510),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 55, 80),
                ForeColor = TextGray,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnLimpiar.FlatAppearance.BorderSize = 0;
            _btnLimpiar.Click += (s, e) => LimpiarFiltros();

            _sidebar.Controls.Add(_btnLimpiar);
            _sidebar.Controls.Add(_txtBuscar);
            _sidebar.Controls.Add(buscarLabel);
            _sidebar.Controls.Add(filtrosPanel);
            _sidebar.Controls.Add(filtrosLabel);
            _sidebar.Controls.Add(_cmbCuentas);
            _sidebar.Controls.Add(cuentaLabel);
            _sidebar.Controls.Add(logoPanel);
        }

        private Panel CrearBotonFiltro(string texto, string valor, int y)
        {
            var btn = new Panel
            {
                Size = new Size(240, 38),
                Location = new Point(0, y),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            var label = new Label
            {
                Text = texto,
                Font = new Font("Segoe UI", 10),
                ForeColor = TextGray,
                Location = new Point(15, 10),
                AutoSize = true
            };

            btn.Controls.Add(label);

            btn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = btn.ClientRectangle;

                if (_filtroActivo == valor)
                {
                    // Activo: fondo con color y borde izquierdo
                    using (var brush = new SolidBrush(Color.FromArgb(40, AccentPrimary.R, AccentPrimary.G, AccentPrimary.B)))
                        e.Graphics.FillRectangle(brush, rect);
                    using (var brush = new SolidBrush(AccentPrimary))
                        e.Graphics.FillRectangle(brush, new Rectangle(0, 5, 3, rect.Height - 10));
                    label.ForeColor = AccentPrimary;
                    label.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
                else
                {
                    // Inactivo: solo borde redondeado en hover
                    if (btn.ClientRectangle.Contains(btn.PointToClient(Cursor.Position)))
                    {
                        using (var path = RoundedRect(rect, 8))
                        using (var pen = new Pen(Color.FromArgb(60, AccentPrimary.R, AccentPrimary.G, AccentPrimary.B), 1))
                            e.Graphics.DrawPath(pen, path);
                    }
                    label.ForeColor = TextGray;
                    label.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                }
            };

            btn.MouseEnter += (s, e) => btn.Invalidate();
            btn.MouseLeave += (s, e) => btn.Invalidate();
            btn.Click += (s, e) =>
            {
                _filtroActivo = valor;
                AplicarFiltros();
                // Refrescar todos los botones
                foreach (Control c in btn.Parent.Controls)
                    c.Invalidate();
            };

            return btn;
        }

        private void ConstruirMainArea()
        {
            // Tarjetas de estadísticas
            var statsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.Transparent
            };

            var cardSaldo = CrearCardEstadistica("SALDO DISPONIBLE", ref _lblSaldoValor, AccentPrimary);
            var cardIngresos = CrearCardEstadistica("INGRESOS", ref _lblIngresosValor, AccentSuccess);
            var cardGastos = CrearCardEstadistica("GASTOS", ref _lblGastosValor, AccentDanger);
            var cardMovimientos = CrearCardEstadistica("MOVIMIENTOS", ref _lblMovimientosValor, AccentWarning);

            cardSaldo.Location = new Point(0, 0);
            cardIngresos.Location = new Point(220, 0);
            cardGastos.Location = new Point(440, 0);
            cardMovimientos.Location = new Point(660, 0);

            statsPanel.Controls.AddRange(new Control[] { cardSaldo, cardIngresos, cardGastos, cardMovimientos });

            // Header de la tabla
            var tableHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = BgCard,
                Margin = new Padding(0, 20, 0, 0)
            };
            tableHeader.Paint += (s, e) =>
            {
                using (var path = RoundedRect(tableHeader.ClientRectangle, 10))
                using (var brush = new SolidBrush(BgCard))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);
                }
            };

            var headerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(20, 0, 20, 0)
            };
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));

            string[] headers = { "TIPO", "CONCEPTO / DESCRIPCIÓN", "FECHA", "IMPORTE" };
            for (int i = 0; i < headers.Length; i++)
            {
                var lbl = new Label
                {
                    Text = headers[i],
                    ForeColor = TextDarkGray,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    TextAlign = i == 3 ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill
                };
                headerLayout.Controls.Add(lbl, i, 0);
            }
            tableHeader.Controls.Add(headerLayout);

            // Contenedor de movimientos
            _movimientosContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 10)
            };

            _mainArea.Controls.Add(_movimientosContainer);
            _mainArea.Controls.Add(tableHeader);
            _mainArea.Controls.Add(statsPanel);
        }

        private Panel CrearCardEstadistica(string titulo, ref Label valorLabel, Color color)
        {
            var card = new Panel
            {
                Size = new Size(200, 100),
                BackColor = BgCard
            };
            card.Paint += (s, e) =>
            {
                using (var path = RoundedRect(card.ClientRectangle, 12))
                using (var brush = new SolidBrush(BgCard))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);
                    using (var pen = new Pen(Color.FromArgb(30, color.R, color.G, color.B), 1))
                        e.Graphics.DrawPath(pen, path);
                }
                // Línea decorativa superior
                using (var brush = new SolidBrush(color))
                    e.Graphics.FillRectangle(brush, new Rectangle(0, 0, card.Width, 3));
            };

            var lblTitulo = new Label
            {
                Text = titulo,
                ForeColor = TextDarkGray,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(15, 15),
                AutoSize = true
            };

            valorLabel = new Label
            {
                Text = "€0,00",
                ForeColor = color,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(15, 45),
                AutoSize = true
            };

            card.Controls.Add(lblTitulo);
            card.Controls.Add(valorLabel);
            return card;
        }

        private void CargarCuentas()
        {
            if (SesionActual.Instancia?.Usuario == null) return;
            var cuentas = _cuentaService.ObtenerCuentasPorUsuario(SesionActual.Instancia.Usuario.Id);

            _cmbCuentas.Items.Clear();
            foreach (var c in cuentas)
            {
                string numeroMostrar = c.NumeroCuenta?.Length > 4
                    ? "•••• " + c.NumeroCuenta.Substring(c.NumeroCuenta.Length - 4)
                    : "•••• ••••";
                _cmbCuentas.Items.Add(new CuentaItem { Cuenta = c, Display = $"{c.TipoCuenta ?? "Cuenta"} • {numeroMostrar}" });
            }

            if (_cuentaInicial != null)
            {
                for (int i = 0; i < _cmbCuentas.Items.Count; i++)
                    if (((CuentaItem)_cmbCuentas.Items[i]).Cuenta.Id == _cuentaInicial.Id)
                    {
                        _cmbCuentas.SelectedIndex = i;
                        break;
                    }
            }
            else if (_cmbCuentas.Items.Count > 0)
                _cmbCuentas.SelectedIndex = 0;
        }

        private void CargarMovimientos()
        {
            if (_cmbCuentas.SelectedItem == null) return;
            var cuenta = ((CuentaItem)_cmbCuentas.SelectedItem).Cuenta;

            try
            {
                _movimientos = _movimientoService.ObtenerMovimientosPorCuenta(cuenta.Id, 2000) ?? new List<Movimiento>();
                _movimientos = _movimientos.OrderByDescending(m => m.Fecha).ToList();
            }
            catch { _movimientos = new List<Movimiento>(); }

            _lblSaldoValor.Text = cuenta.Saldo.ToString("C2", CultureInfo.CreateSpecificCulture("es-ES"));
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            if (_movimientos == null) return;

            var filtrados = new List<Movimiento>(_movimientos);

            // Filtro de tiempo
            var ahora = DateTime.Now;
            var hoy = ahora.Date;

            switch (_filtroActivo)
            {
                case "HOY":
                    filtrados = filtrados.Where(m => m.Fecha.Date == hoy).ToList();
                    break;
                case "SEMANA":
                    var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek + (int)DayOfWeek.Monday);
                    filtrados = filtrados.Where(m => m.Fecha.Date >= inicioSemana).ToList();
                    break;
                case "MES":
                    filtrados = filtrados.Where(m => m.Fecha.Month == ahora.Month && m.Fecha.Year == ahora.Year).ToList();
                    break;
                case "TRIMESTRE":
                    var trimestre = (ahora.Month - 1) / 3;
                    filtrados = filtrados.Where(m => m.Fecha.Year == ahora.Year && (m.Fecha.Month - 1) / 3 == trimestre).ToList();
                    break;
                case "AÑO":
                    filtrados = filtrados.Where(m => m.Fecha.Year == ahora.Year).ToList();
                    break;
            }

            // Filtro de búsqueda
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
            _filtroActivo = "TODO";
            foreach (Control c in _filtroHoy.Parent.Controls)
                c.Invalidate();
            AplicarFiltros();
        }

        private void ActualizarStats()
        {
            var fmt = CultureInfo.CreateSpecificCulture("es-ES");
            decimal ingresos = _movimientosFiltrados.Where(m => m.TipoMovimiento == "Ingreso").Sum(m => m.Monto);
            decimal gastos = _movimientosFiltrados.Where(m => m.TipoMovimiento != "Ingreso").Sum(m => m.Monto);

            _lblIngresosValor.Text = ingresos.ToString("C2", fmt);
            _lblGastosValor.Text = gastos.ToString("C2", fmt);
            _lblMovimientosValor.Text = _movimientosFiltrados.Count.ToString("N0");
        }

        private void RenderMovimientos()
        {
            _movimientosContainer.Controls.Clear();

            if (_movimientosFiltrados.Count == 0)
            {
                var emptyPanel = new Panel
                {
                    Size = new Size(_mainArea.Width - 40, 200),
                    BackColor = Color.Transparent
                };
                var lblEmpty = new Label
                {
                    Text = "✨  No hay movimientos para mostrar\n\nPrueba a cambiar los filtros o seleccionar otra cuenta",
                    ForeColor = TextDarkGray,
                    Font = new Font("Segoe UI", 12),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                emptyPanel.Controls.Add(lblEmpty);
                _movimientosContainer.Controls.Add(emptyPanel);
                return;
            }

            foreach (var mov in _movimientosFiltrados)
            {
                _movimientosContainer.Controls.Add(CrearFilaMovimiento(mov));
            }
        }

        private Panel CrearFilaMovimiento(Movimiento mov)
        {
            bool esIngreso = mov.TipoMovimiento?.Equals("Ingreso", StringComparison.OrdinalIgnoreCase) == true;
            Color colorMov = esIngreso ? AccentSuccess : AccentDanger;
            string signo = esIngreso ? "+" : "−";

            var fila = new Panel
            {
                Size = new Size(_mainArea.Width - 40, 65),
                BackColor = BgCard,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 8)
            };

            // Borde redondeado
            fila.Paint += (s, e) =>
            {
                using (var path = RoundedRect(fila.ClientRectangle, 10))
                using (var brush = new SolidBrush(BgCard))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);
                }
            };

            // Hover
            fila.MouseEnter += (s, e) => fila.BackColor = BgCardHover;
            fila.MouseLeave += (s, e) => fila.BackColor = BgCard;

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(20, 0, 20, 0)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));

            // Icono + Tipo
            var tipoPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var icono = new Label
            {
                Text = esIngreso ? "⬇️" : "⬆️",
                Font = new Font("Segoe UI Emoji", 14),
                Location = new Point(0, 22),
                AutoSize = true
            };
            var tipoLabel = new Label
            {
                Text = esIngreso ? "Ingreso" : "Gasto",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = colorMov,
                Location = new Point(30, 24),
                AutoSize = true
            };
            tipoPanel.Controls.AddRange(new Control[] { icono, tipoLabel });
            tlp.Controls.Add(tipoPanel, 0, 0);

            // Concepto
            tlp.Controls.Add(new Label
            {
                Text = mov.Concepto ?? "—",
                ForeColor = TextLight,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoEllipsis = true
            }, 1, 0);

            // Fecha
            tlp.Controls.Add(new Label
            {
                Text = mov.Fecha.ToString("dd MMM yyyy • HH:mm", CultureInfo.CreateSpecificCulture("es-ES")),
                ForeColor = TextGray,
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            }, 2, 0);

            // Importe
            tlp.Controls.Add(new Label
            {
                Text = $"{signo} {mov.Monto.ToString("C2", CultureInfo.CreateSpecificCulture("es-ES"))}",
                ForeColor = colorMov,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill
            }, 3, 0);

            fila.Controls.Add(tlp);
            return fila;
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

        private class CuentaItem
        {
            public CuentaBancaria Cuenta { get; set; }
            public string Display { get; set; }
            public override string ToString() => Display;
        }
    }
}