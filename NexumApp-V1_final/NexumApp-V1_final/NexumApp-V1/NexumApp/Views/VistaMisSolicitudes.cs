using NexumApp.Forms.Becas;
using NexumApp.Models;
using NexumApp.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NexumApp.Views
{
    public class VistaMisSolicitudes : UserControl
    {
        private static readonly Color Indigo      = Color.FromArgb(99,  102, 241);
        private static readonly Color IndigoDark  = Color.FromArgb(49,  46,  129);
        private static readonly Color IndigoLight = Color.FromArgb(165, 180, 252);
        private static readonly Color BgPage      = Color.FromArgb(244, 246, 252);
        private static readonly Color White       = Color.White;
        private static readonly Color TextDark    = Color.FromArgb(17,  24,  39);
        private static readonly Color TextGray    = Color.FromArgb(107, 114, 128);
        private static readonly Color Border      = Color.FromArgb(229, 231, 235);
        private static readonly Color GreenOk     = Color.FromArgb(16,  185, 129);
        private static readonly Color Amber       = Color.FromArgb(245, 158,  11);
        private static readonly Color RedWarn     = Color.FromArgb(239,  68,  68);

        private readonly BecaSolicitudService _service = new BecaSolicitudService();
        private FlowLayoutPanel _pnlLista;
        private Panel           _scroll;

        public event EventHandler VolverAlInicio;

        public VistaMisSolicitudes()
        {
            BackColor = BgPage;
            Dock      = DockStyle.Fill;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ConstruirUI();
            CargarSolicitudes();
        }

        private void ConstruirUI()
        {
            Controls.Clear();

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2,
                BackColor = Color.Transparent
            };
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlp.Controls.Add(ConstruirHeader(), 0, 0);

            _scroll = new Panel
            {
                Dock = DockStyle.Fill, AutoScroll = true,
                BackColor = BgPage, Padding = new Padding(28, 20, 28, 20)
            };

            _pnlLista = new FlowLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown, WrapContents = false,
                BackColor = Color.Transparent
            };

            _scroll.Controls.Add(_pnlLista);
            _scroll.SizeChanged += (s, ev) => SincronizarAnchos();
            tlp.Controls.Add(_scroll, 0, 1);

            Controls.Add(tlp);
        }

        private void SincronizarAnchos()
        {
            int w = Math.Max(400, _scroll.ClientSize.Width - _scroll.Padding.Horizontal);
            _pnlLista.Width = w;
            foreach (Control c in _pnlLista.Controls)
                c.Width = w;
        }

        private Panel ConstruirHeader()
        {
            var pnl = new Panel { Dock = DockStyle.Fill };
            pnl.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new LinearGradientBrush(Point.Empty,
                    new Point(pnl.Width, pnl.Height), IndigoDark, Indigo))
                    ev.Graphics.FillRectangle(br, pnl.ClientRectangle);
                using (var br2 = new SolidBrush(Color.FromArgb(20, 255, 255, 255)))
                {
                    ev.Graphics.FillEllipse(br2, pnl.Width - 160, -50, 240, 240);
                    ev.Graphics.FillEllipse(br2, pnl.Width - 50,   40, 110, 110);
                }
            };

            bool hov = false;
            var btnVolver = new Panel
            {
                Location = new Point(16, 12), Size = new Size(80, 28),
                BackColor = Color.Transparent, Cursor = Cursors.Hand
            };
            btnVolver.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = btnVolver.ClientRectangle;
                if (hov)
                    using (var path = RRect(r, 8))
                        ev.Graphics.FillPath(new SolidBrush(Color.FromArgb(40, 255, 255, 255)), path);
                Color tc = hov ? Color.White : Color.FromArgb(200, 255, 255, 255);
                int cy = r.Height / 2;
                using (var pen = new Pen(tc, 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    ev.Graphics.DrawLine(pen, 18, cy, 10, cy);
                    ev.Graphics.DrawLines(pen, new[] { new PointF(14f, cy - 4f), new PointF(10f, cy), new PointF(14f, cy + 4f) });
                }
                TextRenderer.DrawText(ev.Graphics, "Becas",
                    new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    new Rectangle(24, 0, r.Width - 26, r.Height), tc,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            };
            btnVolver.MouseEnter += (s, ev) => { hov = true;  btnVolver.Invalidate(); };
            btnVolver.MouseLeave += (s, ev) => { hov = false; btnVolver.Invalidate(); };
            btnVolver.Click      += (s, ev) => VolverAlInicio?.Invoke(this, EventArgs.Empty);

            pnl.Controls.Add(btnVolver);
            pnl.Controls.Add(new Label
            {
                Text = "📋  Mis Solicitudes", ForeColor = White,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                AutoSize = true, BackColor = Color.Transparent, Location = new Point(36, 46)
            });
            pnl.Controls.Add(new Label
            {
                Text = "Seguimiento de tus solicitudes de beca Nexum",
                ForeColor = IndigoLight, Font = new Font("Segoe UI", 11),
                AutoSize = true, BackColor = Color.Transparent, Location = new Point(38, 92)
            });
            return pnl;
        }

        private void CargarSolicitudes()
        {
            _pnlLista.Controls.Clear();

            int uid = SesionActual.Instancia?.Usuario?.Id ?? 0;
            var lista = _service.ObtenerPorUsuario(uid);

            if (lista.Count == 0)
            {
                MostrarEstadoVacio();
            }
            else
            {
                foreach (var sol in lista)
                {
                    var card = CrearCardSolicitud(sol);
                    card.Margin = new Padding(0, 0, 0, 14);
                    _pnlLista.Controls.Add(card);
                }
            }

            BeginInvoke(new Action(SincronizarAnchos));
        }

        private void MostrarEstadoVacio()
        {
            var pnl = new Panel { Height = 240, BackColor = White };
            pnl.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RRect(new Rectangle(0, 0, pnl.Width, pnl.Height), 16))
                {
                    ev.Graphics.FillPath(new SolidBrush(White), path);
                    ev.Graphics.DrawPath(new Pen(Border, 1), path);
                }
            };

            var flp = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, BackColor = Color.Transparent,
                Padding = new Padding(0, 24, 0, 0)
            };

            flp.Controls.Add(new Label
            {
                Text = "🎓", Font = new Font("Segoe UI", 32),
                AutoSize = true, BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 8),
                Anchor = AnchorStyles.None
            });
            flp.Controls.Add(new Label
            {
                Text = "Todavía no tienes solicitudes",
                ForeColor = TextDark, Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true, BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 6)
            });
            flp.Controls.Add(new Label
            {
                Text = "Explora las becas disponibles y solicita la que mejor se adapte a ti.",
                ForeColor = TextGray, Font = new Font("Segoe UI", 10),
                AutoSize = true, BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 16)
            });

            var btn = new Button
            {
                Text = "Ver becas disponibles",
                Size = new Size(220, 40),
                BackColor = Indigo, ForeColor = White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => VolverAlInicio?.Invoke(this, EventArgs.Empty);
            flp.Controls.Add(btn);

            // Centrar el FlowLayoutPanel
            flp.SizeChanged += (s, ev) =>
            {
                foreach (Control c in flp.Controls)
                    c.Left = (flp.ClientSize.Width - c.Width) / 2;
            };

            pnl.Controls.Add(flp);
            pnl.Margin = new Padding(0);
            _pnlLista.Controls.Add(pnl);
        }

        private Panel CrearCardSolicitud(SolicitudBeca sol)
        {
            Color barColor   = sol.EsAprobada ? GreenOk : sol.EsDenegada ? RedWarn : Amber;
            Color badgeColor = barColor;
            string badgeText = sol.EsAprobada ? "✓  Aprobada" : sol.EsDenegada ? "✕  Denegada" : "⏳  Pendiente";

            bool tieneAcademico = !string.IsNullOrEmpty(sol.CentroEducativo);
            int  cardHeight     = tieneAcademico ? 138 : 110;

            var card = new Panel { Height = cardHeight, BackColor = White };
            card.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RRect(new Rectangle(0, 0, card.Width, card.Height), 14))
                {
                    ev.Graphics.FillPath(new SolidBrush(White), path);
                    ev.Graphics.DrawPath(new Pen(Border, 1), path);
                }
                using (var br = new SolidBrush(barColor))
                    ev.Graphics.FillRectangle(br, new Rectangle(0, 14, 5, card.Height - 28));
            };

            // TLP: [info flex] [derecha 210px]
            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(18, 0, 16, 0)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // ── Columna izquierda ──────────────────────────────────
            var pnlInfo = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            pnlInfo.Controls.Add(new Label
            {
                Text = sol.BecaTitulo, ForeColor = TextDark,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = false, Size = new Size(380, 22),
                BackColor = Color.Transparent, Location = new Point(0, 14)
            });
            pnlInfo.Controls.Add(new Label
            {
                Text = $"{sol.BecaImporte:N0} €", ForeColor = Indigo,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true, BackColor = Color.Transparent, Location = new Point(0, 42)
            });
            pnlInfo.Controls.Add(new Label
            {
                Text = $"Solicitada el {sol.FechaSolicitud:dd/MM/yyyy}",
                ForeColor = TextGray, Font = new Font("Segoe UI", 8.5f),
                AutoSize = true, BackColor = Color.Transparent, Location = new Point(0, 65)
            });
            if (!string.IsNullOrEmpty(sol.NumeroContrato))
            {
                pnlInfo.Controls.Add(new Label
                {
                    Text = $"Nº {sol.NumeroContrato}",
                    ForeColor = Color.FromArgb(156, 163, 175), Font = new Font("Segoe UI", 8),
                    AutoSize = true, BackColor = Color.Transparent, Location = new Point(0, 84)
                });
            }

            // Datos académicos si existen
            if (tieneAcademico)
            {
                string partes = sol.CentroEducativo;
                if (!string.IsNullOrEmpty(sol.Titulacion))    partes += $"  ·  {sol.Titulacion}";
                if (!string.IsNullOrEmpty(sol.AnioAcademico)) partes += $"  ·  {sol.AnioAcademico}";

                pnlInfo.Controls.Add(new Panel
                {
                    Location  = new Point(0, 102), Size = new Size(350, 1),
                    BackColor = Border
                });
                pnlInfo.Controls.Add(new Label
                {
                    Text         = $"📚  {partes}",
                    ForeColor    = TextGray, Font = new Font("Segoe UI", 8),
                    AutoSize     = false, Size = new Size(350, 18),
                    BackColor    = Color.Transparent, Location = new Point(0, 108),
                    AutoEllipsis = true
                });
            }

            // ── Columna derecha ────────────────────────────────────
            var pnlRight = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            var lblBadge = new Label
            {
                Text = badgeText, ForeColor = White, BackColor = badgeColor,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = true, Padding = new Padding(10, 4, 10, 4),
                Location = new Point(10, 16)
            };
            pnlRight.Controls.Add(lblBadge);

            if (sol.EsAprobada && !string.IsNullOrEmpty(sol.NumeroContrato))
            {
                var btnContrato = new Button
                {
                    Text = "📄  Ver Contrato",
                    Size = new Size(168, 34), Location = new Point(10, 58),
                    BackColor = Indigo, ForeColor = White, FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
                };
                btnContrato.FlatAppearance.BorderSize = 0;
                var solCapturada = sol;
                btnContrato.Click += (s, e) => AbrirContrato(solCapturada);
                pnlRight.Controls.Add(btnContrato);
            }

            tlp.Controls.Add(pnlInfo,  0, 0);
            tlp.Controls.Add(pnlRight, 1, 0);
            card.Controls.Add(tlp);

            return card;
        }

        private void AbrirContrato(SolicitudBeca sol)
        {
            using (var frm = new FrmContratoBeca(sol))
                frm.ShowDialog(this.FindForm());
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
