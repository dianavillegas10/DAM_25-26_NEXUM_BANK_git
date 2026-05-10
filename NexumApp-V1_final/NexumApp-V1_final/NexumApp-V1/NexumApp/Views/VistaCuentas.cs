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
    public class VistaCuentas : UserControl
    {
        private readonly CuentaService _cuentaService = new CuentaService();
        private readonly MovimientoService _movimientoService = new MovimientoService();

        // Colores dinámicos — se adaptan al tema activo
        private Color BgDark      => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(10,  12,  28)  : Color.FromArgb(244, 247, 254);
        private Color BgCard      => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(18,  22,  45)  : Color.White;
        private Color BgCardHover => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(26,  31,  60)  : Color.FromArgb(240, 242, 250);
        private Color TextPrimary => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(241, 245, 249) : Color.FromArgb(31,  41,  55);
        private Color TextMuted   => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(100, 116, 139) : Color.FromArgb(107, 114, 128);
        private Color BorderColor => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(30,  35,  65)  : Color.FromArgb(220, 225, 235);
        // Colores de acento — iguales en ambos temas
        private static readonly Color AccentBlue   = Color.FromArgb(99,  102, 241);
        private static readonly Color AccentViolet = Color.FromArgb(139, 92,  246);
        private static readonly Color AccentGold   = Color.FromArgb(251, 191, 36);
        private static readonly Color AccentGreen  = Color.FromArgb(52,  211, 153);
        private static readonly Color AccentRed    = Color.FromArgb(248, 113, 113);

        private List<CuentaBancaria> _cuentas = new List<CuentaBancaria>();
        private CuentaBancaria _cuentaSeleccionada = null;
        private Panel _pnlSidebar;
        private Panel _pnlDetalle;
        private Panel _hoveredCard = null;

        // Moneda dinámica — sigue MonedaPreferida del usuario
        private CultureInfo Cultura => Helpers.AppSettings.CultureMoneda;

        public VistaCuentas()
        {
            BackColor = BgDark;
            Dock = DockStyle.Fill;
            // Reconstruir UI completa cuando cambia tema, idioma o moneda
            Helpers.AppSettings.ConfiguracionChanged += (s, e) =>
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(new Action(() =>
                {
                    ConstruirLayout();
                    CargarCuentas();
                }));
            };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ConstruirLayout();
            CargarCuentas();
        }

        private void ConstruirLayout()
        {
            Controls.Clear();
            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
                BackColor = Color.Transparent, Padding = new Padding(0)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 390));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _pnlSidebar = new Panel { Dock = DockStyle.Fill, BackColor = BgCard };
            _pnlSidebar.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BorderColor, 1))
                    ev.Graphics.DrawLine(pen, _pnlSidebar.Width - 1, 0, _pnlSidebar.Width - 1, _pnlSidebar.Height);
            };

            _pnlDetalle = new Panel
            {
                Dock = DockStyle.Fill, BackColor = BgDark,
                Padding = new Padding(32, 28, 32, 28), AutoScroll = true
            };

            tlp.Controls.Add(_pnlSidebar, 0, 0);
            tlp.Controls.Add(_pnlDetalle, 1, 0);
            Controls.Add(tlp);
        }

        public void CargarCuentas()
        {
            if (_pnlSidebar == null) return;
            _cuentas.Clear();
            if (SesionActual.Instancia?.Usuario == null) return;
            try
            {
                var r = _cuentaService.ObtenerCuentasPorUsuario(SesionActual.Instancia.Usuario.Id);
                _cuentas = r ?? new List<CuentaBancaria>();
            }
            catch { }
            RenderSidebar();
            if (_cuentas.Count > 0) SeleccionarCuenta(_cuentas[0]);
            else MostrarEstadoVacio();
            // Aplicar idioma activo a todos los controles
            Helpers.AppSettings.AplicarTraduccionesRecursivo(this);
        }

        // ────────────────── SIDEBAR ──────────────────
        private void RenderSidebar()
        {
            _pnlSidebar.Controls.Clear();

            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = Color.Transparent };
            pnlHeader.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BorderColor, 1))
                    ev.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };
            pnlHeader.Controls.Add(new Label { Text = "Mis Cuentas", ForeColor = TextPrimary, Font = new Font("Segoe UI", 17, FontStyle.Bold), Location = new Point(22, 18), AutoSize = true });
            pnlHeader.Controls.Add(new Label { Text = $"{_cuentas.Count} cuenta{(_cuentas.Count != 1 ? "s" : "")} activa{(_cuentas.Count != 1 ? "s" : "")}", ForeColor = TextMuted, Font = new Font("Segoe UI", 9), Location = new Point(22, 48), AutoSize = true });
            _pnlSidebar.Controls.Add(pnlHeader);

            var btnNueva = CrearBtnNuevaCuenta();
            btnNueva.Dock = DockStyle.Bottom;
            _pnlSidebar.Controls.Add(btnNueva);

            var scroll = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(14, 10, 14, 10)
            };
            scroll.SizeChanged += (s, e2) =>
            {
                foreach (Control ctrl in scroll.Controls)
                    ctrl.Width = scroll.ClientSize.Width - scroll.Padding.Horizontal;
            };

            if (_cuentas.Count == 0)
            {
                scroll.Controls.Add(new Label { Text = "Aún no tienes cuentas.\nPulsa el botón para crear una.", ForeColor = TextMuted, Font = new Font("Segoe UI", 10, FontStyle.Italic), TextAlign = ContentAlignment.MiddleCenter, AutoSize = false, Width = 340, Height = 100 });
            }
            else
            {
                decimal total = _cuentas.Sum(c => c.Saldo);
                var pnlRes = CrearPanelResumenTotal(total);
                pnlRes.Dock = DockStyle.None;
                pnlRes.Height = 68;
                scroll.Controls.Add(pnlRes);

                foreach (var cuenta in _cuentas)
                {
                    var card = CrearCardCuentaSidebar(cuenta);
                    card.Dock = DockStyle.None;
                    scroll.Controls.Add(card);
                }
            }

            _pnlSidebar.Controls.Add(scroll);
        }

        private Panel CrearPanelResumenTotal(decimal total)
        {
            var pnl = new Panel { Height = 68, Margin = new Padding(0, 0, 0, 6), BackColor = Color.Transparent };
            pnl.Controls.Add(new Label { Text = "SALDO TOTAL", ForeColor = TextMuted, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(0, 4), AutoSize = true });
            pnl.Controls.Add(new Label { Text = total.ToString("C2", Cultura), ForeColor = AccentGreen, Font = new Font("Segoe UI", 22, FontStyle.Bold), Location = new Point(0, 22), AutoSize = true });
            return pnl;
        }

        private Panel CrearCardCuentaSidebar(CuentaBancaria cuenta)
        {
            bool esSel = _cuentaSeleccionada?.Id == cuenta.Id;
            Color accent = ObtenerColorCuenta(cuenta.TipoCuenta);

            var card = new Panel { Dock = DockStyle.Top, Height = 86, Margin = new Padding(0, 0, 0, 8), Cursor = Cursors.Hand, BackColor = Color.Transparent };
            card.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                bool sel = _cuentaSeleccionada?.Id == cuenta.Id;
                bool hov = _hoveredCard == card;
                using (var path = RoundedRect(card.ClientRectangle, 12))
                {
                    Color bg = sel ? Color.FromArgb(38, accent.R, accent.G, accent.B) : hov ? BgCardHover : Color.Transparent;
                    ev.Graphics.FillPath(new SolidBrush(bg), path);
                    if (sel) using (var p = new Pen(accent, 2.5f)) ev.Graphics.DrawLine(p, 2, 10, 2, card.Height - 10);
                    else if (hov) using (var p = new Pen(BorderColor, 1)) ev.Graphics.DrawPath(p, path);
                }
            };

            var iconBox = new Panel { Size = new Size(40, 40), Location = new Point(14, 23) };
            iconBox.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(iconBox.ClientRectangle, 10))
                    ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(30, accent.R, accent.G, accent.B)), path);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString(ObtenerIconoCuenta(cuenta.TipoCuenta), new Font("Segoe UI", 14), new SolidBrush(accent), iconBox.ClientRectangle, fmt);
            };

            string ult4 = cuenta.NumeroCuenta?.Replace(" ", "").Replace("-", "") ?? "";
            if (ult4.Length > 4) ult4 = "•••• " + ult4.Substring(ult4.Length - 4);

            var lblTipo  = new Label { Text = (cuenta.TipoCuenta ?? "CUENTA").ToUpper(), ForeColor = TextMuted, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(62, 18), AutoSize = true };
            var lblNum   = new Label { Text = ult4, ForeColor = TextPrimary, Font = new Font("Consolas", 10, FontStyle.Bold), Location = new Point(62, 35), AutoSize = true };
            var lblSaldo = new Label { Text = cuenta.Saldo.ToString("C2", Cultura), ForeColor = accent, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(62, 55), AutoSize = true };

            void Sel(object s, EventArgs ev2)  { SeleccionarCuenta(cuenta); }
            void HE(object s, EventArgs ev2)   { _hoveredCard = card; card.Invalidate(); }
            void HL(object s, EventArgs ev2)   { _hoveredCard = null; card.Invalidate(); }
            foreach (Control c in new Control[] { card, iconBox, lblTipo, lblNum, lblSaldo })
            { c.Click += Sel; c.MouseEnter += HE; c.MouseLeave += HL; }

            card.Controls.AddRange(new Control[] { iconBox, lblTipo, lblNum, lblSaldo });
            return card;
        }

        private Panel CrearBtnNuevaCuenta()
        {
            var pnl = new Panel { Height = 62, BackColor = Color.Transparent, Padding = new Padding(14, 10, 14, 10) };
            pnl.Paint += (s, ev) => { using (var pen = new Pen(BorderColor, 1)) ev.Graphics.DrawLine(pen, 0, 0, pnl.Width, 0); };
            var btn = new Button { Text = "＋  Abrir nueva cuenta", Dock = DockStyle.Fill, BackColor = AccentBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => { using (var frm = new Forms.Cuentas.FrmAbrirCuenta()) if (frm.ShowDialog() == DialogResult.OK) CargarCuentas(); };
            this.BeginInvoke(new Action(() => Redondear(btn, 10)));
            pnl.Controls.Add(btn);
            return pnl;
        }

        // ────────────────── DETALLE ──────────────────
        private void SeleccionarCuenta(CuentaBancaria cuenta)
        {
            _cuentaSeleccionada = cuenta;
            RenderSidebar();
            RenderDetalle(cuenta);
        }

        private void RenderDetalle(CuentaBancaria cuenta)
        {
            _pnlDetalle.Controls.Clear();
            Color accent = ObtenerColorCuenta(cuenta.TipoCuenta);

            var inner = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, AutoScroll = true, BackColor = Color.Transparent
            };
            inner.SizeChanged += (s, e) =>
            { foreach (Control c in inner.Controls) c.Width = inner.ClientSize.Width - inner.Padding.Horizontal; };

            inner.Controls.Add(CrearSeccionCabecera(cuenta, accent));
            inner.Controls.Add(CrearTarjetaVisual(cuenta, accent));
            inner.Controls.Add(CrearSeccionStats(cuenta));
            inner.Controls.Add(CrearSeccionAcciones(cuenta, accent));
            inner.Controls.Add(CrearSeccionMovimientos(cuenta));
            _pnlDetalle.Controls.Add(inner);
        }

        private Panel CrearSeccionCabecera(CuentaBancaria cuenta, Color accent)
        {
            var pnl = new Panel { Height = 62, BackColor = Color.Transparent, Margin = new Padding(0, 0, 0, 14) };
            pnl.Controls.Add(new Label { Text = ObtenerIconoCuenta(cuenta.TipoCuenta) + "  Cuenta " + (cuenta.TipoCuenta ?? ""), ForeColor = accent, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(0, 2), AutoSize = true });
            pnl.Controls.Add(new Label { Text = FormatearIBAN(cuenta.NumeroCuenta), ForeColor = TextMuted, Font = new Font("Consolas", 10), Location = new Point(0, 30), AutoSize = true });
            pnl.Controls.Add(new Label { Text = "Abierta el " + cuenta.FechaApertura.ToString("dd MMM yyyy"), ForeColor = TextMuted, Font = new Font("Segoe UI", 9), Location = new Point(380, 30), AutoSize = true });
            return pnl;
        }

        private Panel CrearTarjetaVisual(CuentaBancaria cuenta, Color accent)
        {
            var wrapper = new Panel { Height = 204, BackColor = Color.Transparent, Margin = new Padding(0, 0, 0, 18) };

            var card = new Panel { Size = new Size(356, 200), Location = new Point(0, 2) };
            Color c2 = accent == AccentGold ? Color.FromArgb(245, 158, 11) : accent == AccentGreen ? Color.FromArgb(16, 185, 129) : AccentViolet;

            card.Paint += (s, ev) =>
            {
                var r = card.ClientRectangle;
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(r, 18))
                using (var brush = new LinearGradientBrush(r, accent, c2, 135f))
                {
                    ev.Graphics.FillPath(brush, path);
                    using (var shine = new LinearGradientBrush(new Rectangle(r.X, r.Y, r.Width, r.Height / 2), Color.FromArgb(45, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), LinearGradientMode.Vertical))
                        ev.Graphics.FillPath(shine, path);
                    using (var cb = new SolidBrush(Color.FromArgb(18, 255, 255, 255)))
                    { ev.Graphics.FillEllipse(cb, r.Right - 95, r.Top - 35, 155, 155); ev.Graphics.FillEllipse(cb, r.Right - 55, r.Bottom - 65, 105, 105); }
                }
            };

            var chip = new Panel { Size = new Size(34, 26), Location = new Point(20, 56) };
            chip.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(chip.ClientRectangle, 5))
                using (var b = new LinearGradientBrush(chip.ClientRectangle, Color.FromArgb(212, 175, 55), Color.FromArgb(255, 215, 80), 45f))
                    ev.Graphics.FillPath(b, path);
                using (var pen = new Pen(Color.FromArgb(150, 130, 20), 0.7f))
                { ev.Graphics.DrawLine(pen, 0, 13, 34, 13); ev.Graphics.DrawLine(pen, 17, 0, 17, 26); }
            };

            string ult4 = cuenta.NumeroCuenta?.Replace(" ", "").Replace("-", "") ?? "";
            if (ult4.Length > 4) ult4 = ult4.Substring(ult4.Length - 4);

            card.Controls.Add(new Label { Text = "NEXUM BANK", ForeColor = Color.FromArgb(210, 255, 255, 255), Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20), BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = (cuenta.TipoCuenta ?? "NEXUM").ToUpper(), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Location = new Point(262, 17), BackColor = Color.Transparent });
            card.Controls.Add(chip);
            card.Controls.Add(new Label { Text = $"••••   ••••   ••••   {ult4}", ForeColor = Color.White, Font = new Font("Courier New", 13, FontStyle.Bold), AutoSize = true, Location = new Point(16, 102), BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = (SesionActual.Instancia?.Usuario?.NombreCompleto ?? "TITULAR").ToUpper(), ForeColor = Color.FromArgb(210, 255, 255, 255), Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, Location = new Point(20, 158), BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = "VÁLIDA", ForeColor = Color.FromArgb(160, 255, 255, 255), Font = new Font("Segoe UI", 7), AutoSize = true, Location = new Point(268, 152), BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = cuenta.FechaApertura.AddYears(5).ToString("MM/yy"), ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), AutoSize = true, Location = new Point(265, 163), BackColor = Color.Transparent });

            // Panel saldo a la derecha
            var pnlSaldo = new Panel { Size = new Size(190, 94), Location = new Point(368, 52), BackColor = Color.Transparent };
            pnlSaldo.Controls.Add(new Label { Text = "SALDO DISPONIBLE", ForeColor = TextMuted, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) });
            pnlSaldo.Controls.Add(new Label { Text = cuenta.Saldo.ToString("C2", Cultura), ForeColor = accent, Font = new Font("Segoe UI", 20, FontStyle.Bold), AutoSize = true, Location = new Point(0, 18), MaximumSize = new Size(190, 0) });

            var badge = new Panel { Size = new Size(64, 22), Location = new Point(0, 68) };
            badge.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(badge.ClientRectangle, 6))
                    ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(28, 52, 211, 153)), path);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString("● ACTIVA", new Font("Segoe UI", 7, FontStyle.Bold), new SolidBrush(AccentGreen), badge.ClientRectangle, fmt);
            };
            pnlSaldo.Controls.Add(badge);

            wrapper.Controls.Add(card);
            wrapper.Controls.Add(pnlSaldo);
            return wrapper;
        }

        private Panel CrearSeccionStats(CuentaBancaria cuenta)
        {
            var pnl = new Panel { Height = 94, BackColor = Color.Transparent, Margin = new Padding(0, 0, 0, 18) };
            decimal ingresos = 0, gastos = 0;
            try
            {
                var movs = _movimientoService.ObtenerMovimientosPorCuenta(cuenta.Id, 100) ?? new List<Movimiento>();
                var mes = movs.Where(m => m.Fecha.Month == DateTime.Now.Month && m.Fecha.Year == DateTime.Now.Year);
                ingresos = mes.Where(m => m.TipoMovimiento == "Ingreso").Sum(m => m.Monto);
                gastos   = mes.Where(m => m.TipoMovimiento != "Ingreso").Sum(m => m.Monto);
            }
            catch { }

            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Color.Transparent };
            for (int i = 0; i < 3; i++) tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
            tlp.Controls.Add(CrearStatCard("↑  INGRESOS MES",  ingresos.ToString("C0", Cultura), AccentGreen), 0, 0);
            tlp.Controls.Add(CrearStatCard("↓  GASTOS MES",    gastos.ToString("C0", Cultura),   AccentRed),   1, 0);
            tlp.Controls.Add(CrearStatCard("📅  APERTURA",      cuenta.FechaApertura.ToString("MMM yyyy"),                          AccentBlue),  2, 0);
            pnl.Controls.Add(tlp);
            return pnl;
        }

        private Panel CrearStatCard(string titulo, string valor, Color color)
        {
            var card = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0) };
            card.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(card.ClientRectangle, 12))
                {
                    ev.Graphics.FillPath(new SolidBrush(BgCard), path);
                    ev.Graphics.DrawPath(new Pen(BorderColor, 1), path);
                }
            };
            card.Controls.Add(new Label { Text = titulo, ForeColor = TextMuted, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(14, 12), AutoSize = true });
            card.Controls.Add(new Label { Text = valor, ForeColor = color, Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(14, 36), AutoSize = true, MaximumSize = new Size(150, 0) });
            return card;
        }

        private Panel CrearSeccionAcciones(CuentaBancaria cuenta, Color accent)
        {
            var pnl = new Panel { Height = 56, BackColor = Color.Transparent, Margin = new Padding(0, 0, 0, 22) };
            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = Color.Transparent };
            for (int i = 0; i < 4; i++) tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            var acciones = new (string txt, Color col, Action action)[]
            {
                ("📥 Ingresar",  AccentGreen, () => AbrirIngreso(cuenta)),
                ("📤 Retirar",   AccentRed,   () => AbrirRetiro(cuenta)),
                ("✈ Transferir", AccentBlue,  () => AbrirTransferencia()),
                ("📋 Historial", TextMuted,   () => AbrirHistorial(cuenta))
            };

            for (int i = 0; i < acciones.Length; i++)
            {
                var (txt, col, accion) = acciones[i];
                var btn = new Button
                {
                    Text = txt, Dock = DockStyle.Fill, Margin = new Padding(0, 0, i < 3 ? 10 : 0, 0),
                    BackColor = Color.FromArgb(22, col.R, col.G, col.B), ForeColor = col,
                    FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(55, col.R, col.G, col.B);
                btn.FlatAppearance.BorderSize = 1;
                btn.Click += (s, e) => accion();
                this.BeginInvoke(new Action(() => Redondear(btn, 10)));
                tlp.Controls.Add(btn, i, 0);
            }
            pnl.Controls.Add(tlp);
            return pnl;
        }

        private Panel CrearSeccionMovimientos(CuentaBancaria cuenta)
        {
            var pnl = new Panel { MinimumSize = new Size(0, 100), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent };

            var pnlHead = new Panel { Height = 36, Dock = DockStyle.Top, BackColor = Color.Transparent };
            pnlHead.Controls.Add(new Label { Text = "Últimos movimientos", ForeColor = TextPrimary, Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(0, 4), AutoSize = true });
            pnl.Controls.Add(pnlHead);

            List<Movimiento> movs = new List<Movimiento>();
            try { movs = _movimientoService.ObtenerMovimientosPorCuenta(cuenta.Id, 8) ?? movs; } catch { }

            if (movs.Count == 0)
            {
                pnl.Controls.Add(new Label { Text = "Sin movimientos registrados.", ForeColor = TextMuted, Font = new Font("Segoe UI", 10, FontStyle.Italic), Location = new Point(0, 44), AutoSize = true });
                pnl.Height = 80;
            }
            else
            {
                int y = 44;
                foreach (var mov in movs) { var f = CrearFilaMovimiento(mov); f.Location = new Point(0, y); pnl.Controls.Add(f); y += 56; }
                pnl.Height = y + 10;
            }
            return pnl;
        }

        private Panel CrearFilaMovimiento(Movimiento mov)
        {
            bool esIngreso = mov.TipoMovimiento?.Equals("Ingreso", StringComparison.OrdinalIgnoreCase) == true;
            Color col = esIngreso ? AccentGreen : AccentRed;

            var fila = new Panel { Size = new Size(600, 50), BackColor = Color.Transparent };
            fila.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(new Rectangle(0, 2, fila.Width, fila.Height - 4), 10))
                    ev.Graphics.FillPath(new SolidBrush(BgCard), path);
            };

            var ico = new Panel { Size = new Size(34, 34), Location = new Point(10, 8) };
            ico.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(ico.ClientRectangle, 10))
                    ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(25, col.R, col.G, col.B)), path);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString(esIngreso ? "↑" : "↓", new Font("Segoe UI", 14, FontStyle.Bold), new SolidBrush(col), ico.ClientRectangle, fmt);
            };

            fila.Controls.Add(ico);
            fila.Controls.Add(new Label { Text = mov.Concepto ?? mov.TipoMovimiento, ForeColor = TextPrimary, Font = new Font("Segoe UI", 10), Location = new Point(52, 7), AutoSize = true });
            fila.Controls.Add(new Label { Text = mov.Fecha.ToString("dd MMM yyyy · HH:mm"), ForeColor = TextMuted, Font = new Font("Segoe UI", 8), Location = new Point(52, 27), AutoSize = true });
            fila.Controls.Add(new Label { Text = (esIngreso ? "+" : "-") + mov.Monto.ToString("C2", Cultura), ForeColor = col, Font = new Font("Segoe UI", 11, FontStyle.Bold), Anchor = AnchorStyles.Top | AnchorStyles.Right, AutoSize = true, Location = new Point(455, 13) });
            return fila;
        }

        private void MostrarEstadoVacio()
        {
            _pnlDetalle.Controls.Clear();
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            pnl.Controls.Add(new Label { Text = "💳\n\nAún no tienes cuentas.\nPulsa el botón para abrir tu primera cuenta.", ForeColor = TextMuted, Font = new Font("Segoe UI", 12, FontStyle.Italic), TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill });
            _pnlDetalle.Controls.Add(pnl);
        }

        // ────────────────── ACCIONES ──────────────────
        private void AbrirIngreso(CuentaBancaria cuenta)
        {
            using (var frm = new Forms.Movimientos.FrmIngresarEfectivo(cuenta))
                if (frm.ShowDialog() == DialogResult.OK) { CargarCuentas(); if (_cuentaSeleccionada != null) SeleccionarCuenta(_cuentaSeleccionada); }
        }

        private void AbrirRetiro(CuentaBancaria cuenta)
        {
            using (var frm = new Forms.Movimientos.FrmRetirarEfectivo(cuenta))
                if (frm.ShowDialog() == DialogResult.OK) { CargarCuentas(); if (_cuentaSeleccionada != null) SeleccionarCuenta(_cuentaSeleccionada); }
        }

        private void AbrirTransferencia()
        {
            var frm = new Form { Text = "Nueva Transferencia | Nexum Bank", Size = new Size(700, 600), StartPosition = FormStartPosition.CenterParent, BackColor = BgDark };
            var vista = new VistaNuevaTransferencia { Dock = DockStyle.Fill };
            frm.Controls.Add(vista);
            frm.ShowDialog();
            CargarCuentas();
        }

        private void AbrirHistorial(CuentaBancaria cuenta)
        {
            using (var frm = new Forms.Movimientos.FrmHistorialMovimientos()) frm.ShowDialog();
        }

        // ────────────────── HELPERS ──────────────────
        private static Color ObtenerColorCuenta(string tipo)
        {
            if (tipo == null) return AccentBlue;
            switch (tipo.ToLower()) { case "ahorro": return AccentGreen; case "nomina": case "nómina": return AccentGold; default: return AccentBlue; }
        }

        private static string ObtenerIconoCuenta(string tipo)
        {
            if (tipo == null) return "💳";
            switch (tipo.ToLower()) { case "ahorro": return "🏦"; case "nomina": case "nómina": return "💼"; default: return "💳"; }
        }

        private static string FormatearIBAN(string iban)
        {
            if (string.IsNullOrEmpty(iban)) return "—";
            var limpio = iban.Replace(" ", "").Replace("-", "");
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < limpio.Length; i++) { if (i > 0 && i % 4 == 0) sb.Append(' '); sb.Append(limpio[i]); }
            return sb.ToString();
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseAllFigures();
            return path;
        }

        private static void Redondear(Control c, int r)
        {
            if (c.Width <= 0 || c.Height <= 0) return;
            c.Region = new Region(RoundedRect(c.ClientRectangle, r));
        }
    }
}
