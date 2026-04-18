using InvoiceManager.Domain.Audit;
using InvoiceManager.Domain.Bills;
using InvoiceManager.Domain.Licensing;
using InvoiceManager.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Persistence;

public sealed class InvoiceManagerDbContext(DbContextOptions<InvoiceManagerDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<BillCategory> BillCategories => Set<BillCategory>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<BillAttachment> BillFiles => Set<BillAttachment>();
    public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();
    public DbSet<ReminderRule> ReminderRules => Set<ReminderRule>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<LicenseBinding> LicenseBindings => Set<LicenseBinding>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Username).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<BillCategory>(entity =>
        {
            entity.ToTable("bill_categories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.HasIndex(x => new { x.Type, x.Name }).IsUnique();
        });

        modelBuilder.Entity<Bill>(entity =>
        {
            entity.ToTable("bills");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReferenceNumber).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.PaymentStatus).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PropertyName).HasMaxLength(300).IsRequired();
            entity.Property(x => x.ProviderName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.AccountNumber).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Currency).HasMaxLength(10).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Notes).HasMaxLength(4000);
            entity.Property(x => x.Keywords).HasMaxLength(1000);
            entity.HasIndex(x => x.ReferenceNumber);
            entity.HasIndex(x => x.CustomerName);
            entity.HasIndex(x => x.PropertyName);
            entity.HasIndex(x => x.Type);
            entity.HasIndex(x => x.PaymentStatus);
            entity.HasOne(x => x.BillCategory)
                .WithMany(x => x.Bills)
                .HasForeignKey(x => x.BillCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Attachments)
                .WithOne(x => x.Bill)
                .HasForeignKey(x => x.BillId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.PaymentRecords)
                .WithOne(x => x.Bill)
                .HasForeignKey(x => x.BillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BillAttachment>(entity =>
        {
            entity.ToTable("bill_files");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<PaymentRecord>(entity =>
        {
            entity.ToTable("payment_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AmountPaid).HasPrecision(18, 2);
            entity.Property(x => x.PaymentMethod).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ReferenceNumber).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Note).HasMaxLength(1000);
            entity.HasIndex(x => x.BillId);
            entity.HasIndex(x => x.PaidOn);
        });

        modelBuilder.Entity<ReminderRule>(entity =>
        {
            entity.ToTable("reminder_rules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Channel).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Recipient).HasMaxLength(250).IsRequired();
            entity.Property(x => x.BillType).HasConversion<string>().HasMaxLength(32);
            entity.HasOne(x => x.BillCategory)
                .WithMany(x => x.ReminderRules)
                .HasForeignKey(x => x.BillCategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Username).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EntityId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.MetadataJson).HasColumnType("text");
            entity.HasIndex(x => x.OccurredAtUtc);
        });

        modelBuilder.Entity<LicenseBinding>(entity =>
        {
            entity.ToTable("license_bindings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LicenseId).HasMaxLength(120).IsRequired();
            entity.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.MachineFingerprintHash).HasMaxLength(256).IsRequired();
            entity.Property(x => x.BindingStatus).HasMaxLength(50).IsRequired();
            entity.Property(x => x.FeaturesJson).HasColumnType("text");
            entity.HasIndex(x => x.LicenseId).IsUnique();
            entity.HasIndex(x => x.MachineFingerprintHash);
        });
    }
}
