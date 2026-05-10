// Services/UsuarioService.cs
using MySql.Data.MySqlClient;
using NexumApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NexumApp.Services
{
    public class UsuarioService
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

        public async Task<List<Usuario>> ObtenerTodosAsync()
        {
            return await Task.Run(() =>
            {
                var usuarios = new List<Usuario>();

                try
                {
                    using (var conn = new MySqlConnection(ConnectionString))
                    {
                        conn.Open();
                        string query = "SELECT Id, Nombre, Apellidos, Email, Telefono, DNI, Direccion, Ciudad, CodigoPostal, Pais, FechaNacimiento, FechaRegistro, UltimoAcceso, FotoPerfil, EsAdmin, Activo, Verificado FROM usuarios";

                        using (var cmd = new MySqlCommand(query, conn))
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                usuarios.Add(new Usuario
                                {
                                    Id = reader.GetInt32("Id"),
                                    Nombre = reader.GetString("Nombre"),
                                    Apellidos = reader.GetString("Apellidos"),
                                    Email = reader.GetString("Email"),
                                    Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString("Telefono"),
                                    DNI = reader.IsDBNull(reader.GetOrdinal("DNI")) ? null : reader.GetString("DNI"),
                                    Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion")) ? null : reader.GetString("Direccion"),
                                    Ciudad = reader.IsDBNull(reader.GetOrdinal("Ciudad")) ? null : reader.GetString("Ciudad"),
                                    CodigoPostal = reader.IsDBNull(reader.GetOrdinal("CodigoPostal")) ? null : reader.GetString("CodigoPostal"),
                                                                      FechaNacimiento = reader.IsDBNull(reader.GetOrdinal("FechaNacimiento")) ? (DateTime?)null : reader.GetDateTime("FechaNacimiento"),
                                    FechaRegistro = reader.GetDateTime("FechaRegistro"),
                                    UltimoAcceso = reader.IsDBNull(reader.GetOrdinal("UltimoAcceso")) ? (DateTime?)null : reader.GetDateTime("UltimoAcceso"),
                                   
                                    EsAdmin = reader.GetBoolean("EsAdmin"),
                                    Activo = reader.GetBoolean("Activo"),
                                   
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error ObtenerTodosAsync: {ex.Message}");
                }

                return usuarios;
            });
        }

        public bool ActualizarDatosPersonales(int usuarioId, string dni, string telefono)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    const string sql =
                        "UPDATE usuarios SET DNI=@dni, Telefono=@tel WHERE Id=@uid";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@dni", dni      ?? "");
                        cmd.Parameters.AddWithValue("@tel", telefono ?? "");
                        cmd.Parameters.AddWithValue("@uid", usuarioId);
                        cmd.ExecuteNonQuery();
                    }
                    return true;
                }
            }
            catch { return false; }
        }

        /// <summary>
        /// Obtiene un usuario por su ID.
        /// </summary>
        public Usuario ObtenerPorId(int usuarioId)
        {
            Usuario usuario = null;

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"SELECT Id, Nombre, Apellidos, Email, Telefono, DNI, Direccion, Ciudad, 
                                            CodigoPostal, Pais, FechaNacimiento, FechaRegistro, UltimoAcceso, 
                                            FotoPerfil, EsAdmin, Activo, Verificado 
                                     FROM usuarios 
                                     WHERE Id = @id";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", usuarioId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                usuario = new Usuario
                                {
                                    Id = reader.GetInt32("Id"),
                                    Nombre = reader.GetString("Nombre"),
                                    Apellidos = reader.GetString("Apellidos"),
                                    Email = reader.GetString("Email"),
                                    Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString("Telefono"),
                                    DNI = reader.IsDBNull(reader.GetOrdinal("DNI")) ? null : reader.GetString("DNI"),
                                    Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion")) ? null : reader.GetString("Direccion"),
                                    Ciudad = reader.IsDBNull(reader.GetOrdinal("Ciudad")) ? null : reader.GetString("Ciudad"),
                                    CodigoPostal = reader.IsDBNull(reader.GetOrdinal("CodigoPostal")) ? null : reader.GetString("CodigoPostal"),
                                    
                                    FechaNacimiento = reader.IsDBNull(reader.GetOrdinal("FechaNacimiento")) ? (DateTime?)null : reader.GetDateTime("FechaNacimiento"),
                                    FechaRegistro = reader.GetDateTime("FechaRegistro"),
                                    UltimoAcceso = reader.IsDBNull(reader.GetOrdinal("UltimoAcceso")) ? (DateTime?)null : reader.GetDateTime("UltimoAcceso"),
                                    
                                    EsAdmin = reader.GetBoolean("EsAdmin"),
                                    Activo = reader.GetBoolean("Activo"),
                                    
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ObtenerPorId: {ex.Message}");
                MessageBox.Show($"Error al obtener el usuario: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return usuario;
        }

        /// <summary>
        /// Obtiene un usuario por su email (versión síncrona).
        /// </summary>
        public Usuario ObtenerPorEmail(string email)
        {
            Usuario usuario = null;

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"SELECT Id, Nombre, Apellidos, Email, Telefono, DNI, Direccion, Ciudad, 
                                            CodigoPostal, Pais, FechaNacimiento, FechaRegistro, UltimoAcceso, 
                                            FotoPerfil, EsAdmin, Activo, Verificado 
                                     FROM usuarios 
                                     WHERE Email = @email";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                usuario = new Usuario
                                {
                                    Id = reader.GetInt32("Id"),
                                    Nombre = reader.GetString("Nombre"),
                                    Apellidos = reader.GetString("Apellidos"),
                                    Email = reader.GetString("Email"),
                                    Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString("Telefono"),
                                    DNI = reader.IsDBNull(reader.GetOrdinal("DNI")) ? null : reader.GetString("DNI"),
                                    Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion")) ? null : reader.GetString("Direccion"),
                                    Ciudad = reader.IsDBNull(reader.GetOrdinal("Ciudad")) ? null : reader.GetString("Ciudad"),
                                    CodigoPostal = reader.IsDBNull(reader.GetOrdinal("CodigoPostal")) ? null : reader.GetString("CodigoPostal"),
                                    
                                    FechaNacimiento = reader.IsDBNull(reader.GetOrdinal("FechaNacimiento")) ? (DateTime?)null : reader.GetDateTime("FechaNacimiento"),
                                    FechaRegistro = reader.GetDateTime("FechaRegistro"),
                                    UltimoAcceso = reader.IsDBNull(reader.GetOrdinal("UltimoAcceso")) ? (DateTime?)null : reader.GetDateTime("UltimoAcceso"),
                                    
                                    EsAdmin = reader.GetBoolean("EsAdmin"),
                                    Activo = reader.GetBoolean("Activo"),
                                    
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ObtenerPorEmail: {ex.Message}");
            }

            return usuario;
        }

        /// <summary>
        /// Actualiza los datos básicos de un usuario en la base de datos.
        /// </summary>
        public async Task<bool> ActualizarUsuarioAsync(int id, string nombre, string email, bool esAdmin)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var conn = new MySqlConnection(ConnectionString))
                    {
                        conn.Open();
                        string query = @"UPDATE usuarios 
                                 SET Nombre = @nombre, 
                                     Email = @email, 
                                     EsAdmin = @esAdmin 
                                 WHERE Id = @id";

                        using (var cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@nombre", nombre);
                            cmd.Parameters.AddWithValue("@email", email);
                            cmd.Parameters.AddWithValue("@esAdmin", esAdmin);
                            cmd.Parameters.AddWithValue("@id", id);

                            int filasAfectadas = cmd.ExecuteNonQuery();
                            return filasAfectadas > 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error ActualizarUsuarioAsync: {ex.Message}");
                    return false;
                }
            });
        }

        /// <summary>
        /// Obtiene un usuario por su email (versión asíncrona).
        /// </summary>
        public async Task<Usuario> ObtenerPorEmailAsync(string email)
        {
            return await Task.Run(() => ObtenerPorEmail(email));
        }
    }
}