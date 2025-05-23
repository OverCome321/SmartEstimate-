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
        /// Преобразует модель пользователя в сущность.
        /// </summary>
        /// <param name="model">Модель пользователя.</param>
        /// <returns>Сущность пользователя.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если модель null.</exception>
        public static Entities.User ToEntity(User model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            return _mapper.Map<Entities.User>(model);
        }

        /// <summary>
        /// Преобразует сущность пользователя в модель.
        /// </summary>
        /// <param name="entity">Сущность пользователя.</param>
        /// <returns>Модель пользователя.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если сущность null.</exception>
        public static User ToModel(Entities.User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            return _mapper.Map<User>(entity);
        }

        /// <summary>
        /// Преобразует модель роли в сущность.
        /// </summary>
        /// <param name="model">Модель роли.</param>
        /// <returns>Сущность роли.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если модель null.</exception>
        public static Entities.Role ToEntity(Role model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            return _mapper.Map<Entities.Role>(model);
        }

        /// <summary>
        /// Преобразует сущность роли в модель.
        /// </summary>
        /// <param name="entity">Сущность роли.</param>
        /// <returns>Модель роли.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если сущность null.</exception>
        public static Role ToModel(Entities.Role entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            return _mapper.Map<Role>(entity);
        }

        /// <summary>
        /// Преобразует модель проекта в сущность.
        /// </summary>
        /// <param name="model">Модель проекта (UI-слой).</param>
        /// <returns>Сущность проекта (Entity-слой).</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если модель null.</exception>
        public static Entities.Project ToEntity(Project model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            return _mapper.Map<Entities.Project>(model);
        }

        /// <summary>
        /// Преобразует сущность проекта в модель.
        /// </summary>
        /// <param name="entity">Сущность проекта (Entity-слой).</param>
        /// <returns>Модель проекта (UI-слой).</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если сущность null.</exception>
        public static Project ToModel(Entities.Project entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            return _mapper.Map<Project>(entity);
        }

        /// <summary>
        /// Преобразует UI-модель чата в бизнес-сущность.
        /// </summary>
        /// <param name="model">UI-модель чата.</param>
        /// <returns>Бизнес-сущность <see cref="Entities.Chat"/>.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="model"/> равна null.</exception>
        public static Entities.Chat ToEntity(Chat model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            return _mapper.Map<Entities.Chat>(model);
        }

        /// <summary>
        /// Преобразует бизнес-сущность чата в UI-модель.
        /// </summary>
        /// <param name="entity">Бизнес-сущность <see cref="Entities.Chat"/>.</param>
        /// <returns>UI-модель <see cref="Chat"/>.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="entity"/> равна null.</exception>
        public static Chat ToModel(Entities.Chat entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            return _mapper.Map<Chat>(entity);
        }

        /// <summary>
        /// Преобразует UI-модель сообщения в бизнес-сущность.
        /// </summary>
        /// <param name="model">UI-модель сообщения.</param>
        /// <returns>Бизнес-сущность <see cref="Entities.Message"/>.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="model"/> равна null.</exception>
        public static Entities.Message ToEntity(Message model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            return _mapper.Map<Entities.Message>(model);
        }

        /// <summary>
        /// Преобразует бизнес-сущность сообщения в UI-модель.
        /// </summary>
        /// <param name="entity">Бизнес-сущность <see cref="Entities.Message"/>.</param>
        /// <returns>UI-модель <see cref="Message"/>.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="entity"/> равна null.</exception>
        public static Message ToModel(Entities.Message entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            return _mapper.Map<Message>(entity);
        }

    }
}
