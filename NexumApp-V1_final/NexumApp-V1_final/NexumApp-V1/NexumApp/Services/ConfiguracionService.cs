using MySql.Data.MySqlClient;
using NexumApp.Models;
using System;
using System.Configuration;

namespace NexumApp.Services
{
    public class ConfiguracionService
    {
        private static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["NexumDB"].ConnectionString;

        private static bool _tablaVerificada = false;

        // Crea la tabla si no existe (se llama una sola vez por sesión de app)
        private void EnsureTable()
        {
            if (_tablaVerificada) return;
            _tablaVerificada = true;
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    new MySqlCommand(@"
                        CREATE TABLE IF NOT EXISTS `configuracion_usuario` (
                          `UsuarioId`               INT(11)      NOT NULL,
                          `NotificacionesEmail`     TINYINT(1)   NOT NULL DEFAULT 1,
                          `NotificacionesSMS`       TINYINT(1)   NOT NULL DEFAULT 0,
                          `NotificacionesPush`      TINYINT(1)   NOT NULL DEFAULT 1,
                          `NotificacionesMarketing` TINYINT(1)   NOT NULL DEFAULT 0,
                          `ModoOscuro`              TINYINT(1)   NOT NULL DEFAULT 0,
                          `AltoContraste`           TINYINT(1)   NOT NULL DEFAULT 0,
                          `Idioma`                  VARCHAR(5)   NOT NULL DEFAULT 'es',
                          `MonedaPreferida`         VARCHAR(3)   NOT NULL DEFAULT 'EUR',
                          `TamanoFuente`            INT(11)      NOT NULL DEFAULT 100,
                          `DosFactores`             TINYINT(1)   NOT NULL DEFAULT 0,
                          `CodigoVerificacion`      VARCHAR(100)          DEFAULT NULL,
                          `SesionSegura`            TINYINT(1)   NOT NULL DEFAULT 1,
                          `TiempoSesionMinutos`     INT(11)      NOT NULL DEFAULT 30,
                          `MostrarSaldoInicio`      TINYINT(1)   NOT NULL DEFAULT 1,
                          `OrdenarCuentasPorSaldo`  TINYINT(1)   NOT NULL DEFAULT 0,
                          `ConfirmarTransferencias` TINYINT(1)   NOT NULL DEFAULT 1,
                          `GuardarBeneficiarios`    TINYINT(1)   NOT NULL DEFAULT 1,
                          PRIMARY KEY (`UsuarioId`)
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;", conn).ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ConfiguracionService.EnsureTable: {ex.Message}");
            }
        }

        public ConfiguracionUsuario ObtenerConfiguracion(int usuarioId)
        {
            EnsureTable();
            var cfg = new ConfiguracionUsuario { UsuarioId = usuarioId };
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(@"
                        SELECT NotificacionesEmail, NotificacionesSMS, NotificacionesPush, NotificacionesMarketing,
                               ModoOscuro, AltoContraste, Idioma, MonedaPreferida, TamanoFuente,
                               DosFactores, CodigoVerificacion, SesionSegura, TiempoSesionMinutos,
                               MostrarSaldoInicio, OrdenarCuentasPorSaldo, ConfirmarTransferencias, GuardarBeneficiarios
                        FROM configuracion_usuario
                        WHERE UsuarioId = @uid", conn);
                    cmd.Parameters.AddWithValue("@uid", usuarioId);

                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            cfg.NotificacionesEmail     = r.GetBoolean("NotificacionesEmail");
                            cfg.NotificacionesSMS       = r.GetBoolean("NotificacionesSMS");
                            cfg.NotificacionesPush      = r.GetBoolean("NotificacionesPush");
                            cfg.NotificacionesMarketing = r.GetBoolean("NotificacionesMarketing");
                            cfg.ModoOscuro              = r.GetBoolean("ModoOscuro");
                            cfg.AltoContraste           = r.GetBoolean("AltoContraste");
                            cfg.Idioma                  = r.IsDBNull(r.GetOrdinal("Idioma"))          ? "es"  : r.GetString("Idioma");
                            cfg.MonedaPreferida         = r.IsDBNull(r.GetOrdinal("MonedaPreferida")) ? "EUR" : r.GetString("MonedaPreferida");
                            cfg.TamanoFuente            = r.GetInt32("TamanoFuente");
                            cfg.DosFactores             = r.GetBoolean("DosFactores");
                            cfg.CodigoVerificacion      = r.IsDBNull(r.GetOrdinal("CodigoVerificacion")) ? null : r.GetString("CodigoVerificacion");
                            cfg.SesionSegura            = r.GetBoolean("SesionSegura");
                            cfg.TiempoSesionMinutos     = r.GetInt32("TiempoSesionMinutos");
                            cfg.MostrarSaldoInicio      = r.GetBoolean("MostrarSaldoInicio");
                            cfg.OrdenarCuentasPorSaldo  = r.GetBoolean("OrdenarCuentasPorSaldo");
                            cfg.ConfirmarTransferencias = r.GetBoolean("ConfirmarTransferencias");
                            cfg.GuardarBeneficiarios    = r.GetBoolean("GuardarBeneficiarios");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ConfiguracionService.Obtener: {ex.Message}");
            }
            return cfg;
        }

