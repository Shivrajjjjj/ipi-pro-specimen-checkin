using Xunit;
using Microsoft.EntityFrameworkCore;
using IpiPro.Api.Context;
using IpiPro.Api.Models;
using IpiPro.Api.Services;

namespace IpiPro.Tests;

public class MockTenantProvider : ITenantProvider
{
    public Guid ActiveLabId { get; set; }
    public Guid GetCurrentLabId() => ActiveLabId;
}

public class TenantIsolationTests
{
    /// <summary>
    /// Verify that DbContext query filters enforce tenant isolation.
    /// Lab A data must not be visible when querying as Lab B.
    /// </summary>
    [Fact]
    public async Task DbContext_Enforces_Query_Filter_Isolation()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"test_{Guid.NewGuid()}")
            .Options;

        var tenantProvider = new MockTenantProvider();
        var labAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var labBId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // 1. Seed manifest under Lab A
        tenantProvider.ActiveLabId = labAId;
        using (var context = new AppDbContext(options, tenantProvider))
        {
            var manifest = new Manifest
            {
                Id = Guid.NewGuid(),
                LabId = labAId,
                Code = "LAB-A-SECRET-01",
                OriginClinic = "Lab A Clinic",
                SentAt = DateTime.UtcNow,
                Status = ManifestStatus.Open
            };
            context.Manifests.Add(manifest);
            await context.SaveChangesAsync();
        }

        // 2. Query as Lab B — should see zero results
        tenantProvider.ActiveLabId = labBId;
        using (var context = new AppDbContext(options, tenantProvider))
        {
            var manifests = await context.Manifests.ToListAsync();
            Assert.Empty(manifests);
        }

        // 3. Query as Lab A — should see the manifest
        tenantProvider.ActiveLabId = labAId;
        using (var context = new AppDbContext(options, tenantProvider))
        {
            var manifests = await context.Manifests.ToListAsync();
            Assert.Single(manifests);
            Assert.Equal("LAB-A-SECRET-01", manifests.First().Code);
        }
    }

    /// <summary>
    /// Verify that SaveChangesAsync enforces LabId on new entities.
    /// </summary>
    [Fact]
    public async Task DbContext_Auto_Sets_LabId_On_SaveChanges()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"test_{Guid.NewGuid()}")
            .Options;

        var tenantProvider = new MockTenantProvider();
        var labAId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        tenantProvider.ActiveLabId = labAId;
        using (var context = new AppDbContext(options, tenantProvider))
        {
            var manifest = new Manifest
            {
                Code = "AUTO-LAB-ID-TEST",
                OriginClinic = "Test Clinic",
                SentAt = DateTime.UtcNow
                // LabId is NOT set explicitly
            };

            context.Manifests.Add(manifest);
            await context.SaveChangesAsync();

            // Verify LabId was set by SaveChangesAsync
            Assert.Equal(labAId, manifest.LabId);
        }
    }

    /// <summary>
    /// Verify Specimen records inherit tenant isolation.
    /// </summary>
    [Fact]
    public async Task DbContext_Isolates_Specimens_By_Tenant()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"test_{Guid.NewGuid()}")
            .Options;

        var tenantProvider = new MockTenantProvider();
        var labAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var labBId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var manifestId = Guid.NewGuid();

        // Seed manifest + specimens as Lab A
        tenantProvider.ActiveLabId = labAId;
        using (var context = new AppDbContext(options, tenantProvider))
        {
            var manifest = new Manifest
            {
                Id = manifestId,
                Code = "TEST-MANIFEST",
                OriginClinic = "Test Clinic",
                SentAt = DateTime.UtcNow
            };
            var specimen = new Specimen
            {
                ManifestId = manifestId,
                Code = "SP-TEST-001",
                PatientName = "Test Patient",
                Site = "Test Site",
                Provider = "Test Provider"
            };

            context.Manifests.Add(manifest);
            context.Specimens.Add(specimen);
            await context.SaveChangesAsync();
        }

        // Query specimens as Lab B — should be empty
        tenantProvider.ActiveLabId = labBId;
        using (var context = new AppDbContext(options, tenantProvider))
        {
            var specimens = await context.Specimens.ToListAsync();
            Assert.Empty(specimens);
        }

        // Query specimens as Lab A — should find 1
        tenantProvider.ActiveLabId = labAId;
        using (var context = new AppDbContext(options, tenantProvider))
        {
            var specimens = await context.Specimens.ToListAsync();
            Assert.Single(specimens);
            Assert.Equal("SP-TEST-001", specimens.First().Code);
        }
    }
}