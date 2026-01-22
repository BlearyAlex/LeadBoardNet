using LeadBoard.Shared.Entities;

namespace LeadBoardNet.API.Repositories;

public interface IProjectRepository
{
    Task<Project> CreateAsync(Project project);
}