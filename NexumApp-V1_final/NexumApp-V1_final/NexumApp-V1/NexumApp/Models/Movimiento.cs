namespace NexumApp.Models
{
    /// <summary>
    /// Modelo que representa un movimiento bancario en NexumApp.
    /// Corresponde a la tabla 'Movimientos' en la base de datos MySQL.
    /// </summary>
    /// <remarks>
    /// Un movimiento es cualquier operación que afecta al saldo de una cuenta.
    /// Se registra automáticamente cuando ocurre:
    /// - Un ingreso de efectivo
    /// - Un retiro de efectivo
    /// - Una transferencia (enviada o recibida)
    /// - Un pago de servicio
    /// 
    /// Cada movimiento guarda el saldo antes y después de la operación,
    /// lo que permite auditar y reconstruir el historial completo.
    /// </remarks>
    public class Movimiento
    {
        /// <summary>
        /// Identificador único del movimiento (clave primaria auto-incremental).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la cuenta bancaria donde ocurrió el movimiento (clave foránea).
        /// </summary>
        public int CuentaId { get; set; }

        /// <summary>
        /// Tipo de operación realizada.
        /// </summary>
        /// <value>
        /// Valores posibles: "Ingreso", "Retiro", "Transferencia Enviada", 
        /// "Transferencia Recibida", "Pago Servicio", "Comisión"
        /// </value>
        public string TipoMovimiento { get; set; }

        /// <summary>
        /// Cantidad de dinero involucrada en el movimiento.
        /// Siempre positivo; el tipo indica si suma o resta.
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Fecha y hora en que se realizó el movimiento.
        /// </summary>
        public System.DateTime Fecha { get; set; }

        /// <summary>
        /// Descripción o referencia del movimiento.
        /// Ejemplo: "Ingreso en cajero", "Transferencia a Juan García".
        /// </summary>
        public string Concepto { get; set; }

        /// <summary>
        /// Saldo de la cuenta inmediatamente antes del movimiento.
        /// Permite verificar la integridad del historial.
        /// </summary>
        public decimal SaldoAnterior { get; set; }

        /// <summary>
        /// Saldo de la cuenta inmediatamente después del movimiento.
        /// Debe coincidir con SaldoAnterior +/- Monto según el tipo.
        /// </summary>
        public decimal SaldoPosterior { get; set; }

        /// <summary>
        /// Propiedad calculada que devuelve el monto formateado como moneda.
        /// </summary>
        public string MontoFormateado => Monto.ToString("C2");

        /// <summary>
        /// Cuenta bancaria donde se registró el movimiento.
        /// Navegación hacia la entidad CuentaBancaria relacionada.
        /// </summary>
        public virtual CuentaBancaria Cuenta { get; set; }
    }
}
