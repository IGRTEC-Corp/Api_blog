namespace Api_bolsatrabajo.Model.Dtos
{
    public class DashboardEmpresaDto
    {
        public int TotalVacantes { get; set; }
        public int VacantesActivas { get; set; }
        public int TotalPostulaciones { get; set; }
        public int TotalVistas { get; set; }
        public int TotalFavoritos { get; set; }
        public decimal ConversionGlobal { get; set; }

        // Series para gráficas
        public List<KpiSerieDto> PostulacionesPorDia { get; set; }
        public List<KpiSerieDto> VistasPorDia { get; set; }

        // Detalle por vacante
        public List<VacanteDashboardDto> Vacantes { get; set; }
    }
    public class VacanteDashboardDto
    {
        public int IdVacante { get; set; }
        public string Titulo { get; set; }

        public int Vistas { get; set; }
        public int Favoritos { get; set; }
        public int Postulaciones { get; set; }

        public decimal Conversion { get; set; }
    }
    public class KpiSerieDto
    {
        public DateTime Fecha { get; set; }
        public int Total { get; set; }
    }


}
