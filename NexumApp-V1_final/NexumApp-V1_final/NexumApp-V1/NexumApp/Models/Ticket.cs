using System;

namespace NexumApp.Models
{
    public class Ticket
    {
        // Datos básicos del Ticket
        public int Id { get; set; }
        public string Asunto { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; } // Pendiente, En proceso, Resuelto, Cerrado
        public string Prioridad { get; set; } // Baja, Media, Alta, Urgente
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }

        // Datos del Usuario que creó el ticket
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; }
        public string UsuarioEmail { get; set; }

        // Datos de la respuesta del Administrador
        public string RespuestaAdmin { get; set; } // <--- SOLO UNA VEZ
        public DateTime? FechaRespuesta { get; set; }
        public int? AdminId { get; set; }
        public string AdminNombre { get; set; }
    }

    public class TicketRequest
    {
        public string Asunto { get; set; }
        public string Descripcion { get; set; }
        public string Prioridad { get; set; }
    }
}