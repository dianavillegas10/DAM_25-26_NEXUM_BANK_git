using NexumApp.Helpers;
using NexumApp.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using static NexumApp.Helpers.DashboardLayout;

namespace NexumApp.Views
{
    public class UC_Inicio : UserControl
    {
        private const int SectionSpacing = 12;
        private const int SectionPadding = 14;
        private const int PanelSaldoHeight = 140;
        private const int PanelMovimientosHeight = 300;
        private const int RightColumnMinWidth = 390;

        private Panel panelSaldo;
        private Panel panelPresupuesto;
        private Panel panelObjetivos;
        private Panel panelMovimientos;

        private Label lblSaldo, lblCuenta;
        private ComboBox cmbCuentasActivas;
        private Label lblPresupuestoRestante, lblPresupuestoRestanteLabel;
        private Label lblIngresos, lblGastos, lblObjetivo;
        private Label lblPresupuestoMensaje, lblPuntos;
        private FlowLayoutPanel flpMovimientosItems;
        private Panel pnlObjetivosContenedor;
        private bool _saldoVisible = true;
        private decimal _saldoActual;
        private string _textoCuenta;
        private bool _cargandoComboCuentas;

        // Datos para el donut del presupuesto (se actualizan en LoadPresupuesto)
        private float _donutIngresos = 0f;
        private float _donutGastos = 0f;
        private Panel _donutPanel;

        // Controles de la tarjeta principal (se actualizan con la cuenta activa)
        private Label _lblTarjetaNumero;
        private Label _lblTarjetaTitular;
        private Label _lblTarjetaVencimiento;
        private Label _lblTarjetaTipo;
        private Label _lblTarjetaCuentaAsociada;  // ← NUEVO: label para mostrar cuenta asociada
        private int _cuentaActivaId = 0;  // ← NUEVO: guarda el ID de la cuenta activa
        private List<CuentaBancaria> _cuentasParaTarjeta = new List<CuentaBancaria>();
        private List<Tarjeta> _tarjetasActuales = new List<Tarjeta>();

        public event EventHandler TarjetasVerTodoClicked;
        public event EventHandler HuchaCreada;
        public event EventHandler HuchasVerTodoClicked;
        public event EventHandler<(int HuchaId, decimal Monto)> AbonarHuchaRequested;
        public event EventHandler<int> EditarHuchaRequested;
        public event EventHandler<int> EliminarHuchaRequested;
        public event EventHandler EnviarDineroClicked;
        public event EventHandler RecibirDineroClicked;
        public event EventHandler VerTodosClicked;
        public event EventHandler<int> AccesoRapidoClicked;
        public event EventHandler FondosIndexadosClicked;
        public event EventHandler BizumClicked;
        public event EventHandler PresupuestoDetallesClicked;
        public event EventHandler ObjetivosVerTodoClicked;
        public event EventHandler BeneficiosVerTodoClicked;
        public event EventHandler RetoParticiparClicked;
        public event EventHandler<int> CuentaActivaChanged;

        public UC_Inicio()
        {
            BackColor = FondoPrincipal;
            Dock = DockStyle.Fill;
            // Reconstruir cuando cambia tema o idioma
            AppSettings.ConfiguracionChanged += (s, e) =>
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(new Action(() =>
                {
                    if (IsDisposed) return;
                    BackColor = FondoPrincipal;
                    ConstruirUI();
                }));
            };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ConstruirUI();
        }

        #region Módulos UI

