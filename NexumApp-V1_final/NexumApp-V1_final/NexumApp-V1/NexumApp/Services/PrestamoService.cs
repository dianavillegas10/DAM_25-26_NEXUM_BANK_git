using MySql.Data.MySqlClient;
using NexumApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace NexumApp.Services
{
    public class PrestamoService
    {
        private static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["NexumDB"].ConnectionString;

        // ── Tasas por tipo ────────────────────────────────────────────
        public static decimal ObtenerTasa(string tipo)
        {
            switch (tipo)
            {
                case "Hipoteca":  return 2.8m;
                case "Coche":     return 5.9m;
                case "Estudios":  return 3.5m;
                default:          return 8.5m;
            }
        }

        // ── Fórmula francesa de amortización ─────────────────────────
        public static decimal CalcularCuota(decimal monto, int plazoMeses, decimal tasaAnual)
        {
            if (plazoMeses <= 0 || monto <= 0) return 0;
            decimal tm = tasaAnual / 100m / 12m;
            if (tm == 0) return Math.Round(monto / plazoMeses, 2);
            double factor = Math.Pow((double)(1 + tm), plazoMeses);
            return Math.Round(monto * tm * (decimal)factor / ((decimal)factor - 1), 2);
        }

        // ── CRUD ──────────────────────────────────────────────────────
        public List<Prestamo> ObtenerPorUsuario(int usuarioId)
        {
            var lista = new List<Prestamo>();
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    const string sql = @"
                        SELECT Id, UsuarioId, CuentaId, TipoPrestamo,
                               MontoSolicitado, MontoAprobado, PlazoMeses, TasaInteres,
                               CuotaMensual, FechaSolicitud, FechaAprobacion,
                               Estado, SaldoPendiente, ProximoPago
                        FROM prestamos
                        WHERE UsuarioId = @uid
                        ORDER BY FechaSolicitud DESC";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", usuarioId);
                        using (var r = cmd.ExecuteReader())
                            while (r.Read()) lista.Add(Mapear(r));
                    }
                }
            }
            catch { }
            return lista;
        }

        public (bool Exito, string Error) Solicitar(Prestamo p)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    const string sql = @"
                        INSERT INTO prestamos
                            (UsuarioId, CuentaId, TipoPrestamo, MontoSolicitado, MontoAprobado,
                             PlazoMeses, TasaInteres, CuotaMensual, FechaSolicitud,
                             FechaAprobacion, Estado, SaldoPendiente, ProximoPago)
                        VALUES
                            (@uid, @cid, @tipo, @monto, @monto,
                             @plazo, @tasa, @cuota, NOW(),
                             NOW(), 'Aprobado', @monto, @proximoPago)";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid",        p.UsuarioId);
                        cmd.Parameters.AddWithValue("@cid",        p.CuentaId);
                        cmd.Parameters.AddWithValue("@tipo",       p.TipoPrestamo);
                        cmd.Parameters.AddWithValue("@monto",      p.MontoSolicitado);
                        cmd.Parameters.AddWithValue("@plazo",      p.PlazoMeses);
                        cmd.Parameters.AddWithValue("@tasa",       p.TasaInteres);
                        cmd.Parameters.AddWithValue("@cuota",      p.CuotaMensual ?? 0);
                        cmd.Parameters.AddWithValue("@proximoPago",
                            new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1));
                        cmd.ExecuteNonQuery();
                    }
                    return (true, null);
                }
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        private static Prestamo Mapear(MySqlDataReader r)
        {
            return new Prestamo
            {
                Id              = r.GetInt32("Id"),
                UsuarioId       = r.GetInt32("UsuarioId"),
                CuentaId        = r.GetInt32("CuentaId"),
                TipoPrestamo    = r.GetString("TipoPrestamo"),
                MontoSolicitado = r.GetDecimal("MontoSolicitado"),
                MontoAprobado   = r.IsDBNull(r.GetOrdinal("MontoAprobado"))   ? (decimal?)null : r.GetDecimal("MontoAprobado"),
                PlazoMeses      = r.GetInt32("PlazoMeses"),
                TasaInteres     = r.GetDecimal("TasaInteres"),
                CuotaMensual    = r.IsDBNull(r.GetOrdinal("CuotaMensual"))    ? (decimal?)null : r.GetDecimal("CuotaMensual"),
                FechaSolicitud  = r.GetDateTime("FechaSolicitud"),
                FechaAprobacion = r.IsDBNull(r.GetOrdinal("FechaAprobacion")) ? (DateTime?)null : r.GetDateTime("FechaAprobacion"),
                Estado          = r.GetString("Estado"),
                SaldoPendiente  = r.IsDBNull(r.GetOrdinal("SaldoPendiente"))  ? (decimal?)null : r.GetDecimal("SaldoPendiente"),
                ProximoPago     = r.IsDBNull(r.GetOrdinal("ProximoPago"))     ? (DateTime?)null : r.GetDateTime("ProximoPago")
            };
        }
    }
}
