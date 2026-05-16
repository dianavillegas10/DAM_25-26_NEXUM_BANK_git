using MySql.Data.MySqlClient;
using NexumApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace NexumApp.Services
{
    public class MovimientoService
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

        // =========================================================
        // 💳 PAGO DE SERVICIOS (VERSIÓN CORREGIDA - ÚNICA)
        // =========================================================
        public bool RegistrarPagoServicio(int cuentaId, decimal importe, string servicio, string compañia, out string error)
        {
            error = string.Empty;

            if (importe <= 0)
            {
                error = "El importe debe ser mayor que cero.";
                return false;
            }

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        decimal saldoAnterior;

                        // 1. Obtener saldo actual de la cuenta
                        using (var cmd = new MySqlCommand("SELECT Saldo FROM cuentas_bancarias WHERE Id = @cuentaId", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@cuentaId", cuentaId);
                            var resultado = cmd.ExecuteScalar();

                            if (resultado == null)
                            {
                                error = "Cuenta no encontrada.";
                                return false;
                            }

                            saldoAnterior = Convert.ToDecimal(resultado);
                        }

                        // 2. Verificar saldo suficiente
                        if (saldoAnterior < importe)
                        {
                            error = $"Saldo insuficiente. Saldo actual: {saldoAnterior:C}";
                            return false;
                        }

                        decimal saldoNuevo = saldoAnterior - importe;
                        string concepto = $"Pago de {servicio} - {compañia}";

                        // 3. Insertar el movimiento (GASTO)
                        using (var cmd = new MySqlCommand(@"
                            INSERT INTO movimientos (CuentaId, TipoMovimiento, Monto, Fecha, Concepto, SaldoAnterior, SaldoPosterior)
                            VALUES (@cuentaId, @tipo, @monto, NOW(), @concepto, @saldoAnterior, @saldoNuevo)", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@cuentaId", cuentaId);
                            cmd.Parameters.AddWithValue("@tipo", "Gasto");
                            cmd.Parameters.AddWithValue("@monto", importe);
                            cmd.Parameters.AddWithValue("@concepto", concepto);
                            cmd.Parameters.AddWithValue("@saldoAnterior", saldoAnterior);
                            cmd.Parameters.AddWithValue("@saldoNuevo", saldoNuevo);
                            cmd.ExecuteNonQuery();
                        }

                        // 4. ACTUALIZAR EL SALDO DE LA CUENTA (¡PARTE CLAVE!)
                        using (var cmd = new MySqlCommand("UPDATE cuentas_bancarias SET Saldo = @saldoNuevo WHERE Id = @cuentaId", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@saldoNuevo", saldoNuevo);
                            cmd.Parameters.AddWithValue("@cuentaId", cuentaId);
                            int filasAfectadas = cmd.ExecuteNonQuery();

                            if (filasAfectadas == 0)
                            {
                                error = "No se pudo actualizar el saldo de la cuenta.";
                                return false;
                            }
                        }

                        trans.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                System.Diagnostics.Debug.WriteLine($"Error en RegistrarPagoServicio: {ex.Message}");
                return false;
            }
        }

        // =========================================================
        // 🔍 MOVIMIENTOS RECIENTES
        // =========================================================
        public List<Movimiento> ObtenerMovimientosRecientesPorUsuario(int usuarioId, int limite = 10)
        {
            var lista = new List<Movimiento>();

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();

                    int limitVal = Math.Max(1, Math.Min(limite, 100));
                    string query = $@"SELECT m.Id, m.CuentaId, m.TipoMovimiento, m.Monto, m.Fecha, m.Concepto, m.SaldoAnterior, m.SaldoPosterior
                                    FROM movimientos m
                                    INNER JOIN cuentas_bancarias c ON m.CuentaId = c.Id
                                    WHERE c.UsuarioId = @usuarioId
                                    ORDER BY m.Fecha DESC
                                    LIMIT {limitVal}";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new Movimiento
                                {
                                    Id = reader.GetInt32("Id"),
                                    CuentaId = reader.GetInt32("CuentaId"),
                                    TipoMovimiento = reader.GetString("TipoMovimiento"),
                                    Monto = reader.GetDecimal("Monto"),
                                    Fecha = reader.GetDateTime("Fecha"),
                                    Concepto = reader["Concepto"] == DBNull.Value ? "" : reader.GetString("Concepto"),
                                    SaldoAnterior = reader.GetDecimal("SaldoAnterior"),
                                    SaldoPosterior = reader.GetDecimal("SaldoPosterior")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ObtenerMovimientosRecientesPorUsuario: {ex.Message}");
            }

            return lista;
        }

        // =========================================================
        // 💰 INGRESO
        // =========================================================
        public bool RegistrarIngreso(int cuentaId, decimal monto, string concepto = "")
        {
            if (monto <= 0) return false;

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();

                    using (var trans = conn.BeginTransaction())
                    {
                        decimal saldoAnterior;

                        using (var cmdSel = new MySqlCommand("SELECT Saldo FROM cuentas_bancarias WHERE Id = @id", conn, trans))
                        {
                            cmdSel.Parameters.AddWithValue("@id", cuentaId);
                            saldoAnterior = Convert.ToDecimal(cmdSel.ExecuteScalar());
                        }

                        decimal saldoNuevo = saldoAnterior + monto;

                        InsertarMovimiento(conn, trans, cuentaId, "Ingreso", monto, concepto, saldoAnterior, saldoNuevo);
                        ActualizarSaldo(conn, trans, cuentaId, saldoNuevo);

                        trans.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error RegistrarIngreso: {ex.Message}");
                return false;
            }
        }

        // =========================================================
        // 💸 RETIRO
        // =========================================================
        public bool RegistrarRetiro(int cuentaId, decimal monto, string concepto, out string error)
        {
            error = null;

            if (monto <= 0)
            {
                error = "El monto debe ser mayor que 0.";
                return false;
            }

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();

                    using (var trans = conn.BeginTransaction())
                    {
                        decimal saldoAnterior;

                        using (var cmdSel = new MySqlCommand("SELECT Saldo FROM cuentas_bancarias WHERE Id = @id", conn, trans))
                        {
                            cmdSel.Parameters.AddWithValue("@id", cuentaId);
                            saldoAnterior = Convert.ToDecimal(cmdSel.ExecuteScalar());
                        }

                        if (saldoAnterior < monto)
                        {
                            error = "Saldo insuficiente.";
                            return false;
                        }

                        decimal saldoNuevo = saldoAnterior - monto;

                        InsertarMovimiento(conn, trans, cuentaId, "Retiro", monto, concepto, saldoAnterior, saldoNuevo);
                        ActualizarSaldo(conn, trans, cuentaId, saldoNuevo);

                        trans.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                System.Diagnostics.Debug.WriteLine($"Error RegistrarRetiro: {ex.Message}");
                return false;
            }
        }

        // =========================================================
        // 💸 TRANSFERENCIA
        // =========================================================
        public bool RealizarTransferencia(int cuentaOrigenId, string cuentaDestino, decimal importe, string concepto)
        {
            var transferenciaService = new TransferenciaService();
            string error;
            bool ok = transferenciaService.RealizarTransferencia(
                cuentaOrigenId,
                cuentaDestino,
                null,
                importe,
                concepto,
                out error
            );

            if (!ok && !string.IsNullOrWhiteSpace(error))
                System.Diagnostics.Debug.WriteLine($"Error Transferencia: {error}");

            return ok;
        }

        // =========================================================
        // 🔧 MÉTODOS AUXILIARES
        // =========================================================
        private void InsertarMovimiento(MySqlConnection conn, MySqlTransaction trans,
            int cuentaId, string tipo, decimal monto, string concepto,
            decimal saldoAnterior, decimal saldoNuevo)
        {
            using (var cmd = new MySqlCommand(
                @"INSERT INTO movimientos (CuentaId, TipoMovimiento, Monto, Fecha, Concepto, SaldoAnterior, SaldoPosterior)
                  VALUES (@cuentaId, @tipo, @monto, NOW(), @concepto, @saldoAnt, @saldoNuevo)", conn, trans))
            {
                cmd.Parameters.AddWithValue("@cuentaId", cuentaId);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.Parameters.AddWithValue("@monto", monto);
                cmd.Parameters.AddWithValue("@concepto", concepto ?? "");
                cmd.Parameters.AddWithValue("@saldoAnt", saldoAnterior);
                cmd.Parameters.AddWithValue("@saldoNuevo", saldoNuevo);

                cmd.ExecuteNonQuery();
            }
        }

        private void ActualizarSaldo(MySqlConnection conn, MySqlTransaction trans,
            int cuentaId, decimal saldoNuevo)
        {
            using (var cmd = new MySqlCommand(
                "UPDATE cuentas_bancarias SET Saldo = @saldo WHERE Id = @id", conn, trans))
            {
                cmd.Parameters.AddWithValue("@saldo", saldoNuevo);
                cmd.Parameters.AddWithValue("@id", cuentaId);
                cmd.ExecuteNonQuery();
            }
        }

        // =========================================================
        // 📊 RESUMEN MENSUAL
        // =========================================================
        public (decimal Ingresos, decimal Gastos) ObtenerResumenMensual(int usuarioId)
        {
            decimal ingresos = 0, gastos = 0;
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT m.TipoMovimiento, SUM(m.Monto) AS Total
                        FROM movimientos m
                        INNER JOIN cuentas_bancarias c ON m.CuentaId = c.Id
                        WHERE c.UsuarioId = @usuarioId
                          AND MONTH(m.Fecha) = MONTH(CURRENT_DATE())
                          AND YEAR(m.Fecha)  = YEAR(CURRENT_DATE())
                        GROUP BY m.TipoMovimiento";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string tipo = reader.GetString("TipoMovimiento");
                                decimal total = reader.GetDecimal("Total");
                                if (tipo.Equals("Ingreso", StringComparison.OrdinalIgnoreCase))
                                    ingresos += total;
                                else
                                    gastos += total;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ObtenerResumenMensual: {ex.Message}");
            }
            return (ingresos, gastos);
        }

        // =========================================================
        // 📊 MOVIMIENTOS POR CUENTA
        // =========================================================
        public List<Movimiento> ObtenerMovimientosPorCuenta(int cuentaId, int limite = 50)
        {
            var lista = new List<Movimiento>();

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();

                    int limitVal = Math.Max(1, Math.Min(limite, 200));
                    string query = $@"SELECT m.Id, m.CuentaId, m.TipoMovimiento, m.Monto, m.Fecha, m.Concepto, m.SaldoAnterior, m.SaldoPosterior
                                    FROM movimientos m
                                    WHERE m.CuentaId = @cuentaId
                                    ORDER BY m.Fecha DESC
                                    LIMIT {limitVal}";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@cuentaId", cuentaId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new Movimiento
                                {
                                    Id = reader.GetInt32("Id"),
                                    CuentaId = reader.GetInt32("CuentaId"),
                                    TipoMovimiento = reader.GetString("TipoMovimiento"),
                                    Monto = reader.GetDecimal("Monto"),
                                    Fecha = reader.GetDateTime("Fecha"),
                                    Concepto = reader["Concepto"] == DBNull.Value ? "" : reader.GetString("Concepto"),
                                    SaldoAnterior = reader.GetDecimal("SaldoAnterior"),
                                    SaldoPosterior = reader.GetDecimal("SaldoPosterior")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ObtenerMovimientosPorCuenta: {ex.Message}");
            }

            return lista;
        }
    }
}