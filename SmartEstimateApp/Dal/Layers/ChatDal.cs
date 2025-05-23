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
    private readonly IMapper _mapper;
    private readonly ILogger<ChatDal> _logger;

    public ChatDal(
        SmartEstimateDbContext context,
        IMapper mapper,
        ILogger<ChatDal> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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

        return dbList.Select(MapToEntityChat).ToList();
    }

    protected override async Task<long> AddOrUpdateInternalAsync(Entities.Chat entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var dbObj = MapToDbChat(entity);
        if (entity.Id > 0 && await _context.Chats.AnyAsync(c => c.Id == entity.Id))
        {
            _context.Chats.Update(dbObj);
            _logger.LogInformation("Обновлён чат Id={ChatId}", dbObj.Id);
        }
        else
        {
            dbObj.StartedAt = DateTime.UtcNow;
            await _context.Chats.AddAsync(dbObj);
            _logger.LogInformation("Добавлен новый чат для UserId={UserId}", dbObj.UserId);
        }
        return dbObj.Id;
    }

    protected override async Task<IList<long>> AddOrUpdateInternalAsync(
        IList<Entities.Chat> entities)
    {
        var ids = new List<long>();
        foreach (var e in entities)
            ids.Add(await AddOrUpdateInternalAsync(e));
        return ids;
    }

    protected override Task UpdateBeforeSavingAsync(
        Entities.Chat entity, Chat dbObject, bool exists)
    {
        // Всё что нужно для StartedAt делаем в AddOrUpdateInternalAsync
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

    #region Mapping Helpers

    /// <summary>
    /// Преобразовать из Dal.DbModels.Chat в Entities.Chat
    /// </summary>
    private Entities.Chat MapToEntityChat(Chat dbChat)
        => _mapper.Map<Entities.Chat>(dbChat);

    /// <summary>
    /// Преобразовать из Entities.Chat в Dal.DbModels.Chat
    /// </summary>
    private Chat MapToDbChat(Entities.Chat entity)
        => _mapper.Map<Chat>(entity);

    #endregion
}
