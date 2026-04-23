using BolsaDeTrabajo.Api.DTOs;

namespace Api_bolsatrabajo.Model.Dtos
{
    public class LoginRequestDto
    {
        public string Correo { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponseDto
    {
        public string Token { get; set; }
        public DateTime Expira { get; set; }
        public UsuarioDto Usuario { get; set; }
    }
    public class EmpresaLoginResponseDto
    {
        public string Rol { get; set; }
        public int Idusuario { get; set; }
        public string Token { get; set; }
        public DateTime Expira { get; set; }
        public EmpresaDto Empresa { get; set; }
    }
}
