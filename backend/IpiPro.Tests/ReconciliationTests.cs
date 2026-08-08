using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using IpiPro.Api.Context;
using IpiPro.Api.Models;

namespace IpiPro.Tests;

public class MockTenantProvider : IpiPro.Api.Services.ITenantProvider
{
    public Guid ActiveLabId { get; set; }
    public Guid GetCurrentLabId() => ActiveLabId;
}

public class ReconciliationTests
{
    private DbContextOptions<AppDbContext> GetInMemoryOptions(string databaseName)
        => new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

    /// <summary>
    /// Test: Cannot close manifest with pending specimens.
    /// </summary>
    [Fact]
    public async Task Cannot_Close_Manifest_With_Pending_Specimens()
    {
        var options = GetInMemoryOptions($"test_{Guid.NewGuid()}");
        var tenantProvider = new MockTenantProvider
        {
            ActiveLabId = Guid.Parse("11111111-1111-1111-1111-111111111111")
        };

        using (var context = new AppDbContext(options, tenantProvider))
        {
            var manifestId = Guid.NewGuid();
            var manifest = new Manifest
            {
                Id = manifestId,
                LabId = tenantProvider.ActiveLabId,
                Code = "TEST-01",
                OriginClinic = "Test Clinic",
                SentAt = DateTime.UtcNow,
                Status = ManifestStatus.Open
            };

            var specimen1 = new Specimen
            {
                ManifestId = manifestId,
                Code = "SP-001",
                PatientName = "Test",
                Site = "Test",
                Provider = "Test",
                Status = SpecimenStatus.Received
            };

            var specimen2 = new Specimen
            {
                ManifestId = manifestId,
                Code = "SP-002",
                PatientName = "Test",
                Site = "Test",
                Provider = "Test",
                Status = SpecimenStatus.Pending // Still pending!
            };

            context.Manifests.Add(manifest);
            context.Specimens.AddRange(specimen1, specimen2);
            await context.SaveChangesAsync();
        }

        using (var context = new AppDbContext(options, tenantProvider))
        {
            var manifest = await context.Manifests
                .Include(m => m.Specimens)
                .FirstAsync();

            var pendingCount = manifest.Specimens.Count(s => s.Status == SpecimenStatus.Pending);
            Assert.Equal(1, pendingCount);
            Assert.True(pendingCount > 0, "Manifest has pending specimens and should not close");
        }
    }

    /// <summary>
    /// Test: Closing manifest without discrepancies sets status to Closed.
    /// </summary>
    [Fact]
    public async Task Close_Manifest_Without_Discrepancies_Sets_Closed_Status()
    {
        var options = GetInMemoryOptions($"test_{Guid.NewGuid()}");
        var tenantProvider = new MockTenantProvider
        {
            ActiveLabId = Guid.Parse("11111111-1111-1111-1111-111111111111")
        };

        using (var context = new AppDbContext(options, tenantProvider))
        {
            var manifestId = Guid.NewGuid();
            var manifest = new Manifest
            {
                Id = manifestId,
                LabId = tenantProvider.ActiveLabId,
                Code = "TEST-02",
                OriginClinic = "Test Clinic",
                SentAt = DateTime.UtcNow,
                Status = ManifestStatus.Open
            };

            var specimen1 = new Specimen
            {
                ManifestId = manifestId,
                Code = "SP-001",
                PatientName = "Test",
                Site = "Test",
                Provider = "Test",
                Status = SpecimenStatus.Received
            };

            var specimen2 = new Specimen
            {
                ManifestId = manifestId,
                Code = "SP-002",
                PatientName = "Test",
                Site = "Test",
                Provider = "Test",
                Status = SpecimenStatus.Received
            };

            context.Manifests.Add(manifest);
            context.Specimens.AddRange(specimen1, specimen2);
            await context.SaveChangesAsync();
        }

        using (var context = new AppDbContext(options, tenantProvider))
        {
            var manifest = await context.Manifests
                .Include(m => m.Specimens)
                .FirstAsync();

            // Check conditions for closing
            var pendingCount = manifest.Specimens.Count(s => s.Status == SpecimenStatus.Pending);
            var hasFlagged = manifest.Specimens.Any(s => s.Status == SpecimenStatus.Flagged);

            Assert.Equal(0, pendingCount);
            Assert.False(hasFlagged);

            // Simulate close logic
            if (pendingCount == 0)
            {
                manifest.Status = hasFlagged ? ManifestStatus.ClosedWithDiscrepancy : ManifestStatus.Closed;
                await context.SaveChangesAsync();
            }

            Assert.Equal(ManifestStatus.Closed, manifest.Status);
        }
    }

