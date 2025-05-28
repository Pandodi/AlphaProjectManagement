using Domain.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class ProjectEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Image {  get; set; }
    public string ProjectName { get; set; } = null!;
    public string Description { get; set; } = null!;

    [Column(TypeName = "date")]
    public DateTime StartDate { get; set; }
    [Column(TypeName = "date")]
    public DateTime EndDate { get; set; }
    public DateTime Created {  get; set; } = DateTime.Now;
    public decimal? Budget {  get; set; }

    [ForeignKey(nameof(Client))]
    public string ClientId { get; set; } = null!;
    public ClientEntity Client { get; set; } = null!;
    
    [ForeignKey(nameof(Status))]
    public int? StatusId { get; set; }
    public StatusEntity Status { get; set; } = null!;

    public virtual ICollection<UserEntity> Members { get; set; } = [];
}

