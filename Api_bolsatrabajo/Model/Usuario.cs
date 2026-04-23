
using Api_bolsatrabajo.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BolsaDeTrabajo.Api.Models
{
    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required, StringLength(100)]
        public string? Nombre { get; set; } = null!;
        public string? Password { get; set; } = null!;
        public bool Activo { get; set; }
        public string? Telefono { get; set; } = null!;



        [Required, StringLength(100)]
        public string? Apellidos { get; set; } = null!;

        [Required, StringLength(150)]
        public string? Correo { get; set; } = null!;
        public DateTime? FechaRegistro { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        [StringLength(300)]
        public string? UrlCV { get; set; }

        [StringLength(300)]
        public string? UrlPortafolio { get; set; }

        [StringLength(300)]
        public string? UrlFotoPerfil { get; set; }

        // Preferencias de empleo
        [StringLength(50)]
        public string? TipoEmpleo { get; set; }

        [StringLength(50)]
        public string? Modalidad { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? SalarioMin { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? SalarioMax { get; set; }

        [StringLength(50)]
        public string? Disponibilidad { get; set; }

        [StringLength(100)]
        public string? Sector1 { get; set; }

        [StringLength(100)]
        public string? Sector2 { get; set; }

        public bool PuedeViajar { get; set; }
        public bool PuedeReubicarse { get; set; }
        public bool PuedeRotarTurnos { get; set; }

        // Navegación
        public virtual ICollection<EmpresaVacantesPostulaciones> EmpresaVacantesPostulaciones { get; set; }
    = new List<EmpresaVacantesPostulaciones>();

        public ICollection<UsuarioFormacion> Formaciones { get; set; } = new HashSet<UsuarioFormacion>();
        public ICollection<UsuarioExperiencia> Experiencias { get; set; } = new HashSet<UsuarioExperiencia>();
        public ICollection<UsuarioReferenciaLaboral> ReferenciasLaborales { get; set; } = new HashSet<UsuarioReferenciaLaboral>();
        public ICollection<UsuarioCompetencia> Competencias { get; set; } = new HashSet<UsuarioCompetencia>();
        public ICollection<UsuarioCertificacion> Certificaciones { get; set; } = new HashSet<UsuarioCertificacion>();
    }

    [Table("UsuarioFormacion")]
    public class UsuarioFormacion
    {
        [Key]
        public int IdFormacion { get; set; }

        public int IdUsuario { get; set; }

        [StringLength(50)]
        public string? NivelEstudios { get; set; }

        [StringLength(150)]
        public string? Institucion { get; set; }

        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        [StringLength(150)]
        public string? TituloObtenido { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public Usuario Usuario { get; set; } = null!;
    }

    [Table("UsuarioExperiencia")]
    public class UsuarioExperiencia
    {
        [Key]
        public int IdExperiencia { get; set; }

        public int IdUsuario { get; set; }

        [Required, StringLength(150)]
        public string Empresa { get; set; } = null!;

        [Required, StringLength(150)]
        public string Puesto { get; set; } = null!;

        public byte? MesEntrada { get; set; }   // 1–12
        public short? AnioEntrada { get; set; }

        public byte? MesSalida { get; set; }
        public short? AnioSalida { get; set; }

        [StringLength(150)]
        public string? Ubicacion { get; set; }

        public string? ResponsabilidadesYLogros { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public Usuario Usuario { get; set; } = null!;
    }

    [Table("UsuarioReferenciaLaboral")]
    public class UsuarioReferenciaLaboral
    {
        [Key]
        public int IdReferencia { get; set; }

        public int IdUsuario { get; set; }

        [Required, StringLength(150)]
        public string Nombre { get; set; } = null!;

        [StringLength(50)]
        public string? Telefono { get; set; }

        [StringLength(150)]
        public string? CorreoContacto { get; set; }
        public string? Empresa { get; set; }


        [StringLength(150)]
        public string? RelacionLaboral { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public Usuario Usuario { get; set; } = null!;
    }

    [Table("UsuarioCompetencia")]
    public class UsuarioCompetencia
    {
        [Key]
        public int IdCompetencia { get; set; }

        public int IdUsuario { get; set; }

        /// <summary>
        /// 'TECNICA', 'PERSONAL', 'IDIOMA'
        /// </summary>
        [Required, StringLength(20)]
        public string Tipo { get; set; } = null!;

        [Required, StringLength(150)]
        public string Nombre { get; set; } = null!;

        [StringLength(50)]
        public string? NivelDominio { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public Usuario Usuario { get; set; } = null!;
    }

    [Table("UsuarioCertificacion")]
    public class UsuarioCertificacion
    {
        [Key]
        public int IdCertificacion { get; set; }

        public int IdUsuario { get; set; }

        [Required, StringLength(150)]
        public string NombreCurso { get; set; } = null!;

        [StringLength(150)]
        public string? Institucion { get; set; }

        public DateTime? FechaCert { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public Usuario Usuario { get; set; } = null!;
    }
}