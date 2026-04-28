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

namespace OnClickSystem.Desktop
{
    public partial class FormRede : Form
    {
        private List<object> listaCompletaRede = new List<object>();

        public FormRede()
        {
            InitializeComponent();
        }

        private void FormRede_Load(object sender, EventArgs e)
        {
            // Traz a tabela de volta caso a tenhamos escondido na versão dos cartões
            if (dgvRede != null) dgvRede.Visible = true;
            if (guna2ComboBox1 != null) guna2ComboBox1.Visible = true;
            if (txtBuscar != null) txtBuscar.Visible = true;

            EstiloVisual.AplicarTemaPremium(dgvRede);

            // Garante que a tabela não quebre ao desenhar
            if (dgvRede != null) dgvRede.AutoGenerateColumns = true;

            // Preenche os níveis automaticamente
            if (guna2ComboBox1 != null)
            {
                if (guna2ComboBox1.Items.Count == 0)
                {
                    guna2ComboBox1.Items.Add("Nível 1 (Diretos)");
                    guna2ComboBox1.Items.Add("Nível 2");
                    guna2ComboBox1.Items.Add("Nível 3");
                    guna2ComboBox1.Items.Add("Nível 4");
                    guna2ComboBox1.Items.Add("Nível 5");
                    guna2ComboBox1.Items.Add("Todos os Níveis"); // Bónus para ver a rede toda junta!
                }

                guna2ComboBox1.SelectedIndexChanged -= guna2ComboBox1_SelectedIndexChanged;
                guna2ComboBox1.SelectedIndex = 0;
                guna2ComboBox1.SelectedIndexChanged += guna2ComboBox1_SelectedIndexChanged;
            }

            if (txtBuscar != null)
            {
                txtBuscar.TextChanged -= txtBuscar_TextChanged;
                txtBuscar.TextChanged += txtBuscar_TextChanged;
            }

            CarregarDadosRede();
        }

        // ==========================================================
        // O MOTOR MATEMÁTICO: Busca exatamente o nível selecionado
        // ==========================================================
        private void CarregarDadosRede()
        {
            if (SessaoAtual.UsuarioLogado == null) return;

            try
            {
                using (var db = ConfiguracaoBanco.ObterContexto())
                {
                    int meuId = SessaoAtual.UsuarioLogado.ID;
                    int nivelSelecionado = (guna2ComboBox1?.SelectedIndex ?? 0) + 1;
                    bool todosOsNiveis = guna2ComboBox1?.Text == "Todos os Níveis";

                    // Traz tudo para a memória para ser super rápido
                    var todosUsuarios = db.Usuarios.ToList();

                    List<Usuario> usuariosExibicao = new List<Usuario>();
                    List<int> idsNivelAtual = new List<int> { meuId };

                    // Limite máximo de busca
                    int limiteNiveis = todosOsNiveis ? 5 : nivelSelecionado;

                    for (int i = 1; i <= limiteNiveis; i++)
                    {
                        // Encontra quem tem o patrocinador correspondente aos IDs atuais
                        var encontradosNesteNivel = todosUsuarios
                            .Where(u => u.ID_Patrocinador.HasValue && idsNivelAtual.Contains(u.ID_Patrocinador.Value))
                            .ToList();

                        if (!encontradosNesteNivel.Any()) break;

                        // Se for "Todos os níveis", acumula. Se for um nível específico, só guarda o do loop final
                        if (todosOsNiveis || i == nivelSelecionado)
                        {
                            usuariosExibicao.AddRange(encontradosNesteNivel);
                        }

                        // Prepara os IDs para o próximo nível
                        idsNivelAtual = encontradosNesteNivel.Select(u => u.ID).ToList();
                    }

                    // ========================================================
                    // ATUALIZA OS CARTÕES SUPERIORES (KPIs)
                    // ========================================================
                    int total = usuariosExibicao.Count;
                    int ativos = usuariosExibicao.Count(u => u.Ativo);
                    int inativos = total - ativos;

                    // Busca na tabela de transações os seus lucros da rede
                    var ganhos = db.Transacoes
                        .Where(t => t.ID_Usuario == meuId && t.Tipo.ToLower() == "credito" && t.Descricao.Contains("Rede"))
                        .Sum(t => (decimal?)t.Valor) ?? 0m;

                    if (lblTotalRede != null) lblTotalRede.Text = total.ToString();
                    if (lblAtivos != null) lblAtivos.Text = ativos.ToString();
                    if (lblInativos != null) lblInativos.Text = inativos.ToString();
                    if (lblGanhosRede != null) lblGanhosRede.Text = $"R$ {ganhos:N2}";

                    // ========================================================
                    // PREPARA A TABELA INTELIGENTE
                    // ========================================================
                    listaCompletaRede = usuariosExibicao.Select(u => new
                    {
                        Usuário = $"{u.Nome}\n{u.Email}",
                        Telefone = string.IsNullOrEmpty(u.Telefone) ? "Não informado" : u.Telefone,
                        Status = u.Ativo ? "Ativo" : "Inativo",
                        Cadastro = u.DataCadastro.ToString("dd/MM/yyyy")
                    }).Cast<object>().ToList();

                    AtualizarGrid();
                }
            }
            catch (Exception ex)
            {
                Toast.Erro("Erro ao carregar rede: " + ex.Message);
            }
        }

