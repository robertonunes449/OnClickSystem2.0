using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnClickSystem.Application.Services;
using OnClickSystem.Domain.Entities;
using System.Security.Claims;
using System.Threading.Tasks;
using OnClickSystem.Application.DTOs;

namespace OnClickSystem.Controllers
{
    [Authorize]
    public class FinanceiroController : Controller
    {
        private readonly FinanceiroService _financeiroService;

        // O Controller agora só conhece o Service, não o Banco diretamente
        public FinanceiroController(FinanceiroService financeiroService)
        {
            _financeiroService = financeiroService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.FindFirst("StatusAtivo")?.Value == "False")
            {
                TempData["Info"] = "Você precisa adquirir um Kit de Ativação antes de acessar o Financeiro.";
                return RedirectToAction("Index", "Loja");
            }

            var userId = GetIdLogado();

            // 1. O saldo que o Service calcula (Comissões + Créditos - Débitos)
            decimal saldoTotal = await _financeiroService.CalcularSaldoDisponivel(userId);

            // 2. Buscamos o extrato para calcular os detalhes da tela
            var extrato = await _financeiroService.ObterExtrato(userId);

            // --- SINCRONIZAÇÃO COM A VIEW ---
            ViewBag.Saldo = saldoTotal; // Agora o nome bate com a View!
            ViewBag.GanhosTotais = extrato.Where(t => t.Tipo == "Credito").Sum(t => t.Valor);
            ViewBag.SaquesEfetuados = extrato.Where(t => t.Tipo == "Debito").Sum(t => t.Valor);
            // --------------------------------

            return View(extrato); // Enviamos o extrato como Model principal
        }

        public async Task<IActionResult> SolicitarSaque()
        {
            var userId = GetIdLogado();

            // Calculamos o saldo para exibir na tela de saque
            ViewBag.SaldoDisponivel = await _financeiroService.CalcularSaldoDisponivel(userId);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarSaque(SolicitacaoSaqueDTO pedido)
        {
            var userId = GetIdLogado();

            var resultado = await _financeiroService.ProcessarSolicitacaoSaque(userId, pedido);

            if (resultado.Sucesso)
            {
                // Esta "etiqueta" TempData precisa ser lida na Index
                TempData["Sucesso"] = resultado.Mensagem;
                return RedirectToAction("Index");
            }

            TempData["Erro"] = resultado.Mensagem;
            return RedirectToAction("SolicitarSaque");
        }

        private int GetIdLogado()
        {
            // Tenta pegar pelo NameIdentifier (Padrão)
            var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Se falhar, tenta pegar por uma claim chamada simplesmente "Id" ou "ID"
            if (string.IsNullOrEmpty(claimId))
                claimId = User.FindFirst("Id")?.Value ?? User.FindFirst("ID")?.Value;

            if (int.TryParse(claimId, out int userId))
            {
                return userId;
            }

            return 0;
        }
    }
}