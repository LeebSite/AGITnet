using AGITnet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AGITnet.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Planning> Plannings => Set<Planning>();
    public DbSet<PlanningSlot> PlanningSlots => Set<PlanningSlot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Planning>(entity =>
        {
            entity.HasKey(e => e.PlanningId);
            
            entity.HasIndex(e => e.RequestCode)
                  .IsUnique();

            entity.Property(e => e.RequestCode)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.CandidateToken)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Status)
                  .HasMaxLength(50);

            entity.HasMany(e => e.Slots)
                  .WithOne(s => s.Planning)
                  .HasForeignKey(s => s.PlanningId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlanningSlot>(entity =>
        {
            entity.HasKey(e => e.PlanningSlotId);

            entity.Property(e => e.SlotName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.OriginalQuantity)
                  .IsRequired();

            entity.Property(e => e.BalancedQuantity)
                  .IsRequired();
        });
    }
}
