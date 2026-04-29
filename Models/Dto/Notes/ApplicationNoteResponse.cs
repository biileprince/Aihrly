using Aihrly.Api.Models.Entities;

namespace Aihrly.Api.Models.Dto.Notes;

public class ApplicationNoteResponse
{
    public int Id { get; init; }
    public NoteType Type { get; init; }
    public string Description { get; init; } = string.Empty;
    public int CreatedById { get; init; }
    public string CreatedByName { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}
