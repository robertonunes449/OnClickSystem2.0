using System;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;

namespace OnClickSystem.Desktop
{
    public partial class FormPerfil : Form
    {
        public FormPerfil()
        {
            InitializeComponent();

            // 1. Liga o evento que carrega a tela
            this.Load += new System.EventHandler(this.FormPerfil_Load);

            // 2. LIGA O BOTÃO DE SALVAR AO CÓDIGO
            this.btnSalvar.Click += new System.EventHandler(this.btnSalvar_Click);
        }

        private void FormPerfil_Load(object sender, EventArgs e)
        {
            CarregarDadosUtilizador();

            // Limpa as senhas por segurança sempre que a tela abre
            txtNovaSenha.Text = "";
            txtConfirmarSenha.Text = "";

            // Garante que o texto digitado vire "bolinhas" ou "asteriscos"
            txtNovaSenha.UseSystemPasswordChar = true;
            txtConfirmarSenha.UseSystemPasswordChar = true;
        }

        private void CarregarDadosUtilizador()
        {
            var usuario = SessaoAtual.UsuarioLogado;

            if (usuario != null)
            {
                txtNome.Text = usuario.Nome;
                txtEmail.Text = usuario.Email;
                txtCpf.Text = usuario.CPF;
                txtTelefone.Text = usuario.Telefone;
                lblPerfilTipo.Text = $"Nível de Acesso: {usuario.Perfil}";
            }
        }

        // ==========================================================
        // 2. BOTÃO SALVAR (COM VALIDAÇÃO DE SENHA)
        // ==========================================================

        private async void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                if (SessaoAtual.UsuarioLogado == null)
                {
                    Toast.Aviso("Erro: O sistema não identificou o seu login nesta sessão.");
                    return;
                }

                // ========================================================
                // VALIDAÇÃO DAS SENHAS 
                // ========================================================
                // Só valida se o usuário tiver digitado alguma coisa em um dos campos
                if (!string.IsNullOrWhiteSpace(txtNovaSenha.Text) || !string.IsNullOrWhiteSpace(txtConfirmarSenha.Text))
                {
                    if (txtNovaSenha.Text != txtConfirmarSenha.Text)
                    {
                        Toast.Aviso("A nova senha e a confirmação não coincidem!");
                        return; // Interrompe a execução aqui para não salvar errado
                    }

                    if (txtNovaSenha.Text.Length < 6)
                    {
                        Toast.Aviso("A nova senha deve ter pelo menos 6 caracteres.");
                        return;
                    }
                }

                using (var db = ConfiguracaoBanco.ObterContexto())
                {
                    var usuarioBanco = await db.Usuarios.FindAsync(SessaoAtual.UsuarioLogado.ID);

                    if (usuarioBanco != null)
                    {
                        usuarioBanco.Nome = txtNome.Text;
                        usuarioBanco.Email = txtEmail.Text;
                        usuarioBanco.CPF = txtCpf.Text;
                        usuarioBanco.Telefone = txtTelefone.Text;

                        string notaAuditoriaSenha = "";

                        // ========================================================
                        // SALVAR NOVA SENHA (CRIPTOGRAFADA)
                        // ========================================================
                        if (!string.IsNullOrWhiteSpace(txtNovaSenha.Text))
                        {
                            // Usa o BCrypt (padrão no seu projeto) para gerar o Hash seguro
                            usuarioBanco.Senha = BCrypt.Net.BCrypt.HashPassword(txtNovaSenha.Text);
                            notaAuditoriaSenha = " | [ATENÇÃO: A senha de acesso também foi alterada]";
                        }

                        // ========================================================
                        // AUDITORIA: Registar a alteração dos próprios dados
                        // ========================================================
                        var logPerfil = new LogSistema
                        {
                            DataHora = DateTime.Now,
                            UsuarioResponsavel = SessaoAtual.UsuarioLogado.Email,
                            Acao = "Alteração de perfil",
                            Detalhes = $"O utilizador atualizou os seus dados pessoais. Novo Nome: {usuarioBanco.Nome} | Telefone: {usuarioBanco.Telefone}{notaAuditoriaSenha}"
                        };
                        db.LogsSistema.Add(logPerfil);

                        // Grava os dados do utilizador e o Log ao mesmo tempo
                        await db.SaveChangesAsync();

                        // Atualiza a sessão em memória para refletir a mudança instantaneamente na tela Principal
                        SessaoAtual.UsuarioLogado.Nome = usuarioBanco.Nome;
                        SessaoAtual.UsuarioLogado.Email = usuarioBanco.Email;
                        SessaoAtual.UsuarioLogado.CPF = usuarioBanco.CPF;
                        SessaoAtual.UsuarioLogado.Telefone = usuarioBanco.Telefone;

                        Toast.Sucesso("Dados atualizados com sucesso no banco de dados!");
                        this.Close(); // Fecha a tela de perfil automaticamente
                    }
                    else
                    {
                        Toast.Aviso("Usuário não encontrado no banco de dados.");
                    }
                }
            }
            catch (Exception ex)
            {
                Toast.Erro($"Erro ao salvar alterações: {ex.Message}");
            }
        }

        // ==========================================================
        // 3. BOTÃO CANCELAR
        // ==========================================================
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}