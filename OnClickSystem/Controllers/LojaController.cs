using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;
using OnClickSystem.Application.Services;
using OnClickSystem.Extensions;
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

        // 1. TELA PRINCIPAL DA LOJA (O que estava faltando!)
        public IActionResult Index()
        {
            var kits = _context.Kits.Where(k => k.Ativo).ToList();
            return View(kits);
        }

        // 2. ADICIONAR ITEM
        public IActionResult AdicionarAoCarrinho(int id)
        {
            var kit = _context.Kits.Find(id);
            if (kit == null) return NotFound();

            var carrinho = HttpContext.Session.Get<List<CarrinhoItem>>(SessionKeyCarrinho) ?? new List<CarrinhoItem>();

            if (carrinho.Any(c => c.KitId == id))
            {
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

        // 3. VISUALIZAR CARRINHO
        public IActionResult Carrinho()
        {
            var carrinho = HttpContext.Session.Get<List<CarrinhoItem>>(SessionKeyCarrinho) ?? new List<CarrinhoItem>();
            return View(carrinho);
        }

        // 4. REMOVER ITEM
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

        // 5. TELA DE CHECKOUT
        public IActionResult Checkout()
        {
            var carrinho = HttpContext.Session.Get<List<CarrinhoItem>>(SessionKeyCarrinho) ?? new List<CarrinhoItem>();
            if (!carrinho.Any())
            {
                TempData["Erro"] = "Seu carrinho está vazio.";
                return RedirectToAction("Index");
            }
            return View(carrinho);
        }

        // 6. PROCESSAR PAGAMENTO E LOGS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessarCheckout(string formaPagamento)
        {
            try
            {
                var carrinho = HttpContext.Session.Get<List<CarrinhoItem>>(SessionKeyCarrinho);
                if (carrinho == null || !carrinho.Any()) return RedirectToAction("Index");

                var idLogado = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var usuario = await _context.Usuarios.FindAsync(idLogado);
                bool eraInativo = !usuario.Ativo;

                foreach (var item in carrinho)
                {
                    var pedido = new Pedido
                    {
                        ID_Usuario = idLogado,
                        ID_Kit = item.KitId,
                        DataPedido = DateTime.Now,
                        Valor = item.Preco,
                        Status = "Aprovado"
                    };

                    _context.Pedidos.Add(pedido);
                    await _context.SaveChangesAsync();

                    // Paga comissão para o patrocinador
                    await _comissaoService.ProcessarComissao(pedido.ID, usuario.ID, item.Preco);

                    // LOG FINANCEIRO
                    await RegistrarLog("Financeiro", $"Compra aprovada via {formaPagamento}: {item.Nome} (R$ {item.Preco:N2})");
                }

                if (eraInativo)
                {
                    usuario.Ativo = true;
                    _context.Usuarios.Update(usuario);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.Remove(SessionKeyCarrinho);
                    TempData["Sucesso"] = "Conta ativada com sucesso! Faça login novamente para atualizar seu acesso.";
                    return RedirectToAction("Sair", "Login");
                }

                HttpContext.Session.Remove(SessionKeyCarrinho);
                TempData["Sucesso"] = "Compra realizada com sucesso!";
                return RedirectToAction("Index", "Pedidos");
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao processar: {ex.Message}";
                return RedirectToAction("Carrinho");
            }
        }

        private async Task RegistrarLog(string categoria, string detalhes)
        {
            var log = new LogSistema
            {
                DataHora = DateTime.Now,
                UsuarioResponsavel = User.Identity.Name ?? "Comprador",
                Acao = categoria,
                Detalhes = detalhes
            };
            _context.LogsSistema.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}