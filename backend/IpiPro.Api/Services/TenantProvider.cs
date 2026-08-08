using System;
using Microsoft.AspNetCore.Http;

namespace IpiPro.Api.Services;

public interface ITenantProvider
{
    Guid GetCurrentLabId();
}

public class HeaderTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public HeaderTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCurrentLabId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null && context.Request.Headers.TryGetValue("X-Lab-Id", out var labIdStr))
        {
            if (Guid.TryParse(labIdStr, out var labId)) return labId;
        }
        // Fallback default Lab A
        return Guid.Parse("11111111-1111-1111-1111-111111111111");
    }
}