namespace Api_bolsatrabajo.Model.Dtos
{
    public class AdministradorAuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}
