namespace Api_bolsatrabajo.Model.Dtos
{
    public class PerfilCandidatoResumenDto
    {
        // Información general
        public string? TipoEmpleo { get; set; }
        public string? Modalidad { get; set; }
        public decimal? SalarioMin { get; set; }
        public decimal? SalarioMax { get; set; }

        // Formación (resumen)
        public string? NivelEstudios { get; set; }
        public string? Institucion { get; set; }
        public string? TituloObtenido { get; set; }

        // Experiencia (última o principal)
        public string? UltimaEmpresa { get; set; }
        public string? UltimoPuesto { get; set; }
        public int? AniosExperiencia { get; set; }

        // Extras útiles
        public bool PuedeViajar { get; set; }
        public bool PuedeReubicarse { get; set; }
    }
}
