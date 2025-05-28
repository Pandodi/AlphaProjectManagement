using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class MemberAddressEntity
{
    [Key, ForeignKey("Member")]
    public string UserId { get; set; } = null!;
    public string? StreetName { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public MemberEntity Member { get; set; } = null!;
}