        private void ConstruirUI()
        {
            Controls.Clear();
            var tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var panelColumnaIzquierda = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, SectionSpacing, 0),
                AutoScroll = true
            };
            var panelColumnaDerecha = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(SectionSpacing, 0, 0, 0),
                AutoScroll = true,
                MinimumSize = new Size(RightColumnMinWidth, 0)
            };

            var panelNovedades = CrearPanelNovedades();
            panelNovedades.Dock = DockStyle.Top;

            panelMovimientos = CrearPanelMovimientos();
            panelMovimientos.Dock = DockStyle.Top;

            var panelAccesos = CrearPanelAccesos();
            panelAccesos.Dock = DockStyle.Top;

            panelSaldo = CrearPanelSaldo();
            panelSaldo.Dock = DockStyle.Top;

            panelColumnaIzquierda.Controls.Add(panelNovedades);
            panelColumnaIzquierda.Controls.Add(panelMovimientos);
            panelColumnaIzquierda.Controls.Add(panelAccesos);
            panelColumnaIzquierda.Controls.Add(panelSaldo);

            var panelTarjetas = CrearPanelTarjetas();
            panelTarjetas.Dock = DockStyle.Top;

            _panelHuchasContainer = CrearPanelHuchas();
            var panelHuchas = _panelHuchasContainer;
            panelHuchas.Dock = DockStyle.Top;

            var panelPresupuestoSeccion = CrearPanelPresupuesto();
            panelPresupuestoSeccion.Dock = DockStyle.Top;

            // Orden de add = orden visual inverso (el último add = arriba)
            panelColumnaDerecha.Controls.Add(panelPresupuestoSeccion);
            panelColumnaDerecha.Controls.Add(panelTarjetas);
            panelColumnaDerecha.Controls.Add(panelHuchas);             // arriba

            tlpMain.Controls.Add(panelColumnaIzquierda, 0, 0);
            tlpMain.Controls.Add(panelColumnaDerecha, 1, 0);
            Controls.Add(tlpMain);
        }

        private static Panel CrearSectionContainer(int height, int marginBottom)
        {
            return new Panel
            {
                Dock = DockStyle.Top,
                Height = height,
                Margin = new Padding(0, 0, 0, marginBottom),
                BackColor = Color.Transparent
            };
        }

        private Panel CrearPanelSaldo()
        {
            var section = CrearSectionContainer(PanelSaldoHeight, SectionSpacing);
            var card = new Panel { Dock = DockStyle.Fill, Padding = new Padding(SectionPadding) };
            card.Paint += (s, e) =>
            {
                var r = card.ClientRectangle;
                using (var path = UiHelper.CrearRoundedRect(r, BorderRadius))
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(r, Color.FromArgb(37, 99, 235), GradienteFin, 45f))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);
                }
            };

            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));

            var flpIzq = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, BackColor = Color.Transparent };
            var pnlTitulo = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent };
            pnlTitulo.Controls.Add(new Label { Text = "Saldo disponible", ForeColor = Color.FromArgb(255, 255, 255, 230), Font = new Font("Segoe UI", 11), AutoSize = true });
            var btnOjo = new Label { Text = "👁", Font = new Font("Segoe UI", 11), ForeColor = Color.White, Cursor = Cursors.Hand, AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
            btnOjo.Click += (s, ev) => ToggleSaldo();
            pnlTitulo.Controls.Add(btnOjo);
            lblSaldo = new Label { Text = "", ForeColor = Color.White, Font = new Font("Segoe UI", 26, FontStyle.Bold), AutoSize = true, MaximumSize = new Size(350, 0), Margin = new Padding(0, 4, 0, 0) };
            lblCuenta = new Label { Text = "", ForeColor = Color.FromArgb(255, 255, 255, 230), Font = new Font("Segoe UI", 10), AutoSize = true, MaximumSize = new Size(400, 0), Margin = new Padding(0, 4, 0, 0) };
            cmbCuentasActivas = new ComboBox
            {
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(226, 232, 255),
                ForeColor = Color.FromArgb(34, 44, 90),
                Margin = new Padding(0, 6, 0, 0)
            };
            cmbCuentasActivas.SelectedIndexChanged += CmbCuentasActivas_SelectedIndexChanged;
            flpIzq.Controls.Add(pnlTitulo);
            flpIzq.Controls.Add(lblSaldo);
            flpIzq.Controls.Add(lblCuenta);
            flpIzq.Controls.Add(cmbCuentasActivas);

            var tlpBtns = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0),
                BackColor = Color.Transparent
            };
            tlpBtns.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpBtns.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            var btnEnviar = CrearBotonAccionRapida("Enviar dinero", "enviar", (s, ev) => EnviarDineroClicked?.Invoke(this, EventArgs.Empty));
            var btnRecibir = CrearBotonAccionRapida("Recibir dinero", "recibir", (s, ev) => RecibirDineroClicked?.Invoke(this, EventArgs.Empty));
            btnEnviar.Dock = DockStyle.Fill; btnEnviar.Margin = new Padding(0, 6, 0, 4);
            btnRecibir.Dock = DockStyle.Fill; btnRecibir.Margin = new Padding(0, 4, 0, 6);
            tlpBtns.Controls.Add(btnEnviar, 0, 0);
            tlpBtns.Controls.Add(btnRecibir, 0, 1);

            tlp.Controls.Add(flpIzq, 0, 0);
            tlp.Controls.Add(tlpBtns, 1, 0);
            card.Controls.Add(tlp);
            section.Controls.Add(card);
            return section;
        }

        private Panel CrearPanelAccesos()
        {
            var section = CrearSectionContainer(128, SectionSpacing);
            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(SectionPadding), BackColor = CardFondo };
            pnl.Paint += (s, e) =>
            {
                using (var path = UiHelper.CrearRoundedRect(pnl.ClientRectangle, BorderRadius))
                using (var b = new SolidBrush(CardFondo))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(b, path);
                    e.Graphics.DrawPath(new Pen(BorderCard), path);
                }
            };
            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 1, BackColor = Color.Transparent };
            for (int i = 0; i < 6; i++) tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 6));

            var items = new[] {
                ("🎁", "Cashback",        Color.FromArgb(254, 243, 199), Color.FromArgb(245,158, 11)),
                ("🧾", "Pagar servicios", Color.FromArgb(209, 250, 229), Color.FromArgb( 16,185,129)),
                ("📱", "Recargar móvil",  Color.FromArgb(219, 234, 254), Color.FromArgb( 59,130,246)),
                ("📈", "Invertir",        Color.FromArgb(255, 237, 213), Color.FromArgb(249,115, 22)),
                ("📊", "Analizar gastos", Color.FromArgb(224, 231, 255), Color.FromArgb(139, 92,246)),
                ("👥", "Dividir cuenta",  Color.FromArgb(237, 233, 254), Color.FromArgb(100,116,139))
            };
            for (int i = 0; i < items.Length; i++)
            {
                var idx = i;
                var item = CrearItemAcceso(items[i].Item1, items[i].Item2, items[i].Item3, items[i].Item4);
                item.Dock = DockStyle.Fill;
                item.Margin = new Padding(4, 2, 4, 2);
                EventHandler clickHandler = (s, ev) => AccesoRapidoClicked?.Invoke(this, idx);
                PropagateClickRecursivo(item, clickHandler);
                tlp.Controls.Add(item, i, 0);
            }
            pnl.Controls.Add(tlp);
            section.Controls.Add(pnl);
            return section;
        }

        private Panel CrearItemAcceso(string icono, string texto, Color bgColor, Color accentColor)
        {
            bool hovered = false;
            bool pressed = false;
            const int CIRC = 52;

            var p = new Panel { Cursor = Cursors.Hand, BackColor = Color.Transparent };

            // Fondo blanco con borde redondeado + hover
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var r = p.ClientRectangle;
                Color bg = pressed ? BgPressed
                         : hovered ? BgHover
                         : CardFondo;
                Color border = hovered ? accentColor : BorderCard;
                using (var path = UiHelper.CrearRoundedRect(r, 14))
                {
                    e.Graphics.FillPath(new SolidBrush(bg), path);
                    e.Graphics.DrawPath(new Pen(border, hovered ? 1.6f : 1f), path);
                }
            };

            // Círculo de color con emoji
            var circ = new Panel { Size = new Size(CIRC, CIRC), BackColor = Color.Transparent, Cursor = Cursors.Hand };
            circ.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Color cirBg = pressed
                    ? Color.FromArgb(Math.Max(0, bgColor.R - 20), Math.Max(0, bgColor.G - 20), Math.Max(0, bgColor.B - 20))
                    : hovered
                    ? Color.FromArgb(Math.Max(0, bgColor.R - 12), Math.Max(0, bgColor.G - 12), Math.Max(0, bgColor.B - 12))
                    : bgColor;
                using (var path = UiHelper.CrearRoundedRect(circ.ClientRectangle, CIRC / 2))
                    e.Graphics.FillPath(new SolidBrush(cirBg), path);
                TextRenderer.DrawText(e.Graphics, icono,
                    new Font("Segoe UI Emoji", 22),
                    circ.ClientRectangle, accentColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            // Texto centrado debajo
            var lbl = new Label
            {
                Text = texto,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = TextoOscuro,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Cursor = Cursors.Hand
            };

            p.Controls.Add(circ);
            p.Controls.Add(lbl);

            // Posicionar circ y lbl centrados al redimensionar
            p.Resize += (s, e) =>
            {
                int w = p.Width, h = p.Height;
                int topPad = Math.Max(6, (h - CIRC - 22) / 2);
                circ.Location = new Point((w - CIRC) / 2, topPad);
                lbl.Location = new Point(4, circ.Bottom + 5);
                lbl.Size = new Size(w - 8, 20);
            };

            // Hover + pressed: propagar a todos los controles hijos
            EventHandler enter = (s, e) => { hovered = true; p.Invalidate(true); circ.Invalidate(); };
            EventHandler leave = (s, e) => { hovered = false; pressed = false; p.Invalidate(true); circ.Invalidate(); };
            MouseEventHandler down = (s, e) => { pressed = true; p.Invalidate(true); circ.Invalidate(); };
            MouseEventHandler up = (s, e) => { pressed = false; p.Invalidate(true); circ.Invalidate(); };

            foreach (var ctrl in new Control[] { p, circ, lbl })
            {
                ctrl.MouseEnter += enter;
                ctrl.MouseLeave += leave;
                ctrl.MouseDown += down;
                ctrl.MouseUp += up;
                ctrl.Cursor = Cursors.Hand;
            }

            return p;
        }

        // Propaga el click a TODOS los controles hijos de forma recursiva
        private static void PropagateClickRecursivo(Control ctrl, EventHandler handler)
        {
            ctrl.Click += handler;
            ctrl.Cursor = Cursors.Hand;
            foreach (Control c in ctrl.Controls)
                PropagateClickRecursivo(c, handler);
        }

        private Panel CrearPanelMovimientos()
        {
            var section = CrearSectionContainer(PanelMovimientosHeight, SectionSpacing);
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = CardFondo, Padding = new Padding(SectionPadding) };
            pnl.Paint += (s, e) =>
            {
                using (var path = UiHelper.CrearRoundedRect(pnl.ClientRectangle, BorderRadius))
                using (var b = new SolidBrush(CardFondo))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(b, path);
                    e.Graphics.DrawPath(new Pen(BorderCard), path);
                }
            };

            var tlpHead = new TableLayoutPanel { Dock = DockStyle.Top, Height = 44, ColumnCount = 2, RowCount = 1 };
            tlpHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpHead.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            tlpHead.Controls.Add(new Label { Text = "Movimientos Recientes", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = TextoOscuro, AutoSize = true, MaximumSize = new Size(300, 0) }, 0, 0);
            var lblVerTodos = new Label { Text = "Ver todos >", Font = new Font("Segoe UI", 11), ForeColor = AzulPrimario, Cursor = Cursors.Hand, AutoSize = true };
            lblVerTodos.Click += (s, ev) => VerTodosClicked?.Invoke(this, EventArgs.Empty);
            tlpHead.Controls.Add(lblVerTodos, 1, 0);

            flpMovimientosItems = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(0, 8, 0, 0) };
            flpMovimientosItems.SizeChanged += (s, e) => AjustarAnchoFilasMovimientos();
            pnl.Controls.Add(flpMovimientosItems);
            pnl.Controls.Add(tlpHead);
            section.Controls.Add(pnl);
            return section;
        }

        private Panel CrearPanelNovedades()
        {
            var section = CrearSectionContainer(205, 0);
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = CardFondo, Padding = new Padding(SectionPadding) };
            pnl.Paint += (s, e) =>
            {
                using (var path = UiHelper.CrearRoundedRect(pnl.ClientRectangle, BorderRadius))
                using (var b = new SolidBrush(CardFondo))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(b, path);
                    e.Graphics.DrawPath(new Pen(BorderCard), path);
                }
            };

            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var headerNovedades = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            headerNovedades.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // Barra izquierda degradada
                using (var b = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(0, 4, 3, 28),
                    Color.FromArgb(99, 102, 241), Color.FromArgb(139, 92, 246), 90f))
                    g.FillRectangle(b, 0, 5, 3, 26);

                // Título
                using (var f = new Font("Segoe UI", 14, FontStyle.Bold))
                using (var tb = new SolidBrush(TextoOscuro))
                    g.DrawString("Novedades", f, tb, new PointF(12, 5));

                // Subtítulo
                using (var f = new Font("Segoe UI", 8.5f))
                using (var sb = new SolidBrush(Color.FromArgb(156, 163, 175)))
                    g.DrawString("Lo más nuevo en Nexum", f, sb, new PointF(14, 22));
            };
            tlp.Controls.Add(headerNovedades, 0, 0);

            var tlpCards = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            var card1 = CrearCardNovedad("Becas del Banco", "Programas para estudiantes y formación", "NUEVO", Color.FromArgb(30, 41, 59), "Ver becas", (s, e) => FondosIndexadosClicked?.Invoke(this, e));
            var card2 = CrearCardNovedad("Plan de Pensiones", "Ahorra hoy para tu jubilación", "POPULAR", Color.FromArgb(245, 158, 11), "Explorar plan", (s, e) => BizumClicked?.Invoke(this, e));
            card1.Margin = new Padding(0, 0, Margen / 2, 0);
            card2.Margin = new Padding(Margen / 2, 0, 0, 0);
            tlpCards.Controls.Add(card1, 0, 0);
            tlpCards.Controls.Add(card2, 1, 0);
            tlp.Controls.Add(tlpCards, 0, 1);

            pnl.Controls.Add(tlp);
            section.Controls.Add(pnl);
            return section;
        }

        private Panel CrearCardNovedad(string titulo, string subtitulo, string badge, Color bg, string btnTexto, EventHandler click)
        {
            var card = new Panel { Dock = DockStyle.Fill, BackColor = bg, Cursor = Cursors.Hand };
            card.Paint += (s, e) =>
            {
                using (var path = UiHelper.CrearRoundedRect(card.ClientRectangle, BorderRadius))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(new SolidBrush(bg), path);
                }
            };
            var flp = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(PaddingCard) };
            var lblBadge = new Label { Text = badge, Font = new Font("Segoe UI", 7, FontStyle.Bold), ForeColor = Color.White, BackColor = badge == "NUEVO" ? GradienteFin : Color.FromArgb(16, 185, 129), AutoSize = true };
            flp.Controls.Add(lblBadge);
            flp.Controls.Add(new Label { Text = titulo, ForeColor = Color.White, Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true, MaximumSize = new Size(250, 0) });
            flp.Controls.Add(new Label { Text = subtitulo, ForeColor = Color.FromArgb(200, 200, 200), Font = new Font("Segoe UI", 10), AutoSize = true, MaximumSize = new Size(250, 0) });
            var btn = new Button { Text = btnTexto, BackColor = bg == Color.FromArgb(245, 158, 11) ? Color.White : AzulPrimario, ForeColor = bg == Color.FromArgb(245, 158, 11) ? Color.FromArgb(245, 158, 11) : Color.White, FlatStyle = FlatStyle.Flat, Height = 32, Cursor = Cursors.Hand, Margin = new Padding(0, 8, 0, 0) };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, ev) => click?.Invoke(this, EventArgs.Empty);
            flp.Controls.Add(btn);
            card.Controls.Add(flp);
            return card;
        }

        private Panel CrearPanelPresupuesto()
        {
            var section = CrearSectionContainer(310, SectionSpacing);
            panelPresupuesto = new Panel { Dock = DockStyle.Fill, BackColor = CardFondo, Padding = new Padding(SectionPadding) };
            panelPresupuesto.Paint += (s, e) =>
            {
                using (var path = UiHelper.CrearRoundedRect(panelPresupuesto.ClientRectangle, BorderRadius))
                using (var b = new SolidBrush(CardFondo))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(b, path);
                    e.Graphics.DrawPath(new Pen(BorderCard), path);
                }
            };

            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 116));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Fila 0 — Cabecera
            var tlpHead = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
            tlpHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpHead.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
            tlpHead.Controls.Add(new Label { Text = "Presupuesto del Mes", Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold), ForeColor = TextoOscuro, AutoSize = true }, 0, 0);
            var lblVerDetalles = new Label { Text = "Ver detalles", Font = new Font("Segoe UI", 10), ForeColor = AzulPrimario, Cursor = Cursors.Hand, AutoSize = true };
            lblVerDetalles.Click += (s, e) => PresupuestoDetallesClicked?.Invoke(this, EventArgs.Empty);
            tlpHead.Controls.Add(lblVerDetalles, 1, 0);
            tlp.Controls.Add(tlpHead, 0, 0);

            // Fila 1 — Donut + cifra central
            _donutPanel = new Panel { Height = 116 };
            _donutPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var rect = new Rectangle(10, 8, 96, 96);
                // Fondo (pista gris)
                e.Graphics.DrawArc(new Pen(Color.FromArgb(231, 235, 244), 12), rect, 0, 360);

                float total = _donutIngresos + _donutGastos;
                if (total > 0)
                {
                    float angIngreso = 360f * (_donutIngresos / total);
                    float angGasto = 360f * (_donutGastos / total);
                    // Arco ingresos (verde)
                    using (var pen = new Pen(Color.FromArgb(16, 185, 129), 12) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round })
                        e.Graphics.DrawArc(pen, rect, -90, angIngreso);
                    // Arco gastos (naranja)
                    using (var pen = new Pen(Color.FromArgb(249, 115, 22), 12) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round })
                        e.Graphics.DrawArc(pen, rect, -90 + angIngreso, angGasto);
                }
                else
                {
                    // Sin datos: arco morado (objetivo)
                    e.Graphics.DrawArc(new Pen(Color.FromArgb(124, 58, 237), 12), rect, -90, 360);
                }
            };
            lblPresupuestoRestante = new Label { Text = "—", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = TextoOscuro, AutoSize = true, Location = new Point(24, 34) };
            lblPresupuestoRestanteLabel = new Label { Text = "Restante", Font = new Font("Segoe UI", 8.5F), ForeColor = TextoGris, AutoSize = true, Location = new Point(28, 58) };
            _donutPanel.Controls.Add(lblPresupuestoRestante);
            _donutPanel.Controls.Add(lblPresupuestoRestanteLabel);
            tlp.Controls.Add(_donutPanel, 0, 1);

            // Fila 2 — Leyenda (con margen para que quede al lado del donut)
            var tlpLeyenda = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, Margin = new Padding(120, 0, 0, 0), BackColor = Color.Transparent };
            tlpLeyenda.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            tlpLeyenda.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            tlpLeyenda.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            tlpLeyenda.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpLeyenda.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));

            tlpLeyenda.Controls.Add(new Label { Text = "●  Ingresos", Font = new Font("Segoe UI", 9.5F), ForeColor = Color.FromArgb(16, 185, 129), AutoSize = true }, 0, 0);
            lblIngresos = new Label { Text = "—", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = TextoOscuro, AutoSize = true };
            tlpLeyenda.Controls.Add(lblIngresos, 1, 0);

            tlpLeyenda.Controls.Add(new Label { Text = "●  Gastos", Font = new Font("Segoe UI", 9.5F), ForeColor = Color.FromArgb(249, 115, 22), AutoSize = true }, 0, 1);
            lblGastos = new Label { Text = "—", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = TextoOscuro, AutoSize = true };
            tlpLeyenda.Controls.Add(lblGastos, 1, 1);

            tlpLeyenda.Controls.Add(new Label { Text = "●  Objetivo", Font = new Font("Segoe UI", 9.5F), ForeColor = Color.FromArgb(124, 58, 237), AutoSize = true }, 0, 2);
            lblObjetivo = new Label { Text = "—", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = TextoOscuro, AutoSize = true };
            tlpLeyenda.Controls.Add(lblObjetivo, 1, 2);
            tlp.Controls.Add(tlpLeyenda, 0, 2);

            // Fila 3 — Badge motivacional
            lblPresupuestoMensaje = new Label
            {
                Text = "",
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(5, 150, 105),
                BackColor = Color.FromArgb(220, 252, 231),
                AutoSize = true,
                Padding = new Padding(12, 5, 12, 5),
                Visible = false
            };
            tlp.Controls.Add(lblPresupuestoMensaje, 0, 3);

            panelPresupuesto.Controls.Add(tlp);
            section.Controls.Add(panelPresupuesto);
            return section;
        }

        private Panel CrearPanelObjetivos()
        {
            var section = CrearSectionContainer(250, SectionSpacing);
            panelObjetivos = new Panel { Dock = DockStyle.Fill, BackColor = CardFondo, Padding = new Padding(SectionPadding) };
            panelObjetivos.Paint += (s, e) =>
            {
                using (var path = UiHelper.CrearRoundedRect(panelObjetivos.ClientRectangle, BorderRadius))
                using (var b = new SolidBrush(CardFondo))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(b, path);
                    e.Graphics.DrawPath(new Pen(BorderCard), path);
                }
            };

            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var tlpHead = new TableLayoutPanel { Height = 36, ColumnCount = 2, RowCount = 1 };
            tlpHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpHead.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78));
            tlpHead.Controls.Add(new Label { Text = "Objetivos de Ahorro", Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold), ForeColor = TextoOscuro, AutoSize = true }, 0, 0);
            var lblVerObjetivos = new Label { Text = "Ver todo", Font = new Font("Segoe UI", 10), ForeColor = AzulPrimario, Cursor = Cursors.Hand, AutoSize = true };
            lblVerObjetivos.Click += (s, e) => ObjetivosVerTodoClicked?.Invoke(this, EventArgs.Empty);
            tlpHead.Controls.Add(lblVerObjetivos, 1, 0);
            tlp.Controls.Add(tlpHead, 0, 0);
            pnlObjetivosContenedor = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            tlp.Controls.Add(pnlObjetivosContenedor, 0, 1);
            panelObjetivos.Controls.Add(tlp);
            section.Controls.Add(panelObjetivos);
            return section;
        }

        // Campos para el status bar y botón de bloquear (actualizables desde LoadTarjetas)
        private Label _lblTarjetaEstado;
        private Label _lblTarjetaLimite;
        private Panel _pnlAccionesTarjeta;

        private Panel CrearPanelTarjetas()
        {
            var section = CrearSectionContainer(370, SectionSpacing); // Aumentado ligeramente para el nuevo label
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(18, 22, 40), Padding = new Padding(SectionPadding) };
            pnl.Paint += (s, e) =>
            {
                using (var path = UiHelper.CrearRoundedRect(pnl.ClientRectangle, BorderRadius))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(new SolidBrush(Color.FromArgb(18, 22, 40)), path);
                }
            };

            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7 }; // Aumentado a 7 filas
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // 0: Header
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 150)); // 1: Tarjeta grande
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // 2: Cuenta asociada (NUEVA)
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));  // 3: Acciones rápidas
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));  // 4: Status bar
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));  // 5: Botón nueva cuenta
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // 6: Relleno

            // Fila 0 — Cabecera
            var tlpHead = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
            tlpHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpHead.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            tlpHead.Controls.Add(new Label { Text = "Mis Tarjetas", Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold), ForeColor = Color.White, AutoSize = true }, 0, 0);
            var lblVerTodo = new Label { Text = "Ver todo", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(147, 171, 255), Cursor = Cursors.Hand, AutoSize = true };
            lblVerTodo.Click += (s, e) => TarjetasVerTodoClicked?.Invoke(this, EventArgs.Empty);
            tlpHead.Controls.Add(lblVerTodo, 1, 0);
            tlp.Controls.Add(tlpHead, 0, 0);

            // Fila 1 — Tarjeta principal (estilo credit card)
            var mainCard = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 6, 0, 6), Cursor = Cursors.Hand };
            mainCard.Paint += (s, e) =>
            {
                var r = mainCard.ClientRectangle;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = UiHelper.CrearRoundedRect(r, 16))
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    r, Color.FromArgb(99, 102, 241), Color.FromArgb(139, 92, 246), 135f))
                {
                    e.Graphics.FillPath(brush, path);
                    using (var shine = new System.Drawing.Drawing2D.LinearGradientBrush(
                        new Rectangle(r.X, r.Y, r.Width, r.Height / 2),
                        Color.FromArgb(40, 255, 255, 255), Color.FromArgb(0, 255, 255, 255),
                        System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                        e.Graphics.FillPath(shine, path);
                    using (var circleBrush = new SolidBrush(Color.FromArgb(25, 255, 255, 255)))
                    {
                        e.Graphics.FillEllipse(circleBrush, r.Right - 80, r.Top - 30, 120, 120);
                        e.Graphics.FillEllipse(circleBrush, r.Right - 50, r.Bottom - 60, 90, 90);
                    }
                }
            };

            // Cargar datos reales desde tabla tarjetas
            string numeroInicial = "•••• •••• •••• ••••";
            string titularInicial = SesionActual.Instancia?.Usuario?.NombreCompleto?.ToUpper() ?? "TITULAR";
            string vencInicial = "••/••";
            string tipoInicial = "SIN TARJETA";

            if (SesionActual.Instancia?.Usuario != null)
            {
                try
                {
                    var svcT = new Services.TarjetaService();
                    var tarjetas = svcT.ObtenerTarjetasPorUsuario(SesionActual.Instancia.Usuario.Id);
                    if (tarjetas != null && tarjetas.Count > 0)
                    {
                        _tarjetasActuales = tarjetas;
                        var principal = tarjetas.Find(t => t.EsPrincipal) ?? tarjetas[0];
                        numeroInicial = FormatearNumeroTarjeta(principal.NumeroTarjeta);
                        vencInicial = principal.FechaCaducidad.ToString("MM/yy");
                        tipoInicial = (principal.TipoTarjeta ?? "DÉBITO").ToUpper();
                        titularInicial = principal.NombreTitular;
                    }
                }
                catch { }
                // Cuentas para el botón de abrir cuenta (mantiene funcionalidad existente)
                try
                {
                    var svcC = new Services.CuentaService();
                    _cuentasParaTarjeta = svcC.ObtenerCuentasPorUsuario(SesionActual.Instancia.Usuario.Id)
                                         ?? new List<CuentaBancaria>();
                }
                catch { }
            }

            mainCard.Controls.Add(new Label { Text = "NEXUM BANK", ForeColor = Color.FromArgb(200, 255, 255, 255), Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, Location = new Point(16, 14) });

            _lblTarjetaTipo = new Label { Text = tipoInicial, ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(220, 12) };
            mainCard.Controls.Add(_lblTarjetaTipo);

            // Chip dorado
            var chip = new Panel { Size = new Size(30, 22), Location = new Point(16, 44), BackColor = Color.FromArgb(212, 175, 55) };
            chip.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var p = UiHelper.CrearRoundedRect(chip.ClientRectangle, 4))
                using (var b = new System.Drawing.Drawing2D.LinearGradientBrush(chip.ClientRectangle, Color.FromArgb(212, 175, 55), Color.FromArgb(255, 215, 80), 45f))
                    e.Graphics.FillPath(b, p);
            };
            mainCard.Controls.Add(chip);

            _lblTarjetaNumero = new Label { Text = numeroInicial, ForeColor = Color.White, Font = new Font("Courier New", 13, FontStyle.Bold), AutoSize = true, Location = new Point(16, 82) };
            mainCard.Controls.Add(_lblTarjetaNumero);

            _lblTarjetaTitular = new Label { Text = titularInicial, ForeColor = Color.FromArgb(200, 255, 255, 255), Font = new Font("Segoe UI", 9), AutoSize = true, Location = new Point(16, 120) };
            mainCard.Controls.Add(_lblTarjetaTitular);

            _lblTarjetaVencimiento = new Label { Text = vencInicial, ForeColor = Color.FromArgb(200, 255, 255, 255), Font = new Font("Segoe UI", 9), AutoSize = true, Location = new Point(230, 120) };
            mainCard.Controls.Add(_lblTarjetaVencimiento);

            tlp.Controls.Add(mainCard, 0, 1);

            // ── Fila 2 — Cuenta asociada (NUEVO) ─────────────────────
            var pnlCuentaAsociada = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            _lblTarjetaCuentaAsociada = new Label
            {
                Name = "lblTarjetaCuentaAsociada",
                Text = "🏦 Cargando cuenta...",
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(150, 175, 210),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(8, 6)
            };
            pnlCuentaAsociada.Controls.Add(_lblTarjetaCuentaAsociada);
            pnlCuentaAsociada.Resize += (s, e) =>
            {
                _lblTarjetaCuentaAsociada.Location = new Point(8, (pnlCuentaAsociada.Height - _lblTarjetaCuentaAsociada.Height) / 2);
            };
            tlp.Controls.Add(pnlCuentaAsociada, 0, 2);

            // ── Fila 3 — Acciones rápidas ────────────────────────
            _pnlAccionesTarjeta = CrearAccionesTarjeta();
            tlp.Controls.Add(_pnlAccionesTarjeta, 0, 3);

            // ── Fila 4 — Status bar ───────────────────────────────
            var pnlStatus = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var tarjetaRef = _tarjetasActuales?.Count > 0
                ? ObtenerTarjetaPorCuentaActiva() ?? (_tarjetasActuales.Find(t => t.EsPrincipal) ?? _tarjetasActuales[0])
                : null;

            string estadoTxt = tarjetaRef == null ? "Sin tarjeta"
                : tarjetaRef.Bloqueada ? "🔒 Bloqueada"
                : "● Activa";
            Color estadoClr = tarjetaRef?.Bloqueada == true
                ? Color.FromArgb(239, 68, 68)
                : Color.FromArgb(52, 211, 153);

            _lblTarjetaEstado = new Label
            {
                Text = estadoTxt,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = estadoClr,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(2, 6)
            };

            string limiteTxt = tarjetaRef != null
                ? $"Límite diario {tarjetaRef.LimiteDiario.ToString("C0", AppSettings.CultureMoneda)}  ·  Mensual {tarjetaRef.LimiteMensual.ToString("C0", AppSettings.CultureMoneda)}"
                : "";
            _lblTarjetaLimite = new Label
            {
                Text = limiteTxt,
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(120, 145, 185),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(2, 6)
            };

            pnlStatus.Controls.Add(_lblTarjetaEstado);
            pnlStatus.Controls.Add(_lblTarjetaLimite);
            pnlStatus.Resize += (s, e) =>
            {
                _lblTarjetaEstado.Location = new Point(4, (pnlStatus.Height - _lblTarjetaEstado.Height) / 2);
                _lblTarjetaLimite.Location = new Point(pnlStatus.Width - _lblTarjetaLimite.Width - 4,
                    (pnlStatus.Height - _lblTarjetaLimite.Height) / 2);
            };
            tlp.Controls.Add(pnlStatus, 0, 4);

            // ── Fila 5 — Botón abrir nueva cuenta ────────────────
            var btnAnadir = new Panel { Dock = DockStyle.Fill, Cursor = Cursors.Hand, BackColor = Color.Transparent, Margin = new Padding(0, 2, 0, 0) };
            btnAnadir.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = UiHelper.CrearRoundedRect(btnAnadir.ClientRectangle, 8))
                {
                    e.Graphics.DrawPath(new Pen(Color.FromArgb(70, 147, 171, 255), 1.2f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }, path);
                    e.Graphics.DrawString("+ Abrir nueva cuenta",
                        new Font("Segoe UI", 9),
                        new SolidBrush(Color.FromArgb(110, 147, 171, 255)),
                        btnAnadir.ClientRectangle,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }
            };
            btnAnadir.Click += (s, e) =>
            {
                using (var frm = new Forms.Cuentas.FrmAbrirCuenta())
                {
                    if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK && SesionActual.Instancia?.Usuario != null)
                    {
                        try { _tarjetasActuales = new Services.TarjetaService().ObtenerTarjetasPorUsuario(SesionActual.Instancia.Usuario.Id) ?? new List<Tarjeta>(); } catch { }
                        try { _cuentasParaTarjeta = new Services.CuentaService().ObtenerCuentasPorUsuario(SesionActual.Instancia.Usuario.Id) ?? new List<CuentaBancaria>(); } catch { }
                        ActualizarTarjetaPorCuentaActiva();
                        ActualizarAccionesTarjeta();
                    }
                }
            };
            tlp.Controls.Add(btnAnadir, 0, 5);

            pnl.Controls.Add(tlp);
            section.Controls.Add(pnl);
            return section;
        }


        // ────────────────────────────────────────────────────────
        //  HELPERS TARJETAS — datos reales de cuentas
        // ────────────────────────────────────────────────────────

        private static string FormatearNumeroTarjeta(string numeroCuenta)
        {
            if (string.IsNullOrEmpty(numeroCuenta)) return "•••• •••• •••• ••••";
            var digitos = numeroCuenta.Replace(" ", "").Replace("-", "");
            string ultimos4 = digitos.Length >= 4 ? digitos.Substring(digitos.Length - 4) : digitos;
            return $"••••  ••••  ••••  {ultimos4}";
        }

        private static string GenerarFechaVencimiento(DateTime fechaApertura)
        {
            return fechaApertura.AddYears(5).ToString("MM/yy");
        }

        public void LoadTarjetas(List<Tarjeta> tarjetas)
        {
            _tarjetasActuales = tarjetas ?? new List<Tarjeta>();
            ActualizarTarjetaPorCuentaActiva();
            ActualizarAccionesTarjeta();
        }

        // NUEVO: Obtiene la tarjeta asociada a la cuenta activa actual
        private Tarjeta ObtenerTarjetaPorCuentaActiva()
        {
            if (_tarjetasActuales == null || _tarjetasActuales.Count == 0)
                return null;

            if (_cuentaActivaId > 0)
            {
                var tarjeta = _tarjetasActuales.FirstOrDefault(t => t.CuentaId == _cuentaActivaId);
                if (tarjeta != null)
                    return tarjeta;
            }

            // Si no hay tarjeta para esta cuenta, devolver la principal o la primera
            return _tarjetasActuales.Find(t => t.EsPrincipal) ?? _tarjetasActuales[0];
        }

        // NUEVO: Actualiza la tarjeta mostrada según la cuenta activa
        private void ActualizarTarjetaPorCuentaActiva()
        {
            if (_lblTarjetaNumero == null) return;

            var tarjeta = ObtenerTarjetaPorCuentaActiva();

            if (tarjeta != null)
            {
                _lblTarjetaNumero.Text = FormatearNumeroTarjeta(tarjeta.NumeroTarjeta);
                _lblTarjetaTitular.Text = tarjeta.NombreTitular;
                _lblTarjetaVencimiento.Text = tarjeta.FechaCaducidad.ToString("MM/yy");
                _lblTarjetaTipo.Text = (tarjeta.TipoTarjeta ?? "DÉBITO").ToUpper();

                // Actualizar status bar
                if (_lblTarjetaEstado != null)
                {
                    _lblTarjetaEstado.Text = tarjeta.Bloqueada ? "🔒 Bloqueada" : "● Activa";
                    _lblTarjetaEstado.ForeColor = tarjeta.Bloqueada
                        ? Color.FromArgb(239, 68, 68)
                        : Color.FromArgb(52, 211, 153);
                }

                if (_lblTarjetaLimite != null)
                {
                    var es = AppSettings.CultureMoneda;
                    _lblTarjetaLimite.Text = $"Límite diario {tarjeta.LimiteDiario.ToString("C0", es)}  ·  Mensual {tarjeta.LimiteMensual.ToString("C0", es)}";
                }
            }
            else
            {
                _lblTarjetaNumero.Text = "•••• •••• •••• ••••";
                _lblTarjetaTitular.Text = SesionActual.Instancia?.Usuario?.NombreCompleto?.ToUpper() ?? "TITULAR";
                _lblTarjetaVencimiento.Text = "••/••";
                _lblTarjetaTipo.Text = "SIN TARJETA";

                if (_lblTarjetaEstado != null)
                {
                    _lblTarjetaEstado.Text = "Sin tarjeta";
                    _lblTarjetaEstado.ForeColor = Color.FromArgb(100, 116, 139);
                }

                if (_lblTarjetaLimite != null)
                {
                    _lblTarjetaLimite.Text = "Solicita una tarjeta";
                }
            }

            // Actualizar el label de cuenta asociada
            ActualizarInfoCuentaAsociada();
        }

        // NUEVO: Actualiza el label que muestra la cuenta asociada
        private void ActualizarInfoCuentaAsociada()
        {
            if (_lblTarjetaCuentaAsociada == null) return;

            var cuentaActiva = _cuentasParaTarjeta?.FirstOrDefault(c => c.Id == _cuentaActivaId);
            var tarjetaAsociada = ObtenerTarjetaPorCuentaActiva();

            if (cuentaActiva != null && tarjetaAsociada != null)
            {
                string ibanFormateado = FormatearCorto(cuentaActiva.NumeroCuenta);
                _lblTarjetaCuentaAsociada.Text = $"🏦 Vinculada a cuenta: {ibanFormateado} • {cuentaActiva.TipoCuenta ?? "Cuenta"}";
                _lblTarjetaCuentaAsociada.ForeColor = Color.FromArgb(150, 175, 210);
            }
            else if (cuentaActiva != null)
            {
                string ibanFormateado = FormatearCorto(cuentaActiva.NumeroCuenta);
                _lblTarjetaCuentaAsociada.Text = $"🏦 Cuenta: {ibanFormateado} (sin tarjeta asociada)";
                _lblTarjetaCuentaAsociada.ForeColor = Color.FromArgb(239, 158, 11);
            }
            else if (tarjetaAsociada != null && tarjetaAsociada.CuentaId > 0)
            {
                _lblTarjetaCuentaAsociada.Text = "⚠️ Cuenta no encontrada";
                _lblTarjetaCuentaAsociada.ForeColor = Color.FromArgb(239, 68, 68);
            }
            else
            {
                _lblTarjetaCuentaAsociada.Text = "🏦 Sin cuenta vinculada";
                _lblTarjetaCuentaAsociada.ForeColor = Color.FromArgb(150, 175, 210);
            }
        }

        // ActualizarTarjetaPrincipal se mantiene por compatibilidad pero usa el nuevo método
        private void ActualizarTarjetaPrincipal()
        {
            ActualizarTarjetaPorCuentaActiva();
        }

        private void ActualizarInformacionTarjeta(CuentaBancaria cuenta)
        {
            if (cuenta == null) return;

            // Aquí es donde "se ven" los cambios
            _lblTarjetaNumero.Text = cuenta.NumeroCuenta; // O el número de tarjeta asociado
            _lblTarjetaCuentaAsociada.Text = cuenta.TipoCuenta;
            _saldoActual = cuenta.Saldo;

          

            // Forzar al panel a redibujarse si tiene gráficos personalizados
            panelSaldo.Invalidate();
        }

        // ── Acciones rápidas de tarjeta ───────────────────────────
        private Panel CrearAccionesTarjeta()
        {
            var pnl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 6, 0, 0)
            };
            pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

            var tarjeta = ObtenerTarjetaPorCuentaActiva();

            bool bloq = tarjeta?.Bloqueada == true;

            // [Bloquear / Desbloquear]
            pnl.Controls.Add(CrearBotonAccionTarjeta(
                bloq ? "🔓" : "🔒",
                bloq ? "Desbloquear" : "Bloquear",
                bloq ? Color.FromArgb(16, 185, 129) : Color.FromArgb(239, 68, 68),
                () =>
                {
                    if (tarjeta == null) return;
                    var svc = new Services.TarjetaService();
                    bool ok = tarjeta.Bloqueada ? svc.DesbloquearTarjeta(tarjeta.Id) : svc.BloquearTarjeta(tarjeta.Id);
                    if (ok)
                    {
                        try { _tarjetasActuales = svc.ObtenerTarjetasPorUsuario(SesionActual.Instancia.Usuario.Id); } catch { }
                        ActualizarTarjetaPorCuentaActiva();
                        ActualizarAccionesTarjeta();
                    }
                }), 0, 0);

            // [Ver detalles → VistaTarjetas]
            pnl.Controls.Add(CrearBotonAccionTarjeta(
                "ℹ️", "Detalles",
                Color.FromArgb(147, 171, 255),
                () => TarjetasVerTodoClicked?.Invoke(this, EventArgs.Empty)), 1, 0);

            // [Ajustes → VistaTarjetas]
            pnl.Controls.Add(CrearBotonAccionTarjeta(
                "⚙️", "Ajustes",
                Color.FromArgb(147, 171, 255),
                () => TarjetasVerTodoClicked?.Invoke(this, EventArgs.Empty)), 2, 0);

            return pnl;
        }

        private static Panel CrearBotonAccionTarjeta(string ico, string txt, Color clr, System.Action onClick)
        {
            var btn = new Panel { Dock = DockStyle.Fill, Cursor = Cursors.Hand, BackColor = Color.Transparent, Margin = new Padding(2, 0, 2, 0) };
            bool hov = false;
            Color bgNormal = Color.FromArgb(22, clr.R, clr.G, clr.B);
            Color bgHov = Color.FromArgb(38, clr.R, clr.G, clr.B);

            btn.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var r = btn.ClientRectangle;
                using (var path = UiHelper.CrearRoundedRect(r, 10))
                    g.FillPath(new SolidBrush(hov ? bgHov : bgNormal), path);
                var sfC = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(ico, new Font("Segoe UI Emoji", 11), new SolidBrush(clr), new RectangleF(0, 2, r.Width, r.Height / 2f), sfC);
                g.DrawString(txt, new Font("Segoe UI", 7.5f, FontStyle.Bold), new SolidBrush(clr), new RectangleF(0, r.Height / 2f - 2, r.Width, r.Height / 2f), sfC);
            };
            btn.MouseEnter += (s, e) => { hov = true; btn.Invalidate(); };
            btn.MouseLeave += (s, e) => { hov = false; btn.Invalidate(); };
            btn.Click += (s, e) => onClick();
            return btn;
        }

        private void ActualizarAccionesTarjeta()
        {
            if (_pnlAccionesTarjeta == null) return;
            var parent = _pnlAccionesTarjeta.Parent;
            if (parent == null) return;
            int idx = ((System.Windows.Forms.TableLayoutPanel)parent).GetCellPosition(_pnlAccionesTarjeta).Row;
            parent.Controls.Remove(_pnlAccionesTarjeta);
            _pnlAccionesTarjeta = CrearAccionesTarjeta();
            ((System.Windows.Forms.TableLayoutPanel)parent).Controls.Add(_pnlAccionesTarjeta, 0, idx);

            // Actualizar también el status bar
            var tarjeta = ObtenerTarjetaPorCuentaActiva();
            if (_lblTarjetaEstado != null)
            {
                _lblTarjetaEstado.Text = tarjeta?.Bloqueada == true ? "🔒 Bloqueada" : tarjeta != null ? "● Activa" : "Sin tarjeta";
                _lblTarjetaEstado.ForeColor = tarjeta?.Bloqueada == true ? Color.FromArgb(239, 68, 68) : tarjeta != null ? Color.FromArgb(52, 211, 153) : Color.FromArgb(100, 116, 139);
            }
            if (_lblTarjetaLimite != null && tarjeta != null)
            {
                var es = AppSettings.CultureMoneda;
                _lblTarjetaLimite.Text = $"Límite diario {tarjeta.LimiteDiario.ToString("C0", es)}  ·  Mensual {tarjeta.LimiteMensual.ToString("C0", es)}";
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  HUCHAS DIGITALES — datos demo (se conectará a BD en siguiente sprint)
        // ══════════════════════════════════════════════════════════════
        // Lista de huchas y referencia al panel para reconstruirlo
        private List<Hucha> _huchasActuales = new List<Hucha>();
        private Panel _panelHuchasContainer = null;

        public void LoadHuchas(List<Hucha> huchas)
        {
            _huchasActuales = huchas ?? new List<Hucha>();

            // Reconstruir el panel con los nuevos datos
            if (_panelHuchasContainer?.Parent == null) return;

            var parent = _panelHuchasContainer.Parent;
            int idx = parent.Controls.GetChildIndex(_panelHuchasContainer);
            parent.Controls.Remove(_panelHuchasContainer);
            _panelHuchasContainer = CrearPanelHuchas();
            _panelHuchasContainer.Dock = DockStyle.Top;
            parent.Controls.Add(_panelHuchasContainer);
            parent.Controls.SetChildIndex(_panelHuchasContainer, idx);
        }

        private Panel CrearPanelHuchas()
        {
            const int CARD_H = 96, GAP = 10, PAD = 14;
            const int MAX_DASHBOARD = 2;   // máximo de huchas visibles en el dashboard

            // Ordenar por progreso desc y mostrar solo las primeras MAX_DASHBOARD
            var huchasOrdenadas = new System.Collections.Generic.List<Hucha>(_huchasActuales);
            huchasOrdenadas.Sort((a, b) => b.Progreso.CompareTo(a.Progreso));
            var huchas = huchasOrdenadas.Count > MAX_DASHBOARD
                ? huchasOrdenadas.GetRange(0, MAX_DASHBOARD)
                : huchasOrdenadas;

            int count = Math.Max(1, huchas.Count);
            int totalH = PAD + 36 + GAP + count * (CARD_H + GAP) + PAD;

            var section = CrearSectionContainer(totalH, SectionSpacing);
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = CardFondo };
            pnl.Paint += (s, e) =>
            {
                using (var path = UiHelper.CrearRoundedRect(pnl.ClientRectangle, BorderRadius))
                using (var b = new SolidBrush(CardFondo))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(b, path);
                    e.Graphics.DrawPath(new Pen(BorderCard), path);
                }
            };

            // Posicionamiento manual (evita problemas de DockStyle)
            var inner = new Panel { BackColor = Color.Transparent, AutoSize = true };
            pnl.Controls.Add(inner);
            pnl.Resize += (s, e) =>
            {
                inner.Width = Math.Max(100, pnl.ClientSize.Width - PAD * 2);
                inner.Left = PAD; inner.Top = PAD;
                foreach (Control c in inner.Controls) c.Width = inner.Width;
            };

            int y = 0;

            // ── Cabecera ─────────────────────────────────────────────
            var hdrPanel = new Panel { Location = new Point(0, y), Height = 36, BackColor = Color.Transparent };
            // Título con emoji via Label (renderiza correctamente)
            var lIcoHdr = new Label { Text = "🐷", Font = new Font("Segoe UI Emoji", 14), BackColor = Color.Transparent, AutoSize = true, Location = new Point(0, 6) };
            var lblTit = new Label { Text = "Mis Huchas", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = TextoOscuro, BackColor = Color.Transparent, AutoSize = true, Location = new Point(30, 6) };
            var lblVer = new Label { Text = "Ver todo", Font = new Font("Segoe UI", 10), ForeColor = AzulPrimario, Cursor = Cursors.Hand, AutoSize = true };
            lblVer.Click += (s, e) => HuchasVerTodoClicked?.Invoke(this, EventArgs.Empty);

            var btnAdd = new Panel { Size = new Size(26, 26), BackColor = AzulPrimario, Cursor = Cursors.Hand };
            btnAdd.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = UiHelper.CrearRoundedRect(btnAdd.ClientRectangle, 7))
                    e.Graphics.FillPath(new SolidBrush(AzulPrimario), path);
                TextRenderer.DrawText(e.Graphics, "+", new Font("Segoe UI", 13, FontStyle.Bold),
                    btnAdd.ClientRectangle, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            EventHandler applyBtnR = (s, ev) => { try { btnAdd.Region = new Region(UiHelper.CrearRoundedRect(btnAdd.ClientRectangle, 7)); } catch { } };
            btnAdd.HandleCreated += applyBtnR; btnAdd.Resize += applyBtnR;
            btnAdd.Click += (s, e) =>
            {
                using (var dlg = new NexumApp.Forms.Principal.FrmNuevaHucha())
                {
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        HuchaCreada?.Invoke(this, EventArgs.Empty);
                }
            };

            hdrPanel.Controls.Add(lIcoHdr);
            hdrPanel.Controls.Add(lblTit);
            hdrPanel.Controls.Add(lblVer);
            hdrPanel.Controls.Add(btnAdd);
            hdrPanel.Resize += (s, e) =>
            {
                lblVer.Location = new Point(hdrPanel.Width - btnAdd.Width - lblVer.Width - 10, 8);
                btnAdd.Location = new Point(hdrPanel.Width - btnAdd.Width, 5);
            };
            inner.Controls.Add(hdrPanel);
            y += 36 + GAP;

            // ── Cards desde BD (o empty state si no hay huchas) ───────
            if (huchas.Count == 0)
            {
                var empty = new Label
                {
                    Text = "Aún no tienes huchas. ¡Crea tu primera con el botón +!",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = TextoGris,
                    AutoSize = false,
                    Height = CARD_H,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(0, y),
                    BackColor = Color.Transparent
                };
                inner.Controls.Add(empty);
                y += CARD_H + GAP;
            }
            else
            {
                foreach (var h in huchas)
                {
                    Color clr = ParseColor(h.ColorHex);
                    var card = CrearCardHucha(h, clr);
                    card.Location = new Point(0, y);
                    inner.Controls.Add(card);
                    y += CARD_H + GAP;
                }
            }
            inner.Height = y;

            section.Controls.Add(pnl);
            return section;
        }

        private Panel CrearCardHucha(Hucha h, Color clr)
        {
            string emoji = h.Emoji ?? "🐷";
            string nombre = h.Nombre;
            decimal saldo = h.SaldoActual;
            decimal meta = h.MetaObjetivo;

            const int H = 96;
            const int BTN = 22;   // tamaño de cada botón de acción
            const int GAP = 5;    // separación entre botones

            int pct = meta > 0 ? (int)Math.Min(100, saldo * 100 / meta) : 0;
            var es = AppSettings.CultureMoneda;
            bool hov = false;

            Color bgLight = Color.FromArgb(18, clr.R, clr.G, clr.B);
            Color clrEdit = Color.FromArgb(99, 102, 241);           // indigo
            Color clrDel = Color.FromArgb(220, 38, 38);            // rojo

            var card = new Panel { Height = H, BackColor = CardFondo, Cursor = Cursors.Hand };

            EventHandler applyR = (s, e) =>
            {
                if (card.Width > 4) try { card.Region = new Region(UiHelper.CrearRoundedRect(card.ClientRectangle, 14)); } catch { }
            };
            card.HandleCreated += applyR;
            card.Resize += applyR;

            // ── Calcula las zonas de los 3 botones (derecha, centrados verticalmente) ──
            // Se recalculan en Paint y MouseDown usando el mismo método
            Rectangle ZonaAbonar(int w) => new Rectangle(w - (BTN + GAP) * 3 - 8, (H - BTN) / 2, BTN, BTN);
            Rectangle ZonaEditar(int w) => new Rectangle(w - (BTN + GAP) * 2 - 8, (H - BTN) / 2, BTN, BTN);
            Rectangle ZonaElim(int w) => new Rectangle(w - (BTN + GAP) - 8, (H - BTN) / 2, BTN, BTN);

            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var r = card.ClientRectangle;

                // Fondo
                Color bg = hov ? Color.FromArgb(10, clr.R, clr.G, clr.B) : CardFondo;
                using (var path = UiHelper.CrearRoundedRect(r, 14))
                {
                    g.FillPath(new SolidBrush(bg), path);
                    g.DrawPath(new Pen(hov ? Color.FromArgb(80, clr.R, clr.G, clr.B) : BorderCard, 1.5f), path);
                }

                // Franja izquierda
                using (var path = UiHelper.CrearRoundedRect(new Rectangle(0, 8, 5, r.Height - 16), 3))
                    g.FillPath(new SolidBrush(clr), path);

                // Círculo emoji
                var circR = new Rectangle(14, (H - 50) / 2, 50, 50);
                using (var path = UiHelper.CrearRoundedRect(circR, 25))
                    g.FillPath(new SolidBrush(bgLight), path);

                // Zona de texto: se estrecha cuando hay botones visibles
                int textoW = hov ? r.Width - 74 - (BTN + GAP) * 3 - 16 : r.Width - 165;

                // Nombre
                TextRenderer.DrawText(g, nombre,
                    new Font("Segoe UI", 11f, FontStyle.Bold),
                    new Rectangle(74, 12, textoW, 22), TextoOscuro,
                    TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

                // Importes
                string imp = $"{saldo.ToString("C0", es)}  ·  meta {meta.ToString("C0", es)}";
                TextRenderer.DrawText(g, imp,
                    new Font("Segoe UI", 8.5f),
                    new Rectangle(74, 32, textoW, 18), TextoGris, TextFormatFlags.Left);

                // Barra de progreso
                int bx = 74, by = H - 22, bw = textoW, bh = 7;
                using (var path = UiHelper.CrearRoundedRect(new Rectangle(bx, by, bw, bh), 4))
                    g.FillPath(new SolidBrush(BorderCard), path);
                if (pct > 0)
                {
                    int fw = Math.Max(10, bw * pct / 100);
                    using (var path = UiHelper.CrearRoundedRect(new Rectangle(bx, by, fw, bh), 4))
                    using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                        new Rectangle(bx, by, fw + 1, bh),
                        Color.FromArgb(Math.Min(255, clr.R + 50), Math.Min(255, clr.G + 50), Math.Min(255, clr.B + 50)),
                        clr, System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                        g.FillPath(brush, path);
                }

                if (!hov)
                {
                    // Badge % (solo cuando no hay hover)
                    string pctTxt = pct + "%";
                    var pF = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                    var pSz = TextRenderer.MeasureText(pctTxt, pF);
                    var pR = new Rectangle(r.Width - pSz.Width - 18, 10, pSz.Width + 12, 22);
                    using (var path = UiHelper.CrearRoundedRect(pR, 8))
                        g.FillPath(new SolidBrush(bgLight), path);
                    TextRenderer.DrawText(g, pctTxt, pF, pR, clr,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                else
                {
                    // ── 3 botones de acción visibles al hacer hover ──────────

                    // [+] Abonar
                    var zA = ZonaAbonar(r.Width);
                    using (var path = UiHelper.CrearRoundedRect(zA, 7))
                        g.FillPath(new SolidBrush(clr), path);
                    TextRenderer.DrawText(g, "+", new Font("Segoe UI", 13, FontStyle.Bold),
                        zA, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    // [✏] Editar
                    var zE = ZonaEditar(r.Width);
                    using (var path = UiHelper.CrearRoundedRect(zE, 7))
                        g.FillPath(new SolidBrush(clrEdit), path);
                    TextRenderer.DrawText(g, "✏", new Font("Segoe UI", 9),
                        zE, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    // [🗑] Eliminar
                    var zD = ZonaElim(r.Width);
                    using (var path = UiHelper.CrearRoundedRect(zD, 7))
                        g.FillPath(new SolidBrush(clrDel), path);
                    TextRenderer.DrawText(g, "✕", new Font("Segoe UI", 9, FontStyle.Bold),
                        zD, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            };

            // Emoji label
            var lIco = new Label
            {
                Text = emoji,
                Font = new Font("Segoe UI Emoji", 20),
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(50, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(14, (H - 50) / 2),
                Cursor = Cursors.Hand
            };
            card.Controls.Add(lIco);

            // Hover propagado desde el label del emoji
            card.MouseEnter += (s, e) => { hov = true; card.Invalidate(); };
            card.MouseLeave += (s, e) => { hov = false; card.Invalidate(); };
            lIco.MouseEnter += (s, e) => { hov = true; card.Invalidate(); };
            lIco.MouseLeave += (s, e) => { hov = false; card.Invalidate(); };

            // ── Clic: hit-testing sobre los 3 botones ───────────────────────
            System.Windows.Forms.MouseEventHandler onMouseDown = (s, e) =>
            {
                if (e.Button != System.Windows.Forms.MouseButtons.Left) return;

                // Convertir coordenadas al espacio de la card si viene del label
                Point pt = (s == (object)lIco)
                    ? card.PointToClient(lIco.PointToScreen(e.Location))
                    : e.Location;

                int w = card.Width;
                if (ZonaEditar(w).Contains(pt))
                {
                    EditarHuchaRequested?.Invoke(card, h.Id);
                }
                else if (ZonaElim(w).Contains(pt))
                {
                    EliminarHuchaRequested?.Invoke(card, h.Id);
                }
                else
                {
                    // Clic en cualquier otra zona → Abonar
                    using (var frm = new Forms.Principal.FrmAbonarHucha(h, _saldoActual))
                        if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            AbonarHuchaRequested?.Invoke(card, (h.Id, frm.MontoIngresado));
                }
            };

            card.MouseDown += onMouseDown;
            lIco.MouseDown += onMouseDown;

            return card;
        }

        private static Color ParseColor(string hex)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(hex))
                    return System.Drawing.ColorTranslator.FromHtml(hex);
            }
            catch { }
            return Color.FromArgb(59, 130, 246);
        }

        #endregion

        #region Datos (preparado para BD)

        public void LoadDashboardData(decimal saldo, string textoCuenta, List<Movimiento> movimientos, List<ObjetivoAhorro> objetivos, DatosPresupuesto presupuesto, int puntos = 0)
        {
            LoadSaldo(saldo);
            LoadCuenta(textoCuenta);
            RenderMovimientos(movimientos ?? new List<Movimiento>());
            LoadObjetivos(objetivos ?? new List<ObjetivoAhorro>());
            LoadPresupuesto(presupuesto);
            LoadPuntos(puntos);
        }

        public void LoadSaldo(decimal saldo) { _saldoActual = saldo; ActualizarTextoSaldo(); }
        public void UpdateSaldo(decimal saldo) { _saldoActual = saldo; ActualizarTextoSaldo(); }
        public void LoadCuenta(string textoCuenta) { _textoCuenta = textoCuenta ?? ""; if (lblCuenta != null) lblCuenta.Text = _textoCuenta; }

        public void LoadCuentasActivas(List<CuentaBancaria> cuentas, int cuentaActivaId)
        {
            if (cmbCuentasActivas == null) return;

            _cargandoComboCuentas = true;
            _cuentaActivaId = cuentaActivaId;  // ← Guardar la cuenta activa
            cmbCuentasActivas.DataSource = null;
            cmbCuentasActivas.Items.Clear();

            var lista = cuentas ?? new List<CuentaBancaria>();
            foreach (var c in lista)
            {
                cmbCuentasActivas.Items.Add(new ComboCuentaItem
                {
                    CuentaId = c.Id,
                    Texto = $"{(string.IsNullOrWhiteSpace(c.TipoCuenta) ? "Cuenta" : c.TipoCuenta)} · {FormatearCorto(c.NumeroCuenta)}"
                });
            }

            if (cmbCuentasActivas.Items.Count > 0)
            {
                int index = 0;
                for (int i = 0; i < cmbCuentasActivas.Items.Count; i++)
                {
                    if (cmbCuentasActivas.Items[i] is ComboCuentaItem item && item.CuentaId == cuentaActivaId)
                    {
                        index = i;
                        break;
                    }
                }
                cmbCuentasActivas.SelectedIndex = index;
                cmbCuentasActivas.Enabled = true;
            }
            else
            {
                cmbCuentasActivas.Enabled = false;
            }

            _cargandoComboCuentas = false;

            // Actualizar la tarjeta visual con los datos reales de las cuentas
            _cuentasParaTarjeta = lista;
            ActualizarTarjetaPorCuentaActiva();
            ActualizarAccionesTarjeta();
        }

        private void ToggleSaldo() { _saldoVisible = !_saldoVisible; ActualizarTextoSaldo(); }

        private void ActualizarTextoSaldo()
        {
            if (lblSaldo == null) return;
            lblSaldo.Text = _saldoVisible ? _saldoActual.ToString("C2", AppSettings.CultureMoneda) : "••••••";
        }

        private void CmbCuentasActivas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cargandoComboCuentas) return;
            if (cmbCuentasActivas?.SelectedItem is ComboCuentaItem item)
            {
                _cuentaActivaId = item.CuentaId;  // ← Guardar la cuenta activa
                CuentaActivaChanged?.Invoke(this, item.CuentaId);

                // IMPORTANTE: Actualizar la tarjeta con la nueva cuenta seleccionada
                ActualizarTarjetaPorCuentaActiva();
                ActualizarAccionesTarjeta();
            }
        }

        private static string FormatearCorto(string numeroCuenta)
        {
            if (string.IsNullOrWhiteSpace(numeroCuenta)) return "••••";
            var limpio = numeroCuenta.Replace(" ", "");
            if (limpio.Length <= 4) return limpio;
            return limpio.Substring(Math.Max(0, limpio.Length - 4));
        }

        private class ComboCuentaItem
        {
            public int CuentaId { get; set; }
            public string Texto { get; set; }
            public override string ToString() { return Texto; }
        }

        public void RenderMovimientos(List<Movimiento> movimientos)
        {
            if (flpMovimientosItems == null) return;
            flpMovimientosItems.Controls.Clear();
            if (movimientos == null || movimientos.Count == 0)
            {
                flpMovimientosItems.Controls.Add(new Label { Text = "No hay movimientos", Font = new Font("Segoe UI", 11), ForeColor = TextoGris });
                return;
            }
            foreach (var m in movimientos) flpMovimientosItems.Controls.Add(CrearFilaMovimiento(m));
            AjustarAnchoFilasMovimientos();
        }

        private void AjustarAnchoFilasMovimientos()
        {
            if (flpMovimientosItems == null) return;
            int ancho = Math.Max(260, flpMovimientosItems.ClientSize.Width - 24);
            foreach (Control c in flpMovimientosItems.Controls)
                c.Width = ancho;
        }

        private Panel CrearFilaMovimiento(Movimiento m)
        {
            var es = AppSettings.CultureMoneda;
            bool esIngreso =
                (m.TipoMovimiento != null &&
                    (m.TipoMovimiento.IndexOf("Ingreso", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     m.TipoMovimiento.IndexOf("Recibida", StringComparison.OrdinalIgnoreCase) >= 0))
                ||
                (m.Concepto != null &&
                    (m.Concepto.IndexOf("Ingreso", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     m.Concepto.IndexOf("Recibida", StringComparison.OrdinalIgnoreCase) >= 0));

            // Texto del concepto y subtítulo
            string concepto = string.IsNullOrEmpty(m.Concepto) ? m.TipoMovimiento : m.Concepto;
            string subtitulo = "";
            if (!string.IsNullOrEmpty(m.Concepto) && m.Concepto.IndexOf(" - ", StringComparison.Ordinal) >= 0)
            {
                var partes = m.Concepto.Split(new[] { " - " }, 2, StringSplitOptions.None);
                concepto = partes[0];
                subtitulo = partes.Length > 1 ? partes[1] : "";
            }
            if (string.IsNullOrEmpty(subtitulo))
                subtitulo = ObtenerSubtitulo(m.TipoMovimiento, m.Concepto);

            // Colores según tipo
            Color iconBg, iconFg, clrMonto;
            ObtenerColoresTipo(m.TipoMovimiento, m.Concepto, esIngreso,
                               out iconBg, out iconFg, out clrMonto);

            string icono = ObtenerIconoTipo(m.TipoMovimiento, m.Concepto);
            string montoTexto = (esIngreso ? "+ " : "- ") + m.Monto.ToString("C2", es);
            string fechaTexto = m.Fecha.ToString("dd MMM", es).TrimEnd('.') +
                                " · " + m.Fecha.ToString("HH:mm", es);

            // ── Fila principal ────────────────────────────────────
            const int H = 68;
            var row = new Panel { Height = H, Margin = new Padding(0), BackColor = Color.Transparent };

            // Layout: [icono 58px] [contenido 100%] [derecha 145px]
            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0, 8, 0, 8)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));

            // ── Icono (44×44 redondeado, color según tipo) ───────
            var pnlIconWrap = new Panel { Dock = DockStyle.Fill };
            var pnlIcon = new Panel
            {
                Size = new Size(44, 44),
                BackColor = iconBg,
                Location = new Point(0, 0)
            };
            pnlIcon.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = UiHelper.CrearRoundedRect(pnlIcon.ClientRectangle, 12))
                    e.Graphics.FillPath(new System.Drawing.SolidBrush(iconBg), path);
            };
            var lIco = new Label
            {
                Text = icono,
                Font = new Font("Segoe UI Emoji", 16),
                AutoSize = false,
                Size = new Size(44, 44),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                ForeColor = iconFg,
                Location = new Point(0, 0)
            };
            pnlIcon.Controls.Add(lIco);
            pnlIconWrap.Controls.Add(pnlIcon);

            // ── Concepto + subtítulo ──────────────────────────────
            var contenidoTlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(4, 0, 0, 0)
            };
            contenidoTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            contenidoTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            contenidoTlp.Controls.Add(new Label
            {
                Text = concepto,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = TextoOscuro,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                MaximumSize = new Size(0, 0)
            }, 0, 0);
            contenidoTlp.Controls.Add(new Label
            {
                Text = subtitulo,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = TextoGris,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft
            }, 0, 1);

            // ── Monto + fecha (alineados a la derecha) ────────────
            var derechaTlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            derechaTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            derechaTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            derechaTlp.Controls.Add(new Label
            {
                Text = montoTexto,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = clrMonto,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomRight
            }, 0, 0);
            derechaTlp.Controls.Add(new Label
            {
                Text = fechaTexto,
                Font = new Font("Segoe UI", 8f),
                ForeColor = TextoGris,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopRight
            }, 0, 1);

            tlp.Controls.Add(pnlIconWrap, 0, 0);
            tlp.Controls.Add(contenidoTlp, 1, 0);
            tlp.Controls.Add(derechaTlp, 2, 0);
            row.Controls.Add(tlp);
            row.Controls.Add(new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = DividerColor
            });
            return row;
        }

        private static void ObtenerColoresTipo(string tipo, string concepto, bool esIngreso,
            out Color iconBg, out Color iconFg, out Color clrMonto)
        {
            // Huchas
            if (!string.IsNullOrEmpty(concepto) &&
                concepto.IndexOf("hucha", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                iconBg = Color.FromArgb(237, 233, 254); // violeta claro
                iconFg = Color.FromArgb(109, 40, 217);
                clrMonto = Color.FromArgb(220, 38, 38);
                return;
            }
            // Cashback / Rewards
            if (!string.IsNullOrEmpty(tipo) && tipo.IndexOf("Cashback", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                iconBg = Color.FromArgb(254, 243, 199);
                iconFg = Color.FromArgb(217, 119, 6);
                clrMonto = Color.FromArgb(16, 185, 129);
                return;
            }
            if (esIngreso)
            {
                iconBg = Color.FromArgb(209, 250, 229); // verde claro
                iconFg = Color.FromArgb(16, 185, 129);
                clrMonto = Color.FromArgb(16, 185, 129);
            }
            else if (!string.IsNullOrEmpty(tipo) && tipo.IndexOf("Transferencia", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                iconBg = Color.FromArgb(219, 234, 254); // azul claro
                iconFg = Color.FromArgb(59, 130, 246);
                clrMonto = Color.FromArgb(220, 38, 38); // saliente = rojo
            }
            else if (!string.IsNullOrEmpty(concepto) &&
                     (concepto.IndexOf("Recibida", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      concepto.IndexOf("recibida", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                iconBg = Color.FromArgb(209, 250, 229); // verde claro
                iconFg = Color.FromArgb(16, 185, 129);
                clrMonto = Color.FromArgb(16, 185, 129);
            }
            else
            {
                iconBg = Color.FromArgb(254, 226, 226); // rojo claro
                iconFg = Color.FromArgb(220, 38, 38);
                clrMonto = Color.FromArgb(220, 38, 38);
            }
        }

        // Cambio 5: detecta operaciones de hucha por concepto
        private static string ObtenerIconoTipo(string tipo, string concepto = "")
        {
            if (!string.IsNullOrEmpty(concepto) &&
                concepto.IndexOf("hucha", StringComparison.OrdinalIgnoreCase) >= 0)
                return "🐷";
            if (string.IsNullOrEmpty(tipo)) return "💰";
            if (tipo.IndexOf("Ingreso", StringComparison.OrdinalIgnoreCase) >= 0 ||
                tipo.IndexOf("Recibida", StringComparison.OrdinalIgnoreCase) >= 0) return "📥";
            if (tipo.IndexOf("Transferencia", StringComparison.OrdinalIgnoreCase) >= 0) return "↔️";
            if (tipo.IndexOf("Retiro", StringComparison.OrdinalIgnoreCase) >= 0) return "🏧";
            if (tipo.IndexOf("Pago", StringComparison.OrdinalIgnoreCase) >= 0) return "💳";
            if (tipo.IndexOf("Cashback", StringComparison.OrdinalIgnoreCase) >= 0) return "🎁";
            return "💰";
        }

        // Cambio 4: subtítulo descriptivo según tipo de movimiento
        private static string ObtenerSubtitulo(string tipo, string concepto)
        {
            if (!string.IsNullOrEmpty(concepto) &&
                concepto.IndexOf("hucha", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Ahorro en hucha";
            if (!string.IsNullOrEmpty(concepto) &&
                concepto.IndexOf("recibida", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Transferencia entrante";
            if (string.IsNullOrEmpty(tipo)) return "Operación bancaria";
            if (tipo.IndexOf("Transferencia Enviada", StringComparison.OrdinalIgnoreCase) >= 0) return "Transferencia saliente";
            if (tipo.IndexOf("Transferencia Recibida", StringComparison.OrdinalIgnoreCase) >= 0) return "Transferencia entrante";
            if (tipo.IndexOf("Transferencia", StringComparison.OrdinalIgnoreCase) >= 0) return "Transferencia";
            if (tipo.IndexOf("Ingreso", StringComparison.OrdinalIgnoreCase) >= 0) return "Ingreso en cuenta";
            if (tipo.IndexOf("Retiro", StringComparison.OrdinalIgnoreCase) >= 0) return "Retirada de fondos";
            if (tipo.IndexOf("Pago", StringComparison.OrdinalIgnoreCase) >= 0) return "Pago de servicio";
            if (tipo.IndexOf("Cashback", StringComparison.OrdinalIgnoreCase) >= 0) return "Nexum Rewards";
            return "Operación bancaria";
        }

        public void LoadObjetivos(List<ObjetivoAhorro> objetivos)
        {
            if (pnlObjetivosContenedor == null) return;
            pnlObjetivosContenedor.Controls.Clear();
            if (objetivos == null || objetivos.Count == 0)
            {
                pnlObjetivosContenedor.Controls.Add(new Label
                {
                    Text = "Sin objetivos configurados",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = TextoGris,
                    AutoSize = true,
                    Margin = new Padding(4)
                });
                return;
            }

            var flp = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false
            };

            foreach (var obj in objetivos)
            {
                int progreso = Math.Min(100, Math.Max(0, (int)obj.Progreso));
                int ancho = Math.Max(240, pnlObjetivosContenedor.ClientSize.Width - 18);

                // Colores según progreso
                Color colorBadge = progreso < 50 ? Color.FromArgb(16, 185, 129) : Color.FromArgb(249, 115, 22);
                Color colorBarra1 = progreso < 50 ? Color.FromArgb(99, 241, 180) : Color.FromArgb(251, 191, 36);
                Color colorBarra2 = progreso < 50 ? Color.FromArgb(16, 185, 129) : Color.FromArgb(249, 115, 22);

                var card = new Panel
                {
                    Height = 110,
                    Width = ancho,
                    Margin = new Padding(0, 0, 0, 10),
                    BackColor = CardFondo,
                    Padding = new Padding(12)
                };
                card.Paint += (s, e) =>
                {
                    using (var path = UiHelper.CrearRoundedRect(card.ClientRectangle, 12))
                    using (var cb = new SolidBrush(CardFondo))
                    {
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(cb, path);
                        e.Graphics.DrawPath(new Pen(BorderCard), path);
                    }
                };

                // Icono con gradiente (48x48, radius 12)
                var iconPanel = new Panel
                {
                    Size = new Size(48, 48),
                    Location = new Point(12, 12),
                    BackColor = Color.Transparent
                };
                iconPanel.Paint += (s, e) =>
                {
                    var r = iconPanel.ClientRectangle;
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (var path = UiHelper.CrearRoundedRect(r, 12))
                    using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                        r, colorBarra2, colorBarra1, 45f))
                        e.Graphics.FillPath(brush, path);
                };
                iconPanel.Controls.Add(new Label
                {
                    Text = "🎯",
                    Font = new Font("Segoe UI", 18),
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Location = new Point(6, 6)
                });

                // Nombre
                var lblNombre = new Label
                {
                    Text = obj.Nombre ?? "",
                    Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold),
                    ForeColor = TextoOscuro,
                    AutoSize = true,
                    MaximumSize = new Size(ancho - 130, 0),
                    Location = new Point(70, 10)
                };

                // Badge de porcentaje
                var lblBadge = new Label
                {
                    Text = progreso + "%",
                    Font = new Font("Segoe UI", 8, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = colorBadge,
                    AutoSize = true,
                    Padding = new Padding(6, 2, 6, 2),
                    Location = new Point(ancho - 52, 10)
                };

                // Montos + fecha
                string montoTxt = obj.MontoObjetivo > 0
                    ? obj.MontoActual.ToString("C0", AppSettings.CultureMoneda)
                      + " / " + obj.MontoObjetivo.ToString("C0", AppSettings.CultureMoneda)
                    : progreso + "%";
                string fechaTxt = obj.FechaObjetivo.HasValue
                    ? "Meta: " + obj.FechaObjetivo.Value.ToString("MMM yyyy", AppSettings.CultureMoneda)
                    : "";

                var lblMonto = new Label
                {
                    Text = montoTxt + (string.IsNullOrEmpty(fechaTxt) ? "" : "   " + fechaTxt),
                    Font = new Font("Segoe UI", 8.5F),
                    ForeColor = TextoGris,
                    AutoSize = true,
                    Location = new Point(70, 34)
                };

                // Barra de progreso con gradiente
                int barraY = 74;
                int barraH = 8;
                var pnlBarraFondo = new Panel
                {
                    Location = new Point(12, barraY),
                    Size = new Size(ancho - 24, barraH),
                    BackColor = BorderCard
                };
                pnlBarraFondo.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (var path = UiHelper.CrearRoundedRect(pnlBarraFondo.ClientRectangle, 4))
                        e.Graphics.FillPath(new SolidBrush(BorderCard), path);
                };

                int progresoAncho = Math.Max(0, (int)((ancho - 24) * progreso / 100.0));
                if (progresoAncho > 0)
                {
                    var pnlBarraRelleno = new Panel
                    {
                        Location = new Point(0, 0),
                        Size = new Size(progresoAncho, barraH),
                        BackColor = Color.Transparent
                    };
                    pnlBarraRelleno.Paint += (s, e) =>
                    {
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        using (var path = UiHelper.CrearRoundedRect(pnlBarraRelleno.ClientRectangle, 4))
                        using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                            pnlBarraRelleno.ClientRectangle.IsEmpty
                                ? new Rectangle(0, 0, 1, 1)
                                : pnlBarraRelleno.ClientRectangle,
                            colorBarra1, colorBarra2,
                            System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                            e.Graphics.FillPath(brush, path);
                    };
                    pnlBarraFondo.Controls.Add(pnlBarraRelleno);
                }

                card.Controls.Add(iconPanel);
                card.Controls.Add(lblNombre);
                card.Controls.Add(lblBadge);
                card.Controls.Add(lblMonto);
                card.Controls.Add(pnlBarraFondo);
                flp.Controls.Add(card);
            }
            pnlObjetivosContenedor.Controls.Add(flp);
        }

        public void LoadPresupuesto(DatosPresupuesto datos)
        {
            if (datos == null) return;

            var es = AppSettings.CultureMoneda;
            if (lblPresupuestoRestante != null) lblPresupuestoRestante.Text = datos.Restante.ToString("C0", es);
            if (lblIngresos != null) lblIngresos.Text = datos.Ingresos.ToString("C0", es);
            if (lblGastos != null) lblGastos.Text = datos.Gastos.ToString("C0", es);
            if (lblObjetivo != null) lblObjetivo.Text = datos.Objetivo.ToString("C0", es);

            // Actualizar datos del donut y redibujar
            _donutIngresos = (float)datos.Ingresos;
            _donutGastos = (float)datos.Gastos;
            _donutPanel?.Invalidate();

            // Badge motivacional
            if (lblPresupuestoMensaje != null && !string.IsNullOrEmpty(datos.MensajeEstado))
            {
                lblPresupuestoMensaje.Text = datos.MensajeEstado;
                // Color según situación
                bool positivo = datos.Restante >= 0 && (datos.Ingresos > 0 || datos.Gastos > 0);
                lblPresupuestoMensaje.ForeColor = positivo
                    ? Color.FromArgb(5, 150, 105)
                    : Color.FromArgb(180, 50, 30);
                lblPresupuestoMensaje.BackColor = positivo
                    ? Color.FromArgb(220, 252, 231)
                    : Color.FromArgb(254, 226, 226);
                lblPresupuestoMensaje.Visible = true;
            }
        }

        public void LoadPuntos(int puntos)
        {
            if (lblPuntos != null) lblPuntos.Text = puntos > 0 ? puntos.ToString("N0") + " pts disponibles" : "";
        }

        #endregion

        #region Botones acción rápida

        private static Panel CrearBotonAccionRapida(string texto, string tipo, EventHandler click)
        {
            bool hovered = false;
            bool pressed = false;

            Color gradA = tipo == "enviar" ? Color.FromArgb(99, 102, 241) : Color.FromArgb(16, 185, 129);
            Color gradB = tipo == "enviar" ? Color.FromArgb(139, 92, 246) : Color.FromArgb(5, 150, 105);

            var pnl = new Panel { Cursor = Cursors.Hand, BackColor = Color.Transparent };

            EventHandler aplicarRegion = (s, e) =>
            {
                if (pnl.Width > 4 && pnl.Height > 4)
                    try { pnl.Region = new Region(UiHelper.CrearRoundedRect(pnl.ClientRectangle, 10)); } catch { }
            };
            pnl.HandleCreated += aplicarRegion;
            pnl.Resize += aplicarRegion;

            pnl.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var r = pnl.ClientRectangle;
                if (r.Width < 2 || r.Height < 2) return;

                Color c1 = pressed ? AjustarBrillo(gradA, 0.80f) : hovered ? AjustarBrillo(gradA, 1.15f) : gradA;
                Color c2 = pressed ? AjustarBrillo(gradB, 0.80f) : hovered ? AjustarBrillo(gradB, 1.15f) : gradB;

                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(r, c1, c2, 135f))
                    g.FillRectangle(brush, r);

                using (var shine = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(r.X, r.Y, r.Width, r.Height / 2),
                    Color.FromArgb(50, 255, 255, 255), Color.FromArgb(0, 255, 255, 255),
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                    g.FillRectangle(shine, r.X, r.Y, r.Width, r.Height / 2);

                int iconSz = Math.Min(20, r.Height - 10);
                int iconX = 12;
                int iconY = (r.Height - iconSz) / 2;
                DibujarIconoBotonRapido(g, new Rectangle(r.X + iconX, r.Y + iconY, iconSz, iconSz), tipo);

                var fmt = new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near, FormatFlags = StringFormatFlags.NoWrap };
                var textRect = new RectangleF(r.X + iconX + iconSz + 8, r.Y, r.Width - iconX - iconSz - 14, r.Height);
                using (var f = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                    g.DrawString(texto, f, Brushes.White, textRect, fmt);
            };

            pnl.MouseEnter += (s, e) => { hovered = true; pnl.Invalidate(); };
            pnl.MouseLeave += (s, e) => { hovered = false; pressed = false; pnl.Invalidate(); };
            pnl.MouseDown += (s, e) => { pressed = true; pnl.Invalidate(); };
            pnl.MouseUp += (s, e) => { pressed = false; pnl.Invalidate(); };
            pnl.Click += click;
            return pnl;
        }

        private static Color AjustarBrillo(Color c, float factor)
            => Color.FromArgb(c.A, Math.Min(255, (int)(c.R * factor)), Math.Min(255, (int)(c.G * factor)), Math.Min(255, (int)(c.B * factor)));

        private static void DibujarIconoBotonRapido(System.Drawing.Graphics g, Rectangle r, string tipo)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            float cx = r.X + r.Width / 2f, cy = r.Y + r.Height / 2f;
            float m = r.Width * 0.14f;
            float x = r.X + m, y = r.Y + m, w = r.Width - m * 2, h = r.Height - m * 2;

            using (var pen = new Pen(Color.White, 1.8f) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round, StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round })
            {
                if (tipo == "enviar")
                {
                    g.DrawLines(pen, new[] { new PointF(x, y + h), new PointF(x + w, y), new PointF(x + w * 0.35f, y + h * 0.55f), new PointF(x + w, y), new PointF(x + w * 0.6f, y + h), new PointF(x + w * 0.35f, y + h * 0.55f), new PointF(x, y + h * 0.62f), new PointF(x, y + h) });
                }
                else
                {
                    g.DrawEllipse(pen, r.X + 1, r.Y + 1, r.Width - 2, r.Height - 2);
                    g.DrawLine(pen, cx, cy - h * 0.28f, cx, cy + h * 0.28f);
                    g.DrawLines(pen, new[] { new PointF(cx - w * 0.28f, cy), new PointF(cx, cy + h * 0.28f), new PointF(cx + w * 0.28f, cy) });
                }
            }
        }

        #endregion
    }
}