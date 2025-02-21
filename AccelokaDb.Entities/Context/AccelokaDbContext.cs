using System;
using System.Collections.Generic;
using AccelokaDb.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace AccelokaDb.Entities.Context;

public partial class AccelokaDbContext : DbContext
{

    public AccelokaDbContext(DbContextOptions<AccelokaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BookedTicket> BookedTickets { get; set; }

    public virtual DbSet<Tiket> Tikets { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=HelloWorldHello;Database=acceloka_db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookedTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("booked_tickets_pkey");

            entity.ToTable("booked_tickets");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BookedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("booked_date");
            entity.Property(e => e.KodeTiket)
                .HasMaxLength(50)
                .HasColumnName("kode_tiket");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.KodeTiketNavigation).WithMany(p => p.BookedTickets)
                .HasPrincipalKey(p => p.KodeTiket)
                .HasForeignKey(d => d.KodeTiket)
                .HasConstraintName("booked_tickets_kode_tiket_fkey");
        });

        modelBuilder.Entity<Tiket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tickets_pkey");

            entity.ToTable("tickets");

            entity.HasIndex(e => e.KodeTiket, "tickets_kode_tiket_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Harga)
                .HasPrecision(10, 2)
                .HasColumnName("harga");
            entity.Property(e => e.KodeTiket)
                .HasMaxLength(50)
                .HasColumnName("kode_tiket");
            entity.Property(e => e.NamaKategori)
                .HasMaxLength(100)
                .HasColumnName("nama_kategori");
            entity.Property(e => e.NamaTiket)
                .HasMaxLength(100)
                .HasColumnName("nama_tiket");
            entity.Property(e => e.SisaQuota).HasColumnName("sisa_quota");
            entity.Property(e => e.TanggalEvent)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("tanggal_event");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
