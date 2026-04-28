using Microsoft.EntityFrameworkCore;
using OnClickSystem.Infrastructure.Data;
using OnClickSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using System.IO;
using System.Text;

namespace OnClickSystem.Desktop
{
    public partial class FormAdminLogs : Form
    {
        private List<LogSistema> _logsOriginais = new List<LogSistema>();
        private bool _filtrosAtivados = false;

        public FormAdminLogs()
        {
            InitializeComponent();
        }

        private void FormAdminLogs_Load(object sender, EventArgs e)
        {
            EstiloVisual.AplicarTemaPremium(dgvLogs);

            // ========================================================
            // A BALA DE PRATA 1: Obriga a tabela a desenhar os dados!
            // ========================================================
            dgvLogs.AutoGenerateColumns = true;

            if (dtpInicio != null) dtpInicio.Value = DateTime.Today.AddDays(-30);
            if (dtpFim != null) dtpFim.Value = DateTime.Today;

            ConfigurarComboBoxCategorias();

            // ========================================================
            // A BALA DE PRATA 2: Forçar a ligação dos gatilhos do Ecrã
            // ========================================================
            if (txtBuscar != null)
            {
                txtBuscar.TextChanged -= txtBuscar_TextChanged;
                txtBuscar.TextChanged += txtBuscar_TextChanged;
            }

            // Liga forçadamente o botão "Filtrar" ao código!
            // Isso garante que assim que apertar, ele vai funcionar.
            if (this.Controls.Find("btnFiltrar", true).FirstOrDefault() is Control btn)
            {
                btn.Click -= btnFiltrar_Click;
                btn.Click += btnFiltrar_Click;
            }

            CarregarLogs();
        }

        private void ConfigurarComboBoxCategorias()
        {
            if (cmbCategoria != null)
            {
                cmbCategoria.Items.Clear();
                cmbCategoria.Items.Add("Todos");
                cmbCategoria.Items.Add("Alteração de perfil");
                cmbCategoria.Items.Add("Alteração de permissões");
                cmbCategoria.Items.Add("Alteração de status");
                cmbCategoria.Items.Add("Compra de kits");
                cmbCategoria.Items.Add("Criação de kits");
                cmbCategoria.Items.Add("Edição de kits");
                cmbCategoria.Items.Add("Exclusão de Conta");
                cmbCategoria.Items.Add("Falha de login");
                cmbCategoria.Items.Add("Login Realizado");
                cmbCategoria.Items.Add("Novo Cadastro");
                cmbCategoria.SelectedIndex = 0;
            }
        }

        private void CarregarLogs()
        {
            try
            {
                using (var db = ConfiguracaoBanco.ObterContexto())
                {
                    // Puxa TODOS os logs do banco, do mais novo para o mais velho
                    _logsOriginais = db.LogsSistema.OrderByDescending(l => l.DataHora).ToList();

                    lblTotalLogs.Text = _logsOriginais.Count.ToString();
                    lblLogsHoje.Text = _logsOriginais.Count(l => l.DataHora.Date == DateTime.Today).ToString();
                    lblErros.Text = _logsOriginais.Count(l =>
                        (l.Acao != null && (l.Acao.ToLower().Contains("erro") || l.Acao.ToLower().Contains("falha"))) ||
                        (l.Detalhes != null && (l.Detalhes.ToLower().Contains("erro") || l.Detalhes.ToLower().Contains("falha")))
                    ).ToString();

                    if (_logsOriginais.Count == 0)
                    {
                        Toast.Aviso("O banco de dados de Logs está vazio no momento.");
                    }

                    AtualizarGrid();
                }
            }
            catch (Exception)
            {
                Toast.Erro("Erro ao conectar com o banco de dados de Logs.");
            }
        }

        // ==========================================================
        // MOTOR INTELIGENTE DE FILTRAGEM POR CATEGORIA
        // Identifica as palavras-chave na base de dados
        // ==========================================================
        private string MapearAcaoParaFiltro(string acaoOriginal)
        {
            if (string.IsNullOrEmpty(acaoOriginal)) return "";
            string a = acaoOriginal.ToLower().Trim();

            if (a.Contains("perfil") || a.Contains("pessoais")) return "alteração de perfil";
            if (a.Contains("permiss")) return "alteração de permissões";
            if (a.Contains("status") || a.Contains("bloquead") || a.Contains("ativad")) return "alteração de status";
            if (a.Contains("compra") || a.Contains("pagamento")) return "compra de kits";
            if (a.Contains("cria") || a.Contains("novo kit")) return "criação de kits";
            if (a.Contains("edi") || a.Contains("atualiza")) return "edição de kits";
            if (a.Contains("exclu") || a.Contains("delet")) return "exclusão de conta";
            if (a.Contains("falha") || a.Contains("incorreta") || a.Contains("erro")) return "falha de login";
            if (a.Contains("login") || a.Contains("sucesso") || a.Contains("encerrou")) return "login realizado";
            if (a.Contains("cadastro") || a.Contains("registrado")) return "novo cadastro";

            return a;
        }

