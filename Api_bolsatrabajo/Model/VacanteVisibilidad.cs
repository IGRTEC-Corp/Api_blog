using BolsaDeTrabajo.Api.Models;

namespace Api_bolsatrabajo.Model
{
    public class VacanteVisibilidad
    {
        public int Id { get; set; }

        public int IdVacante { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        // Relación opcional (si quieres usar Include)
    }
}
