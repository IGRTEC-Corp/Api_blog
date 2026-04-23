namespace Api_bolsatrabajo.Model.Dtos
{
    public class PagoUnicoEmpresaDto
    {
        public int IdUsuarioEmpresa { get; set; }
        public decimal Monto { get; set; } // MXN
        public string Concepto { get; set; } = "Recarga monedero";
    }
}
