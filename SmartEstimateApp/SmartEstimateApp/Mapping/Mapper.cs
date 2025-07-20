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

        // --- User Mappings ---

        /// <summary>
        /// Преобразует модель пользователя (UI) в сущность пользователя (Entity).
        /// </summary>
        /// <param name="model">Модель пользователя для преобразования.</param>
        /// <returns>Сущность пользователя.</returns>
        public static Entities.User ToEntity(User model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            return _mapper.Map<Entities.User>(model);
        }

        /// <summary>
        /// Преобразует сущность пользователя (Entity) в модель пользователя (UI).
        /// </summary>
        /// <param name="entity">Сущность пользователя для преобразования.</param>
        /// <returns>Модель пользователя.</returns>
        public static User ToModel(Entities.User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            return _mapper.Map<User>(entity);
        }

        // --- Role Mappings ---

        /// <summary>
        /// Преобразует модель роли (UI) в сущность роли (Entity).
        /// </summary>
        /// <param name="model">Модель роли для преобразования.</param>
        /// <returns>Сущность роли.</returns>
        public static Entities.Role ToEntity(Role model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            return _mapper.Map<Entities.Role>(model);
        }

        /// <summary>
        /// Преобразует сущность роли (Entity) в модель роли (UI).
        /// </summary>
        /// <param name="entity">Сущность роли для преобразования.</param>
        /// <returns>Модель роли.</returns>
        public static Role ToModel(Entities.Role entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            return _mapper.Map<Role>(entity);
        }

        // --- Project Mappings ---

        /// <summary>
        /// Преобразует модель проекта (UI) в сущность проекта (Entity).
        /// </summary>
        /// <param name="model">Модель проекта для преобразования.</param>
        /// <returns>Сущность проекта.</returns>
        public static Entities.Project ToEntity(Project model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            return _mapper.Map<Entities.Project>(model);
        }

        /// <summary>
        /// Преобразует сущность проекта (Entity) в модель проекта (UI).
        /// </summary>
        /// <param name="entity">Сущность проекта для преобразования.</param>
        /// <returns>Модель проекта.</returns>
        public static Project ToModel(Entities.Project entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            return _mapper.Map<Project>(entity);
        }

        // --- Estimate Mappings ---

        /// <summary>
        /// Преобразует модель сметы (UI) в сущность сметы (Entity).
        /// </summary>
        /// <param name="model">Модель сметы для преобразования.</param>
        /// <returns>Сущность сметы.</returns>
        public static Entities.Estimate ToEntity(Estimate model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            return _mapper.Map<Entities.Estimate>(model);
        }

        /// <summary>
        /// Преобразует сущность сметы (Entity) в модель сметы (UI).
        /// </summary>
        /// <param name="entity">Сущность сметы для преобразования.</param>
        /// <returns>Модель сметы.</returns>
        public static Estimate ToModel(Entities.Estimate entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            return _mapper.Map<Estimate>(entity);
        }

        /// <summary>
        /// Преобразует модель позиции сметы (UI) в сущность позиции сметы (Entity).
        /// </summary>
        /// <param name="model">Модель позиции сметы для преобразования.</param>
        /// <returns>Сущность позиции сметы.</returns>
        public static Entities.EstimateItem ToEntity(EstimateItem model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            return _mapper.Map<Entities.EstimateItem>(model);
        }

        /// <summary>
        /// Преобразует сущность позиции сметы (Entity) в модель позиции сметы (UI).
        /// </summary>
        /// <param name="entity">Сущность позиции сметы для преобразования.</param>
        /// <returns>Модель позиции сметы.</returns>
        public static EstimateItem ToModel(Entities.EstimateItem entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            return _mapper.Map<EstimateItem>(entity);
        }

        // --- Chat Mappings ---

        /// <summary>
        /// Преобразует модель чата (UI) в сущность чата (Entity).
        /// </summary>
        /// <param name="model">Модель чата для преобразования.</param>
        /// <returns>Сущность чата.</returns>
        public static Entities.Chat ToEntity(Chat model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            return _mapper.Map<Entities.Chat>(model);
        }

        /// <summary>
        /// Преобразует сущность чата (Entity) в модель чата (UI).
        /// </summary>
        /// <param name="entity">Сущность чата для преобразования.</param>
        /// <returns>Модель чата.</returns>
        public static Chat ToModel(Entities.Chat entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            return _mapper.Map<Chat>(entity);
        }

        // --- Message Mappings ---

        /// <summary>
        /// Преобразует модель сообщения (UI) в сущность сообщения (Entity).
        /// </summary>
        /// <param name="model">Модель сообщения для преобразования.</param>
        /// <returns>Сущность сообщения.</returns>
        public static Entities.Message ToEntity(Message model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            return _mapper.Map<Entities.Message>(model);
        }

        /// <summary>
        /// Преобразует сущность сообщения (Entity) в модель сообщения (UI).
        /// </summary>
        /// <param name="entity">Сущность сообщения для преобразования.</param>
        /// <returns>Модель сообщения.</returns>
        public static Message ToModel(Entities.Message entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            return _mapper.Map<Message>(entity);
        }

        // --- Client Mappings ---

        /// <summary>
        /// Преобразует модель сообщения (UI) в сущность сообщения (Entity).
        /// </summary>
        /// <param name="model">Модель сообщения для преобразования.</param>
        /// <returns>Сущность сообщения.</returns>
        public static Entities.Client ToEntity(Client model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            return _mapper.Map<Entities.Client>(model);
        }

        /// <summary>
        /// Преобразует сущность сообщения (Entity) в модель сообщения (UI).
        /// </summary>
        /// <param name="entity">Сущность сообщения для преобразования.</param>
        /// <returns>Модель сообщения.</returns>
        public static Client ToModel(Entities.Client entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            return _mapper.Map<Client>(entity);
        }
    }
}