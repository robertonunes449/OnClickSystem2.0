using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;
using System.Security.Claims;

namespace OnClickSystem.Controllers
{
    [Authorize]
    public class PedidosController : Controller
    {
        private readonly OnClickContext _context;

        public PedidosController(OnClickContext context)
        {
            _context = context;
        }

        // LISTA DE PEDIDOS DO USUÁRIO LOGADO
        public IActionResult Index()
        {
            var idUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var meusPedidos = _context.Pedidos
                .Include(p => p.Kit) // Traz o nome do Kit junto
                .Where(p => p.ID_Usuario == idUsuario)
                .OrderByDescending(p => p.DataPedido)
                .ToList();

            return View(meusPedidos);
        }

        // DETALHES DO PEDIDO (RECIBO)
        public IActionResult Detalhes(int id)
        {
            var pedido = _context.Pedidos
                .Include(p => p.Kit)
                .Include(p => p.Usuario) // Traz dados do comprador para o recibo
                .FirstOrDefault(p => p.ID == id);

            if (pedido == null) return NotFound();

            // Segurança: Só o dono do pedido pode ver
            var idLogado = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (pedido.ID_Usuario != idLogado && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AcessoNegado", "Login");
            }

            return View(pedido);
        }
    }
}