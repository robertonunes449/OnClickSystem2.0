using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnClickSystem.Application.Services;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;
using System; // <-- Necessário para o DateTime.Now
using System.Threading.Tasks;

namespace OnClickSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UsuarioService _usuarioService;
        private readonly OnClickContext _context; // <-- 1. Adicionamos a ligação à Base de Dados

        // 2. Atualizamos o construtor para receber ambos
        public AdminController(UsuarioService usuarioService, OnClickContext context)
        {
            _usuarioService = usuarioService;
            _context = context;
        }

        public async Task<IActionResult> Usuarios(string busca)
        {
            if (!string.IsNullOrEmpty(busca))
                ViewData["BuscaAtual"] = busca;

            var lista = await _usuarioService.ListarUsuarios(busca);
            return View(lista);
        }

        public async Task<IActionResult> Detalhes(int id)
        {
            if (id <= 0) return NotFound();
            var usuario = await _usuarioService.ObterDetalhes(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _usuarioService.ObterDetalhes(id);
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
                // --- LOG: EDIÇÃO DE UTILIZADOR ---
                var log = new LogSistema
                {
                    DataHora = DateTime.Now,
                    UsuarioResponsavel = User.Identity.Name ?? "Admin",
                    Acao = "Edição de Perfil", // Vai para a aba Sistema
                    Detalhes = $"O perfil do utilizador {usuarioForm.Nome} (ID: {id}) foi atualizado."
                };
                _context.LogsSistema.Add(log);
                await _context.SaveChangesAsync();
                // ---------------------------------

                TempData["Sucesso"] = resultado.Mensagem;
                return RedirectToAction(nameof(Usuarios));
            }

            TempData["Erro"] = resultado.Mensagem;
            return View(usuarioForm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarPerfil(int id, string novoPerfil)
        {
            var resultado = await _usuarioService.AlterarPerfil(id, novoPerfil);

            if (resultado.Sucesso)
            {
                // --- LOG: ALTERAÇÃO DE HIERARQUIA ---
                var log = new LogSistema
                {
                    DataHora = DateTime.Now,
                    UsuarioResponsavel = User.Identity.Name ?? "Admin",
                    Acao = "Alteração de Permissões",
                    Detalhes = $"O utilizador de ID {id} foi promovido/rebaixado para o nível de {novoPerfil}."
                };
                _context.LogsSistema.Add(log);
                await _context.SaveChangesAsync();
                // ------------------------------------
                TempData["Sucesso"] = resultado.Mensagem;
            }
            else
            {
                TempData["Erro"] = resultado.Mensagem;
            }

            return RedirectToAction(nameof(Usuarios));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarStatus(int id)
        {
            var resultado = await _usuarioService.AlternarStatus(id);

            if (resultado.Sucesso)
            {
                // --- LOG: BLOQUEIO/DESBLOQUEIO ---
                var log = new LogSistema
                {
                    DataHora = DateTime.Now,
                    UsuarioResponsavel = User.Identity.Name ?? "Admin",
                    Acao = "Alteração de Status",
                    Detalhes = $"O acesso do utilizador de ID {id} foi alterado (Bloqueado/Desbloqueado)."
                };
                _context.LogsSistema.Add(log);
                await _context.SaveChangesAsync();
                // ---------------------------------
                TempData["Sucesso"] = resultado.Mensagem;
            }
            else
            {
                TempData["Erro"] = resultado.Mensagem;
            }

            return RedirectToAction(nameof(Usuarios));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deletar(int id)
        {
            var resultado = await _usuarioService.DeletarUsuario(id);

            if (resultado.Sucesso)
            {
                // --- LOG: EXCLUSÃO DE CONTA ---
                var log = new LogSistema
                {
                    DataHora = DateTime.Now,
                    UsuarioResponsavel = User.Identity.Name ?? "Admin",
                    Acao = "Exclusão de Conta", // Categoria Sistema
                    Detalhes = $"O utilizador de ID {id} foi permanentemente apagado do sistema."
                };
                _context.LogsSistema.Add(log);
                await _context.SaveChangesAsync();
                // ------------------------------
                TempData["Sucesso"] = resultado.Mensagem;
            }
            else
            {
                TempData["Erro"] = resultado.Mensagem;
            }

            return RedirectToAction(nameof(Usuarios));
        }
    }
}