        // ==========================================================
        // MOTOR DE FILTRAGEM INSTANTÂNEA DA TABELA
        // ==========================================================
        private void AtualizarGrid()
        {
            string busca = txtBuscar?.Text.ToLower() ?? "";

            var filtrados = string.IsNullOrWhiteSpace(busca)
                ? listaCompletaRede
                : listaCompletaRede.Where(u =>
                {
                    var propUsuario = u.GetType().GetProperty("Usuário")?.GetValue(u, null)?.ToString().ToLower();
                    return propUsuario != null && propUsuario.Contains(busca);
                }).ToList();

            if (dgvRede != null)
            {
                dgvRede.DataSource = null;
                dgvRede.DataSource = filtrados;

                // Proteção para as colunas
                if (dgvRede.Columns.Contains("Usuário"))
                {
                    dgvRede.Columns["Usuário"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dgvRede.Columns["Usuário"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                }
                if (dgvRede.Columns.Contains("Telefone")) dgvRede.Columns["Telefone"].Width = 150;
                if (dgvRede.Columns.Contains("Status")) dgvRede.Columns["Status"].Width = 100;
                if (dgvRede.Columns.Contains("Cadastro")) dgvRede.Columns["Cadastro"].Width = 120;
            }
        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CarregarDadosRede(); // Ao trocar o nível, recarrega a tabela e os cálculos
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            AtualizarGrid(); // Ao digitar, filtra a tabela
        }

        // ==========================================================
        // PINTURA CONDICIONAL (Verde para Ativo, Vermelho para Inativo)
        // ==========================================================
        private void dgvRede_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvRede.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                e.CellStyle.SelectionBackColor = Color.White;

                if (status == "Ativo")
                {
                    e.CellStyle.BackColor = Color.FromArgb(220, 252, 231);
                    e.CellStyle.ForeColor = Color.FromArgb(22, 163, 74);
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
                else if (status == "Inativo")
                {
                    e.CellStyle.BackColor = Color.FromArgb(254, 226, 226);
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
            }
        }

        // ==========================================================
        // BOTÃO DE CADASTRO (Com Auditoria Segura)
        // ==========================================================
        private void btnCadastrar_Click(object sender, EventArgs e)
        {
            FormCadastro telaCadastro = new FormCadastro();
            if (telaCadastro.ShowDialog() == DialogResult.OK)
            {
                // Se registrou alguém, recarrega a rede para aparecer!
                CarregarDadosRede();
            }
        }
    }
}