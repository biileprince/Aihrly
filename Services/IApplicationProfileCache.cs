using Aihrly.Api.Models.Dto.Applications;

namespace Aihrly.Api.Services;

public interface IApplicationProfileCache
{
    Task<ApplicationProfileResponse?> GetAsync(int applicationId, CancellationToken cancellationToken);
    Task SetAsync(int applicationId, ApplicationProfileResponse profile, CancellationToken cancellationToken);
    Task InvalidateAsync(int applicationId, CancellationToken cancellationToken);
}
