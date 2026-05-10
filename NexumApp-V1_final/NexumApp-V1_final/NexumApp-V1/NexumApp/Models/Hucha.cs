namespace NexumApp.Models
{
    public class Hucha
    {
        public int     Id            { get; set; }
        public int     UsuarioId     { get; set; }
        public string  Nombre        { get; set; }
        public string  Emoji         { get; set; } = "🐷";
        public decimal SaldoActual   { get; set; }
        public decimal MetaObjetivo  { get; set; }
        public string  ColorHex      { get; set; } = "#3B82F6";
        public bool    Activa        { get; set; } = true;

        public int Progreso =>
            MetaObjetivo > 0
                ? (int)System.Math.Min(100, SaldoActual * 100 / MetaObjetivo)
                : 0;
    }
}
