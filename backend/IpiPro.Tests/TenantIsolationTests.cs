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
    [Fact]
    public async Task DbContext_Enforces_Tenant_Data_Isolation()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantProvider = new MockTenantProvider();

        // 1. Seed data under Lab A
        tenantProvider.ActiveLabId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        using (var context = new AppDbContext(options, tenantProvider))
        {
            context.Manifests.Add(new Manifest { Id = Guid.NewGuid(), Code = "LAB-A-01" });
            await context.SaveChangesAsync();
        }

        // 2. Query data under Lab B context
        tenantProvider.ActiveLabId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        using (var context = new AppDbContext(options, tenantProvider))
        {
            var manifests = await context.Manifests.ToListAsync();

            // Assert Lab B sees zero records from Lab A
            Assert.Empty(manifests);
        }
    }
}