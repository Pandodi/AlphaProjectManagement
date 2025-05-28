
using Business.Dtos;
using Business.Models;
using Data.Contexts;
using Data.Entities;
using Data.Repositories;
using Domain.Extensions;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business.Services;

public interface IProjectService
{
    Task<ProjectResult> CreateProjectAsync(AddProjectFormData formData);
    Task<ProjectResult> DeleteProjectAsync(string projectId);
    Task<ProjectResult<Project>> GetProjectAsync(string id);
    Task<ProjectResult<IEnumerable<Project>>> GetProjectsAsync();
    Task<ProjectResult<IEnumerable<Project>>> GetProjectsByStatusIdAsync(int statusId);
    Task<ProjectResult> UpdateProjectAsync(EditProjectFormData formData);
}

public class ProjectService(IProjectRepository projectRepository, IStatusService statusService, IUserService userService) : IProjectService
{
    private readonly IProjectRepository _projectRepository = projectRepository;
    private readonly IStatusService _statusService = statusService;
    private readonly IUserService _userService = userService;



    public async Task<ProjectResult> CreateProjectAsync(AddProjectFormData formData)
    {
        if (formData == null)
            return new ProjectResult { Succeeded = false, StatusCode = 400, Error = "Not all required fields are supplied." };

        var projectEntity = formData.MapTo<ProjectEntity>();
        var statusResult = await _statusService.GetStatusByIdAsync(1);

        if (statusResult == null || statusResult.Result == null)
        {
            return new ProjectResult
            {
                Succeeded = false,
                StatusCode = 404,
                Error = "Status with ID 1 not found."
            };
        }

        var status = statusResult.Result;
        projectEntity.StatusId = status.Id;

        if (formData.SelectedMembersIds != null && formData.SelectedMembersIds.Any())
        {
            var users = new List<UserEntity>();
            foreach (var id in formData.SelectedMembersIds)
            {
                var memberResult = await _userService.GetUserByIdAsync(id);
                if (memberResult?.Succeeded == true && memberResult.Result != null)
                {
                    var existingUser = _projectRepository.GetTrackedUserEntity(id);
                    if (existingUser != null)
                        users.Add(existingUser);
                    else
                    {
                        users.Add(new UserEntity { Id = id });
                    }
                }
                else
                {
                    return new ProjectResult { Succeeded = false, StatusCode = 404, Error = "Added Member not found in DB" };
                }
            }
            projectEntity.Members = users;

        }
        else
        {
            projectEntity.Members = [];
        }

        var result = await _projectRepository.AddAsync(projectEntity);


        return result.Succeeded
            ? new ProjectResult { Succeeded = true, StatusCode = 200 }
            : new ProjectResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
    }

    public async Task<ProjectResult> UpdateProjectAsync(EditProjectFormData formData)
    {
        if (formData is null)
            return new ProjectResult { Succeeded = false, StatusCode = 400, Error = "Form data required." };

        var projectEntity = await _projectRepository.GetEntityWithMembersAsync(formData.Id);
        if (projectEntity is null)
            return new ProjectResult { Succeeded = false, StatusCode = 404, Error = "Project not found." };

        projectEntity.ProjectName = formData.ProjectName;
        projectEntity.Description = formData.Description;
        projectEntity.StartDate = formData.StartDate;
        projectEntity.EndDate = formData.EndDate;
        projectEntity.Budget = formData.Budget;
        projectEntity.Image = formData.Image;
        projectEntity.StatusId = formData.SelectedStatusId;


        var newIds = formData.SelectedMembersIds ?? [];

        
        var toRemove = projectEntity.Members
            .Where(u => !newIds.Contains(u.Id))
            .ToList();
        foreach (var u in toRemove)
            projectEntity.Members.Remove(u);

        var existingIds = projectEntity.Members.Select(u => u.Id).ToHashSet();
        foreach (var id in newIds.Where(id => !existingIds.Contains(id)))
        {
            var userRes = await _userService.GetUserByIdAsync(id);
            if (userRes?.Result is null)
                return new ProjectResult { Succeeded = false, StatusCode = 404, Error = $"User {id} not found." };

            var tracked = _projectRepository.GetTrackedUserEntity(id)
                          ?? new UserEntity { Id = id };
            projectEntity.Members.Add(tracked);
        }

        var result = await _projectRepository.UpdateAsync(projectEntity);
        return result.Succeeded
            ? new ProjectResult { Succeeded = true, StatusCode = 200 }
            : new ProjectResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
    }


