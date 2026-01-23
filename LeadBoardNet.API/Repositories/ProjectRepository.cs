using LeadBoard.Shared.Entities;
using LeadBoardNet.API.Data;
using Microsoft.EntityFrameworkCore;

namespace LeadBoardNet.API.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext _context;

    public ProjectRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Project> CreateAsync(Project project)
    {
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task<Project?> GetByIdAsync(long id)
    {
        return await _context.Projects
            .Include(p => p.Gallery)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Project?> GetByTitleAsync(string title)
    {
        return await _context.Projects
            .FirstOrDefaultAsync(p => EF.Functions.Like(p.Title, title));
    }
}
