using BolsaDeTrabajo.Api.DTOs;

namespace Api_bolsatrabajo.Model.Dtos
{
    public class VacanteCandidatoDto
    {
        public int IdPostulacion { get; set; }
        public int IdVacante { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaPostulacion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string UrlCV { get; set; } = string.Empty;
        public bool EsFavorito { get; set; }
        public VacanteSimpleDto Vacante { get; set; }
        public List<VacantePreguntaRespuestaDto> Respuestas { get; set; }
    = new();
        public UsuarioDto Candidato { get; set; } = new UsuarioDto();
        public PerfilCandidatoResumenDto? Perfil { get; set; }

    }
}
