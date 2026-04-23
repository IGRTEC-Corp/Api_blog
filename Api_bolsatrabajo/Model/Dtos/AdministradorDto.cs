namespace Api_bolsatrabajo.Model.Dtos
{
    public class AdministradorDto
    {
        public int IdAdministrador { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Rol { get; set; } = "Admin";
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
