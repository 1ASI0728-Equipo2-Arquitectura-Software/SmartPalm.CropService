using Microsoft.EntityFrameworkCore;
using SmartPalm.CropService.Domain.Repositories;
using SmartPalm.CropService.Domain.Plantations;
using SmartPalm.CropService.Infrastructure.Messaging;

namespace SmartPalm.CropService.Infrastructure.Persistence;

public class CropDbContext(DbContextOptions<CropDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Plantation> Plantations => Set<Plantation>();
    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<AgronomistAffiliation> Affiliations => Set<AgronomistAffiliation>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("crop");

        modelBuilder.Entity<Plantation>(entity =>
        {
            entity.ToTable("plantations");
            entity.HasKey(value => value.Id);
            entity.Property(value => value.Id).ValueGeneratedOnAdd();
            entity.Property(value => value.Name).HasMaxLength(200).IsRequired();
            entity.Property(value => value.Address).HasMaxLength(500).IsRequired();
            entity.Property(value => value.Coordinates).HasMaxLength(100);
            entity.Property(value => value.Hectares).HasPrecision(10, 2);
            entity.Property(value => value.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(value => value.InstallationMessage).HasMaxLength(500);
            entity.HasIndex(value => value.PalmGrowerId);
            entity.HasMany(value => value.Sectors)
                .WithOne()
                .HasForeignKey(value => value.PlantationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(value => value.Sectors).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Sector>(entity =>
        {
            entity.ToTable("sectors");
            entity.HasKey(value => value.Id);
            entity.Property(value => value.Id).ValueGeneratedOnAdd();
            entity.Property(value => value.Name).HasMaxLength(100).IsRequired();
            entity.Property(value => value.IotDeviceMacAddress).HasMaxLength(50).IsRequired(false);
            entity.Property(value => value.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasIndex(value => value.IotDeviceMacAddress).IsUnique();
        });

        modelBuilder.Entity<AgronomistAffiliation>(entity =>
        {
            entity.ToTable("agronomist_plantation_affiliations");
            entity.HasKey(value => value.Id);
            entity.Property(value => value.Id).ValueGeneratedOnAdd();
            entity.HasIndex(value => new { value.AgronomistId, value.PlantationId }).IsUnique();
            entity.HasIndex(value => value.PlantationId);
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages");
            entity.HasKey(value => value.Id);
            entity.Property(value => value.EventType).HasMaxLength(200).IsRequired();
            entity.Property(value => value.Content).HasColumnType("jsonb").IsRequired();
            entity.Property(value => value.Error).HasMaxLength(2000);
            entity.HasIndex(value => new { value.ProcessedOnUtc, value.OccurredOnUtc });
        });
        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.ToTable("inbox_messages");
            entity.HasKey(value => value.Id);
            entity.Property(value => value.EventType).HasMaxLength(200).IsRequired();
        });
    }

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
