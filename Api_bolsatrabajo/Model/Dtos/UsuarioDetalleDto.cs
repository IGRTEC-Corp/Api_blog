using BolsaDeTrabajo.Api.DTOs;

namespace Api_bolsatrabajo.Model.Dtos
{
    public class UsuarioDetalleDto
    {
        public int IdUsuario { get; set; }
        public bool Activo { get; set; }

        public string Nombre { get; set; } = "";
        public string Apellidos { get; set; } = "";
        public string Correo { get; set; } = "";
        public string Telefono { get; set; } = "";

        public DateTime? FechaNacimiento { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public string Direccion { get; set; } = "";

        public string UrlCV { get; set; } = "";
        public string UrlPortafolio { get; set; } = "";
        public string UrlFotoPerfil { get; set; } = "";

        public string TipoEmpleo { get; set; } = "";
        public string Modalidad { get; set; } = "";

        public decimal SalarioMin { get; set; }
        public decimal SalarioMax { get; set; }

        public string Disponibilidad { get; set; } = "";
        public string Sector1 { get; set; } = "";
        public string Sector2 { get; set; } = "";

        public bool PuedeViajar { get; set; }
        public bool PuedeReubicarse { get; set; }
        public bool PuedeRotarTurnos { get; set; }

        public List<UsuarioFormacionDtos> Formaciones { get; set; } = new();
        public List<UsuarioExperienciaDto> Experiencias { get; set; } = new();
        public List<UsuarioCompetenciaDto> Competencias { get; set; } = new();
        public List<UsuarioCertificacionDto> Certificaciones { get; set; } = new();
        public List<UsuarioPostulacionDto> Postulaciones { get; set; } = new();
    }
}
