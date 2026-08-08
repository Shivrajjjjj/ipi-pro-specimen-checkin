namespace IpiPro.Api.Models;

public class Lab
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}

public enum ManifestStatus { Open, Closed, ClosedWithDiscrepancy }

public class Manifest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LabId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string OriginClinic { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public ManifestStatus Status { get; set; } = ManifestStatus.Open;
    public List<Specimen> Specimens { get; set; } = new();
}

public enum SpecimenStatus { Pending, Received, Flagged }

public class Specimen
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LabId { get; set; }
    public Guid ManifestId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string Site { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public SpecimenStatus Status { get; set; } = SpecimenStatus.Pending;
    public string? ReceivedBy { get; set; }
    public DateTime? ReceivedAt { get; set; }
}

public enum DiscrepancyType { Missing, OffManifest }
public enum DiscrepancyStatus { Open, Resolved }

public class Discrepancy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LabId { get; set; }
    public Guid ManifestId { get; set; }
    public Guid? SpecimenId { get; set; }
    public DiscrepancyType Type { get; set; }
    public DiscrepancyStatus Status { get; set; } = DiscrepancyStatus.Open;
    public DateTime FlaggedAt { get; set; } = DateTime.UtcNow;
}