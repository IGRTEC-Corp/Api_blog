using Api_bolsatrabajo.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("VacantePreguntaRespuestas", Schema = "admin")]
public class VacantePreguntaRespuesta
{
    [Key]
    public int IdRespuesta { get; set; }

    [Required]
    public int IdPregunta { get; set; }

    [Required]
    public int IdPostulacion { get; set; }

    [Required]
    public string Respuesta { get; set; } = string.Empty;

    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

    // 🔗 Navegaciones CORRECTAMENTE ANCLADAS
    [ForeignKey(nameof(IdPregunta))]
    public VacantePregunta Pregunta { get; set; } = null!;

    [ForeignKey(nameof(IdPostulacion))]
    public EmpresaVacantesPostulaciones Postulacion { get; set; } = null!;
}
