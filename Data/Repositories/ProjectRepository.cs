using Data.Contexts;
using Data.Entities;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;


public interface IProjectRepository : IBaseRepository<ProjectEntity, Project>
{
    Task<ProjectEntity?> GetEntityWithMembersAsync(string id);
    UserEntity? GetTrackedUserEntity(string id);
}
public class ProjectRepository(AppDbContext context) : BaseRepository<ProjectEntity, Project>(context), IProjectRepository
{
    public UserEntity? GetTrackedUserEntity(string id)
    {
        return _context.Users.Local.FirstOrDefault(u => u.Id == id) ?? _context.Users.Find(id);
    }

    public async Task<ProjectEntity?> GetEntityWithMembersAsync(string id)
    {
        return await _context.Projects!
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
