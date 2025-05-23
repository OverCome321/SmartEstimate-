using Common.Search;
using Entities;

namespace Dal.Interfaces;

/// <summary>
/// Интерфейс доступа к данным чатов и сообщений
/// </summary>
public interface IChatDal : IBaseDal<Chat, long, ChatSearchParams, object>
{
    /// <summary>
    /// Создать новый чат (диалог) для пользователя
    /// </summary>
    /// <param name="userId">Id пользователя-владельца</param>
    /// <returns>Id созданного чата</returns>
    Task<long> CreateChatAsync(long userId);

    /// <summary>
    /// Отправить сообщение в чат
    /// </summary>
    /// <param name="chatId">Id чата</param>
    /// <param name="senderUserId">Id отправителя (пользователь или бот)</param>
    /// <param name="text">Текст сообщения</param>
    /// <returns>Id сохранённого сообщения</returns>
    Task<long> SendMessageAsync(long chatId, long senderUserId, string text);
}
