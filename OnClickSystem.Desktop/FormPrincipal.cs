using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using System.Linq;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;
using System.Globalization; // ADICIONADO PARA FORMATAR A DATA EM PORTUGUÊS

namespace OnClickSystem.Desktop
{
    public partial class FormPrincipal : Form
    {
        private Form formAtivo = null;

        // ==========================================
        // CORREÇÃO: Forçando o Timer do Windows Forms
        // ==========================================
        private System.Windows.Forms.Timer timerRelogio;

        public FormPrincipal()
        {
            InitializeComponent();
            ConfigurarRelogio(); // INICIA O RELÓGIO JUNTO COM O FORMULÁRIO
        }

        // ==========================================
        // MÉTODOS DO RELÓGIO E CABEÇALHO
        // ==========================================
        private void ConfigurarRelogio()
        {
            // CORREÇÃO: Instanciando o Timer exato
            timerRelogio = new System.Windows.Forms.Timer();
            timerRelogio.Interval = 1000; // Atualiza a cada 1000ms (1 segundo)
            timerRelogio.Tick += TimerRelogio_Tick;
            timerRelogio.Start();

            // Chama a função uma vez imediatamente para não ficar em branco no primeiro segundo
            AtualizarDataHora();
        }

        private void TimerRelogio_Tick(object sender, EventArgs e)
        {
            AtualizarDataHora();
        }

        private void AtualizarDataHora()
        {
            // Formata a data. Ex: "Segunda-feira, 27 de abril de 2026 | 20:02:29"
            CultureInfo culturaBR = new CultureInfo("pt-BR");
            string dataHoraFormatada = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy | HH:mm:ss", culturaBR);

            // Capitaliza a primeira letra do dia da semana (fica mais bonito)
            dataHoraFormatada = char.ToUpper(dataHoraFormatada[0]) + dataHoraFormatada.Substring(1);

            // Verifica se o label existe na tela antes de atualizar
            if (lblCabecalho != null)
            {
                lblCabecalho.Text = dataHoraFormatada;
            }
        }

        // ==========================================
        // MÉTODOS DE INICIALIZAÇÃO E SESSÃO
        // ==========================================
        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            AtualizarLabelUsuario();
            this.BeginInvoke(new Action(() => { btnDashboard.PerformClick(); }));
        }

        public void AtualizarLabelUsuario()
        {
            if (SessaoAtual.UsuarioLogado != null)
            {
                string nomeCompleto = SessaoAtual.UsuarioLogado.Nome;
                if (!string.IsNullOrWhiteSpace(nomeCompleto))
                {
                    string primeiroNome = nomeCompleto.Trim().Split(' ')[0];
                    lblNomeUsuario.Text = $"Olá, {primeiroNome}";
                }
                else
                {
                    lblNomeUsuario.Text = $"Olá, {SessaoAtual.UsuarioLogado.Perfil}";
                }
            }
            else
            {
                lblNomeUsuario.Text = "Olá, Visitante";
            }
        }

        // ==========================================
        // NAVEGAÇÃO DE TELAS (CLEAN ARCHITECTURE)
        // ==========================================
        private void AbrirFormNoPainel(Form formFilho)
        {
            if (formAtivo != null)
            {
                formAtivo.Close();
                formAtivo.Dispose();
            }

            formAtivo = formFilho;
            formFilho.TopLevel = false;
            formFilho.FormBorderStyle = FormBorderStyle.None;
            formFilho.Dock = DockStyle.Fill;

            pnlConteudo.Controls.Clear();
            pnlConteudo.Controls.Add(formFilho);
            pnlConteudo.Tag = formFilho;

            formFilho.BringToFront();
            formFilho.Show();
        }

        private void DestacarBotaoMenu(Guna2Button botaoClicado)
        {
            foreach (Control controle in pnlMenu.Controls)
            {
                if (controle is Guna2Button botao)
                {
                    if (botao == botaoClicado)
                    {
                        botao.FillColor = Color.FromArgb(30, 41, 59);
                        botao.CustomBorderThickness = new Padding(4, 0, 0, 0);
                        botao.CustomBorderColor = Color.FromArgb(212, 175, 55); // Cor Dourada Premium
                    }
                    else
                    {
                        botao.FillColor = Color.Transparent;
                        botao.CustomBorderThickness = new Padding(0, 0, 0, 0);
                    }
                }
            }
        }

        // ==========================================
        // EVENTOS DE CLIQUE (BOTÕES DO MENU)
        // ==========================================
        private void btnPerfil_Click(object sender, EventArgs e)
        {
            using (FormPerfil popUp = new FormPerfil())
            {
                popUp.ShowDialog();
                AtualizarLabelUsuario();
            }
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            DestacarBotaoMenu((Guna2Button)sender);
            AbrirFormNoPainel(new FormDashboard());
        }

        private void btnRede_Click(object sender, EventArgs e)
        {
            DestacarBotaoMenu((Guna2Button)sender);
            AbrirFormNoPainel(new FormRede());
        }

        private void btnFinanceiro2_Click(object sender, EventArgs e)
        {
            DestacarBotaoMenu((Guna2Button)sender);
            AbrirFormNoPainel(new FormAdminFinanceiro());
        }

        private void btnKits_Click(object sender, EventArgs e)
        {
            DestacarBotaoMenu((Guna2Button)sender);
            AbrirFormNoPainel(new FormAdminKits());
        }

        private void btnUsuarios_Click(object sender, EventArgs e)
        {
            DestacarBotaoMenu((Guna2Button)sender);
            AbrirFormNoPainel(new FormAdminUsuarios());
        }

        private void btnLogs_Click(object sender, EventArgs e)
        {
            DestacarBotaoMenu((Guna2Button)sender);
            AbrirFormNoPainel(new FormAdminLogs());
        }

        private void btnSair_Click_1(object sender, EventArgs e)
        {
            if (MessageBox.Show("Deseja realmente sair do sistema?", "Encerrar Sessão", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    if (SessaoAtual.UsuarioLogado != null)
                    {
                        using (var db = ConfiguracaoBanco.ObterContexto())
                        {
                            // ========================================================
                            // AUDITORIA: LOGOUT (ENCERRAR SESSÃO)
                            // ========================================================
                            var logSaida = new LogSistema
                            {
                                DataHora = DateTime.Now,
                                UsuarioResponsavel = SessaoAtual.UsuarioLogado.Email,
                                Acao = "Logout Realizado",
                                Detalhes = "O usuário encerrou a sua sessão no sistema Desktop (Logout efetuado com sucesso)."
                            };
                            db.LogsSistema.Add(logSaida);
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception) { }

                SessaoAtual.UsuarioLogado = null;
                System.Windows.Forms.Application.Restart();
            }
        }
    }
}