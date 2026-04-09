using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using OnClickSystem.Domain.Entities;
using OnClickSystem.Infrastructure.Data;
using OnClickSystem.Application.ViewModels;

namespace OnClickSystem.Application.Services
{
    public class RedeService
    {
        private readonly OnClickContext _context;

        public RedeService(OnClickContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Valida se a mudança de patrocinador é segura.
        /// Retorna (true, "") se for permitido ou (false, "motivo") se for proibido.
        /// </summary>
        public async Task<(bool Sucesso, string Mensagem)> ValidarMudancaPatrocinador(int idUsuario, int? idNovoPatrocinador)
        {
            // 1. Se virou 'raiz' (sem patrocinador), sempre pode.
            if (idNovoPatrocinador == null) return (true, "");

            // 2. Não pode ser pai de si mesmo.
            if (idNovoPatrocinador == idUsuario)
                return (false, "Um usuário não pode ser seu próprio patrocinador.");

            // 3. O novo patrocinador existe?
            var existePatrocinador = await _context.Usuarios.AnyAsync(u => u.ID == idNovoPatrocinador);
            if (!existePatrocinador)
                return (false, "O patrocinador informado não existe no sistema.");

            // 4. DETECÇÃO DE CICLO (Loop Infinito)
            // Verifica se o Novo Patrocinador é, na verdade, um filho/neto/bisneto do Usuário atual.
            bool vaiGerarCiclo = await VerificarSeEhDescendente(idUsuario, idNovoPatrocinador.Value);

            if (vaiGerarCiclo)
                return (false, "Ação negada: O novo patrocinador faz parte da rede descendente deste usuário (Ciclo detectado).");

            return (true, "");
        }

        /// <summary>
        /// Sobe a árvore genealógica do 'idSuspeito' para ver se encontra o 'idPai'.
        /// </summary>
        private async Task<bool> VerificarSeEhDescendente(int idPai, int idSuspeito)
        {
            int? atual = idSuspeito;
            int profundidadeMaxima = 100; // Segurança contra loops infinitos no while

            for (int i = 0; i < profundidadeMaxima; i++)
            {
                // Busca apenas o ID e o ID_Patrocinador para ser rápido
                var usuarioAtual = await _context.Usuarios
                    .Select(u => new { u.ID, u.ID_Patrocinador })
                    .FirstOrDefaultAsync(u => u.ID == atual);

                if (usuarioAtual == null) break; // Chegou num usuário inexistente

                // SE ENCONTRAMOS O PAI SUBINDO A ÁRVORE, É UM CICLO!
                if (usuarioAtual.ID_Patrocinador == idPai) return true;

                // Sobe um nível
                atual = usuarioAtual.ID_Patrocinador;

                // Se chegou na raiz da empresa (null), não é descendente.
                if (atual == null) return false;
            }

            return false;
        }
        // ============================================================
        // MOTOR DE BUSCA DE REDE AUTOMATIZADA (N NÍVEIS)
        // ============================================================
        public async Task<List<Usuario>> ObterRedeAbaixo(int idRaiz, int niveisMaximos = 10)
        {
            var redeCompleta = new List<Usuario>();

            // Começamos a busca a partir do ID do utilizador que está logado
            var idsNivelAtual = new List<int> { idRaiz };

            // O ciclo vai rodar de 1 até ao limite de níveis configurado (ex: de 1 a 10)
            for (int nivel = 1; nivel <= niveisMaximos; nivel++)
            {
                // Se não houver ninguém no nível anterior, paramos a busca para poupar o servidor
                if (!idsNivelAtual.Any()) break;

                // Busca todos os filhos de toda a gente que está no nível atual (Tudo numa única consulta rápida!)
                var usuariosDesteNivel = await _context.Usuarios
                    .Where(u => u.ID_Patrocinador != null && idsNivelAtual.Contains(u.ID_Patrocinador.Value))
                    .OrderByDescending(u => u.Ativo)
                    .ThenByDescending(u => u.DataCadastro)
                    .ToListAsync();

                // Se não encontrou ninguém neste nível, a rede acabou. Pode parar.
                if (!usuariosDesteNivel.Any()) break;

                // Etiqueta todas as pessoas encontradas com o número do nível atual (1, 2, 3...)
                foreach (var u in usuariosDesteNivel)
                {
                    u.NivelNaRede = nivel;
                }

                // Adiciona este grupo de pessoas à lista final
                redeCompleta.AddRange(usuariosDesteNivel);

                // Prepara a lista de IDs para buscar os filhos deles na PRÓXIMA volta do ciclo
                idsNivelAtual = usuariosDesteNivel.Select(u => u.ID).ToList();
            }

            return redeCompleta;
        }
    }
}