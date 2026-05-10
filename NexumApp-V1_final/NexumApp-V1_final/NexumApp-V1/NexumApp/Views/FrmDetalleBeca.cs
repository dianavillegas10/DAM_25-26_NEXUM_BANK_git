using NexumApp.Forms.Becas;
using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NexumApp.Views
{
    /// <summary>
    /// Formulario de detalle y solicitud de una beca. Estilo modal bancario moderno.
    /// </summary>
    internal class FrmDetalleBeca : Form
    {
        private static readonly Color Indigo     = Color.FromArgb(99, 102, 241);
        private static readonly Color IndigoDark = Color.FromArgb(49, 46, 129);
        private static readonly Color White      = Color.White;
        private static readonly Color BgGray     = Color.FromArgb(244, 246, 252);
        private static readonly Color TextDark   = Color.FromArgb(17, 24, 39);
        private static readonly Color TextGray   = Color.FromArgb(107, 114, 128);
        private static readonly Color GreenOk    = Color.FromArgb(16, 185, 129);
        private static readonly Color Border     = Color.FromArgb(229, 231, 235);

        private readonly Beca                 _beca;
        private readonly BecaSolicitudService _solicitudService = new BecaSolicitudService();
        private bool _solicitado = false;

        public FrmDetalleBeca(Beca beca)
        {
            _beca = beca;
            InitForm();
            ConstruirUI();
        }

        private void InitForm()
        {
            this.Text = _beca.Titulo + " — Nexum Bank";
            this.Size = new Size(560, 660);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = BgGray;
            this.MaximizeBox = false;

            // Bordes redondeados en el formulario
            this.Load += (s, e) =>
            {
                if (Width > 0 && Height > 0)
                {
                    var path = new GraphicsPath();
                    int r = 20;
                    path.AddArc(0, 0, r, r, 180, 90);
                    path.AddArc(Width - r, 0, r, r, 270, 90);
                    path.AddArc(Width - r, Height - r, r, r, 0, 90);
                    path.AddArc(0, Height - r, r, r, 90, 90);
                    path.CloseFigure();
                    this.Region = new Region(path);
                }
            };
        }

        private void ConstruirUI()
        {
            // ── Header degradado ──────────────────────────────────
            var pnlHeader = new Panel
            {
                Size = new Size(560, 120),
                Location = new Point(0, 0)
            };
            pnlHeader.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new LinearGradientBrush(
                    new Point(0, 0), new Point(pnlHeader.Width, pnlHeader.Height),
                    IndigoDark, Indigo))
                    g.FillRectangle(br, pnlHeader.ClientRectangle);

                using (var br2 = new SolidBrush(Color.FromArgb(20, 255, 255, 255)))
                {
                    g.FillEllipse(br2, pnlHeader.Width - 110, -30, 180, 180);
                    g.FillEllipse(br2, pnlHeader.Width - 30, 40, 90, 90);
                }
            };

            // Botón cerrar
            var btnCerrar = new Button
            {
                Text = "✕",
                Size = new Size(32, 32),
                Location = new Point(516, 12),
                BackColor = Color.FromArgb(60, 255, 255, 255),
                ForeColor = White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Click += (s, e) => this.Close();

            // Badge categoría
            var lblCat = new Label
            {
                Text = "  " + TextoCategoria(_beca.Categoria) + "  ",
                BackColor = Color.FromArgb(50, 255, 255, 255),
                ForeColor = White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(6, 3, 6, 3),
                Location = new Point(24, 18),
                Cursor = Cursors.Default
            };

            var lblTit = new Label
            {
                Text = _beca.Titulo,
                ForeColor = White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Size = new Size(460, 42),
                BackColor = Color.Transparent,
                Location = new Point(24, 48)
            };

            var lblEnt = new Label
            {
                Text = _beca.EntidadConvocante,
                ForeColor = Color.FromArgb(165, 180, 252),
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(26, 92)
            };

            pnlHeader.Controls.Add(btnCerrar);
            pnlHeader.Controls.Add(lblCat);
            pnlHeader.Controls.Add(lblTit);
            pnlHeader.Controls.Add(lblEnt);

            // ── Cuerpo ─────────────────────────────────────────────
            int y = 136;

            // Fila de 3 métricas
            var pnlMetricas = new Panel
            {
                Location = new Point(24, y),
                Size = new Size(508, 74),
                BackColor = White
            };
            pnlMetricas.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundRect(new Rectangle(0, 0, pnlMetricas.Width, pnlMetricas.Height), 12))
                using (var br = new SolidBrush(White))
                    ev.Graphics.FillPath(br, path);
                using (var pen = new Pen(Border, 1))
                    using (var path = RoundRect(new Rectangle(0, 0, pnlMetricas.Width, pnlMetricas.Height), 12))
                        ev.Graphics.DrawPath(pen, path);
            };
            AgregarMetrica(pnlMetricas, 0,   "Importe",  $"{_beca.Importe:N0} €", Indigo);
            AgregarMetrica(pnlMetricas, 170, "Duración", _beca.DuracionTexto,     TextDark);
            AgregarMetrica(pnlMetricas, 340, "Plazas",   $"{_beca.PlazasDisponibles}", GreenOk);

            y += 88;

            // Descripción
            y = AgregarSeccion("Descripción", _beca.Descripcion, y);

            // Requisitos
            y = AgregarSeccion("Requisitos", _beca.Requisitos, y);

            // Fecha cierre
            string fechaTxt = _beca.FechaCierre.HasValue
                ? _beca.FechaCierre.Value.ToString("dd 'de' MMMM 'de' yyyy",
                    new System.Globalization.CultureInfo("es-ES"))
                : "Sin fecha límite";
            y = AgregarSeccion("Fecha límite de solicitud", fechaTxt, y);

            // ── Botón solicitar ───────────────────────────────────
            var btnSol = new Button
            {
                Text = "SOLICITAR BECA",
                Size = new Size(508, 50),
                Location = new Point(24, 548),
                BackColor = Indigo,
                ForeColor = White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSol.FlatAppearance.BorderSize = 0;
            btnSol.Click += (s, e) => OnSolicitar(btnSol);

            var lblAvis = new Label
            {
                Text = "Al solicitar aceptas las bases de la convocatoria Nexum Bank",
                ForeColor = TextGray,
                Font = new Font("Segoe UI", 8),
                AutoSize = false,
                Size = new Size(508, 18),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Location = new Point(24, 604)
            };

            Controls.Add(pnlHeader);
            Controls.Add(pnlMetricas);
            Controls.Add(btnSol);
            Controls.Add(lblAvis);
        }

        private void AgregarMetrica(Panel parent, int x, string titulo, string valor, Color valorColor)
        {
            parent.Controls.Add(new Label
            {
                Text = titulo,
                ForeColor = TextGray,
                Font = new Font("Segoe UI", 8),
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(x + 16, 12)
            });
            parent.Controls.Add(new Label
            {
                Text = valor,
                ForeColor = valorColor,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(x + 16, 32)
            });
        }

        private int AgregarSeccion(string titulo, string contenido, int y)
        {
            var lblTit = new Label
            {
                Text = titulo.ToUpper(),
                ForeColor = TextGray,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(24, y)
            };
            y += 22;

            var lblCont = new Label
            {
                Text = contenido,
                ForeColor = TextDark,
                Font = new Font("Segoe UI", 10),
                Size = new Size(508, 0),
                AutoSize = false,
                BackColor = Color.Transparent,
                Location = new Point(24, y)
            };
            lblCont.Height = lblCont.GetPreferredSize(new Size(508, 0)).Height + 4;
            y += lblCont.Height + 16;

            Controls.Add(lblTit);
            Controls.Add(lblCont);
            return y;
        }

        private void OnSolicitar(Button btn)
        {
            if (_solicitado) return;

            int uid = SesionActual.Instancia?.Usuario?.Id ?? 0;
            if (_solicitudService.YaSolicito(uid, _beca.Id))
            {
                MessageBox.Show("Ya tienes una solicitud enviada para esta beca.\n\nConsulta el estado en «Mis Solicitudes».",
                    "Solicitud existente", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btn.Text      = "✓  YA SOLICITADA";
                btn.BackColor = GreenOk;
                btn.Enabled   = false;
                return;
            }

            using (var frm = new FrmSolicitarBeca(_beca))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _solicitado   = true;
                    btn.Text      = "✓  SOLICITUD ENVIADA";
                    btn.BackColor = GreenOk;
                    btn.Enabled   = false;

                    MessageBox.Show(
                        $"¡Beca concedida!\n\n" +
                        $"Tu contrato: {frm.NumeroContrato}\n\n" +
                        "Puedes ver y descargar el contrato desde «Mis Solicitudes».",
                        "¡Enhorabuena!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private static string TextoCategoria(CategoriaBeca cat)
        {
            switch (cat)
            {
                case CategoriaBeca.Universitaria: return "Universitaria";
                case CategoriaBeca.Posgrado:      return "Posgrado";
                case CategoriaBeca.FP:            return "FP / CFGS";
                case CategoriaBeca.Digital:       return "Digital";
                case CategoriaBeca.Deportiva:     return "Deportiva";
                case CategoriaBeca.Arte:          return "Arte";
                default:                          return "General";
            }
        }

        private static GraphicsPath RoundRect(Rectangle r, int radius)
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
}
