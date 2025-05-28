using Domain.Models;
using Microsoft.AspNetCore.Http;

namespace Business.Dtos;
public class AddMemberFormData
{
    public string? UserImage { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? JobTitle { get; set; }
    public DateTime DateOfBirth { get; set; }

    public MemberAddress Address { get; set; } = new();
}
