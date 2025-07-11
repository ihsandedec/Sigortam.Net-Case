using System;
using System.Collections.Generic;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public partial class SigortamNetDbContext : DbContext
{
    public SigortamNetDbContext()
    {
    }

    public SigortamNetDbContext(DbContextOptions<SigortamNetDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("Data Source=sigortamnet.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(e => e.Tckn, "IX_Customers_TCKN").IsUnique();

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.IsActive).HasDefaultValue(1);
            entity.Property(e => e.Tckn).HasColumnName("TCKN");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
