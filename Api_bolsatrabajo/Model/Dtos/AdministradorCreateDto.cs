namespace Api_bolsatrabajo.Model.Dtos
{
    public class AdministradorCreateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // plano, luego se hashea
        public string Rol { get; set; } = "Admin";
    }
}
