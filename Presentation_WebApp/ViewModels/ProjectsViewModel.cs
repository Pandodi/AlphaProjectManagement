using Domain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation_WebApp.ViewModels;

namespace Presentation_WebApp.Models;

public class ProjectsViewModel
{
    public IEnumerable<Project> Projects { get; set; } = [];
    public IEnumerable<Client> Clients { get; set; } = [];

    public AddProjectViewModel AddProjectFormData { get; set; } = new();
    public EditProjectViewModel EditProjectFormData { get; set; } = new();

    public List<SelectListItem> ClientOptions { get; set; } = [];
    public IEnumerable<Status> StatusOptions { get; set; } = [];

    public int? SelectedStatusId { get; set; }
}
