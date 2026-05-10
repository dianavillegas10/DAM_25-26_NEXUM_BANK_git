using NexumApp.Helpers;
using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace NexumApp.Views
{
    public class VistaTarjetas : UserControl
    {
        private CultureInfo ES => Helpers.AppSettings.CultureMoneda;

        // Paleta dinámica
        private Color BgPage  => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(10,  12,  28)  : Color.FromArgb(246, 248, 252);
        private static readonly Color BgHdr = Color.FromArgb(13,  17,  42);   // header siempre oscuro (gradiente)
        private Color White   => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(18,  22,  46)  : Color.White;
        private Color Borde   => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(38,  44,  80)  : Color.FromArgb(226, 232, 240);
        private Color Oscuro  => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15,  23,  42);
        private Color Gris    => Helpers.AppSettings.ModoOscuro ? Color.FromArgb(100, 116, 139) : Color.FromArgb(100, 116, 139);
        private static readonly Color GrisClr = Color.FromArgb(241, 245, 249);
        private static readonly Color Verde   = Color.FromArgb(16,  185, 129);
        private static readonly Color Rojo    = Color.FromArgb(220,  38,  38);
        private static readonly Color Indigo  = Color.FromArgb(99,  102, 241);
        private static readonly Color Amber   = Color.FromArgb(245, 158,  11);

        private readonly TarjetaService _svc = new TarjetaService();
        private List<Tarjeta> _tarjetas = new List<Tarjeta>();

        private Label  _lblResumen;
        private Panel  _pnlLista;   // contenedor de tarjetas (sin AutoScroll propio)

        public event EventHandler VolverAlInicio;

        public VistaTarjetas()
        {
            BackColor  = BgPage;
            Dock       = DockStyle.Fill;
            AutoScroll = true;       // el scroll lo gestiona el UserControl directamente
            Helpers.AppSettings.ConfiguracionChanged += (s, e) =>
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(new Action(() => { BuildUI(); BeginInvoke((Action)CargarDatos); Helpers.AppSettings.AplicarTraduccionesRecursivo(this); }));
            };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            BuildUI();
            BeginInvoke((Action)(() => BeginInvoke((Action)CargarDatos)));
        }

        // ─────────────────────────────────────────────────────────
        //  UI ESTÁTICA — header fijo + zona de lista
        // ─────────────────────────────────────────────────────────
        private void BuildUI()
        {
            Controls.Clear();
            Padding = new Padding(0);

            // ── Header ────────────────────────────────────────────
            var hdr = new Panel { Dock = DockStyle.Top, Height = 92, BackColor = BgHdr };
            hdr.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = hdr.ClientRectangle;
                using (var b = new LinearGradientBrush(r,
                    Color.FromArgb(25, 32, 80), Color.FromArgb(10, 14, 42), LinearGradientMode.Vertical))
                    g.FillRectangle(b, r);
                using (var br = new SolidBrush(Color.FromArgb(8, 255, 255, 255)))
                { g.FillEllipse(br, r.Width - 150, -55, 220, 220); g.FillEllipse(br, -50, -25, 140, 140); }
                g.DrawLine(new Pen(Color.FromArgb(28, 255, 255, 255)), 0, r.Bottom - 1, r.Width, r.Bottom - 1);
            };

            var lTit = new Label { Text = "💳  Mis Tarjetas",
                Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = White,
                BackColor = Color.Transparent, AutoSize = true, Location = new Point(28, 14) };

            _lblResumen = new Label { Text = "Cargando...",
                Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(150, 185, 225),
                BackColor = Color.Transparent, AutoSize = true, Location = new Point(30, 50) };

            var btnVolver = new Button { Text = "← Inicio",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = White,
                BackColor = Color.FromArgb(55, 255, 255, 255), FlatStyle = FlatStyle.Flat,
                Size = new Size(106, 34), Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnVolver.FlatAppearance.BorderSize = 0;
            btnVolver.FlatAppearance.MouseOverBackColor = Color.FromArgb(85, 255, 255, 255);
            void apRV() { try { btnVolver.Region = new Region(RR(btnVolver.ClientRectangle, 10)); } catch { } }
            btnVolver.HandleCreated += (s, ev) => apRV(); btnVolver.Resize += (s, ev) => apRV();
            btnVolver.Click += (s, ev) => VolverAlInicio?.Invoke(this, EventArgs.Empty);
            hdr.Controls.AddRange(new Control[] { lTit, _lblResumen, btnVolver });
            hdr.Resize += (s, ev) =>
                btnVolver.Location = new Point(hdr.Width - btnVolver.Width - 22, (hdr.Height - btnVolver.Height) / 2);

            // ── Panel de lista (sin scroll propio) ───────────────
            _pnlLista = new Panel { BackColor = BgPage, Dock = DockStyle.Top, Height = 0 };

            // Padding inferior
            var footer = new Panel { BackColor = BgPage, Dock = DockStyle.Top, Height = 24 };

            // ORDEN CORRECTO: en WinForms con DockStyle.Top el último añadido
            // tiene mayor z-order y se coloca primero (arriba). Por eso header va ÚLTIMO.
            Controls.Add(footer);
            Controls.Add(_pnlLista);
            Controls.Add(hdr);

            // Ajuste de anchos al resize del UserControl
            Resize += (s, e) => AjustarAnchos();
        }

        // ─────────────────────────────────────────────────────────
        //  DATOS
        // ─────────────────────────────────────────────────────────
        private void CargarDatos()
        {
            if (!SesionActual.Instancia?.EstaLogeado ?? true) return;
            try   { _tarjetas = _svc.ObtenerTarjetasPorUsuario(SesionActual.Instancia.Usuario.Id); }
            catch { _tarjetas = new List<Tarjeta>(); }

            _tarjetas.Sort((a, b) =>
            {
                if (a.EsPrincipal != b.EsPrincipal) return b.EsPrincipal.CompareTo(a.EsPrincipal);
                bool aOk = a.Activa && !a.Bloqueada, bOk = b.Activa && !b.Bloqueada;
                return bOk.CompareTo(aOk);
            });

            ActualizarResumen();
            RenderLista();
        }

        private void ActualizarResumen()
        {
            int total = _tarjetas.Count, act = 0, bloq = 0;
            foreach (var t in _tarjetas)
            { if (t.Bloqueada) bloq++; else if (t.Activa) act++; }
            _lblResumen.Text = total == 0 ? "No tienes tarjetas registradas"
                : $"{total} tarjeta{(total != 1 ? "s" : "")}  ·  {act} activa{(act != 1 ? "s" : "")}  ·  {bloq} bloqueada{(bloq != 1 ? "s" : "")}";
        }

        // ─────────────────────────────────────────────────────────
        //  RENDER LISTA — Dock.Top encadenado, scroll en el UserControl
        // ─────────────────────────────────────────────────────────
        private const int PX = 28, PY = 16, GY = 10;

        private void RenderLista()
        {
            _pnlLista.Controls.Clear();
            int cw = Math.Max(360, ClientSize.Width - PX * 2);
            int y  = PY;

            if (_tarjetas.Count == 0)
            {
                _pnlLista.Controls.Add(new Label
                {
                    Text = "No tienes tarjetas.\nAbre una cuenta para obtener tu tarjeta Nexum.",
                    Font = new Font("Segoe UI", 11), ForeColor = Gris,
                    AutoSize = false, Size = new Size(cw, 70),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(PX, PY + 20), BackColor = Color.Transparent
                });
                _pnlLista.Height = 130;
                return;
            }

            // Etiqueta sección
            _pnlLista.Controls.Add(SecLabel("Tarjetas activas", new Point(PX, y)));
            y += 34;

            foreach (var t in _tarjetas)
            {
                var card = CrearCard(t, cw);
                card.Location = new Point(PX, y);
                _pnlLista.Controls.Add(card);
                y += card.Height + GY;
            }

            // Separador + botón
            y += 6;
            _pnlLista.Controls.Add(new Panel { Location = new Point(PX, y), Size = new Size(cw, 1), BackColor = Borde });
            y += 14;

            var btnNew = CrearBotonNueva(cw);
            btnNew.Location = new Point(PX, y);
            _pnlLista.Controls.Add(btnNew);
            y += btnNew.Height + PY;

            _pnlLista.Height = y;

            // Scroll al inicio
            AutoScrollPosition = new Point(0, 0);
        }

        // ─────────────────────────────────────────────────────────
        //  CARD TARJETA MODERNA
        // ─────────────────────────────────────────────────────────
        private Panel CrearCard(Tarjeta t, int w)
        {
            const int H  = 130;
            const int TW = 120, TH = 76;  // mini tarjeta

            bool bloq = t.Bloqueada;
            bool activ = t.Activa && !bloq;

            var card = new Panel { Size = new Size(w, H), BackColor = White };
            void apCard() { try { card.Region = new Region(RR(card.ClientRectangle, 14)); } catch { } }
            card.HandleCreated += (s, e) => apCard(); card.Resize += (s, e) => apCard();

            card.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = card.ClientRectangle;
                using (var path = RR(r, 14))
                { g.FillPath(Brushes.White, path); g.DrawPath(new Pen(Borde, 1f), path); }

                // Franja superior de color
                Color stripClr = bloq ? Rojo : (t.EsPrincipal ? Indigo : Verde);
                g.FillRectangle(new SolidBrush(stripClr), new Rectangle(0, 0, r.Width, 3));

                // Badge PRINCIPAL (esquina superior derecha)
                if (t.EsPrincipal && !bloq)
                {
                    string badge = "PRINCIPAL";
                    var bF = new Font("Segoe UI", 7.5f, FontStyle.Bold);
                    var bSz = TextRenderer.MeasureText(badge, bF);
                    var bR  = new Rectangle(r.Width - bSz.Width - 20, 10, bSz.Width + 14, 20);
                    using (var path = RR(bR, 6))
                        g.FillPath(new SolidBrush(Color.FromArgb(238, 242, 255)), path);
                    TextRenderer.DrawText(g, badge, bF, bR, Indigo,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            };

            // ── Mini tarjeta (izquierda) ──────────────────────────
            var mini = new Panel
            {
                Size = new Size(TW, TH),
                Location = new Point(14, (H - TH) / 2),
                BackColor = Color.Transparent
            };
            mini.Paint += (s, e) => PintarMini(e.Graphics, mini.ClientRectangle, t);
            card.Controls.Add(mini);

            // ── Info central ──────────────────────────────────────
            int ix  = 14 + TW + 16;
            int btnW = 110;
            int iw  = w - ix - btnW - 24;

            string num = t.NumeroTarjeta ?? "";
            string numD = num.Length >= 4 ? $"•••• •••• •••• {num.Substring(num.Length - 4)}" : num;

            var lblNum = new Label { Text = numD, Location = new Point(ix, 12),
                Font = new Font("Courier New", 11, FontStyle.Bold), ForeColor = Oscuro,
                AutoSize = true, BackColor = Color.Transparent };

            var lblTipo = new Label
            {
                Text = $"{t.TipoTarjeta ?? "Débito"}  ·  {t.Marca ?? "Visa"}  ·  Cad. {t.FechaCaducidad:MM/yyyy}",
                Location = new Point(ix, 38), Font = new Font("Segoe UI", 8.5f),
                ForeColor = Gris, AutoSize = true, BackColor = Color.Transparent
            };

            // Límites con mini barra
            string limTxt = $"Límite diario {t.LimiteDiario.ToString("C0", ES)}  ·  Mensual {t.LimiteMensual.ToString("C0", ES)}";
            var lblLim = new Label { Text = limTxt, Location = new Point(ix, 62),
                Font = new Font("Segoe UI", 8f), ForeColor = Gris,
                AutoSize = true, BackColor = Color.Transparent };

            // Estado
            Color estClr = bloq ? Rojo : (activ ? Verde : Amber);
            string estTxt = bloq ? "● Bloqueada" : (activ ? "● Activa" : "● Inactiva");
            var lblEst = new Label { Text = estTxt, Location = new Point(ix, 86),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = estClr,
                AutoSize = true, BackColor = Color.Transparent };

            card.Controls.AddRange(new Control[] { lblNum, lblTipo, lblLim, lblEst });

            // ── Botones (extremo derecho) ─────────────────────────
            var pnlBtns = new Panel
            {
                Size = new Size(btnW, H - 16),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            // Botón bloquear/desbloquear
            var btnBlq = new Button
            {
                Text = bloq ? "Desbloquear" : "Bloquear",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = White,
                BackColor = bloq ? Verde : Rojo,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(btnW, 32), Location = new Point(0, 16),
                Cursor = Cursors.Hand
            };
            btnBlq.FlatAppearance.BorderSize = 0;
            btnBlq.FlatAppearance.MouseOverBackColor = bloq
                ? ControlPaint.Dark(Verde, 0.1f) : ControlPaint.Dark(Rojo, 0.1f);
            void apBlq() { try { btnBlq.Region = new Region(RR(btnBlq.ClientRectangle, 8)); } catch { } }
            btnBlq.HandleCreated += (s, e) => apBlq(); btnBlq.Resize += (s, e) => apBlq();
            btnBlq.Click += (s, e) =>
            {
                bool ok = bloq ? _svc.DesbloquearTarjeta(t.Id) : _svc.BloquearTarjeta(t.Id);
                if (ok) CargarDatos();
                else MessageBox.Show("No se pudo actualizar el estado.", "Nexum Bank",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };

            // Botón detalles
            var btnDet = new Button
            {
                Text = "Ver detalles",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Indigo, BackColor = Color.FromArgb(238, 242, 255),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(btnW, 32), Location = new Point(0, 56),
                Cursor = Cursors.Hand
            };
            btnDet.FlatAppearance.BorderSize = 0;
            btnDet.FlatAppearance.MouseOverBackColor = Color.FromArgb(224, 231, 255);
            void apDet() { try { btnDet.Region = new Region(RR(btnDet.ClientRectangle, 8)); } catch { } }
            btnDet.HandleCreated += (s, e) => apDet(); btnDet.Resize += (s, e) => apDet();
            btnDet.Click += (s, e) => MostrarDetalles(t);

            pnlBtns.Controls.AddRange(new Control[] { btnBlq, btnDet });
            card.Controls.Add(pnlBtns);

            Action posBtns = () =>
                pnlBtns.Location = new Point(card.Width - pnlBtns.Width - 14, (H - pnlBtns.Height) / 2);
            posBtns();
            card.Resize += (s, e) => posBtns();

            // Hover sutil
            card.MouseEnter += (s, e) => { card.BackColor = Color.FromArgb(250, 251, 255); card.Invalidate(); };
            card.MouseLeave += (s, e) => { card.BackColor = White; card.Invalidate(); };

            return card;
        }

        // ─────────────────────────────────────────────────────────
        //  MINI TARJETA
        // ─────────────────────────────────────────────────────────
        private static void PintarMini(Graphics g, Rectangle r, Tarjeta t)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color c1, c2;
            if (t.Bloqueada)        { c1 = Color.FromArgb(75, 75, 100); c2 = Color.FromArgb(55, 55, 80); }
            else if (t.EsPrincipal) { c1 = Color.FromArgb(79, 70, 229); c2 = Color.FromArgb(139, 92, 246); }
            else                    { c1 = Color.FromArgb(14, 165, 233); c2 = Color.FromArgb(37, 99, 235); }

            using (var path = RR(r, 10))
            using (var br   = new LinearGradientBrush(r, c1, c2, 135f))
                g.FillPath(br, path);

            using (var sh = new LinearGradientBrush(new Rectangle(r.X, r.Y, r.Width, r.Height / 2),
                Color.FromArgb(28, 255, 255, 255), Color.Transparent, LinearGradientMode.Vertical))
            using (var path = RR(r, 10)) g.FillPath(sh, path);

            using (var cb = new SolidBrush(Color.FromArgb(14, 255, 255, 255)))
                g.FillEllipse(cb, r.Right - 50, -12, 72, 72);

            // Chip
            using (var path = RR(new Rectangle(8, 9, 22, 15), 3))
            using (var bc = new LinearGradientBrush(new Rectangle(8, 9, 22, 15),
                Color.FromArgb(212, 175, 55), Color.FromArgb(255, 220, 80), 45f))
                g.FillPath(bc, path);

            var sf = new StringFormat { FormatFlags = StringFormatFlags.NoWrap };
            string num = t.NumeroTarjeta ?? "";
            string ult = num.Length >= 4 ? num.Substring(num.Length - 4) : "••••";
            g.DrawString($"•••• {ult}", new Font("Courier New", 8, FontStyle.Bold),
                Brushes.White, new PointF(6, 30), sf);
            g.DrawString(t.FechaCaducidad.ToString("MM/yy"),
                new Font("Segoe UI", 7.5f, FontStyle.Bold),
                new SolidBrush(Color.FromArgb(210, 255, 255, 255)), new PointF(6, 52), sf);

            // Marca
            string m   = t.Marca ?? "Visa";
            var szM = g.MeasureString(m, new Font("Arial", 8, FontStyle.Bold | FontStyle.Italic));
            g.DrawString(m, new Font("Arial", 8, FontStyle.Bold | FontStyle.Italic),
                new SolidBrush(Color.FromArgb(200, 255, 255, 255)),
                new PointF(r.Width - szM.Width - 5, r.Height - szM.Height - 3), sf);

            if (t.Bloqueada)
            {
                using (var ov = new SolidBrush(Color.FromArgb(110, 0, 0, 0)))
                using (var path = RR(r, 10)) g.FillPath(ov, path);
                var sfC = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("🔒", new Font("Segoe UI Emoji", 13), Brushes.White,
                    new RectangleF(0, 0, r.Width, r.Height), sfC);
            }
        }

        // ─────────────────────────────────────────────────────────
        //  BOTÓN NUEVA TARJETA
        // ─────────────────────────────────────────────────────────
        private static Panel CrearBotonNueva(int w)
        {
            var pnl = new Panel { Size = new Size(w, 46), BackColor = Color.Transparent, Cursor = Cursors.Hand };
            pnl.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = pnl.ClientRectangle;
                using (var path = RR(r, 10))
                using (var pen  = new Pen(Color.FromArgb(60, 99, 102, 241), 1.5f) { DashStyle = DashStyle.Dash })
                    g.DrawPath(pen, path);
                var sfC = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("+ Solicitar nueva tarjeta", new Font("Segoe UI", 10, FontStyle.Bold),
                    new SolidBrush(Color.FromArgb(130, 99, 102, 241)),
                    new RectangleF(0, 0, r.Width, r.Height), sfC);
            };
            pnl.Click += (s, e) =>
                MessageBox.Show(
                    "Para obtener una nueva tarjeta, abre una cuenta desde el apartado Cuentas.\n" +
                    "Cada cuenta incluye automáticamente una tarjeta Visa Débito Nexum.",
                    "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return pnl;
        }

        // ─────────────────────────────────────────────────────────
        //  DETALLES
        // ─────────────────────────────────────────────────────────
        private void MostrarDetalles(Tarjeta t)
        {
            string num = t.NumeroTarjeta ?? "";
            string nF  = num.Length == 16
                ? $"{num.Substring(0,4)} {num.Substring(4,4)} {num.Substring(8,4)} {num.Substring(12)}"
                : num;
            MessageBox.Show(
                $"Número:          {nF}\n" +
                $"Titular:         {t.NombreTitular}\n" +
                $"Tipo:            {t.TipoTarjeta}  ·  {t.Marca}\n" +
                $"Emisión:         {t.FechaEmision:dd/MM/yyyy}\n" +
                $"Caducidad:       {t.FechaCaducidad:MM/yyyy}\n\n" +
                $"Límite diario:   {t.LimiteDiario.ToString("C2", ES)}\n" +
                $"Límite mensual:  {t.LimiteMensual.ToString("C2", ES)}\n\n" +
                $"Estado:  {(t.Bloqueada ? "Bloqueada 🔒" : t.Activa ? "Activa ✅" : "Inactiva")}",
                "Nexum Bank — Detalle de tarjeta", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        // ─────────────────────────────────────────────────────────
        //  HELPERS
        // ─────────────────────────────────────────────────────────
        private void AjustarAnchos()
        {
            int cw = Math.Max(360, ClientSize.Width - PX * 2);
            foreach (Control c in _pnlLista.Controls)
                if (!(c is Label)) c.Width = cw;
        }

        private static Label SecLabel(string txt, Point loc) =>
            new Label { Text = txt, Location = loc, AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59) };

        private static GraphicsPath RR(Rectangle r, int rad)
        {
            var p = new GraphicsPath(); int d = rad * 2;
            if (r.Width < d || r.Height < d) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure(); return p;
        }
    }
}
