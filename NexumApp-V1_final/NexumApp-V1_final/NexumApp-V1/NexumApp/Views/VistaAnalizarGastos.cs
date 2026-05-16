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
    public class VistaAnalizarGastos : UserControl
    {
        // Paleta moderna - Dark/Light combinado
        private static readonly Color BgPage = Color.FromArgb(15, 20, 35);
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

        private static readonly Color[] COLORES_GRAFICO = {
            AccentDanger, AccentPrimary, AccentSuccess,
            AccentWarning, Color.FromArgb(139, 92, 246), Color.FromArgb(236, 72, 153),
        };

        private readonly MovimientoService _movService = new MovimientoService();
        private List<Movimiento> _movimientos = new List<Movimiento>();

        private ComboBox _cmbRangoFechas;
        private DateTimePicker _dtpDesde, _dtpHasta;
        private Button _btnAplicarFiltros;
        private Panel _contenedorGrafico;
        private FlowLayoutPanel _contenedorCategorias;
        private FlowLayoutPanel _contenedorMovimientos;

        private Label _lblIngresosValor, _lblGastosValor, _lblBalanceValor, _lblTotalGastosValor;

        private string _filtroActual = "MES";
        private DateTime _fechaDesde = DateTime.Now.AddMonths(-1);
        private DateTime _fechaHasta = DateTime.Now;

        public VistaAnalizarGastos()
        {
            BackColor = BgPage;
            Dock = DockStyle.Fill;
            DoubleBuffered = true;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            CargarDatosIniciales();
            BuildUI();
        }

        private void CargarDatosIniciales()
        {
            try
            {
                if (SesionActual.Instancia?.EstaLogeado == true)
                {
                    _movimientos = _movService.ObtenerMovimientosRecientesPorUsuario(
                        SesionActual.Instancia.Usuario.Id, 500) ?? new List<Movimiento>();
                }
            }
            catch { _movimientos = new List<Movimiento>(); }

            _fechaDesde = DateTime.Now.AddMonths(-1);
            _fechaHasta = DateTime.Now;
        }

        private void BuildUI()
        {
            Controls.Clear();

            // Scroll principal
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = BgPage };
            var main = new Panel { Width = 1200, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent };
            scroll.Controls.Add(main);
            scroll.Resize += (s, ev) => { main.Left = Math.Max(20, (scroll.ClientSize.Width - 1200) / 2); };
            Controls.Add(scroll);

            int y = 28;

            // ========== HEADER ==========
            var headerPanel = new Panel { Location = new Point(0, y), Width = 1160, Height = 70, BackColor = BgCard };
            Redondear(headerPanel, 12);

            var icono = new Label { Text = "📊", Font = new Font("Segoe UI Emoji", 28), Location = new Point(20, 18), AutoSize = true };
            var titulo = new Label { Text = "Analizar gastos", ForeColor = TextLight, Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(95, 12), AutoSize = true };
            var subtitulo = new Label { Text = "Visualiza y comprende tus hábitos de gasto", ForeColor = TextGray, Font = new Font("Segoe UI", 11), Location = new Point(95, 44), AutoSize = true };
            headerPanel.Controls.AddRange(new Control[] { icono, titulo, subtitulo });
            main.Controls.Add(headerPanel);
            y += 85;

            // ========== FILTROS ==========
            var filtrosPanel = new Panel { Location = new Point(0, y), Width = 1160, Height = 80, BackColor = BgCard };
            Redondear(filtrosPanel, 12);

            // Rango rápido
            var lblRapido = new Label { Text = "PERÍODO RÁPIDO:", ForeColor = TextDarkGray, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(20, 28), AutoSize = true };
            _cmbRangoFechas = new ComboBox
            {
                Size = new Size(150, 32),
                Location = new Point(140, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                BackColor = BgPage,
                ForeColor = TextLight,
                FlatStyle = FlatStyle.Flat
            };
            _cmbRangoFechas.Items.AddRange(new string[] { "Última semana", "Último mes", "Último trimestre", "Último año", "Personalizado" });
            _cmbRangoFechas.SelectedIndex = 1;
            _cmbRangoFechas.SelectedIndexChanged += (s, e) => AplicarRapido();

            // Fechas personalizadas
            var lblDesde = new Label { Text = "DESDE:", ForeColor = TextDarkGray, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(320, 14), AutoSize = true };
            _dtpDesde = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Size = new Size(110, 30),
                Location = new Point(320, 32),
                Value = _fechaDesde,
                BackColor = BgPage,
                ForeColor = TextLight
            };

            var lblHasta = new Label { Text = "HASTA:", ForeColor = TextDarkGray, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(450, 14), AutoSize = true };
            _dtpHasta = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Size = new Size(110, 30),
                Location = new Point(450, 32),
                Value = _fechaHasta,
                BackColor = BgPage,
                ForeColor = TextLight
            };

            _btnAplicarFiltros = new Button
            {
                Text = "Aplicar filtros",
                Size = new Size(140, 38),
                Location = new Point(580, 22),
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentPrimary,
                ForeColor = TextLight,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnAplicarFiltros.FlatAppearance.BorderSize = 0;
            _btnAplicarFiltros.Click += (s, e) => AplicarFiltrosPersonalizados();

            filtrosPanel.Controls.AddRange(new Control[] { lblRapido, _cmbRangoFechas, lblDesde, _dtpDesde, lblHasta, _dtpHasta, _btnAplicarFiltros });
            main.Controls.Add(filtrosPanel);
            y += 95;

            // ========== KPIs ==========
            var kpisLayout = new TableLayoutPanel
            {
                Location = new Point(0, y),
                Size = new Size(1160, 120),
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            for (int i = 0; i < 4; i++) kpisLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            kpisLayout.Controls.Add(CrearKpiModerno("💰 INGRESOS", ref _lblIngresosValor, AccentSuccess), 0, 0);
            kpisLayout.Controls.Add(CrearKpiModerno("💸 GASTOS", ref _lblGastosValor, AccentDanger), 1, 0);
            kpisLayout.Controls.Add(CrearKpiModerno("⚖️ BALANCE", ref _lblBalanceValor, AccentPrimary), 2, 0);
            kpisLayout.Controls.Add(CrearKpiModerno("📊 TOTAL GASTOS", ref _lblTotalGastosValor, AccentWarning), 3, 0);

            main.Controls.Add(kpisLayout);
            y += 135;

            // ========== GRÁFICO Y CATEGORÍAS ==========
            var analisisPanel = new Panel { Location = new Point(0, y), Width = 1160, Height = 380, BackColor = Color.Transparent };

            // Gráfico de donut
            _contenedorGrafico = new Panel { Location = new Point(0, 0), Size = new Size(380, 380), BackColor = BgCard };
            Redondear(_contenedorGrafico, 16);

            // Categorías
            _contenedorCategorias = new FlowLayoutPanel
            {
                Location = new Point(400, 0),
                Size = new Size(760, 380),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = BgCard,
                Padding = new Padding(20)
            };
            Redondear(_contenedorCategorias, 16);

            analisisPanel.Controls.AddRange(new Control[] { _contenedorGrafico, _contenedorCategorias });
            main.Controls.Add(analisisPanel);
            y += 395;

            // ========== MOVIMIENTOS RECIENTES ==========
            var movimientosHeader = new Label
            {
                Text = "📋 MOVIMIENTOS RECIENTES",
                ForeColor = TextLight,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Location = new Point(0, y),
                AutoSize = true
            };
            main.Controls.Add(movimientosHeader);
            y += 35;

            _contenedorMovimientos = new FlowLayoutPanel
            {
                Location = new Point(0, y),
                Size = new Size(1160, 400),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = BgCard,
                Padding = new Padding(0)
            };
            Redondear(_contenedorMovimientos, 16);
            main.Controls.Add(_contenedorMovimientos);
            y += 415;

            main.Height = y + 30;

            // Aplicar datos iniciales
            AplicarFiltros();
        }

        private void AplicarRapido()
        {
            var ahora = DateTime.Now;
            switch (_cmbRangoFechas.SelectedIndex)
            {
                case 0: // Semana
                    _fechaDesde = ahora.AddDays(-7);
                    _fechaHasta = ahora;
                    break;
                case 1: // Mes
                    _fechaDesde = ahora.AddMonths(-1);
                    _fechaHasta = ahora;
                    break;
                case 2: // Trimestre
                    _fechaDesde = ahora.AddMonths(-3);
                    _fechaHasta = ahora;
                    break;
                case 3: // Año
                    _fechaDesde = ahora.AddYears(-1);
                    _fechaHasta = ahora;
                    break;
                case 4: // Personalizado
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
            if (_cmbRangoFechas.SelectedIndex != 4)
                _cmbRangoFechas.SelectedIndex = 4;
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            var movimientosFiltrados = _movimientos
                .Where(m => m.Fecha.Date >= _fechaDesde.Date && m.Fecha.Date <= _fechaHasta.Date)
                .ToList();

            decimal ingresos = movimientosFiltrados
                .Where(m => m.TipoMovimiento == "Ingreso" || m.TipoMovimiento?.Contains("Recibida") == true)
                .Sum(m => m.Monto);

            decimal gastos = movimientosFiltrados
                .Where(m => m.TipoMovimiento != "Ingreso" && !m.TipoMovimiento?.Contains("Recibida") == true)
                .Sum(m => m.Monto);

            decimal balance = ingresos - gastos;

            _lblIngresosValor.Text = ingresos.ToString("C2", ES);
            _lblGastosValor.Text = gastos.ToString("C2", ES);
            _lblBalanceValor.Text = balance.ToString("C2", ES);
            _lblBalanceValor.ForeColor = balance >= 0 ? AccentSuccess : AccentDanger;

            // Categorías
            var categorias = new Dictionary<string, decimal>();
            foreach (var m in movimientosFiltrados)
            {
                bool esGasto = m.TipoMovimiento != "Ingreso" && !m.TipoMovimiento?.Contains("Recibida") == true;
                if (!esGasto) continue;
                string cat = ClasificarCategoria(m.Concepto, m.TipoMovimiento);
                if (categorias.ContainsKey(cat)) categorias[cat] += m.Monto;
                else categorias[cat] = m.Monto;
            }

            var sortedCats = categorias.OrderByDescending(x => x.Value).Take(6).ToList();
            decimal totalGastos = sortedCats.Sum(x => x.Value);
            _lblTotalGastosValor.Text = totalGastos.ToString("C2", ES);

            DibujarGraficoDonut(sortedCats, totalGastos);
            RenderizarCategorias(sortedCats, totalGastos);
            RenderizarMovimientos(movimientosFiltrados.Take(15).ToList());
        }

        private void DibujarGraficoDonut(List<KeyValuePair<string, decimal>> categorias, decimal total)
        {
            _contenedorGrafico.Controls.Clear();
            if (categorias.Count == 0 || total == 0)
            {
                var empty = new Label
                {
                    Text = "Sin datos en este período",
                    ForeColor = TextGray,
                    Font = new Font("Segoe UI", 11, FontStyle.Italic),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                _contenedorGrafico.Controls.Add(empty);
                return;
            }

            var donutPanel = new Panel { Dock = DockStyle.Fill };
            donutPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(60, 70, 260, 260);

                float startAngle = -90f;
                for (int i = 0; i < categorias.Count; i++)
                {
                    float sweepAngle = 360f * (float)(categorias[i].Value / total);
                    using (var pen = new Pen(COLORES_GRAFICO[i % COLORES_GRAFICO.Length], 40))
                    {
                        pen.StartCap = LineCap.Round;
                        pen.EndCap = LineCap.Round;
                        g.DrawArc(pen, rect, startAngle, sweepAngle);
                    }
                    startAngle += sweepAngle;
                }

                // Texto central
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var font = new Font("Segoe UI", 18, FontStyle.Bold))
                    g.DrawString($"€{total:N0}", font, new SolidBrush(TextLight), new Rectangle(60, 70, 260, 180), fmt);
                using (var font = new Font("Segoe UI", 10))
                    g.DrawString("gastados", font, new SolidBrush(TextGray), new Rectangle(60, 150, 260, 180), fmt);
            };
            _contenedorGrafico.Controls.Add(donutPanel);
        }

        private void RenderizarCategorias(List<KeyValuePair<string, decimal>> categorias, decimal total)
        {
            _contenedorCategorias.Controls.Clear();

            if (categorias.Count == 0)
            {
                var empty = new Label
                {
                    Text = "No hay categorías para mostrar",
                    ForeColor = TextGray,
                    Font = new Font("Segoe UI", 11, FontStyle.Italic),
                    AutoSize = true
                };
                _contenedorCategorias.Controls.Add(empty);
                return;
            }

            for (int i = 0; i < categorias.Count; i++)
            {
                decimal pct = total > 0 ? categorias[i].Value / total * 100m : 0;
                Color col = COLORES_GRAFICO[i % COLORES_GRAFICO.Length];

                var item = new Panel { Width = 680, Height = 55, BackColor = BgPage, Margin = new Padding(0, 0, 0, 10) };
                Redondear(item, 8);

                var dot = new Panel { Location = new Point(15, 18), Size = new Size(18, 18), BackColor = col };
                Redondear(dot, 9);

                var lblNombre = new Label { Text = categorias[i].Key, ForeColor = TextLight, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(45, 8), AutoSize = true };
                var lblPorcentaje = new Label { Text = $"{pct:F1}%", ForeColor = TextGray, Font = new Font("Segoe UI", 9), Location = new Point(45, 28), AutoSize = true };
                var lblMonto = new Label { Text = categorias[i].Value.ToString("C2", ES), ForeColor = col, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(580, 16), AutoSize = true };

                // Barra de progreso
                var barBg = new Panel { Location = new Point(200, 38), Size = new Size(370, 6), BackColor = Color.FromArgb(60, 70, 100) };
                Redondear(barBg, 3);
                int barWidth = (int)(370 * (float)(categorias[i].Value / total));
                var barFill = new Panel { Location = new Point(0, 0), Size = new Size(Math.Max(4, barWidth), 6), BackColor = col };
                Redondear(barFill, 3);
                barBg.Controls.Add(barFill);

                item.Controls.AddRange(new Control[] { dot, lblNombre, lblPorcentaje, lblMonto, barBg });
                _contenedorCategorias.Controls.Add(item);
            }

            // Total
            var totalItem = new Panel { Width = 680, Height = 45, BackColor = BgPage, Margin = new Padding(0, 5, 0, 0) };
            Redondear(totalItem, 8);
            totalItem.Controls.Add(new Label { Text = "TOTAL GASTOS:", ForeColor = TextGray, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(15, 14), AutoSize = true });
            totalItem.Controls.Add(new Label { Text = total.ToString("C2", ES), ForeColor = AccentPrimary, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(540, 10), AutoSize = true });
            _contenedorCategorias.Controls.Add(totalItem);
        }

        private void RenderizarMovimientos(List<Movimiento> movimientos)
        {
            _contenedorMovimientos.Controls.Clear();

            if (movimientos.Count == 0)
            {
                var empty = new Label
                {
                    Text = "No hay movimientos en este período",
                    ForeColor = TextGray,
                    Font = new Font("Segoe UI", 11, FontStyle.Italic),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(1160, 100)
                };
                _contenedorMovimientos.Controls.Add(empty);
                return;
            }

            foreach (var m in movimientos)
            {
                bool esIngreso = m.TipoMovimiento == "Ingreso" || m.TipoMovimiento?.Contains("Recibida") == true;
                Color colorMov = esIngreso ? AccentSuccess : AccentDanger;
                string signo = esIngreso ? "+" : "-";
                string icono = esIngreso ? "📥" : "📤";

                var fila = new Panel { Width = 1160, Height = 55, BackColor = BgPage, Margin = new Padding(0, 0, 0, 1) };

                var iconoLabel = new Label { Text = icono, Font = new Font("Segoe UI Emoji", 16), Location = new Point(15, 16), AutoSize = true };
                var tipoLabel = new Label { Text = m.TipoMovimiento ?? "Movimiento", ForeColor = colorMov, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(50, 10), Size = new Size(100, 20) };
                var fechaLabel = new Label { Text = m.Fecha.ToString("dd MMM yyyy • HH:mm", ES), ForeColor = TextDarkGray, Font = new Font("Segoe UI", 9), Location = new Point(50, 28), AutoSize = true };
                var conceptoLabel = new Label { Text = m.Concepto ?? "—", ForeColor = TextGray, Font = new Font("Segoe UI", 10), Location = new Point(170, 16), Size = new Size(500, 25), AutoEllipsis = true };
                var montoLabel = new Label { Text = $"{signo} {m.Monto.ToString("C2", ES)}", ForeColor = colorMov, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(980, 16), AutoSize = true };

                fila.Controls.AddRange(new Control[] { iconoLabel, tipoLabel, fechaLabel, conceptoLabel, montoLabel });
                _contenedorMovimientos.Controls.Add(fila);
            }
        }

        private Panel CrearKpiModerno(string titulo, ref Label valorLabel, Color color)
        {
            var card = new Panel { Margin = new Padding(8, 0, 8, 0), BackColor = BgCard };
            card.Dock = DockStyle.Fill;
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(card.ClientRectangle, 12))
                using (var brush = new SolidBrush(BgCard))
                {
                    e.Graphics.FillPath(brush, path);
                    using (var pen = new Pen(Color.FromArgb(30, color.R, color.G, color.B), 1))
                        e.Graphics.DrawPath(pen, path);
                }
            };
            Redondear(card, 12);

            valorLabel = new Label { Text = "€0,00", ForeColor = color, Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(20, 55), AutoSize = true };
            var lblTitulo = new Label { Text = titulo, ForeColor = TextDarkGray, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(20, 28), AutoSize = true };

            card.Controls.AddRange(new Control[] { lblTitulo, valorLabel });
            return card;
        }

        private string ClasificarCategoria(string concepto, string tipo)
        {
            if (string.IsNullOrEmpty(concepto)) return tipo ?? "Otros";
            string c = concepto.ToLower();
            if (c.Contains("spotify") || c.Contains("netflix") || c.Contains("hbo") || c.Contains("prime") || c.Contains("disney")) return "Suscripciones";
            if (c.Contains("mercadona") || c.Contains("carrefour") || c.Contains("lidl") || c.Contains("dia") || c.Contains("super") || c.Contains("alimenta")) return "Alimentación";
            if (c.Contains("luz") || c.Contains("gas") || c.Contains("agua") || c.Contains("electricidad") || c.Contains("suministro")) return "Suministros";
            if (c.Contains("alquiler") || c.Contains("hipoteca") || c.Contains("comunidad") || c.Contains("ibi")) return "Vivienda";
            if (c.Contains("transfer") || c.Contains("bizum") || c.Contains("envío")) return "Transferencias";
            if (c.Contains("amazon") || c.Contains("compra") || c.Contains("zara") || c.Contains("ropa") || c.Contains("tienda")) return "Compras";
            if (c.Contains("gasolina") || c.Contains("taxi") || c.Contains("uber") || c.Contains("metro") || c.Contains("tren") || c.Contains("bus")) return "Transporte";
            if (c.Contains("gym") || c.Contains("sport") || c.Contains("deporte") || c.Contains("salud") || c.Contains("farmacia") || c.Contains("médico")) return "Salud";
            if (c.Contains("restaurante") || c.Contains("bar") || c.Contains("cafe") || c.Contains("pizza") || c.Contains("comida")) return "Restauración";
            if (c.Contains("cine") || c.Contains("teatro") || c.Contains("concierto") || c.Contains("ocio")) return "Ocio";
            return "Otros";
        }

        private static GraphicsPath RR(Rectangle r, int rad)
        {
            int d = rad * 2;
            var p = new GraphicsPath();
            if (r.Width < d || r.Height < d) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseAllFigures();
            return p;
        }

        private static void Redondear(Control c, int r)
        {
            try { if (c.Width > 0 && c.Height > 0) c.Region = new Region(RR(c.ClientRectangle, r)); }
            catch { }
        }

        private static readonly CultureInfo ES = CultureInfo.CreateSpecificCulture("es-ES");
    }
}