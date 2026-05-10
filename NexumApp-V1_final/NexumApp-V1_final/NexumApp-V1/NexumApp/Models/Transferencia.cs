namespace NexumApp.Models
{
    /// <summary>
    /// Modelo que representa una transferencia bancaria en NexumApp.
    /// Corresponde a la tabla 'Transferencias' en la base de datos MySQL.
    /// </summary>
    /// <remarks>
    /// Una transferencia mueve dinero de una cuenta origen (del sistema)
    /// a una cuenta destino (puede ser interna o externa).
    /// 
    /// Flujo de una transferencia:
    /// 1. Usuario selecciona cuenta origen y escribe cuenta destino
    /// 2. Se valida saldo suficiente en cuenta origen
    /// 3. Se crea la transferencia con estado "Pendiente"
    /// 4. Se procesa: resta de origen, suma a destino (si es interna)
    /// 5. Se actualiza estado a "Completada" o "Fallida"
    /// </remarks>
    public class Transferencia
    {
        /// <summary>
        /// Identificador único de la transferencia (clave primaria auto-incremental).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la cuenta desde la que se envía el dinero (clave foránea).
        /// </summary>
        public int CuentaOrigenId { get; set; }

        /// <summary>
        /// Número de cuenta destino (puede ser interna o de otro banco).
        /// Se guarda como string para permitir cualquier formato.
        /// </summary>
        public string CuentaDestino { get; set; }

        /// <summary>
        /// Nombre del titular de la cuenta destino.
        /// Introducido por el usuario para referencia.
        /// </summary>
        public string NombreBeneficiario { get; set; }

        /// <summary>
        /// Cantidad de dinero transferida en euros.
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Fecha y hora en que se realizó la transferencia.
        /// </summary>
        public System.DateTime Fecha { get; set; }

        /// <summary>
        /// Descripción o motivo de la transferencia.
        /// Ejemplo: "Alquiler mensual", "Pago a proveedor".
        /// </summary>
        public string Concepto { get; set; }

        /// <summary>
        /// Estado actual de la transferencia.
        /// </summary>
        /// <value>
        /// Valores posibles: "Pendiente", "Completada", "Fallida", "Cancelada"
        /// </value>
        public string Estado { get; set; }

        /// <summary>
        /// Propiedad calculada que devuelve el monto formateado como moneda.
        /// </summary>
        public string MontoFormateado => Monto.ToString("C2");

        /// <summary>
        /// Cuenta bancaria desde la que se origina la transferencia.
        /// Navegación hacia la entidad CuentaBancaria relacionada.
        /// </summary>
        public virtual CuentaBancaria CuentaOrigen { get; set; }
    }
}
