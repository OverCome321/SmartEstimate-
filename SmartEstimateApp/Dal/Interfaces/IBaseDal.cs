using Common.Search;

namespace Dal.Interfaces
{
    /// <summary>
    /// Базовый интерфейс доступа к данным
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности</typeparam>
    /// <typeparam name="TObjectId">Тип идентификатора</typeparam>
    /// <typeparam name="TSearchParams">Тип параметров поиска</typeparam>
    /// <typeparam name="TConvertParams">Тип параметров конвертации</typeparam>
    public interface IBaseDal<TEntity, TObjectId, TSearchParams, TConvertParams>
        where TEntity : class
        where TSearchParams : BaseSearchParams
        where TConvertParams : class
    {
        /// <summary>
        /// Добавляет или обновляет сущность в базе данных
        /// </summary>
        /// <param name="entity">Сущность для добавления или обновления</param>
        /// <returns>Идентификатор сохраненной сущности</returns>
        Task<TObjectId> AddOrUpdateAsync(TEntity entity);

        /// <summary>
        /// Добавляет или обновляет список сущностей в базе данных
        /// </summary>
        /// <param name="entities">Список сущностей для добавления или обновления</param>
        /// <returns>Список идентификаторов сохраненных сущностей</returns>
        Task<IList<TObjectId>> AddOrUpdateAsync(IList<TEntity> entities);

        /// <summary>
        /// Получает сущность по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <param name="convertParams">Параметры конвертации</param>
        /// <returns>Найденная сущность или null</returns>
        Task<TEntity> GetAsync(TObjectId id, TConvertParams convertParams = null);

        /// <summary>
        /// Удаляет сущность по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>True, если сущность была удалена, иначе False</returns>
        Task<bool> DeleteAsync(TObjectId id);

        /// <summary>
        /// Получает список сущностей по параметрам поиска
        /// </summary>
        /// <param name="searchParams">Параметры поиска</param>
        /// <param name="convertParams">Параметры конвертации</param>
        /// <returns>Результат поиска с пагинацией</returns>
        Task<SearchResult<TEntity>> GetAsync(TSearchParams searchParams, TConvertParams? convertParams = null);
    }
}