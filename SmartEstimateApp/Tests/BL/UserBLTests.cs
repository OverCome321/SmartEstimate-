using Bl;
using Bl.DI;
using Common.Search;
using Dal.Interfaces;
using Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tests.Extensions;

namespace Tests.BL;

public class UserBLTests
{
    private readonly Mock<IUserDal> _userDalMock;
    private readonly Mock<IOptions<BusinessLogicOptions>> _optionsMock;
    private readonly Mock<ILogger<UserBL>> _loggerMock;
    private readonly UserBL _userBL;
    private readonly BusinessLogicOptions _defaultOptions;

    public UserBLTests()
    {
        _userDalMock = new Mock<IUserDal>();
        _optionsMock = new Mock<IOptions<BusinessLogicOptions>>();
        _loggerMock = new Mock<ILogger<UserBL>>();
        _defaultOptions = new BusinessLogicOptions
        {
            MinPasswordLength = 8,
            EnableExtendedValidation = true,
            RequireComplexPassword = true
        };
        _optionsMock.Setup(o => o.Value).Returns(_defaultOptions);

        _userBL = new UserBL(_userDalMock.Object, _optionsMock.Object, _loggerMock.Object);
    }

    #region AddOrUpdateAsync Tests

    [Fact]
    public async Task AddOrUpdateAsync_NullUser_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _userBL.AddOrUpdateAsync(null));
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task AddOrUpdateAsync_InvalidEmail_ThrowsArgumentException()
    {
        var user = new User
        {
            Email = "invalid-email",
            PasswordHash = "ComplexPass123!",
            Role = new Role { Id = 1 }
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.ValidationCommand(user));
        Assert.Equal(ErrorMessages.EmailInvalidFormat + " (Parameter 'Email')", exception.Message);
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task AddOrUpdateAsync_EmailExists_ThrowsInvalidOperationException()
    {
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "ComplexPass123!",
            Role = new Role { Id = 1 }
        };
        _userDalMock.Setup(d => d.ExistsAsync(user.Email)).ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _userBL.ValidationCommand(user));
        Assert.Equal(ErrorMessages.EmailAlreadyExists, exception.Message);
        _userDalMock.Verify(d => d.ExistsAsync(user.Email), Times.Once());
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task AddOrUpdateAsync_NonComplexPassword_ThrowsArgumentException()
    {
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "SimplePassword",
            Role = new Role { Id = 1 }
        };
        _userDalMock.Setup(d => d.ExistsAsync(user.Email)).ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.ValidationCommand(user));
        Assert.Equal(ErrorMessages.ComplexPasswordRequired + " (Parameter 'password')", exception.Message);
        _loggerMock.VerifyLog(LogLevel.Error, Times.AtLeastOnce());
    }

    #endregion

    #region ValidationCommand Tests

    [Fact]
    public async Task ValidationCommand_NullUser_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _userBL.ValidationCommand(null));
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task ValidationCommand_EmptyEmail_ThrowsArgumentException()
    {
        var user = new User { Email = "", PasswordHash = "ComplexPass123!", Role = new Role { Id = 1 } };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.ValidationCommand(user));
        Assert.Equal(ErrorMessages.EmailInvalidFormat + " (Parameter 'Email')", exception.Message);
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task ValidationCommand_InvalidEmail_ThrowsArgumentException()
    {
        var user = new User
        {
            Email = "invalid-email",
            PasswordHash = "ComplexPass123!",
            Role = new Role { Id = 1 }
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.ValidationCommand(user));
        Assert.Equal(ErrorMessages.EmailInvalidFormat + " (Parameter 'Email')", exception.Message);
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task ValidationCommand_NullRole_ThrowsArgumentException()
    {
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "ComplexPass123!",
            Role = null
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.ValidationCommand(user));
        Assert.Equal(ErrorMessages.RoleNotSpecified + " (Parameter 'Role')", exception.Message);
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task ValidationCommand_ZeroRoleId_ThrowsArgumentException()
    {
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "ComplexPass123!",
            Role = new Role { Id = 0 }
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.ValidationCommand(user));
        Assert.Equal(ErrorMessages.RoleNotSpecified + " (Parameter 'Role')", exception.Message);
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task ValidationCommand_WeakPassword_ThrowsArgumentException()
    {
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "weak",
            Role = new Role { Id = 1 }
        };
        _userDalMock.Setup(d => d.ExistsAsync(user.Email)).ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.ValidationCommand(user));
        Assert.Equal(string.Format(ErrorMessages.PasswordTooShort + " (Parameter 'password')", _defaultOptions.MinPasswordLength), exception.Message);
        _loggerMock.VerifyLog(LogLevel.Error, Times.AtLeastOnce());
    }

    [Fact]
    public async Task ValidationCommand_NonComplexPassword_ThrowsArgumentException()
    {
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "SimplePassword",
            Role = new Role { Id = 1 }
        };
        _userDalMock.Setup(d => d.ExistsAsync(user.Email)).ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.ValidationCommand(user));
        Assert.Equal(ErrorMessages.ComplexPasswordRequired + " (Parameter 'password')", exception.Message);
        _loggerMock.VerifyLog(LogLevel.Error, Times.AtLeastOnce());
    }

    [Fact]
    public async Task ValidationCommand_EmailExists_ThrowsInvalidOperationException()
    {
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "ComplexPass123!",
            Role = new Role { Id = 1 }
        };
        _userDalMock.Setup(d => d.ExistsAsync(user.Email)).ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _userBL.ValidationCommand(user));
        Assert.Equal(ErrorMessages.EmailAlreadyExists, exception.Message);
        _userDalMock.Verify(d => d.ExistsAsync(user.Email), Times.Once());
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task ValidationCommand_ValidUser_Passes()
    {
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "ComplexPass123!",
            Role = new Role { Id = 1 }
        };
        _userDalMock.Setup(d => d.ExistsAsync(user.Email)).ReturnsAsync(false);

        await _userBL.ValidationCommand(user);

        _userDalMock.Verify(d => d.ExistsAsync(user.Email), Times.Once());
        _loggerMock.VerifyLog(LogLevel.Debug, Times.AtLeastOnce());
    }

    #endregion

    #region Other Tests (with logging)

    [Fact]
    public async Task ExistsAsync_ById_ReturnsTrue()
    {
        const long id = 1;
        _userDalMock.Setup(d => d.ExistsAsync(id)).ReturnsAsync(true);

        var result = await _userBL.ExistsAsync(id);

        Assert.True(result);
        _loggerMock.VerifyLog(LogLevel.Debug, Times.AtLeastOnce());
    }

    [Fact]
    public async Task GetAsync_ByIdWithRole_ReturnsUser()
    {
        const long id = 1;
        var user = new User { Id = id, Email = "test@example.com", Role = new Role { Id = 2 } };
        _userDalMock.Setup(d => d.GetAsync(id, true))
            .ReturnsAsync(user);

        var result = await _userBL.GetAsync(id, true);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.NotNull(result.Role);
        _loggerMock.VerifyLog(LogLevel.Debug, Times.AtLeastOnce());
    }

    [Fact]
    public async Task DeleteAsync_ValidId_ReturnsTrue()
    {
        const long id = 1;
        _userDalMock.Setup(d => d.DeleteAsync(id)).ReturnsAsync(true);

        var result = await _userBL.DeleteAsync(id);

        Assert.True(result);
        _loggerMock.VerifyLog(LogLevel.Information, Times.AtLeastOnce());
    }

    [Fact]
    public async Task GetAsync_WithSearchParams_ReturnsSearchResult()
    {
        var searchParams = new UserSearchParams { Email = "test@example.com" };
        var user = new User { Id = 1, Email = "test@example.com", Role = new Role { Id = 2 } };
        var searchResult = new SearchResult<User> { Objects = new[] { user }, Total = 1 };
        _userDalMock.Setup(d => d.GetAsync(It.Is<UserSearchParams>(p => p.Email == searchParams.Email), true))
            .ReturnsAsync(searchResult);

        var result = await _userBL.GetAsync(searchParams, true);

        Assert.NotNull(result);
        Assert.Equal(1, result.Total);
        Assert.Single(result.Objects);
        Assert.Equal(user.Email, result.Objects.First().Email);
        Assert.NotNull(result.Objects.First().Role);
        _loggerMock.VerifyLog(LogLevel.Debug, Times.AtLeastOnce());
    }
    #endregion
}