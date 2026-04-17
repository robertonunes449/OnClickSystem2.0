using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OnClickSystem.Infrastructure.Data
{
    public class OnClickContextFactory : IDesignTimeDbContextFactory<OnClickContext>
    {
        public OnClickContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OnClickContext>();

            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=OnClickSystem;Trusted_Connection=True;TrustServerCertificate=True;");

            return new OnClickContext(optionsBuilder.Options);
        }
    }
}