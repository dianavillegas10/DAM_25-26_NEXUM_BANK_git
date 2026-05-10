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
        private static readonly Color C_Red    = Color.FromArgb(236, 0, 0);
        private static readonly Color C_BgPage = Color.FromArgb(246, 247, 249);
        private static readonly Color C_White  = Color.White;
        private static readonly Color C_Border = Color.FromArgb(220, 224, 230);
        private static readonly Color C_Text   = Color.FromArgb(30, 30, 50);
        private static readonly Color C_Muted  = Color.FromArgb(110, 120, 140);
        private static readonly Color C_Green  = Color.FromArgb(0, 168, 89);
        private static readonly CultureInfo ES = CultureInfo.CreateSpecificCulture("es-ES");

        private static readonly Color[] COLORES_GRAFICO = {
            Color.FromArgb(236, 0, 0), Color.FromArgb(0, 122, 204), Color.FromArgb(0, 168, 89),
            Color.FromArgb(255, 140, 0), Color.FromArgb(128, 0, 128), Color.FromArgb(0, 160, 160),
        };

        private readonly MovimientoService _movService = new MovimientoService();

        public VistaAnalizarGastos() { BackColor = C_BgPage; Dock = DockStyle.Fill; DoubleBuffered = true; }
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); BuildUI(); }

        private void BuildUI()
        {
            Controls.Clear();
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = C_BgPage };
            var main   = new Panel { Width = 900, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent };
            scroll.Controls.Add(main);
            scroll.Resize += (s, ev) => { main.Left = Math.Max(32, (scroll.ClientSize.Width - 900) / 2); };
            Controls.Add(scroll);
            int y = 28;

            // Cabecera
            var icoHdr = MakeIconCircle(new Point(0, 6), 48, C_Red, "📊", 20f);
            main.Controls.Add(icoHdr);
            main.Controls.Add(new Label { Text = "Analizar gastos", ForeColor = C_Text, Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(62, 8), AutoSize = true });
            main.Controls.Add(new Label { Text = "Resumen detallado de tus movimientos y patrones de gasto", ForeColor = C_Muted, Font = new Font("Segoe UI", 10), Location = new Point(62, 38), AutoSize = true }); y += 76;

            // Cargar datos
            var (ingresos, gastos) = (0m, 0m);
            var movimientos = new List<Movimiento>();
            try {
                if (SesionActual.Instancia?.EstaLogeado == true) {
                    var res = _movService.ObtenerResumenMensual(SesionActual.Instancia.Usuario.Id);
                    ingresos = res.Ingresos; gastos = res.Gastos;
                    movimientos = _movService.ObtenerMovimientosRecientesPorUsuario(SesionActual.Instancia.Usuario.Id, 60) ?? new List<Movimiento>();
                }
            } catch { }

            // KPIs
            var tlpKpis = new TableLayoutPanel { Location = new Point(0, y), Size = new Size(860, 100),
                ColumnCount = 3, RowCount = 1, BackColor = Color.Transparent, Padding = new Padding(0) };
            tlpKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpKpis.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            tlpKpis.Controls.Add(CrearKpi("Ingresos este mes", ingresos.ToString("C2", ES), C_Green, "📥"), 0, 0);
            tlpKpis.Controls.Add(CrearKpi("Gastos este mes",   gastos.ToString("C2", ES),   C_Red,   "📤"), 1, 0);
            decimal balance = ingresos - gastos;
            tlpKpis.Controls.Add(CrearKpi("Balance neto", balance.ToString("C2", ES), balance >= 0 ? C_Green : C_Red, "💰"), 2, 0);
            main.Controls.Add(tlpKpis); y += 114;

            // Categorías de gastos
            var categorias = new Dictionary<string, decimal>();
            foreach (var m in movimientos)
            {
                bool esGasto = m.TipoMovimiento.Contains("Retiro") || m.TipoMovimiento.Contains("Transferencia") || m.TipoMovimiento.Contains("Pago");
                if (!esGasto) continue;
                string cat = ClasificarCategoria(m.Concepto, m.TipoMovimiento);
                if (categorias.ContainsKey(cat)) categorias[cat] += m.Monto;
                else categorias[cat] = m.Monto;
            }

            // Layout 2 columnas: gráfico + lista
            var tlp2 = new TableLayoutPanel { Location = new Point(0, y), Size = new Size(860, 320),
                ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
            tlp2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
            tlp2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlp2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Gráfico de barras
            var cardGrafico = MakeCard(new Point(0, 0), new Size(280, 300));
            cardGrafico.Dock = DockStyle.Fill;
            var sortedCats = categorias.OrderByDescending(x => x.Value).Take(6).ToList();
            decimal totalGastos = sortedCats.Sum(x => x.Value);
            cardGrafico.Paint += (s, ev) => DibujarGraficoBarras(ev.Graphics, cardGrafico.ClientRectangle, sortedCats, totalGastos);
            tlp2.Controls.Add(cardGrafico, 0, 0);

            // Lista de categorías
            var cardLista = MakeCard(new Point(0, 0), new Size(540, 300));
            cardLista.Dock = DockStyle.Fill; cardLista.Padding = new Padding(20, 16, 20, 16);
            cardLista.Controls.Add(new Label { Text = "Desglose por categoría", ForeColor = C_Text, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 16), AutoSize = true });
            int ly = 46;
            if (sortedCats.Count == 0) {
                cardLista.Controls.Add(new Label { Text = "Sin gastos registrados este período.", ForeColor = C_Muted, Font = new Font("Segoe UI", 10, FontStyle.Italic), Location = new Point(20, ly), AutoSize = true });
            } else {
                for (int i = 0; i < sortedCats.Count; i++) {
                    Color col = COLORES_GRAFICO[i % COLORES_GRAFICO.Length];
                    decimal pct = totalGastos > 0 ? sortedCats[i].Value / totalGastos * 100m : 0;
                    var filaPanel = new Panel { Location = new Point(20, ly), Size = new Size(cardLista.Width - 40, 38), BackColor = Color.Transparent };
                    // Punto color
                    var dot = new Panel { Location = new Point(0, 11), Size = new Size(14, 14), BackColor = col }; Redondear(dot, 7);
                    filaPanel.Controls.Add(dot);
                    filaPanel.Controls.Add(new Label { Text = sortedCats[i].Key, ForeColor = C_Text, Font = new Font("Segoe UI", 10), Location = new Point(22, 8), AutoSize = true });
                    filaPanel.Controls.Add(new Label { Text = $"{pct:F1}%", ForeColor = C_Muted, Font = new Font("Segoe UI", 8), Location = new Point(200, 12), AutoSize = true });
                    filaPanel.Controls.Add(new Label { Text = sortedCats[i].Value.ToString("C2", ES), ForeColor = col, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(380, 8), Size = new Size(120, 22), TextAlign = ContentAlignment.MiddleRight });
                    // Barra mini
                    int barW = (int)(Math.Min(1m, sortedCats[i].Value / Math.Max(1m, totalGastos)) * 370);
                    var barBg = new Panel { Location = new Point(22, 30), Size = new Size(370, 4), BackColor = Color.FromArgb(220, 224, 230) }; Redondear(barBg, 2);
                    var barFl = new Panel { Location = new Point(0, 0), Size = new Size(Math.Max(4, barW), 4), BackColor = col }; Redondear(barFl, 2);
                    barBg.Controls.Add(barFl); filaPanel.Controls.Add(barBg);
                    cardLista.Controls.Add(filaPanel); ly += 40;
                }
                cardLista.Controls.Add(new Panel { Location = new Point(20, ly), Size = new Size(cardLista.Width - 40, 1), BackColor = C_Border }); ly += 8;
                cardLista.Controls.Add(new Label { Text = $"Total gastos:  {totalGastos.ToString("C2", ES)}", ForeColor = C_Text, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, ly), AutoSize = true });
            }
            cardLista.Height = Math.Max(300, ly + 40);
            tlp2.Controls.Add(cardLista, 1, 0);
            tlp2.Height = Math.Max(300, cardLista.Height);
            main.Controls.Add(tlp2); y += tlp2.Height + 14;

            // Historial reciente
            main.Controls.Add(new Label { Text = "Últimos movimientos", ForeColor = C_Text, Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(0, y), AutoSize = true }); y += 30;
            var cardHistorial = MakeCard(new Point(0, y), new Size(860, 40)); main.Controls.Add(cardHistorial);
            int hy = 16;
            var ultimos = movimientos.Take(10).ToList();
            if (ultimos.Count == 0) {
                cardHistorial.Controls.Add(new Label { Text = "No hay movimientos recientes.", ForeColor = C_Muted, Font = new Font("Segoe UI", 10, FontStyle.Italic), Location = new Point(20, hy), AutoSize = true }); hy += 28;
            } else {
                foreach (var m in ultimos)
                {
                    bool esGasto = m.TipoMovimiento.Contains("Retiro") || m.TipoMovimiento.Contains("Transferencia") || m.TipoMovimiento.Contains("Pago");
                    Color col = esGasto ? C_Red : C_Green;
                    string signo = esGasto ? "-" : "+";
                    string concepto = string.IsNullOrWhiteSpace(m.Concepto) ? m.TipoMovimiento : m.Concepto;
                    var fila = new Panel { Location = new Point(0, hy), Size = new Size(860, 38), BackColor = Color.Transparent };
                    fila.Paint += (s, ev) => { if (fila.Top > 16) { ev.Graphics.DrawLine(new Pen(C_Border), 20, 0, fila.Width - 20, 0); } };
                    fila.Controls.Add(new Label { Text = m.Fecha.ToString("dd/MM/yy"), ForeColor = C_Muted, Font = new Font("Segoe UI", 9), Location = new Point(20, 10), Size = new Size(80, 18) });
                    fila.Controls.Add(new Label { Text = m.TipoMovimiento, ForeColor = col, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(110, 10), Size = new Size(100, 18) });
                    fila.Controls.Add(new Label { Text = concepto, ForeColor = C_Text, Font = new Font("Segoe UI", 9), Location = new Point(220, 10), Size = new Size(380, 18), AutoEllipsis = true });
                    fila.Controls.Add(new Label { Text = $"{signo}{m.Monto.ToString("C2", ES)}", ForeColor = col, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(620, 8), Size = new Size(220, 22), TextAlign = ContentAlignment.MiddleRight });
                    cardHistorial.Controls.Add(fila); hy += 38;
                }
            }
            cardHistorial.Height = hy + 16; y += cardHistorial.Height + 14;
            main.Height = y + 20;
        }

        private Panel CrearKpi(string titulo, string valor, Color col, string icono)
        {
            var card = new Panel { Margin = new Padding(0, 0, 12, 0), BackColor = C_White };
            card.Dock = DockStyle.Fill;
            card.Paint += (s, ev) => { ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using (var path = RR(card.ClientRectangle, 12)) { ev.Graphics.FillPath(Brushes.White, path); ev.Graphics.DrawPath(new Pen(C_Border, 1), path); } };
            BeginInvoke(new Action(() => Redondear(card, 12)));
            var icoLbl = MakeIconCircle(new Point(16, 20), 36, col, icono, 15f);
            card.Controls.Add(icoLbl);
            card.Controls.Add(new Label { Text = titulo, ForeColor = C_Muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(62, 20), AutoSize = true });
            card.Controls.Add(new Label { Text = valor, ForeColor = col, Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(62, 40), AutoSize = true });
            return card;
        }

        private void DibujarGraficoBarras(Graphics g, Rectangle bounds, List<KeyValuePair<string, decimal>> datos, decimal total)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = RR(bounds, 12)) { g.FillPath(Brushes.White, path); g.DrawPath(new Pen(C_Border, 1), path); }
            if (datos.Count == 0) return;

            var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("Por categoría", new Font("Segoe UI", 10, FontStyle.Bold), new SolidBrush(C_Text), new Rectangle(16, 12, bounds.Width - 32, 24), fmt);

            int chartTop = 44; int chartH = bounds.Height - chartTop - 20;
            int barW = (bounds.Width - 40) / Math.Max(1, datos.Count); int barGap = 6;

            for (int i = 0; i < datos.Count; i++)
            {
                Color col = COLORES_GRAFICO[i % COLORES_GRAFICO.Length];
                decimal pct = total > 0 ? datos[i].Value / total : 0;
                int barH = (int)(chartH * (double)pct);
                int x = 20 + i * barW + barGap;
                int w = barW - barGap * 2;
                int barY = chartTop + chartH - barH;
                using (var path = RR(new Rectangle(x, barY, w, Math.Max(4, barH)), 4))
                    g.FillPath(new SolidBrush(col), path);
                // Porcentaje
                if (pct > 0.05m)
                    g.DrawString($"{pct * 100:F0}%", new Font("Segoe UI", 7, FontStyle.Bold), Brushes.White,
                        new Rectangle(x, barY + 4, w, 16), fmt);
                // Etiqueta abajo truncada
                string lbl = datos[i].Key.Length > 6 ? datos[i].Key.Substring(0, 6) : datos[i].Key;
                g.DrawString(lbl, new Font("Segoe UI", 7), new SolidBrush(C_Muted),
                    new Rectangle(x, chartTop + chartH + 2, w, 16), fmt);
            }
        }

        private string ClasificarCategoria(string concepto, string tipo)
        {
            if (string.IsNullOrEmpty(concepto)) return tipo;
            string c = concepto.ToLower();
            if (c.Contains("spotify") || c.Contains("netflix") || c.Contains("hbo") || c.Contains("prime")) return "Suscripciones";
            if (c.Contains("mercadona") || c.Contains("carrefour") || c.Contains("lidl") || c.Contains("super") || c.Contains("alimenta")) return "Alimentación";
            if (c.Contains("luz") || c.Contains("gas") || c.Contains("agua") || c.Contains("electricidad") || c.Contains("suministro")) return "Suministros";
            if (c.Contains("alquiler") || c.Contains("hipoteca") || c.Contains("comunidad") || c.Contains("ibi")) return "Vivienda";
            if (c.Contains("transfer") || c.Contains("bizum") || c.Contains("envío")) return "Transferencias";
            if (c.Contains("amazon") || c.Contains("compra") || c.Contains("zara") || c.Contains("ropa")) return "Compras";
            if (c.Contains("gasolina") || c.Contains("taxi") || c.Contains("uber") || c.Contains("metro") || c.Contains("tren")) return "Transporte";
            if (c.Contains("gym") || c.Contains("sport") || c.Contains("deporte") || c.Contains("salud") || c.Contains("farmacia")) return "Salud";
            if (c.Contains("restaurante") || c.Contains("bar") || c.Contains("cafe") || c.Contains("pizza") || c.Contains("comida")) return "Restauración";
            return string.IsNullOrWhiteSpace(concepto) ? tipo : concepto.Split(' ')[0];
        }

        private Panel MakeCard(Point loc, Size sz) { var p = new Panel { Location = loc, Size = sz, BackColor = C_White }; p.Paint += (s, ev) => { ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using (var path = RR(p.ClientRectangle, 12)) { ev.Graphics.FillPath(Brushes.White, path); ev.Graphics.DrawPath(new Pen(C_Border, 1), path); } }; BeginInvoke(new Action(() => Redondear(p, 12))); return p; }
        private Panel MakeIconCircle(Point loc, int size, Color col, string g, float fs) { var p = new Panel { Location = loc, Size = new Size(size, size), BackColor = Color.Transparent }; p.Paint += (s, ev) => { ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias; ev.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(20, col.R, col.G, col.B)), p.ClientRectangle); var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }; ev.Graphics.DrawString(g, new Font("Segoe UI", fs), new SolidBrush(col), p.ClientRectangle, fmt); }; return p; }
        private static GraphicsPath RR(Rectangle r, int rad) { int d = rad * 2; var p = new GraphicsPath(); p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90); p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90); p.CloseAllFigures(); return p; }
        private static void Redondear(Control c, int r) { try { if (c.Width > 0 && c.Height > 0) c.Region = new Region(RR(c.ClientRectangle, r)); } catch { } }
    }
}
