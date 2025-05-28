using Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities;

public class MemberEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;

    public UserEntity User { get; set; } = null!;

    public string? PhoneNumber {  get; set; }

    public string? JobTitle { get; set; }

    [Column(TypeName = "date")]
    public DateTime DateOfBirth { get; set; }

    public MemberAddressEntity? Address { get; set; }
}
