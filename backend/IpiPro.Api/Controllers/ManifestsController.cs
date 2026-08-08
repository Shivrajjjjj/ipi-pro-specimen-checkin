using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IpiPro.Api.Context;
using IpiPro.Api.Models;

namespace IpiPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManifestsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ManifestsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetManifests()
    {
        var manifests = await _db.Manifests.Include(m => m.Specimens).ToListAsync();
        return Ok(manifests);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetManifest(Guid id)
    {
        var manifest = await _db.Manifests.Include(m => m.Specimens).FirstOrDefaultAsync(m => m.Id == id);
        if (manifest == null) return NotFound(new { error = "Manifest not found or unauthorized access." });
        return Ok(manifest);
    }

    [HttpPost("{id}/specimens/{sid}/receive")]
    public async Task<IActionResult> MarkReceived(Guid id, Guid sid)
    {
        var specimen = await _db.Specimens.FirstOrDefaultAsync(s => s.ManifestId == id && s.Id == sid);
        if (specimen == null) return NotFound(new { error = "Specimen not found." });

        // Idempotent operation
        if (specimen.Status != SpecimenStatus.Received)
        {
            specimen.Status = SpecimenStatus.Received;
            specimen.ReceivedBy = "Lab Tech 1";
            specimen.ReceivedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return Ok(specimen);
    }

    [HttpPost("{id}/specimens/{sid}/flag")]
    public async Task<IActionResult> FlagMissing(Guid id, Guid sid)
    {
        var specimen = await _db.Specimens.FirstOrDefaultAsync(s => s.ManifestId == id && s.Id == sid);
        if (specimen == null) return NotFound(new { error = "Specimen not found." });

        specimen.Status = SpecimenStatus.Flagged;

        var discrepancy = new Discrepancy
        {
            ManifestId = id,
            SpecimenId = sid,
            Type = DiscrepancyType.Missing,
            Status = DiscrepancyStatus.Open
        };

        _db.Discrepancies.Add(discrepancy);
        await _db.SaveChangesAsync();

        return Ok(new { specimen, discrepancy });
    }

    [HttpPost("{id}/close")]
    public async Task<IActionResult> CloseManifest(Guid id)
    {
        var manifest = await _db.Manifests.Include(m => m.Specimens).FirstOrDefaultAsync(m => m.Id == id);
        if (manifest == null) return NotFound();

        var pendingCount = manifest.Specimens.Count(s => s.Status == SpecimenStatus.Pending);
        if (pendingCount > 0)
        {
            return BadRequest(new { error = "Cannot close manifest with pending specimens remaining." });
        }

        var hasFlagged = manifest.Specimens.Any(s => s.Status == SpecimenStatus.Flagged);
        manifest.Status = hasFlagged ? ManifestStatus.ClosedWithDiscrepancy : ManifestStatus.Closed;

        await _db.SaveChangesAsync();
        return Ok(manifest);
    }
}