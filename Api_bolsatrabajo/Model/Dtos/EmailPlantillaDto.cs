namespace Api_bolsatrabajo.Model.Dtos
{
    public class EmailPlantillaDto
    {
        public int IdPlantilla { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public bool Activa { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
