using NexumApp.Helpers;
using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace NexumApp.Forms.Principal
{
    public partial class FrmEditarPerfil : Form
    {
        private readonly AuthService _authService = new AuthService();
        private readonly Usuario     _usuario;

        // ── Tema dinámico ─────────────────────────────────────
        private Color BgMain   => AppSettings.ModoOscuro ? Color.FromArgb(10,  12,  28) : Color.FromArgb(244, 247, 254);
        private Color BgCard   => AppSettings.ModoOscuro ? Color.FromArgb(18,  22,  46) : Color.White;
        private Color BgInput  => AppSettings.ModoOscuro ? Color.FromArgb(24,  29,  58) : Color.FromArgb(245, 247, 250);
        private Color Border   => AppSettings.ModoOscuro ? Color.FromArgb(38,  44,  80) : Color.FromArgb(220, 225, 235);
        private Color TxtMain  => AppSettings.ModoOscuro ? Color.FromArgb(241, 245, 249): Color.FromArgb(31,  41,  55);
        private Color TxtMuted => AppSettings.ModoOscuro ? Color.FromArgb(100, 116, 139): Color.FromArgb(107, 114, 128);

        private static readonly Color C_Blue  = Color.FromArgb(99,  102, 241);
        private static readonly Color C_Violet= Color.FromArgb(139,  92, 246);
        private static readonly Color C_Green = Color.FromArgb(52,  211, 153);
        private static readonly Color C_Red   = Color.FromArgb(248, 113, 113);

        // Controles
        private TextBox        _txtNombre, _txtApellidos, _txtEmail, _txtTelefono;
        private TextBox        _txtDNI, _txtDireccion, _txtCiudad, _txtCP;
        private DateTimePicker _dtpNacimiento;
        private Label          _lblError;
        private Button         _btnGuardar;

        // Foto de perfil
        private string         _nuevaRutaFoto;   // null = sin cambios
        private Panel          _avatarPanel;

        public FrmEditarPerfil()
        {
            InitializeComponent();
            _usuario = SesionActual.Instancia.Usuario;
            DoubleBuffered = true;
        }

        protected override void OnShown(EventArgs e) { base.OnShown(e); BuildUI(); }

        // ══════════════════════════════════════════════════════
        //  UI
        // ══════════════════════════════════════════════════════
        private void BuildUI()
        {
            Controls.Clear();
            Text = AppSettings.T("Editar perfil") + " — Nexum Bank";
            Size = new Size(600, 720);
            MinimumSize = Size; MaximumSize = Size;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = BgMain;

            Paint += (s, ev) =>
            {
                if (!AppSettings.ModoOscuro) return;
                using (var b = new LinearGradientBrush(ClientRectangle, Color.FromArgb(12, 14, 34), BgMain, LinearGradientMode.Vertical))
                    ev.Graphics.FillRectangle(b, ClientRectangle);
            };

            // ── Bloque de foto + nombre (header del form) ─────
            var hdr = new Panel { Location = new Point(0, 0), Size = new Size(600, 128), BackColor = Color.Transparent };
            hdr.Paint += (s, ev) =>
            {
                var g = ev.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var b = new LinearGradientBrush(hdr.ClientRectangle, C_Blue, C_Violet, LinearGradientMode.Horizontal))
                    g.FillRectangle(b, hdr.ClientRectangle);
                // Brillo superior
                using (var b2 = new LinearGradientBrush(new Rectangle(0, 0, 600, 60), Color.FromArgb(40, 255, 255, 255), Color.Transparent, LinearGradientMode.Vertical))
                    g.FillRectangle(b2, 0, 0, 600, 60);
            };
            Controls.Add(hdr);

            // Avatar circular (foto actual o iniciales)
            _avatarPanel = new Panel { Size = new Size(88, 88), Location = new Point(32, 20), BackColor = Color.Transparent };
            _avatarPanel.Paint += PaintAvatar;
            hdr.Controls.Add(_avatarPanel);

            // Botón cambiar foto (superpuesto abajo-derecha del avatar)
            var btnCamFoto = new Panel { Size = new Size(28, 28), Location = new Point(_avatarPanel.Right - 28, _avatarPanel.Bottom - 28), BackColor = Color.Transparent, Cursor = Cursors.Hand };
            btnCamFoto.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(btnCamFoto.ClientRectangle, 14))
                    ev.Graphics.FillPath(new SolidBrush(C_Green), path);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString("📷", new Font("Segoe UI", 11), Brushes.White, btnCamFoto.ClientRectangle, fmt);
            };
            btnCamFoto.Click += (s, e) => ElegirFoto();
            hdr.Controls.Add(btnCamFoto);

            // Nombre + email en el header
            hdr.Controls.Add(CLabel(_usuario?.NombreCompleto ?? "—", new Point(134, 30), 420, 26, new Font("Segoe UI", 15, FontStyle.Bold), Color.White, ContentAlignment.MiddleLeft));
            hdr.Controls.Add(CLabel(_usuario?.Email ?? "—", new Point(134, 60), 420, 18, new Font("Segoe UI", 9), Color.FromArgb(200, 255, 255, 255), ContentAlignment.MiddleLeft));
            hdr.Controls.Add(CLabel("Haz clic en 📷 para cambiar tu foto de perfil", new Point(134, 84), 420, 18, new Font("Segoe UI", 8, FontStyle.Italic), Color.FromArgb(170, 255, 255, 255), ContentAlignment.MiddleLeft));

            // ── Card principal con scroll ─────────────────────
            var scroll = new Panel { Location = new Point(24, 142), Size = new Size(552, 500), AutoScroll = true, BackColor = BgCard };
            scroll.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(scroll.ClientRectangle, 14))
                { ev.Graphics.FillPath(new SolidBrush(BgCard), path); ev.Graphics.DrawPath(new Pen(Border, 1), path); }
            };
            Controls.Add(scroll);
            BeginInvoke(new Action(() => Redondear(scroll, 14)));

            int iw = 504, y = 20;

            // ── Datos personales ──────────────────────────────
            scroll.Controls.Add(SecLbl("DATOS PERSONALES", 20, y)); y += 24;

            var tlpRow1 = new TableLayoutPanel { Location = new Point(20, y), Size = new Size(iw, 52), ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
            tlpRow1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            tlpRow1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            var pnlN = InputPanel("Nombre",    _usuario?.Nombre    ?? "", out _txtNombre);
            var pnlA = InputPanel("Apellidos", _usuario?.Apellidos ?? "", out _txtApellidos);
            pnlN.Dock = DockStyle.Fill; pnlN.Margin = new Padding(0, 0, 8, 0);
            pnlA.Dock = DockStyle.Fill;
            tlpRow1.Controls.Add(pnlN, 0, 0);
            tlpRow1.Controls.Add(pnlA, 1, 0);
            scroll.Controls.Add(tlpRow1); y += 60;

            scroll.Controls.Add(SecLbl("Email", 20, y)); y += 20;
            InputBoxFull(scroll, new Point(20, y), iw, _usuario?.Email ?? "", out _txtEmail); y += 48;

            var tlpRow2 = new TableLayoutPanel { Location = new Point(20, y), Size = new Size(iw, 52), ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
            tlpRow2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpRow2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            var pnlTel = InputPanel("Teléfono", _usuario?.Telefono ?? "", out _txtTelefono);
            var pnlDNI = InputPanel("DNI / NIF", _usuario?.DNI     ?? "", out _txtDNI);
            pnlTel.Dock = DockStyle.Fill; pnlTel.Margin = new Padding(0, 0, 8, 0);
            pnlDNI.Dock = DockStyle.Fill;
            tlpRow2.Controls.Add(pnlTel, 0, 0);
            tlpRow2.Controls.Add(pnlDNI, 1, 0);
            scroll.Controls.Add(tlpRow2); y += 60;

            scroll.Controls.Add(SecLbl("Fecha de nacimiento", 20, y)); y += 20;
            _dtpNacimiento = new DateTimePicker
            {
                Location = new Point(20, y), Size = new Size(iw, 36),
                Format = DateTimePickerFormat.Short,
                Value = _usuario?.FechaNacimiento ?? DateTime.Today.AddYears(-25),
                Font = new Font("Segoe UI", 10),
                CalendarForeColor = TxtMain, CalendarMonthBackground = BgCard
            };
            scroll.Controls.Add(_dtpNacimiento); y += 48;

            // ── Dirección ──────────────────────────────────────
            scroll.Controls.Add(new Panel { Location = new Point(20, y), Size = new Size(iw, 1), BackColor = Border }); y += 14;
            scroll.Controls.Add(SecLbl("DIRECCIÓN", 20, y)); y += 24;

            InputBoxFull(scroll, new Point(20, y), iw, _usuario?.Direccion ?? "", out _txtDireccion); y += 48;

            var tlpRow3 = new TableLayoutPanel { Location = new Point(20, y), Size = new Size(iw, 52), ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
            tlpRow3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            tlpRow3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            var pnlCiu = InputPanel("Ciudad",         _usuario?.Ciudad        ?? "", out _txtCiudad);
            var pnlCP  = InputPanel("Código postal",  _usuario?.CodigoPostal  ?? "", out _txtCP);
            pnlCiu.Dock = DockStyle.Fill; pnlCiu.Margin = new Padding(0, 0, 8, 0);
            pnlCP.Dock  = DockStyle.Fill;
            tlpRow3.Controls.Add(pnlCiu, 0, 0);
            tlpRow3.Controls.Add(pnlCP, 1, 0);
            scroll.Controls.Add(tlpRow3); y += 60;

            // ── Error + botones ────────────────────────────────
            _lblError = new Label { Location = new Point(20, y), Size = new Size(iw, 18), ForeColor = C_Red, Font = new Font("Segoe UI", 8, FontStyle.Bold), Visible = false, BackColor = Color.Transparent };
            scroll.Controls.Add(_lblError); y += 22;

            scroll.Controls.Add(new Panel { Location = new Point(20, y), Size = new Size(iw, 1), BackColor = Border }); y += 12;

            _btnGuardar = MakeBtn("✓  Guardar cambios", C_Green, new Point(20, y), new Size(iw, 44));
            _btnGuardar.ForeColor = Color.FromArgb(4, 30, 20);
            _btnGuardar.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(_btnGuardar.ClientRectangle, 10))
                using (var b = new LinearGradientBrush(_btnGuardar.ClientRectangle, C_Green, Color.FromArgb(34, 197, 130), LinearGradientMode.Horizontal))
                    ev.Graphics.FillPath(b, path);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                ev.Graphics.DrawString(_btnGuardar.Text, _btnGuardar.Font, new SolidBrush(Color.FromArgb(4, 40, 20)), _btnGuardar.ClientRectangle, fmt);
            };
            _btnGuardar.FlatAppearance.BorderSize = 0;
            _btnGuardar.Click += BtnGuardar_Click;
            scroll.Controls.Add(_btnGuardar); y += 52;
            BeginInvoke(new Action(() => { Redondear(_btnGuardar, 10); }));

            var btnCancelar = new Button
            {
                Text = "Cancelar", Location = new Point(20, y), Size = new Size(iw, 30),
                BackColor = Color.Transparent, ForeColor = TxtMuted,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.MouseEnter += (s, e) => btnCancelar.ForeColor = TxtMain;
            btnCancelar.MouseLeave += (s, e) => btnCancelar.ForeColor = TxtMuted;
            btnCancelar.Click += BtnCancelar_Click;
            scroll.Controls.Add(btnCancelar);
        }

        // ── Pintar avatar (foto o iniciales) ──────────────────
        private void PaintAvatar(object sender, PaintEventArgs ev)
        {
            var g = ev.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = _avatarPanel.ClientRectangle;

            // Sombra suave
            using (var sh = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
                g.FillEllipse(sh, r.X + 2, r.Y + 4, r.Width - 2, r.Height - 2);

            // ¿Hay foto?
            string fotoPath = _nuevaRutaFoto ?? _usuario?.FotoPerfil;
            if (!string.IsNullOrEmpty(fotoPath) && File.Exists(fotoPath))
            {
                try
                {
                    using (var img = Image.FromFile(fotoPath))
                    {
                        // Clip circular
                        using (var path = new GraphicsPath())
                        {
                            path.AddEllipse(r);
                            g.SetClip(path);
                            g.DrawImage(img, r);
                            g.ResetClip();
                        }
                    }
                    // Borde blanco
                    using (var p = new Pen(Color.FromArgb(200, 255, 255, 255), 2))
                        g.DrawEllipse(p, r.X + 1, r.Y + 1, r.Width - 2, r.Height - 2);
                    return;
                }
                catch { /* Si falla, caer en iniciales */ }
            }

            // Sin foto: iniciales con degradado
            using (var b = new LinearGradientBrush(r, Color.FromArgb(180, 255, 255, 255), Color.FromArgb(60, 255, 255, 255), 135f))
                g.FillEllipse(b, r);
            using (var p = new Pen(Color.FromArgb(200, 255, 255, 255), 2))
                g.DrawEllipse(p, r.X + 1, r.Y + 1, r.Width - 2, r.Height - 2);
            string ini = ((_usuario?.Nombre?.Length > 0 ? $"{_usuario.Nombre[0]}" : "?")
                        + (_usuario?.Apellidos?.Length > 0 ? $"{_usuario.Apellidos[0]}" : "")).ToUpper();
            var fmt2 = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(ini, new Font("Segoe UI", 26, FontStyle.Bold), Brushes.White, r, fmt2);
        }

        // ── Elegir foto ────────────────────────────────────────
        private void ElegirFoto()
        {
            using (var dlg = new OpenFileDialog
            {
                Title  = "Selecciona tu foto de perfil",
                Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Multiselect = false
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    // Guardar copia en AppData
                    string dir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "NexumApp", "Fotos");
                    Directory.CreateDirectory(dir);
                    string dest = Path.Combine(dir, $"{_usuario.Id}.jpg");

                    // Convertir y redimensionar a 200×200 JPEG
                    using (var orig = Image.FromFile(dlg.FileName))
                    {
                        int sz = 200;
                        using (var bmp = new Bitmap(sz, sz))
                        using (var g = Graphics.FromImage(bmp))
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode     = SmoothingMode.AntiAlias;
                            // Crop cuadrado centrado
                            int srcSz = Math.Min(orig.Width, orig.Height);
                            int srcX  = (orig.Width  - srcSz) / 2;
                            int srcY  = (orig.Height - srcSz) / 2;
                            g.DrawImage(orig, new Rectangle(0, 0, sz, sz),
                                new Rectangle(srcX, srcY, srcSz, srcSz), GraphicsUnit.Pixel);
                            bmp.Save(dest, ImageFormat.Jpeg);
                        }
                    }
                    _nuevaRutaFoto = dest;
                    _avatarPanel?.Invalidate();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo cargar la imagen: " + ex.Message, "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        // ── Guardado ───────────────────────────────────────────
        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            _lblError.Visible = false;
            string nombre    = _txtNombre?.Text.Trim() ?? "";
            string apellidos = _txtApellidos?.Text.Trim() ?? "";
            string email     = _txtEmail?.Text.Trim() ?? "";
            string telefono  = _txtTelefono?.Text.Trim();
            string dni       = _txtDNI?.Text.Trim();
            string dir       = _txtDireccion?.Text.Trim();
            string ciudad    = _txtCiudad?.Text.Trim();
            string cp        = _txtCP?.Text.Trim();
            DateTime? fNac   = _dtpNacimiento?.Value.Date;

            if (string.IsNullOrWhiteSpace(nombre))   { ErrMsg("El nombre es obligatorio."); return; }
            if (string.IsNullOrWhiteSpace(apellidos)) { ErrMsg("Los apellidos son obligatorios."); return; }
            if (string.IsNullOrWhiteSpace(email))     { ErrMsg("El email es obligatorio."); return; }
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) { ErrMsg("El email no tiene un formato válido."); return; }
            if (_authService.EmailExisteParaOtro(email, _usuario.Id))  { ErrMsg("Este email ya está registrado."); return; }

            _btnGuardar.Enabled = false; _btnGuardar.Text = "Guardando...";
            try
            {
                bool ok = _authService.ActualizarPerfilCompleto(_usuario.Id, nombre, apellidos, email, telefono, dni, dir, ciudad, cp, fNac);

                // Guardar foto si se eligió una nueva
                if (ok && _nuevaRutaFoto != null)
                    _authService.ActualizarFotoPerfil(_usuario.Id, _nuevaRutaFoto);

                if (ok)
                {
                    // Actualizar modelo en sesión
                    _usuario.Nombre = nombre; _usuario.Apellidos = apellidos; _usuario.Email = email;
                    _usuario.Telefono = telefono; _usuario.DNI = dni; _usuario.Direccion = dir;
                    _usuario.Ciudad = ciudad; _usuario.CodigoPostal = cp; _usuario.FechaNacimiento = fNac;
                    if (_nuevaRutaFoto != null) _usuario.FotoPerfil = _nuevaRutaFoto;

                    MessageBox.Show("Perfil actualizado correctamente.", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.None);
                    DialogResult = DialogResult.OK; Close();
                }
                else
                {
                    ErrMsg("Error al guardar. Inténtalo de nuevo.");
                    _btnGuardar.Enabled = true; _btnGuardar.Text = "✓  Guardar cambios";
                }
            }
            catch (Exception ex)
            {
                ErrMsg("Error: " + ex.Message);
                _btnGuardar.Enabled = true; _btnGuardar.Text = "✓  Guardar cambios";
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e) => Close();
        private void ErrMsg(string msg) { _lblError.Text = "⚠  " + msg; _lblError.Visible = true; }

        // ── Helpers UI ─────────────────────────────────────────
        private Panel InputPanel(string label, string val, out TextBox txt)
        {
            var pnl = new Panel { BackColor = Color.Transparent };
            pnl.Controls.Add(new Label { Text = label.ToUpper(), ForeColor = TxtMuted, Font = new Font("Segoe UI", 7, FontStyle.Bold), Location = new Point(0, 0), AutoSize = true });
            var box = new Panel { Location = new Point(0, 18), Height = 34, BackColor = BgInput };
            box.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(box.ClientRectangle, 8))
                { ev.Graphics.FillPath(new SolidBrush(BgInput), path); ev.Graphics.DrawPath(new Pen(Border, 1), path); }
            };
            txt = new TextBox { Location = new Point(8, 6), Height = 22, Font = new Font("Segoe UI", 10), BackColor = BgInput, ForeColor = TxtMain, BorderStyle = BorderStyle.None, Text = val };
            box.Controls.Add(txt);
            txt.GotFocus  += (s, e) => { box.Tag = C_Blue;  box.Paint -= BorderDyn; box.Paint += BorderDyn; box.Invalidate(); };
            txt.LostFocus += (s, e) => { box.Tag = Border;  box.Paint -= BorderDyn; box.Paint += BorderDyn; box.Invalidate(); };
            pnl.Controls.Add(box);
            pnl.Height = 52;
            var txtLocal = txt;
            box.Resize += (s, e) => txtLocal.Width = box.Width - 16;
            pnl.Resize  += (s, e) => box.Width = pnl.Width;
            return pnl;
        }

        private void InputBoxFull(Panel parent, Point loc, int w, string val, out TextBox txt)
        {
            var pnl = new Panel { Location = loc, Size = new Size(w, 40), BackColor = BgInput };
            pnl.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RR(pnl.ClientRectangle, 9))
                { ev.Graphics.FillPath(new SolidBrush(BgInput), path); ev.Graphics.DrawPath(new Pen(Border, 1), path); }
            };
            txt = new TextBox { Location = new Point(10, 9), Size = new Size(w - 20, 22), Font = new Font("Segoe UI", 10), BackColor = BgInput, ForeColor = TxtMain, BorderStyle = BorderStyle.None, Text = val };
            txt.GotFocus  += (s, e) => { pnl.Tag = C_Blue;  pnl.Paint -= BorderDyn; pnl.Paint += BorderDyn; pnl.Invalidate(); };
            txt.LostFocus += (s, e) => { pnl.Tag = Border;  pnl.Paint -= BorderDyn; pnl.Paint += BorderDyn; pnl.Invalidate(); };
            pnl.Controls.Add(txt);
            parent.Controls.Add(pnl);
        }

        private void BorderDyn(object s, PaintEventArgs ev)
        {
            var p = (Panel)s; var col = p.Tag is Color c ? c : Border;
            ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = RR(p.ClientRectangle, 9))
            { ev.Graphics.FillPath(new SolidBrush(BgInput), path); ev.Graphics.DrawPath(new Pen(col, 1.5f), path); }
        }

        private Label SecLbl(string t, int x, int y) =>
            new Label { Text = t, Location = new Point(x, y), ForeColor = TxtMuted, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true };

        private Label CLabel(string text, Point loc, int w, int h, Font f, Color col, ContentAlignment align) =>
            new Label { Text = text, Location = loc, Size = new Size(w, h), Font = f, ForeColor = col, TextAlign = align, BackColor = Color.Transparent };

        private Button MakeBtn(string text, Color col, Point loc, Size sz)
        {
            var btn = new Button { Text = text, Location = loc, Size = sz, BackColor = col, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
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

        private void BtnCerrar_Click(object sender, EventArgs e) => Close();
        private void BtnEditar_Click(object sender, EventArgs e) { }
    }
}
