using Microsoft.EntityFrameworkCore;
using OnClickSystem.Infrastructure.Data;
using OnClickSystem.Domain.Entities;
using System;
using System.Windows.Forms;

namespace OnClickSystem.Desktop
{
    public partial class FormCadastro : Form
    {
        public FormCadastro()
        {
            InitializeComponent();
        }

        // ==========================================================
        // BOTÃO SALVAR (Guarda no Banco de Dados e Cria o Log)
        // ==========================================================
        private void btnSalvar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text) || string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtSenha.Text))
            {
                Toast.Aviso("Por favor, preencha pelo menos o Nome, E-mail e Senha!");
                return;
            }

            try
            {
                using (var db = ConfiguracaoBanco.ObterContexto())
                {
                    // 1. Cria o Novo Usuário
                    var novoUsuario = new Usuario
                    {
                        Nome = txtNome.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        Telefone = txtTelefone.Text.Trim(),
                        CPF = txtCpf.Text.Trim(),
                        Senha = txtSenha.Text,
                        ID_Patrocinador = SessaoAtual.UsuarioLogado.ID,
                        Ativo = false,
                        DataCadastro = DateTime.Now,
                        Perfil = "Usuario"
                    };

                    db.Usuarios.Add(novoUsuario);

                    // ========================================================
                    // 2. AUDITORIA: Salva quem cadastrou a pessoa!
                    // ========================================================
                    var logCadastro = new LogSistema
                    {
                        DataHora = DateTime.Now,
                        UsuarioResponsavel = SessaoAtual.UsuarioLogado.Email, // O email do admin/patrocinador logado
                        Acao = "Novo Cadastro",
                        Detalhes = $"Novo usuário cadastrado. Nome: {novoUsuario.Nome} | E-mail: {novoUsuario.Email}"
                    };
                    db.LogsSistema.Add(logCadastro);

                    // 3. Salva os dois (Usuário e Log) juntos
                    db.SaveChanges();

                    Toast.Sucesso("Novo indicado cadastrado com sucesso na sua rede!");

                    // Fecha a janelinha
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Toast.Erro("Erro ao salvar cadastro: " + ex.Message);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}