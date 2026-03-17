using Microsoft.EntityFrameworkCore;
using SIGRE_PYME.Models;

namespace SIGRE_PYME.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
    }
}