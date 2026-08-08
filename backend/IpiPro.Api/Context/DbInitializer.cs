using IpiPro.Api.Models;

namespace IpiPro.Api.Context;

public static class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        if (context.Labs.IgnoreQueryFilters().Any()) return;

        var labA = new Lab { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Central Lab — Receiving" };
        var labB = new Lab { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "West Coast Pathology" };

        context.Labs.AddRange(labA, labB);

        var manifestA1 = new Manifest
        {
            Id = Guid.Parse("a0000000-0000-0000-0000-000000000001"),
            LabId = labA.Id,
            Code = "MF-2026-0042",
            OriginClinic = "Riverside Clinic — Bay 2",
            SentAt = DateTime.UtcNow.AddHours(-3),
            Status = ManifestStatus.Open
        };

        var specimens = new List<Specimen>
        {
            new Specimen { Id = Guid.NewGuid(), LabId = labA.Id, ManifestId = manifestA1.Id, Code = "SP-2026-A0041", PatientName = "Sarah Lin", Site = "Right cheek", Provider = "Dr. Patel", Status = SpecimenStatus.Pending },
            new Specimen { Id = Guid.NewGuid(), LabId = labA.Id, ManifestId = manifestA1.Id, Code = "SP-2026-A0042", PatientName = "Sarah Lin", Site = "Left cheek", Provider = "Dr. Patel", Status = SpecimenStatus.Pending },
            new Specimen { Id = Guid.NewGuid(), LabId = labA.Id, ManifestId = manifestA1.Id, Code = "SP-2026-A0043", PatientName = "Marcus Reed", Site = "Back, upper", Provider = "Dr. Chen", Status = SpecimenStatus.Pending },
            new Specimen { Id = Guid.NewGuid(), LabId = labA.Id, ManifestId = manifestA1.Id, Code = "SP-2026-A0044", PatientName = "Marcus Reed", Site = "Right shoulder", Provider = "Dr. Chen", Status = SpecimenStatus.Pending },
            new Specimen { Id = Guid.NewGuid(), LabId = labA.Id, ManifestId = manifestA1.Id, Code = "SP-2026-A0045", PatientName = "Priya Shah", Site = "Scalp", Provider = "Dr. Reed", Status = SpecimenStatus.Pending },
            new Specimen { Id = Guid.NewGuid(), LabId = labA.Id, ManifestId = manifestA1.Id, Code = "SP-2026-A0046", PatientName = "Tom Alvarez", Site = "Left forearm", Provider = "Dr. Patel", Status = SpecimenStatus.Pending },
            new Specimen { Id = Guid.NewGuid(), LabId = labA.Id, ManifestId = manifestA1.Id, Code = "SP-2026-A0047", PatientName = "Jane Doe", Site = "Left forearm", Provider = "Dr. Patel", Status = SpecimenStatus.Pending }
        };

        context.Manifests.Add(manifestA1);
        context.Specimens.AddRange(specimens);
        context.SaveChanges();
    }
}