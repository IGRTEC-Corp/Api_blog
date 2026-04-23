namespace BolsaDeTrabajo.Api.DTOs
{
    public class EmpresaDto
    {
        public int IdEmpresa { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Password { get; set; } = null!;
        public string? CedulaFiscalUrl { get; set; }
        public decimal SaldoMonedero { get; set; }

        public string Sector { get; set; } = string.Empty;
        public string Tamano { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string QuienesSomos { get; set; } = string.Empty;
        public string Telefono { get; set; } = null!;

        public string TipoPlan { get; set; } = "Starter";   // Starter / Estandar / Premium
        public DateTime? FechaVencimientoPlan { get; set; }
        public string PasarelaPago { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
    }

    public class EmpresaPreguntaDto
    {
        public int IdPregunta { get; set; }
        public int IdEmpresa { get; set; }
        public string Pregunta { get; set; } = string.Empty;
        public string Respuesta { get; set; } = string.Empty;
    }

    public class EmpresaVacanteDto
    {
        public int IdVacante { get; set; }
        public int IdEmpresa { get; set; }
        public bool EsBorrador { get; set; }
        public string EstadoVacante { get; set; } = "Activa";

        public string NombreEmpresa { get; set; } = string.Empty;
        public decimal? SueldoMax { get; set; }

        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public bool Destacado { get; set; }

        public string DiasLaborales { get; set; } = string.Empty;


        public string Titulo { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public string Modalidad { get; set; } = string.Empty;
        public string Ubicacion_estado { get; set; } = string.Empty;   // NUEVO
        public string Ubicacion_Ciudad { get; set; } = string.Empty;   // NUEVO

        public string? Responsabilidades { get; set; }   // NUEVO
        public string? Requisitos { get; set; }          // NUEVO

        public decimal Sueldo { get; set; }
        public string TipoEmpleo { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaPublicacion { get; set; }
        public bool Activa { get; set; }
        public List<VacantePreguntaDto> Preguntas { get; set; } = new();

    }

    public class VacantePreguntaDto
    {
        public int IdPregunta { get; set; }
        public string Pregunta { get; set; } = string.Empty;
    }

    public class UsuarioEmpresaDto
    {
        public int IdUsuarioEmpresa { get; set; }
        public int IdEmpresa { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty; // ya encriptado

        public string Rol { get; set; } = "Reclutador"; // Admin, Reclutador, etc.
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
    }

    public class UsuarioEmpresaPermisoDto
    {
        public int IdPermiso { get; set; }
        public int IdUsuarioEmpresa { get; set; }

        public string Seccion { get; set; } = string.Empty;
        public bool ReadPerm { get; set; }
        public bool WritePerm { get; set; }
        public bool UpdatePerm { get; set; }
        public bool DeletePerm { get; set; }
    }

    // Para cambiar plan / pagos rápidamente
    public class EmpresaPlanDto
    {
        public string TipoPlan { get; set; } = string.Empty;
        public DateTime? FechaVencimientoPlan { get; set; }
        public string PasarelaPago { get; set; } = string.Empty;
    }

    // Perfil completo de empresa (similar a PerfilUsuarioDto)
    public class EmpresaPerfilDto
    {
        public EmpresaDto Empresa { get; set; } = new EmpresaDto();
        public List<UsuarioEmpresaDto> Usuarios { get; set; } = new();
        public List<UsuarioEmpresaPermisoDto> Permisos { get; set; } = new();
        public List<EmpresaVacanteDto> Vacantes { get; set; } = new();
        public List<EmpresaPreguntaDto> PreguntasFrecuentes { get; set; } = new();
    }
}
