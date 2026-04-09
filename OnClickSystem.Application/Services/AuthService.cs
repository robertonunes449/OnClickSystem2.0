using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OnClickSystem.Application.ViewModels;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using BCrypt.Net;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;

namespace OnClickSystem.Application.Services
{
    public class AuthService
    {
        private readonly OnClickContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor; // 1. Cria a variável para a internet

        // 2. Modifica o construtor para receber ambos
        public AuthService(OnClickContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool Sucesso, ClaimsPrincipal? Principal, string Mensagem)> RealizarLogin(string email, string senha)
        {
            // --- NOVO: CAPTURAR DADOS DO INVASOR/UTILIZADOR ---
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "IP Desconhecido";
            var navegador = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Navegador Desconhecido";
            // --------------------------------------------------

            // 1. Busca utilizador pelo email
            var emailNormalizado = email.ToLower();
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == emailNormalizado);

            if (usuario == null)
            {
                var logFalhaEmail = new LogSistema
                {
                    DataHora = DateTime.Now,
                    UsuarioResponsavel = email,
                    Acao = "Falha de Login",
                    // --- REFINAMENTO: ADICIONAMOS O IP E NAVEGADOR AQUI ---
                    Detalhes = $"Tentativa falhada (E-mail não encontrado). IP: {ip} | Navegador: {navegador}"
                };
                _context.LogsSistema.Add(logFalhaEmail);
                await _context.SaveChangesAsync();

                return (false, null, "Usuário ou senha incorretos.");
            }

            // 2. VERIFICAÇÃO SEGURA DE SENHA
            bool senhaValida = false;
            try
            {
                senhaValida = BCrypt.Net.BCrypt.Verify(senha, usuario.Senha);
            }
            catch
            {
                if (usuario.Senha == senha)
                {
                    senhaValida = true;
                    usuario.Senha = BCrypt.Net.BCrypt.HashPassword(senha);
                    _context.Usuarios.Update(usuario);
                    await _context.SaveChangesAsync();
                }
            }

            if (!senhaValida)
            {
                var logFalhaSenha = new LogSistema
                {
                    DataHora = DateTime.Now,
                    UsuarioResponsavel = email,
                    Acao = "Falha de Login",
                    // --- REFINAMENTO: ADICIONAMOS O IP E NAVEGADOR AQUI ---
                    Detalhes = $"Tentativa falhada (Senha Incorreta). IP: {ip} | Navegador: {navegador}"
                };
                _context.LogsSistema.Add(logFalhaSenha);
                await _context.SaveChangesAsync();

                return (false, null, "Usuário ou senha incorretos.");
            }

            // LOG DE SUCESSO REFINADO
            var logSucesso = new LogSistema
            {
                DataHora = DateTime.Now,
                UsuarioResponsavel = usuario.Email,
                Acao = "Login Realizado",
                Detalhes = $"Acesso com sucesso. IP: {ip}"
            };
            _context.LogsSistema.Add(logSucesso);
            await _context.SaveChangesAsync();

            // 4. Cria a Identidade do Utilizador
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, usuario.Nome ?? "Usuário"),
        new Claim(ClaimTypes.NameIdentifier, usuario.ID.ToString()),
        new Claim(ClaimTypes.Email, usuario.Email),
        new Claim(ClaimTypes.Role, usuario.Perfil ?? "Comum"),
        new Claim("StatusAtivo", usuario.Ativo.ToString())
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            return (true, principal, "Login realizado com sucesso!");
        }

        // Método auxiliar para criar a Criptografia (usado no Cadastro)
        public string GerarHashSenha(string senhaPura)
        {
            return BCrypt.Net.BCrypt.HashPassword(senhaPura);
        }
    }
}