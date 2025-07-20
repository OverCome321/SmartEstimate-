using AutoMapper;

namespace Dal.Helpers
{
    public static class CollectionSyncHelper
    {
        /// <summary>
        /// Синхронизирует коллекцию из базы данных с коллекцией DTO.
        /// Выполняет удаление, обновление и добавление элементов для приведения
        /// dbCollection в соответствие с dtoCollection.
        /// </summary>
        /// <typeparam name="TDb">Тип сущности в БД (например, Dal.DbModels.Estimate).</typeparam>
        /// <typeparam name="TDto">Тип DTO (например, Entities.Estimate).</typeparam>
        /// <typeparam name="TKey">Тип ключа для сопоставления (например, long для ID).</typeparam>
        /// <param name="dbCollection">Коллекция, загруженная из БД и отслеживаемая EF.</param>
        /// <param name="dtoCollection">Коллекция с актуальными данными.</param>
        /// <param name="keySelector">Функция, возвращающая ключ из DTO для сопоставления.</param>
        /// <param name="updateAction">Действие, которое нужно выполнить для обновления существующего элемента.</param>
        /// <param name="addAction">Функция, которая создает новый элемент БД из DTO.</param>
        /// <param name="mapper">Экземпляр IMapper для преобразования типов.</param>
        public static void Sync<TDb, TDto, TKey>(
            ICollection<TDb> dbCollection,
            ICollection<TDto> dtoCollection,
            Func<TDto, TKey> keySelector,
            Action<TDb, TDto> updateAction,
            Func<TDto, TDb> addAction,
            IMapper mapper) where TDb : class where TKey : IEquatable<TKey>
        {
            if (dtoCollection == null || !dtoCollection.Any())
            {
                dbCollection.Clear();
                return;
            }

            var dtoDict = dtoCollection.ToDictionary(keySelector);

            var toRemove = dbCollection
                .Where(dbItem => !dtoDict.ContainsKey(keySelector(mapper.Map<TDto>(dbItem))))
                .ToList();

            foreach (var item in toRemove)
            {
                dbCollection.Remove(item);
            }

            foreach (var dtoItem in dtoCollection)
            {
                var key = keySelector(dtoItem);

                var dbItem = dbCollection.FirstOrDefault(db => keySelector(mapper.Map<TDto>(db)).Equals(key));

                if (dbItem != null)
                {
                    updateAction(dbItem, dtoItem);
                }
                else
                {
                    dbCollection.Add(addAction(dtoItem));
                }
            }
        }
    }
}