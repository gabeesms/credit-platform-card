using CardService.Domain.Entities;
using CardService.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;

namespace CardService.Infrastructure.Persistence;

public sealed class CardDbContext(DbContextOptions<CardDbContext> options) : DbContext(options)
{
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<ProcessedCreditEvent> ProcessedCreditEvents => Set<ProcessedCreditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(card => card.Id);
            entity.HasIndex(card => card.CreditApplicationId).IsUnique();
            entity.HasIndex(card => card.CustomerId);
            entity.Property(card => card.CreditLimit).HasPrecision(18, 2).IsRequired();
            entity.Property(card => card.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(card => card.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(card => card.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<ProcessedCreditEvent>(entity =>
        {
            entity.HasKey(item => item.EventId);
            entity.Property(item => item.ProcessedAt).IsRequired();
        });
    }
}
