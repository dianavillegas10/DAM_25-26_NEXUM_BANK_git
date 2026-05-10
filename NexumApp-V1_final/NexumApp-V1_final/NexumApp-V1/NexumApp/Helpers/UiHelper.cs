using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;

namespace NexumApp.Helpers
{
    public static class UiHelper
    {
        public static GraphicsPath CrearRoundedRect(Rectangle r, int radius)
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

        public static void DibujarFondoGradiente(Graphics g, Rectangle r, Color c1, Color c2, bool horizontal = true)
        {
            using (var brush = new LinearGradientBrush(r, c1, c2, horizontal ? 0f : 90f))
                g.FillRectangle(brush, r);
        }

        public static Image CargarImagen(string nombreArchivo)
        {
            var path = System.IO.Path.Combine(Application.StartupPath, "Resources", nombreArchivo);
            if (System.IO.File.Exists(path))
                return Image.FromFile(path);
            return null;
        }

        public static void GenerarLogoNexum(string rutaDestino)
        {
            const int W = 480, H = 500;
            using (var bmp = new Bitmap(W, H))
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode      = SmoothingMode.AntiAlias;
                g.TextRenderingHint  = TextRenderingHint.AntiAliasGridFit;
                g.InterpolationMode  = InterpolationMode.HighQualityBicubic;
                g.CompositingQuality = CompositingQuality.HighQuality;

                // Fondo degradado oscuro
                using (var bg = new LinearGradientBrush(new Rectangle(0, 0, W, H),
                    Color.FromArgb(13, 11, 35), Color.FromArgb(22, 20, 54), 150f))
                    g.FillRectangle(bg, 0, 0, W, H);

                int cx = W / 2;

                // Halo difuso detrás del icono
                using (var gp = new GraphicsPath())
                {
                    gp.AddEllipse(cx - 155, 65, 310, 310);
                    using (var pgb = new PathGradientBrush(gp))
                    {
                        pgb.CenterColor    = Color.FromArgb(60, 99, 102, 241);
                        pgb.SurroundColors = new[] { Color.Transparent };
                        g.FillPath(pgb, gp);
                    }
                }

                // Cuadrado redondeado del icono
                const int SZ = 172;
                int ix = cx - SZ / 2, iy = 78;
                var iconRect = new Rectangle(ix, iy, SZ, SZ);
                using (var ip = CrearRoundedRect(iconRect, 38))
                using (var ib = new LinearGradientBrush(iconRect,
                    Color.FromArgb(79, 70, 229), Color.FromArgb(139, 92, 246), 135f))
                {
                    g.FillPath(ib, ip);
                    using (var shine = new SolidBrush(Color.FromArgb(38, 255, 255, 255)))
                        g.FillEllipse(shine, ix - 12, iy - 12, SZ * 0.6f, SZ * 0.6f);
                    using (var border = new Pen(Color.FromArgb(90, 167, 139, 250), 1.8f))
                        g.DrawPath(border, ip);
                }

                // Letra "N" con sombra
                using (var fN = new Font("Segoe UI", 90, FontStyle.Bold, GraphicsUnit.Pixel))
                {
                    var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    using (var shadow = new SolidBrush(Color.FromArgb(70, 0, 0, 0)))
                        g.DrawString("N", fN, shadow, new RectangleF(ix + 2, iy + 4, SZ, SZ), fmt);
                    g.DrawString("N", fN, Brushes.White, new RectangleF(ix, iy, SZ, SZ), fmt);
                }

                float ty = iy + SZ + 24;

                // "NEXUM"
                using (var fNexum = new Font("Segoe UI", 56, FontStyle.Bold, GraphicsUnit.Pixel))
                {
                    var sz = g.MeasureString("NEXUM", fNexum);
                    g.DrawString("NEXUM", fNexum, Brushes.White, new PointF(cx - sz.Width / 2, ty));
                    ty += sz.Height + 2;
                }

                // "BANK"
                using (var fBank = new Font("Segoe UI", 27, FontStyle.Bold, GraphicsUnit.Pixel))
                using (var ab = new SolidBrush(Color.FromArgb(167, 139, 250)))
                {
                    var sz = g.MeasureString("BANK", fBank);
                    g.DrawString("BANK", fBank, ab, new PointF(cx - sz.Width / 2, ty));
                    ty += sz.Height + 14;
                }

                // Línea separadora
                using (var lp = new Pen(Color.FromArgb(55, 99, 102, 241), 1f))
                    g.DrawLine(lp, cx - 70, ty, cx + 70, ty);
                ty += 13;

                // Tagline
                using (var fTag = new Font("Segoe UI", 15, FontStyle.Regular, GraphicsUnit.Pixel))
                using (var tb = new SolidBrush(Color.FromArgb(90, 110, 135)))
                {
                    var sz = g.MeasureString("Tu banco digital de confianza", fTag);
                    g.DrawString("Tu banco digital de confianza", fTag, tb,
                        new PointF(cx - sz.Width / 2, ty));
                }

                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(rutaDestino));
                bmp.Save(rutaDestino, ImageFormat.Png);
            }
        }
    }
}
