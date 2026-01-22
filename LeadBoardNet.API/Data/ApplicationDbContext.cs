using LeadBoard.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeadBoardNet.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectImage> ProjectImages { get; set; }
    public DbSet<ProjectTag> ProjectTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}