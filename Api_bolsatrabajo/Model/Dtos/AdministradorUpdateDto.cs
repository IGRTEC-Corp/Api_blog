namespace Api_bolsatrabajo.Model.Dtos
{
    public class AdministradorUpdateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Rol { get; set; } = "Admin";
        public bool Activo { get; set; } = true;

        // Campo opcional: solo se actualiza si se envía
        public string? PasswordNueva { get; set; }
    }
}
