using System.ComponentModel.DataAnnotations;
using Aihrly.Api.Models.Entities;

namespace Aihrly.Api.Models.Dto.Notes;

public class ApplicationNoteCreateRequest
{
    [Required]
    public NoteType Type { get; set; }

    [Required]
    [StringLength(4000)]
    public string Description { get; set; } = string.Empty;
}
