using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;
using OnClickSystem.Application.DTOs;

namespace OnClickSystem.Application.Services
{
    public class FinanceiroService : IFinanceiroService
    {
        private readonly OnClickContext _context;

        public FinanceiroService(OnClickContext context)
        {
            _context = context;
        }

        // 1. CALCULA O SALDO: Soma Comissões + Créditos - Débitos
        public async Task<decimal> CalcularSaldoDisponivel(int userId)
        {
            // 1. A fonte real de ganhos: Tabela de Comissões (os R$ 950)
            var totalComissoes = await _context.Comissoes
                .Where(c => c.ID_Beneficiario == userId)
                .SumAsync(c => (decimal?)c.Valor) ?? 0;

            // 2. A fonte real de gastos/saques: Tabela de Transações (Apenas Débitos)
            var totalDebitos = await _context.Transacoes
                .Where(t => t.ID_Usuario == userId && t.Tipo == "Debito")
                .SumAsync(t => (decimal?)t.Valor) ?? 0;

            // O saldo é o que ganhou na rede menos o que já saiu da conta
            return totalComissoes - totalDebitos;
        }


        public async Task<(bool Sucesso, string Mensagem)> ProcessarSolicitacaoSaque(int userId, SolicitacaoSaqueDTO dto)
        {
            System.Diagnostics.Debug.WriteLine($"---> Iniciando saque para user {userId}, valor {dto.Valor}");

            if (dto.Valor <= 0) return (false, "Valor inválido.");

            var saldo = await CalcularSaldoDisponivel(userId);
            if (dto.Valor > saldo) return (false, "Saldo insuficiente.");

            try
            {
                // 1. O Pedido de Saque
                var pedido = new SolicitacaoSaque
                {
                    ID_Usuario = userId,
                    Valor = dto.Valor,
                    TipoChave = dto.TipoChave,
                    ChavePix = dto.ChavePix,
                    DataSolicitacao = DateTime.Now,
                    Status = "Pendente"
                };
                _context.SolicitacoesSaque.Add(pedido);

                // 2. O Débito (Para baixar o saldo na hora)
                _context.Transacoes.Add(new Transacao
                {
                    ID_Usuario = userId,
                    Valor = dto.Valor,
                    Tipo = "Debito",
                    Data = DateTime.Now,
                    Descricao = $"Saque Solicitado (PIX: {dto.ChavePix})"
                });

                // 3. O Log (Para o Admin ver)
                _context.LogsSistema.Add(new LogSistema
                {
                    DataHora = DateTime.Now,
                    UsuarioResponsavel = "Sistema/Financeiro",
                    Acao = "Financeiro",
                    Detalhes = $"Saque solicitado pelo ID {userId}: R$ {dto.Valor:N2}"
                });

                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("---> Saque salvo com sucesso no banco!");

                return (true, "Solicitação enviada com sucesso! Seu saque está em análise.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"---> ERRO NO SAQUE: {ex.Message}");
                return (false, "Erro ao salvar no banco: " + ex.Message);
            }
        }
        public async Task<List<Transacao>> ObterExtrato(int userId)
        {
            return await _context.Transacoes
                .Where(t => t.ID_Usuario == userId)
                .OrderByDescending(t => t.Data)
                .ToListAsync();
        }

        public async Task<List<SolicitacaoSaque>> ObterMeusSaques(int userId)
        {
            return await _context.SolicitacoesSaque
                .Where(s => s.ID_Usuario == userId)
                .OrderByDescending(s => s.DataSolicitacao)
                .ToListAsync();
        }

        // Métodos obrigatórios da Interface
        public Task<List<SaquePendenteDTO>> ObterSaquesPendentesAsync() => Task.FromResult(new List<SaquePendenteDTO>());
        public Task AprovarSaqueAsync(int id) => Task.CompletedTask;
    }
}