// Services/TarjetaService.cs
using MySql.Data.MySqlClient; // O el que uses (MySql.Data)
using NexumApp.Helpers;
using NexumApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace NexumApp.Services
{
    public class TarjetaService
    {
        private readonly string _connectionString; // Campo privado

        // 1. AGREGA ESTE CONSTRUCTOR
        public TarjetaService()
        {
            _connectionString = ConnectionString;
        }

        // Propiedad que lee del Config
        private static string ConnectionString
        {
            get
            {
                var cs = ConfigurationManager.ConnectionStrings["NexumDB"];
                if (cs == null || string.IsNullOrEmpty(cs.ConnectionString))
                    throw new InvalidOperationException("La cadena de conexión 'NexumDB' no está configurada.");
                return cs.ConnectionString;
            }
        }

        public List<Tarjeta> ObtenerTarjetasPorUsuario(int usuarioId)
        {
            var tarjetas = new List<Tarjeta>();

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT t.*, c.NumeroCuenta, c.TipoCuenta 
                    FROM tarjetas t
                    INNER JOIN cuentas_bancarias c ON t.CuentaId = c.Id
                    WHERE t.UsuarioId = @usuarioId AND t.Activa = 1
                    ORDER BY t.EsPrincipal DESC, t.Id ASC";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tarjetas.Add(new Tarjeta
                            {
                                Id = reader.GetInt32("Id"),
                                UsuarioId = reader.GetInt32("UsuarioId"),
                                CuentaId = reader.GetInt32("CuentaId"),
                                NumeroTarjeta = reader.GetString("NumeroTarjeta"),
                                TipoTarjeta = reader.GetString("TipoTarjeta"),
                                Marca = reader.GetString("Marca"),
                                NombreTitular = reader.GetString("NombreTitular"),
                                FechaEmision = reader.GetDateTime("FechaEmision"),
                                FechaCaducidad = reader.GetDateTime("FechaCaducidad"),
                                CVV = reader.GetString("CVV"),
                                LimiteDiario = reader.GetDecimal("LimiteDiario"),
                                LimiteMensual = reader.GetDecimal("LimiteMensual"),
                                LimiteCredito = reader.IsDBNull(reader.GetOrdinal("LimiteCredito"))
                ? (decimal?)null
                : reader.GetDecimal(reader.GetOrdinal("LimiteCredito")),
                                SaldoPendiente = reader.GetDecimal("SaldoPendiente"),
                                EsPrincipal = reader.GetBoolean("EsPrincipal"),
                                Activa = reader.GetBoolean("Activa"),
                                Bloqueada = reader.GetBoolean("Bloqueada"),
                                FechaBloqueo = reader.IsDBNull(reader.GetOrdinal("FechaBloqueo"))
               ? (DateTime?)null
               : reader.GetDateTime(reader.GetOrdinal("FechaBloqueo")),
                            });
                        }
                    }
                }
            }

            return tarjetas;
        }

        public bool BloquearTarjeta(int tarjetaId)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(
                        "UPDATE tarjetas SET Bloqueada = 1, FechaBloqueo = NOW() WHERE Id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", tarjetaId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TarjetaService.BloquearTarjeta: {ex.Message}");
                return false;
            }
        }

        public bool DesbloquearTarjeta(int tarjetaId)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(
                        "UPDATE tarjetas SET Bloqueada = 0, FechaBloqueo = NULL WHERE Id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", tarjetaId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TarjetaService.DesbloquearTarjeta: {ex.Message}");
                return false;
            }
        }

        public void GenerarTarjetaParaCuenta(int usuarioId, int cuentaId, string tipoTarjeta = "Debito")
        {
            var random = new Random();

            // Generar número de tarjeta (16 dígitos)
            string numeroTarjeta = GenerarNumeroTarjeta(random);

            // Generar CVV (3 dígitos)
            string cvv = random.Next(100, 999).ToString();

            // Obtener datos del usuario
            var usuario = ObtenerUsuario(usuarioId);

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
                    INSERT INTO tarjetas 
                    (UsuarioId, CuentaId, NumeroTarjeta, TipoTarjeta, Marca, 
                     NombreTitular, FechaEmision, FechaCaducidad, CVV, 
                     LimiteDiario, LimiteMensual, EsPrincipal, Activa)
                    VALUES 
                    (@usuarioId, @cuentaId, @numero, @tipo, 'Visa',
                     @titular, CURDATE(), @fechaCad, @cvv,
                     1000.00, 3000.00, 
                     (SELECT NOT EXISTS(SELECT 1 FROM tarjetas WHERE UsuarioId = @usuarioId)),
                     1)";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                    cmd.Parameters.AddWithValue("@cuentaId", cuentaId);
                    cmd.Parameters.AddWithValue("@numero", numeroTarjeta);
                    cmd.Parameters.AddWithValue("@tipo", tipoTarjeta);
                    cmd.Parameters.AddWithValue("@titular", $"{usuario.Nombre} {usuario.Apellidos}".ToUpper());
                    cmd.Parameters.AddWithValue("@fechaCad", DateTime.Now.AddYears(4));
                    cmd.Parameters.AddWithValue("@cvv", cvv);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string GenerarNumeroTarjeta(Random random)
        {
            // Prefijos según marca
            string[] prefijos = { "4532", "4916", "5412", "6011" };
            string prefijo = prefijos[random.Next(prefijos.Length)];

            string resto = "";
            for (int i = 0; i < 12; i++)
                resto += random.Next(0, 10).ToString();

            return prefijo + resto;
        }

        private Usuario ObtenerUsuario(int usuarioId)
        {
            // Implementa según tu servicio de usuarios
            return new UsuarioService().ObtenerPorId(usuarioId);
        }
    }
}