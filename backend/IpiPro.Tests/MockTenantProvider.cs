using IpiPro.Api.Services;

namespace IpiPro.Tests;

/// <summary>
/// Shared mock implementation of ITenantProvider for unit tests.
/// Allows tests to control the active lab ID for tenant isolation verification.
/// </summary>
public class MockTenantProvider : ITenantProvider
{
    public Guid ActiveLabId { get; set; }

    public Guid GetCurrentLabId() => ActiveLabId;
}