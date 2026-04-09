using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;
using OnClickSystem.Application.ViewModels;

namespace OnClickSystem.Application.Services
{
    public class UsuarioService
    {
        private readonly OnClickContext _context;
        private readonly RedeService _redeService;
        private readonly AuthService _authService;

        public UsuarioService(OnClickContext context, RedeService redeService, AuthService authService)
        {
            _context = context;
            _redeService = redeService;
            _authService = authService;
        }

        // ============================================================
        // PERFIL (MEUS DADOS) / ATUALIZAÇÃO SEGURA
        // ============================================================
        // Atualiza dados pessoais do usuário com validações:
        // - Impede e-mail duplicado
        // - Atualiza senha somente se o usuário digitou
        // - Faz hash da senha
        public async Task<(bool Sucesso, string Mensagem)> AtualizarPerfil(int id, Usuario dadosForm, string? novaSenha)
        {
            // Busca o usuário no banco pelo ID (entidade TRACKED)
            var usuarioDb = await _context.Usuarios.FindAsync(id);
            if (usuarioDb == null)
                return (false, "Usuário não encontrado.");

            // Se o e-mail mudou, checa se já existe outro usuário com esse e-mail
            if (usuarioDb.Email != dadosForm.Email)
            {
                bool emailEmUso = await _context.Usuarios.AnyAsync(u => u.Email == dadosForm.Email && u.ID != id);
                if (emailEmUso)
                    return (false, "Este e-mail já está sendo usado por outra pessoa.");
            }

            // Atualiza campos editáveis
            usuarioDb.Nome = dadosForm.Nome;
            usuarioDb.Email = dadosForm.Email;
            usuarioDb.Telefone = dadosForm.Telefone;
            usuarioDb.CPF = dadosForm.CPF;
            usuarioDb.ChavePix = dadosForm.ChavePix;
            usuarioDb.TipoChavePix = dadosForm.TipoChavePix;

            // Atualiza senha SOMENTE se veio uma nova
            if (!string.IsNullOrEmpty(novaSenha))
            {
                usuarioDb.Senha = _authService.GerarHashSenha(novaSenha);
            }

            try
            {
                // Aqui nem precisaria do Update() porque o FindAsync já retorna trackeado,
                // mas manter não quebra (só é redundante).
                _context.Usuarios.Update(usuarioDb);

                var linhas = await _context.SaveChangesAsync();
                if (linhas == 0)
                    return (false, "Nenhuma alteração foi salva no banco.");

                return (true, "Perfil atualizado com sucesso!");
            }
            catch (Exception ex)
            {
                return (false, "Erro ao salvar: " + ex.Message);
            }
        }

        // ============================================================
        // ADMIN: ALTERAR PERFIL / HIERARQUIA
        // ============================================================
        // IMPORTANTE: no seu sistema:
        // - Admin  => "Admin"
        // - Afiliado (na UI) => "Comum" (no banco / interno)
        // - Cliente => "Cliente"
        //
        // Então o service deve salvar "Comum" quando receber "Comum".
        public async Task<(bool Sucesso, string Mensagem)> AlterarPerfil(int id, string novoPerfil)
        {
            // Busca trackeada (ideal para alterar e salvar)
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return (false, "Usuário não encontrado.");

            // Normaliza entrada
            var perfilRecebido = (novoPerfil ?? "").Trim();

            // Validação de segurança: só aceita o que o sistema realmente usa
            // (Isso evita qualquer string aleatória virar perfil no banco)
            var perfisValidos = new[] { "Admin", "Comum", "Cliente" };

            if (!perfisValidos.Contains(perfilRecebido))
                return (false, $"Perfil inválido: '{perfilRecebido}'");

            // Se não mudou, evita salvar atoa
            if (usuario.Perfil == perfilRecebido)
                return (true, "Perfil já estava nesse nível.");

            // Atribui o novo perfil
            usuario.Perfil = perfilRecebido;

            // Garantia extra: marca explicitamente como modificado
            // (evita casos de “sucesso falso” por tracking/config)
            _context.Entry(usuario).Property(u => u.Perfil).IsModified = true;

            try
            {
                var linhas = await _context.SaveChangesAsync();

                // Se 0, significa que nada foi persistido (normalmente entity sem tracking, ou nenhum change detectado)
                if (linhas == 0)
                    return (false, "Nenhuma alteração foi salva no banco (0 linhas afetadas).");

                return (true, "Perfil alterado com sucesso!");
            }
            catch (Exception ex)
            {
                return (false, "Erro ao alterar perfil: " + ex.Message);
            }
        }

