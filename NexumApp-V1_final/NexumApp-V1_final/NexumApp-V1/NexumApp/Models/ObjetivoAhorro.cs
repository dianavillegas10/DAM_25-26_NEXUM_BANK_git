namespace NexumApp.Models
{
    /// <summary>
    /// Modelo para objetivos de ahorro. Preparado para cargar desde base de datos.
    /// </summary>
    public class ObjetivoAhorro
    {
        /// <summary>Nombre del objetivo (ej: "Viaje a Japón").</summary>
        public string Nombre { get; set; }

        /// <summary>Progreso actual (0-100).</summary>
        public decimal Progreso { get; set; }

        /// <summary>Imagen/icono (ruta o recurso). Opcional.</summary>
        public string ImagenPath { get; set; }

        /// <summary>Cantidad actual ahorrada.</summary>
        public decimal MontoActual { get; set; }

        /// <summary>Cantidad objetivo total.</summary>
        public decimal MontoObjetivo { get; set; }

        /// <summary>Fecha límite del objetivo (opcional).</summary>
        public System.DateTime? FechaObjetivo { get; set; }
    }
}
