using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnClickSystem.Application.Services;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OnClickSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UsuarioService _usuarioService;
        private readonly OnClickContext _context;
        private readonly ComissaoService _comissaoService;

        public AdminController(UsuarioService usuarioService, OnClickContext context, ComissaoService comissaoService)
        {
            _usuarioService = usuarioService;
            _context = context;
            _comissaoService = comissaoService;
        }

        #region Gestão de Usuários (Tabela Principal)

        public async Task<IActionResult> Usuarios(string busca)
        {
            if (!string.IsNullOrEmpty(busca))
                ViewData["BuscaAtual"] = busca;

            var lista = await _usuarioService.ListarUsuarios(busca);
            return View(lista);
        }

        #endregion

        #region Ações da View Detalhes (Passo 2)

        public async Task<IActionResult> Detalhes(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Patrocinador)
                .FirstOrDefaultAsync(u => u.ID == id);

            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // Método chamado pelo formulário de "Alterar Cargo" na sua View Detalhes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarPerfilDetalhes(int id, string novoPerfil)
        {
            var resultado = await _usuarioService.AlterarPerfil(id, novoPerfil);

            if (resultado.Sucesso)
            {
                await RegistrarLog("Segurança", $"Cargo do usuário ID {id} alterado para {novoPerfil}.");
                TempData["Sucesso"] = "Cargo atualizado com sucesso!";
            }
            else
            {
                TempData["Erro"] = resultado.Mensagem;
            }

            return RedirectToAction("Detalhes", new { id = id });
        }

        // Método chamado pelo botão "Excluir Definitivamente" na sua View Detalhes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deletar(int id)
        {
            var resultado = await _usuarioService.DeletarUsuario(id);
            if (resultado.Sucesso)
            {
                await RegistrarLog("Segurança", $"Usuário ID {id} foi EXCLUÍDO permanentemente.");
                TempData["Sucesso"] = "Usuário removido do sistema.";
                return RedirectToAction(nameof(Usuarios));
            }

            TempData["Erro"] = resultado.Mensagem;
            return RedirectToAction("Detalhes", new { id = id });
        }

        #endregion

        #region Ações da View Editar (Passo 3)

        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Usuario usuarioForm, string? NovaSenha)
        {
            if (id != usuarioForm.ID) return NotFound();

            var resultado = await _usuarioService.AtualizarUsuario(id, usuarioForm, NovaSenha);

            if (resultado.Sucesso)
            {
                await RegistrarLog("Segurança", $"Dados de {usuarioForm.Nome} editados via painel administrativo.");
                TempData["Sucesso"] = "Dados atualizados com sucesso!";
                return RedirectToAction(nameof(Usuarios));
            }

            TempData["Erro"] = resultado.Mensagem;
            return View(usuarioForm);
        }

        #endregion

        #region Status, Saques e Relatórios

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarStatus(int id)
        {
            var usuarioAntes = await _context.Usuarios.FindAsync(id);
            if (usuarioAntes == null) return NotFound();

            var resultado = await _usuarioService.AlternarStatus(id);

            if (resultado.Sucesso)
            {
                // Lógica de comissão se for a primeira ativação
                if (usuarioAntes.Ativo == false && usuarioAntes.ID_Patrocinador.HasValue)
                {
                    await _comissaoService.GerarComissaoCadastro(usuarioAntes.ID_Patrocinador.Value, usuarioAntes.ID);
                }

                await RegistrarLog("Segurança", $"Status de {usuarioAntes.Nome} alterado pelo Admin.");
                TempData["Sucesso"] = resultado.Mensagem;
            }

            // Redireciona para onde o usuário estava (Usuarios ou Detalhes)
            var returnUrl = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);

            return RedirectToAction(nameof(Usuarios));
        }

        public async Task<IActionResult> AprovarSaques()
        {
            var saques = await _context.SolicitacoesSaque
                .Include(s => s.Usuario)
                .OrderByDescending(s => s.DataSolicitacao)
                .ToListAsync();
            return View(saques);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessarSaque(int id, string acao)
        {
            var saque = await _context.SolicitacoesSaque.Include(s => s.Usuario).FirstOrDefaultAsync(s => s.ID == id);
            if (saque == null) return NotFound();

            if (acao == "Aprovar")
            {
                saque.Status = "Pago";
                saque.DataPagamento = DateTime.Now;
                _context.Transacoes.Add(new Transacao
                {
                    ID_Usuario = saque.ID_Usuario,
                    Valor = saque.Valor,
                    Tipo = "Debito",
                    Data = DateTime.Now,
                    Descricao = $"Saque Aprovado - {saque.ChavePix}"
                });
                await RegistrarLog("Financeiro", $"Saque de R$ {saque.Valor} aprovado para {saque.Usuario?.Nome}.");
            }
            else
            {
                saque.Status = "Negado";
                await RegistrarLog("Financeiro", $"Saque de R$ {saque.Valor} negado para {saque.Usuario?.Nome}.");
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AprovarSaques));
        }

        public async Task<IActionResult> Relatorios(string busca, string acaoFiltro, DateTime? inicio, DateTime? fim)
        {
            var query = _context.LogsSistema.AsQueryable();
            if (!string.IsNullOrEmpty(busca)) query = query.Where(l => l.Detalhes.Contains(busca));
            if (!string.IsNullOrEmpty(acaoFiltro)) query = query.Where(l => l.Acao == acaoFiltro);
            if (inicio.HasValue) query = query.Where(l => l.DataHora >= inicio.Value);
            if (fim.HasValue) query = query.Where(l => l.DataHora <= fim.Value.AddDays(1));

            ViewBag.AcoesDisponiveis = await _context.LogsSistema.Select(l => l.Acao).Distinct().ToListAsync();
            var logs = await query.OrderByDescending(l => l.DataHora).Take(200).ToListAsync();
            return View(logs);
        }

        #endregion

        private async Task RegistrarLog(string categoria, string detalhes)
        {
            var log = new LogSistema
            {
                DataHora = DateTime.Now,
                UsuarioResponsavel = User.Identity.Name ?? "Admin",
                Acao = categoria,
                Detalhes = detalhes
            };
            _context.LogsSistema.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}