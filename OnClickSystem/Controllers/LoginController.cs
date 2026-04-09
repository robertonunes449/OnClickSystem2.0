using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OnClickSystem.Application.Services;
using System;
using System.Threading.Tasks;

namespace OnClickSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly AuthService _authService;

        // Injeta apenas o serviço de Autenticação
        public LoginController(AuthService authService)
        {
            _authService = authService;
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
            // O Controller apenas delega a tarefa para o AuthService
            var resultado = await _authService.RealizarLogin(email, senha);

            if (resultado.Sucesso)
            {
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = lembrar,
                    ExpiresUtc = lembrar ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddMinutes(60)
                };

                // Cria o cookie no navegador
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    resultado.Principal,
                    authProperties
                );

                // --- NOVA REGRA: TRAVA DE ATIVAÇÃO ---
                // Verifica se no crachá está escrito que ele é "False" (Inativo)
                var statusAtivo = resultado.Principal.FindFirst("StatusAtivo")?.Value;
                if (statusAtivo == "False")
                {
                    TempData["Info"] = "Bem-vindo! Para liberar seu acesso ao painel, adquira seu Kit de Ativação.";
                    return RedirectToAction("Index", "Loja"); // Joga ele pra Loja!
                }
                // -------------------------------------

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);

                return RedirectToAction("Index", "Home");
            
        }

            // Se falhar
            ViewBag.Erro = resultado.Mensagem;
            ViewBag.ReturnUrl = returnUrl;
            return View("Index");
        }

        public async Task<IActionResult> Sair()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        public IActionResult AcessoNegado() => View();
    }
}