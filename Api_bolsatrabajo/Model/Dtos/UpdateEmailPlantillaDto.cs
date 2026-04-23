namespace Api_bolsatrabajo.Model.Dtos
{
    public class UpdateEmailPlantillaDto
    {
        public int IdPlantilla { get; set; }
        public string Asunto { get; set; } = string.Empty;
        public string HtmlBase { get; set; } = string.Empty;
        public bool Activa { get; set; }
    }
}
