using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnClickSystem.Application.Services;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;
using System.Threading.Tasks;

namespace OnClickSystem.Controllers
{
    public class CadastroController : Controller
    {
        private readonly UsuarioService _usuarioService;
        private readonly OnClickContext _context; // Apenas para buscar nome do patrocinador na tela (leitura)

        public CadastroController(UsuarioService usuarioService, OnClickContext context)
        {
            _usuarioService = usuarioService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Novo(string patrocinador)
        {
            if (!string.IsNullOrEmpty(patrocinador))
            {
                var patro = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Nome == patrocinador || u.Email == patrocinador);

                if (patro != null)
                {
                    ViewBag.PatrocinadorNome = patro.Nome;
                    ViewBag.PatrocinadorID = patro.ID;
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(Usuario usuario, int? ID_Patrocinador)
        {
            // Define o patrocinador vindo do Form
            usuario.ID_Patrocinador = ID_Patrocinador;

            // Chama o serviço para tentar registrar com SEGURANÇA (Hash + verificação de email)
            var resultado = await _usuarioService.RegistrarUsuario(usuario);

            if (resultado.Sucesso)
            {
                TempData["Sucesso"] = resultado.Mensagem;
                return RedirectToAction("Index", "Login");
            }
            else
            {
                // Se der erro (ex: email duplicado), volta para a tela mantendo os dados
                ViewBag.Erro = resultado.Mensagem;

                // Recupera nome do patrocinador para não perder na tela
                if (ID_Patrocinador.HasValue)
                {
                    var p = await _context.Usuarios.FindAsync(ID_Patrocinador);
                    if (p != null) { ViewBag.PatrocinadorNome = p.Nome; ViewBag.PatrocinadorID = p.ID; }
                }

                return View("Novo", usuario);
            }
        }
    }
}