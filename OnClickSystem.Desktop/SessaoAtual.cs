using OnClickSystem.Domain.Entities;

namespace OnClickSystem.Desktop
{
    public static class SessaoAtual
    {
        // O cofre onde vamos guardar a pessoa que fez login
        public static Usuario UsuarioLogado { get; set; }
    }
}