using AutoMapper;

namespace Dal.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Маппинг Dal.DbModels.Role → Entities.Role
            CreateMap<Dal.DbModels.Role, Entities.Role>();

            // Маппинг Entities.Role → Dal.DbModels.Role
            CreateMap<Entities.Role, Dal.DbModels.Role>();

            // Маппинг Dal.DbModels.User → Entities.User
            CreateMap<Dal.DbModels.User, Entities.User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.PasswordHash))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src =>
                    src.Role != null ? new Entities.Role { Id = src.Role.Id, Name = src.Role.Name } : null))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => src.LastLogin));

            // Маппинг Entities.User → Dal.DbModels.User
            CreateMap<Entities.User, Dal.DbModels.User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.PasswordHash))
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.Role != null ? src.Role.Id : Guid.Empty))
                .ForMember(dest => dest.Role, opt => opt.Ignore()) // Игнорируем маппинг объекта Role
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => src.LastLogin));
        }
    }
}