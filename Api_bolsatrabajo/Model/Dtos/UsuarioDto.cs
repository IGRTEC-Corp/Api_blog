namespace BolsaDeTrabajo.Api.DTOs
{
    // ============
    //  DTO BASE USUARIO
    // ============

    public class UsuarioDto
    {
        public int IdUsuario { get; set; }

        // Datos personales
        public string Password { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string Telefono { get; set; } = null!;

        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
        public DateTime? FechaRegistro { get; set; }

        public string? Direccion { get; set; }
        public string? UrlCV { get; set; }
        public string? UrlPortafolio { get; set; }
        public string? UrlFotoPerfil { get; set; }

        // Preferencias de empleo
        public string? TipoEmpleo { get; set; }
        public string? Modalidad { get; set; }
        public decimal? SalarioMin { get; set; }
        public decimal? SalarioMax { get; set; }
        public string? Disponibilidad { get; set; }
        public string? Sector1 { get; set; }
        public string? Sector2 { get; set; }
        public bool PuedeViajar { get; set; }
        public bool PuedeReubicarse { get; set; }
        public bool PuedeRotarTurnos { get; set; }
    }

    // ============
    //  DTO FORMACIÓN ACADÉMICA
    // ============

    public class UsuarioFormacionDto
    {
        public int IdFormacion { get; set; }
        public int IdUsuario { get; set; }

        public string? NivelEstudios { get; set; }
        public string? Institucion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? TituloObtenido { get; set; }
    }

    // ============
    //  DTO EXPERIENCIA LABORAL
    // ============

    public class UsuarioExperienciaDto
    {
        public int IdExperiencia { get; set; }
        public int IdUsuario { get; set; }

        public string Empresa { get; set; } = string.Empty;
        public string Puesto { get; set; } = string.Empty;
        public byte? MesEntrada { get; set; }
        public short? AnioEntrada { get; set; }
        public byte? MesSalida { get; set; }
        public short? AnioSalida { get; set; }
        public string? Ubicacion { get; set; }
        public string? ResponsabilidadesYLogros { get; set; }
    }

    // ============
    //  DTO REFERENCIAS LABORALES
    // ============

    public class UsuarioReferenciaLaboralDto
    {
        public int IdReferencia { get; set; }
        public int IdUsuario { get; set; }
        public string Empresa { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? CorreoContacto { get; set; }
        public string? RelacionLaboral { get; set; }
    }

    // ============
    //  DTO COMPETENCIAS (HABILIDADES + IDIOMAS)
    // ============

    public class UsuarioCompetenciaDto
    {
        public int IdCompetencia { get; set; }
        public int IdUsuario { get; set; }

        /// <summary>
        /// 'TECNICA', 'PERSONAL', 'IDIOMA'
        /// </summary>
        public string Tipo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;
        public string? NivelDominio { get; set; }
    }

    // ============
    //  DTO CERTIFICACIONES / CURSOS
    // ============

    public class UsuarioCertificacionDto
    {
        public int IdCertificacion { get; set; }
        public int IdUsuario { get; set; }

        public string NombreCurso { get; set; } = string.Empty;
        public string? Institucion { get; set; }
        public DateTime? FechaCert { get; set; }
    }

    // =====================================
    //  DTO GENERAL: PERFIL COMPLETO DE USUARIO
    // =====================================

 
}
