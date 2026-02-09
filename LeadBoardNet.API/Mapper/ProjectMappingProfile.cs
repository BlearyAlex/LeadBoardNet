using AutoMapper;
using LeadBoard.Shared.Dtos.Settings.Images;
using LeadBoard.Shared.Dtos.Settings.Projects;
using LeadBoard.Shared.Entities;

namespace LeadBoardNet.API.Mapper;

public class ProjectMappingProfile : Profile
{
    public ProjectMappingProfile()
    {
        CreateMap<TagResponse, ProjectTag>();
        CreateMap<ProjectTag, TagResponse>();
        CreateMap<ProjectImage, ImageDetailsResponse>();

        CreateMap<ProjectRequest, Project>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.MainImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.MainImagePublicId, opt => opt.Ignore())
            .ForMember(dest => dest.Gallery, opt => opt.Ignore())
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));

        CreateMap<Project, ProjectResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
