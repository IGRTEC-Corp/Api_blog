namespace Api_bolsatrabajo.Model.Dtos
{
    public class EnviarEmailPlantillaDto
    {
        public string CodigoPlantilla { get; set; } = string.Empty;

        public string CorreoDestino { get; set; } = string.Empty;

        // Variables dinámicas
        // Ej: NombreEmpresa, TituloVacante, Estado, etc.
        public Dictionary<string, string> Variables { get; set; }
            = new Dictionary<string, string>();
    }
}
