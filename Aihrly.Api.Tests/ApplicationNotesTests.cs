using System.Net;
using System.Net.Http.Json;
using Aihrly.Api.Models.Dto.Applications;
using Aihrly.Api.Models.Dto.Jobs;
using Aihrly.Api.Models.Dto.Notes;
using Aihrly.Api.Models.Entities;
using Xunit;

namespace Aihrly.Api.Tests;

public class ApplicationNotesTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public ApplicationNotesTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateNote_ReturnsAuthorName()
    {
        var job = await CreateJobAsync();
        var application = await CreateApplicationAsync(job.Id);

        var noteRequest = new ApplicationNoteCreateRequest
        {
            Type = NoteType.General,
            Description = "Initial screen completed"
        };

        using var noteMessage = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/applications/{application.Id}/notes")
        {
            Content = JsonContent.Create(noteRequest)
        };

        noteMessage.Headers.Add("X-Team-Member-Id", "1");
        var noteResponse = await _client.SendAsync(noteMessage);

        Assert.Equal(HttpStatusCode.Created, noteResponse.StatusCode);

        var notesResponse = await _client.GetAsync($"/api/applications/{application.Id}/notes");
        var notes = await notesResponse.Content.ReadFromJsonAsync<List<ApplicationNoteResponse>>();

        Assert.NotNull(notes);
        Assert.NotEmpty(notes);
        Assert.Equal("Alex Johnson", notes![0].CreatedByName);
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
