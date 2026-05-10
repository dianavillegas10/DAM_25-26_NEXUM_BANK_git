using System.Collections.Generic;

namespace NexumApp.Models
{
    /// <summary>
    /// Modelo que representa una cuenta bancaria en el sistema NexumApp.
    /// Corresponde a la tabla 'CuentasBancarias' en la base de datos MySQL.
    /// </summary>
    /// <remarks>
    /// Cada usuario puede tener múltiples cuentas bancarias.
    /// Las cuentas son el centro de las operaciones financieras:
    /// - Reciben y envían transferencias
    /// - Registran movimientos (ingresos, retiros)
    /// - Mantienen el saldo actualizado
    /// </remarks>
    public class CuentaBancaria
    {
        /// <summary>
        /// Identificador único de la cuenta (clave primaria auto-incremental).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del usuario propietario de la cuenta (clave foránea a Usuarios).
        /// </summary>
        public int UsuarioId { get; set; }

        /// <summary>
        /// Número de cuenta bancaria (formato IBAN o interno).
        /// Es el identificador público usado para recibir transferencias.
        /// </summary>
        public string NumeroCuenta { get; set; }

        /// <summary>
        /// Tipo de cuenta: "Corriente", "Ahorro", "Nómina", etc.
        /// Determina las características y límites de operación.
        /// </summary>
        public string TipoCuenta { get; set; }

        /// <summary>
        /// Saldo actual de la cuenta en euros.
        /// Se actualiza automáticamente con cada movimiento.
        /// </summary>
        public decimal Saldo { get; set; }

        /// <summary>
        /// Fecha en que se abrió/creó la cuenta.
        /// </summary>
        public System.DateTime FechaApertura { get; set; }

        /// <summary>
        /// Indica si la cuenta está activa y operativa.
        /// </summary>
        /// <value>
        /// True: La cuenta puede realizar y recibir operaciones.
        /// False: La cuenta está bloqueada o cerrada.
        /// </value>
        public bool Activa { get; set; }

        /// <summary>
        /// Propiedad calculada que devuelve el saldo formateado como moneda.
        /// </summary>
        /// <remarks>
        /// Usa el formato "C2" que aplica el símbolo de moneda local
        /// y 2 decimales. Ejemplo: "1.234,56 €"
        /// </remarks>
        public string SaldoFormateado => Saldo.ToString("C2");

        /// <summary>
        /// Usuario propietario de la cuenta.
        /// Navegación hacia la entidad Usuario relacionada.
        /// </summary>
        public virtual Usuario Usuario { get; set; }

        /// <summary>
        /// Colección de movimientos asociados a esta cuenta.
        /// Incluye ingresos, retiros y cargos por transferencias.
        /// </summary>
        public virtual ICollection<Movimiento> Movimientos { get; set; }

        /// <summary>
        /// Colección de transferencias enviadas desde esta cuenta.
        /// </summary>
        public virtual ICollection<Transferencia> TransferenciasEnviadas { get; set; }
    }
}
