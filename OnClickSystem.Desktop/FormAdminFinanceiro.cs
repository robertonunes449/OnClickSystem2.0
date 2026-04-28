using Microsoft.EntityFrameworkCore;
using OnClickSystem.Infrastructure.Data;
using OnClickSystem.Domain.Entities;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OnClickSystem.Desktop
{
    public partial class FormAdminFinanceiro : Form
    {
        public FormAdminFinanceiro()
        {
            InitializeComponent();
        }

        private void FormAdminFinanceiro_Load(object sender, EventArgs e)
        {
            EstiloVisual.AplicarTemaPremium(dgvSaques);
            CarregarSaques();
        }

        private void CarregarSaques()
        {
            try
            {
                using (var db = ConfiguracaoBanco.ObterContexto())
                {
                    var saques = db.SolicitacoesSaque.Include(s => s.Usuario).ToList();

                    var pendentes = saques.Where(s => s.Status == "Pendente").Sum(s => s.Valor);
                    var pagos = saques.Where(s => s.Status == "Aprovado").Sum(s => s.Valor);
                    var rejeitados = saques.Where(s => s.Status == "Rejeitado").Sum(s => s.Valor);

                    lblPendente.Text = $"R$ {pendentes:N2}";
                    lblPago.Text = $"R$ {pagos:N2}";
                    lblRejeitado.Text = $"R$ {rejeitados:N2}";

                    var listaPendentes = saques.Where(s => s.Status == "Pendente").Select(s => new
                    {
                        ID = s.ID,
                        Usuário = s.Usuario.Nome,
                        ChavePIX = s.ChavePix ?? "Não informada",
                        Valor = $"R$ {s.Valor:N2}",
                        Data = s.DataSolicitacao.ToString("dd/MM/yyyy HH:mm"),
                        Status = s.Status
                    }).ToList();

                    dgvSaques.DataSource = listaPendentes;

                    if (dgvSaques.Columns.Count > 0)
                    {
                        dgvSaques.Columns["ID"].Visible = false;
                        dgvSaques.Columns["Usuário"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        dgvSaques.Columns["ChavePIX"].Width = 200;
                        dgvSaques.Columns["Valor"].Width = 120;
                        dgvSaques.Columns["Data"].Width = 150;
                        dgvSaques.Columns["Status"].Width = 100;
                    }
                }
            }
            catch (Exception)
            {
                Toast.Erro("Erro ao carregar o financeiro.");
            }
        }

        private void btnAprovar_Click(object sender, EventArgs e)
        {
            if (dgvSaques.SelectedRows.Count == 0)
            {
                Toast.Aviso("Por favor, selecione uma linha na tabela primeiro!");
                return;
            }
            int idSaque = Convert.ToInt32(dgvSaques.SelectedRows[0].Cells["ID"].Value);
            DialogResult confirmacao = MessageBox.Show("Você já realizou a transferência PIX deste valor para o usuário?\nEsta ação não pode ser desfeita.", "Confirmar Pagamento", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmacao == DialogResult.Yes) MudarStatusSaque(idSaque, "Aprovado");
        }

        private void btnRejeitar_Click(object sender, EventArgs e)
        {
            if (dgvSaques.SelectedRows.Count == 0)
            {
                Toast.Aviso("Por favor, selecione uma linha na tabela primeiro!");
                return;
            }
            int idSaque = Convert.ToInt32(dgvSaques.SelectedRows[0].Cells["ID"].Value);
            DialogResult confirmacao = MessageBox.Show("Deseja rejeitar este saque e DEVOLVER automaticamente o saldo para a conta do usuário?", "Estornar Saque", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmacao == DialogResult.Yes) MudarStatusSaque(idSaque, "Rejeitado");
        }

        private void MudarStatusSaque(int idSaque, string novoStatus)
        {
            try
            {
                using (var db = ConfiguracaoBanco.ObterContexto())
                {
                    var saque = db.SolicitacoesSaque.Include(s => s.Usuario).FirstOrDefault(s => s.ID == idSaque);
                    if (saque != null)
                    {
                        saque.Status = novoStatus;
                        string infoEstorno = "";

                        if (novoStatus == "Rejeitado")
                        {
                            var estorno = new Transacao
                            {
                                ID_Usuario = saque.ID_Usuario,
                                Valor = saque.Valor,
                                Tipo = "Credito",
                                Descricao = "Estorno automático de Saque Rejeitado",
                                Data = DateTime.Now
                            };
                            db.Transacoes.Add(estorno);
                            infoEstorno = " (O valor foi integralmente estornado para a carteira do usuário).";
                        }

                        // ========================================================
                        // AUDITORIA: PAGAMENTO OU ESTORNO REALIZADO
                        // ========================================================
                        var logFinanceiro = new LogSistema
                        {
                            DataHora = DateTime.Now,
                            UsuarioResponsavel = SessaoAtual.UsuarioLogado.Email,
                            Acao = "Alteração de status",
                            Detalhes = $"Solicitação de Saque ID {saque.ID} no valor de R$ {saque.Valor:N2} pedida por '{saque.Usuario.Nome}' foi {novoStatus.ToUpper()}{infoEstorno}"
                        };
                        db.LogsSistema.Add(logFinanceiro);

                        db.SaveChanges();

                        Toast.Sucesso($"Saque marcado como {novoStatus} com sucesso!");
                        CarregarSaques();
                    }
                }
            }
            catch (Exception)
            {
                Toast.Erro("Erro ao processar o saque financeiro.");
            }
        }

        private void dgvSaques_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvSaques.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                e.CellStyle.SelectionBackColor = Color.White;

                if (status == "Pendente")
                {
                    e.CellStyle.BackColor = Color.FromArgb(254, 240, 138);
                    e.CellStyle.ForeColor = Color.FromArgb(161, 98, 7);
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
            }
        }
    }
}