using Azure;
using Azure.Core;
using Business.Dtos;
using Business.Models;
using Business.Services;
using Data.Contexts;
using Domain.Extensions;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Presentation_WebApp.Models;
using Presentation_WebApp.ViewModels;
using System.Linq;

namespace Presentation_WebApp.Controllers;

[Authorize]
public class ProjectsController(AppDbContext context, IClientService clientService, IProjectService projectService, IUserService userService, IStatusService statusService) : Controller
{
    private readonly IClientService _clientService = clientService;
    private readonly IProjectService _projectService = projectService;
    private readonly IUserService _userService = userService;
    private readonly IStatusService _statusService = statusService;
    private readonly AppDbContext _context = context;

    [Route("projects")]
    [HttpGet]
    public async Task<IActionResult> Projects(int? statusId = null)
    {
        var projectResult = await _projectService.GetProjectsAsync();
        var clientResult = await _clientService.GetClientsAsync();
        var memberResult = await _userService.GetUsersAsync();
        var statusResult = await _statusService.GetStatusesAsync();

        if (statusId.HasValue)
        {
            projectResult = await _projectService.GetProjectsByStatusIdAsync(statusId.Value);
        }

        var model = new ProjectsViewModel();

        if (projectResult.Succeeded && clientResult.Succeeded && memberResult.Succeeded)
        {
            model.Projects = projectResult.Result!;
            model.Clients = clientResult.Result!;
            model.ClientOptions = clientResult.Result!
                .Select (c => new SelectListItem
                 {
                    Value = c.Id,
                    Text = c.ClientName,
                 }).ToList ();
            model.AddProjectFormData.ClientOptions = model.ClientOptions;
            model.EditProjectFormData.ClientOptions = model.ClientOptions;
            model.StatusOptions = statusResult.Result!;
            model.SelectedStatusId = statusId;
        } 
        else
        {
            ViewBag.Error = projectResult.Error ?? clientResult.Error;
        }

        return View(model);
    }




    [HttpPost]
    public async Task<IActionResult> AddProject(AddProjectViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            return BadRequest(new { success = false, errors });
        }

        string? projectImage = null;
        if (model.ProjectImage != null && model.ProjectImage.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(model.ProjectImage.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("ProjectImage", "Only JPG, JPEG, PNG, and GIF files are allowed.");
                return BadRequest(new { success = false, errors = ModelState });
            }

            if (model.ProjectImage.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("ProjectImage", "File size must not exceed 5MB.");
                return BadRequest(new { success = false, errors = ModelState });
            }

            var fileName = Guid.NewGuid().ToString() + extension;
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads"));

            using (var stream = new FileStream(uploadPath, FileMode.Create))
            {
                await model.ProjectImage.CopyToAsync(stream);
            }

            projectImage = $"/uploads/{fileName}";
        }

        var addProjectFormData = model.MapTo<AddProjectFormData>();
        addProjectFormData.Image = projectImage;
        addProjectFormData.SelectedMembersIds = model.SelectedMembersIds;

        var result = await _projectService.CreateProjectAsync(addProjectFormData);

        if (result.Succeeded)
        {
            return Ok(new { success = true });
        }

        return StatusCode(result.StatusCode, new { success = false, errors = new { General = new[] { result.Error } } });
    }

    [HttpGet]
    public async Task<JsonResult> SearchUsers(string term)
    {
        if (string.IsNullOrEmpty(term))
            return Json(new List<object>());

        var users = await _context.Users
            .Where(x => x.FirstName!.Contains(term) || x.LastName!.Contains(term) || x.Email!.Contains(term))
            .Select(x => new { x.Id, x.UserImage, FullName = x.FirstName + " " + x.LastName })
            .ToListAsync();

        return Json(users);
    }


    [HttpPost]
    public async Task<IActionResult> EditProject(EditProjectViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            return BadRequest(new { success = false, errors });
        }

        string? projectImage = null;
        if (model.ProjectImage != null && model.ProjectImage.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(model.ProjectImage.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("ProjectImage", "Only JPG, JPEG, PNG, and GIF files are allowed.");
                return BadRequest(new { success = false, errors = ModelState });
            }

            if (model.ProjectImage.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("ProjectImage", "File size must not exceed 5MB.");
                return BadRequest(new { success = false, errors = ModelState });
            }

            var fileName = Guid.NewGuid().ToString() + extension;
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads"));

            using (var stream = new FileStream(uploadPath, FileMode.Create))
            {
                await model.ProjectImage.CopyToAsync(stream);
            }

            projectImage = $"/uploads/{fileName}";
        }

        var editProjectFormData = model.MapTo<EditProjectFormData>();
        editProjectFormData.Image = projectImage;

        var result = await _projectService.UpdateProjectAsync(editProjectFormData);

        if (result.Succeeded)
        {
            return Ok(new { success = true });
        }

        return StatusCode(result.StatusCode, new { success = false, errors = new { General = new[] { result.Error } } });
    }

    [HttpGet]
    public async Task<IActionResult> EditProject(string id)
    {
        try
        {
            Console.WriteLine($"Fetching project with ID: {id}");
            if (string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Project ID is null or empty.");
                return BadRequest("Project ID is required.");
            }

            var projectResult = await _projectService.GetProjectAsync(id);
            if (!projectResult.Succeeded)
            {
                Console.WriteLine($"Project {id} not found.");
                return NotFound();
            }

            var project = projectResult.Result;
            if (project == null)
            {
                Console.WriteLine($"Project {id} is null.");
                return NotFound();
            }
            Console.WriteLine($"Project {id} fetched. Members count: {project.Members?.Count ?? 0}");

            var clientsResult = await _clientService.GetClientsAsync();
            if (!clientsResult.Succeeded || clientsResult.Result == null)
            {
                Console.WriteLine("Failed to fetch clients or clients list is null.");
                return StatusCode(500, "Unable to load client options.");
            }

            var statusResult = await _statusService.GetStatusesAsync();

            var model = new EditProjectViewModel
            {
                Id = id,
                ProjectName = project.ProjectName ?? "",
                Description = project.Description ?? "",
                ClientId = project.Client?.Id,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Budget = project.Budget,
                Image = project.Image ?? "",
                SelectedMembersIds = project.Members?.Select(m => m.Id).ToList() ?? [],
                CurrentMembers = project.Members?.Select(m => new UserViewModel
                {
                    Id = m.Id ?? "",
                    FullName = $"{m.FirstName ?? ""} {m.LastName ?? ""}".Trim(),
                    UserImage = m.UserImage ?? ""
                }).ToList() ?? [],
                ClientOptions = clientsResult.Result
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id ?? "",
                        Text = c.ClientName ?? "Unknown Client"
                    })
                    .ToList(),
                SelectedStatusId = project.Status?.Id,
                StatusOptions = statusResult.Result!.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.StatusName ?? "Unknown Status" }).ToList()
            };

            
            Console.WriteLine($"CurrentMembers count: {model.CurrentMembers.Count}");
            model.CurrentMembers.ForEach(m => Console.WriteLine($"Member: {m.Id}, {m.FullName}, {m.UserImage}"));
            Console.WriteLine($"ClientOptions count: {model.ClientOptions.Count}");

            return PartialView("~/Views/Shared/Modals/_EditProjectModalPartial.cshtml", model);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in EditProject: {ex.Message}\n{ex.StackTrace}");
            return StatusCode(500, $"An error occurred while loading the project: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteProject(string id)
    {
        var result = await _projectService.DeleteProjectAsync(id);
        if (result.Succeeded)
            return Ok(new { success = true });

        return StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }
}
