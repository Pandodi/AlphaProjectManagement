using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation_WebApp.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace Presentation_WebApp.Models;

public class EditProjectViewModel
{
    public string Id { get; set; } = null!;
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

    [Display(Name = "Status", Prompt = "Select a Status")]
    [DataType(DataType.Text)]
    public int? SelectedStatusId { get; set; }
    public List<SelectListItem> ClientOptions { get; set; } = [];
    public List<SelectListItem> StatusOptions { get; set; } = [];
    public List<UserViewModel> CurrentMembers { get; set; } = [];
}

public class UserViewModel
{
    public string Id { get; set; } = null!;
    public string? FullName { get; set; }
    public string? UserImage { get; set; }
}