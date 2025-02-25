using System;
using ScooterDomain.Model;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ScooterInfrastructure;


public partial class ScootersContext : DbContext
{
    public ScootersContext()
    {
    }

    public ScootersContext(DbContextOptions<ScootersContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChargingStation> ChargingStations { get; set; }

    public virtual DbSet<Discount> Discounts { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Rental> Rentals { get; set; }

    public virtual DbSet<RentalStatus> RentalStatuses { get; set; }

    public virtual DbSet<Rider> Riders { get; set; }

    public virtual DbSet<Scooter> Scooters { get; set; }

    public virtual DbSet<ScooterStatus> ScooterStatuses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-4H0AUGU\\SQLEXPRESS; Database=Scooters; Trusted_Connection=True; TrustServerCertificate=True; ");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChargingStation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Charging__3214EC0740E33A38");

            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Discount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Discount__3214EC07DFFA9BD7");

            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Percentage).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PaymentM__3214EC07BFE55B0F");

            entity.HasIndex(e => e.Name, "UQ_PaymentMethods_Name").IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Rental>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Rentals__3214EC073B94991F");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.TotalCost).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Rentals)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Rentals__Payment__5441852A");

            entity.HasOne(d => d.Rider).WithMany(p => p.Rentals)
                .HasForeignKey(d => d.RiderId)
                .HasConstraintName("FK__Rentals__RiderId__5165187F");

            entity.HasOne(d => d.Scooter).WithMany(p => p.Rentals)
                .HasForeignKey(d => d.ScooterId)
                .HasConstraintName("FK__Rentals__Scooter__52593CB8");

            entity.HasOne(d => d.Status).WithMany(p => p.Rentals)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK__Rentals__StatusI__534D60F1");
        });

        modelBuilder.Entity<RentalStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RentalSt__3214EC07F11819E6");

            entity.HasIndex(e => e.Name, "UQ_RentalStatuses_Name").IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Rider>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Rider__3214EC071A080A86");

            entity.ToTable("Rider");

            entity.HasIndex(e => e.PhoneNumber, "UQ__Rider__85FB4E38D8C6C142").IsUnique();

            entity.Property(e => e.AccountBalance).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);

            entity.HasMany(d => d.Discounts).WithMany(p => p.Riders)
                .UsingEntity<Dictionary<string, object>>(
                    "RiderDiscount",
                    r => r.HasOne<Discount>().WithMany()
                        .HasForeignKey("DiscountId")
                        .HasConstraintName("FK__RiderDisc__Disco__48CFD27E"),
                    l => l.HasOne<Rider>().WithMany()
                        .HasForeignKey("RiderId")
                        .HasConstraintName("FK__RiderDisc__Rider__47DBAE45"),
                    j =>
                    {
                        j.HasKey("RiderId", "DiscountId").HasName("PK__RiderDis__03319AB9DCE2EE60");
                        j.ToTable("RiderDiscounts");
                    });
        });

        modelBuilder.Entity<Scooter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Scooters__3214EC0705145E7D");

            entity.Property(e => e.CurrentLocation)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Model)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Station).WithMany(p => p.Scooters)
                .HasForeignKey(d => d.StationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Scooters__Statio__3F466844");

            entity.HasOne(d => d.Status).WithMany(p => p.Scooters)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK__Scooters__Status__3E52440B");
        });

        modelBuilder.Entity<ScooterStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ScooterS__3214EC073261A798");

            entity.HasIndex(e => e.Name, "UQ_ScooterStatuses_Name").IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
