using BlueCommand.Domain.Entities;
using BlueCommand.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlueCommand.Infrastructure.Data;

public class BlueCommandDbContext : DbContext
{
    public BlueCommandDbContext(DbContextOptions<BlueCommandDbContext> options) : base(options)
    {
    }

    public DbSet<Rol> Roluri => Set<Rol>();
    public DbSet<Sectie> Sectii => Set<Sectie>();
    public DbSet<Utilizator> Utilizatori => Set<Utilizator>();
    public DbSet<Dosar> Dosare => Set<Dosar>();
    public DbSet<DosarAgent> DosarAgenti => Set<DosarAgent>();
    public DbSet<DocumentDosar> DocumenteDosar => Set<DocumentDosar>();
    public DbSet<Raport> Rapoarte => Set<Raport>();
    public DbSet<IstoricSectie> IstoricSectii => Set<IstoricSectie>();
    public DbSet<IstoricUtilizator> IstoricUtilizatori => Set<IstoricUtilizator>();
    public DbSet<IstoricDosar> IstoricDosare => Set<IstoricDosar>();
    public DbSet<AuditLog> AuditLog => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>(b =>
        {
            b.ToTable("roluri");
            b.HasKey(x => x.Id);
            b.Property(x => x.Denumire).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Sectie>(b =>
        {
            b.ToTable("sectii");
            b.HasKey(x => x.Id);
            b.Property(x => x.Nume).HasMaxLength(100).IsRequired();
            b.Property(x => x.Adresa).HasMaxLength(200);
            b.Property(x => x.Zona).HasMaxLength(100);
            b.Property(x => x.CreatLa).HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<Utilizator>(b =>
        {
            b.ToTable("utilizatori");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Username).IsUnique();
            b.Property(x => x.Username).HasMaxLength(100).IsRequired();
            b.Property(x => x.ParolaHash).HasMaxLength(255).IsRequired();
            b.Property(x => x.Nume).HasMaxLength(100);
            b.Property(x => x.Prenume).HasMaxLength(100);
            b.Property(x => x.Grad).HasMaxLength(100);
            b.Property(x => x.DataCreare).HasDefaultValueSql("NOW()");
            b.Property(x => x.EsteActiv).HasDefaultValue(true);

            b.HasOne(x => x.Rol).WithMany(x => x.Utilizatori).HasForeignKey(x => x.RolId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Sectie).WithMany(x => x.Utilizatori).HasForeignKey(x => x.SectieId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Dosar>(b =>
        {
            b.ToTable("dosare");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.NumarDosar).IsUnique();
            b.Property(x => x.NumarDosar).HasMaxLength(50).IsRequired();
            b.Property(x => x.Titlu).HasMaxLength(200);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(DosarStatus.DESCHIS);
            b.Property(x => x.TipIncident).HasMaxLength(100);
            b.Property(x => x.CreatLa).HasDefaultValueSql("NOW()");

            b.HasOne(x => x.Sectie).WithMany(x => x.Dosare).HasForeignKey(x => x.SectieId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.CreatDeUtilizator).WithMany(x => x.DosareCreate).HasForeignKey(x => x.CreatDe).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DosarAgent>(b =>
        {
            b.ToTable("dosar_agenti");
            b.HasKey(x => new { x.DosarId, x.UtilizatorId });
            b.HasOne(x => x.Dosar).WithMany(x => x.DosarAgenti).HasForeignKey(x => x.DosarId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Utilizator).WithMany(x => x.DosarAgenti).HasForeignKey(x => x.UtilizatorId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentDosar>(b =>
        {
            b.ToTable("documente_dosar");
            b.HasKey(x => x.Id);
            b.Property(x => x.NumeFisier).HasMaxLength(255).IsRequired();
            b.Property(x => x.CaleFisier).HasMaxLength(500).IsRequired();
            b.Property(x => x.DataIncarcare).HasDefaultValueSql("NOW()");

            b.HasOne(x => x.Dosar).WithMany(x => x.Documente).HasForeignKey(x => x.DosarId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.IncarcatDeUtilizator).WithMany(x => x.DocumenteIncarcate).HasForeignKey(x => x.IncarcatDe).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Raport>(b =>
        {
            b.ToTable("rapoarte");
            b.HasKey(x => x.Id);
            b.Property(x => x.Tip).HasMaxLength(50).IsRequired();
            b.Property(x => x.FiltruPerioada).HasMaxLength(100);
            b.Property(x => x.CaleFisier).HasMaxLength(500);
            b.Property(x => x.DataGenerare).HasDefaultValueSql("NOW()");
            b.HasOne(x => x.Utilizator).WithMany(x => x.Rapoarte).HasForeignKey(x => x.UtilizatorId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IstoricSectie>(b =>
        {
            b.ToTable("istoric_sectii");
            b.HasKey(x => x.Id);
            b.Property(x => x.CampModificat).HasMaxLength(100).IsRequired();
            b.Property(x => x.ModificatLa).HasDefaultValueSql("NOW()");
            b.HasOne(x => x.Sectie).WithMany(x => x.Istoric).HasForeignKey(x => x.SectieId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.ModificatDeUtilizator).WithMany().HasForeignKey(x => x.ModificatDe).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<IstoricUtilizator>(b =>
        {
            b.ToTable("istoric_utilizatori");
            b.HasKey(x => x.Id);
            b.Property(x => x.CampModificat).HasMaxLength(100).IsRequired();
            b.Property(x => x.ModificatLa).HasDefaultValueSql("NOW()");
            b.HasOne(x => x.Utilizator).WithMany().HasForeignKey(x => x.UtilizatorId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.ModificatDeUtilizator).WithMany().HasForeignKey(x => x.ModificatDe).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<IstoricDosar>(b =>
        {
            b.ToTable("istoric_dosare");
            b.HasKey(x => x.Id);
            b.Property(x => x.CampModificat).HasMaxLength(100).IsRequired();
            b.Property(x => x.ModificatLa).HasDefaultValueSql("NOW()");
            b.HasOne(x => x.Dosar).WithMany(x => x.Istoric).HasForeignKey(x => x.DosarId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.ModificatDeUtilizator).WithMany().HasForeignKey(x => x.ModificatDe).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(b =>
        {
            b.ToTable("audit_log");
            b.HasKey(x => x.Id);
            b.Property(x => x.Actiune).HasMaxLength(200).IsRequired();
            b.Property(x => x.IpAdresa).HasMaxLength(50);
            b.Property(x => x.CreatLa).HasDefaultValueSql("NOW()");
            b.HasOne(x => x.Utilizator).WithMany().HasForeignKey(x => x.UtilizatorId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}

