using AutoMapper;
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
                .ReverseMap();

            // Маппинг User: SmartEstimateApp.Models.User <-> Entities.User
            CreateMap<Entities.User, Models.User>()
                            .ReverseMap();

            // Маппинг Role: SmartEstimateApp.Models.Role <-> Entities.Role
            CreateMap<Entities.Role, Models.Role>()
                .ReverseMap();

            // Маппинг Message: Entities.Message <-> Models.Message
            CreateMap<Entities.Message, Models.Message>()
                .ReverseMap();

            // **Маппинг Chat: Entities.Chat -> Models.Chat**
            CreateMap<Entities.Chat, Models.Chat>()
                .ForMember(dest => dest.Messages, opt => opt.MapFrom(src => src.Messages))
                .ReverseMap();
        }
    }
}
