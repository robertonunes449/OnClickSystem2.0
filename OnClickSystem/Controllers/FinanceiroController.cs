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
            // --- TRAVA DE SEGURANÇA ---
            if (User.FindFirst("StatusAtivo")?.Value == "False")
            {
                TempData["Info"] = "Você precisa adquirir um Kit de Ativação antes de acessar o Financeiro.";
                return RedirectToAction("Index", "Loja");
            }
            // --------------------------


            // Pega o ID do usuário logado de forma segura
            var userId = GetIdLogado();

            // Busca dados usando o Service
            ViewBag.SaldoTotal = await _financeiroService.CalcularSaldoDisponivel(userId);
            ViewBag.MeusSaques = await _financeiroService.ObterMeusSaques(userId);

            var extrato = await _financeiroService.ObterExtrato(userId);

            return View(extrato);
        }

        public IActionResult SolicitarSaque()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarSaque(SolicitacaoSaqueDTO pedido)
        {
            if (!ModelState.IsValid)
            {
                TempData["Erro"] = "Verifique os dados informados no formulário.";
                return View("SolicitarSaque", pedido);
            }

            var userId = GetIdLogado();

            var dto = new SolicitacaoSaqueDTO
            {
                // Preenche com os dados do pedido
                Valor = pedido.Valor,
                // adiciona outros campos se tiver
            };

            var resultado = await _financeiroService.ProcessarSolicitacaoSaque(userId, dto);

            if (resultado.Sucesso)
            {
                TempData["Sucesso"] = resultado.Mensagem;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Erro"] = resultado.Mensagem;
                return RedirectToAction("SolicitarSaque");
            }
        }
        private int GetIdLogado()
        {
            // Pega o ID de forma segura sem dar erro se estiver nulo
            var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Tenta converter para número inteiro
            if (int.TryParse(claimId, out int userId))
            {
                return userId;
            }

            return 0; // Se falhar, retorna 0 em vez de quebrar a página
        }
    }
}