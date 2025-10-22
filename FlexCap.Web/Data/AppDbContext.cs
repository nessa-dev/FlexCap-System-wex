using FlexCap.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FlexCap.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Colaborador> Colaboradores { get; set; }
        public DbSet<TabelaTeste> DadosDeTeste { get; set; } = default!;
    }
}
