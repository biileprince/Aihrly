using System.ComponentModel.DataAnnotations;
using Aihrly.Api.Models.Entities;

namespace Aihrly.Api.Models.Dto.Jobs;

public class JobCreateRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;

    public JobStatus? Status { get; set; }
}
