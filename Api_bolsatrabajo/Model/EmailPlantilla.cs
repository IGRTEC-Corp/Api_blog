using System.ComponentModel.DataAnnotations;

namespace Api_bolsatrabajo.Model
{
    public class EmailPlantilla
    {
        [Key]
        public int IdPlantilla { get; set; }

        [Required]
        [MaxLength(80)]
        public string Codigo { get; set; } = string.Empty;
        // EJ: EMPRESA_REGISTRO_PENDIENTE

        [Required]
        [MaxLength(200)]
        public string Asunto { get; set; } = string.Empty;

        [Required]
        public string HtmlBase { get; set; } = string.Empty;

        public bool Activa { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
