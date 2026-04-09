using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnClickSystem.Application.ViewModels;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OnClickSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LogsController : Controller
    {
        private readonly OnClickContext _context;

        public LogsController(OnClickContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Inicializa a "Caixa" que vai levar os dados
            var viewModel = new LogsViewModel();

            // ---------------------------------------------------------
            // 1. CÁLCULOS FINANCEIROS (KPIs)
            // ---------------------------------------------------------
            viewModel.FaturamentoTotal = await _context.Pedidos
                .Where(p => p.Status == "Pago")
                .SumAsync(p => p.Valor);

            viewModel.TotalSaquesPagos = await _context.SolicitacoesSaque
                .Where(s => s.Status == "Pago")
                .SumAsync(s => s.Valor);

            viewModel.LucroLiquido = viewModel.FaturamentoTotal - viewModel.TotalSaquesPagos;
            viewModel.TotalUsuarios = await _context.Usuarios.CountAsync();

            // ---------------------------------------------------------
            // 2. DADOS DO GRÁFICO (Vendas dos últimos 6 meses)
            // ---------------------------------------------------------
            var dataLimite = DateTime.Now.AddMonths(-6);
            var vendasPorMes = await _context.Pedidos
                .Where(p => p.Status == "Pago" && p.DataPedido >= dataLimite)
                .GroupBy(p => new { p.DataPedido.Year, p.DataPedido.Month })
                .Select(g => new
                {
                    Ano = g.Key.Year,
                    Mes = g.Key.Month,
                    Total = g.Sum(x => x.Valor)
                })
                .OrderBy(x => x.Ano).ThenBy(x => x.Mes)
                .ToListAsync();

            var labels = new List<string>();
            var valores = new List<decimal>();

            foreach (var v in vendasPorMes)
            {
                // Formata o nome do mês (Ex: "Jan/2026")
                string mesNome = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(v.Mes);
                labels.Add($"{mesNome}/{v.Ano}");
                valores.Add(v.Total);
            }
            viewModel.GraficoMeses = labels.ToArray();
            viewModel.GraficoFaturamento = valores.ToArray();

            // ---------------------------------------------------------
            // 3. TOP KITS (Para o Gráfico de Pizza)
            // ---------------------------------------------------------
            viewModel.TopKits = await _context.Pedidos
                .Include(p => p.Kit)
                .Where(p => p.Status == "Pago")
                .GroupBy(p => p.Kit.Nome)
                .Select(g => new KitPerformance
                {
                    Nome = g.Key,
                    Quantidade = g.Count()
                })
                .OrderByDescending(x => x.Quantidade)
                .Take(5)
                .ToListAsync();

            // ---------------------------------------------------------
            // 4. LOGS (A lista original que você já tinha)
            // ---------------------------------------------------------
            viewModel.ListaDeLogs = await _context.LogsSistema
                .OrderByDescending(l => l.DataHora)
                .Take(100) // Pega apenas os últimos 100 para não pesar
                .ToListAsync();

            return View(viewModel);
        }
    }
}   