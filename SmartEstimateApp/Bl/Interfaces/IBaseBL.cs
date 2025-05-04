namespace Bl.Interfaces
{
    /// <summary>
    /// Базовый интерфейс для бизнес-логики с общими операциями CRUD
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности</typeparam>
    /// <typeparam name="TId">Тип идентификатора сущности</typeparam>
    public interface IBaseBL<TEntity, TId> where TEntity : class
    {
        /// <summary>
        /// Добавляет или обновляет сущность
        /// </summary>
        /// <param name="entity">Сущность</param>
        /// <returns>Идентификатор добавленной или обновленной сущности</returns>
        Task<TId> AddOrUpdateAsync(TEntity entity);

        /// <summary>
        /// Проверяет существование сущности по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>True, если сущность существует; иначе false</returns>
        Task<bool> ExistsAsync(TId id);

        /// <summary>
        /// Получает сущность по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <param name="includeRelated">Включать ли связанные данные</param>
        /// <returns>Найденная сущность или null</returns>
        Task<TEntity> GetAsync(TId id, bool includeRelated = false);

        /// <summary>
        /// Удаляет сущность по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>True, если сущность была удалена; иначе false</returns>
        Task<bool> DeleteAsync(TId id);
    }
}