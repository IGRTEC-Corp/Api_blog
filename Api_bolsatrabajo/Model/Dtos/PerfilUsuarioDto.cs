using BolsaDeTrabajo.Api.DTOs;

namespace Api_bolsatrabajo.Model.Dtos
{
    public class PerfilUsuarioDto
    {
        public UsuarioDto Usuario { get; set; } = new UsuarioDto();

        public List<UsuarioFormacionDto> Formaciones { get; set; } = new();
        public List<UsuarioExperienciaDto> Experiencias { get; set; } = new();
        public List<UsuarioReferenciaLaboralDto> ReferenciasLaborales { get; set; } = new();
        public List<UsuarioCompetenciaDto> Competencias { get; set; } = new();
        public List<UsuarioCertificacionDto> Certificaciones { get; set; } = new();
    }

    public class CandidatoPerfilCompletoDto
    {
        public UsuarioDto Usuario { get; set; } = new();

        public List<UsuarioFormacionDto> Formaciones { get; set; } = new();
        public List<UsuarioExperienciaDto> Experiencias { get; set; } = new();
        public List<UsuarioReferenciaLaboralDto> ReferenciasLaborales { get; set; } = new();
        public List<UsuarioCompetenciaDto> Competencias { get; set; } = new();
        public List<UsuarioCertificacionDto> Certificaciones { get; set; } = new();
    }
}
