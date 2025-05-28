using Domain.Models;

namespace Business.Dtos;

public class AddProjectFormData
{
    public string? Image { get; set; }
    public string ProjectName { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> SelectedMembersIds { get; set; } = [];
    public decimal? Budget { get; set; }
}
