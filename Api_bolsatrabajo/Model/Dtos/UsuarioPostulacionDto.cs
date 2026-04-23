namespace Api_bolsatrabajo.Model.Dtos
{
    public class UsuarioPostulacionDto
    {
        public DateTime FechaPostulacion { get; set; }
        public string Empresa { get; set; } = "";
        public string Vacante { get; set; } = "";
        public string Estado { get; set; } = "";
    }
}
