namespace Api_bolsatrabajo.Model.Dtos
{
    public class VacanteClicksDto
    {
        public int IdVacante { get; set; }
        public string Titulo { get; set; }
        public string Empresa { get; set; }
        public string CorreoEmpresa { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public string EstadoVacante { get; set; }
        public int TotalClicks { get; set; }
    }
}
