using System.Collections.Generic;
using System.Threading.Tasks;
using OnClickSystem.Domain.Entities;

namespace OnClickSystem.Application.Services
{
    public interface IFinanceiroService
    {
        Task<decimal> CalcularSaldoDisponivel(int userId);
        Task<(bool Sucesso, string Mensagem)> ProcessarSolicitacaoSaque(int userId, SolicitacaoSaque pedido);
        Task<List<Transacao>> ObterExtrato(int userId);
        Task<List<SolicitacaoSaque>> ObterMeusSaques(int userId);
    }
}