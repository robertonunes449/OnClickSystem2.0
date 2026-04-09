using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnClickSystem.Application.Services;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnClickSystem.Controllers
{
    [Authorize] // Só entra se estiver logado
    public class PerfilController : Controller
    {
        private readonly UsuarioService _usuarioService;
        private readonly FinanceiroService _financeiroService;

        public PerfilController(UsuarioService usuarioService, FinanceiroService financeiroService)
        {
            _usuarioService = usuarioService;
            _financeiroService = financeiroService;
        }

        // TELA INICIAL DO PERFIL
        public async Task<IActionResult> Index()
        {
            var userId = GetIdLogado();

            // Busca os dados do usuário para preencher os campos
            var usuario = await _usuarioService.ObterDetalhes(userId);

            if (usuario == null) return RedirectToAction("Index", "Login");

            // Busca o saldo atual para mostrar no topo da página
            ViewBag.SaldoDisponivel = await _financeiroService.CalcularSaldoDisponivel(userId);

            return View(usuario);
        }

        // QUANDO CLICA EM "SALVAR"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Salvar(Usuario model, string? NovaSenha)
        {
            var userId = GetIdLogado();

            // Chama o serviço para aplicar as regras (email único, criptografia, etc)
            var resultado = await _usuarioService.AtualizarPerfil(userId, model, NovaSenha);

            if (resultado.Sucesso)
            {
                TempData["Sucesso"] = resultado.Mensagem;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Erro"] = resultado.Mensagem;

                // Se der erro, precisamos recarregar o saldo para a tela não quebrar
                ViewBag.SaldoDisponivel = await _financeiroService.CalcularSaldoDisponivel(userId);
                return View("Index", model);
            }
        }

        // Pega o ID do usuário que está no Cookie
        private int GetIdLogado()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }
    }
}