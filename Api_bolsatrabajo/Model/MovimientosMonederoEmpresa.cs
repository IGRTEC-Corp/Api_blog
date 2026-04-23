using BolsaDeTrabajo.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_bolsatrabajo.Model
{
    public class MovimientosMonederoEmpresa
    {
        [Key]
        public int IdMovimiento { get; set; }

        [Required]
        public int IdEmpresa { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

    

     
        public string? Referencia { get; set; }
        // Ej: Stripe SessionId / InvoiceId



        [Required]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        // ===============================
        // 🔗 Navegación
        // ===============================
        [ForeignKey(nameof(IdEmpresa))]
        public Empresa Empresa { get; set; }
    }
}
