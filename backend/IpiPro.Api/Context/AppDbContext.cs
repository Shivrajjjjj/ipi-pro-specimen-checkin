using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        // Configure relationships
        modelBuilder.Entity<Manifest>()
            .HasMany(m => m.Specimens)
            .WithOne()
            .HasForeignKey(s => s.ManifestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filters for tenant isolation (soft enforcement)
        try
        {
            var currentLabId = _tenantProvider.GetCurrentLabId();
            if (currentLabId != Guid.Empty)
            {
                modelBuilder.Entity<Manifest>()
                    .HasQueryFilter(m => m.LabId == currentLabId);
                modelBuilder.Entity<Specimen>()
                    .HasQueryFilter(s => s.LabId == currentLabId);
                modelBuilder.Entity<Discrepancy>()
                    .HasQueryFilter(d => d.LabId == currentLabId);
            }
        }
        catch
        {
            // No tenant context during migrations or model creation
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-inject LabId for new entities
        try
        {
            var currentLabId = _tenantProvider.GetCurrentLabId();
            if (currentLabId != Guid.Empty)
            {
                foreach (var entry in ChangeTracker.Entries())
                {
                    if (entry.State == EntityState.Added)
                    {
                        var labIdProp = entry.Entity.GetType().GetProperty("LabId");
                        if (labIdProp != null)
                        {
                            var currentValue = (Guid)labIdProp.GetValue(entry.Entity)!;
                            if (currentValue == Guid.Empty)
                            {
                                labIdProp.SetValue(entry.Entity, currentLabId);
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // If we can't get tenant context, skip injection
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}