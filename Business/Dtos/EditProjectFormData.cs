using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Dtos;

public class EditProjectFormData
{
    public string Id { get; set; } = null!;
    public string? Image { get; set; }
    public string ProjectName { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public int? SelectedStatusId { get; set; }
    public string Description { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> SelectedMembersIds { get; set; } = [];
    public decimal? Budget { get; set; }

}
