using System.Net;
using System.Net.Http.Json;
using Aihrly.Api.Models.Dto.Applications;
using Aihrly.Api.Models.Dto.Jobs;
using Aihrly.Api.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Aihrly.Api.Tests;

public class DuplicateApplicationTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public DuplicateApplicationTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DuplicateApplication_IsRejected()
    {
        var job = await CreateJobAsync();

        var applicationRequest = new ApplicationCreateRequest
        {
            CandidateName = "Jamie Doe",
            CandidateEmail = "jamie.doe@example.com"
        };

        var firstResponse = await _client.PostAsJsonAsync(
            $"/api/jobs/{job.Id}/applications",
            applicationRequest);

        firstResponse.EnsureSuccessStatusCode();

        var secondResponse = await _client.PostAsJsonAsync(
            $"/api/jobs/{job.Id}/applications",
            applicationRequest);

        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);

        var problem = await secondResponse.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal("Duplicate application", problem!.Title);
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
}
