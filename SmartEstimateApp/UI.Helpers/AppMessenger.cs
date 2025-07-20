namespace UI.Helpers
{
    public static class AppMessenger
    {
        /// <summary>
        /// Внутренний приватный класс, использующий дженерики для создания
        /// отдельного набора событий для каждого типа сущности (TEntity).
        /// Например, события для Events<Client> будут полностью независимы от событий для Events<Project>.
        /// </summary>
        private static class Events<TEntity> where TEntity : class
        {
            /// <summary>
            /// Событие, срабатывающее при обновлении или создании сущности.
            /// Передает в качестве параметра саму измененную сущность.
            /// </summary>
            public static event Action<TEntity> EntityUpdated;

            /// <summary>
            /// Событие, срабатывающее при удалении сущности.
            /// Передает в качестве параметра ID удаленной сущности.
            /// </summary>
            public static event Action<long> EntityDeleted;

            /// <summary>
            /// Внутренний метод для безопасного вызова события обновления.
            /// Проверка '?' (null-conditional) гарантирует, что не будет ошибки,
            /// если у события нет подписчиков.
            /// </summary>
            public static void InvokeUpdate(TEntity entity) => EntityUpdated?.Invoke(entity);

            /// <summary>
            /// Внутренний метод для безопасного вызова события удаления.
            /// </summary>
            public static void InvokeDelete(long entityId) => EntityDeleted?.Invoke(entityId);
        }

        #region Публичные методы для отправки сообщений (Издатели)

        /// <summary>
        /// Отправляет (публикует) сообщение о том, что сущность была обновлена или создана.
        /// Все подписчики на тип TEntity получат это сообщение.
        /// </summary>
        /// <typeparam name="TEntity">Тип сущности (например, Client, Project).</typeparam>
        /// <param name="entity">Обновленная или созданная сущность.</param>
        public static void SendEntityUpdateMessage<TEntity>(TEntity entity) where TEntity : class
        {
            Events<TEntity>.InvokeUpdate(entity);
        }

        /// <summary>
        /// Отправляет (публикует) сообщение о том, что сущность была удалена.
        /// </summary>
        /// <typeparam name="TEntity">Тип удаляемой сущности.</typeparam>
        /// <param name="entityId">ID удаленной сущности.</param>
        public static void SendEntityDeleteMessage<TEntity>(long entityId) where TEntity : class
        {
            Events<TEntity>.InvokeDelete(entityId);
        }

        #endregion

        #region Публичные методы для управления подпиской (Подписчики)

        /// <summary>
        /// Подписывает обработчик на событие обновления/создания сущности указанного типа.
        /// </summary>
        /// <typeparam name="TEntity">Тип сущности, на который нужно подписаться.</typeparam>
        /// <param name="handler">Метод-обработчик (Action), который будет вызван при получении сообщения.</param>
        public static void RegisterForEntityUpdate<TEntity>(Action<TEntity> handler) where TEntity : class
        {
            Events<TEntity>.EntityUpdated += handler;
        }

        /// <summary>
        /// Отписывает обработчик от события обновления/создания сущности.
        /// Крайне важно вызывать этот метод для подписчиков с коротким временем жизни,
        /// чтобы избежать утечек памяти.
        /// </summary>
        public static void UnregisterFromEntityUpdate<TEntity>(Action<TEntity> handler) where TEntity : class
        {
            Events<TEntity>.EntityUpdated -= handler;
        }

        /// <summary>
        /// Подписывает обработчик на событие удаления сущности указанного типа.
        /// </summary>
        public static void RegisterForEntityDelete<TEntity>(Action<long> handler) where TEntity : class
        {
            Events<TEntity>.EntityDeleted += handler;
        }

        /// <summary>
        /// Отписывает обработчик от события удаления сущности.
        /// </summary>
        public static void UnregisterFromEntityDelete<TEntity>(Action<long> handler) where TEntity : class
        {
            Events<TEntity>.EntityDeleted -= handler;
        }

        #endregion
    }
}