namespace Api_bolsatrabajo.Model.Dtos
{
    public class VacanteMetricasDto
    {
        public int IdVacante { get; set; }
        public int Visibilidad { get; set; }      // Veces que apareció en listados
        public int Vistas { get; set; }      // Veces que apareció en listados

        public int Resultados { get; set; }       // Postulaciones
        public int DiasActivo { get; set; }       // Días desde publicación
    }
}
