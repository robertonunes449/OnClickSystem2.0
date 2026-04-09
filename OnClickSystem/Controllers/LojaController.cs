using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;
using OnClickSystem.Application.Services;
using OnClickSystem.Extensions; // Importante para usar a extensão criada acima
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace OnClickSystem.Controllers
{
    [Authorize]
    public class LojaController : Controller
    {
        private readonly OnClickContext _context;
        private readonly ComissaoService _comissaoService;
        private const string SessionKeyCarrinho = "CarrinhoCompras";

        public LojaController(OnClickContext context, ComissaoService comissaoService)
        {
            _context = context;
            _comissaoService = comissaoService;
        }

        public IActionResult Index()
        {
            return View(_context.Kits.Where(k => k.Ativo).ToList());
        }

        // --- LÓGICA DO CARRINHO ---

        public IActionResult AdicionarAoCarrinho(int id)
        {
            var kit = _context.Kits.Find(id);
            if (kit == null) return NotFound();

            // Recupera carrinho atual ou cria novo
            var carrinho = HttpContext.Session.Get<List<CarrinhoItem>>(SessionKeyCarrinho) ?? new List<CarrinhoItem>();

            // Verifica se já existe
            var itemExistente = carrinho.FirstOrDefault(c => c.KitId == id);
            if (itemExistente != null)
            {
                // Se já existe, avisa ou incrementa (optei por avisar pois são Kits de ativação)
                TempData["Info"] = "Este kit já está no seu carrinho.";
            }
            else
            {
                carrinho.Add(new CarrinhoItem
                {
                    KitId = kit.ID,
                    Nome = kit.Nome,
                    Preco = kit.Preco,
                    Quantidade = 1
                });
                HttpContext.Session.Set(SessionKeyCarrinho, carrinho);
                TempData["Sucesso"] = "Kit adicionado ao carrinho!";
            }

            return RedirectToAction("Carrinho");
        }

        public IActionResult Carrinho()
        {
            var carrinho = HttpContext.Session.Get<List<CarrinhoItem>>(SessionKeyCarrinho) ?? new List<CarrinhoItem>();
            return View(carrinho);
        }

        public IActionResult RemoverDoCarrinho(int id)
        {
            var carrinho = HttpContext.Session.Get<List<CarrinhoItem>>(SessionKeyCarrinho) ?? new List<CarrinhoItem>();
            var item = carrinho.FirstOrDefault(c => c.KitId == id);

            if (item != null)
            {
                carrinho.Remove(item);
                HttpContext.Session.Set(SessionKeyCarrinho, carrinho);
            }

            return RedirectToAction("Carrinho");
        }

        // --- CHECKOUT AGORA PROCESSA O CARRINHO INTEIRO ---

        public IActionResult Checkout()
        {
            var carrinho = HttpContext.Session.Get<List<CarrinhoItem>>(SessionKeyCarrinho) ?? new List<CarrinhoItem>();

            if (!carrinho.Any())
            {
                TempData["Erro"] = "Seu carrinho está vazio.";
                return RedirectToAction("Index");
            }

            return View(carrinho); // Passa a lista para a View
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarCompra()
        {
            try
            {
                var carrinho = HttpContext.Session.Get<List<CarrinhoItem>>(SessionKeyCarrinho);
                if (carrinho == null || !carrinho.Any()) return RedirectToAction("Index");

                var idLogado = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var usuario = await _context.Usuarios.FindAsync(idLogado);

                // 1. Guardamos na memória se ele era Inativo ANTES da compra
                bool eraInativo = !usuario.Ativo;

                foreach (var item in carrinho)
                {
                    var pedido = new Pedido
                    {
                        ID_Usuario = idLogado,
                        ID_Kit = item.KitId,
                        DataPedido = DateTime.Now,
                        Valor = item.Preco,
                        Status = "Pago"
                    };

                    _context.Pedidos.Add(pedido);
                    await _context.SaveChangesAsync();

                    await _comissaoService.ProcessarComissao(pedido.ID, usuario.ID, item.Preco);
                }

                // 2. Se era inativo, ativamos agora no banco de dados
                if (eraInativo)
                {
                    usuario.Ativo = true;
                    _context.Usuarios.Update(usuario);
                    await _context.SaveChangesAsync();
                }

                // 3. Limpa o carrinho
                HttpContext.Session.Remove(SessionKeyCarrinho);

                // 4. A NOVA REGRA DE REDIRECIONAMENTO:
                if (eraInativo)
                {
                    // Se era a PRIMEIRA compra (Ativação), desloga para atualizar o crachá
                    TempData["Sucesso"] = "Parabéns, sua conta foi ativada! Por favor, faça login novamente para entrar no painel.";
                    return RedirectToAction("Sair", "Login");
                }
                else
                {
                    // Se já era ativo (Recompra), apenas avisa que deu certo e volta para a Home ou Pedidos
                    TempData["Sucesso"] = "Nova compra realizada com sucesso!";
                    return RedirectToAction("Index", "Pedidos"); // Pode mudar para "Home" se preferir
                }
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao processar: {ex.Message}";
                return RedirectToAction("Carrinho");
            }
        }
    }
}