using System;

namespace NexumApp.Models
{
    public class SolicitudBeca
    {
        public int       Id              { get; set; }
        public int       UsuarioId       { get; set; }
        public int       BecaId          { get; set; }
        public string    BecaTitulo      { get; set; }
        public decimal   BecaImporte     { get; set; }
        public DateTime  FechaSolicitud  { get; set; }
        public string    Estado          { get; set; }
        public string    Motivacion      { get; set; }
        public string    NumeroContrato  { get; set; }
        public DateTime? FechaResolucion { get; set; }

        public string NombreUsuario          { get; set; }
        public string EmailUsuario           { get; set; }
        public string CentroEducativo        { get; set; }
        public string Titulacion             { get; set; }
        public string AnioAcademico          { get; set; }
        public string NotaMediaODescripcion  { get; set; }

        public bool EsPendiente => Estado == "Pendiente";
        public bool EsAprobada  => Estado == "Aprobada";
        public bool EsDenegada  => Estado == "Denegada";
    }
}
