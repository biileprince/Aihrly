namespace Aihrly.Api.Models.Entities;

public class Job
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public JobStatus Status { get; set; }

    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
