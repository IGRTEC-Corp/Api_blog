using System.ComponentModel.DataAnnotations;

namespace Api_bolsatrabajo.Model
{
    public class AdministradorPermisos
    {
        [Key]
        public int IdPermiso { get; set; }

        public int IdAdministrador { get; set; }

        public string Seccion { get; set; } = string.Empty;

        public bool ReadPerm { get; set; }

        public bool WritePerm { get; set; }

        public bool UpdatePerm { get; set; }

        public bool DeletePerm { get; set; }
    }
}
