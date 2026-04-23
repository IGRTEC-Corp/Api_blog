using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_bolsatrabajo.Model
{
    public class PagosPlanes
    {
        [Key]
        public int IdPago { get; set; }

        [Required]
        public int IdEmpresa { get; set; }

        [Required]
        [MaxLength(30)]
        public string PlanNombre { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }

        public DateTime FechaPago { get; set; } = DateTime.Now;
    }
}
