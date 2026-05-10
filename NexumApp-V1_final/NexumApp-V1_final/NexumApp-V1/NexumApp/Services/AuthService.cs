using MySql.Data.MySqlClient;
using NexumApp.Models;
using System;
using System.Configuration;
using System.Windows.Forms; // 👈 Para MessageBox

namespace NexumApp.Services
{
    /// <summary>
    /// Enumeración que representa los posibles resultados de un intento de login.
    /// Permite al formulario mostrar mensajes de error específicos según cada caso.
    /// </summary>
    public enum ResultadoLogin
    {
        /// <summary>Login exitoso, el usuario ha sido autenticado correctamente.</summary>
        Exitoso,
        /// <summary>El email introducido no existe en la base de datos.</summary>
        UsuarioNoExiste,
        /// <summary>El email existe pero la contraseña no coincide.</summary>
        ContrasenaIncorrecta,
        /// <summary>El usuario existe pero su cuenta está desactivada.</summary>
        UsuarioInactivo,
        /// <summary>No se pudo conectar con la base de datos MySQL.</summary>
        ErrorConexion
    }

    /// <summary>
    /// Servicio de autenticación que gestiona el login, registro y validación de usuarios.
    /// Utiliza conexión directa a MySQL mediante MySql.Data y BCrypt para el hash de contraseñas.
    /// </summary>
    public class AuthService
    {
        /// <summary>
        /// Obtiene la cadena de conexión desde App.config.
        /// </summary>
        private static string ConnectionString
        {
            get
            {
                var cs = ConfigurationManager.ConnectionStrings["NexumDB"];
                if (cs == null || string.IsNullOrEmpty(cs.ConnectionString))
                    throw new InvalidOperationException("La cadena de conexión 'NexumDB' no está configurada en App.config.");
                return cs.ConnectionString;
            }
        }

        /// <summary>
        /// Intenta autenticar un usuario contra la base de datos MySQL.
        /// </summary>
        public (Usuario Usuario, ResultadoLogin Resultado) Login(string email, string password)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();

                    // 👇 CORREGIDO: usuarios en minúsculas
                    string query = "SELECT Id, Nombre, Apellidos, Email, PasswordHash, EsAdmin, Activo, Telefono, Direccion, FechaRegistro FROM usuarios WHERE Email = @email";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);

                        Usuario usuario = null;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool activo = reader.GetBoolean("Activo");
                                if (!activo)
                                    return (null, ResultadoLogin.UsuarioInactivo);

