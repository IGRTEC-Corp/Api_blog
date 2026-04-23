using BolsaDeTrabajo.Api.Models;

namespace Api_bolsatrabajo.Model
{
    public class UsuarioVacanteFavorita
    {
        public int IdFavorito { get; set; }
        public int IdUsuario { get; set; }
        public int IdVacante { get; set; }
        public DateTime FechaRegistro { get; set; }

        public Usuario Usuario { get; set; }
        public EmpresaVacantes Vacante { get; set; }
    }
}
