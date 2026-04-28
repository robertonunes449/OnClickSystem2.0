using Microsoft.EntityFrameworkCore;
using OnClickSystem.Infrastructure.Data;
using OnClickSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BCrypt.Net; // Necessário para redefinir a palavra-passe

namespace OnClickSystem.Desktop
{
    public partial class FormAdminUsuarios : Form
    {
        private List<Usuario> _usuariosOriginais = new List<Usuario>();

        public FormAdminUsuarios()
        {
            InitializeComponent();
        }

        private void FormAdminUsuarios_Load(object sender, EventArgs e)
        {
            EstiloVisual.AplicarTemaPremium(dgvUsuarios);

            // Ativa o nosso Menu de Clique Direito "Super Poderoso"
            ConfigurarMenuCliqueDireito();

            CarregarUsuarios();
        }

        // ==========================================================
        // 1. MENU DE CLIQUE DIREITO (AGORA COM 5 FUNÇÕES PREMIUM)
        // ==========================================================
        private void ConfigurarMenuCliqueDireito()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Font = new Font("Segoe UI", 10F);
            menu.BackColor = Color.White;

            ToolStripMenuItem menuEditar = new ToolStripMenuItem("✏️ Editar Dados Pessoais");
            menuEditar.Click += (s, e) => EditarUsuario();

            ToolStripMenuItem menuSenha = new ToolStripMenuItem("🔑 Redefinir Palavra-passe");
            menuSenha.Click += (s, e) => RedefinirSenha();

            ToolStripMenuItem menuAdmin = new ToolStripMenuItem("👑 Promover/Remover Administrador");
            menuAdmin.Click += (s, e) => MudarPerfilAdmin();

            ToolStripMenuItem menuMudarHierarquia = new ToolStripMenuItem("🔄 Mudar Patrocinador (Hierarquia)");
            menuMudarHierarquia.Click += (s, e) => MudarHierarquia();

            ToolStripMenuItem menuExcluir = new ToolStripMenuItem("❌ Excluir Conta Permanentemente");
            menuExcluir.ForeColor = Color.FromArgb(220, 38, 38);
            menuExcluir.Click += (s, e) => DeletarUsuario();

            menu.Items.Add(menuEditar);
            menu.Items.Add(menuSenha);
            menu.Items.Add(menuAdmin);
            menu.Items.Add(new ToolStripSeparator()); // Linha divisória
            menu.Items.Add(menuMudarHierarquia);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(menuExcluir);

            if (dgvUsuarios != null)
            {
                dgvUsuarios.ContextMenuStrip = menu;
            }
        }

        // ==========================================================
        // 2. FUNÇÕES DO MENU SECRETO
        // ==========================================================

        private void EditarUsuario()
        {
            if (dgvUsuarios.SelectedRows.Count == 0) return;
            int idUsuario = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells["ID"].Value);

