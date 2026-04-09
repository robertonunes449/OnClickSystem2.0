using System.Collections.Generic;
using OnClickSystem.Domain.Entities;

namespace OnClickSystem.Application.ViewModels
{
    // Esta classe serve APENAS para transportar dados para a tela (não vira tabela no banco)
    public class LogsViewModel
    {
        // 1. Números Gerais (KPIs)
        public decimal FaturamentoTotal { get; set; }
        public decimal TotalSaquesPagos { get; set; }
        public decimal LucroLiquido { get; set; }
        public int TotalUsuarios { get; set; }

        // 2. Dados para os Gráficos
        public string[] GraficoMeses { get; set; }
        public decimal[] GraficoFaturamento { get; set; }
        public List<KitPerformance> TopKits { get; set; }

        // 3. A Lista de Logs (Do arquivo que você já tem)
        public List<LogSistema> ListaDeLogs { get; set; }
    }

    public class KitPerformance
    {
        public string Nome { get; set; }
        public int Quantidade { get; set; }
    }
}