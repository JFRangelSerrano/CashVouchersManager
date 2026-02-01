using Microsoft.EntityFrameworkCore;
using CashVouchersManager.Domain.Entities;

namespace CashVouchersManager.Infrastructure.Data;

/// <summary>
/// Database context for the Cash Vouchers Manager application
/// </summary>
public class CashVouchersDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CashVouchersDbContext"/> class
    /// </summary>
    /// <param name="options">The context options</param>
    public CashVouchersDbContext(DbContextOptions<CashVouchersDbContext> options) 
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the cash vouchers dataset
    /// </summary>
    public DbSet<CashVoucher> CashVouchers { get; set; }

    /// <summary>
    /// Configures the model for the database
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CashVoucher>(entity =>
        {
            entity.ToTable("CashVouchers");
            
            // No primary key configuration as per requirements
            entity.HasNoKey();

            // Configure Code property
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(13);

            // Create non-unique index on Code
            entity.HasIndex(e => e.Code)
                .HasDatabaseName("IX_CashVouchers_Code");

            // Configure Amount with 2 decimal precision
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Configure dates
            entity.Property(e => e.CreationDate)
                .IsRequired();

            entity.Property(e => e.RedemptionDate);

            entity.Property(e => e.ExpirationDate);

            // Configure store ID
            entity.Property(e => e.IssuingStoreId)
                .IsRequired();

            // Configure sale IDs
            entity.Property(e => e.IssuingSaleId)
                .HasMaxLength(128);

            entity.Property(e => e.RedemptionSaleId)
                .HasMaxLength(128);

            // Configure InUse flag
            entity.Property(e => e.InUse)
                .IsRequired()
                .HasDefaultValue(false);

            // Ignore calculated property
            entity.Ignore(e => e.Status);
        });
    }
}
