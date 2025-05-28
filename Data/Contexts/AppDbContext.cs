using Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<UserEntity>(options)
{
    public DbSet<ClientEntity>? Clients { get; set; }
    public DbSet<StatusEntity>? Statuses { get; set; }
    public DbSet<ProjectEntity>? Projects { get; set; }
    public DbSet<MemberEntity>? Members { get; set; }
    public virtual DbSet<MemberAddressEntity>? MemberAddresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MemberEntity>()
            .HasOne(m => m.User)
            .WithOne()
            .HasForeignKey<MemberEntity>(m => m.UserId);

        modelBuilder.Entity<MemberEntity>()
            .HasOne(m => m.Address)
            .WithOne(a => a.Member)
            .HasForeignKey<MemberAddressEntity>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StatusEntity>().HasData(
            new StatusEntity { Id = 1, StatusName = "Not Started" },
            new StatusEntity { Id = 2, StatusName = "Ongoing" },
            new StatusEntity { Id = 3, StatusName = "Completed" }
        );

        modelBuilder.Entity<ProjectEntity>()
            .HasMany(p => p.Members)
            .WithMany(u => u.Projects)
            .UsingEntity(j => j.ToTable("ProjectUser"));
    }
}


