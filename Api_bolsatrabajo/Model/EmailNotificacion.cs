using System.ComponentModel.DataAnnotations;

namespace Api_bolsatrabajo.Model
{
    public class EmailNotificacion
    {
        [Key]
        public int IdNotificacion { get; set; }

        [Required]
        [MaxLength(80)]
        public string CodigoPlantilla { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Mensaje { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CorreoDestino { get; set; } = string.Empty;

        public bool Enviado { get; set; } = false;

        [MaxLength(500)]
        public string? ErrorEnvio { get; set; }

        public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
    }
}
