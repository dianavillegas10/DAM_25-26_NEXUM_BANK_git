namespace NexumApp.Models
{
    /// <summary>
    /// Configuración de preferencias del usuario (implementación Diana).
    /// En V1 se guarda en memoria durante la sesión.
    /// </summary>
    public class ConfiguracionUsuario
    {
        public int UsuarioId { get; set; }

        // Notificaciones
        public bool NotificacionesEmail { get; set; } = true;
        public bool NotificacionesSMS { get; set; } = false;
        public bool NotificacionesPush { get; set; } = true;
        public bool NotificacionesMarketing { get; set; } = false;

        // Apariencia
        public bool ModoOscuro { get; set; } = false;
        public bool AltoContraste { get; set; } = false;
        public string Idioma { get; set; } = "es";
        public string MonedaPreferida { get; set; } = "EUR";
        public int TamanoFuente { get; set; } = 100;

        // Seguridad
        public bool DosFactores { get; set; } = false;
        public string CodigoVerificacion { get; set; } = null; // hash BCrypt del PIN — no se muestra en UI
        public bool SesionSegura { get; set; } = true;
        public int TiempoSesionMinutos { get; set; } = 30;

        // Preferencias de cuenta
        public bool MostrarSaldoInicio { get; set; } = true;
        public bool OrdenarCuentasPorSaldo { get; set; } = false;
        public bool ConfirmarTransferencias { get; set; } = true;
        public bool GuardarBeneficiarios { get; set; } = true;

        // Presupuesto mensual
        /// <summary>Objetivo mensual de ahorro fijado por el usuario. 0 = no configurado.</summary>
        public decimal PresupuestoObjetivo { get; set; } = 0m;
    }
}
