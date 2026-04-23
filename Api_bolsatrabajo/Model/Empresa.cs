using Api_bolsatrabajo.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BolsaDeTrabajo.Api.Models
{
    // ======================================
    // EMPRESA
    // ======================================
    [Table("Empresa")]
    public class Empresa
    {
        [Key]
        public int IdEmpresa { get; set; }

        [Required, StringLength(200)]
        public string Nombre { get; set; } = null!;
        public string Password { get; set; } = null!;
        public decimal SaldoMonedero { get; set; } 

        public string Telefono { get; set; } = null!;
        public string? CedulaFiscalUrl { get; set; }


        [StringLength(100)]
        public string? Sector { get; set; }

        [StringLength(50)]
        public string? Tamano { get; set; }  // Pequeña / Mediana / Grande

        [StringLength(300)]
        public string? LogoUrl { get; set; }

        public string? QuienesSomos { get; set; }

        // Plan y pagos
        [StringLength(50)]
        public string? TipoPlan { get; set; } // Starter / Estandar / Premium

        public DateTime? FechaVencimientoPlan { get; set; }

        [StringLength(100)]
        public string? PasarelaPago { get; set; } // por ahora vacío / "0"

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public bool Activo { get; set; } = true;

        // =====================================================

        [StringLength(100)]
        public string StripeCustomerId { get; set; } = string.Empty;   // cus_xxx

        [StringLength(100)]
        public string StripeSubscriptionId { get; set; } = string.Empty; // sub_xxx

        public bool PlanActivo { get; set; } = false;

        public DateTime FechaInicioPlan { get; set; }
            = new DateTime(1900, 1, 1);

        public DateTime FechaFinPlan { get; set; }
            = new DateTime(1900, 1, 1);
        // Navegación
        public ICollection<UsuarioEmpresa> Usuarios { get; set; } = new HashSet<UsuarioEmpresa>();
        public ICollection<EmpresaPreguntasFrecuentes> PreguntasFrecuentes { get; set; } = new HashSet<EmpresaPreguntasFrecuentes>();
        public ICollection<EmpresaVacantes> Vacantes { get; set; } = new HashSet<EmpresaVacantes>();
    }

    // ======================================
    // USUARIO INTERNO DE EMPRESA
    // ======================================
    [Table("UsuarioEmpresa")]
    public class UsuarioEmpresa
    {
        [Key]
        public int IdUsuarioEmpresa { get; set; }

        public int IdEmpresa { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; } = null!;

        [Required, StringLength(100)]
        public string Apellidos { get; set; } = null!;

        [Required, StringLength(150)]
        public string Correo { get; set; } = null!;

        [Required, StringLength(300)]
        public string PasswordHash { get; set; } = null!;

        [StringLength(50)]
        public string Rol { get; set; } = "Reclutador"; // Admin, Reclutador, etc.

        public bool Activo { get; set; } = true;

        public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

        // Navegación
        [ForeignKey(nameof(IdEmpresa))]
        public Empresa Empresa { get; set; } = null!;

        public ICollection<UsuarioEmpresaPermisos> Permisos { get; set; } = new HashSet<UsuarioEmpresaPermisos>();
    }

    // ======================================
    // PERMISOS POR USUARIO-EMPRESA
    // ======================================
    [Table("UsuarioEmpresaPermisos")]
    public class UsuarioEmpresaPermisos
    {
        [Key]
        public int IdPermiso { get; set; }

        public int IdUsuarioEmpresa { get; set; }

        [Required, StringLength(100)]
        public string Seccion { get; set; } = null!; // Dashboard, Vacantes, Pagos, etc.

        public bool ReadPerm { get; set; }
        public bool WritePerm { get; set; }
        public bool UpdatePerm { get; set; }
        public bool DeletePerm { get; set; }

        [ForeignKey(nameof(IdUsuarioEmpresa))]
        public UsuarioEmpresa UsuarioEmpresa { get; set; } = null!;
    }

    // ======================================
    // PREGUNTAS FRECUENTES DE EMPRESA
    // ======================================
    [Table("EmpresaPreguntasFrecuentes")]
    public class EmpresaPreguntasFrecuentes
    {
        [Key]
        public int IdPregunta { get; set; }

        public int IdEmpresa { get; set; }

        [Required, StringLength(500)]
        public string Pregunta { get; set; } = null!;

        [Required, StringLength(1000)]
        public string Respuesta { get; set; } = null!;

        [ForeignKey(nameof(IdEmpresa))]
        public Empresa Empresa { get; set; } = null!;
    }

    // ======================================
    // VACANTES DE EMPRESA
    // ======================================
    [Table("EmpresaVacantes")]
    public class EmpresaVacantes
    {
        [Key]
        public int IdVacante { get; set; }
        public string EstadoVacante { get; set; } = "Activa";
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SueldoMax { get; set; }
        [Required]
        public TimeSpan HoraInicio { get; set; }
        public bool Destacado { get; set; }

        [Required]
        public TimeSpan HoraFin { get; set; }
        [Required, StringLength(30)]
        public string DiasLaborales { get; set; } = "Lunes-Viernes";

        public int IdEmpresa { get; set; }

        [Required, StringLength(200)]
        public string Titulo { get; set; } = null!;
        public string Ubicacion_estado { get; set; } = string.Empty;
        public string Ubicacion_Ciudad { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Sector { get; set; }

        [StringLength(50)]
        public string? Modalidad { get; set; }  // remoto, híbrido, presencial

        [Column(TypeName = "decimal(18,2)")]
        public decimal Sueldo { get; set; }

        [StringLength(50)]
        public string? TipoEmpleo { get; set; }  // tiempo completo, medio tiempo, etc.

        [StringLength(50)]
        public string? Nivel { get; set; }       // junior, senior...

        public string? Descripcion { get; set; }

        public string? Responsabilidades { get; set; }   // NUEVO
        public string? Requisitos { get; set; }          // NUEVO
        public bool EsBorrador { get; set; } = true;


        public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;

        public bool Activa { get; set; } = true;
        public ICollection<VacantePregunta> Preguntas { get; set; } = new List<VacantePregunta>();

        [ForeignKey(nameof(IdEmpresa))]
        public Empresa Empresa { get; set; } = null!;
        public virtual ICollection<EmpresaVacantesPostulaciones> EmpresaVacantesPostulaciones { get; set; }
    = new List<EmpresaVacantesPostulaciones>();

    }
}