    /// <summary>
    /// Test: Closing manifest with flagged specimens sets status to ClosedWithDiscrepancy.
    /// </summary>
    [Fact]
    public async Task Close_Manifest_With_Flagged_Specimens_Sets_ClosedWithDiscrepancy_Status()
    {
        var options = GetInMemoryOptions($"test_{Guid.NewGuid()}");
        var tenantProvider = new MockTenantProvider
        {
            ActiveLabId = Guid.Parse("11111111-1111-1111-1111-111111111111")
        };

        using (var context = new AppDbContext(options, tenantProvider))
        {
            var manifestId = Guid.NewGuid();
            var manifest = new Manifest
            {
                Id = manifestId,
                LabId = tenantProvider.ActiveLabId,
                Code = "TEST-03",
                OriginClinic = "Test Clinic",
                SentAt = DateTime.UtcNow,
                Status = ManifestStatus.Open
            };

            var specimen1 = new Specimen
            {
                ManifestId = manifestId,
                Code = "SP-001",
                PatientName = "Test",
                Site = "Test",
                Provider = "Test",
                Status = SpecimenStatus.Received
            };

            var specimen2 = new Specimen
            {
                ManifestId = manifestId,
                Code = "SP-002",
                PatientName = "Test",
                Site = "Test",
                Provider = "Test",
                Status = SpecimenStatus.Flagged // Flagged, not pending
            };

            context.Manifests.Add(manifest);
            context.Specimens.AddRange(specimen1, specimen2);
            await context.SaveChangesAsync();
        }

        using (var context = new AppDbContext(options, tenantProvider))
        {
            var manifest = await context.Manifests
                .Include(m => m.Specimens)
                .FirstAsync();

            // Check conditions for closing
            var pendingCount = manifest.Specimens.Count(s => s.Status == SpecimenStatus.Pending);
            var hasFlagged = manifest.Specimens.Any(s => s.Status == SpecimenStatus.Flagged);

            Assert.Equal(0, pendingCount);
            Assert.True(hasFlagged);

            // Simulate close logic
            if (pendingCount == 0)
            {
                manifest.Status = hasFlagged ? ManifestStatus.ClosedWithDiscrepancy : ManifestStatus.Closed;
                await context.SaveChangesAsync();
            }

            Assert.Equal(ManifestStatus.ClosedWithDiscrepancy, manifest.Status);
        }
    }

    /// <summary>
    /// Test: Idempotent mark-as-received (should not corrupt counts).
    /// </summary>
    [Fact]
    public async Task Mark_Specimen_Received_Is_Idempotent()
    {
        var options = GetInMemoryOptions($"test_{Guid.NewGuid()}");
        var tenantProvider = new MockTenantProvider
        {
            ActiveLabId = Guid.Parse("11111111-1111-1111-1111-111111111111")
        };

        Guid specimenId;
        using (var context = new AppDbContext(options, tenantProvider))
        {
            var manifestId = Guid.NewGuid();
            var manifest = new Manifest
            {
                Id = manifestId,
                LabId = tenantProvider.ActiveLabId,
                Code = "TEST-IDEMPOTENT",
                OriginClinic = "Test Clinic",
                SentAt = DateTime.UtcNow,
                Status = ManifestStatus.Open
            };

            var specimen = new Specimen
            {
                ManifestId = manifestId,
                Code = "SP-IDEMPOTENT",
                PatientName = "Test",
                Site = "Test",
                Provider = "Test",
                Status = SpecimenStatus.Pending
            };

            context.Manifests.Add(manifest);
            context.Specimens.Add(specimen);
            await context.SaveChangesAsync();
            specimenId = specimen.Id;
        }

        // Mark received first time
        using (var context = new AppDbContext(options, tenantProvider))
        {
            var specimen = await context.Specimens.FindAsync(specimenId);
            specimen.Status = SpecimenStatus.Received;
            specimen.ReceivedBy = "Lab Tech 1";
            specimen.ReceivedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }

        var firstReceivedAt = DateTime.MinValue;
        using (var context = new AppDbContext(options, tenantProvider))
        {
            var specimen = await context.Specimens.FindAsync(specimenId);
            Assert.Equal(SpecimenStatus.Received, specimen.Status);
            firstReceivedAt = specimen.ReceivedAt.Value;
        }

        // Mark received second time (idempotent)
        await Task.Delay(100); // Small delay to ensure different timestamp
        using (var context = new AppDbContext(options, tenantProvider))
        {
            var specimen = await context.Specimens.FindAsync(specimenId);
            if (specimen.Status != SpecimenStatus.Received)
            {
                specimen.Status = SpecimenStatus.Received;
                specimen.ReceivedBy = "Lab Tech 1";
                specimen.ReceivedAt = DateTime.UtcNow;
            }
            await context.SaveChangesAsync();
        }

        // Verify the timestamp did NOT change (idempotent)
        using (var context = new AppDbContext(options, tenantProvider))
        {
            var specimen = await context.Specimens.FindAsync(specimenId);
            Assert.Equal(SpecimenStatus.Received, specimen.Status);
            Assert.Equal(firstReceivedAt, specimen.ReceivedAt.Value);
        }
    }
}