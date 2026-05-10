using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace NexumApp.Forms.Principal
{
    public class FrmNuevaHucha : Form
    {
        private static readonly (string Emoji, string Hex, string Nombre)[] _opts =
        {
            ("🏖️","#3B82F6","Vacaciones"),  ("🚗","#F97316","Coche nuevo"),
            ("🛡️","#10B981","Emergencia"),   ("✈️","#6366F1","Viajes"),
            ("🏠","#14B8A6","Casa"),          ("🎓","#8B5CF6","Estudios"),
            ("💍","#EC4899","Boda"),          ("🎁","#F59E0B","Capricho"),
            ("🏋️","#EF4444","Deporte"),       ("💡","#64748B","Personalizada"),
        };

        private static readonly CultureInfo ES = CultureInfo.CreateSpecificCulture("es-ES");
        private static readonly Color Bg     = Color.FromArgb(248, 250, 252);
        private static readonly Color Oscuro = Color.FromArgb( 15,  23,  42);
        private static readonly Color Gris   = Color.FromArgb(100, 116, 139);
        private static readonly Color Border = Color.FromArgb(226, 232, 240);

        private int      _sel = 0;
        private Panel    _preview;
        private TextBox  _txtNombre, _txtMeta, _txtEmojiPersonalizado;
        private Panel    _pnlEmojiCustom;
        private Panel[]  _btns;

        public Hucha HuchaCreada { get; private set; }

        public FrmNuevaHucha()
        {
            Text            = "Nueva Hucha";
            Size            = new Size(500, 620);
            MinimumSize     = MaximumSize = new Size(500, 620);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false; MinimizeBox = false;
            BackColor       = Bg;
            Font            = new Font("Segoe UI", 10);
            BuildUI();
        }

        private void BuildUI()
        {
            const int PX = 28;

            // ── HEADER con gradiente ──────────────────────────────
            var header = new Panel { Location = new Point(0, 0), Size = new Size(500, 88), BackColor = Color.FromArgb(99, 102, 241) };
            header.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = header.ClientRectangle;
                using (var b = new LinearGradientBrush(r, Color.FromArgb(99,102,241), Color.FromArgb(139,92,246), 135f))
                    g.FillRectangle(b, r);
                using (var b2 = new LinearGradientBrush(new Rectangle(0,0,r.Width,r.Height/2),
                    Color.FromArgb(45,255,255,255), Color.FromArgb(0,255,255,255), LinearGradientMode.Vertical))
                    g.FillRectangle(b2, 0, 0, r.Width, r.Height/2);
                using (var br = new SolidBrush(Color.FromArgb(15,255,255,255)))
                { g.FillEllipse(br, r.Width-130,-40,180,180); g.FillEllipse(br,-40,-30,130,130); }
            };
            // Emoji del cerdo (Label para renderizado correcto)
            var lPig = new Label { Text = "🐷", Font = new Font("Segoe UI Emoji",24), BackColor = Color.Transparent, AutoSize = true, Location = new Point(PX, 20) };
            // Títulos con posición explícita a la derecha del emoji
            var lT1 = new Label { Text = "Nueva Hucha de Ahorro", Font = new Font("Segoe UI",15,FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent, AutoSize = true, Location = new Point(PX + 56, 16) };
            var lT2 = new Label { Text = "Guarda dinero para tus objetivos", Font = new Font("Segoe UI",9.5f), ForeColor = Color.FromArgb(220,255,255,255), BackColor = Color.Transparent, AutoSize = true, Location = new Point(PX + 56, 46) };
            header.Controls.Add(lPig);
            header.Controls.Add(lT1);
            header.Controls.Add(lT2);
            Controls.Add(header);

            int y = 100;  // inicio del cuerpo

            // ── SECCIÓN: elige icono ──────────────────────────────
            Controls.Add(SecLabel("Elige un icono:", new Point(PX, y))); y += 24;

            // Grid 2×5 de emojis — posición absoluta y controlada
            const int bW = 76, bH = 66, gX = 8, gY = 8, COLS = 5;
            _btns = new Panel[_opts.Length];
            for (int i = 0; i < _opts.Length; i++)
            {
                int idx = i;
                var (emoji, hex, nom) = _opts[i];
                Color clr = ParseHex(hex);

                int col = idx % COLS, row = idx / COLS;
                var btn = new Panel
                {
                    Size      = new Size(bW, bH),
                    Location  = new Point(PX + col*(bW+gX), y + row*(bH+gY)),
                    BackColor = Color.White,
                    Cursor    = Cursors.Hand
                };
                EventHandler ar = (s, ev) => { try { btn.Region = new Region(RR(btn.ClientRectangle,12)); } catch { } };
                btn.HandleCreated += ar; btn.Resize += ar;

                btn.Paint += (s, e) =>
                {
                    bool sel = idx == _sel;
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    var r = btn.ClientRectangle;
                    using (var path = RR(r, 12))
                    {
                        e.Graphics.FillPath(new SolidBrush(sel ? Color.FromArgb(18,clr.R,clr.G,clr.B) : Color.White), path);
                        e.Graphics.DrawPath(new Pen(sel ? clr : Border, sel ? 2f : 1f), path);
                    }
                    TextRenderer.DrawText(e.Graphics, nom,
                        new Font("Segoe UI", 7f),
                        new Rectangle(0, r.Height-17, r.Width, 16),
                        sel ? clr : Gris,
                        TextFormatFlags.HorizontalCenter);
                };

                var lE = new Label { Text = emoji, Font = new Font("Segoe UI Emoji",19), BackColor = Color.Transparent, AutoSize = false, Size = new Size(bW, bH-17), Location = new Point(0,0), TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };
                btn.Controls.Add(lE);

                btn.Click  += (s, e) => SelOpt(idx);
                lE.Click   += (s, e) => SelOpt(idx);
                _btns[i] = btn;
                Controls.Add(btn);
            }
            y += 2*(bH+gY) + 12;

            // ── Emoji personalizado (solo visible cuando Otros) ───
            _pnlEmojiCustom = new Panel { Location = new Point(PX, y), Size = new Size(444, 36), BackColor = Color.Transparent, Visible = false };
            Controls.Add(_pnlEmojiCustom);
            var lEmojiLbl = new Label { Text = "Emoji personalizado:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Gris, AutoSize = true, Location = new Point(0, 10) };
            _txtEmojiPersonalizado = new TextBox { Location = new Point(160, 5), Size = new Size(60, 28), Font = new Font("Segoe UI Emoji", 14), BorderStyle = BorderStyle.FixedSingle, Text = "💡", TextAlign = HorizontalAlignment.Center };
            _txtEmojiPersonalizado.TextChanged += (s, e) => _preview?.Invalidate();
            _pnlEmojiCustom.Controls.Add(lEmojiLbl);
            _pnlEmojiCustom.Controls.Add(_txtEmojiPersonalizado);

            // ── PREVIEW ───────────────────────────────────────────
            int yPreview = y + 44;
            _preview = new Panel { Location = new Point(PX, yPreview), Size = new Size(444, 62), BackColor = Color.White };
            _preview.Paint += PintarPreview;
            EventHandler arP = (s, ev) => { try { _preview.Region = new Region(RR(_preview.ClientRectangle,14)); } catch { } };
            _preview.HandleCreated += arP; _preview.Resize += arP;
            Controls.Add(_preview);
            y = yPreview + 72;

            // ── NOMBRE ────────────────────────────────────────────
            Controls.Add(SecLabel("Nombre de la hucha:", new Point(PX, y))); y += 24;
            _txtNombre = new TextBox { Location = new Point(PX, y), Size = new Size(444, 34), Font = new Font("Segoe UI",11.5f,FontStyle.Bold), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, ForeColor = Oscuro };
            _txtNombre.TextChanged += (s, e) => _preview?.Invalidate();
            Controls.Add(_txtNombre); y += 46;

            // ── META ──────────────────────────────────────────────
            Controls.Add(SecLabel("Importe objetivo (€):", new Point(PX, y))); y += 24;
            _txtMeta = new TextBox { Location = new Point(PX, y), Size = new Size(200, 34), Font = new Font("Segoe UI",14,FontStyle.Bold), BorderStyle = BorderStyle.FixedSingle, Text = "1.000", ForeColor = Color.FromArgb(99,102,241) };
            _txtMeta.TextChanged += (s, e) => _preview?.Invalidate();
            Controls.Add(_txtMeta); y += 52;

            // ── BOTONES ───────────────────────────────────────────
            var btnOk = MakeBtn("✓   Crear Hucha", new Point(PX, y), new Size(214, 46), Color.FromArgb(16,185,129));
            btnOk.Click += BtnCrear_Click;
            Controls.Add(btnOk);

            var btnNo = MakeBtn("Cancelar", new Point(PX+222, y), new Size(130, 46), Color.FromArgb(241,245,249));
            btnNo.ForeColor = Gris;
            btnNo.FlatAppearance.MouseOverBackColor = Color.FromArgb(226,232,240);
            btnNo.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnNo);

            SelOpt(0);
        }

        // ── Seleccionar opción ────────────────────────────────────
        private void SelOpt(int idx)
        {
            _sel = idx;
            foreach (var b in _btns) b.Invalidate();

            // SIEMPRE actualiza el nombre al hacer clic en un emoji
            if (_txtNombre != null)
                _txtNombre.Text = _opts[idx].Nombre;

            // Mostrar campo emoji personalizado solo en "Personalizada" (idx=9)
            if (_pnlEmojiCustom != null)
            {
                bool esPersonalizada = (idx == _opts.Length - 1);
                _pnlEmojiCustom.Visible = esPersonalizada;
                // Desplazar preview y controles inferiores según visibilidad
                int offsetY = esPersonalizada ? 44 : 0;
                if (_preview != null)
                {
                    int baseY = _pnlEmojiCustom.Top + offsetY;
                    _preview.Top = baseY + (esPersonalizada ? 0 : 0);
                }
            }

            _preview?.Invalidate();
        }

        // ── Paint preview ─────────────────────────────────────────
        private void PintarPreview(object s, PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = _preview.ClientRectangle;

            bool esPersonalizada = _sel == _opts.Length - 1;
            string emoji = esPersonalizada
                ? (_txtEmojiPersonalizado?.Text ?? "💡")
                : _opts[_sel].Emoji;
            Color clr = ParseHex(_opts[_sel].Hex);

            using (var path = RR(r, 14)) { g.FillPath(Brushes.White, path); g.DrawPath(new Pen(Border), path); }
            using (var path = RR(new Rectangle(0, 10, 5, r.Height-20), 3))
                g.FillPath(new SolidBrush(clr), path);

            // Círculo con emoji (via DrawString con Segoe UI Emoji)
            var cR = new Rectangle(12, (r.Height-44)/2, 44, 44);
            using (var path = RR(cR, 22))
                g.FillPath(new SolidBrush(Color.FromArgb(18,clr.R,clr.G,clr.B)), path);
            var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(emoji, new Font("Segoe UI Emoji", 18), Brushes.Black,
                new RectangleF(cR.X, cR.Y, cR.Width, cR.Height), fmt);

            string nombre = string.IsNullOrWhiteSpace(_txtNombre?.Text) ? "Mi hucha" : _txtNombre.Text;
            TextRenderer.DrawText(g, nombre, new Font("Segoe UI",11,FontStyle.Bold),
                new Rectangle(66, 8, r.Width-130, 24), Oscuro, TextFormatFlags.Left);

            string metaTxt = "Meta: ";
            if (decimal.TryParse(_txtMeta?.Text?.Replace(".","").Replace(",","."),
                System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out decimal m))
                metaTxt += m.ToString("C0", ES);
            TextRenderer.DrawText(g, metaTxt, new Font("Segoe UI",9),
                new Rectangle(66, 34, r.Width-130, 20), Gris, TextFormatFlags.Left);

            var bR = new Rectangle(r.Width-52, (r.Height-24)/2, 44, 24);
            using (var path = RR(bR, 9))
                g.FillPath(new SolidBrush(Color.FromArgb(18,clr.R,clr.G,clr.B)), path);
            TextRenderer.DrawText(g, "0%", new Font("Segoe UI",9,FontStyle.Bold), bR, clr,
                TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
        }

        // ── Crear hucha ───────────────────────────────────────────
        private void BtnCrear_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtNombre.Text))
            { MessageBox.Show("Introduce un nombre.", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning); _txtNombre.Focus(); return; }

            string raw = (_txtMeta.Text ?? "").Replace(".", "").Replace(",", ".");
            if (!decimal.TryParse(raw, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out decimal meta) || meta <= 0)
            { MessageBox.Show("Introduce un importe objetivo válido.", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning); _txtMeta.Focus(); return; }

            int uid = Models.SesionActual.Instancia?.Usuario?.Id ?? 0;
            if (uid == 0) return;

            bool esPersonalizada = _sel == _opts.Length - 1;
            string emoji = esPersonalizada ? (_txtEmojiPersonalizado?.Text?.Trim() ?? "💡") : _opts[_sel].Emoji;

            bool ok = new HuchaService().Crear(new Hucha
            {
                UsuarioId = uid, Nombre = _txtNombre.Text.Trim(),
                Emoji = emoji, MetaObjetivo = meta, ColorHex = _opts[_sel].Hex
            });

            if (ok)
            {
                HuchaCreada  = new Hucha { Nombre = _txtNombre.Text.Trim() };
                DialogResult = DialogResult.OK;
                Close();
            }
            else
                MessageBox.Show("No se pudo guardar la hucha.", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // ── Helpers ───────────────────────────────────────────────
        private static Button MakeBtn(string txt, Point loc, Size sz, Color bg)
        {
            var b = new Button { Text = txt, Location = loc, Size = sz, BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI",10.5f,FontStyle.Bold), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(bg, 0.08f);
            EventHandler ar = (s, e) => { try { b.Region = new Region(RR(b.ClientRectangle,12)); } catch { } };
            b.HandleCreated += ar; b.Resize += ar;
            return b;
        }

        private static Label SecLabel(string txt, Point loc)
            => new Label { Text = txt, Location = loc, AutoSize = true, BackColor = Color.Transparent, Font = new Font("Segoe UI",9,FontStyle.Bold), ForeColor = Gris };

        private static Color ParseHex(string hex)
        { try { return ColorTranslator.FromHtml(hex); } catch { return Color.FromArgb(99,102,241); } }

        private static GraphicsPath RR(Rectangle r, int rad)
        {
            var p = new GraphicsPath(); int d = rad*2;
            if (r.Width<d||r.Height<d){p.AddRectangle(r);return p;}
            p.AddArc(r.X,r.Y,d,d,180,90); p.AddArc(r.Right-d,r.Y,d,d,270,90);
            p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90); p.AddArc(r.X,r.Bottom-d,d,d,90,90);
            p.CloseFigure(); return p;
        }
    }
}
