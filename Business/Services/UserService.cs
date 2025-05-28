
using Business.Dtos;
using Business.Models;
using Data.Entities;
using Data.Repositories;
using Domain.Extensions;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Business.Services;

public interface IUserService
{
    Task<UserResult> AddUserToRole(string userId, string roleName);
    Task<UserResult> ConvertMemberToUserAsync(User user);
    Task<UserResult> CreateUserAsync(SignUpFormData formData, string roleName = "User");
    Task<UserResult> DeleteUserAsync(string userId);
    Task<UserResult<User>> GetUserByEmailAsync(string id);
    Task<UserResult<User>> GetUserByIdAsync(string id);
    Task<UserResult> GetUsersAsync();
    Task<UserResult<List<User>>> SearchUsersAsync(string term);
}

public class UserService(IUserRepository userRepository, IMemberRepository memberRepository, UserManager<UserEntity> userManager, RoleManager<IdentityRole> roleManager) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMemberRepository _memberRepository = memberRepository;
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;


    //Method was genereated by ChatGPT
    public async Task<UserResult<List<User>>> SearchUsersAsync(string term)
    {
        if (string.IsNullOrEmpty(term))
            return new UserResult<List<User>> { Succeeded = true, StatusCode = 200, Result = new List<User>() };

        try
        {
            Expression<Func<UserEntity, bool>> whereClause = x =>
                (x.FirstName != null && x.FirstName.Contains(term)) ||
                (x.LastName != null && x.LastName.Contains(term)) ||
                (x.Email != null && x.Email.Contains(term));

            Expression<Func<UserEntity, User>> selector = x => new User
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                UserImage = x.UserImage,
                Email = x.Email!
            };

            var result = await _userRepository.GetAllAsync(selector, where: whereClause);

            if (!result.Succeeded)
                return new UserResult<List<User>> { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };

            return new UserResult<List<User>> { Succeeded = true, StatusCode = 200, Result = result.Result!.ToList() };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new UserResult<List<User>> { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }


    }
    public async Task<UserResult> GetUsersAsync()
    {
        var result = await _userRepository.GetAllAsync();
        return result.MapTo<UserResult>();
    }

    public async Task<UserResult<User>> GetUserByEmailAsync(string email)
    {
        var result = await _userRepository.GetAsync(x => x.Email == email);
        return result.Succeeded
            ? new UserResult<User> { Succeeded = true, StatusCode = 200, Result = result.Result }
            : new UserResult<User> { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
    }

    public async Task<UserResult<User>> GetUserByIdAsync(string Id)
    {
        var result = await _userRepository.GetAsync(x => x.Id == Id);
        if (!result.Succeeded || result.Result == null)
            return new UserResult<User> { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };

        var user = result.Result.MapTo<User>();

        return new UserResult<User> { Succeeded = true, StatusCode = 200, Result = user };
    }

    public async Task<UserResult> AddUserToRole(string userId, string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
            return new UserResult { Succeeded = false, StatusCode = 404, Error = "Role dosn't exists." };

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new UserResult { Succeeded = false, StatusCode = 404, Error = "User dosn't exists." };

        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded
            ? new UserResult { Succeeded = true, StatusCode = 200 }
            : new UserResult { Succeeded = false, StatusCode = 500, Error = "Unable to add user to role." };
    }

    public async Task<UserResult> CreateUserAsync(SignUpFormData formData, string roleName = "User")
    {
        if (formData == null)
            return new UserResult { Succeeded = false, StatusCode = 400, Error = "Form data cannot be null" };

        var existsResult = await _userRepository.ExistsAsync(x => x.Email == formData.Email);
        if(existsResult.Succeeded)
            return new UserResult { Succeeded = false, StatusCode = 409, Error = "User with this email already exists." };

        try
        {
            var userEntity = formData.MapTo<UserEntity>();
            userEntity.UserName = formData.Email;

            var result = await _userManager.CreateAsync(userEntity, formData.Password);

            if (result.Succeeded)
            {
                
                //Adding Member when user is created:
                var memberEntity = new MemberEntity {
                    UserId = userEntity.Id, 
                    User = userEntity, 
                    Address = new MemberAddressEntity { UserId = userEntity.Id } 
                };         
                
                var memberResult = await _memberRepository.AddAsync(memberEntity);
                if (!memberResult.Succeeded)
                {
                    return new UserResult
                    {
                        Succeeded = false,
                        StatusCode = 500,
                        Error = "User created but failed to create member entity."
                    };
                }

                var addToRoleResult = await AddUserToRole(userEntity.Id, roleName);
                return addToRoleResult.Succeeded
                    ? new UserResult { Succeeded = true, StatusCode = 200 }
                    : new UserResult { Succeeded = false, StatusCode = 201, Error = "User created but not added to role." };
            }

            return new UserResult
            {
                Succeeded = false,
                StatusCode = 500,
                Error = "Unable to create user"
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new UserResult { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }

    public async Task<UserResult> UpdateUserAsync(string userId)
    {
        if (userId == null)
            return new UserResult { Succeeded = false, StatusCode = 400, Error = "Can't find id" };

        var result = await _userManager.FindByIdAsync(userId);
        if (result == null)
            return new UserResult { Succeeded = false, StatusCode = 404, Error = "Can't find user" };

        var deleteResult = await _userManager.DeleteAsync(result);
        if (deleteResult.Succeeded)
            return new UserResult { Succeeded = true, StatusCode = 200 };

        return new UserResult { Succeeded = false, StatusCode = 500, Error = "Can't delete user" };
    }

    //Method generated by ChatGPT
    public async Task <UserResult> ConvertMemberToUserAsync(User user)
    {
        try
        {
            var userEntity = await _userManager.FindByIdAsync(user.Id);
            if (userEntity == null)
                return new UserResult { Succeeded = false, StatusCode = 404, Error = "User not found" };

            userEntity.PhoneNumber = user.PhoneNumber;
            userEntity.JobTitle = user.JobTitle;
            userEntity.UserImage = user.UserImage;

            var updateResult = await _userManager.UpdateAsync(userEntity);
            if (updateResult.Succeeded)
            {
                return new UserResult { Succeeded = true, StatusCode = 200 };
            }

            return new UserResult
            {
                Succeeded = false,
                StatusCode = 500,
                Error = "Failed to update user"
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new UserResult { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }

    public async Task<UserResult> DeleteUserAsync(string userId)
    {
        if (userId == null)
            return new UserResult { Succeeded = false, StatusCode = 400, Error = "Can't find id" };

        var result = await _userManager.FindByIdAsync(userId);
        if (result == null)
            return new UserResult { Succeeded = false, StatusCode = 404, Error = "Can't find user" };

        var deleteResult = await _userManager.DeleteAsync(result);
        if (deleteResult.Succeeded)
            return new UserResult { Succeeded = true, StatusCode = 200 };

        return new UserResult { Succeeded = false, StatusCode = 500, Error = "Can't delete user" };
    }
}
