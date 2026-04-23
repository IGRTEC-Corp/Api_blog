using BolsaDeTrabajo.Api.Models;

namespace Api_bolsatrabajo.Model
{
    public class VacanteVistasDetalle
    {
        public int Id { get; set; }
        public int IdVacante { get; set; }
        public int? IdUsuario { get; set; }  // opcional
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

    }
}
