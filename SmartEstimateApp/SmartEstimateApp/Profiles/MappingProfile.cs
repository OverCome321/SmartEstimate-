using AutoMapper;
using SmartEstimateApp.Models;
namespace SmartEstimateApp.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Маппинг User: SmartEstimateApp.Models.User <-> Entities.User
            CreateMap<User, Entities.User>()
                .ReverseMap(); // Двусторонний маппинг

            // Маппинг Role: SmartEstimateApp.Models.Role <-> Entities.Role
            CreateMap<Role, Entities.Role>()
                .ReverseMap(); // Двусторонний маппинг
        }
    }
}
