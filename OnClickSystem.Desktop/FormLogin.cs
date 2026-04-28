using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using OnClickSystem.Infrastructure.Data;
using OnClickSystem.Domain.Entities;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OnClickSystem.Desktop
{
    public partial class FormLogin : Form
    {
        // O caminho seguro onde o Windows vai guardar o ficheiro "invisível" do seu Login
        private string arquivoLembrar = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OnClickSystem", "login.dat");

        public FormLogin()
        {
            InitializeComponent();
            // Garante que o sistema tenta ler a senha logo que a tela abrir
            this.Load += FormLogin_Load;
        }

        // ==========================================================
        // 1. QUANDO A TELA ABRE (Tenta preencher sozinho)
        // ==========================================================
        private void FormLogin_Load(object sender, EventArgs e)
        {
            if (File.Exists(arquivoLembrar))
            {
                try
                {
                    // Lê e decodifica o ficheiro secreto
                    string textoCodificado = File.ReadAllText(arquivoLembrar);
                    string textoDecodificado = Encoding.UTF8.GetString(Convert.FromBase64String(textoCodificado));

                    string[] dados = textoDecodificado.Split('|');
                    if (dados.Length == 2)
                    {
                        txtEmail.Text = dados[0];
                        txtSenha.Text = dados[1];

                        // Encontra a sua caixinha e marca-a como "Check"
                        var controleChave = this.Controls.Find("chkLembrarSenha", true).FirstOrDefault();
                        if (controleChave is CheckBox cb) cb.Checked = true;
                        else if (controleChave != null)
                        {
                            var prop = controleChave.GetType().GetProperty("Checked");
                            if (prop != null) prop.SetValue(controleChave, true);
                        }
                    }
                }
                catch
                {
                    // Se o ficheiro estiver corrompido, ignora sem travar o sistema
                }
            }
        }

        // ==========================================================
        // 2. BOTÃO DE ENTRAR (Valida e Guarda a Senha)
        // ==========================================================
        private void btnEntrar_Click(object sender, EventArgs e)
        {
            string emailDigitado = txtEmail.Text.Trim();
            string senhaDigitada = txtSenha.Text;

            if (string.IsNullOrEmpty(emailDigitado) || string.IsNullOrEmpty(senhaDigitada))
            {
                Toast.Aviso("Preencha o e-mail e a palavra-passe.");
                return;
            }

            try
            {
                using (var db = ConfiguracaoBanco.ObterContexto())
                {
                    var utilizador = db.Usuarios.FirstOrDefault(u => u.Email == emailDigitado);

                    if (utilizador != null)
                    {
                        bool senhaCorreta = false;

                        try
                        {
                            senhaCorreta = BCrypt.Net.BCrypt.Verify(senhaDigitada, utilizador.Senha);
                        }
                        catch (BCrypt.Net.SaltParseException)
                        {
                            if (senhaDigitada == utilizador.Senha)
                            {
                                senhaCorreta = true;
                                utilizador.Senha = BCrypt.Net.BCrypt.HashPassword(senhaDigitada);
                                db.SaveChanges();
                            }
                        }

                        if (senhaCorreta)
                        {
                            if (!utilizador.Ativo && utilizador.Perfil != "Admin")
                            {
                                Toast.Erro("Conta inativa. Contacte o administrador.");
                                return;
                            }

                            // ========================================================
                            // LÓGICA DE SALVAR OU APAGAR A SENHA
                            // ========================================================
                            bool lembrar = false;
                            var controleChave = this.Controls.Find("chkLembrarSenha", true).FirstOrDefault();

                            if (controleChave is CheckBox cb) lembrar = cb.Checked;
                            else if (controleChave != null)
                            {
                                var prop = controleChave.GetType().GetProperty("Checked");
                                if (prop != null) lembrar = (bool)prop.GetValue(controleChave);
                            }

                            if (lembrar)
                            {
                                string pasta = Path.GetDirectoryName(arquivoLembrar);
                                if (!Directory.Exists(pasta)) Directory.CreateDirectory(pasta);

                                // Aplica uma codificação Base64 para não guardar a senha em texto puro
                                string textoParaSalvar = $"{emailDigitado}|{senhaDigitada}";
                                string textoCodificado = Convert.ToBase64String(Encoding.UTF8.GetBytes(textoParaSalvar));
                                File.WriteAllText(arquivoLembrar, textoCodificado);
                            }
                            else
                            {
                                // Se você desmarcar a caixinha, o sistema apaga o ficheiro salvo
                                if (File.Exists(arquivoLembrar)) File.Delete(arquivoLembrar);
                            }

                            // AUDITORIA: LOGIN COM SUCESSO
                            var logSucesso = new LogSistema
                            {
                                DataHora = DateTime.Now,
                                UsuarioResponsavel = utilizador.Email,
                                Acao = "Login Realizado",
                                Detalhes = $"Acesso autorizado no sistema Desktop. Perfil logado: {utilizador.Perfil}."
                            };
                            db.LogsSistema.Add(logSucesso);
                            db.SaveChanges();

                            SessaoAtual.UsuarioLogado = utilizador;
                            FormPrincipal formPrincipal = new FormPrincipal();

                            // -------------------------------------------------------------
                            // AQUI ESTÁ A MUDANÇA!
                            // Como o login agora é um popup em cima do vídeo, precisamos
                            // garantir que o programa todo fecha quando sairmos do FormPrincipal
                            // -------------------------------------------------------------
                            formPrincipal.FormClosed += (s, args) => System.Windows.Forms.Application.Exit();

                            this.Hide();
                            formPrincipal.Show();
                        }
                        else
                        {
                            // AUDITORIA: FALHA SENHA
                            var logFalhaSenha = new LogSistema { DataHora = DateTime.Now, UsuarioResponsavel = emailDigitado, Acao = "Falha de login", Detalhes = "Tentativa de login bloqueada: Senha incorreta informada no Desktop." };
                            db.LogsSistema.Add(logFalhaSenha);
                            db.SaveChanges();

                            Toast.Erro("E-mail ou palavra-passe incorretos.");
                        }
                    }
                    else
                    {
                        // AUDITORIA: EMAIL FALSO
                        var logFalhaEmail = new LogSistema { DataHora = DateTime.Now, UsuarioResponsavel = emailDigitado, Acao = "Falha de login", Detalhes = "Tentativa de login bloqueada: E-mail não registado no sistema." };
                        db.LogsSistema.Add(logFalhaEmail);
                        db.SaveChanges();

                        Toast.Erro("E-mail ou palavra-passe incorretos.");
                    }
                }
            }
            catch (Exception ex)
            {
                Toast.Erro("Erro de servidor:\n" + ex.Message);
            }
        }


        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            // Força o encerramento total do programa imediatamente
            System.Windows.Forms.Application.Exit();
        }
    }
}