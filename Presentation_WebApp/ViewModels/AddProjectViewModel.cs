using Business.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Presentation_WebApp.ViewModels;

public class AddProjectViewModel()
{

    [Display(Name = "Project Image", Prompt = "Select a image")]
    [DataType(DataType.Upload)]
    public IFormFile? ProjectImage { get; set; }
    public string? Image { get; set; }

    [Display(Name = "Project Name", Prompt = "Project Name")]
    [DataType(DataType.Text)]
    [Required(ErrorMessage = "Required")]
    public string ProjectName { get; set; } = null!;

    [Display(Name = "Client Name", Prompt = "Select a Client")]
    [DataType(DataType.Text)]
    [Required(ErrorMessage = "Required")]
    public string ClientId { get; set; } = null!;

    [Display(Name = "Description", Prompt = "Type something")]
    [DataType(DataType.Text)]
    [Required(ErrorMessage = "Required")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Required")]
    [Display(Name = "Start Date", Prompt = "Start Date")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Required")]
    [Display(Name = "End Date", Prompt = "End Date")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Required")]
    [Display(Name = "Members", Prompt = "Select member(s)")]
    [DataType(DataType.Text)]
    public List<string> SelectedMembersIds { get; set; } = [];

    [Display(Name = "Budget", Prompt = "0")]
    [DataType(DataType.Currency)]
    public decimal? Budget { get; set; }
    public List<SelectListItem> ClientOptions { get; set; } = [];
}
