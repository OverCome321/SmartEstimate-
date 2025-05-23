using Common.Search;
using Entities;

namespace Bl.Interfaces;

/// <summary>
/// Интерфейс бизнес-логики для работы с чатами и сообщениями
/// </summary>
public interface IChatBL : IBaseBL<Chat, long>
{
    /// <summary>
    /// Создать новый чат для пользователя
    /// </summary>
    Task<long> CreateChatAsync(long userId);

    /// <summary>
    /// Отправить сообщение в чат
    /// </summary>
    Task<long> SendMessageAsync(long chatId, long senderUserId, string text);

    /// <summary>
    /// Получить список чатов по параметрам поиска
    /// </summary>
    Task<SearchResult<Chat>> GetAsync(ChatSearchParams searchParams, bool includeMessages = true);
}
