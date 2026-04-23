using BolsaDeTrabajo.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Api_bolsatrabajo.Model
{
    public class EmpresaVacantesPostulaciones
    {
        [Key]
        public int IdPostulacion { get; set; }
        public int IdVacante { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaPostulacion { get; set; }
        public string Estado { get; set; } = null!;
        public string? Mensaje { get; set; }
        public string? UrlCV { get; set; }
        public decimal? SalarioPretendido { get; set; }
        public bool EsFavorito { get; set; } = true;

        public ICollection<VacantePreguntaRespuesta> Respuestas { get; set; } = new List<VacantePreguntaRespuesta>();

        public virtual EmpresaVacantes IdVacanteNavigation { get; set; } = null!;
        public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
    }

}
