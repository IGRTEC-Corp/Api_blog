namespace Api_bolsatrabajo.Model.Dtos
{
    public class EnviarWhatsAppDto
    {
        public string Telefono { get; set; }          // 527353466571
        public string Plantilla { get; set; }         // postulacion_recibida | notificacion
        public List<string>? Variables { get; set; } // opcional
    }
}
