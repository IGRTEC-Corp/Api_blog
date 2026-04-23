using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_bolsatrabajo.Model
{
    [Table("Administrador")]
    public class Administrador
    {
        [Key]
        public int IdAdministrador { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;  // email o username
        public string PasswordHash { get; set; } = string.Empty;
        public string Rol { get; set; } = "Admin";            // SuperAdmin / Admin
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
