using Microsoft.AspNetCore.Identity;
using System;

namespace Data.Entities;

public class UserEntity : IdentityUser
{
    public string? UserImage { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? JobTitle { get; set; }
    public ICollection<ProjectEntity> Projects { get; set; } = [];
}

