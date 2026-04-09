using Microsoft.EntityFrameworkCore;
using OnClickSystem.Domain.Entities;

namespace OnClickSystem.Infrastructure.Data
{
    public class OnClickContext : DbContext
    {
        public OnClickContext(DbContextOptions<OnClickContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Kit> Kits { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }

        // --- ESTAS ERAM AS LINHAS QUE FALTAVAM E CAUSAVAM O ERRO ---
        public DbSet<Comissao> Comissoes { get; set; }
        public DbSet<ConfiguracaoComissao> ConfiguracoesComissao { get; set; }
        public DbSet<Transacao> Transacoes { get; set; }
        public DbSet<LogSistema> LogsSistema { get; set; }
        public DbSet<SolicitacaoSaque> SolicitacoesSaque { get; set; }
        // -----------------------------------------------------------
    }
}