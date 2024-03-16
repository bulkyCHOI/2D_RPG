using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedDB
{
    public class SharedDbContext : DbContext
    {
        public DbSet<TokenDb> Tokens { get; set; }
        public DbSet<ServerInfoDb> ServerInfos { get; set; }

        // ASP.NET
        public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options)
        {
        
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TokenDb>()
                .HasIndex(p => p.AccountDbId)
                .IsUnique();

            modelBuilder.Entity<ServerInfoDb>()
                .HasIndex(s => s.ServerName)
                .IsUnique();
        }

        // GameServer
        public SharedDbContext()
        {
            
        }
        public static string ConnectionString { get; set; } = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SharedDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(optionsBuilder.IsConfigured == false)    //ASP.NET에서 이미 Configuring을 했기 때문에 다시 하지 않도록 한다.
                optionsBuilder.UseSqlServer(ConnectionString);
        }

    }
}
