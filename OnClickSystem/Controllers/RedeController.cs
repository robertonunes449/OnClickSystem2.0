using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;
using OnClickSystem.Application.Services; // Importante para usar o serviço
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;

namespace OnClickSystem.Controllers
{
    [Authorize]
    public class RedeController : Controller
    {
        private readonly OnClickContext _context;
        private readonly RedeService _redeService; // 1. Preparamos o Serviço aqui

        // 2. Injetamos o Serviço no construtor
        public RedeController(OnClickContext context, RedeService redeService)
        {
            _context = context;
            _redeService = redeService;
        }

        public async Task<IActionResult> Index()
        {
            // --- TRAVA DE SEGURANÇA ---
            if (User.FindFirst("StatusAtivo")?.Value == "False")
            {
                TempData["Info"] = "Você precisa de adquirir um Kit de Ativação antes de visualizar a sua Rede.";
                return RedirectToAction("Index", "Loja");
            }
            // --------------------------

            // 1. Identifica o utilizador logado
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null) return RedirectToAction("Index", "Login");

            int idLogado = int.Parse(idClaim.Value);

            // 2. CHAMA O NOVO MOTOR DE BUSCA (Busca até 10 níveis abaixo automaticamente)
            // Se no futuro quiser 20 níveis, basta mudar o número 10 para 20!
            var redeCompleta = await _redeService.ObterRedeAbaixo(idLogado, 10);

            // 3. Separa os Diretos (Nível 1) dos Indiretos (Nível 2 em diante)
            // Fazemos isto para que o seu ecrã atual continue a funcionar sem precisar de mexer no HTML
            var diretos = redeCompleta.Where(u => u.NivelNaRede == 1).ToList();
            var indiretos = redeCompleta.Where(u => u.NivelNaRede > 1).ToList();

            // --- ENVIO PARA A VIEW (Ecrã) ---
            ViewBag.TotalDiretos = diretos.Count;
            ViewBag.TotalIndiretos = indiretos.Count;
            ViewBag.ListaIndiretos = indiretos;

            return View(diretos);
        }
    }
}