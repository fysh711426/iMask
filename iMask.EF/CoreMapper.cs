using iMask.EF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace iMask.EF
{
    public class CoreMapper
    {
        public void Map(EntityTypeBuilder<Amount> entity)
        {
            entity.ToTable("Amount");
            entity.HasKey(p => p.Id);
        }

        public void Map(EntityTypeBuilder<Query> entity)
        {
            entity.ToTable("Query");
            entity.HasKey(p => p.Id);
        }

        public void Map(EntityTypeBuilder<QueryAmount> entity)
        {
            entity.ToTable("QueryAmount");
            entity.HasKey(p => p.Id);

            entity.HasOne(p => p.Query).WithMany(p => p.QueryAmounts)
                .HasForeignKey(p => p.QueryId);
            entity.HasOne(p => p.Amount).WithMany(p => p.QueryAmounts)
                .HasForeignKey(p => p.AmountId);
        }
    }
}
