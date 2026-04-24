using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OnClickSystem.Application.Services;
using OnClickSystem.Infrastructure.Data;
using OnClickSystem.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace OnClickSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly AuthService _authService;
        private readonly OnClickContext _context;

        public LoginController(AuthService authService, OnClickContext context)
        {
            _authService = authService;
            _context = context;
        }

        public IActionResult Index(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Entrar(string email, string senha, bool lembrar, string returnUrl = null)
        {
            var resultado = await _authService.RealizarLogin(email, senha);

            if (resultado.Sucesso)
            {
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = lembrar,
                    ExpiresUtc = lembrar ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddMinutes(60)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, resultado.Principal, authProperties);

                // --- LOG DE SEGURANÇA ---
                await RegistrarLog("Segurança", $"Login realizado com sucesso para o e-mail: {email}", email);

                var statusAtivo = resultado.Principal.FindFirst("StatusAtivo")?.Value;
                if (statusAtivo == "False")
                {
                    TempData["Info"] = "Bem-vindo! Para liberar seu acesso ao painel, adquira seu Kit de Ativação.";
                    return RedirectToAction("Index", "Loja");
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Erro = resultado.Mensagem;
            return View("Index");
        }

        public async Task<IActionResult> Sair()
        {
            if (User.Identity.IsAuthenticated)
                await RegistrarLog("Segurança", $"Usuário encerrou a sessão.");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        private async Task RegistrarLog(string categoria, string detalhes, string userEmail = null)
        {
            var log = new LogSistema
            {
                DataHora = DateTime.Now,
                UsuarioResponsavel = userEmail ?? User.Identity.Name ?? "Sistema",
                Acao = categoria,
                Detalhes = detalhes
            };
            _context.LogsSistema.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}