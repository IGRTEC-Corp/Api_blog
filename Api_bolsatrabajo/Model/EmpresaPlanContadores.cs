using System.ComponentModel.DataAnnotations;

namespace Api_bolsatrabajo.Model
{
    public class EmpresaPlanContadores
    {
        [Key]
        public int IdEmpresa { get; set; }

        public string IdVacante { get; set; }

        public int VacantesPublicadas { get; set; } = 0;

        public int CvsDescargados { get; set; } = 0;
    }
}
