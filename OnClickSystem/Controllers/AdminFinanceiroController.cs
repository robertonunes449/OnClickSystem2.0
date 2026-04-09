using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;

using System;
using System.Linq;

namespace OnClickSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminFinanceiroController : Controller
    {
        private readonly OnClickContext _context;

        public AdminFinanceiroController(OnClickContext context) { _context = context; }

        public IActionResult Index()
        {
            var pedidos = _context.SolicitacoesSaque
                .Include(s => s.Usuario)
                .OrderBy(s => s.Status == "Pendente" ? 0 : 1)
                .ThenByDescending(s => s.DataSolicitacao)
                .ToList();

            return View(pedidos);
        }

        [HttpPost]
        public IActionResult AprovarSaque(int id)
        {
            // Alterado para incluir o Utilizador para podermos colocar o nome dele no Log
            var saque = _context.SolicitacoesSaque.Include(s => s.Usuario).FirstOrDefault(s => s.ID == id);

            if (saque != null && saque.Status == "Pendente")
            {
                saque.Status = "Pago";
                saque.DataPagamento = DateTime.Now;

                // --- NOVO: LOG DE SAQUE APROVADO ---
                var logAprovacao = new LogSistema
                {
                    DataHora = DateTime.Now,
                    UsuarioResponsavel = User.Identity.Name ?? "Admin",
                    Acao = "Saque Aprovado", // A palavra 'Saque' envia este log para a aba Financeiro!
                    Detalhes = $"O saque #{saque.ID} no valor de R$ {saque.Valor:N2} de {saque.Usuario?.Nome} foi pago."
                };
                _context.LogsSistema.Add(logAprovacao);
                // -----------------------------------

                _context.SaveChanges();
                TempData["Sucesso"] = "Saque marcado como PAGO!";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RejeitarSaque(int id)
        {
            var saque = _context.SolicitacoesSaque.Include(s => s.Usuario).FirstOrDefault(s => s.ID == id);

            if (saque != null && saque.Status == "Pendente")
            {
                saque.Status = "Rejeitado";

                var estorno = new Transacao
                {
                    ID_Usuario = saque.ID_Usuario,
                    Valor = saque.Valor,
                    Tipo = "Credito",
                    Data = DateTime.Now,
                    Descricao = $"Estorno: Saque #{saque.ID} Rejeitado"
                };
                _context.Transacoes.Add(estorno);

                // --- NOVO: LOG DE SAQUE REJEITADO ---
                var logRejeicao = new LogSistema
                {
                    DataHora = DateTime.Now,
                    UsuarioResponsavel = User.Identity.Name ?? "Admin",
                    Acao = "Saque Rejeitado", // Vai para a aba Financeiro
                    Detalhes = $"O saque #{saque.ID} de {saque.Usuario.Nome} foi rejeitado e o valor de R$ {saque.Valor:N2} foi estornado."
                };
                _context.LogsSistema.Add(logRejeicao);
                // ------------------------------------

                _context.SaveChanges();
                TempData["Sucesso"] = $"Saque rejeitado. R$ {saque.Valor:N2} foram devolvidos para {saque.Usuario.Nome}.";
            }
            return RedirectToAction("Index");
        }
    }
}