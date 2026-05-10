namespace NexumApp.Models
{
    /// <summary>
    /// Datos del presupuesto mensual. Preparado para cargar desde base de datos.
    /// </summary>
    public class DatosPresupuesto
    {
        public decimal Ingresos { get; set; }
        public decimal Gastos { get; set; }
        public decimal Objetivo { get; set; }
        public decimal Restante { get; set; }
        public string Mes { get; set; }
        public string MensajeEstado { get; set; }
    }
}
