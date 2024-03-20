using Microsoft.EntityFrameworkCore;

namespace Nexum_WebApp.DataAccess
{
    public class NexumWebAppDbContext : DbContext
    {
        public NexumWebAppDbContext(DbContextOptions options)
        : base(options)
        {
        }
        public static async Task Intitalize(IServiceProvider serviceProvider)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}
