using MySql.Data.MySqlClient;
using NexumApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace NexumApp.Services
{
    internal class TransferenciaService
    {
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
        /// Realiza una transferencia desde la cuenta origen a la cuenta destino (IBAN).
        /// Descuenta el saldo de origen, registra el movimiento y guarda en la tabla transferencias.
        /// </summary>
        public bool RealizarTransferencia(int cuentaOrigenId, string cuentaDestino,
            string nombreBeneficiario, decimal monto, string concepto, out string error)
        {
            error = null;

            if (monto <= 0) { error = "El importe debe ser mayor que 0."; return false; }
            if (string.IsNullOrWhiteSpace(cuentaDestino)) { error = "La cuenta de destino es obligatoria."; return false; }

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        // 1. Obtener saldo actual de la cuenta origen
                        decimal saldoAnterior;
                        using (var cmd = new MySqlCommand(
                            "SELECT Saldo FROM cuentas_bancarias WHERE Id = @id AND Activa = 1", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@id", cuentaOrigenId);
                            var result = cmd.ExecuteScalar();
                            if (result == null)
                            {
                                trans.Rollback();
                                error = "Cuenta de origen no encontrada.";
                                return false;
                            }
                            saldoAnterior = Convert.ToDecimal(result);
                        }

                        if (saldoAnterior < monto)
                        {
                            trans.Rollback();
                            error = "Saldo insuficiente.";
                            return false;
                        }

                        decimal saldoNuevo = saldoAnterior - monto;
                        string conceptoFinal = string.IsNullOrWhiteSpace(concepto)
                            ? "Transferencia a " + cuentaDestino
                            : concepto;

                        // 2. Registrar movimiento de salida
                        using (var cmd = new MySqlCommand(
                            @"INSERT INTO movimientos (CuentaId, TipoMovimiento, Monto, Fecha, Concepto, SaldoAnterior, SaldoPosterior)
                              VALUES (@cuentaId, 'Transferencia', @monto, NOW(), @concepto, @saldoAnt, @saldoNuevo)",
                            conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@cuentaId", cuentaOrigenId);
                            cmd.Parameters.AddWithValue("@monto", monto);
                            cmd.Parameters.AddWithValue("@concepto", conceptoFinal);
                            cmd.Parameters.AddWithValue("@saldoAnt", saldoAnterior);
                            cmd.Parameters.AddWithValue("@saldoNuevo", saldoNuevo);
                            cmd.ExecuteNonQuery();
                        }

                        // 3. Actualizar saldo de la cuenta origen
                        using (var cmd = new MySqlCommand(
                            "UPDATE cuentas_bancarias SET Saldo = @saldo WHERE Id = @id", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@saldo", saldoNuevo);
                            cmd.Parameters.AddWithValue("@id", cuentaOrigenId);
                            cmd.ExecuteNonQuery();
                        }

                        // 4. Si el destino es una cuenta interna, acreditarla
                        // Normalizar IBAN quitando espacios y guiones para comparar correctamente
                        string ibanNormalizado = cuentaDestino.Replace(" ", "").Replace("-", "").ToUpperInvariant();
                        int? cuentaDestinoId = null;
                        decimal saldoDestAnterior = 0;

                        // Buscar por NumeroCuenta primero
                        using (var cmd = new MySqlCommand(
                            "SELECT Id, Saldo FROM cuentas_bancarias WHERE REPLACE(REPLACE(NumeroCuenta,' ',''),'-','') = @iban AND Activa = 1 LIMIT 1",
                            conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@iban", ibanNormalizado);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    cuentaDestinoId = reader.GetInt32("Id");
                                    saldoDestAnterior = reader.GetDecimal("Saldo");
                                }
                            }
                        }

                        // Si no se encontró por NumeroCuenta, intentar por columna IBAN (si existe)
                        if (!cuentaDestinoId.HasValue)
                        {
                            try
                            {
                                using (var cmd = new MySqlCommand(
                                    "SELECT Id, Saldo FROM cuentas_bancarias WHERE REPLACE(REPLACE(IBAN,' ',''),'-','') = @iban AND Activa = 1 LIMIT 1",
                                    conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@iban", ibanNormalizado);
                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            cuentaDestinoId = reader.GetInt32("Id");
                                            saldoDestAnterior = reader.GetDecimal("Saldo");
                                        }
                                    }
                                }
                            }
                            catch { /* Columna IBAN no existe en esta versión de BD */ }
                        }

                        if (cuentaDestinoId.HasValue)
                        {
                            decimal saldoDestNuevo = saldoDestAnterior + monto;
                            string conceptoIngreso = string.IsNullOrWhiteSpace(concepto)
                                ? "Transferencia recibida"
                                : concepto;

                            using (var cmd = new MySqlCommand(
                                @"INSERT INTO movimientos (CuentaId, TipoMovimiento, Monto, Fecha, Concepto, SaldoAnterior, SaldoPosterior)
                                  VALUES (@cuentaId, 'Transferencia Recibida', @monto, NOW(), @concepto, @saldoAnt, @saldoNuevo)",
                                conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@cuentaId", cuentaDestinoId.Value);
                                cmd.Parameters.AddWithValue("@monto", monto);
                                cmd.Parameters.AddWithValue("@concepto", conceptoIngreso);
                                cmd.Parameters.AddWithValue("@saldoAnt", saldoDestAnterior);
                                cmd.Parameters.AddWithValue("@saldoNuevo", saldoDestNuevo);
                                cmd.ExecuteNonQuery();
                            }

                            using (var cmd = new MySqlCommand(
                                "UPDATE cuentas_bancarias SET Saldo = @saldo WHERE Id = @id",
                                conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@saldo", saldoDestNuevo);
                                cmd.Parameters.AddWithValue("@id", cuentaDestinoId.Value);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 5. Registrar en tabla transferencias
                        using (var cmd = new MySqlCommand(
                            @"INSERT INTO transferencias (CuentaOrigenId, CuentaDestinoNumero, NombreBeneficiario, Monto, Fecha, Concepto, Estado)
                              VALUES (@origen, @destino, @beneficiario, @monto, NOW(), @concepto, 'Completada')",
                            conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@origen", cuentaOrigenId);
                            cmd.Parameters.AddWithValue("@destino", cuentaDestino);
                            cmd.Parameters.AddWithValue("@beneficiario",
                                string.IsNullOrWhiteSpace(nombreBeneficiario)
                                    ? (object)DBNull.Value
                                    : nombreBeneficiario);
                            cmd.Parameters.AddWithValue("@monto", monto);
                            cmd.Parameters.AddWithValue("@concepto", concepto ?? "");
                            cmd.ExecuteNonQuery();
                        }

                        trans.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                error = "Error inesperado: " + ex.Message;
                System.Diagnostics.Debug.WriteLine($"Error TransferenciaService: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene el historial de transferencias enviadas por un usuario.
        /// </summary>
        public List<Transferencia> ObtenerTransferenciasPorUsuario(int usuarioId, int limite = 50)
        {
            var lista = new List<Transferencia>();
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    int limitVal = Math.Max(1, Math.Min(limite, 200));
                    string query = $@"
                        SELECT t.Id, t.CuentaOrigenId, t.CuentaDestinoNumero, t.NombreBeneficiario,
                               t.Monto, t.Fecha, t.Concepto, t.Estado
                        FROM transferencias t
                        INNER JOIN cuentas_bancarias c ON t.CuentaOrigenId = c.Id
                        WHERE c.UsuarioId = @usuarioId
                        ORDER BY t.Fecha DESC
                        LIMIT {limitVal}";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new Transferencia
                                {
                                    Id = reader.GetInt32("Id"),
                                    CuentaOrigenId = reader.GetInt32("CuentaOrigenId"),
                                    CuentaDestino = reader.GetString("CuentaDestinoNumero"),
                                    NombreBeneficiario = reader["NombreBeneficiario"] == DBNull.Value
                                        ? null : reader.GetString("NombreBeneficiario"),
                                    Monto = reader.GetDecimal("Monto"),
                                    Fecha = reader.GetDateTime("Fecha"),
                                    Concepto = reader["Concepto"] == DBNull.Value
                                        ? "" : reader.GetString("Concepto"),
                                    Estado = reader.GetString("Estado")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ObtenerTransferencias: {ex.Message}");
            }
            return lista;
        }
    }
}