        // ==========================================================
        // GATILHO DO BOTÃO FILTRAR (ATIVA OS FILTROS)
        // ==========================================================
        private void btnFiltrar_Click(object sender, EventArgs e)
        {
            _filtrosAtivados = true;
            AtualizarGrid();
            Toast.Sucesso("Filtros aplicados com sucesso!");
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            AtualizarGrid();
        }

        private void AtualizarGrid()
        {
            if (_logsOriginais == null || _logsOriginais.Count == 0)
            {
                dgvLogs.DataSource = null;
                return;
            }

            var filtrados = _logsOriginais;

            // Só recorta por data/categoria se o botão Filtrar foi clicado!
            if (_filtrosAtivados)
            {
                DateTime dataInicio = dtpInicio?.Value.Date ?? DateTime.MinValue;
                DateTime dataFim = dtpFim?.Value.Date.AddDays(1).AddTicks(-1) ?? DateTime.MaxValue;
                string categoriaSelecionada = cmbCategoria?.Text.ToLower() ?? "todos";

                filtrados = filtrados.Where(l => l.DataHora >= dataInicio && l.DataHora <= dataFim).ToList();

                if (categoriaSelecionada != "todos")
                {
                    filtrados = filtrados.Where(l => MapearAcaoParaFiltro(l.Acao) == categoriaSelecionada).ToList();
                }
            }

            // A pesquisa por texto escrito na caixinha continua sempre rápida!
            string busca = txtBuscar?.Text.ToLower() ?? "";
            if (!string.IsNullOrWhiteSpace(busca))
            {
                filtrados = filtrados.Where(l =>
                    (l.UsuarioResponsavel != null && l.UsuarioResponsavel.ToLower().Contains(busca)) ||
                    (l.Acao != null && l.Acao.ToLower().Contains(busca)) ||
                    (l.Detalhes != null && l.Detalhes.ToLower().Contains(busca))
                ).ToList();
            }

            // Remove acentos dos nomes das variáveis para não bugar o Windows Forms
            var listaParaGrid = filtrados.Select(l => new
            {
                Data = l.DataHora.ToString("dd/MM/yyyy HH:mm"),
                Usuario = string.IsNullOrEmpty(l.UsuarioResponsavel) ? "Sistema" : l.UsuarioResponsavel,
                Acao = l.Acao ?? "-",
                Detalhes = l.Detalhes ?? "-"
            }).ToList();

            // Limpa qualquer lixo visual e insere os dados
            dgvLogs.Columns.Clear();
            dgvLogs.DataSource = null;
            dgvLogs.DataSource = listaParaGrid;

            // Devolve os acentos apenas nos cabeçalhos visuais da tabela
            if (dgvLogs.Columns.Contains("Usuario")) dgvLogs.Columns["Usuario"].HeaderText = "Usuário";
            if (dgvLogs.Columns.Contains("Acao")) dgvLogs.Columns["Acao"].HeaderText = "Ação";

            // Ajuste estético da tabela
            if (dgvLogs.Columns.Contains("Data")) dgvLogs.Columns["Data"].Width = 140;
            if (dgvLogs.Columns.Contains("Usuario")) dgvLogs.Columns["Usuario"].Width = 150;
            if (dgvLogs.Columns.Contains("Acao")) dgvLogs.Columns["Acao"].Width = 200;
            if (dgvLogs.Columns.Contains("Detalhes"))
            {
                dgvLogs.Columns["Detalhes"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvLogs.Columns["Detalhes"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            }
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            if (dgvLogs.Rows.Count == 0)
            {
                Toast.Aviso("Não há dados para exportar.");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Ficheiro CSV (*.csv)|*.csv";
            sfd.FileName = $"Auditoria_Logs_{DateTime.Now:dd-MM-yyyy_HHmm}.csv";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Data;Usuário;Ação;Detalhes");

                    foreach (DataGridViewRow row in dgvLogs.Rows)
                    {
                        string data = row.Cells["Data"].Value?.ToString() ?? "";
                        string usuario = row.Cells["Usuario"].Value?.ToString() ?? "";
                        string acao = row.Cells["Acao"].Value?.ToString() ?? "";
                        string detalhes = row.Cells["Detalhes"].Value?.ToString() ?? "";

                        detalhes = detalhes.Replace(";", ",");

                        sb.AppendLine($"{data};{usuario};{acao};{detalhes}");
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    Toast.Sucesso("Exportação concluída com sucesso!");
                }
                catch (Exception ex)
                {
                    Toast.Erro("Erro ao exportar o ficheiro.");
                }
            }
        }

        private void dgvLogs_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // O código agora lê "Acao" sem acento da memória!
            if (dgvLogs.Columns[e.ColumnIndex].Name == "Acao" && e.Value != null)
            {
                string acao = e.Value.ToString().ToLower();

                if (acao.Contains("erro") || acao.Contains("falha") || acao.Contains("exclu") || acao.Contains("bloquead"))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(248, 113, 113);
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
                else if (acao.Contains("sucesso") || acao.Contains("ativad") || acao.Contains("salvo") || acao.Contains("cria") || acao.Contains("login") || acao.Contains("cadastro"))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(74, 222, 128);
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
                else if (acao.Contains("edi") || acao.Contains("permiss") || acao.Contains("compra") || acao.Contains("perfil"))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(212, 175, 55);
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
            }
        }
    }
}