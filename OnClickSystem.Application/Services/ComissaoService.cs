
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Application.ViewModels;
using OnClickSystem.Infrastructure.Data;

namespace OnClickSystem.Application.Services
{
    public class ComissaoService
    {
        private readonly OnClickContext _context;

        public ComissaoService(OnClickContext context)
        {
            _context = context;
        }

        public async Task ProcessarComissao(int idPedido, int idComprador, decimal valorVenda)
        {
            // CONFIGURAÇÃO MANUAL (Substitui a consulta ao banco de dados)
            // Chave: Nível, Valor: Porcentagem
            var regrasFixas = new Dictionary<int, decimal>
            {
                { 1, 10.00m }, // 10% para o patrocinador direto
                { 2, 5.00m },  // 5% para o nível 2
                { 3, 2.00m }   // 2% para o nível 3
                // Adicione quantos níveis desejar aqui
            };

            var comprador = await _context.Usuarios.FindAsync(idComprador);
            if (comprador == null || comprador.ID_Patrocinador == null) return;

            int? idAtualParaReceber = comprador.ID_Patrocinador;

            // Ordenamos as chaves para garantir que o pagamento siga a ordem de nível 1, 2, 3...
            foreach (var nivel in regrasFixas.Keys.OrderBy(n => n))
            {
                if (idAtualParaReceber == null) break;

                var beneficiario = await _context.Usuarios.FindAsync(idAtualParaReceber);
                if (beneficiario == null) break;

                decimal porcentagem = regrasFixas[nivel];
                decimal valorComissao = valorVenda * (porcentagem / 100);

                if (valorComissao > 0)
                {
                    // A. Registro do Histórico
                    var historico = new Comissao
                    {
                        ID_Pedido = idPedido,
                        ID_Beneficiario = beneficiario.ID,
                        Nivel = nivel,
                        Valor = valorComissao,
                        DataGeracao = DateTime.Now
                    };
                    _context.Comissoes.Add(historico);

                    // B. Registro do Crédito (Transação)
                    var pagamento = new Transacao
                    {
                        ID_Usuario = beneficiario.ID,
                        Valor = valorComissao,
                        Tipo = "Credito",
                        Data = DateTime.Now,
                        Descricao = $"Bônus de Nível {nivel} - Compra de {comprador.Nome}"
                    };
                    _context.Transacoes.Add(pagamento);
                }

                // Sobe para o próximo patrocinador na árvore
                idAtualParaReceber = beneficiario.ID_Patrocinador;
            }

            await _context.SaveChangesAsync();
        }
    }
}