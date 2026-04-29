namespace Aihrly.Api.Models.Entities;

public class ApplicationNote
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public Application? Application { get; set; }

    public NoteType Type { get; set; }
    public string Description { get; set; } = string.Empty;

    public int CreatedById { get; set; }
    public TeamMember? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
