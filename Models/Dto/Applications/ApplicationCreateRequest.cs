using System.ComponentModel.DataAnnotations;

namespace Aihrly.Api.Models.Dto.Applications;

public class ApplicationCreateRequest
{
    [Required]
    [StringLength(200)]
    public string CandidateName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string CandidateEmail { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? CoverLetter { get; set; }
}
