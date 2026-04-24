using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnClickSystem.Application.Services;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace OnClickSystem.Controllers
{
    public class CadastroController : Controller
    {
        private readonly UsuarioService _usuarioService;
        private readonly OnClickContext _context;

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
                    ViewBag.PatrocinadorSugerido = patrocinador;
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(Usuario usuario, int? ID_Patrocinador)
        {
            usuario.ID_Patrocinador = ID_Patrocinador;
            var resultado = await _usuarioService.RegistrarUsuario(usuario);

            if (resultado.Sucesso)
            {
                // --- LOG DE REDE ---
                string infoPatro = ID_Patrocinador.HasValue ? $"Indicado por ID: {ID_Patrocinador}" : "Cadastro Direto";
                await RegistrarLog("Rede", $"Novo usuário registrado: {usuario.Nome} ({usuario.Email}). Origem: {infoPatro}");

                TempData["Sucesso"] = resultado.Mensagem;
                return RedirectToAction("Index", "Login");
            }
            else
            {
                ViewBag.Erro = resultado.Mensagem;
                if (ID_Patrocinador.HasValue)
                {
                    var p = await _context.Usuarios.FindAsync(ID_Patrocinador);
                    if (p != null) { ViewBag.PatrocinadorNome = p.Nome; ViewBag.PatrocinadorID = p.ID; }
                }
                return View("Novo", usuario);
            }
        }

        private async Task RegistrarLog(string categoria, string detalhes)
        {
            var log = new LogSistema
            {
                DataHora = DateTime.Now,
                UsuarioResponsavel = "Visitante/Novo Cadastro",
                Acao = categoria,
                Detalhes = detalhes
            };
            _context.LogsSistema.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}