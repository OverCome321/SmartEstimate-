using AutoMapper;
using Common.Search;
using Dal.DbModels;
using Dal.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Dal.Layers;

/// <summary>
/// DAL для работы с чатами и сообщениями
/// </summary>
/// <summary>
public class ChatDal
    : BaseDal<Chat, Entities.Chat, long, ChatSearchParams, object>,
      IChatDal
{
    private readonly SmartEstimateDbContext _context;
    private readonly ILogger<ChatDal> _logger;

    public ChatDal(
        SmartEstimateDbContext context,
        IMapper mapper,
        ILogger<ChatDal> logger) : base(mapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override bool RequiresUpdatesAfterObjectSaving => false;

    /// <summary>
    /// Создать новый чат (диалог) для пользователя
    /// </summary>
    public async Task<long> CreateChatAsync(long userId)
    {
        var dbChat = new Chat
        {
            UserId = userId,
            StartedAt = DateTime.UtcNow
        };
        await _context.Chats.AddAsync(dbChat);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Создан чат Id={ChatId} для UserId={UserId}", dbChat.Id, userId);
        return dbChat.Id;
    }

    /// <summary>
    /// Отправить сообщение в чат
    /// </summary>
    public async Task<long> SendMessageAsync(long chatId, long senderUserId, string text)
    {
        var msg = new Message
        {
            ChatId = chatId,
            SenderUserId = senderUserId,
            Text = text,
            SentAt = DateTime.UtcNow
        };
        await _context.Messages.AddAsync(msg);
        await _context.SaveChangesAsync();
        _logger.LogInformation("В чат Id={ChatId} добавлено сообщение Id={MessageId}", chatId, msg.Id);
        return msg.Id;
    }

    #region BaseDal Overrides

    protected override Expression<Func<Chat, long>> GetIdByDbObjectExpression() => c => c.Id;
    protected override Expression<Func<Entities.Chat, long>> GetIdByEntityExpression() => e => e.Id;

    protected override IQueryable<Chat> BuildDbQuery(ChatSearchParams p)
    {
        var q = _context.Chats.AsQueryable();
        if (p.UserId.HasValue)
            q = q.Where(c => c.UserId == p.UserId.Value);
        return q;
    }

    protected override async Task<IList<Entities.Chat>> BuildEntitiesListAsync(
        IQueryable<Chat> dbObjects, bool isFull)
    {
        var q = dbObjects.AsNoTracking();
        if (isFull)
            q = q.Include(c => c.Messages);
        var dbList = await q.ToListAsync();

        return dbList.Select(MapToEntity).ToList();
    }

    protected override async Task<Dal.DbModels.Chat> AddOrUpdateInternalAsync(Entities.Chat entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        try
        {
            var existingChat = entity.Id > 0
                ? await _context.Chats.FindAsync(entity.Id)
                : null;

            if (existingChat != null)
            {
                _logger.LogInformation("Обновление чата Id={ChatId}", entity.Id);
                _mapper.Map(entity, existingChat);
                await UpdateBeforeSavingAsync(entity, existingChat, true);
                return existingChat;
            }

            _logger.LogInformation("Добавление чата для пользователя UserId={UserId}", entity.UserId);
            var newDbChat = _mapper.Map<Chat>(entity);

            await UpdateBeforeSavingAsync(entity, newDbChat, false);
            await _context.Chats.AddAsync(newDbChat);

            return newDbChat;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении/обновлении чата : {@ChatEntity}", entity);
            throw;
        }
    }


    protected override async Task<IList<Dal.DbModels.Chat>> AddOrUpdateInternalAsync(
        IList<Entities.Chat> entities)
    {
        var chats = new List<Dal.DbModels.Chat>();
        foreach (var e in entities)
            chats.Add(await AddOrUpdateInternalAsync(e));
        return chats;
    }

    protected override Task UpdateBeforeSavingAsync(
        Entities.Chat entity, Chat dbObject, bool exists)
    {
        if (!exists)
        {
            dbObject.StartedAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    protected override async Task<int> CountAsync(IQueryable<Chat> query)
        => await query.CountAsync();

    protected override async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    protected override async Task<T> ExecuteWithTransactionAsync<T>(Func<Task<T>> action)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var res = await action();
            await tx.CommitAsync();
            return res;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    protected override async Task<bool> DeleteAsync(Expression<Func<Chat, bool>> predicate)
    {
        var list = await _context.Chats.Where(predicate).ToListAsync();
        if (!list.Any()) return false;
        _context.Chats.RemoveRange(list);
        await _context.SaveChangesAsync();
        return true;
    }

    protected override IQueryable<Chat> Where(Expression<Func<Chat, bool>> predicate)
        => _context.Chats.Where(predicate);

    #endregion
}
