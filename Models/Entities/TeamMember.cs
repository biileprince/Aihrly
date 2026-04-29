namespace Aihrly.Api.Models.Entities;

public class TeamMember
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public TeamMemberRole Role { get; set; }

    public ICollection<ApplicationNote> NotesCreated { get; set; } = new List<ApplicationNote>();
    public ICollection<StageHistory> StageChanges { get; set; } = new List<StageHistory>();
}
