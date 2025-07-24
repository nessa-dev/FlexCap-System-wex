using Microsoft.EntityFrameworkCore;
using FlexCap.Web.Models; // importa a model Usuario

namespace FlexCap.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
    }
}
