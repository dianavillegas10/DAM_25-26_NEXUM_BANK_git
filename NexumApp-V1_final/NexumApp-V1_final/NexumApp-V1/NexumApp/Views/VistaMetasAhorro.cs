using NexumApp.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace NexumApp.Views
{
    public class VistaMetasAhorro : UserControl
    {
        private FlowLayoutPanel _flpMetas;
        private List<ObjetivoAhorro> _metas;

        private static readonly Color AzulPrimario = Color.FromArgb(79, 70, 229);
        private static readonly Color TextoOscuro  = Color.FromArgb(17, 24, 39);
        private static readonly Color TextoGris    = Color.FromArgb(107, 114, 128);
        private static readonly Color FondoPage    = Color.FromArgb(248, 249, 252);

        public VistaMetasAhorro()
        {
            BackColor = FondoPage;
            Dock      = DockStyle.Fill;
            AutoScroll = true;
            Padding   = new Padding(28, 24, 28, 24);

            // Datos iniciales (en demo son hardcoded; en prod vendrían de BD)
            _metas = new List<ObjetivoAhorro>
            {
                new ObjetivoAhorro { Nombre = "Viaje a Japón ✈️",      MontoActual = 1800m,  MontoObjetivo = 5000m,  Progreso = 36m, FechaObjetivo = new DateTime(2026,12,1) },
                new ObjetivoAhorro { Nombre = "Entrada piso nuevo 🏡", MontoActual = 15250m, MontoObjetivo = 30000m, Progreso = 51m, FechaObjetivo = new DateTime(2027,6,1)  },
                new ObjetivoAhorro { Nombre = "Fondo de emergencia 🛡️", MontoActual = 2000m,  MontoObjetivo = 6000m,  Progreso = 33m, FechaObjetivo = null },
            };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ConstruirUI();
        }

        private void ConstruirUI()
        {
            Controls.Clear();

            // ── CABECERA ─────────────────────────────────────────────
            var tlpHead = new TableLayoutPanel
            {
                Dock        = DockStyle.Top,
                Height      = 60,
                ColumnCount = 2,
                RowCount    = 1,
                BackColor   = Color.Transparent
            };
            tlpHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpHead.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));

            tlpHead.Controls.Add(new Label
            {
                Text      = "🎯  Metas de Ahorro",
                Font      = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = TextoOscuro,
                AutoSize  = true,
                Anchor    = AnchorStyles.Left | AnchorStyles.Top
            }, 0, 0);

            var btnNueva = new Button
            {
                Text      = "+ Nueva meta",
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = AzulPrimario,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(148, 38),
                Anchor    = AnchorStyles.Right | AnchorStyles.Top,
                Cursor    = Cursors.Hand
            };
            btnNueva.FlatAppearance.BorderSize = 0;
            btnNueva.FlatAppearance.MouseOverBackColor = Color.FromArgb(67, 56, 202);
            EventHandler applyRegion = (s, e2) =>
            {
                if (btnNueva.Width > 4)
                    try { btnNueva.Region = new Region(RoundedRect(btnNueva.ClientRectangle, 10)); } catch { }
            };
            btnNueva.HandleCreated += applyRegion;
            btnNueva.Resize        += applyRegion;
            btnNueva.Click         += (s, e2) => AbrirFormNuevaMeta();
            tlpHead.Controls.Add(btnNueva, 1, 0);

            Controls.Add(tlpHead);

            // Subtítulo
            var lblSub = new Label
            {
                Text      = "Establece tus objetivos financieros y haz seguimiento de tu progreso.",
                Font      = new Font("Segoe UI", 10.5f),
                ForeColor = TextoGris,
                AutoSize  = false,
                Height    = 28,
                Dock      = DockStyle.Top,
                Padding   = new Padding(0, 2, 0, 8)
            };
            Controls.Add(lblSub);

            // Separador
            Controls.Add(new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(229, 231, 235), Margin = new Padding(0, 0, 0, 16) });
            Controls.Add(new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Color.Transparent });

            // ── LISTA DE METAS ────────────────────────────────────────
            _flpMetas = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                AutoScroll    = false,
                BackColor     = Color.Transparent
            };
            _flpMetas.SizeChanged += (s, e2) => AjustarAnchoMetas();
            Controls.Add(_flpMetas);

            RenderMetas();
        }

        private void RenderMetas()
        {
            _flpMetas.Controls.Clear();

            if (_metas == null || _metas.Count == 0)
            {
                _flpMetas.Controls.Add(new Label
                {
                    Text      = "Aún no tienes metas de ahorro. ¡Crea la primera!",
                    Font      = new Font("Segoe UI", 12),
                    ForeColor = TextoGris,
                    AutoSize  = true,
                    Margin    = new Padding(0, 24, 0, 0)
                });
                return;
            }

            foreach (var meta in _metas)
                _flpMetas.Controls.Add(CrearCardMeta(meta));
        }

        private Panel CrearCardMeta(ObjetivoAhorro meta)
        {
            int progreso = Math.Min(100, Math.Max(0, (int)meta.Progreso));
            var es = CultureInfo.CreateSpecificCulture("es-ES");

            Color c1 = progreso < 50 ? Color.FromArgb(16, 185, 129) : Color.FromArgb(249, 115, 22);
            Color c2 = progreso < 50 ? Color.FromArgb(99, 241, 180)  : Color.FromArgb(251, 191, 36);

            var card = new Panel
            {
                Height    = 120,
                BackColor = Color.White,
                Margin    = new Padding(0, 0, 0, 14),
                Padding   = new Padding(20, 14, 20, 14),
                Cursor    = Cursors.Hand
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(card.ClientRectangle, 14))
                {
                    e.Graphics.FillPath(Brushes.White, path);
                    e.Graphics.DrawPath(new Pen(Color.FromArgb(229, 231, 235), 1), path);
                }
            };

            // Icono
            var iconPnl = new Panel { Size = new Size(50, 50), Location = new Point(20, (120 - 50) / 2), BackColor = Color.Transparent };
            iconPnl.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = iconPnl.ClientRectangle;
                using (var path = RoundedRect(r, 12))
                using (var brush = new LinearGradientBrush(r, c1, c2, 45f))
                    e.Graphics.FillPath(brush, path);
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString("🎯", new Font("Segoe UI", 20), Brushes.White, new RectangleF(0, 0, 50, 50), fmt);
            };
            card.Controls.Add(iconPnl);

            // Nombre
            var lblNombre = new Label
            {
                Text      = meta.Nombre,
                Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = TextoOscuro,
                AutoSize  = true,
                Location  = new Point(82, 16)
            };
            card.Controls.Add(lblNombre);

            // Badge %
            var lblBadge = new Label
            {
                Text      = progreso + "%",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = c1,
                AutoSize  = true,
                Padding   = new Padding(8, 3, 8, 3),
                Location  = new Point(card.Width - 60, 16),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };
            card.Controls.Add(lblBadge);

            // Monto y fecha
            string montoTxt = meta.MontoObjetivo > 0
                ? meta.MontoActual.ToString("C0", es) + " / " + meta.MontoObjetivo.ToString("C0", es)
                : progreso + "% completado";
            string fechaTxt = meta.FechaObjetivo.HasValue
                ? "  ·  Meta: " + meta.FechaObjetivo.Value.ToString("MMM yyyy", es)
                : "";

            var lblMonto = new Label
            {
                Text      = montoTxt + fechaTxt,
                Font      = new Font("Segoe UI", 9),
                ForeColor = TextoGris,
                AutoSize  = true,
                Location  = new Point(82, 40)
            };
            card.Controls.Add(lblMonto);

            // Barra de progreso
            int barY  = 80;
            int barH  = 8;
            var barFondo = new Panel { Location = new Point(82, barY), Size = new Size(100, barH), BackColor = Color.FromArgb(229, 231, 235) };
            barFondo.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(barFondo.ClientRectangle, 4))
                    e.Graphics.FillPath(new SolidBrush(Color.FromArgb(229, 231, 235)), path);
            };

            if (progreso > 0)
            {
                var barRelleno = new Panel { Location = new Point(0, 0), BackColor = Color.Transparent };
                barRelleno.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    var r = barRelleno.ClientRectangle;
                    if (r.Width < 2) return;
                    using (var path = RoundedRect(r, 4))
                    using (var brush = new LinearGradientBrush(r, c2, c1, LinearGradientMode.Horizontal))
                        e.Graphics.FillPath(brush, path);
                };
                barFondo.Controls.Add(barRelleno);
                barFondo.Resize += (s, e) =>
                {
                    int pw = Math.Max(0, (int)(barFondo.Width * progreso / 100.0));
                    barRelleno.Size = new Size(pw, barH);
                };
            }
            card.Controls.Add(barFondo);

            // Ajustar anchos dinámicamente
            card.Resize += (s, e) =>
            {
                int w = card.Width;
                lblNombre.MaximumSize = new Size(w - 160, 0);
                lblBadge.Location     = new Point(w - lblBadge.Width - 20, 16);
                barFondo.Size         = new Size(Math.Max(40, w - 102), barH);
                lblMonto.MaximumSize  = new Size(w - 90, 0);
            };

            return card;
        }

        private void AjustarAnchoMetas()
        {
            if (_flpMetas == null) return;
            int ancho = Math.Max(200, _flpMetas.ClientSize.Width - 4);
            foreach (Control c in _flpMetas.Controls)
                c.Width = ancho;
        }

        private void AbrirFormNuevaMeta()
        {
            using (var dlg = new FormNuevaMeta())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.MetaCreada != null)
                {
                    _metas.Add(dlg.MetaCreada);
                    RenderMetas();
                    AjustarAnchoMetas();
                }
            }
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // ── FORMULARIO NUEVA META ─────────────────────────────────────────
    internal class FormNuevaMeta : Form
    {
        public ObjetivoAhorro MetaCreada { get; private set; }

        private TextBox _txtNombre;
        private TextBox _txtObjetivo;
        private TextBox _txtActual;
        private DateTimePicker _dtpFecha;
        private CheckBox _chkFecha;

        public FormNuevaMeta()
        {
            Text            = "Nueva Meta de Ahorro";
            Size            = new Size(420, 380);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            BackColor       = Color.White;
            Font            = new Font("Segoe UI", 10);

            ConstruirFormulario();
        }

        private void ConstruirFormulario()
        {
            int y = 24, labelW = 130, fieldX = 150, fieldW = 220;

            Agregar("Nombre de la meta:", ref y, labelW);
            _txtNombre = Campo(fieldX, y - 28, fieldW); Controls.Add(_txtNombre);

            Agregar("Importe objetivo (€):", ref y, labelW);
            _txtObjetivo = Campo(fieldX, y - 28, fieldW); Controls.Add(_txtObjetivo);

            Agregar("Importe actual (€):", ref y, labelW);
            _txtActual = Campo(fieldX, y - 28, fieldW, "0"); Controls.Add(_txtActual);

            Agregar("¿Tiene fecha límite?", ref y, labelW);
            _chkFecha = new CheckBox { Location = new Point(fieldX, y - 28), AutoSize = true, Cursor = Cursors.Hand };
            _chkFecha.CheckedChanged += (s, e) => _dtpFecha.Enabled = _chkFecha.Checked;
            Controls.Add(_chkFecha);

            y += 8;
            Controls.Add(new Label { Text = "Fecha límite:", Location = new Point(24, y), AutoSize = true, ForeColor = Color.FromArgb(107, 114, 128) });
            _dtpFecha = new DateTimePicker { Location = new Point(fieldX, y), Width = fieldW, Format = DateTimePickerFormat.Short, Enabled = false };
            _dtpFecha.MinDate = DateTime.Today.AddDays(1);
            Controls.Add(_dtpFecha);
            y += 36;

            // Botones
            y += 16;
            var btnOk = new Button
            {
                Text      = "Crear Meta",
                Location  = new Point(fieldX, y),
                Size      = new Size(104, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                Cursor    = Cursors.Hand
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += BtnOk_Click;

            var btnCancelar = new Button
            {
                Text      = "Cancelar",
                Location  = new Point(fieldX + 112, y),
                Size      = new Size(100, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(243, 244, 246),
                ForeColor = Color.FromArgb(55, 65, 81),
                Cursor    = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.Add(btnOk);
            Controls.Add(btnCancelar);
        }

        private void Agregar(string label, ref int y, int labelW)
        {
            Controls.Add(new Label { Text = label, Location = new Point(24, y), Width = labelW, AutoSize = false, Height = 28, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(55, 65, 81) });
            y += 36;
        }

        private TextBox Campo(int x, int y, int w, string def = "")
            => new TextBox { Location = new Point(x, y), Width = w, Height = 28, Text = def, Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtNombre.Text))
            { MessageBox.Show("Introduce el nombre de la meta.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (!decimal.TryParse(_txtObjetivo.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal objetivo) || objetivo <= 0)
            { MessageBox.Show("Introduce un importe objetivo válido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            decimal.TryParse(_txtActual.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal actual);
            actual = Math.Max(0, Math.Min(actual, objetivo));

            MetaCreada = new ObjetivoAhorro
            {
                Nombre         = _txtNombre.Text.Trim(),
                MontoObjetivo  = objetivo,
                MontoActual    = actual,
                Progreso       = objetivo > 0 ? Math.Round(actual / objetivo * 100, 1) : 0,
                FechaObjetivo  = _chkFecha.Checked ? (DateTime?)_dtpFecha.Value.Date : null
            };
            DialogResult = DialogResult.OK;
        }
    }
}