        // ============================================================
        // CADASTRO
        // ============================================================
        public async Task<(bool Sucesso, string Mensagem)> RegistrarUsuario(Usuario usuario)
        {
            bool emailExiste = await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email);
            if (emailExiste)
                return (false, "Este e-mail já está em uso.");

            if (!string.IsNullOrEmpty(usuario.CPF))
            {
                bool cpfExiste = await _context.Usuarios.AnyAsync(u => u.CPF == usuario.CPF);
                if (cpfExiste)
                    return (false, "Este CPF já está cadastrado.");
            }

            usuario.Senha = _authService.GerarHashSenha(usuario.Senha);

            // Padrões iniciais
            usuario.DataCadastro = DateTime.Now;
            usuario.Ativo = false;

            // No seu sistema, "Comum" é o perfil padrão (Afiliado)
            usuario.Perfil = "Comum";

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return (true, "Cadastro realizado! Faça login.");
        }

        // ============================================================
        // DETALHES / VISUALIZAÇÃO
        // ============================================================
        public async Task<Usuario?> ObterDetalhes(int id)
        {
            return await _context.Usuarios
                .Include(u => u.Patrocinador)
                .Include(u => u.Indicados)
                .FirstOrDefaultAsync(u => u.ID == id);
        }

        // ============================================================
        // LISTAGEM (ADMIN)
        // ============================================================
        public async Task<List<Usuario>> ListarUsuarios(string busca)
        {
            var query = _context.Usuarios.AsQueryable();

            if (!string.IsNullOrEmpty(busca))
            {
                query = query.Where(u => u.Nome.Contains(busca) || u.Email.Contains(busca));
            }

            return await query
                .OrderByDescending(u => u.DataCadastro)
                .ToListAsync();
        }

        // ============================================================
        // ADMIN: ATUALIZAR DADOS DE QUALQUER USUÁRIO
        // ============================================================
        public async Task<(bool Sucesso, string Mensagem)> AtualizarUsuario(int id, Usuario usuarioForm, string? novaSenha)
        {
            return await AtualizarPerfil(id, usuarioForm, novaSenha);
        }

        // ============================================================
        // ADMIN: ATIVAR/DESATIVAR
        // ============================================================
        public async Task<(bool Sucesso, string Mensagem)> AlternarStatus(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return (false, "Não encontrado.");

            usuario.Ativo = !usuario.Ativo;

            var linhas = await _context.SaveChangesAsync();
            if (linhas == 0)
                return (false, "Nenhuma alteração foi salva no banco.");

            return (true, "Status alterado.");
        }

        // ============================================================
        // ADMIN: DELETAR COM SEGURANÇA
        // ============================================================
        public async Task<(bool Sucesso, string Mensagem)> DeletarUsuario(int id)
        {
            var user = await _context.Usuarios.FindAsync(id);
            if (user == null)
                return (false, "Usuário não encontrado.");

            // 1. Verificações de segurança (Impede exclusão se houver vínculos importantes)
            bool temFilhos = await _context.Usuarios.AnyAsync(u => u.ID_Patrocinador == id);
            bool temTransacoes = await _context.Transacoes.AnyAsync(t => t.ID_Usuario == id);

            // Adicionamos a verificação de pedidos também!
            bool temPedidos = await _context.Pedidos.AnyAsync(p => p.ID_Usuario == id);

            if (temFilhos) return (false, "Não é possível excluir: Este usuário possui rede de indicados.");
            if (temTransacoes) return (false, "Não é possível excluir: Existem registros financeiros/transações.");
            if (temPedidos) return (false, "Não é possível excluir: Este usuário possui pedidos no sistema.");

            _context.Usuarios.Remove(user);

            // 2. Bloco Try-Catch para evitar que o sistema "quebre" caso existam outros vínculos (Comissões, Logs, etc)
            try
            {
                var linhas = await _context.SaveChangesAsync();
                if (linhas == 0)
                    return (false, "Nenhuma alteração foi salva no banco.");

                return (true, "Usuário removido definitivamente.");
            }
            catch (DbUpdateException)
            {
                // Este erro ocorre se o banco de dados barrar a exclusão por causa de tabelas relacionadas (Foreign Keys)
                return (false, "Não é possível excluir este usuário pois ele possui outros registros vinculados no banco de dados (como comissões, saques ou logs).");
            }
            catch (Exception ex)
            {
                // Qualquer outro erro inesperado
                return (false, $"Erro inesperado ao excluir: {ex.Message}");
            }
        }
    }
}
