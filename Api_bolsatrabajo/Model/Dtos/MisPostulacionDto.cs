namespace Api_bolsatrabajo.Model.Dtos
{
    public class MisPostulacionDto
    {
        public int IdPostulacion { get; set; }
        public int IdVacante { get; set; }
        public string Titulo { get; set; }
        public string Sector { get; set; }
        public string Modalidad { get; set; }
        public string Sueldo { get; set; }
        public string Nivel { get; set; }
        public string TipoEmpleo { get; set; }
        public string Estado { get; set; }
        public string Empresa { get; set; }
    }
}
