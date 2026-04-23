namespace Api_bolsatrabajo.Model.Dtos
{
    public class CrearPagoPlanDto
    {
        public int IdEmpresa { get; set; }
        public string Plan { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime? FechaPago { get; set; } // opcional
    }
}
