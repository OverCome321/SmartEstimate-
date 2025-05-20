using AutoMapper;
using SmartEstimateApp.Models;
namespace SmartEstimateApp.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Маппинг Project: SmartEstimateApp.Models.Project <-> Entities.Project
            CreateMap<Entities.Project, Models.Project>()
                .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.Client != null ? src.Client.Id : 0))
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client != null ? src.Client.Name : string.Empty))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User != null ? src.User.Id : 0))
                .ReverseMap(); // Двусторонний маппинг

            // Маппинг User: SmartEstimateApp.Models.User <-> Entities.User
            CreateMap<User, Entities.User>()
                .ReverseMap(); // Двусторонний маппинг

            // Маппинг Role: SmartEstimateApp.Models.Role <-> Entities.Role
            CreateMap<Role, Entities.Role>()
                .ReverseMap(); // Двусторонний маппинг
        }
    }
}
