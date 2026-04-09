using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;
using System; // <-- ADICIONADO para usar o DateTime.Now

namespace OnClickSystem.Controllers
{
    // SEGURANÇA MÁXIMA: Só Admin entra aqui
    [Authorize(Roles = "Admin")]
    public class KitsController : Controller
    {
        private readonly OnClickContext _context;

        public KitsController(OnClickContext context)
        {
            _context = context;
        }

        // 1. LISTA DE KITS (A Tabela)
        public IActionResult Index()
        {
            var kits = _context.Kits.ToList();
            return View(kits);
        }

        // 2. CRIAR NOVO (Abre a tela)
        public IActionResult Criar()
        {
            return View();
        }

        // 3. SALVAR NOVO (Recebe os dados)
        [HttpPost]
        public IActionResult Criar(Kit kit)
        {
            if (ModelState.IsValid)
            {
                // 1. Salva o Kit na base de dados
                _context.Kits.Add(kit);
                _context.SaveChanges();

                // --- NOVO: REGISTO DE LOG (CRIAÇÃO) ---
                var logCriacao = new LogSistema
                {
                    DataHora = DateTime.Now,
                    UsuarioResponsavel = User.Identity.Name ?? "Admin",
                    Acao = "Criação de Kit", // Vai para a aba "Sistema"
                    Detalhes = $"O kit '{kit.Nome}' foi criado com sucesso no valor de R$ {kit.Preco:N2}."
                };
                _context.LogsSistema.Add(logCriacao);
                _context.SaveChanges();
                // --------------------------------------

                // MENSAGEM DE SUCESSO
                TempData["Sucesso"] = $"O kit '{kit.Nome}' foi criado com sucesso!";

                return RedirectToAction("Index");
            }
            return View(kit);
        }

        // 4. EDITAR (Abre a tela com dados preenchidos)
        public IActionResult Editar(int id)
        {
            var kit = _context.Kits.Find(id);
            if (kit == null) return NotFound();
            return View(kit);
        }

        // 5. SALVAR EDIÇÃO
        [HttpPost]
        public IActionResult Editar(Kit kit)
        {
            if (ModelState.IsValid)
            {
                // 1. Atualiza as informações do Kit
                _context.Kits.Update(kit);
                _context.SaveChanges();

                // --- NOVO: REGISTO DE LOG (EDIÇÃO) ---
                var logEdicao = new LogSistema
                {
                    DataHora = DateTime.Now,
                    UsuarioResponsavel = User.Identity.Name ?? "Admin",
                    Acao = "Edição de Kit", // Vai para a aba "Sistema"
                    Detalhes = $"As informações do kit '{kit.Nome}' foram atualizadas."
                };
                _context.LogsSistema.Add(logEdicao);
                _context.SaveChanges();
                // -------------------------------------

                // Adicionei também uma mensagem de sucesso para a edição!
                TempData["Sucesso"] = $"O kit '{kit.Nome}' foi atualizado com sucesso!";

                return RedirectToAction("Index");
            }
            return View(kit);
        }
    }
}