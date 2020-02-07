using iMask.EF.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace iMask.EF
{
    public class CoreDbContext : DbContext
    {
        public CoreDbContext(DbContextOptions<CoreDbContext> options)
            : base(options)
        {
        }

        public DbSet<Shop> Shops { get; set; }
        public DbSet<Amount> Amounts { get; set; }
        public DbSet<Query> Querys { get; set; }
        public DbSet<QueryShop> QueryShops { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var mapper = new CoreMapper();

            modelBuilder.Entity<Shop>(entity => mapper.Map(entity));
            modelBuilder.Entity<Amount>(entity => mapper.Map(entity));
            modelBuilder.Entity<Query>(entity => mapper.Map(entity));
            modelBuilder.Entity<QueryShop>(entity => mapper.Map(entity));
        }
    }
}
