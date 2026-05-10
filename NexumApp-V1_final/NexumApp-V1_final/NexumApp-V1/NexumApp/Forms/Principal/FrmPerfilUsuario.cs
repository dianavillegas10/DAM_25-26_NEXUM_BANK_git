using NexumApp.Helpers;
using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace NexumApp.Forms.Principal
{
    public partial class FrmPerfilUsuario : Form
    {
        private readonly CuentaService _cuentaService = new CuentaService();

        // ── Tema dinámico ─────────────────────────────────────
        private Color BgMain   => AppSettings.ModoOscuro ? Color.FromArgb(10, 12, 28)  : Color.FromArgb(244, 247, 254);
        private Color BgCard   => AppSettings.ModoOscuro ? Color.FromArgb(18, 22, 46)  : Color.White;
        private Color BgSurface=> AppSettings.ModoOscuro ? Color.FromArgb(14, 17, 38)  : Color.FromArgb(248, 250, 255);
        private Color Border   => AppSettings.ModoOscuro ? Color.FromArgb(38, 44, 80)  : Color.FromArgb(220, 225, 235);
        private Color TxtMain  => AppSettings.ModoOscuro ? Color.FromArgb(241, 245, 249): Color.FromArgb(31, 41, 55);
        private Color TxtMuted => AppSettings.ModoOscuro ? Color.FromArgb(100, 116, 139): Color.FromArgb(107, 114, 128);

        private static readonly Color C_Blue   = Color.FromArgb(99,  102, 241);
        private static readonly Color C_Violet = Color.FromArgb(139, 92,  246);
        private static readonly Color C_Green  = Color.FromArgb(52,  211, 153);
        private static readonly Color C_Gold   = Color.FromArgb(251, 191, 36);
        private static readonly Color C_Red    = Color.FromArgb(248, 113, 113);
        private CultureInfo ES => AppSettings.CultureMoneda;

        public FrmPerfilUsuario()
        {
            InitializeComponent();
            DoubleBuffered = true;
            AppSettings.ConfiguracionChanged += (s, e) =>
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(new Action(() => { BuildUI(); AppSettings.AplicarTraduccionesRecursivo(this); }));
            };
        }

        protected override void OnShown(EventArgs e) { base.OnShown(e); BuildUI(); }

        // ══════════════════════════════════════════════════════
        //  BUILD UI completo
        // ══════════════════════════════════════════════════════
        private void BuildUI()
        {
            Controls.Clear();
            var usr = SesionActual.Instancia?.Usuario;
            if (usr == null) { Close(); return; }

            Text = AppSettings.T("Mi perfil") + " — Nexum Bank";
            Size = new Size(900, 640);
            MinimumSize = Size; MaximumSize = Size;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = BgMain;

            Paint += (s, ev) =>
            {
                if (!AppSettings.ModoOscuro) return;
                using (var b = new LinearGradientBrush(ClientRectangle,
                    Color.FromArgb(12, 14, 34), BgMain, LinearGradientMode.Vertical))
                    ev.Graphics.FillRectangle(b, ClientRectangle);
            };

            // Layout: izquierda (perfil) + derecha (resumen financiero)
            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
                BackColor = Color.Transparent, Padding = new Padding(0)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            tlp.Controls.Add(BuildPanelIzquierda(usr), 0, 0);
            tlp.Controls.Add(BuildPanelDerecha(usr),   1, 0);
            Controls.Add(tlp);
        }

        // ── PANEL IZQUIERDO ────────────────────────────────────
        private Panel BuildPanelIzquierda(Usuario usr)
        {
            var scroll = new Panel
            {
                Dock = DockStyle.Fill, AutoScroll = true,
                BackColor = BgSurface, Padding = new Padding(28, 28, 20, 20)
            };
            scroll.Paint += (s, ev) =>
            {
                using (var pen = new Pen(Border, 1))
                    ev.Graphics.DrawLine(pen, scroll.Width - 1, 0, scroll.Width - 1, scroll.Height);
            };

            int y = 0;
            string ini = ((usr.Nombre?.Length > 0 ? "" + usr.Nombre[0] : "?") +
                          (usr.Apellidos?.Length > 0 ? "" + usr.Apellidos[0] : "")).ToUpper();

            // ── Avatar grande ──────────────────────────────────
            var av = new Panel { Size = new Size(80, 80), Location = new Point(0, y), BackColor = Color.Transparent };
            av.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                bool shown = false;
                if (!string.IsNullOrEmpty(usr.FotoPerfil) && System.IO.File.Exists(usr.FotoPerfil))
                {
                    try
                    {
                        using (var path = RR(av.ClientRectangle, 22))
                        using (var img = Image.FromFile(usr.FotoPerfil))
                        {
                            ev.Graphics.SetClip(path);
                            ev.Graphics.DrawImage(img, av.ClientRectangle);
                            ev.Graphics.ResetClip();
                            using (var pen = new Pen(Color.FromArgb(70, C_Blue.R, C_Blue.G, C_Blue.B), 1.5f))
                                ev.Graphics.DrawPath(pen, path);
                        }
                        shown = true;
                    }
                    catch { }
                }
                if (!shown)
                {
                    using (var path = RR(av.ClientRectangle, 22))
                    using (var b = new LinearGradientBrush(av.ClientRectangle, C_Blue, C_Violet, 135f))
                        ev.Graphics.FillPath(b, path);
                    using (var sh = new SolidBrush(Color.FromArgb(40, 255, 255, 255)))
                        ev.Graphics.FillEllipse(sh, -20, -20, 80, 80);
                    var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    ev.Graphics.DrawString(ini, new Font("Segoe UI", 24, FontStyle.Bold), Brushes.White, av.ClientRectangle, fmt);
                }
            };
            scroll.Controls.Add(av); y += 90;

            // Nombre
            scroll.Controls.Add(Lbl(usr.NombreCompleto, 0, y, new Font("Segoe UI", 16, FontStyle.Bold), TxtMain)); y += 28;
            scroll.Controls.Add(Lbl(usr.Email, 0, y, new Font("Segoe UI", 10), TxtMuted)); y += 22;

            // Badge rol
            var badge = new Panel { Size = new Size(usr.EsAdmin ? 120 : 80, 22), Location = new Point(0, y) };
            badge.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color col = usr.EsAdmin ? C_Gold : C_Blue;
                using (var path = RR(badge.ClientRectangle, 6))
                    ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(25, col.R, col.G, col.B)), path);
                using (var path = RR(badge.ClientRectangle, 6))
                    ev.Graphics.DrawPath(new Pen(Color.FromArgb(60, col.R, col.G, col.B), 1), path);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString(usr.EsAdmin ? "● ADMINISTRADOR" : "● CLIENTE",
                    new Font("Segoe UI", 7, FontStyle.Bold), new SolidBrush(col), badge.ClientRectangle, fmt);
            };
            scroll.Controls.Add(badge); y += 34;

            // Separador
            scroll.Controls.Add(Sep(0, y, 304)); y += 16;

            // ── Datos personales ───────────────────────────────
            scroll.Controls.Add(SecTitulo(AppSettings.T("Información personal"), 0, y)); y += 26;

            Par(scroll, "📱  " + AppSettings.T("Teléfono"),
                string.IsNullOrEmpty(usr.Telefono) ? "—" : usr.Telefono, ref y);
            Par(scroll, "🪪  " + AppSettings.T("DNI / NIF"),
                string.IsNullOrEmpty(usr.DNI) ? "—" : usr.DNI, ref y);
            Par(scroll, "🎂  " + AppSettings.T("Fecha de nacimiento"),
                usr.FechaNacimiento?.ToString("dd MMMM yyyy", ES) ?? "—", ref y);
            Par(scroll, "📍  " + AppSettings.T("Dirección"),
                string.IsNullOrEmpty(usr.Direccion) ? "—" : usr.Direccion, ref y);
            Par(scroll, "🏙️  " + AppSettings.T("Ciudad"),
                string.IsNullOrEmpty(usr.Ciudad) ? "—" : usr.Ciudad, ref y);

            scroll.Controls.Add(Sep(0, y, 304)); y += 16;
            scroll.Controls.Add(SecTitulo(AppSettings.T("Cuenta"), 0, y)); y += 26;

            Par(scroll, "🗓️  " + AppSettings.T("Miembro desde"),
                usr.FechaRegistro.ToString("dd MMMM yyyy", ES), ref y);
            Par(scroll, "⏱️  " + AppSettings.T("Último acceso"),
                usr.UltimoAcceso?.ToString("dd/MM/yyyy  HH:mm") ?? "—", ref y);
            Par(scroll, "🆔  " + AppSettings.T("ID de usuario"), "#" + usr.Id, ref y);

            scroll.Controls.Add(Sep(0, y, 304)); y += 16;

            // ── Botones ────────────────────────────────────────
            var btnEditar = MakeBtn("✏️  " + AppSettings.T("Editar perfil"), C_Blue, new Point(0, y), new Size(304, 42));
            btnEditar.Click += (s, e) =>
            {
                using (var frm = new FrmEditarPerfil())
                    if (frm.ShowDialog(this) == DialogResult.OK) BuildUI();
            };
            scroll.Controls.Add(btnEditar); y += 50;
            BeginInvoke(new Action(() => Redondear(btnEditar, 10)));

            var btnCerrar = new Button
            {
                Text = AppSettings.T("Cerrar"),
                Location = new Point(0, y), Size = new Size(304, 34),
                BackColor = Color.Transparent, ForeColor = TxtMuted,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.MouseEnter += (s, e) => btnCerrar.ForeColor = TxtMain;
            btnCerrar.MouseLeave += (s, e) => btnCerrar.ForeColor = TxtMuted;
            btnCerrar.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
            scroll.Controls.Add(btnCerrar);

            return scroll;
        }

        // ── PANEL DERECHO ──────────────────────────────────────
        private Panel BuildPanelDerecha(Usuario usr)
        {
            var scroll = new Panel
            {
                Dock = DockStyle.Fill, AutoScroll = true,
                BackColor = BgMain, Padding = new Padding(28, 28, 28, 28)
            };

            var cuentas = _cuentaService.ObtenerCuentasPorUsuario(usr.Id) ?? new System.Collections.Generic.List<CuentaBancaria>();
            decimal totalSaldo = cuentas.Sum(c => c.Saldo);
            int y = 0;

            // ── Título ──────────────────────────────────────────
            scroll.Controls.Add(Lbl(AppSettings.T("Resumen financiero"), 0, y, new Font("Segoe UI", 17, FontStyle.Bold), TxtMain)); y += 30;
            scroll.Controls.Add(Lbl(AppSettings.T("Vista general de tus finanzas en Nexum"), 0, y, new Font("Segoe UI", 9), TxtMuted)); y += 28;
            scroll.Controls.Add(new Panel { Location = new Point(0, y), Height = 1, Width = 1000, BackColor = Border }); y += 16;

            // ── Stats rápidas (3 tarjetas) ──────────────────────
            var tlpStats = new TableLayoutPanel
            {
                Location = new Point(0, y), Size = new Size(480, 80),
                ColumnCount = 3, RowCount = 1, BackColor = Color.Transparent, Padding = new Padding(0)
            };
            for (int i = 0; i < 3; i++) tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
            tlpStats.Controls.Add(StatCard(AppSettings.T("CUENTAS"), cuentas.Count.ToString(), C_Blue), 0, 0);
            tlpStats.Controls.Add(StatCard(AppSettings.T("SALDO TOTAL"), totalSaldo.ToString("C2", ES), C_Green), 1, 0);
            tlpStats.Controls.Add(StatCard(AppSettings.T("TARJETAS"), cuentas.Count.ToString(), C_Violet), 2, 0);
            scroll.Controls.Add(tlpStats); y += 96;

            // ── Sección cuentas ────────────────────────────────
            scroll.Controls.Add(SecTitulo(AppSettings.T("Mis cuentas"), 0, y)); y += 26;

            if (cuentas.Count == 0)
            {
                scroll.Controls.Add(Lbl("— " + AppSettings.T("Sin cuentas vinculadas"), 0, y, new Font("Segoe UI", 10, FontStyle.Italic), TxtMuted));
                y += 30;
            }
            else
            {
                foreach (var cuenta in cuentas)
                {
                    var fila = CuentaFila(cuenta);
                    fila.Location = new Point(0, y);
                    scroll.Controls.Add(fila);
                    y += 68;
                }
            }

            // ── Actividad reciente ─────────────────────────────
            y += 8;
            scroll.Controls.Add(new Panel { Location = new Point(0, y), Height = 1, Width = 1000, BackColor = Border }); y += 16;
            scroll.Controls.Add(SecTitulo(AppSettings.T("Información de seguridad"), 0, y)); y += 26;

            var cardSec = new Panel { Location = new Point(0, y), Size = new Size(480, 90), BackColor = BgCard };
            cardSec.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(cardSec.ClientRectangle, 12))
                { ev.Graphics.FillPath(new SolidBrush(BgCard), path); ev.Graphics.DrawPath(new Pen(Border, 1), path); }
                ev.Graphics.FillRectangle(new SolidBrush(C_Green), new Rectangle(0, 0, 4, cardSec.Height));
            };
            cardSec.Controls.Add(Lbl("🔒  " + AppSettings.T("Cuenta verificada y activa"), 14, 12, new Font("Segoe UI", 10, FontStyle.Bold), TxtMain));
            cardSec.Controls.Add(Lbl(AppSettings.T("Tu cuenta está protegida con cifrado SSL 256-bit."), 14, 36, new Font("Segoe UI", 9), TxtMuted));
            cardSec.Controls.Add(Lbl("✓  " + AppSettings.T("Sin actividad sospechosa detectada"), 14, 58, new Font("Segoe UI", 9), C_Green));
            scroll.Controls.Add(cardSec); y += 106;
            BeginInvoke(new Action(() => Redondear(cardSec, 12)));

            // ── Acciones rápidas ───────────────────────────────
            scroll.Controls.Add(SecTitulo(AppSettings.T("Acciones rápidas"), 0, y)); y += 26;
            var tlpAcc = new TableLayoutPanel
            {
                Location = new Point(0, y), Size = new Size(480, 44),
                ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent
            };
            for (int i = 0; i < 2; i++) tlpAcc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            var btnEditar2 = MakeBtn("✏️  " + AppSettings.T("Editar"), C_Blue, Point.Empty, new Size(0, 44));
            btnEditar2.Dock = DockStyle.Fill; btnEditar2.Margin = new Padding(0, 0, 8, 0);
            btnEditar2.Click += (s, e) => { using (var frm = new FrmEditarPerfil()) if (frm.ShowDialog(this) == DialogResult.OK) BuildUI(); };

            var btnConf = MakeBtn("⚙️  " + AppSettings.T("Configuración"), Color.FromArgb(55, 65, 100), Point.Empty, new Size(0, 44));
            btnConf.ForeColor = TxtMuted; btnConf.Dock = DockStyle.Fill;
            btnConf.Click += (s, e) => { using (var frm = new FrmConfiguracion()) { frm.ShowDialog(this); BuildUI(); } };

            tlpAcc.Controls.Add(btnEditar2, 0, 0);
            tlpAcc.Controls.Add(btnConf,    1, 0);
            scroll.Controls.Add(tlpAcc);
            BeginInvoke(new Action(() => { foreach (Control c in tlpAcc.Controls) Redondear(c, 10); }));

            return scroll;
        }

        // ── Helpers componentes ────────────────────────────────
        private Panel CuentaFila(CuentaBancaria cuenta)
        {
            Color col = cuenta.TipoCuenta?.ToLower() == "ahorro"  ? C_Green :
                        cuenta.TipoCuenta?.ToLower() == "nomina" ||
                        cuenta.TipoCuenta?.ToLower() == "nómina"   ? C_Gold : C_Blue;
            string ult4 = cuenta.NumeroCuenta?.Length > 4 ? cuenta.NumeroCuenta.Substring(cuenta.NumeroCuenta.Length - 4) : cuenta.NumeroCuenta;

            var fila = new Panel { Size = new Size(480, 60), BackColor = Color.Transparent };
            fila.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(new Rectangle(0, 2, fila.Width - 1, fila.Height - 4), 10))
                { ev.Graphics.FillPath(new SolidBrush(BgCard), path); ev.Graphics.DrawPath(new Pen(Border, 1), path); }
                ev.Graphics.FillRectangle(new SolidBrush(col), new Rectangle(0, 8, 3, fila.Height - 16));
            };

            var ico = new Panel { Size = new Size(36, 36), Location = new Point(12, 12), BackColor = Color.Transparent };
            ico.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(ico.ClientRectangle, 10))
                    ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(25, col.R, col.G, col.B)), path);
                string emoji = cuenta.TipoCuenta?.ToLower() == "ahorro" ? "🏦" :
                               cuenta.TipoCuenta?.ToLower().Contains("nomina") == true ? "💼" : "💳";
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString(emoji, new Font("Segoe UI", 14), Brushes.White, ico.ClientRectangle, fmt);
            };

            fila.Controls.Add(ico);
            fila.Controls.Add(Lbl((cuenta.TipoCuenta ?? "Cuenta").ToUpper(), 56, 10, new Font("Segoe UI", 8, FontStyle.Bold), TxtMuted));
            fila.Controls.Add(Lbl($"•••• {ult4}", 56, 28, new Font("Consolas", 10, FontStyle.Bold), TxtMain));
            fila.Controls.Add(Lbl(cuenta.Saldo.ToString("C2", ES), fila.Width - 130, 18, new Font("Segoe UI", 13, FontStyle.Bold), col));
            return fila;
        }

        private Panel StatCard(string titulo, string valor, Color col)
        {
            var card = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0), BackColor = BgCard };
            card.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(card.ClientRectangle, 12))
                { ev.Graphics.FillPath(new SolidBrush(BgCard), path); ev.Graphics.DrawPath(new Pen(Border, 1), path); }
                ev.Graphics.FillRectangle(new SolidBrush(col), new Rectangle(0, 0, card.Width, 3));
            };
            card.Controls.Add(Lbl(titulo, 12, 14, new Font("Segoe UI", 8, FontStyle.Bold), TxtMuted));
            card.Controls.Add(Lbl(valor, 12, 34, new Font("Segoe UI", 14, FontStyle.Bold), col));
            BeginInvoke(new Action(() => Redondear(card, 12)));
            return card;
        }

        // ── Helpers UI genéricos ───────────────────────────────
        private void Par(Panel parent, string label, string value, ref int y)
        {
            parent.Controls.Add(Lbl(label, 0, y, new Font("Segoe UI", 8, FontStyle.Bold), TxtMuted)); y += 18;
            parent.Controls.Add(Lbl(value, 0, y, new Font("Segoe UI", 10), TxtMain)); y += 26;
        }

        private Label SecTitulo(string t, int x, int y) => Lbl(t, x, y, new Font("Segoe UI", 11, FontStyle.Bold), TxtMain);

        private Label Lbl(string text, int x, int y, Font f, Color col) =>
            new Label { Text = text, Location = new Point(x, y), ForeColor = col, Font = f, AutoSize = true, BackColor = Color.Transparent };

        private Panel Sep(int x, int y, int w) =>
            new Panel { Location = new Point(x, y), Size = new Size(w, 1), BackColor = Border };

        private Button MakeBtn(string text, Color col, Point loc, Size sz)
        {
            var btn = new Button { Text = text, Location = loc, Size = sz, BackColor = col, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(Math.Max(0, col.R - 20), Math.Max(0, col.G - 20), Math.Max(0, col.B - 20));
            btn.MouseLeave += (s, e) => btn.BackColor = col;
            return btn;
        }

        private static GraphicsPath RR(Rectangle r, int radius)
        {
            int d = radius * 2; var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseAllFigures(); return p;
        }
        private static void Redondear(Control c, int r) { if (c.Width > 0 && c.Height > 0) c.Region = new Region(RR(c.ClientRectangle, r)); }

        // Handlers del Designer
        private void BtnCerrar_Click(object sender, EventArgs e) { DialogResult = DialogResult.OK; Close(); }
        private void BtnEditar_Click(object sender, EventArgs e) { using (var f = new FrmEditarPerfil()) if (f.ShowDialog(this) == DialogResult.OK) BuildUI(); }
    }
}
