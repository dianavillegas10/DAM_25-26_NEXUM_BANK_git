using System;
using System.Windows.Forms;

namespace NexumApp
{
    /// <summary>
    /// Clase principal de la aplicación NexumApp.
    /// Contiene el punto de entrada y la configuración inicial.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación NexumApp.
        /// Configura los estilos visuales e inicia el formulario de login.
        /// </summary>
        /// <remarks>
        /// Flujo de inicio:
        /// 1. Habilita estilos visuales modernos de Windows
        /// 2. Configura el renderizado de texto compatible
        /// 3. Ejecuta el bucle de mensajes con FrmLogin como formulario inicial
        /// 
        /// Desde FrmLogin, el usuario puede:
        /// - Iniciar sesión (redirige a FrmDashboardAdmin o FrmDashboardUsuario)
        /// - Crear una cuenta nueva (abre FrmRegistro)
        /// 
        /// Requisitos previos:
        /// - Conexión a base de datos remota nexum_db (AlwaysData)
        /// - Cadena de conexión configurada en App.config (NexumDB)
        /// </remarks>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Genera la imagen del logo si aún no existe
            try
            {
                string logoPath = System.IO.Path.Combine(
                    Application.StartupPath, "Resources", "nexum_logo.png");
                if (!System.IO.File.Exists(logoPath))
                    NexumApp.Helpers.UiHelper.GenerarLogoNexum(logoPath);
            }
            catch { }

            Application.Run(new NexumApp.Forms.Auth.FrmLogin());
        }
    }
}
