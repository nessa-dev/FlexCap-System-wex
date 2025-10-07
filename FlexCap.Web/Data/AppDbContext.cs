using Microsoft.EntityFrameworkCore;
using FlexCap.Web.Models;

namespace FlexCap.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Colaborador> Colaboradores { get; set; }
    }
}
