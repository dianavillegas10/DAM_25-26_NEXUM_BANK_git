using System;

namespace NexumApp.Models
{
    public enum CategoriaBeca
    {
        Universitaria,
        Posgrado,
        FP,
        Digital,
        Deportiva,
        Arte
    }

    public enum EstadoSolicitud
    {
        Abierta,
        Cerrada,
        Resuelta
    }

    public class Beca
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public decimal Importe { get; set; }
        public CategoriaBeca Categoria { get; set; }
        public EstadoSolicitud Estado { get; set; }
        public DateTime? FechaCierre { get; set; }
        public string Requisitos { get; set; }
        public string EntidadConvocante { get; set; }
        public bool Destacada { get; set; }
        public int PlazasDisponibles { get; set; }
        public string DuracionTexto { get; set; }
    }
}
