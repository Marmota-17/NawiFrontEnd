namespace NawiWebAdmin.Models
{
    public class Evento
    {
        public long IdEvento { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateOnly Fecha { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public TimeOnly Hora { get; set; } = TimeOnly.FromDateTime(DateTime.Now);
        public string Lugar { get; set; }
        public string Costo { get; set; }
        public int IdCategoria { get; set; }
        public List<string> PublicoObjetivo { get; set; } = new();
        public string? FlyerUrl { get; set; }
        public string? Estado { get; set; }
    }
}