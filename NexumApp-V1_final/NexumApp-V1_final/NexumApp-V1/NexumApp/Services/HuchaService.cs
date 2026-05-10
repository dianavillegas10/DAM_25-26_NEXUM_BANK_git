using MySql.Data.MySqlClient;
using NexumApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace NexumApp.Services
{
    public class HuchaService
    {
        private static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["NexumDB"].ConnectionString;

        // ── Obtener todas las huchas activas de un usuario ────────
        public List<Hucha> ObtenerPorUsuario(int usuarioId)
        {
            var lista = new List<Hucha>();
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(
                        @"SELECT Id, UsuarioId, Nombre, Emoji, SaldoActual,
                                 MetaObjetivo, ColorHex, Activa
                          FROM huchas
                          WHERE UsuarioId = @uid AND Activa = 1
                          ORDER BY FechaCreacion ASC", conn);
                    cmd.Parameters.AddWithValue("@uid", usuarioId);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            lista.Add(new Hucha
                            {
                                Id           = r.GetInt32("Id"),
                                UsuarioId    = r.GetInt32("UsuarioId"),
                                Nombre       = r.GetString("Nombre"),
                                Emoji        = r["Emoji"] == DBNull.Value ? "🐷" : r.GetString("Emoji"),
                                SaldoActual  = r.GetDecimal("SaldoActual"),
                                MetaObjetivo = r.GetDecimal("MetaObjetivo"),
                                ColorHex     = r["ColorHex"] == DBNull.Value ? "#3B82F6" : r.GetString("ColorHex"),
                                Activa       = r.GetBoolean("Activa")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HuchaService.ObtenerPorUsuario: {ex.Message}");
            }
            return lista;
        }

        // ── Crear nueva hucha ─────────────────────────────────────
        public bool Crear(Hucha hucha)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(
                        @"INSERT INTO huchas (UsuarioId, Nombre, Emoji, SaldoActual, MetaObjetivo, ColorHex)
                          VALUES (@uid, @nombre, @emoji, 0, @meta, @color)", conn);
                    cmd.Parameters.AddWithValue("@uid",    hucha.UsuarioId);
                    cmd.Parameters.AddWithValue("@nombre", hucha.Nombre);
                    cmd.Parameters.AddWithValue("@emoji",  hucha.Emoji ?? "🐷");
                    cmd.Parameters.AddWithValue("@meta",   hucha.MetaObjetivo);
                    cmd.Parameters.AddWithValue("@color",  hucha.ColorHex ?? "#3B82F6");
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HuchaService.Crear: {ex.Message}");
                return false;
            }
        }

        // ── Añadir saldo a una hucha (descuenta de la cuenta) ─────
        public bool AñadirSaldo(int huchaId, decimal monto, int cuentaId, MovimientoService movSvc, string nombreHucha = null)
        {
            if (monto <= 0) return false;
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        // 1. Obtener saldo actual de la hucha
                        decimal saldoHucha;
                        using (var cmd = new MySqlCommand(
                            "SELECT SaldoActual FROM huchas WHERE Id = @id", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@id", huchaId);
                            saldoHucha = Convert.ToDecimal(cmd.ExecuteScalar());
                        }

                        // 2. Actualizar saldo de la hucha
                        using (var cmd = new MySqlCommand(
                            "UPDATE huchas SET SaldoActual = SaldoActual + @monto WHERE Id = @id",
                            conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@monto", monto);
                            cmd.Parameters.AddWithValue("@id",    huchaId);
                            cmd.ExecuteNonQuery();
                        }

                        trans.Commit();
                    }
                }

                // 3. Descontar de la cuenta bancaria (fuera de la transacción de hucha)
                string concepto = string.IsNullOrWhiteSpace(nombreHucha)
                    ? "Ingreso a hucha"
                    : $"Ingreso a hucha: {nombreHucha}";
                string errMsg;
                movSvc.RegistrarRetiro(cuentaId, monto, concepto, out errMsg);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HuchaService.AñadirSaldo: {ex.Message}");
                return false;
            }
        }

        // ── Actualizar nombre, emoji, meta y color ────────────────
        public bool Actualizar(Hucha hucha)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(
                        @"UPDATE huchas
                          SET Nombre = @nombre, Emoji = @emoji, MetaObjetivo = @meta, ColorHex = @color
                          WHERE Id = @id", conn);
                    cmd.Parameters.AddWithValue("@nombre", hucha.Nombre);
                    cmd.Parameters.AddWithValue("@emoji",  hucha.Emoji ?? "🐷");
                    cmd.Parameters.AddWithValue("@meta",   hucha.MetaObjetivo);
                    cmd.Parameters.AddWithValue("@color",  hucha.ColorHex ?? "#3B82F6");
                    cmd.Parameters.AddWithValue("@id",     hucha.Id);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HuchaService.Actualizar: {ex.Message}");
                return false;
            }
        }

        // ── Eliminar (soft delete) ────────────────────────────────
        public bool Eliminar(int huchaId)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(
                        "UPDATE huchas SET Activa = 0 WHERE Id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", huchaId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HuchaService.Eliminar: {ex.Message}");
                return false;
            }
        }
    }
}
