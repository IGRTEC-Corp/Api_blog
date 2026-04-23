using BolsaDeTrabajo.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_bolsatrabajo.Model
{
    [Table("VacantePreguntas", Schema = "admin")]
    public class VacantePregunta
    {
        [Key]
        public int IdPregunta { get; set; }

        [Required]
        public int IdVacante { get; set; }

        [Required]
        [MaxLength(500)]
        public string Pregunta { get; set; } = string.Empty;

        public DateTime FechaAlta { get; set; }

        // 🔗 Navegaciones
        public EmpresaVacantes Vacante { get; set; } = null!;
        public ICollection<VacantePreguntaRespuesta> Respuestas { get; set; } = new List<VacantePreguntaRespuesta>();
    }
}
