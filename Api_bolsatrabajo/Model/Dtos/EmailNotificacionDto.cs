namespace Api_bolsatrabajo.Model.Dtos
{
    public class EmailNotificacionDto
    {
        public int IdNotificacion { get; set; }
        public string CodigoPlantilla { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string CorreoDestino { get; set; } = string.Empty;
        public bool Enviado { get; set; }
        public DateTime FechaEnvio { get; set; }
    }
}
