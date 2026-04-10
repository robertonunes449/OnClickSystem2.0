using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;
using OnClickSystem.Application.ViewModels;

namespace OnClickSystem.Application.Services
{
    public class FinanceiroService : IFinanceiroService
    {
        private readonly OnClickContext _context;

        public FinanceiroService(OnClickContext context)
        {
            _context = context;
        }

        // 1. Calcula o saldo atual (Créditos - Débitos)
        public async Task<decimal> CalcularSaldoDisponivel(int userId)
        {
            var transacoes = await _context.Transacoes
                .Where(t => t.ID_Usuario == userId)
                .ToListAsync();

            var creditos = transacoes.Where(t => t.Tipo == "Credito").Sum(t => t.Valor);
            var debitos = transacoes.Where(t => t.Tipo == "Debito").Sum(t => t.Valor);

            return creditos - debitos;
        }

        // 2. Processa o pedido de saque com segurança (Transação de Banco de Dados)
        public async Task<(bool Sucesso, string Mensagem)> ProcessarSolicitacaoSaque(int userId, SolicitacaoSaque pedido)
        {
            // Validações básicas
            if (pedido.Valor <= 0) return (false, "Valor inválido.");

            // Verifica saldo
            var saldoAtual = await CalcularSaldoDisponivel(userId);
            if (pedido.Valor > saldoAtual) return (false, "Saldo insuficiente.");

            // Inicia uma transação segura (ou tudo funciona, ou nada muda)
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // A. Cria o registro do pedido
                pedido.ID_Usuario = userId;
                pedido.DataSolicitacao = DateTime.Now;
                pedido.Status = "Pendente";
                _context.SolicitacoesSaque.Add(pedido);

                // B. Cria o débito imediato no extrato
                var debito = new Transacao
                {
                    ID_Usuario = userId,
                    Valor = pedido.Valor,
                    Tipo = "Debito",
                    Data = DateTime.Now,
                    Descricao = $"Saque Solicitado via PIX ({pedido.ChavePix})"
                };
                _context.Transacoes.Add(debito);

                // Salva e confirma
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Saque solicitado! O valor foi reservado do seu saldo.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Erro ao processar saque: " + ex.Message);
            }
        }

        // 3. Busca o extrato completo
        public async Task<List<Transacao>> ObterExtrato(int userId)
        {
            return await _context.Transacoes
                .Where(t => t.ID_Usuario == userId)
                .OrderByDescending(t => t.Data)
                .ToListAsync();
        }

        // 4. Busca histórico de saques
        public async Task<List<SolicitacaoSaque>> ObterMeusSaques(int userId)
        {
            return await _context.SolicitacoesSaque
                .Where(s => s.ID_Usuario == userId)
                .OrderByDescending(s => s.DataSolicitacao)
                .ToListAsync();
        }
    }
}