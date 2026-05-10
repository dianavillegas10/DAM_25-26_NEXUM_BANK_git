using System.Drawing;

namespace NexumApp.Helpers
{
    /// <summary>
    /// Constantes de layout del dashboard.
    /// Los colores dinámicos se adaptan automáticamente al tema activo (AppSettings.ModoOscuro).
    /// </summary>
    public static class DashboardLayout
    {
        public const int Margen = 20;
        public const int PaddingCard = 16;
        public const int BorderRadius = 16;

        public const int SidebarWidth = 240;
        public const int SidebarRightWidth = 300;

        // ── Colores dinámicos (se recalculan en cada acceso según el tema) ─────
        public static Color FondoPrincipal => AppSettings.ModoOscuro
            ? Color.FromArgb(10,  12,  28)
            : Color.FromArgb(244, 247, 254);

        public static Color CardFondo => AppSettings.ModoOscuro
            ? Color.FromArgb(18, 22, 46)
            : Color.White;

        public static Color TextoOscuro => AppSettings.ModoOscuro
            ? Color.FromArgb(241, 245, 249)
            : Color.FromArgb(31, 41, 55);

        public static Color TextoGris => AppSettings.ModoOscuro
            ? Color.FromArgb(100, 116, 139)
            : Color.FromArgb(107, 114, 128);

        /// <summary>Color de borde para cards y paneles.</summary>
        public static Color BorderCard => AppSettings.ModoOscuro
            ? Color.FromArgb(38, 44, 80)
            : Color.FromArgb(229, 231, 235);

        /// <summary>Fondo para estado hover de items.</summary>
        public static Color BgHover => AppSettings.ModoOscuro
            ? Color.FromArgb(22, 27, 54)
            : Color.FromArgb(250, 251, 253);

        /// <summary>Fondo para estado pressed de items.</summary>
        public static Color BgPressed => AppSettings.ModoOscuro
            ? Color.FromArgb(26, 31, 60)
            : Color.FromArgb(240, 242, 248);

        /// <summary>Color de líneas divisorias dentro de cards.</summary>
        public static Color DividerColor => AppSettings.ModoOscuro
            ? Color.FromArgb(30, 35, 65)
            : Color.FromArgb(241, 243, 246);

        // ── Colores fijos (iguales en ambos temas) ────────────────────────────
        public static readonly Color SidebarFondo    = Color.FromArgb(26, 31, 61);
        public static readonly Color AzulPrimario    = Color.FromArgb(99, 102, 241);
        public static readonly Color GradienteInicio = Color.FromArgb(99, 102, 241);
        public static readonly Color GradienteFin    = Color.FromArgb(139, 92, 246);
    }
}
