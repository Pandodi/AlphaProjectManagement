using Data.Contexts;
using Data.Entities;
using Data.Models;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Data.Repositories;

public interface IMemberRepository : IBaseRepository<MemberEntity, Member>
{
    Task<RepositoryResult<MemberEntity>> GetMemberByIdAsync(string userId);
}
public class MemberRepository(AppDbContext context) : BaseRepository<MemberEntity, Member>(context), IMemberRepository
{
    public async Task<RepositoryResult<MemberEntity>> GetMemberByIdAsync(string userId)
    {
        try
        {
            var member = await _context.Members!
                .Include(m => m.User)
                .Include(m => m.Address)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (member == null)
                return new RepositoryResult<MemberEntity> { Succeeded = false, Error = "Member not found." };

            return new RepositoryResult<MemberEntity> { Succeeded = true, Result = member };
        }
        catch (Exception ex)
        {
            return new RepositoryResult<MemberEntity> { Succeeded = false, Error = $"Exception: {ex.Message}" };
        }
    }
}

