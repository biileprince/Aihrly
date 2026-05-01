using System.Net;
using System.Net.Http.Json;
using Aihrly.Api.Models.Dto.Applications;
using Aihrly.Api.Models.Dto.Jobs;
using Aihrly.Api.Models.Dto.Scores;
using Aihrly.Api.Models.Entities;
using Xunit;

namespace Aihrly.Api.Tests;

public class ApplicationScoresTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public ApplicationScoresTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UpdateScore_OverwritesPreviousValue()
    {
        var job = await CreateJobAsync();
        var application = await CreateApplicationAsync(job.Id);

        await UpdateScoreAsync(application.Id, 1, new ScoreUpdateRequest
        {
            Score = 2,
            Comment = "Initial"
        });

        await UpdateScoreAsync(application.Id, 2, new ScoreUpdateRequest
        {
            Score = 4,
            Comment = "Updated"
        });

        var profileResponse = await _client.GetAsync($"/api/applications/{application.Id}");
        profileResponse.EnsureSuccessStatusCode();

        var profile = await profileResponse.Content.ReadFromJsonAsync<ApplicationProfileResponse>();

        Assert.NotNull(profile);
        Assert.Equal(4, profile!.CultureFit.Score);
        Assert.Equal("Updated", profile.CultureFit.Comment);
        Assert.Equal(2, profile.CultureFit.UpdatedById);
        Assert.NotNull(profile.CultureFit.UpdatedAt);
    }

    private async Task UpdateScoreAsync(int applicationId, int teamMemberId, ScoreUpdateRequest request)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Put,
            $"/api/applications/{applicationId}/scores/culture-fit")
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Add("X-Team-Member-Id", teamMemberId.ToString());

        var response = await _client.SendAsync(message);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<JobDetailResponse> CreateJobAsync()
    {
        var jobRequest = new JobCreateRequest
        {
            Title = "Backend Developer",
            Description = "Build APIs",
            Location = "Remote",
            Status = JobStatus.Open
        };

        using var jobMessage = new HttpRequestMessage(HttpMethod.Post, "/api/jobs")
        {
            Content = JsonContent.Create(jobRequest)
        };

        jobMessage.Headers.Add("X-Team-Member-Id", "1");

        var jobResponse = await _client.SendAsync(jobMessage);
        jobResponse.EnsureSuccessStatusCode();

        var job = await jobResponse.Content.ReadFromJsonAsync<JobDetailResponse>();
        return job!;
    }

    private async Task<ApplicationProfileResponse> CreateApplicationAsync(int jobId)
    {
        var applicationRequest = new ApplicationCreateRequest
        {
            CandidateName = "Jamie Doe",
            CandidateEmail = "jamie.doe@example.com"
        };

        var response = await _client.PostAsJsonAsync(
            $"/api/jobs/{jobId}/applications",
            applicationRequest);

        response.EnsureSuccessStatusCode();

        var application = await response.Content.ReadFromJsonAsync<ApplicationProfileResponse>();
        return application!;
    }
}
