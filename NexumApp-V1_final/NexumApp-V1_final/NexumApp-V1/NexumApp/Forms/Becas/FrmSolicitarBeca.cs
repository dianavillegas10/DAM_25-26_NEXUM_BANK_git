using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NexumApp.Forms.Becas
{
    internal class FrmSolicitarBeca : Form
    {
        private static readonly Color Indigo     = Color.FromArgb(99,  102, 241);
        private static readonly Color IndigoDark = Color.FromArgb(49,  46,  129);
        private static readonly Color White      = Color.White;
        private static readonly Color BgGray     = Color.FromArgb(244, 246, 252);
        private static readonly Color TextDark   = Color.FromArgb(17,  24,  39);
        private static readonly Color TextGray   = Color.FromArgb(107, 114, 128);
        private static readonly Color Border     = Color.FromArgb(229, 231, 235);

        private readonly Beca                 _beca;
        private readonly BecaSolicitudService _service         = new BecaSolicitudService();
        private readonly UsuarioService       _usuarioService  = new UsuarioService();

        private TextBox  _txtDNI;
        private TextBox  _txtTelefono;
        private TextBox  _txtCentro;
        private TextBox  _txtTitulacion;
        private ComboBox _cmbAnio;
        private TextBox  _txtNotaODesc;
        private TextBox  _txtMotivacion;
        private CheckBox _chkVeraz;
        private CheckBox _chkBases;
        private Button   _btnEnviar;

        public string NumeroContrato { get; private set; }

        public FrmSolicitarBeca(Beca beca)
        {
            _beca = beca;
            ConfigurarForm();
            ConstruirUI();
        }

        private void ConfigurarForm()
        {
            Text            = "Solicitar Beca — Nexum Bank";
            Size            = new Size(560, 720);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            BackColor       = BgGray;
            MaximizeBox     = false;

            Load += (s, e) =>
            {
                if (Width <= 0 || Height <= 0) return;
                var p = new GraphicsPath();
                int r = 20;
                p.AddArc(0,         0,          r, r, 180, 90);
                p.AddArc(Width - r, 0,          r, r, 270, 90);
                p.AddArc(Width - r, Height - r, r, r,   0, 90);
                p.AddArc(0,         Height - r, r, r,  90, 90);
                p.CloseFigure();
                Region = new Region(p);
            };
        }

        private void ConstruirUI()
        {
            var usuario = SesionActual.Instancia?.Usuario;

            bool esAcademica = _beca.Categoria == CategoriaBeca.Universitaria
                            || _beca.Categoria == CategoriaBeca.Posgrado
                            || _beca.Categoria == CategoriaBeca.FP;
            bool esDeportiva = _beca.Categoria == CategoriaBeca.Deportiva;

            string lblCentro  = esDeportiva ? "Club / Federación *"
                              : esAcademica ? "Universidad / Centro *"
                                            : "Organización / Entidad *";
            string lblTitulac = esDeportiva ? "Deporte / Disciplina *"
                              : esAcademica ? "Titulación / Estudios *"
                                            : "Nombre del proyecto *";
            string lblNota    = esDeportiva ? "Categoría deportiva / Nivel *"
                              : esAcademica ? "Nota media del expediente *"
                                            : "Descripción del proyecto *";

            // ── Header ────────────────────────────────────────────────
            var pnlHeader = new Panel { Size = new Size(560, 90), Location = Point.Empty };
            pnlHeader.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new LinearGradientBrush(Point.Empty, new Point(560, 90), IndigoDark, Indigo))
                    g.FillRectangle(br, pnlHeader.ClientRectangle);
                using (var br2 = new SolidBrush(Color.FromArgb(20, 255, 255, 255)))
                {
                    g.FillEllipse(br2, 420, -40, 200, 200);
                    g.FillEllipse(br2, 500,  30, 100, 100);
                }
            };
            var btnX = new Button
            {
                Text = "✕", Size = new Size(30, 30), Location = new Point(520, 10),
                BackColor = Color.FromArgb(60, 255, 255, 255), ForeColor = White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnX.FlatAppearance.BorderSize = 0;
            btnX.Click += (s, e) => Close();
            pnlHeader.Controls.Add(new Label
            {
                Text = "🎓  Solicitar Beca", ForeColor = White,
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                AutoSize = true, BackColor = Color.Transparent, Location = new Point(24, 14)
            });
            pnlHeader.Controls.Add(new Label
            {
                Text = _beca.Titulo, ForeColor = Color.FromArgb(165, 180, 252),
                Font = new Font("Segoe UI", 10),
                AutoSize = true, BackColor = Color.Transparent, Location = new Point(26, 52)
            });
            pnlHeader.Controls.Add(btnX);

            // ── Resumen beca ──────────────────────────────────────────
            var pnlResumen = new Panel { Location = new Point(16, 96), Size = new Size(528, 58), BackColor = White };
            pnlResumen.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RRect(new Rectangle(0, 0, pnlResumen.Width, pnlResumen.Height), 10))
                {
                    ev.Graphics.FillPath(new SolidBrush(White), path);
                    ev.Graphics.DrawPath(new Pen(Border, 1), path);
                }
            };
            AgregarDatoResumen(pnlResumen, 12,  $"{_beca.Importe:N0} €", "Importe",  new Font("Segoe UI", 13, FontStyle.Bold), Indigo);
            AgregarDatoResumen(pnlResumen, 178, _beca.DuracionTexto,     "Duración", new Font("Segoe UI", 10, FontStyle.Bold), TextDark);
            AgregarDatoResumen(pnlResumen, 318, _beca.EntidadConvocante, "Entidad",  new Font("Segoe UI", 9),                  TextDark);

            // ── Panel scroll (todo el contenido) ─────────────────────
            var scroll = new Panel
            {
                Location   = new Point(0, 160),
                Size       = new Size(560, 456),
                AutoScroll = true,
                BackColor  = BgGray
            };

            int y = 10;

            // ── Sección 1: Datos Personales ───────────────────────────
            y = AñadirSeccion(scroll, "1  DATOS PERSONALES", y);

            // Fila: Nombre (read-only) + Email (read-only)
            AñadirLabel(scroll, "Nombre completo", 16,  y);
            AñadirLabel(scroll, "Email",           288, y);
            y += 18;
            AñadirReadOnly(scroll, usuario?.NombreCompleto ?? "", 16,  y, 256);
            AñadirReadOnly(scroll, usuario?.Email          ?? "", 288, y, 240);
            y += 46;

            // Fila: DNI + Teléfono (editables)
            AñadirLabel(scroll, "DNI / NIE *", 16,  y);
            AñadirLabel(scroll, "Teléfono *",  288, y);
            y += 18;
            _txtDNI      = CrearTxt(scroll, 16,  y, 256, usuario?.DNI      ?? "");
            _txtTelefono = CrearTxt(scroll, 288, y, 240, usuario?.Telefono ?? "");
            y += 50;

            // ── Sección 2: Datos Académicos ───────────────────────────
            y = AñadirSeccion(scroll, "2  DATOS ACADÉMICOS / PROYECTO", y);

            // Fila: Centro + Año
            AñadirLabel(scroll, lblCentro,         16,  y);
            AñadirLabel(scroll, "Año académico *",  288, y);
            y += 18;
            _txtCentro = CrearTxt(scroll, 16, y, 256, "");
            _cmbAnio = new ComboBox
            {
                Location = new Point(288, y), Size = new Size(240, 28),
                Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat,
                BackColor = White, DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbAnio.Items.AddRange(new object[] { "2024/2025", "2025/2026", "2026/2027" });
            _cmbAnio.SelectedIndex = 1;
            scroll.Controls.Add(_cmbAnio);
            y += 50;

            // Fila: Titulación (ancho completo)
            AñadirLabel(scroll, lblTitulac, 16, y);
            y += 18;
            _txtTitulacion = CrearTxt(scroll, 16, y, 512, "");
            y += 50;

            // Fila: Nota / Descripción (ancho completo)
            AñadirLabel(scroll, lblNota, 16, y);
            y += 18;
            _txtNotaODesc = CrearTxt(scroll, 16, y, 512, "");
            y += 54;

            // ── Sección 3: Carta de motivación ────────────────────────
            y = AñadirSeccion(scroll, "3  CARTA DE MOTIVACIÓN", y);
            scroll.Controls.Add(new Label
            {
                Text = "Explica por qué mereces esta beca (mínimo 50 caracteres)",
                ForeColor = TextGray, Font = new Font("Segoe UI", 8),
                AutoSize = false, Size = new Size(512, 16),
                BackColor = Color.Transparent, Location = new Point(16, y)
            });
            y += 20;
            _txtMotivacion = new TextBox
            {
                Location    = new Point(16, y), Size = new Size(512, 88),
                Multiline   = true, ScrollBars = ScrollBars.Vertical,
                Font        = new Font("Segoe UI", 10),
                BackColor   = White, ForeColor = TextDark,
                BorderStyle = BorderStyle.FixedSingle, MaxLength = 2000
            };
            scroll.Controls.Add(_txtMotivacion);
            y += 100;

            // ── Sección 4: Declaración ────────────────────────────────
            y = AñadirSeccion(scroll, "4  DECLARACIÓN JURADA", y);
            _chkVeraz = new CheckBox
            {
                Text = "Declaro que todos los datos proporcionados son verídicos y completos.",
                Font = new Font("Segoe UI", 9), ForeColor = TextDark,
                AutoSize = false, Size = new Size(512, 22),
                BackColor = Color.Transparent, Location = new Point(16, y)
            };
            scroll.Controls.Add(_chkVeraz);
            y += 28;
            _chkBases = new CheckBox
            {
                Text = "Acepto las bases de la convocatoria Nexum Bank y me comprometo a cumplirlas.",
                Font = new Font("Segoe UI", 9), ForeColor = TextDark,
                AutoSize = false, Size = new Size(512, 22),
                BackColor = Color.Transparent, Location = new Point(16, y)
            };
            scroll.Controls.Add(_chkBases);

            // ── Botones (fixed bottom) ────────────────────────────────
            var pnlBotones = new Panel
            {
                Location = new Point(0, 616), Size = new Size(560, 104), BackColor = BgGray
            };
            pnlBotones.Paint += (s, ev) =>
            {
                using (var pen = new Pen(Border, 1))
                    ev.Graphics.DrawLine(pen, 16, 0, 544, 0);
            };

            _btnEnviar = new Button
            {
                Text = "ENVIAR SOLICITUD",
                Size = new Size(528, 46), Location = new Point(16, 12),
                BackColor = Indigo, ForeColor = White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand
            };
            _btnEnviar.FlatAppearance.BorderSize = 0;
            _btnEnviar.Click += BtnEnviar_Click;

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Size = new Size(528, 30), Location = new Point(16, 64),
                BackColor = Color.Transparent, ForeColor = TextGray,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (s, e) => Close();

            pnlBotones.Controls.Add(_btnEnviar);
            pnlBotones.Controls.Add(btnCancelar);

            Controls.Add(pnlHeader);
            Controls.Add(pnlResumen);
            Controls.Add(scroll);
            Controls.Add(pnlBotones);
        }

        // ── Helpers de layout ─────────────────────────────────────────
        private int AñadirSeccion(Panel parent, string titulo, int y)
        {
            var pnl = new Panel
            {
                Location = new Point(16, y), Size = new Size(512, 24),
                BackColor = Color.FromArgb(238, 242, 255)
            };
            pnl.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RRect(new Rectangle(0, 0, pnl.Width, pnl.Height), 6))
                    ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(238, 242, 255)), path);
            };
            pnl.Controls.Add(new Label
            {
                Text = titulo, ForeColor = Indigo,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                AutoSize = true, BackColor = Color.Transparent, Location = new Point(8, 4)
            });
            parent.Controls.Add(pnl);
            return y + 32;
        }

        private void AñadirLabel(Panel parent, string texto, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = texto, ForeColor = TextGray,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                AutoSize = true, BackColor = Color.Transparent, Location = new Point(x, y)
            });
        }

        private void AñadirReadOnly(Panel parent, string valor, int x, int y, int w)
        {
            parent.Controls.Add(new TextBox
            {
                Location    = new Point(x, y), Size = new Size(w, 28),
                Font        = new Font("Segoe UI", 10),
                BackColor   = Color.FromArgb(243, 244, 246),
                ForeColor   = TextGray,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly    = true, Text = valor
            });
        }

        private TextBox CrearTxt(Panel parent, int x, int y, int w, string valor)
        {
            var tb = new TextBox
            {
                Location    = new Point(x, y), Size = new Size(w, 28),
                Font        = new Font("Segoe UI", 10),
                BackColor   = White, ForeColor = TextDark,
                BorderStyle = BorderStyle.FixedSingle, Text = valor
            };
            parent.Controls.Add(tb);
            return tb;
        }

        private void AgregarDatoResumen(Panel parent, int x, string valor, string label, Font fValor, Color colorValor)
        {
            parent.Controls.Add(new Label
            {
                Text = valor, ForeColor = colorValor, Font = fValor,
                AutoSize = true, BackColor = Color.Transparent, Location = new Point(x, 8)
            });
            parent.Controls.Add(new Label
            {
                Text = label, ForeColor = TextGray, Font = new Font("Segoe UI", 7.5f),
                AutoSize = true, BackColor = Color.Transparent, Location = new Point(x, 38)
            });
        }

        // ── Validación y envío ────────────────────────────────────────
        private void BtnEnviar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtDNI.Text))
            { Error("El DNI / NIE es obligatorio.", _txtDNI); return; }

            if (string.IsNullOrWhiteSpace(_txtTelefono.Text))
            { Error("El teléfono es obligatorio.", _txtTelefono); return; }

            if (string.IsNullOrWhiteSpace(_txtCentro.Text))
            { Error("El centro / organización es obligatorio.", _txtCentro); return; }

            if (string.IsNullOrWhiteSpace(_txtTitulacion.Text))
            { Error("La titulación / nombre del proyecto es obligatoria.", _txtTitulacion); return; }

            if (string.IsNullOrWhiteSpace(_txtNotaODesc.Text))
            { Error("Este campo es obligatorio.", _txtNotaODesc); return; }

            if (_txtMotivacion.Text.Trim().Length < 50)
            { Error("La carta de motivación debe tener al menos 50 caracteres.", _txtMotivacion); return; }

            if (!_chkVeraz.Checked)
            { MessageBox.Show("Debes declarar que los datos son verídicos.", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (!_chkBases.Checked)
            { MessageBox.Show("Debes aceptar las bases de la convocatoria.", "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            int uid = SesionActual.Instancia?.Usuario?.Id ?? 0;
            if (uid == 0) { MessageBox.Show("No hay sesión activa.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            _btnEnviar.Enabled = false;
            _btnEnviar.Text    = "Enviando...";

            var (exito, numContrato, error) = _service.Solicitar(
                uid, _beca,
                _txtMotivacion.Text.Trim(),
                _txtCentro.Text.Trim(),
                _txtTitulacion.Text.Trim(),
                _cmbAnio.SelectedItem?.ToString() ?? "",
                _txtNotaODesc.Text.Trim());

            if (exito)
            {
                if (SesionActual.Instancia?.Usuario != null)
                {
                    string dni = _txtDNI.Text.Trim();
                    string tel = _txtTelefono.Text.Trim();
                    // Actualizar en sesión y en BD
                    SesionActual.Instancia.Usuario.DNI      = dni;
                    SesionActual.Instancia.Usuario.Telefono = tel;
                    _usuarioService.ActualizarDatosPersonales(uid, dni, tel);
                }
                NumeroContrato = numContrato;
                DialogResult   = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show($"Error al enviar la solicitud:\n{error}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _btnEnviar.Enabled = true;
                _btnEnviar.Text    = "ENVIAR SOLICITUD";
            }
        }

        private void Error(string msg, Control foco)
        {
            MessageBox.Show(msg, "Nexum Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            foco.Focus();
        }

        private static GraphicsPath RRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X,         r.Y,          d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y,          d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d,   0, 90);
            path.AddArc(r.X,         r.Bottom - d, d, d,  90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
