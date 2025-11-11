using FlexCap.Web.Models;
using FlexCap.Web.Models.Requests;
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
        public DbSet<RequestEntity> Requests { get; set; }
        public DbSet<RequestLogEntity> RequestLogs { get; set; }
        public DbSet<RequestType> RequestTypes { get; set; }
        public DbSet<FlexCap.Web.Models.Calendar.CalendarModel> CalendarEvents { get; set; }

    }
}
