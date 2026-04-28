using Microsoft.EntityFrameworkCore;
using OnClickSystem.Infrastructure.Data;
using System;
using System.Drawing; // Necessário para as Cores
using System.Linq;
using System.Windows.Forms;
using Guna.Charts.WinForms; // Necessário para os Gráficos

namespace OnClickSystem.Desktop
{
    public partial class FormDashboard : Form
    {
        public FormDashboard()
        {
            InitializeComponent();
        }

        private void FormDashboard_Load(object sender, EventArgs e)
        {
            // 1. Aplica o tema na tabela
            EstiloVisual.AplicarTemaPremium(dgvExtrato);

            // 2. Carrega os números do topo e a tabela
            CarregarResumoFinanceiro();

            // 3. Ativa os gráficos se eles existirem na tela de desenho
            if (this.Controls.Find("chartGanhos", true).FirstOrDefault() != null)
            {
                RenderizarGraficoGanhos();
            }

            if (this.Controls.Find("chartProporcao", true).FirstOrDefault() != null)
            {
                RenderizarGraficoProporcao();
            }
        }

        private void CarregarResumoFinanceiro()
        {
            // BARREIRA DE SEGURANÇA CONTRA ERROS DE TELA VAZIA
            if (SessaoAtual.UsuarioLogado == null) return;

            try
            {
                using (var db = ConfiguracaoBanco.ObterContexto())
                {
                    int meuId = SessaoAtual.UsuarioLogado.ID;

                    // ============================================
                    // 1. CÁLCULO DOS CARDS SUPERIORES
                    // ============================================
                    var entradas = db.Transacoes
                                     .Where(t => t.ID_Usuario == meuId && t.Tipo.ToLower() == "credito")
                                     .Sum(t => (decimal?)t.Valor) ?? 0m;

                    var saidas = db.Transacoes
                                   .Where(t => t.ID_Usuario == meuId && t.Tipo.ToLower() == "debito")
                                   .Sum(t => (decimal?)t.Valor) ?? 0m;

                    decimal saldoAtual = entradas - saidas;

                    lblSaldo.Text = $"R$ {saldoAtual:N2}";
                    lblGanhos.Text = $"R$ {entradas:N2}";

                    int meusDiretos = db.Usuarios.Count(u => u.ID_Patrocinador == meuId);
                    lblDiretos.Text = meusDiretos.ToString();

                    // ============================================
                    // 2. CARREGA O EXTRATO NA TABELA
                    // ============================================
                    var transacoesBanco = db.Transacoes
                        .Where(t => t.ID_Usuario == meuId)
                        .OrderByDescending(t => t.Data)
                        .Take(50)
                        .ToList();

                    var extrato = transacoesBanco
                        .Select(t => new
                        {
                            Data = t.Data.ToString("dd/MM/yyyy HH:mm"),
                            Descricao = t.Descricao,
                            Valor = t.Tipo.ToLower() == "credito" ? $"+ R$ {t.Valor:N2}" : $"- R$ {t.Valor:N2}",
                            Tipo = t.Tipo
                        })
                        .ToList();

                    dgvExtrato.DataSource = extrato;

                    if (dgvExtrato.Columns.Count > 0)
                    {
                        dgvExtrato.Columns["Data"].Width = 120;
                        dgvExtrato.Columns["Valor"].Width = 100;
                        dgvExtrato.Columns["Descricao"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                }
            }
            catch (Exception ex)
            {
                Toast.Erro("Erro ao carregar o dashboard:\n" + ex.Message);
            }
        }

        // ==========================================================
        // 3. GRÁFICO 1: EVOLUÇÃO DE GANHOS (SPLINE AREA)
        // ==========================================================
        private void RenderizarGraficoGanhos()
        {
            try
            {
                using (var db = ConfiguracaoBanco.ObterContexto())
                {
                    int meuId = SessaoAtual.UsuarioLogado.ID;
                    DateTime dataLimite = DateTime.Today.AddMonths(-5);
                    dataLimite = new DateTime(dataLimite.Year, dataLimite.Month, 1);

                    var transacoes = db.Transacoes
                        .Where(t => t.ID_Usuario == meuId && t.Tipo.ToLower() == "credito" && t.Data >= dataLimite)
                        .ToList();

                    var dadosAgrupados = transacoes
                        .GroupBy(t => new { t.Data.Year, t.Data.Month })
                        .Select(g => new
                        {
                            DataGrupo = new DateTime(g.Key.Year, g.Key.Month, 1),
                            Total = g.Sum(t => t.Valor)
                        })
                        .OrderBy(d => d.DataGrupo)
                        .ToList();

                    chartGanhos.Datasets.Clear();
                    chartGanhos.BackColor = Color.FromArgb(24, 24, 24); // Fundo escuro

                    // GunaSplineAreaDataset cria aquela linha curva com preenchimento em baixo (igual ao seu print)
                    var dataset = new GunaSplineAreaDataset();
                    dataset.Label = "Ganhos Mensais";

                    // Borda Dourada e Fundo Dourado Semi-transparente
                    dataset.BorderColor = Color.FromArgb(212, 175, 55);
                    dataset.FillColor = Color.FromArgb(80, 212, 175, 55);

                    foreach (var item in dadosAgrupados)
                    {
                        dataset.DataPoints.Add(item.DataGrupo.ToString("MMM/yy").ToUpper(), Convert.ToDouble(item.Total));
                    }

                    chartGanhos.Datasets.Add(dataset);
                    chartGanhos.Update();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro no gráfico de evolução: " + ex.Message);
            }
        }

        // ==========================================================
        // 4. GRÁFICO 2: PROPORÇÃO ENTRADAS VS SAÍDAS (ROSCA)
        // ==========================================================
        private void RenderizarGraficoProporcao()
        {
            try
            {
                using (var db = ConfiguracaoBanco.ObterContexto())
                {
                    int meuId = SessaoAtual.UsuarioLogado.ID;

                    var totalEntradas = db.Transacoes
                                     .Where(t => t.ID_Usuario == meuId && t.Tipo.ToLower() == "credito")
                                     .Sum(t => (decimal?)t.Valor) ?? 0m;

                    var totalSaidas = db.Transacoes
                                   .Where(t => t.ID_Usuario == meuId && t.Tipo.ToLower() == "debito")
                                   .Sum(t => (decimal?)t.Valor) ?? 0m;

                    chartGanhos.Datasets.Clear();
                    chartGanhos.BackColor = Color.FromArgb(24, 24, 24); // Fundo escuro

                    var dataset = new GunaDoughnutDataset();
                    dataset.Label = "Movimentação";

                    // Verde para Entradas, Vermelho para Saídas
                    dataset.FillColors.Add(Color.FromArgb(74, 222, 128));
                    dataset.FillColors.Add(Color.FromArgb(248, 113, 113));

                    if (totalEntradas > 0) dataset.DataPoints.Add("Ganhos", Convert.ToDouble(totalEntradas));
                    if (totalSaidas > 0) dataset.DataPoints.Add("Saques", Convert.ToDouble(totalSaidas));

                    chartGanhos.Datasets.Add(dataset);
                    chartGanhos.Update();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro no gráfico de proporção: " + ex.Message);
            }
        }
    }
}