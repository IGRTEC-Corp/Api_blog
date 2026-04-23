using System.ComponentModel.DataAnnotations;

namespace Api_bolsatrabajo.Model
{
    public class CorreoValidacion
    {
        [Key]
        public int IdCorreoValidacion { get; set; }
        public string Correo { get; set; }
        public Guid Codigo { get; set; }
        public string TipoUsuario { get; set; } // POSTULANTE / EMPRESA
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Validado { get; set; }
    }
}