            using (var db = ConfiguracaoBanco.ObterContexto())
            {
                var usuario = db.Usuarios.Find(idUsuario);
                if (usuario == null) return;

                // Cria uma janela flutuante para edição rápida
                Form prompt = new Form() { Width = 350, Height = 280, FormBorderStyle = FormBorderStyle.FixedDialog, Text = "Editar Cliente", StartPosition = FormStartPosition.CenterParent, BackColor = Color.White };

                Label lblNome = new Label() { Left = 20, Top = 20, Text = "Nome Completo:", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
                TextBox txtNome = new TextBox() { Left = 20, Top = 45, Width = 290, Text = usuario.Nome, Font = new Font("Segoe UI", 11F) };

                Label lblTelefone = new Label() { Left = 20, Top = 90, Text = "Telefone:", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
                TextBox txtTelefone = new TextBox() { Left = 20, Top = 115, Width = 290, Text = usuario.Telefone, Font = new Font("Segoe UI", 11F) };

                Button btnSalvar = new Button() { Text = "Salvar Alterações", Left = 170, Top = 180, Width = 140, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(20, 83, 45), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

                prompt.Controls.Add(lblNome); prompt.Controls.Add(txtNome);
                prompt.Controls.Add(lblTelefone); prompt.Controls.Add(txtTelefone);
                prompt.Controls.Add(btnSalvar);
                prompt.AcceptButton = btnSalvar;

                if (prompt.ShowDialog() == DialogResult.OK)
                {
                    usuario.Nome = txtNome.Text.Trim();
                    usuario.Telefone = txtTelefone.Text.Trim();

                    db.LogsSistema.Add(new LogSistema { DataHora = DateTime.Now, UsuarioResponsavel = SessaoAtual.UsuarioLogado.Email, Acao = "Alteração de perfil", Detalhes = $"Dados atualizados pelo administrador para o usuário ID {usuario.ID} ({usuario.Email})." });
                    db.SaveChanges();
                    Toast.Sucesso("Dados atualizados com sucesso!");
                    CarregarUsuarios();
                }
            }
        }

        private void RedefinirSenha()
        {
            if (dgvUsuarios.SelectedRows.Count == 0) return;
            int idUsuario = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells["ID"].Value);
            string nomeUsuario = dgvUsuarios.SelectedRows[0].Cells["Nome"].Value.ToString();

            if (MessageBox.Show($"Deseja redefinir a palavra-passe de '{nomeUsuario}' para a senha padrão '123456'?", "Redefinir Palavra-passe", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (var db = ConfiguracaoBanco.ObterContexto())
                {
                    var usuario = db.Usuarios.Find(idUsuario);
                    if (usuario != null)
                    {
                        usuario.Senha = BCrypt.Net.BCrypt.HashPassword("123456"); // Gera a senha segura no banco

                        db.LogsSistema.Add(new LogSistema { DataHora = DateTime.Now, UsuarioResponsavel = SessaoAtual.UsuarioLogado.Email, Acao = "Alteração de perfil", Detalhes = $"A palavra-passe de '{usuario.Email}' foi redefinida para o padrão." });
                        db.SaveChanges();
                        Toast.Sucesso("Palavra-passe redefinida para '123456'!");
                    }
                }
            }
        }

        private void MudarPerfilAdmin()
        {
            if (dgvUsuarios.SelectedRows.Count == 0) return;
            int idUsuario = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells["ID"].Value);

            using (var db = ConfiguracaoBanco.ObterContexto())
            {
                var usuario = db.Usuarios.Find(idUsuario);
                if (usuario != null)
                {
                    if (usuario.ID == SessaoAtual.UsuarioLogado.ID)
                    {
                        Toast.Aviso("Não pode alterar o seu próprio nível de acesso!");
                        return;
                    }

                    string novoPerfil = usuario.Perfil == "Admin" ? "Usuario" : "Admin";
                    string mensagem = usuario.Perfil == "Admin" ? $"Retirar os poderes de Administrador de '{usuario.Nome}'?" : $"Tornar '{usuario.Nome}' num Administrador do sistema?";

                    if (MessageBox.Show(mensagem, "Alterar Permissões", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        usuario.Perfil = novoPerfil;
                        db.LogsSistema.Add(new LogSistema { DataHora = DateTime.Now, UsuarioResponsavel = SessaoAtual.UsuarioLogado.Email, Acao = "Alteração de permissões", Detalhes = $"O perfil de '{usuario.Email}' mudou para {novoPerfil}." });
                        db.SaveChanges();
                        Toast.Sucesso("Permissões alteradas com sucesso!");
                        CarregarUsuarios();
                    }
                }
            }
        }

        private void MudarHierarquia()
        {
            if (dgvUsuarios.SelectedRows.Count == 0) return;
            int idUsuario = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells["ID"].Value);
            string nomeUsuario = dgvUsuarios.SelectedRows[0].Cells["Nome"].Value.ToString();

            Form prompt = new Form() { Width = 400, Height = 220, FormBorderStyle = FormBorderStyle.FixedDialog, Text = "Mudar Hierarquia", StartPosition = FormStartPosition.CenterParent, BackColor = Color.White };
            Label textLabel = new Label() { Left = 20, Top = 20, Width = 350, Text = $"Digite o ID do novo patrocinador para:\n{nomeUsuario}", Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            TextBox inputBox = new TextBox() { Left = 20, Top = 80, Width = 340, Font = new Font("Segoe UI", 12F) };
            Button confirmation = new Button() { Text = "Salvar Mudança", Left = 220, Top = 130, Width = 140, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(212, 175, 55), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            prompt.Controls.Add(textLabel); prompt.Controls.Add(inputBox); prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;

            if (prompt.ShowDialog() == DialogResult.OK)
            {
                if (int.TryParse(inputBox.Text, out int novoPatrocinadorId))
                {
                    using (var db = ConfiguracaoBanco.ObterContexto())
                    {
                        var usuario = db.Usuarios.Find(idUsuario);
                        var novoSponsor = db.Usuarios.Find(novoPatrocinadorId);

                        if (usuario == null || novoSponsor == null) { Toast.Erro("ID inválido ou não encontrado."); return; }
                        if (usuario.ID == novoSponsor.ID) { Toast.Aviso("Não pode ser patrocinador de si mesmo!"); return; }

                        usuario.ID_Patrocinador = novoSponsor.ID;
                        db.LogsSistema.Add(new LogSistema { DataHora = DateTime.Now, UsuarioResponsavel = SessaoAtual.UsuarioLogado.Email, Acao = "Alteração de status", Detalhes = $"'{usuario.Nome}' foi movido para a rede de '{novoSponsor.Nome}' (ID {novoSponsor.ID})." });
                        db.SaveChanges();
                        Toast.Sucesso("Hierarquia atualizada!");
                        CarregarUsuarios();
                    }
                }
            }
        }

        private void DeletarUsuario()
        {
            if (dgvUsuarios.SelectedRows.Count == 0) return;
            int idUsuario = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells["ID"].Value);
            string nomeUsuario = dgvUsuarios.SelectedRows[0].Cells["Nome"].Value.ToString();

            if (MessageBox.Show($"Deseja EXCLUIR PERMANENTEMENTE '{nomeUsuario}'?", "Exclusão de Conta", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    using (var db = ConfiguracaoBanco.ObterContexto())
                    {
                        var usuario = db.Usuarios.Find(idUsuario);
                        if (usuario != null)
                        {
                            db.Usuarios.Remove(usuario);
                            db.LogsSistema.Add(new LogSistema { DataHora = DateTime.Now, UsuarioResponsavel = SessaoAtual.UsuarioLogado.Email, Acao = "Exclusão de Conta", Detalhes = $"Conta de '{nomeUsuario}' excluída do sistema." });
                            db.SaveChanges();
                            Toast.Sucesso("Usuário excluído!");
                            CarregarUsuarios();
                        }
                    }
                }
                catch (Exception)
                {
                    Toast.Erro("Bloqueado: Este utilizador já possui transações financeiras ou indicados na rede.");
                }
            }
        }

        // ==========================================================
        // 3. CARREGAR E PINTAR A TABELA
        // ==========================================================
        private void CarregarUsuarios()
        {
            using (var db = ConfiguracaoBanco.ObterContexto())
            {
                _usuariosOriginais = db.Usuarios.OrderByDescending(u => u.DataCadastro).ToList();
                AtualizarGrid(txtBuscar.Text);
            }
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            AtualizarGrid(txtBuscar.Text);
        }

        private void AtualizarGrid(string filtro)
        {
            string busca = filtro.ToLower();

            var filtrados = _usuariosOriginais.Where(u =>
                string.IsNullOrWhiteSpace(busca) ||
                (u.Nome != null && u.Nome.ToLower().Contains(busca)) ||
                (u.Email != null && u.Email.ToLower().Contains(busca)) ||
                (u.CPF != null && u.CPF.Contains(busca))
            ).ToList();

            var listaParaGrid = filtrados.Select(u => new
            {
                ID = u.ID,
                Nome = u.Nome,
                Perfil = u.Perfil, // Mostramos o Perfil agora na tabela!
                Contato = $"{u.Email}\n{u.Telefone ?? "-"}",
                Status = u.Ativo ? "Ativo" : "Inativo",
                Cadastro = u.DataCadastro.ToString("dd/MM/yyyy")
            }).ToList();

            dgvUsuarios.DataSource = listaParaGrid;

            if (dgvUsuarios.Columns.Count > 0)
            {
                dgvUsuarios.Columns["ID"].Width = 40;
                dgvUsuarios.Columns["Nome"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvUsuarios.Columns["Perfil"].Width = 80;
                dgvUsuarios.Columns["Contato"].Width = 200;
                dgvUsuarios.Columns["Contato"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                dgvUsuarios.Columns["Status"].Width = 80;
                dgvUsuarios.Columns["Cadastro"].Width = 100;
            }
        }

        private void dgvUsuarios_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvUsuarios.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
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

            // Destaca os Administradores de cor Dourada!
            if (dgvUsuarios.Columns[e.ColumnIndex].Name == "Perfil" && e.Value != null)
            {
                if (e.Value.ToString() == "Admin")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(212, 175, 55);
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
            }
        }

        // Mantidos para evitar erros caso ainda estejam interligados no Designer
        private void btnAtivar_Click(object sender, EventArgs e) { MudarStatus(true); }
        private void btnBloquear_Click(object sender, EventArgs e) { MudarStatus(false); }

        private void MudarStatus(bool ativar)
        {
            if (dgvUsuarios.SelectedRows.Count == 0) return;
            int idUsuario = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells["ID"].Value);
            string msg = ativar ? "Deseja ATIVAR esta conta?" : "Deseja BLOQUEAR esta conta?";

            if (MessageBox.Show(msg, "Alterar Status", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (var db = ConfiguracaoBanco.ObterContexto())
                {
                    var u = db.Usuarios.Find(idUsuario);
                    if (u != null)
                    {
                        u.Ativo = ativar;
                        db.LogsSistema.Add(new LogSistema { DataHora = DateTime.Now, UsuarioResponsavel = SessaoAtual.UsuarioLogado.Email, Acao = "Alteração de status", Detalhes = $"Conta de {u.Email} " + (ativar ? "ativada" : "bloqueada") + "." });
                        db.SaveChanges();
                        Toast.Sucesso("Status alterado!");
                        CarregarUsuarios();
                    }
                }
            }
        }
    }
}