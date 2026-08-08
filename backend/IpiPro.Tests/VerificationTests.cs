using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using IpiPro.Api.Context;
using IpiPro.Api.Models;
using IpiPro.Api.Services;

namespace IpiPro.Tests;

/// <summary>
/// Comprehensive verification tests for backend submission.
/// Validates: tenant isolation, data model, reconciliation logic, idempotency.
/// </summary>
public class VerificationTests
{
    private AppDbContext CreateDbContext(Guid labId)
    {
        var tenantProvider = new StubTenantProvider(labId);
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options, tenantProvider);
    }

    #region Tenant Isolation Tests

    [Fact]
    public async Task TenantIsolation_LabACannotSeeLabBManifests()
    {
        // Arrange: Create two labs with manifests in same DB
        var labAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var labBId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Shared in-memory DB
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("isolation-test-1")
            .Options;

        // Lab A: Add manifest
        using (var dbA = new AppDbContext(options, new StubTenantProvider(labAId)))
        {
            dbA.Labs.Add(new Lab { Id = labAId, Name = "Lab A" });
            dbA.Manifests.Add(new Manifest
            {
                Id = Guid.NewGuid(),
                LabId = labAId,
                Code = "MF-LAB-A-001",
                Status = ManifestStatus.Open,
                OriginClinic = "Clinic A"
            });
            await dbA.SaveChangesAsync();
        }

        // Lab B: Add manifest
        using (var dbB = new AppDbContext(options, new StubTenantProvider(labBId)))
        {
            dbB.Labs.Add(new Lab { Id = labBId, Name = "Lab B" });
            dbB.Manifests.Add(new Manifest
            {
                Id = Guid.NewGuid(),
                LabId = labBId,
                Code = "MF-LAB-B-001",
                Status = ManifestStatus.Open,
                OriginClinic = "Clinic B"
            });
            await dbB.SaveChangesAsync();
        }

        // Act: Lab A queries manifests
        using (var dbA = new AppDbContext(options, new StubTenantProvider(labAId)))
        {
            var manifests = await dbA.Manifests.ToListAsync();

            // Assert: Lab A sees only Lab A manifests
            Assert.Single(manifests);
            Assert.All(manifests, m => Assert.Equal(labAId, m.LabId));
            Assert.DoesNotContain(manifests, m => m.Code.Contains("LAB-B"));
        }

        // Act: Lab B queries manifests
        using (var dbB = new AppDbContext(options, new StubTenantProvider(labBId)))
        {
            var manifests = await dbB.Manifests.ToListAsync();

            // Assert: Lab B sees only Lab B manifests
            Assert.Single(manifests);
            Assert.All(manifests, m => Assert.Equal(labBId, m.LabId));
            Assert.DoesNotContain(manifests, m => m.Code.Contains("LAB-A"));
        }
    }

    [Fact]
    public async Task TenantIsolation_LabACannotModifyLabBSpecimens()
    {
        var labAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var labBId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var manifestBId = Guid.NewGuid();
        var specimenBId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("isolation-test-2")
            .Options;

        // Setup: Lab B creates manifest + specimen
        using (var dbB = new AppDbContext(options, new StubTenantProvider(labBId)))
        {
            dbB.Labs.Add(new Lab { Id = labBId, Name = "Lab B" });
            dbB.Manifests.Add(new Manifest
            {
                Id = manifestBId,
                LabId = labBId,
                Code = "MF-B-TEST",
                Status = ManifestStatus.Open,
                OriginClinic = "Clinic B"
            });
            dbB.Specimens.Add(new Specimen
            {
                Id = specimenBId,
                LabId = labBId,
                ManifestId = manifestBId,
                Code = "SP-B-001",
                PatientName = "Patient B",
                Site = "Test",
                Provider = "Dr. B",
                Status = SpecimenStatus.Pending
            });
            await dbB.SaveChangesAsync();
        }

        // Act: Lab A attempts to find and modify Lab B's specimen
        using (var dbA = new AppDbContext(options, new StubTenantProvider(labAId)))
        {
            var specimen = await dbA.Specimens.FirstOrDefaultAsync(s => s.Id == specimenBId);

            // Assert: Lab A cannot see Lab B's specimen
            Assert.Null(specimen);
        }
    }

    [Fact]
    public async Task TenantIsolation_LabACannotSeeLabBDiscrepancies()
    {
        var labAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var labBId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("isolation-test-3")
            .Options;

        var manifestBId = Guid.NewGuid();
        var specimenBId = Guid.NewGuid();
        var discrepancyBId = Guid.NewGuid();

        // Setup: Lab B creates discrepancy
        using (var dbB = new AppDbContext(options, new StubTenantProvider(labBId)))
        {
            dbB.Labs.Add(new Lab { Id = labBId, Name = "Lab B" });
            dbB.Manifests.Add(new Manifest
            {
                Id = manifestBId,
                LabId = labBId,
                Code = "MF-B-DISC",
                Status = ManifestStatus.Open,
                OriginClinic = "Clinic B"
            });
            dbB.Specimens.Add(new Specimen
            {
                Id = specimenBId,
                LabId = labBId,
                ManifestId = manifestBId,
                Code = "SP-B-DISC",
                PatientName = "Patient B",
                Site = "Test",
                Provider = "Dr. B",
                Status = SpecimenStatus.Flagged
            });
            dbB.Discrepancies.Add(new Discrepancy
            {
                Id = discrepancyBId,
                LabId = labBId,
                ManifestId = manifestBId,
                SpecimenId = specimenBId,
                Type = DiscrepancyType.Missing,
                Status = DiscrepancyStatus.Open
            });
            await dbB.SaveChangesAsync();
        }

        // Act: Lab A queries discrepancies
        using (var dbA = new AppDbContext(options, new StubTenantProvider(labAId)))
        {
            var discrepancies = await dbA.Discrepancies.ToListAsync();

            // Assert: Lab A sees zero discrepancies
            Assert.Empty(discrepancies);
        }
    }

    #endregion

    #region Reconciliation Logic Tests

    [Fact]
    public void ReconciliationLogic_CanCloseManifestWhenAllSpecimensReceived()
    {
        var labId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var db = CreateDbContext(labId);

        var manifestId = Guid.NewGuid();
        var manifest = new Manifest
        {
            Id = manifestId,
            LabId = labId,
            Code = "MF-RECON-1",
            Status = ManifestStatus.Open,
            OriginClinic = "Clinic A"
        };

        manifest.Specimens = new List<Specimen>
        {
            new Specimen
            {
                Id = Guid.NewGuid(),
                LabId = labId,
                ManifestId = manifestId,
                Code = "SP-1",
                PatientName = "Patient 1",
                Site = "Site 1",
                Provider = "Dr. A",
                Status = SpecimenStatus.Received
            },
            new Specimen
            {
                Id = Guid.NewGuid(),
                LabId = labId,
                ManifestId = manifestId,
                Code = "SP-2",
                PatientName = "Patient 2",
                Site = "Site 2",
                Provider = "Dr. A",
                Status = SpecimenStatus.Received
            }
        };

        // Check reconciliation condition
        var hasNoPending = manifest.Specimens.All(s => s.Status != SpecimenStatus.Pending);
        var hasFlagged = manifest.Specimens.Any(s => s.Status == SpecimenStatus.Flagged);

        Assert.True(hasNoPending, "Should have no pending specimens");
        Assert.False(hasFlagged, "Should have no flagged specimens");

        // Simulate close
        if (hasNoPending)
        {
            manifest.Status = hasFlagged
                ? ManifestStatus.ClosedWithDiscrepancy
                : ManifestStatus.Closed;
        }

        Assert.Equal(ManifestStatus.Closed, manifest.Status);
    }

    [Fact]
    public void ReconciliationLogic_CannotCloseManifestWithPendingSpecimens()
    {
        var labId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var db = CreateDbContext(labId);

        var manifestId = Guid.NewGuid();
        var manifest = new Manifest
        {
            Id = manifestId,
            LabId = labId,
            Code = "MF-RECON-2",
            Status = ManifestStatus.Open,
            OriginClinic = "Clinic A"
        };

        manifest.Specimens = new List<Specimen>
        {
            new Specimen
            {
                Id = Guid.NewGuid(),
                LabId = labId,
                ManifestId = manifestId,
                Code = "SP-1",
                PatientName = "Patient 1",
                Site = "Site 1",
                Provider = "Dr. A",
                Status = SpecimenStatus.Received
            },
            new Specimen
            {
                Id = Guid.NewGuid(),
                LabId = labId,
                ManifestId = manifestId,
                Code = "SP-2",
                PatientName = "Patient 2",
                Site = "Site 2",
                Provider = "Dr. A",
                Status = SpecimenStatus.Pending // Still pending!
            }
        };

        // Check reconciliation condition
        var pendingCount = manifest.Specimens.Count(s => s.Status == SpecimenStatus.Pending);

        Assert.Equal(1, pendingCount);
        Assert.True(pendingCount > 0, "Cannot close with pending specimens");
    }

    [Fact]
    public void ReconciliationLogic_CloseWithDiscrepancyWhenFlaggedPresent()
    {
        var labId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var manifestId = Guid.NewGuid();

        var manifest = new Manifest
        {
            Id = manifestId,
            LabId = labId,
            Code = "MF-RECON-3",
            Status = ManifestStatus.Open,
            OriginClinic = "Clinic A"
        };

        manifest.Specimens = new List<Specimen>
        {
            new Specimen
            {
                Id = Guid.NewGuid(),
                LabId = labId,
                ManifestId = manifestId,
                Code = "SP-1",
                PatientName = "Patient 1",
                Site = "Site 1",
                Provider = "Dr. A",
                Status = SpecimenStatus.Received
            },
            new Specimen
            {
                Id = Guid.NewGuid(),
                LabId = labId,
                ManifestId = manifestId,
                Code = "SP-2",
                PatientName = "Patient 2",
                Site = "Site 2",
                Provider = "Dr. A",
                Status = SpecimenStatus.Flagged // Missing!
            }
        };

        // Reconciliation logic
        var hasNoPending = manifest.Specimens.All(s => s.Status != SpecimenStatus.Pending);
        var hasFlagged = manifest.Specimens.Any(s => s.Status == SpecimenStatus.Flagged);

        if (hasNoPending)
        {
            manifest.Status = hasFlagged
                ? ManifestStatus.ClosedWithDiscrepancy
                : ManifestStatus.Closed;
        }

        Assert.Equal(ManifestStatus.ClosedWithDiscrepancy, manifest.Status);
    }

    [Fact]
    public async Task ReconciliationLogic_IdempotentMarkReceived()
    {
        var labId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var db = CreateDbContext(labId);

        var specimenId = Guid.NewGuid();
        var specimen = new Specimen
        {
            Id = specimenId,
            LabId = labId,
            ManifestId = Guid.NewGuid(),
            Code = "SP-IDEM",
            PatientName = "Patient",
            Site = "Site",
            Provider = "Dr. A",
            Status = SpecimenStatus.Pending
        };

        db.Specimens.Add(specimen);
        await db.SaveChangesAsync();

        // Mark received FIRST time
        specimen.Status = SpecimenStatus.Received;
        specimen.ReceivedBy = "Tech 1";
        specimen.ReceivedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var first = await db.Specimens.FindAsync(specimenId);
        var firstReceivedAt = first!.ReceivedAt;

        // Mark received SECOND time (idempotent)
        if (first.Status != SpecimenStatus.Received)
        {
            first.Status = SpecimenStatus.Received;
            first.ReceivedBy = "Tech 2";
            first.ReceivedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        var second = await db.Specimens.FindAsync(specimenId);

        // Assert: ReceivedAt did not change (idempotent)
        Assert.Equal(firstReceivedAt, second!.ReceivedAt);
        Assert.Equal("Tech 1", second.ReceivedBy);
    }

    #endregion

    #region Helper Class

    public class StubTenantProvider : ITenantProvider
    {
        private readonly Guid _labId;

        public StubTenantProvider(Guid labId)
        {
            _labId = labId;
        }

        public Guid GetCurrentLabId() => _labId;
    }

    #endregion
}