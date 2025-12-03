namespace NawiWebAdmin.Models
{
    public class Evento
    {
        public long IdEvento { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        // CAMBIO 1: DateTime es más compatible que DateOnly para JSON
        public DateTime Fecha { get; set; } = DateTime.Now;

        // CAMBIO 2: String es perfecto para "HH:mm" que viene del input type="time"
        public string Hora { get; set; } = DateTime.Now.ToString("HH:mm");
        public string Lugar { get; set; }
        public string Costo { get; set; }
        public int IdCategoria { get; set; } // Para guardar (POST/PUT)
        public string? Categoria { get; set; } // Para mostrar en la lista (GET)
        public List<string> PublicoObjetivo { get; set; } = new();
        public string? FlyerUrl { get; set; }
        public string? Estado { get; set; }
    }
}