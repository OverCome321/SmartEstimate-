using Bl.Interfaces;
using Common.Search;
using Dal.Interfaces;
using Entities;
using Microsoft.Extensions.Logging;

namespace Bl;

/// <summary>
/// Бизнес-логика для работы с чатами и сообщениями
/// </summary>
public class ChatBL : IChatBL
{
    private readonly IChatDal _chatDal;
    private readonly ILogger<ChatBL> _logger;

    public ChatBL(IChatDal chatDal, ILogger<ChatBL> logger)
    {
        _chatDal = chatDal ?? throw new ArgumentNullException(nameof(chatDal));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<long> AddOrUpdateAsync(Chat entity)
    {
        try
        {
            if (entity == null)
            {
                _logger.LogWarning("Попытка добавить/обновить чат с null entity");
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity.UserId <= 0)
            {
                _logger.LogWarning("Попытка добавить/обновить чат с некорректным userId={UserId}", entity.UserId);
                throw new ArgumentException("UserId должен быть больше 0", nameof(entity.UserId));
            }

            var result = await _chatDal.AddOrUpdateAsync(entity);
            _logger.LogInformation("Чат успешно добавлен/обновлён: {@Chat}", entity);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении/обновлении чата: {@Chat}", entity);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(long id)
    {
        try
        {
            // Можно добавить DAL-метод ExistsAsync, сейчас просто пытаем получить:
            var chat = await _chatDal.GetAsync(id, isFull: false);
            bool exists = chat != null;
            _logger.LogDebug("Проверка существования чата по Id={Id}: {Exists}", id, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при ExistsAsync(Id={Id})", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Chat> GetAsync(long id, bool includeRelated = false)
    {
        try
        {
            var chat = await _chatDal.GetAsync(id, includeRelated);
            if (chat == null)
                _logger.LogInformation("Чат не найден по Id={Id}", id);
            else
                _logger.LogDebug("Получен чат: {@Chat}", chat);
            return chat;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при GetAsync(Id={Id})", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(long id)
    {
        try
        {
            bool deleted = await _chatDal.DeleteAsync(id);
            _logger.LogInformation("Удаление чата Id={Id}: результат={Deleted}", id, deleted);
            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при DeleteAsync(Id={Id})", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SearchResult<Chat>> GetAsync(ChatSearchParams searchParams, bool includeMessages = true)
    {
        try
        {
            if (searchParams == null)
            {
                _logger.LogWarning("Попытка поиска чатов с null параметрами");
                throw new ArgumentNullException(nameof(searchParams));
            }

            var result = await _chatDal.GetAsync(searchParams, includeMessages);
            _logger.LogDebug("Результат поиска чатов по {@Params}: {@Result}", searchParams, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске чатов по {@Params}", searchParams);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<long> CreateChatAsync(long userId)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("Попытка создать чат с некорректным userId={UserId}", userId);
            throw new ArgumentException("UserId должен быть больше 0", nameof(userId));
        }
        return await _chatDal.CreateChatAsync(userId);
    }

    /// <inheritdoc />
    public async Task<long> SendMessageAsync(long chatId, long senderUserId, string text)
    {
        if (chatId <= 0)
        {
            _logger.LogWarning("Попытка отправить сообщение в некорректный chatId={ChatId}", chatId);
            throw new ArgumentException("ChatId должен быть больше 0", nameof(chatId));
        }
        if (senderUserId <= 0)
        {
            _logger.LogWarning("Попытка отправить сообщение от некорректного senderUserId={Sender}", senderUserId);
            throw new ArgumentException("SenderUserId должен быть больше 0", nameof(senderUserId));
        }
        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogWarning("Попытка отправить пустое сообщение в chatId={ChatId}", chatId);
            throw new ArgumentException("Text не может быть пустым", nameof(text));
        }

        return await _chatDal.SendMessageAsync(chatId, senderUserId, text);
    }
}
