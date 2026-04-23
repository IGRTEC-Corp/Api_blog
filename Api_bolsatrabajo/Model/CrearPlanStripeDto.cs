namespace Api_bolsatrabajo.Model
{
    public class CrearPlanStripeDto
    {
        public string Nombre { get; set; } = string.Empty;   // Ej: Plan Básico
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }                  // Ej: 499
        public string Moneda { get; set; } = "mxn";           // mxn, usd
    }
}
