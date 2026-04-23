namespace Api_bolsatrabajo.Model.Dtos
{
    public class PostularVacanteRequestDto
    {
        public int IdUsuario { get; set; }
        public string? Mensaje { get; set; }
        public string? UrlCV { get; set; }
        public decimal? SalarioPretendido { get; set; }
        public List<VacantePreguntaRespuestaDto>? Respuestas { get; set; }

    }
    public class VacantePreguntaRespuestaDto
    {
        public int IdPregunta { get; set; }
        public string Pregunta { get; set; } = string.Empty;
        public string Respuesta { get; set; } = string.Empty;
    }

}
