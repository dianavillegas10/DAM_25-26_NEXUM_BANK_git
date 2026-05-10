// Models/Tarjeta.cs
using System;

namespace NexumApp.Models
{
    public class Tarjeta
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int CuentaId { get; set; }
        public string NumeroTarjeta { get; set; }
        public string TipoTarjeta { get; set; }  // Debito, Credito, Prepago
        public string Marca { get; set; }         // Visa, Mastercard, etc.
        public string NombreTitular { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaCaducidad { get; set; }
        public string CVV { get; set; }
        public decimal LimiteDiario { get; set; }
        public decimal LimiteMensual { get; set; }
        public decimal? LimiteCredito { get; set; }
        public decimal SaldoPendiente { get; set; }
        public bool EsPrincipal { get; set; }
        public bool Activa { get; set; }
        public bool Bloqueada { get; set; }
        public DateTime? FechaBloqueo { get; set; }

        // Propiedades de navegación
        public virtual Usuario Usuario { get; set; }
        public virtual CuentaBancaria Cuenta { get; set; }
    }
}