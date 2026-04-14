using System.Collections.Generic;
using System.Threading.Tasks;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Application.DTOs; 

namespace OnClickSystem.Application.Services 
{
    public interface IFinanceiroService
    {
        Task<decimal> CalcularSaldoDisponivel(int userId);
        Task<(bool Sucesso, string Mensagem)> ProcessarSolicitacaoSaque(int userId, SolicitacaoSaqueDTO pedido);
        Task<List<Transacao>> ObterExtrato(int userId);
        Task<List<SolicitacaoSaque>> ObterMeusSaques(int userId);

        // As linhas que estavam faltando para os erros sumirem:
        Task<List<SaquePendenteDTO>> ObterSaquesPendentesAsync();
        Task AprovarSaqueAsync(int id);
    }
}