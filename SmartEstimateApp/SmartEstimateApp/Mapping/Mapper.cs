using AutoMapper;
using SmartEstimateApp.Models;

namespace SmartEstimateApp.Mappings
{
    public static class Mapper
    {
        private static readonly IMapper _mapper;

        static Mapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<Profiles.MappingProfile>();
            });
            _mapper = config.CreateMapper();
        }

        /// <summary>
        /// Преобразует модель пользователя в сущность
        /// </summary>
        /// <param name="model">Модель пользователя</param>
        /// <returns>Сущность пользователя</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если модель null</exception>
        public static Entities.User ToEntity(User model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            return _mapper.Map<Entities.User>(model);
        }

        /// <summary>
        /// Преобразует сущность пользователя в модель
        /// </summary>
        /// <param name="entity">Сущность пользователя</param>
        /// <returns>Модель пользователя</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если сущность null</exception>
        public static User ToModel(Entities.User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            return _mapper.Map<User>(entity);
        }

        /// <summary>
        /// Преобразует модель роли в сущность
        /// </summary>
        /// <param name="model">Модель роли</param>
        /// <returns>Сущность роли</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если модель null</exception>
        public static Entities.Role ToEntity(Role model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            return _mapper.Map<Entities.Role>(model);
        }

        /// <summary>
        /// Преобразует сущность роли в модель
        /// </summary>
        /// <param name="entity">Сущность роли</param>
        /// <returns>Модель роли</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если сущность null</exception>
        public static Role ToModel(Entities.Role entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            return _mapper.Map<Role>(entity);
        }
    }
}