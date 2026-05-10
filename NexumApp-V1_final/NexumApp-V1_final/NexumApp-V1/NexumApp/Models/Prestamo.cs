using System;

namespace NexumApp.Models
{
    public class Prestamo
    {
        public int       Id              { get; set; }
        public int       UsuarioId       { get; set; }
        public int       CuentaId        { get; set; }
        public string    TipoPrestamo    { get; set; }
        public decimal   MontoSolicitado { get; set; }
        public decimal?  MontoAprobado   { get; set; }
        public int       PlazoMeses      { get; set; }
        public decimal   TasaInteres     { get; set; }
        public decimal?  CuotaMensual    { get; set; }
        public DateTime  FechaSolicitud  { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public string    Estado          { get; set; }
        public decimal?  SaldoPendiente  { get; set; }
        public DateTime? ProximoPago     { get; set; }

        public bool EsPendiente => Estado == "Pendiente";
        public bool EsAprobado  => Estado == "Aprobado";
        public bool EsRechazado => Estado == "Rechazado";
        public bool EsPagado    => Estado == "Pagado";

        public decimal PorcentajePagado
        {
            get
            {
                if (!MontoAprobado.HasValue || MontoAprobado <= 0 || !SaldoPendiente.HasValue)
                    return 0;
                decimal pagado = MontoAprobado.Value - SaldoPendiente.Value;
                return Math.Max(0, Math.Min(100, pagado / MontoAprobado.Value * 100));
            }
        }

        public string EmojiTipo
        {
            get
            {
                switch (TipoPrestamo)
                {
                    case "Hipoteca":  return "🏠";
                    case "Coche":     return "🚗";
                    case "Estudios":  return "🎓";
                    default:          return "💼";
                }
            }
        }
    }
}
