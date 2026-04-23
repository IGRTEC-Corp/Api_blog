namespace Api_bolsatrabajo.Model.Dtos
{
    public class AdministradorPermisosDto
    {
        public int IdAdministrador { get; set; }
        public string Seccion { get; set; } = string.Empty;

        public bool ReadPerm { get; set; }
        public bool WritePerm { get; set; }
        public bool UpdatePerm { get; set; }
        public bool DeletePerm { get; set; }
    }
}