        public bool GuardarConfiguracion(ConfiguracionUsuario cfg)
        {
            EnsureTable();
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(@"
                        INSERT INTO configuracion_usuario
                            (UsuarioId, NotificacionesEmail, NotificacionesSMS, NotificacionesPush, NotificacionesMarketing,
                             ModoOscuro, AltoContraste, Idioma, MonedaPreferida, TamanoFuente,
                             DosFactores, CodigoVerificacion, SesionSegura, TiempoSesionMinutos,
                             MostrarSaldoInicio, OrdenarCuentasPorSaldo, ConfirmarTransferencias, GuardarBeneficiarios)
                        VALUES
                            (@uid, @email, @sms, @push, @mkt,
                             @oscuro, @contraste, @idioma, @moneda, @fuente,
                             @2fa, @codigo, @sesion, @timer,
                             @saldoInicio, @ordenar, @confirmar, @benef)
                        ON DUPLICATE KEY UPDATE
                            NotificacionesEmail     = @email,
                            NotificacionesSMS       = @sms,
                            NotificacionesPush      = @push,
                            NotificacionesMarketing = @mkt,
                            ModoOscuro              = @oscuro,
                            AltoContraste           = @contraste,
                            Idioma                  = @idioma,
                            MonedaPreferida         = @moneda,
                            TamanoFuente            = @fuente,
                            DosFactores             = @2fa,
                            CodigoVerificacion      = IF(@codigo IS NULL, CodigoVerificacion, @codigo),
                            SesionSegura            = @sesion,
                            TiempoSesionMinutos     = @timer,
                            MostrarSaldoInicio      = @saldoInicio,
                            OrdenarCuentasPorSaldo  = @ordenar,
                            ConfirmarTransferencias = @confirmar,
                            GuardarBeneficiarios    = @benef", conn);

                    cmd.Parameters.AddWithValue("@uid",         cfg.UsuarioId);
                    cmd.Parameters.AddWithValue("@email",       cfg.NotificacionesEmail);
                    cmd.Parameters.AddWithValue("@sms",         cfg.NotificacionesSMS);
                    cmd.Parameters.AddWithValue("@push",        cfg.NotificacionesPush);
                    cmd.Parameters.AddWithValue("@mkt",         cfg.NotificacionesMarketing);
                    cmd.Parameters.AddWithValue("@oscuro",      cfg.ModoOscuro);
                    cmd.Parameters.AddWithValue("@contraste",   cfg.AltoContraste);
                    cmd.Parameters.AddWithValue("@idioma",      cfg.Idioma ?? "es");
                    cmd.Parameters.AddWithValue("@moneda",      cfg.MonedaPreferida ?? "EUR");
                    cmd.Parameters.AddWithValue("@fuente",      cfg.TamanoFuente);
                    cmd.Parameters.AddWithValue("@2fa",         cfg.DosFactores);
                    cmd.Parameters.AddWithValue("@codigo",      (object)cfg.CodigoVerificacion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@sesion",      cfg.SesionSegura);
                    cmd.Parameters.AddWithValue("@timer",       cfg.TiempoSesionMinutos);
                    cmd.Parameters.AddWithValue("@saldoInicio", cfg.MostrarSaldoInicio);
                    cmd.Parameters.AddWithValue("@ordenar",     cfg.OrdenarCuentasPorSaldo);
                    cmd.Parameters.AddWithValue("@confirmar",   cfg.ConfirmarTransferencias);
                    cmd.Parameters.AddWithValue("@benef",       cfg.GuardarBeneficiarios);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ConfiguracionService.Guardar: {ex.Message}");
                return false;
            }
        }

        /// <summary>Actualiza solo el código de verificación 2FA para un usuario.</summary>
        public bool ActualizarCodigo2FA(int usuarioId, string codigoHash)
        {
            EnsureTable();
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(@"
                        INSERT INTO configuracion_usuario (UsuarioId, CodigoVerificacion, DosFactores)
                        VALUES (@uid, @codigo, 1)
                        ON DUPLICATE KEY UPDATE CodigoVerificacion = @codigo, DosFactores = 1", conn);
                    cmd.Parameters.AddWithValue("@uid",    usuarioId);
                    cmd.Parameters.AddWithValue("@codigo", codigoHash);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch { return false; }
        }

        /// <summary>Desactiva 2FA y limpia el código de verificación.</summary>
        public bool Desactivar2FA(int usuarioId)
        {
            EnsureTable();
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(@"
                        INSERT INTO configuracion_usuario (UsuarioId, DosFactores, CodigoVerificacion)
                        VALUES (@uid, 0, NULL)
                        ON DUPLICATE KEY UPDATE DosFactores = 0, CodigoVerificacion = NULL", conn);
                    cmd.Parameters.AddWithValue("@uid", usuarioId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch { return false; }
        }
    }
}
