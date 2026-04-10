using Microsoft.AspNetCore.Mvc;
using OnClickSystem.Application.Services;
using OnClickSystem.Domain.Entities;
using System.Threading.Tasks;

namespace OnClickSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinanceiroController : ControllerBase
    {
        private readonly IFinanceiroService _financeiroService;

        // Injeção de dependência do serviço financeiro
        public FinanceiroController(IFinanceiroService financeiroService)
        {
            _financeiroService = financeiroService;
        }

        // Endpoint para solicitar saque (Chamado pelo Web)
        [HttpPost("solicitar-saque/{userId}")]
        public async Task<IActionResult> SolicitarSaque(int userId, [FromBody] SolicitacaoSaque pedido)
        {
            var resultado = await _financeiroService.ProcessarSolicitacaoSaque(userId, pedido);

            if (resultado.Sucesso)
            {
                return Ok(new { mensagem = resultado.Mensagem });
            }

            // Se der erro de regra de negócio (ex: saldo insuficiente), retorna HTTP 400
            return BadRequest(new { erro = resultado.Mensagem });
        }

        // Endpoint para ver saldo no Dashboard (Chamado pelo Web ou Desktop)
        [HttpGet("saldo/{userId}")]
        public async Task<IActionResult> ObterSaldo(int userId)
        {
            var saldo = await _financeiroService.CalcularSaldoDisponivel(userId);
            return Ok(new { SaldoAtual = saldo });
        }

        // Endpoint para ver histórico de saques (Chamado pelo Desktop/Admin)
        [HttpGet("saques/{userId}")]
        public async Task<IActionResult> ObterSaques(int userId)
        {
            var saques = await _financeiroService.ObterMeusSaques(userId);
            return Ok(saques);
        }
    }
}