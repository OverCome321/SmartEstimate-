using Bl;
using Bl.Interfaces;
using Common.Search;
using Dal.Interfaces;
using Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Extensions;

namespace Tests.BL;

public class ChatBLTests
{
    private readonly Mock<IChatDal> _chatDalMock;
    private readonly Mock<ILogger<ChatBL>> _loggerMock;
    private readonly IChatBL _chatBL;

    public ChatBLTests()
    {
        _chatDalMock = new Mock<IChatDal>();
        _loggerMock = new Mock<ILogger<ChatBL>>();
        _chatBL = new ChatBL(_chatDalMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task AddOrUpdateAsync_ValidChat_ReturnsId()
    {
        // Arrange
        var chat = new Chat { Id = 0, UserId = 42 };
        const long expectedId = 100;
        _chatDalMock
            .Setup(d => d.AddOrUpdateAsync(It.IsAny<Chat>()))
            .ReturnsAsync(expectedId);

        // Act
        var result = await _chatBL.AddOrUpdateAsync(chat);

        // Assert
        Assert.Equal(expectedId, result);
        _chatDalMock.Verify(d => d.AddOrUpdateAsync(It.Is<Chat>(c => c.UserId == 42)), Times.Once);
        _loggerMock.VerifyLog(LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task AddOrUpdateAsync_NullChat_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _chatBL.AddOrUpdateAsync(null!));
        _loggerMock.VerifyLog(LogLevel.Warning, Times.Once());
    }

    [Fact]
    public async Task AddOrUpdateAsync_InvalidUserId_ThrowsArgumentException()
    {
        // Arrange
        var chat = new Chat { Id = 0, UserId = 0 };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _chatBL.AddOrUpdateAsync(chat));
        Assert.Contains("UserId должен быть больше 0", ex.Message);
        _loggerMock.VerifyLog(LogLevel.Warning, Times.Once());
    }

    [Fact]
    public async Task ExistsAsync_ChatExists_ReturnsTrue()
    {
        // Arrange
        const long id = 5;
        var dbChat = new Chat { Id = id, UserId = 1 };
        _chatDalMock.Setup(d => d.GetAsync(id, false))
            .ReturnsAsync(dbChat);

        // Act
        var exists = await _chatBL.ExistsAsync(id);

        // Assert
        Assert.True(exists);
        _loggerMock.VerifyLog(LogLevel.Debug, Times.Once());
    }

    [Fact]
    public async Task ExistsAsync_ChatNotFound_ReturnsFalse()
    {
        // Arrange
        const long id = 6;
        _chatDalMock.Setup(d => d.GetAsync(id, false))
            .ReturnsAsync((Chat?)null);

        // Act
        var exists = await _chatBL.ExistsAsync(id);

        // Assert
        Assert.False(exists);
        _loggerMock.VerifyLog(LogLevel.Debug, Times.Once());
    }

    [Fact]
    public async Task GetAsync_ValidId_ReturnsChat()
    {
        // Arrange
        const long id = 7;
        var chat = new Chat { Id = id, UserId = 1 };
        _chatDalMock.Setup(d => d.GetAsync(id, true))
            .ReturnsAsync(chat);

        // Act
        var result = await _chatBL.GetAsync(id, includeRelated: true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        _loggerMock.VerifyLog(LogLevel.Debug, Times.Once());
    }

    [Fact]
    public async Task DeleteAsync_ValidId_ReturnsTrue()
    {
        // Arrange
        const long id = 8;
        _chatDalMock.Setup(d => d.DeleteAsync(id)).ReturnsAsync(true);

        // Act
        var deleted = await _chatBL.DeleteAsync(id);

        // Assert
        Assert.True(deleted);
        _loggerMock.VerifyLog(LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task GetAsync_NullSearchParams_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _chatBL.GetAsync((ChatSearchParams?)null!));
        _loggerMock.VerifyLog(LogLevel.Warning, Times.Once());
    }

    [Fact]
    public async Task GetAsync_ValidSearchParams_ReturnsResult()
    {
        // Arrange
        var search = new ChatSearchParams { UserId = 99 };
        var sr = new SearchResult<Chat> { Total = 1, Objects = new[] { new Chat { Id = 1, UserId = 99 } } };
        _chatDalMock.Setup(d => d.GetAsync(search, true)).ReturnsAsync(sr);

        // Act
        var result = await _chatBL.GetAsync(search, includeMessages: true);

        // Assert
        Assert.Equal(1, result.Total);
        Assert.Single(result.Objects);
        _loggerMock.VerifyLog(LogLevel.Debug, Times.Once());
    }

    [Fact]
    public async Task CreateChatAsync_InvalidUserId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _chatBL.CreateChatAsync(0));
        _loggerMock.VerifyLog(LogLevel.Warning, Times.Once());
    }

    [Fact]
    public async Task CreateChatAsync_ValidUserId_ReturnsId()
    {
        // Arrange
        const long userId = 10;
        const long chatId = 20;
        _chatDalMock.Setup(d => d.CreateChatAsync(userId)).ReturnsAsync(chatId);

        // Act
        var result = await _chatBL.CreateChatAsync(userId);

        // Assert
        Assert.Equal(chatId, result);
        _chatDalMock.Verify(d => d.CreateChatAsync(userId), Times.Once());
    }

    [Theory]
    [InlineData(0, 1, "hello")]
    [InlineData(1, 0, "hello")]
    [InlineData(1, 1, null)]
    [InlineData(1, 1, "")]
    public async Task SendMessageAsync_InvalidParameters_Throws(long chatId, long sender, string? text)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _chatBL.SendMessageAsync(chatId, sender, text!));
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task SendMessageAsync_ValidParameters_ReturnsId()
    {
        // Arrange
        const long chatId = 11;
        const long sender = 22;
        const string text = "Hi!";
        const long msgId = 33;
        _chatDalMock.Setup(d => d.SendMessageAsync(chatId, sender, text))
                    .ReturnsAsync(msgId);

        // Act
        var result = await _chatBL.SendMessageAsync(chatId, sender, text);

        // Assert
        Assert.Equal(msgId, result);
        _chatDalMock.Verify(d => d.SendMessageAsync(chatId, sender, text), Times.Once());
    }
}
