using Aihrly.Api.Models.Entities;

namespace Aihrly.Api.Services;

public static class StageTransitionRules
{
    private static readonly IReadOnlyDictionary<ApplicationStage, ApplicationStage[]> AllowedTransitions =
        new Dictionary<ApplicationStage, ApplicationStage[]>
        {
            [ApplicationStage.Applied] = new[] { ApplicationStage.Screening, ApplicationStage.Rejected },
            [ApplicationStage.Screening] = new[] { ApplicationStage.Interview, ApplicationStage.Rejected },
            [ApplicationStage.Interview] = new[] { ApplicationStage.Offer, ApplicationStage.Rejected },
            [ApplicationStage.Offer] = new[] { ApplicationStage.Hired, ApplicationStage.Rejected },
            [ApplicationStage.Hired] = Array.Empty<ApplicationStage>(),
            [ApplicationStage.Rejected] = Array.Empty<ApplicationStage>()
        };

    public static bool IsValidTransition(ApplicationStage fromStage, ApplicationStage toStage)
    {
        return AllowedTransitions.TryGetValue(fromStage, out var allowed)
            && Array.IndexOf(allowed, toStage) >= 0;
    }

    public static string BuildErrorMessage(ApplicationStage fromStage, ApplicationStage toStage)
    {
        return $"Invalid stage transition from '{fromStage}' to '{toStage}'.";
    }
}
