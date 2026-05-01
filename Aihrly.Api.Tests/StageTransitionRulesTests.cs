using Aihrly.Api.Models.Entities;
using Aihrly.Api.Services;
using Xunit;

namespace Aihrly.Api.Tests;

public class StageTransitionRulesTests
{
    [Theory]
    [InlineData(ApplicationStage.Applied, ApplicationStage.Screening, true)]
    [InlineData(ApplicationStage.Applied, ApplicationStage.Hired, false)]
    [InlineData(ApplicationStage.Screening, ApplicationStage.Interview, true)]
    [InlineData(ApplicationStage.Interview, ApplicationStage.Rejected, true)]
    [InlineData(ApplicationStage.Offer, ApplicationStage.Rejected, true)]
    [InlineData(ApplicationStage.Hired, ApplicationStage.Rejected, false)]
    public void IsValidTransition_ValidatesExpectedMoves(
        ApplicationStage fromStage,
        ApplicationStage toStage,
        bool expected)
    {
        var actual = StageTransitionRules.IsValidTransition(fromStage, toStage);

        Assert.Equal(expected, actual);
    }
}
