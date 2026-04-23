namespace Api_bolsatrabajo.Model.Dtos
{
    public class UsuarioFormacionDtos
    {
        public string NivelEstudios { get; set; } = "";
        public string Institucion { get; set; } = "";
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string TituloObtenido { get; set; } = "";
    }
}
