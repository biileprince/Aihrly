using System.ComponentModel.DataAnnotations;
using Aihrly.Api.Models.Entities;

namespace Aihrly.Api.Models.Dto.Applications;

public class ApplicationStageChangeRequest
{
    [Required]
    public ApplicationStage ToStage { get; set; }

    [StringLength(1000)]
    public string? Reason { get; set; }
}