                                string passwordHash = reader.GetString("PasswordHash");
                                if (BCrypt.Net.BCrypt.Verify(password, passwordHash))
                                {
                                    usuario = new Usuario
                                    {
                                        Id = reader.GetInt32("Id"),
                                        Nombre = reader.GetString("Nombre"),
                                        Apellidos = reader.GetString("Apellidos"),
                                        Email = reader.GetString("Email"),
                                        PasswordHash = passwordHash,
                                        EsAdmin = reader.GetBoolean("EsAdmin"),
                                        Activo = activo,
                                        Telefono = reader["Telefono"] == DBNull.Value ? null : reader.GetString("Telefono"),
                                        Direccion = reader["Direccion"] == DBNull.Value ? null : reader.GetString("Direccion"),
                                        FechaRegistro = reader["FechaRegistro"] == DBNull.Value ? DateTime.Now : reader.GetDateTime("FechaRegistro")
                                    };
                                }
                                else
                                    return (null, ResultadoLogin.ContrasenaIncorrecta);
                            }
                            else
                                return (null, ResultadoLogin.UsuarioNoExiste);
                        }
                        if (usuario != null)
                        {
                            CargarCamposPerfilOpcionales(conn, usuario);
                            ActualizarUltimoAcceso(conn, usuario.Id);
                            return (usuario, ResultadoLogin.Exitoso);
                        }
                    }
                }
                return (null, ResultadoLogin.ErrorConexion);
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error MySQL en login: {ex.Message}");
                MessageBox.Show($"Error de MySQL: {ex.Message}\n\nNúmero de error: {ex.Number}", "Error de conexión");
                return (null, ResultadoLogin.ErrorConexion);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error general en login: {ex.Message}");
                MessageBox.Show($"Error general: {ex.Message}\n\nTipo: {ex.GetType().Name}", "Error inesperado");
                return (null, ResultadoLogin.ErrorConexion);
            }
        }

        /// <summary>
        /// Carga DNI, Ciudad, CodigoPostal, FechaNacimiento si las columnas existen.
        /// </summary>
        private void CargarCamposPerfilOpcionales(MySqlConnection conn, Usuario usuario)
        {
            try
            {
                // 👇 CORREGIDO: usuarios en minúsculas
                string query = "SELECT DNI, Ciudad, CodigoPostal, FechaNacimiento, UltimoAcceso, FotoPerfil FROM usuarios WHERE Id=@id";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", usuario.Id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario.DNI = reader["DNI"] == DBNull.Value ? null : reader.GetString("DNI");
                            usuario.Ciudad = reader["Ciudad"] == DBNull.Value ? null : reader.GetString("Ciudad");
                            usuario.CodigoPostal = reader["CodigoPostal"] == DBNull.Value ? null : reader.GetString("CodigoPostal");
                            usuario.FechaNacimiento = reader["FechaNacimiento"] == DBNull.Value ? (DateTime?)null : reader.GetDateTime("FechaNacimiento");
                            usuario.UltimoAcceso = reader["UltimoAcceso"] == DBNull.Value ? (DateTime?)null : reader.GetDateTime("UltimoAcceso");
                            usuario.FotoPerfil   = reader["FotoPerfil"]   == DBNull.Value ? null : reader.GetString("FotoPerfil");
                        }
                    }
                }
            }
            catch { /* Columnas no existen aún; ignorar */ }
        }

        /// <summary>Guarda la ruta local de la foto de perfil en la BD.</summary>
        public bool ActualizarFotoPerfil(int userId, string rutaFoto)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("UPDATE usuarios SET FotoPerfil=@foto WHERE Id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@foto", (object)rutaFoto ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@id", userId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch { return false; }
        }

        /// <summary>
        /// Guarda la fecha/hora del login actual en la columna UltimoAcceso.
        /// </summary>
        private void ActualizarUltimoAcceso(MySqlConnection conn, int usuarioId)
        {
            try
            {
                using (var cmd = new MySqlCommand(
                    "UPDATE usuarios SET UltimoAcceso = NOW() WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", usuarioId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch { /* Ignorar si falla — no es bloqueante */ }
        }

        /// <summary>
        /// Registra un nuevo usuario en la base de datos.
        /// </summary>
        public bool RegistrarUsuario(string nombre, string apellidos, string email, string password, bool esAdmin = false)
        {
            try
            {
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();

                    // 👇 CORREGIDO: usuarios en minúsculas
                    string query = @"INSERT INTO usuarios (Nombre, Apellidos, Email, PasswordHash, EsAdmin, Activo, FechaRegistro) 
                                     VALUES (@nombre, @apellidos, @email, @passwordHash, @esAdmin, 1, NOW())";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nombre", nombre);
                        cmd.Parameters.AddWithValue("@apellidos", apellidos);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@passwordHash", passwordHash);
                        cmd.Parameters.AddWithValue("@esAdmin", esAdmin);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en registro: {ex.Message}");
                MessageBox.Show($"Error en registro: {ex.Message}", "Error");
                return false;
            }
        }

        /// <summary>
        /// Obtiene todos los usuarios registrados en el sistema.
        /// </summary>
        public System.Collections.Generic.List<Usuario> ObtenerTodosUsuarios()
        {
            var lista = new System.Collections.Generic.List<Usuario>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    // 👇 CORREGIDO: usuarios en minúsculas
                    string query = "SELECT Id, Nombre, Apellidos, Email, EsAdmin, Activo FROM usuarios ORDER BY Id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Usuario
                            {
                                Id = reader.GetInt32("Id"),
                                Nombre = reader.GetString("Nombre"),
                                Apellidos = reader.GetString("Apellidos"),
                                Email = reader.GetString("Email"),
                                EsAdmin = reader.GetBoolean("EsAdmin"),
                                Activo = reader.GetBoolean("Activo")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener usuarios: {ex.Message}");
                MessageBox.Show($"Error al obtener usuarios: {ex.Message}", "Error");
            }
            return lista;
        }

        /// <summary>
        /// Actualiza los datos de un usuario existente.
        /// </summary>
        public bool ActualizarUsuario(int id, string nombre, string apellidos, string email, bool esAdmin, bool activo)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    // 👇 CORREGIDO: usuarios en minúsculas
                    string query = @"UPDATE usuarios SET Nombre=@nombre, Apellidos=@apellidos, Email=@email, EsAdmin=@esAdmin, Activo=@activo WHERE Id=@id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nombre", nombre);
                        cmd.Parameters.AddWithValue("@apellidos", apellidos);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@esAdmin", esAdmin);
                        cmd.Parameters.AddWithValue("@activo", activo);
                        cmd.Parameters.AddWithValue("@id", id);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al actualizar usuario: {ex.Message}");
                MessageBox.Show($"Error al actualizar usuario: {ex.Message}", "Error");
                return false;
            }
        }

        /// <summary>
        /// Cambia el estado activo/inactivo (banear/desbanear) de un usuario.
        /// </summary>
        public bool CambiarEstadoUsuario(int id, bool activo)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    // 👇 CORREGIDO: usuarios en minúsculas
                    string query = "UPDATE usuarios SET Activo=@activo WHERE Id=@id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@activo", activo);
                        cmd.Parameters.AddWithValue("@id", id);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cambiar estado: {ex.Message}");
                MessageBox.Show($"Error al cambiar estado: {ex.Message}", "Error");
                return false;
            }
        }

        /// <summary>
        /// Verifica si un email ya está registrado, excluyendo un usuario por Id (para edición).
        /// </summary>
        public bool EmailExisteParaOtro(string email, int excluirUsuarioId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    // 👇 CORREGIDO: usuarios en minúsculas
                    string query = excluirUsuarioId > 0
                        ? "SELECT COUNT(*) FROM usuarios WHERE Email = @email AND Id != @excluirId"
                        : "SELECT COUNT(*) FROM usuarios WHERE Email = @email";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        if (excluirUsuarioId > 0)
                            cmd.Parameters.AddWithValue("@excluirId", excluirUsuarioId);
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Actualiza todos los datos de perfil de un usuario (incluye DNI, ciudad, etc.).
        /// </summary>
        public bool ActualizarPerfilCompleto(int id, string nombre, string apellidos, string email,
            string telefono, string dni, string direccion, string ciudad, string codigoPostal, DateTime? fechaNacimiento)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    // 👇 CORREGIDO: usuarios en minúsculas (ya estaba bien)
                    string query = @"UPDATE usuarios SET Nombre=@nombre, Apellidos=@apellidos, Email=@email,
                        Telefono=@telefono, DNI=@dni, Direccion=@direccion, Ciudad=@ciudad, CodigoPostal=@codigoPostal, FechaNacimiento=@fechaNac
                        WHERE Id=@id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nombre", nombre ?? "");
                        cmd.Parameters.AddWithValue("@apellidos", apellidos ?? "");
                        cmd.Parameters.AddWithValue("@email", email ?? "");
                        cmd.Parameters.AddWithValue("@telefono", (object)telefono ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@dni", (object)dni ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@direccion", (object)direccion ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ciudad", (object)ciudad ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@codigoPostal", (object)codigoPostal ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@fechaNac", fechaNacimiento.HasValue ? (object)fechaNacimiento.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@id", id);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ActualizarPerfilCompleto: {ex.Message}");
                MessageBox.Show($"Error ActualizarPerfilCompleto: {ex.Message}", "Error");
                return false;
            }
        }

        /// <summary>
        /// Actualiza los datos de perfil de un usuario (nombre, apellidos, email).
        /// </summary>
        public bool ActualizarPerfilUsuario(int id, string nombre, string apellidos, string email)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    // 👇 CORREGIDO: usuarios en minúsculas
                    string query = @"UPDATE usuarios SET Nombre=@nombre, Apellidos=@apellidos, Email=@email WHERE Id=@id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nombre", nombre);
                        cmd.Parameters.AddWithValue("@apellidos", apellidos);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@id", id);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al actualizar perfil: {ex.Message}");
                MessageBox.Show($"Error al actualizar perfil: {ex.Message}", "Error");
                return false;
            }
        }

        /// <summary>
        /// Cambia la contraseña del usuario. Verifica que la actual sea correcta.
        /// </summary>
        public bool CambiarContraseña(int usuarioId, string contraseñaActual, string nuevaContraseña, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(nuevaContraseña) || nuevaContraseña.Length < 6)
            {
                error = "La nueva contraseña debe tener al menos 6 caracteres.";
                return false;
            }
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    // 👇 CORREGIDO: usuarios en minúsculas
                    string query = "SELECT PasswordHash FROM usuarios WHERE Id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", usuarioId);
                        object result = cmd.ExecuteScalar();
                        if (result == null)
                        {
                            error = "Usuario no encontrado.";
                            return false;
                        }
                        string hashActual = result.ToString();
                        if (!BCrypt.Net.BCrypt.Verify(contraseñaActual, hashActual))
                        {
                            error = "La contraseña actual no es correcta.";
                            return false;
                        }
                    }
                    string nuevoHash = BCrypt.Net.BCrypt.HashPassword(nuevaContraseña);
                    // 👇 CORREGIDO: usuarios en minúsculas
                    using (MySqlCommand cmdUpd = new MySqlCommand("UPDATE usuarios SET PasswordHash = @hash WHERE Id = @id", conn))
                    {
                        cmdUpd.Parameters.AddWithValue("@hash", nuevoHash);
                        cmdUpd.Parameters.AddWithValue("@id", usuarioId);
                        return cmdUpd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error CambiarContraseña: {ex.Message}");
                error = ex.Message;
                MessageBox.Show($"Error al cambiar contraseña: {ex.Message}", "Error");
                return false;
            }
        }

        /// <summary>
        /// Verifica si un email ya está registrado en la base de datos.
        /// </summary>
        public bool EmailExiste(string email)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();

                    // 👇 CORREGIDO: usuarios en minúsculas
                    string query = "SELECT COUNT(*) FROM usuarios WHERE Email = @email";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Prueba la conexión con la base de datos MySQL.
        /// </summary>
        public bool TestConexion()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}