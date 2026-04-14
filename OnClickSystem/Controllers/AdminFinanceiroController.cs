using Microsoft.AspNetCore.Mvc;
using OnClickSystem.Application.Services;
using System.Threading.Tasks;

namespace OnClickSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminFinanceiroController : ControllerBase
    {
        private readonly IFinanceiroService _financeiroService;

        public AdminFinanceiroController(IFinanceiroService financeiroService)
        {
            _financeiroService = financeiroService;
        }

        // OBJETIVO DESKTOP: O Admin no Forms clica em "Aprovar Saque", o Forms chama essa rota.
        [HttpPut("aprovar-saque/{saqueId}")]
        public async Task<IActionResult> AprovarSaqueAdmin(int saqueId)
        {
            try
            {
                // A inteligência toda (atualizar saldo, mudar status) fica AQUI na Nuvem
                await _financeiroService.AprovarSaqueAsync(saqueId);
                return Ok(new { mensagem = "Saque aprovado com sucesso!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        // OBJETIVO DESKTOP: O Admin no Forms quer ver a lista de quem pediu saque.
        [HttpGet("saques-pendentes")]
        public async Task<IActionResult> ListarSaquesPendentes()
        {
            // Busca no banco e devolve pro Desktop mostrar no DataGridView
            var saques = await _financeiroService.ObterSaquesPendentesAsync();
            return Ok(saques);
        }
    }
}