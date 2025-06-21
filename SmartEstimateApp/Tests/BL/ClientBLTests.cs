using Bl;
using Common.Search;
using Dal.Interfaces;
using Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Extensions;

namespace Tests.BL;

public class ClientBLTests
{
    private readonly Mock<IClientDal> _clientDalMock;
    private readonly Mock<ILogger<ClientBL>> _loggerMock;
    private readonly ClientBL _clientBL;

    public ClientBLTests()
    {
        _clientDalMock = new Mock<IClientDal>();
        _loggerMock = new Mock<ILogger<ClientBL>>();
        _clientBL = new ClientBL(_clientDalMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task AddOrUpdateAsync_ValidClient_ReturnsId()
    {
        // Arrange
        var client = new Client
        {
            Id = 1,
            Email = "test@example.com",
            Phone = "+1234567890",
            User = new User { Id = 1 },
            Name = "Test Client"
        };
        const long expectedId = 1;
        _clientDalMock.Setup(d => d.ExistsAsync(client.Email, client.User.Id)).ReturnsAsync(false);
        _clientDalMock.Setup(d => d.ExistsPhoneAsync(client.Phone, client.User.Id)).ReturnsAsync(false);
        _clientDalMock.Setup(d => d.AddOrUpdateAsync(It.IsAny<Client>())).ReturnsAsync(expectedId);

        // Act
        var result = await _clientBL.AddOrUpdateAsync(client);

        // Assert
        Assert.Equal(expectedId, result);
        _clientDalMock.Verify(d => d.AddOrUpdateAsync(It.IsAny<Client>()), Times.Once());
        _loggerMock.VerifyLog(LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task AddOrUpdateAsync_InvalidEmail_ThrowsArgumentException()
    {
        // Arrange
        var client = new Client
        {
            Id = 1,
            Email = "invalid-email",
            Phone = "+1234567890",
            User = new User { Id = 1 }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _clientBL.AddOrUpdateAsync(client));
        Assert.Equal($"{ErrorMessages.InvalidEmailFormat} (Parameter 'Email')", exception.Message);
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task AddOrUpdateAsync_InvalidPhone_ThrowsArgumentException()
    {
        // Arrange
        var client = new Client
        {
            Id = 1,
            Email = "test@example.com",
            Phone = "invalid-phone",
            User = new User { Id = 1 }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _clientBL.AddOrUpdateAsync(client));
        Assert.Equal($"{ErrorMessages.InvalidPhoneFormat} (Parameter 'Phone')", exception.Message);
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task AddOrUpdateAsync_EmailExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var client = new Client
        {
            Id = 0,
            Email = "test@example.com",
            Phone = "+1234567890",
            User = new User { Id = 1 }
        };
        _clientDalMock.Setup(d => d.ExistsAsync(client.Email, client.User.Id)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _clientBL.AddOrUpdateAsync(client));
        Assert.Equal(ErrorMessages.ClientEmailAlreadyExists, exception.Message);
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task AddOrUpdateAsync_PhoneExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var client = new Client
        {
            Id = 0,
            Email = "test@example.com",
            Phone = "+1234567890",
            User = new User { Id = 1 }
        };
        _clientDalMock.Setup(d => d.ExistsAsync(client.Email, client.User.Id)).ReturnsAsync(false);
        _clientDalMock.Setup(d => d.ExistsPhoneAsync(client.Phone, client.User.Id)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _clientBL.AddOrUpdateAsync(client));
        Assert.Equal(ErrorMessages.ClientPhoneAlreadyExists, exception.Message);
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExistsAsync_ById_ReturnsTrue()
    {
        // Arrange
        const long id = 1;
        _clientDalMock.Setup(d => d.ExistsAsync(id)).ReturnsAsync(true);

        // Act
        var result = await _clientBL.ExistsAsync(id);

        // Assert
        Assert.True(result);
        _loggerMock.VerifyLog(LogLevel.Debug, Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExistsAsync_ByEmailAndUser_ReturnsTrue()
    {
        // Arrange
        const string email = "test@example.com";
        const long userId = 1;
        _clientDalMock.Setup(d => d.ExistsAsync(email, userId)).ReturnsAsync(true);

        // Act
        var result = await _clientBL.ExistsAsync(email, userId);

        // Assert
        Assert.True(result);
        _loggerMock.VerifyLog(LogLevel.Debug, Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExistsPhoneAsync_ByPhoneAndUser_ReturnsTrue()
    {
        // Arrange
        const string phone = "+1234567890";
        const long userId = 1;
        _clientDalMock.Setup(d => d.ExistsPhoneAsync(phone, userId)).ReturnsAsync(true);

        // Act
        var result = await _clientBL.ExistsPhoneAsync(phone, userId);

        // Assert
        Assert.True(result);
        _loggerMock.VerifyLog(LogLevel.Debug, Times.AtLeastOnce());
    }

    [Fact]
    public async Task GetAsync_ById_ReturnsClient()
    {
        // Arrange
        const long id = 1;
        var client = new Client { Id = id, Email = "test@example.com", User = new User { Id = 1 } };
        _clientDalMock.Setup(d => d.GetAsync(id, false)).ReturnsAsync(client);

        // Act
        var result = await _clientBL.GetAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.NotNull(result.User);
        Assert.Equal(1, result.User.Id);
        _loggerMock.VerifyLog(LogLevel.Debug, Times.AtLeastOnce());
    }

    [Fact]
    public async Task DeleteAsync_ValidId_ReturnsTrue()
    {
        // Arrange
        const long id = 1;
        _clientDalMock.Setup(d => d.DeleteAsync(id)).ReturnsAsync(true);

        // Act
        var result = await _clientBL.DeleteAsync(id);

        // Assert
        Assert.True(result);
        _loggerMock.VerifyLog(LogLevel.Information, Times.AtLeastOnce());
    }

    [Fact]
    public async Task GetAsync_NullSearchParams_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _clientBL.AddOrUpdateAsync(null));
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task GetAsync_MissingUserIdInSearchParams_ThrowsArgumentException()
    {
        // Arrange
        var searchParams = new ClientSearchParams { Email = "test@example.com" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _clientBL.GetAsync(searchParams));
        Assert.Equal($"{ErrorMessages.UserIdRequired} (Parameter 'UserId')", exception.Message);
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }
}