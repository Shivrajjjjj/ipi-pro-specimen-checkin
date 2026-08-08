using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IpiPro.Api.Context;
using IpiPro.Api.Models;
using IpiPro.Api.Services;

namespace IpiPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManifestsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public ManifestsController(AppDbContext db, ITenantProvider tenantProvider)
    {
        _db = db;
        _tenantProvider = tenantProvider;
    }

    /// <summary>
    /// Get all manifests for the current tenant (lab).
    /// Tenant isolation is enforced via AppDbContext query filters.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetManifests()
    {
        try
        {
            var currentLabId = _tenantProvider.GetCurrentLabId();
            var manifests = await _db.Manifests
                .Include(m => m.Specimens)
                .Where(m => m.LabId == currentLabId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return Ok(manifests);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to fetch manifests.", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific manifest with all its specimens for the current tenant.
    /// Returns 404 if manifest does not exist or belongs to another tenant.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetManifest(Guid id)
    {
        try
        {
            var currentLabId = _tenantProvider.GetCurrentLabId();
            var manifest = await _db.Manifests
                .Include(m => m.Specimens)
                .FirstOrDefaultAsync(m => m.Id == id && m.LabId == currentLabId);

            if (manifest == null)
            {
                return NotFound(new { error = "Manifest not found or unauthorized access." });
            }

            return Ok(manifest);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to fetch manifest.", details = ex.Message });
        }
    }

    /// <summary>
    /// Mark a specimen as received by the current technician.
    /// Idempotent: marking the same specimen twice is safe.
    /// Enforces tenant isolation: specimen must belong to current tenant's manifest.
    /// </summary>
    [HttpPost("{id}/specimens/{sid}/receive")]
    public async Task<IActionResult> MarkReceived(Guid id, Guid sid)
    {
        try
        {
            var currentLabId = _tenantProvider.GetCurrentLabId();

            // Verify manifest belongs to current tenant
            var manifest = await _db.Manifests.FirstOrDefaultAsync(m => m.Id == id && m.LabId == currentLabId);
            if (manifest == null)
            {
                return NotFound(new { error = "Manifest not found or unauthorized access." });
            }

            // Fetch specimen with explicit tenant check
            var specimen = await _db.Specimens.FirstOrDefaultAsync(
                s => s.Id == sid && s.ManifestId == id && s.LabId == currentLabId);

            if (specimen == null)
            {
                return NotFound(new { error = "Specimen not found or unauthorized access." });
            }

            // Idempotent: only update if not already received
            if (specimen.Status != SpecimenStatus.Received)
            {
                specimen.Status = SpecimenStatus.Received;
                specimen.ReceivedBy = "Lab Tech 1"; // TODO: Pull from claims/session
                specimen.ReceivedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            return Ok(new { success = true, specimen = specimen });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to mark specimen received.", details = ex.Message });
        }
    }

    /// <summary>
    /// Flag a specimen as missing and create a discrepancy record.
    /// Enforces tenant isolation on both specimen and discrepancy writes.
    /// </summary>
    [HttpPost("{id}/specimens/{sid}/flag")]
    public async Task<IActionResult> FlagMissing(Guid id, Guid sid)
    {
        try
        {
            var currentLabId = _tenantProvider.GetCurrentLabId();

            // Verify manifest belongs to current tenant
            var manifest = await _db.Manifests.FirstOrDefaultAsync(m => m.Id == id && m.LabId == currentLabId);
            if (manifest == null)
            {
                return NotFound(new { error = "Manifest not found or unauthorized access." });
            }

            // Fetch specimen with explicit tenant check
            var specimen = await _db.Specimens.FirstOrDefaultAsync(
                s => s.Id == sid && s.ManifestId == id && s.LabId == currentLabId);

            if (specimen == null)
            {
                return NotFound(new { error = "Specimen not found or unauthorized access." });
            }

            // Update specimen status
            specimen.Status = SpecimenStatus.Flagged;

            // Create discrepancy record (inherits LabId via SaveChangesAsync)
            var discrepancy = new Discrepancy
            {
                ManifestId = id,
                SpecimenId = sid,
                Type = DiscrepancyType.Missing,
                Status = DiscrepancyStatus.Open,
                FlaggedAt = DateTime.UtcNow
            };

            _db.Discrepancies.Add(discrepancy);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, specimen = specimen, discrepancy = discrepancy });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to flag specimen missing.", details = ex.Message });
        }
    }

    /// <summary>
    /// Close a manifest only if all specimens are reconciled (no pending).
    /// Sets status to Closed or ClosedWithDiscrepancy based on flagged specimens.
    /// Enforces tenant isolation: cannot close another tenant's manifest.
    /// </summary>
    [HttpPost("{id}/close")]
    public async Task<IActionResult> CloseManifest(Guid id)
    {
        try
        {
            var currentLabId = _tenantProvider.GetCurrentLabId();

            // Fetch manifest with specimens, scoped to current tenant
            var manifest = await _db.Manifests
                .Include(m => m.Specimens)
                .FirstOrDefaultAsync(m => m.Id == id && m.LabId == currentLabId);

            if (manifest == null)
            {
                return NotFound(new { error = "Manifest not found or unauthorized access." });
            }

            // Check if already closed
            if (manifest.Status != ManifestStatus.Open)
            {
                return BadRequest(new { error = "Manifest is already closed." });
            }

            // Validate reconciliation: no pending specimens
            var pendingCount = manifest.Specimens.Count(s => s.Status == SpecimenStatus.Pending);
            if (pendingCount > 0)
            {
                return BadRequest(new
                {
                    error = "Cannot close manifest with pending specimens remaining.",
                    pendingCount = pendingCount
                });
            }

            // Determine final status based on flagged specimens
            var hasFlagged = manifest.Specimens.Any(s => s.Status == SpecimenStatus.Flagged);
            manifest.Status = hasFlagged ? ManifestStatus.ClosedWithDiscrepancy : ManifestStatus.Closed;

            await _db.SaveChangesAsync();

            return Ok(new { success = true, manifest = manifest });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to close manifest.", details = ex.Message });
        }
    }
}