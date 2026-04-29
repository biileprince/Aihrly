using System.Text.Json;
using Aihrly.Api.Models.Dto.Applications;
using Microsoft.Extensions.Caching.Distributed;

namespace Aihrly.Api.Services;

public class ApplicationProfileCache : IApplicationProfileCache
{
    private const int DefaultTtlSeconds = 60;
    private readonly IDistributedCache _cache;

    public ApplicationProfileCache(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<ApplicationProfileResponse?> GetAsync(int applicationId, CancellationToken cancellationToken)
    {
        var payload = await _cache.GetStringAsync(BuildKey(applicationId), cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ApplicationProfileResponse>(payload);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public Task SetAsync(int applicationId, ApplicationProfileResponse profile, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(profile);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(DefaultTtlSeconds)
        };

        return _cache.SetStringAsync(BuildKey(applicationId), payload, options, cancellationToken);
    }

    public Task InvalidateAsync(int applicationId, CancellationToken cancellationToken)
    {
        return _cache.RemoveAsync(BuildKey(applicationId), cancellationToken);
    }

    private static string BuildKey(int applicationId)
    {
        return $"applications:{applicationId}:profile";
    }
}
