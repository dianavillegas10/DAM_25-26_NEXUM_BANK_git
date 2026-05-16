using MySql.Data.MySqlClient;
using NexumApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Forms;

namespace NexumApp.Services
{
    public class CuentaService
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
        /// Obtiene todas las cuentas activas de un usuario específico.
        /// </summary>
        public List<CuentaBancaria> ObtenerCuentasPorUsuario(int usuarioId)
        {
            var lista = new List<CuentaBancaria>();
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"SELECT Id, UsuarioId, NumeroCuenta, TipoCuenta, Saldo, FechaApertura, Activa 
                                     FROM cuentas_bancarias 
                                     WHERE UsuarioId = @usuarioId AND Activa = 1 
                                     ORDER BY FechaApertura DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new CuentaBancaria
                                {
                                    Id = reader.GetInt32("Id"),
                                    UsuarioId = reader.GetInt32("UsuarioId"),
                                    NumeroCuenta = reader.GetString("NumeroCuenta"),
                                    TipoCuenta = reader.GetString("TipoCuenta"),
                                    Saldo = reader.GetDecimal("Saldo"),
                                    FechaApertura = reader.GetDateTime("FechaApertura"),
                                    Activa = reader.GetBoolean("Activa")
                                });
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error MySQL: {ex.Message}");
                MessageBox.Show("No se pudo obtener la lista de cuentas. Error de servidor.", "Error de Red", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return lista;
        }

        public CuentaBancaria ObtenerCuentaPorId(int cuentaId)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT Id, UsuarioId, NumeroCuenta, TipoCuenta, Saldo, FechaApertura, Activa FROM cuentas_bancarias WHERE Id = @id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", cuentaId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new CuentaBancaria
                                {
                                    Id = reader.GetInt32("Id"),
                                    UsuarioId = reader.GetInt32("UsuarioId"),
                                    NumeroCuenta = reader.GetString("NumeroCuenta"),
                                    TipoCuenta = reader.GetString("TipoCuenta"),
                                    Saldo = reader.GetDecimal("Saldo"),
                                    FechaApertura = reader.GetDateTime("FechaApertura"),
                                    Activa = reader.GetBoolean("Activa")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ObtenerCuentaPorId: {ex.Message}");
            }
            return null;
        }

        public decimal ObtenerSaldoCuenta(int cuentaId)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("SELECT Saldo FROM cuentas_bancarias WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", cuentaId);
                        var resultado = cmd.ExecuteScalar();
                        return resultado != null ? Convert.ToDecimal(resultado) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ObtenerSaldoCuenta: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Obtiene una cuenta bancaria por su número de cuenta.
        /// </summary>
        public CuentaBancaria ObtenerCuentaPorNumero(string numeroCuenta)
        {
            CuentaBancaria cuenta = null;

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    // Solo seleccionar las columnas que existen en tu modelo
                    string sql = @"SELECT Id, UsuarioId, NumeroCuenta, TipoCuenta, Saldo, FechaApertura, Activa 
                           FROM cuentas_bancarias 
                           WHERE NumeroCuenta = @numeroCuenta";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@numeroCuenta", numeroCuenta);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                cuenta = new CuentaBancaria
                                {
                                    Id = reader.GetInt32("Id"),
                                    UsuarioId = reader.GetInt32("UsuarioId"),
                                    NumeroCuenta = reader.GetString("NumeroCuenta"),
                                    TipoCuenta = reader.GetString("TipoCuenta"),
                                    Saldo = reader.GetDecimal("Saldo"),
                                    FechaApertura = reader.GetDateTime("FechaApertura"),
                                    Activa = reader.GetBoolean("Activa")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ObtenerCuentaPorNumero: {ex.Message}");
            }

            return cuenta;
        }

        /// <summary>
        /// Crea una nueva cuenta bancaria en la base de datos.
        /// </summary>
        public bool CrearCuenta(int usuarioId, string tipoCuenta, decimal saldoInicial, string numeroCuenta, string iban)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO cuentas_bancarias 
                                    (UsuarioId, NumeroCuenta, IBAN, TipoCuenta, Saldo, SaldoDisponible, FechaApertura, Activa, Moneda)
                                    VALUES 
                                    (@uid, @num, @iban, @tipo, @saldo, @saldo, NOW(), 1, 'EUR')";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", usuarioId);
                        cmd.Parameters.AddWithValue("@num", numeroCuenta);
                        cmd.Parameters.AddWithValue("@iban", iban ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@tipo", tipoCuenta);
                        cmd.Parameters.AddWithValue("@saldo", saldoInicial);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error CrearCuenta: {ex.Message}");
                throw;
            }
        }
    }
}