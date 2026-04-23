namespace Api_bolsatrabajo.Model.Dtos
{
    public class LoginGoogleRequestDto
    {
        public string Correo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string GoogleId { get; set; } = string.Empty;
    }
}
