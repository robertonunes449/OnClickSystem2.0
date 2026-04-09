using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using OnClickSystem.Infrastructure.Data;
using OnClickSystem.Application.Services;
using OnClickSystem.Domain.Entities; // ADICIONADO PARA O ASYNC

namespace OnClickSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly OnClickContext _context;
        private readonly RedeService _redeService; // INJETADO O SERVIÇO DE REDE

        public HomeController(OnClickContext context, RedeService redeService)
        {
            _context = context;
            _redeService = redeService;
        }

        // TRANSFORMADO EM ASYNC PARA BUSCAS MAIS RÁPIDAS
        public async Task<IActionResult> Index()
        {
            // --- TRAVA DE SEGURANÇA ---
            if (User.FindFirst("StatusAtivo")?.Value == "False")
            {
                return RedirectToAction("Index", "Loja");
            }
            // --------------------------

            ViewBag.NomeUsuario = User.Identity.Name;
            ViewBag.EhAdmin = User.IsInRole("Admin");
            ViewBag.TotalKits = await _context.Kits.CountAsync();

            // Valores padrão (para não quebrar a tela)
            ViewBag.NomeKitAtual = "Carregando...";
            ViewBag.MeusGanhos = 0.00m;
            ViewBag.MinhaRedeTotal = 0;
            ViewBag.Diretos = 0;
            ViewBag.SaquesPendentes = 0.00m;
            ViewBag.UltimasTransacoes = new List<Transacao>();

            if (User.Identity.IsAuthenticated)
            {
                var claimId = User.FindFirst(ClaimTypes.NameIdentifier);
                if (claimId != null)
                {
                    var idLogado = int.Parse(claimId.Value);

                    // --- 1. Descobrir qual Kit o utilizador tem ---
                    var ultimoPedido = await _context.Pedidos
                        .Include(p => p.Kit)
                        .Where(p => p.ID_Usuario == idLogado && p.Status == "Pago")
                        .OrderByDescending(p => p.DataPedido)
                        .FirstOrDefaultAsync();

                    ViewBag.NomeKitAtual = ultimoPedido != null ? ultimoPedido.Kit.Nome : (ViewBag.EhAdmin ? "Administrador" : "Membro Gratuito");

                    // --- 2. Calcular o Saldo Financeiro ---
                    var transacoes = await _context.Transacoes
                        .Where(t => t.ID_Usuario == idLogado)
                        .ToListAsync();

                    var entradas = transacoes.Where(t => t.Tipo == "Credito").Sum(t => t.Valor);
                    var saidas = transacoes.Where(t => t.Tipo == "Debito").Sum(t => t.Valor);
                    ViewBag.MeusGanhos = entradas - saidas;

                    // --- 3. Buscar Últimas Transações (Para a Tabela) ---
                    ViewBag.UltimasTransacoes = transacoes
                        .OrderByDescending(t => t.Data) // <-- AQUI FOI ALTERADO PARA 'Data'
                        .Take(5)
                        .ToList();

                    // --- 4. Calcular Saques Pendentes ---
                    ViewBag.SaquesPendentes = await _context.SolicitacoesSaque
                        .Where(s => s.ID_Usuario == idLogado && s.Status == "Pendente")
                        .SumAsync(s => s.Valor);

                    // --- 5. Calcular Tamanho Real da Equipa ---
                    var redeCompleta = await _redeService.ObterRedeAbaixo(idLogado, 10);
                    ViewBag.MinhaRedeTotal = redeCompleta.Count;
                    ViewBag.Diretos = redeCompleta.Count(u => u.NivelNaRede == 1);
                }
            }

            return View();
        }
    }
}