using Microsoft.EntityFrameworkCore;
using IpiPro.Api.Models;
using IpiPro.Api.Services;

namespace IpiPro.Api.Context;

public class AppDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Lab> Labs => Set<Lab>();
    public DbSet<Manifest> Manifests => Set<Manifest>();
    public DbSet<Specimen> Specimens => Set<Specimen>();
    public DbSet<Discrepancy> Discrepancies => Set<Discrepancy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Server-Enforced Multi-Tenant Isolation
        Guid currentLabId = _tenantProvider.GetCurrentLabId();

        modelBuilder.Entity<Manifest>().HasQueryFilter(m => m.LabId == currentLabId);
        modelBuilder.Entity<Specimen>().HasQueryFilter(s => s.LabId == currentLabId);
        modelBuilder.Entity<Discrepancy>().HasQueryFilter(d => d.LabId == currentLabId);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Enforce active LabId on newly added entities
        var currentLabId = _tenantProvider.GetCurrentLabId();
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                var labIdProp = entry.Entity.GetType().GetProperty("LabId");
                if (labIdProp != null && (Guid)labIdProp.GetValue(entry.Entity)! == Guid.Empty)
                {
                    labIdProp.SetValue(entry.Entity, currentLabId);
                }
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}