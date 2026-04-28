using Microsoft.EntityFrameworkCore;
using OnClickSystem.Infrastructure.Data;

namespace OnClickSystem.Desktop
{
    public static class ConfiguracaoBanco
    {
        // A string de conexão passa a viver num ÚNICO lugar seguro
        private const string StringDeConexao = "Server=(localdb)\\MSSQLLocalDB;Database=OnClickSystem;Trusted_Connection=True;TrustServerCertificate=True;";

        /// <summary>
        /// Gera e retorna um contexto do banco de dados pronto a usar.
        /// </summary>
        public static OnClickContext ObterContexto()
        {
            var optionsBuilder = new DbContextOptionsBuilder<OnClickContext>();
            optionsBuilder.UseSqlServer(StringDeConexao);

            // Retorna o contexto já configurado
            return new OnClickContext(optionsBuilder.Options);
        }
    }
}