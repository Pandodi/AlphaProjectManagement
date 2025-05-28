using Business.Dtos;
using Business.Models;
using Data.Entities;
using Data.Models;
using Data.Repositories;
using Domain.Extensions;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business.Services;
public interface IMemberService
{
    Task<MemberResult> CreateMemberAsync(AddMemberFormData formData);
    Task<MemberResult> GetMembersAsync();
    Task<MemberResult> UpdateMemberFromUserAsync(EditMemberFormData formData);
}
public class MemberService(IMemberRepository memberRepository, IUserService userService, UserManager<UserEntity> userManager) : IMemberService
{
    private readonly IMemberRepository _memberRepository = memberRepository;
    private readonly IUserService _userService = userService;
    private readonly UserManager<UserEntity> _userManager = userManager;

    public async Task<MemberResult> GetMembersAsync()
    {
        var result = await _memberRepository.GetAllAsync();
        return result.MapTo<MemberResult>();
    }

    public async Task<MemberResult> CreateMemberAsync(AddMemberFormData formData)
    {
        if (formData == null)
            return new MemberResult { Succeeded = false, StatusCode = 400, Error = "Not all required fields are supplied." };

        //Create a user when created a member as admin
        var signUpForm = formData.MapTo<SignUpFormData>();
        signUpForm.Password = "BytMig123!";

        var result = await _userService.CreateUserAsync(signUpForm, "User");
        if (!result.Succeeded)
            return new MemberResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };

        var userResult = await _userService.GetUserByEmailAsync(signUpForm.Email);
        if (userResult == null)
            return new MemberResult { Succeeded = false, StatusCode = 500, Error = "User creation succeeded but user not found in DB" };

        var userEntity = userResult.Result;
        userEntity!.PhoneNumber = formData.PhoneNumber;
        userEntity!.JobTitle = formData.JobTitle;  

        if (!string.IsNullOrEmpty(formData.UserImage))
            userEntity.UserImage = formData.UserImage;

        await _userService.ConvertMemberToUserAsync(userEntity);

        var memberEntity = formData.MapTo<MemberEntity>();
        memberEntity.UserId = userEntity.Id;

        if (formData.Address != null)
        {
            memberEntity.Address = new MemberAddressEntity
            {
                UserId = userEntity.Id,
                StreetName = formData.Address.StreetName,
                PostalCode = formData.Address.PostalCode,
                City = formData.Address.City
            };
        }

        var memberResult = await _memberRepository.AddAsync(memberEntity);

        if (!memberResult.Succeeded)
        {
            return new MemberResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
        }

        var member = memberEntity.MapTo<Member>();
        return new MemberResult { Succeeded = true, StatusCode = 200, Result = [member] };
    }

    public async Task<MemberResult> UpdateMemberFromUserAsync(EditMemberFormData formData)
    {
        if (formData == null)
            return new MemberResult { Succeeded = false, StatusCode = 400, Error = "Not all required field are supplied." };

        var userEntity = await _userManager.FindByIdAsync(formData.Id);
        if (userEntity == null)
            return new MemberResult { Succeeded = false, StatusCode = 404, Error = "User not found in user manager." };

        userEntity.FirstName = formData.FirstName;
        userEntity.LastName = formData.LastName;
        userEntity.Email = formData.Email;
        userEntity.PhoneNumber = formData.PhoneNumber;
        userEntity.JobTitle = formData.JobTitle;
        userEntity.UserImage = formData.UserImage;

        var userUpdate = await _userManager.UpdateAsync(userEntity);
        if (!userUpdate.Succeeded)
            return new MemberResult { Succeeded = false, StatusCode = 500, Error = "Failed to update user info." };

        //For member Entity
        var memberResult = await _memberRepository.GetMemberByIdAsync(userEntity.Id);
        if (!memberResult.Succeeded || memberResult.Result == null)
            return new MemberResult { Succeeded = false, StatusCode = 404, Error = "Member not found." };

        var memberEntity = memberResult.Result;
        memberEntity.DateOfBirth = formData.DateOfBirth;
        memberEntity.UserId = userEntity.Id;

        if (formData.Address != null)
        {
            memberEntity.Address = new MemberAddressEntity
            {
                UserId = userEntity.Id,
                StreetName = formData.Address.StreetName,
                PostalCode = formData.Address.PostalCode,
                City = formData.Address.City
            };
        }

        var updateResult = await _memberRepository.UpdateAsync(memberEntity);
        return updateResult.Succeeded
            ? new MemberResult { Succeeded = true, StatusCode = 200 }
            : new MemberResult { Succeeded = false, StatusCode = 500, Error = "Failed to update member info." };

    }

}