    public async Task<ProjectResult<IEnumerable<Project>>> GetProjectsAsync()
    {
        var entities = await _projectRepository.GetAllAsync<ProjectEntity>(
        selector: e => e, 
        orderByDescending: true,
        sortBy: s => s.Created,
        where: null,
        include => include.Members,
        include => include.Status,
        include => include.Client
    );

        //did manual mapping as there was a problem when trying to MapTo<>()
        var projects = entities.Result!.Select(e => new Project
        {
            Id = e.Id,
            Image = e.Image,
            ProjectName = e.ProjectName,
            Description = e.Description,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            Budget = e.Budget,
            Client = e.Client.MapTo<Client>(),
            Status = e.Status.MapTo<Status>(),
            ClientName = e.Client.ClientName,
            Members = e.Members.Select(m => new User
            {
                Id = m.Id,
                FirstName = m.FirstName,
                LastName = m.LastName,
                UserImage = m.UserImage,
                Email = m.Email!,
                JobTitle = m.JobTitle
            }).ToList()
        }).ToList();

        return new ProjectResult<IEnumerable<Project>> { Succeeded = true, StatusCode = 200, Result = projects };
    }

    public async Task<ProjectResult<Project>> GetProjectAsync(string id)
    {
        var response = await _projectRepository.GetAllAsync<ProjectEntity>(
            selector: e => e,
            orderByDescending: false,
            sortBy: null, 
            where: x => x.Id == id,
            include => include.Members,
            include => include.Status,
            include => include.Client
        );

        if (!response.Succeeded)
            return new ProjectResult<Project> { Succeeded = false, StatusCode = 404, Error = "Project not found." };

        var projectEntity = response.Result!.First();
        var project = new Project
        {
            Id = projectEntity.Id,
            ProjectName = projectEntity.ProjectName,
            Description = projectEntity.Description,
            StartDate = projectEntity.StartDate,
            EndDate = projectEntity.EndDate,
            Budget = projectEntity.Budget,
            Image = projectEntity.Image,
            Members = projectEntity.Members.Select(m => new User
            {
                Id = m.Id,
                FirstName = m.FirstName,
                LastName = m.LastName,
                UserImage = m.UserImage
            }).ToList() ?? []
        };

        return new ProjectResult<Project> { Succeeded = true, StatusCode = 200, Result = project };
    }

    public async Task<ProjectResult<IEnumerable<Project>>> GetProjectsByStatusIdAsync(int statusId)
    {
        var entities = await _projectRepository.GetAllAsync<ProjectEntity>(
        selector: e => e,
        orderByDescending: true,
        sortBy: s => s.Created,
        where: p => p.StatusId == statusId,
        include => include.Members,
        include => include.Status,
        include => include.Client
    );

        //did manual mapping as there was a problem when trying to MapTo<>()
        var projects = entities.Result!.Select(e => new Project
        {
            Id = e.Id,
            Image = e.Image,
            ProjectName = e.ProjectName,
            Description = e.Description,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            Budget = e.Budget,
            Client = e.Client.MapTo<Client>(),
            Status = e.Status.MapTo<Status>(),
            ClientName = e.Client.ClientName,
            Members = e.Members.Select(m => new User
            {
                Id = m.Id,
                FirstName = m.FirstName,
                LastName = m.LastName,
                UserImage = m.UserImage,
                Email = m.Email!,
                JobTitle = m.JobTitle
            }).ToList()
        }).ToList();

        return new ProjectResult<IEnumerable<Project>> { Succeeded = true, StatusCode = 200, Result = projects };
    }

    public async Task<ProjectResult> DeleteProjectAsync(string projectId)
    {
        if (projectId == null)
            return new ProjectResult { Succeeded = false, StatusCode = 400, Error = "Can't find client" };

        var projectEntity = new ProjectEntity { Id = projectId };
        var result = await _projectRepository.DeleteAsync(projectEntity);

        return result.Succeeded
            ? new ProjectResult { Succeeded = true, StatusCode = 200 }
            : new ProjectResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
    }
}
