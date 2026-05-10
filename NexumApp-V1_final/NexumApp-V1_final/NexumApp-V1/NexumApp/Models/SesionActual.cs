using System;

namespace NexumApp.Models
{
    /// <summary>
    /// Clase Singleton que gestiona la sesión del usuario actualmente logueado.
    /// Proporciona acceso global al usuario autenticado desde cualquier parte de la aplicación.
    /// </summary>
    /// <remarks>
    /// Implementa el patrón Singleton con doble verificación de bloqueo (double-check locking)
    /// para garantizar thread-safety en entornos multihilo.
    /// 
    /// Uso típico:
    /// - Después del login: SesionActual.Instancia.IniciarSesion(usuario)
    /// - Para obtener datos: SesionActual.Instancia.Usuario.Nombre
    /// - Para verificar estado: SesionActual.Instancia.EstaLogeado
    /// - Al cerrar sesión: SesionActual.Instancia.CerrarSesion()
    /// </remarks>
    public class SesionActual
    {
        /// <summary>
        /// Instancia única de la clase (patrón Singleton).
        /// </summary>
        private static SesionActual _instancia;

        /// <summary>
        /// Objeto de bloqueo para garantizar thread-safety.
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// Usuario actualmente logueado en la aplicación.
        /// Null si no hay sesión activa.
        /// </summary>
        public Usuario Usuario { get; private set; }

        /// <summary>
        /// Configuración de preferencias del usuario (implementación Diana).
        /// </summary>
        public ConfiguracionUsuario Configuracion { get; set; }

        /// <summary>
        /// Indica si hay un usuario con sesión activa.
        /// </summary>
        /// <value>True si hay usuario logueado, False en caso contrario.</value>
        public bool EstaLogeado => Usuario != null;

        /// <summary>
        /// Constructor privado para evitar instanciación externa.
        /// Solo se puede acceder a través de la propiedad Instancia.
        /// </summary>
        private SesionActual() { }

        /// <summary>
        /// Obtiene la instancia única de SesionActual (Singleton).
        /// </summary>
        /// <remarks>
        /// Utiliza double-check locking:
        /// 1. Primera verificación sin bloqueo (rendimiento)
        /// 2. Si es null, bloquea y verifica de nuevo (seguridad)
        /// 3. Crea la instancia solo si realmente es necesario
        /// </remarks>
        public static SesionActual Instancia
        {
            get
            {
                // Primera verificación sin bloqueo (mejora rendimiento)
                if (_instancia == null)
                {
                    // Bloqueo para evitar que múltiples hilos creen instancias
                    lock (_lock)
                    {
                        // Segunda verificación dentro del bloqueo
                        if (_instancia == null)
                            _instancia = new SesionActual();
                    }
                }
                return _instancia;
            }
        }

        /// <summary>
        /// Inicia la sesión guardando el usuario autenticado.
        /// </summary>
        /// <param name="usuario">Usuario que ha iniciado sesión correctamente.</param>
        /// <remarks>
        /// Debe llamarse después de un login exitoso en AuthService.
        /// El usuario quedará disponible globalmente hasta que se llame a CerrarSesion.
        /// </remarks>
        public void IniciarSesion(Usuario usuario)
        {
            Usuario = usuario;
        }

        /// <summary>
        /// Cierra la sesión eliminando la referencia al usuario.
        /// </summary>
        /// <remarks>
        /// Después de llamar a este método:
        /// - Usuario será null
        /// - EstaLogeado devolverá false
        /// - La aplicación debe redirigir al formulario de login
        /// </remarks>
        public void CerrarSesion()
        {
            Usuario = null;
            Configuracion = null;
        }

        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool EsAdmin { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
    }
}
