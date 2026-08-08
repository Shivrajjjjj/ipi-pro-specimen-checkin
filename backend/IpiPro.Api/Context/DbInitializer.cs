using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using IpiPro.Api.Models;

namespace IpiPro.Api.Context;

public static class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        // Skip seeding if data already exists
        if (context.Labs.IgnoreQueryFilters().Any())
        {
            return;
        }

        try
        {
            // ============================================
            // LABS (Tenants)
            // ============================================
            var labAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var labBId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var labA = new Lab
            {
                Id = labAId,
                Name = "Central Lab — Receiving"
            };

            var labB = new Lab
            {
                Id = labBId,
                Name = "West Coast Pathology"
            };

            context.Labs.AddRange(labA, labB);
            context.SaveChanges();

            // ============================================
            // LAB A: MANIFESTS & SPECIMENS
            // ============================================

            var manifestA1Id = Guid.Parse("a0a0a0a0-a0a0-a0a0-a0a0-a0a0a0a0a0a0");
            var manifestA1 = new Manifest
            {
                Id = manifestA1Id,
                LabId = labAId,
                Code = "MF-2026-0042",
                OriginClinic = "Riverside Clinic — Bay 2",
                SentAt = new DateTime(2026, 5, 26, 10, 48, 0, DateTimeKind.Utc),
                Status = ManifestStatus.Open
            };

            context.Manifests.Add(manifestA1);
            context.SaveChanges();

            var specimensA1 = new List<Specimen>
            {
                new Specimen
                {
                    Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                    LabId = labAId,
                    ManifestId = manifestA1Id,
                    Code = "SP-2026-A0041",
                    PatientName = "Sarah Lin",
                    Site = "Right cheek",
                    Provider = "Dr. Patel",
                    Status = SpecimenStatus.Received,
                    ReceivedBy = "Lab Tech 1",
                    ReceivedAt = new DateTime(2026, 5, 26, 11, 05, 0, DateTimeKind.Utc)
                },
                new Specimen
                {
                    Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"),
                    LabId = labAId,
                    ManifestId = manifestA1Id,
                    Code = "SP-2026-A0042",
                    PatientName = "Sarah Lin",
                    Site = "Left cheek",
                    Provider = "Dr. Patel",
                    Status = SpecimenStatus.Received,
                    ReceivedBy = "Lab Tech 1",
                    ReceivedAt = new DateTime(2026, 5, 26, 11, 06, 0, DateTimeKind.Utc)
                },
                new Specimen
                {
                    Id = Guid.Parse("a3333333-3333-3333-3333-333333333333"),
                    LabId = labAId,
                    ManifestId = manifestA1Id,
                    Code = "SP-2026-A0043",
                    PatientName = "Marcus Reed",
                    Site = "Back, upper",
                    Provider = "Dr. Chen",
                    Status = SpecimenStatus.Flagged
                },
                new Specimen
                {
                    Id = Guid.Parse("a4444444-4444-4444-4444-444444444444"),
                    LabId = labAId,
                    ManifestId = manifestA1Id,
                    Code = "SP-2026-A0044",
                    PatientName = "Marcus Reed",
                    Site = "Right shoulder",
                    Provider = "Dr. Chen",
                    Status = SpecimenStatus.Pending
                },
                new Specimen
                {
                    Id = Guid.Parse("a5555555-5555-5555-5555-555555555555"),
                    LabId = labAId,
                    ManifestId = manifestA1Id,
                    Code = "SP-2026-A0045",
                    PatientName = "Priya Shah",
                    Site = "Scalp",
                    Provider = "Dr. Reed",
                    Status = SpecimenStatus.Received,
                    ReceivedBy = "Lab Tech 1",
                    ReceivedAt = new DateTime(2026, 5, 26, 11, 07, 0, DateTimeKind.Utc)
                },
                new Specimen
                {
                    Id = Guid.Parse("a6666666-6666-6666-6666-666666666666"),
                    LabId = labAId,
                    ManifestId = manifestA1Id,
                    Code = "SP-2026-A0046",
                    PatientName = "Tom Alvarez",
                    Site = "Left forearm",
                    Provider = "Dr. Patel",
                    Status = SpecimenStatus.Pending
                },
                new Specimen
                {
                    Id = Guid.Parse("a7777777-7777-7777-7777-777777777777"),
                    LabId = labAId,
                    ManifestId = manifestA1Id,
                    Code = "SP-2026-A0047",
                    PatientName = "Jane Doe",
                    Site = "Left forearm",
                    Provider = "Dr. Patel",
                    Status = SpecimenStatus.Received,
                    ReceivedBy = "Lab Tech 1",
                    ReceivedAt = new DateTime(2026, 5, 26, 11, 08, 0, DateTimeKind.Utc)
                }
            };

            context.Specimens.AddRange(specimensA1);
            context.SaveChanges();

            // Manifest 2: Closed without discrepancy
            var manifestA2Id = Guid.Parse("a0a0a0a0-a0a0-a0a0-a0a0-a0a0a0a0a0a1");
            var manifestA2 = new Manifest
            {
                Id = manifestA2Id,
                LabId = labAId,
                Code = "MF-2026-0041",
                OriginClinic = "Hillside Medical Center",
                SentAt = new DateTime(2026, 5, 25, 14, 30, 0, DateTimeKind.Utc),
                Status = ManifestStatus.Closed
            };

            context.Manifests.Add(manifestA2);
            context.SaveChanges();

            var specimensA2 = new List<Specimen>
            {
                new Specimen
                {
                    Id = Guid.Parse("b1111111-1111-1111-1111-111111111111"),
                    LabId = labAId,
                    ManifestId = manifestA2Id,
                    Code = "SP-2026-A0031",
                    PatientName = "John Smith",
                    Site = "Biopsy site",
                    Provider = "Dr. Kumar",
                    Status = SpecimenStatus.Received,
                    ReceivedBy = "Lab Tech 1",
                    ReceivedAt = new DateTime(2026, 5, 25, 15, 10, 0, DateTimeKind.Utc)
                },
                new Specimen
                {
                    Id = Guid.Parse("b2222222-2222-2222-2222-222222222222"),
                    LabId = labAId,
                    ManifestId = manifestA2Id,
                    Code = "SP-2026-A0032",
                    PatientName = "Emily White",
                    Site = "Lesion",
                    Provider = "Dr. Kumar",
                    Status = SpecimenStatus.Received,
                    ReceivedBy = "Lab Tech 1",
                    ReceivedAt = new DateTime(2026, 5, 25, 15, 11, 0, DateTimeKind.Utc)
                },
                new Specimen
                {
                    Id = Guid.Parse("b3333333-3333-3333-3333-333333333333"),
                    LabId = labAId,
                    ManifestId = manifestA2Id,
                    Code = "SP-2026-A0033",
                    PatientName = "Michael Brown",
                    Site = "Mole removal",
                    Provider = "Dr. Kumar",
                    Status = SpecimenStatus.Received,
                    ReceivedBy = "Lab Tech 1",
                    ReceivedAt = new DateTime(2026, 5, 25, 15, 12, 0, DateTimeKind.Utc)
                }
            };

            context.Specimens.AddRange(specimensA2);
            context.SaveChanges();

            // Manifest 3: Closed with discrepancy
            var manifestA3Id = Guid.Parse("a0a0a0a0-a0a0-a0a0-a0a0-a0a0a0a0a0a2");
            var manifestA3 = new Manifest
            {
                Id = manifestA3Id,
                LabId = labAId,
                Code = "MF-2026-0040",
                OriginClinic = "Downtown Urgent Care",
                SentAt = new DateTime(2026, 5, 24, 09, 15, 0, DateTimeKind.Utc),
                Status = ManifestStatus.ClosedWithDiscrepancy
            };

            context.Manifests.Add(manifestA3);
            context.SaveChanges();

            var specimensA3 = new List<Specimen>
            {
                new Specimen
                {
                    Id = Guid.Parse("c1111111-1111-1111-1111-111111111111"),
                    LabId = labAId,
                    ManifestId = manifestA3Id,
                    Code = "SP-2026-A0021",
                    PatientName = "Lisa Anderson",
                    Site = "Skin tag",
                    Provider = "Dr. Lee",
                    Status = SpecimenStatus.Received,
                    ReceivedBy = "Lab Tech 1",
                    ReceivedAt = new DateTime(2026, 5, 24, 10, 05, 0, DateTimeKind.Utc)
                },
                new Specimen
                {
                    Id = Guid.Parse("c2222222-2222-2222-2222-222222222222"),
                    LabId = labAId,
                    ManifestId = manifestA3Id,
                    Code = "SP-2026-A0022",
                    PatientName = "David Martinez",
                    Site = "Wart removal",
                    Provider = "Dr. Lee",
                    Status = SpecimenStatus.Flagged
                },
                new Specimen
                {
                    Id = Guid.Parse("c3333333-3333-3333-3333-333333333333"),
                    LabId = labAId,
                    ManifestId = manifestA3Id,
                    Code = "SP-2026-A0023",
                    PatientName = "Robert Taylor",
                    Site = "Biopsy",
                    Provider = "Dr. Lee",
                    Status = SpecimenStatus.Received,
                    ReceivedBy = "Lab Tech 1",
                    ReceivedAt = new DateTime(2026, 5, 24, 10, 07, 0, DateTimeKind.Utc)
                }
            };

            context.Specimens.AddRange(specimensA3);
            context.SaveChanges();

            // ============================================
            // LAB B: MANIFESTS & SPECIMENS
            // ============================================

            var manifestB1Id = Guid.Parse("b0b0b0b0-b0b0-b0b0-b0b0-b0b0b0b0b0b0");
            var manifestB1 = new Manifest
            {
                Id = manifestB1Id,
                LabId = labBId,
                Code = "MF-2026-W001",
                OriginClinic = "Pacific Coast Clinic",
                SentAt = new DateTime(2026, 5, 26, 08, 00, 0, DateTimeKind.Utc),
                Status = ManifestStatus.Open
            };

            context.Manifests.Add(manifestB1);
            context.SaveChanges();

            var specimensB1 = new List<Specimen>
            {
                new Specimen
                {
                    Id = Guid.Parse("d1111111-1111-1111-1111-111111111111"),
                    LabId = labBId,
                    ManifestId = manifestB1Id,
                    Code = "SP-2026-W0001",
                    PatientName = "Alexander Green",
                    Site = "Arm",
                    Provider = "Dr. Sato",
                    Status = SpecimenStatus.Received,
                    ReceivedBy = "Lab Tech 2",
                    ReceivedAt = new DateTime(2026, 5, 26, 08, 45, 0, DateTimeKind.Utc)
                },
                new Specimen
                {
                    Id = Guid.Parse("d2222222-2222-2222-2222-222222222222"),
                    LabId = labBId,
                    ManifestId = manifestB1Id,
                    Code = "SP-2026-W0002",
                    PatientName = "Nicole Johnson",
                    Site = "Leg",
                    Provider = "Dr. Sato",
                    Status = SpecimenStatus.Pending
                },
                new Specimen
                {
                    Id = Guid.Parse("d3333333-3333-3333-3333-333333333333"),
                    LabId = labBId,
                    ManifestId = manifestB1Id,
                    Code = "SP-2026-W0003",
                    PatientName = "Christopher Black",
                    Site = "Hand",
                    Provider = "Dr. Tanaka",
                    Status = SpecimenStatus.Pending
                },
                new Specimen
                {
                    Id = Guid.Parse("d4444444-4444-4444-4444-444444444444"),
                    LabId = labBId,
                    ManifestId = manifestB1Id,
                    Code = "SP-2026-W0004",
                    PatientName = "Victoria Gomez",
                    Site = "Face",
                    Provider = "Dr. Tanaka",
                    Status = SpecimenStatus.Pending
                }
            };

            context.Specimens.AddRange(specimensB1);
            context.SaveChanges();

            // Create discrepancies for flagged specimens
            var discrepancies = new List<Discrepancy>
            {
                new Discrepancy
                {
                    Id = Guid.NewGuid(),
                    LabId = labAId,
                    ManifestId = manifestA1Id,
                    SpecimenId = Guid.Parse("a3333333-3333-3333-3333-333333333333"),
                    Type = DiscrepancyType.Missing,
                    Status = DiscrepancyStatus.Open,
                    FlaggedAt = new DateTime(2026, 5, 26, 11, 09, 0, DateTimeKind.Utc)
                },
                new Discrepancy
                {
                    Id = Guid.NewGuid(),
                    LabId = labAId,
                    ManifestId = manifestA3Id,
                    SpecimenId = Guid.Parse("c2222222-2222-2222-2222-222222222222"),
                    Type = DiscrepancyType.Missing,
                    Status = DiscrepancyStatus.Open,
                    FlaggedAt = new DateTime(2026, 5, 24, 10, 10, 0, DateTimeKind.Utc)
                }
            };

            context.Discrepancies.AddRange(discrepancies);
            context.SaveChanges();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Seeding error: {ex.Message}");
            throw;
        }
    }
}