using System;
using System.Collections.Generic;

namespace NexumApp.Models
{
    /// <summary>
    /// Modelo que representa un usuario del sistema NexumApp.
    /// Corresponde a la tabla 'Usuarios' en la base de datos MySQL.
    /// </summary>
    /// <remarks>
    /// Este modelo es central en la aplicación:
    /// - Se usa para autenticación (Login/Registro)
    /// - Determina el tipo de dashboard a mostrar (Admin/Usuario)
    /// - Mantiene la relación con las cuentas bancarias
    /// </remarks>
    public class Usuario
    {
        /// <summary>
        /// Identificador único del usuario (clave primaria auto-incremental).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Apellidos del usuario.
        /// </summary>
        public string Apellidos { get; set; }

        /// <summary>
        /// Email del usuario. Sirve como identificador único para el login.
        /// Debe ser único en la base de datos.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Hash BCrypt de la contraseña del usuario.
        /// NUNCA se almacena la contraseña en texto plano.
        /// </summary>
        /// <remarks>
        /// El hash incluye: versión del algoritmo + cost factor + salt + hash resultante.
        /// Ejemplo: $2a$11$N9qo8uLOickgx2ZMRZoMye...
        /// </remarks>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Teléfono de contacto del usuario (opcional).
        /// </summary>
        public string Telefono { get; set; }

        /// <summary>
        /// DNI o documento de identidad del usuario (opcional).
        /// </summary>
        public string DNI { get; set; }

        /// <summary>
        /// Dirección postal del usuario (opcional).
        /// </summary>
        public string Direccion { get; set; }

        /// <summary>
        /// Ciudad del usuario (opcional).
        /// </summary>
        public string Ciudad { get; set; }

        /// <summary>
        /// Código postal del usuario (opcional).
        /// </summary>
        public string CodigoPostal { get; set; }

        /// <summary>
        /// Fecha de nacimiento del usuario (opcional).
        /// </summary>
        public System.DateTime? FechaNacimiento { get; set; }

        /// <summary>
        /// Edad calculada a partir de FechaNacimiento. Devuelve 0 si no está definida.
        /// </summary>
        public int Edad
        {
            get
            {
                if (!FechaNacimiento.HasValue) return 0;
                var today = DateTime.Today;
                var age = today.Year - FechaNacimiento.Value.Year;
                if (FechaNacimiento.Value.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        /// <summary>
        /// Fecha y hora en que el usuario se registró en el sistema.
        /// </summary>
        public System.DateTime FechaRegistro { get; set; }

        /// <summary>
        /// Fecha y hora del último acceso exitoso.
        /// Null si nunca ha accedido después del registro.
        /// </summary>
        public System.DateTime? UltimoAcceso { get; set; }

        /// <summary>
        /// Indica si el usuario tiene permisos de administrador.
        /// Determina qué dashboard se muestra después del login.
        /// </summary>
        /// <value>
        /// True: Accede a FrmDashboardAdmin (gestión de usuarios, reportes, etc.)
        /// False: Accede a FrmDashboardUsuario (cuentas personales, transferencias, etc.)
        /// </value>
        public bool EsAdmin { get; set; }

        /// <summary>
        /// Indica si la cuenta del usuario está activa.
        /// </summary>
        /// <value>
        /// True: Puede iniciar sesión normalmente.
        /// False: El login será rechazado con ResultadoLogin.UsuarioInactivo.
        /// </value>
        public bool Activo { get; set; }

        /// <summary>
        /// Propiedad calculada que devuelve el nombre completo del usuario.
        /// </summary>
        /// <remarks>
        /// Útil para mostrar en la UI sin concatenar manualmente.
        /// Ejemplo: "¡Bienvenido, {usuario.NombreCompleto}!"
        /// </remarks>
        public string NombreCompleto => $"{Nombre} {Apellidos}";

        /// <summary>
        /// Ruta local de la foto de perfil del usuario (opcional).
        /// Se guarda en %AppData%\NexumApp\Fotos\ al actualizarla.
        /// </summary>
        public string FotoPerfil { get; set; }

        /// <summary>
        /// Colección de cuentas bancarias asociadas al usuario.
        /// Relación uno-a-muchos con la tabla CuentasBancarias.
        /// </summary>
        /// <remarks>
        /// Virtual permite la carga diferida (lazy loading) cuando se usa Entity Framework.
        /// </remarks>
        public virtual ICollection<CuentaBancaria> Cuentas { get; set; }
    }
}
