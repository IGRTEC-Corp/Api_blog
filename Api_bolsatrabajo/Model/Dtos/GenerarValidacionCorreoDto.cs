namespace Api_bolsatrabajo.Model.Dtos
{
    public class GenerarValidacionCorreoDto
    {

        public string Correo { get; set; }
        public string TipoUsuario { get; set; } // POSTULANTE / EMPRESA
    }
    public class ValidarCorreoDto
    {
        public Guid Token { get; set; }
    }

}
