namespace Api_bolsatrabajo.Model.Dtos
{
    public class SesionUsuario
    {
        public int IdUsuario { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;
        public DateTime Expira { get; set; }
    }
}
