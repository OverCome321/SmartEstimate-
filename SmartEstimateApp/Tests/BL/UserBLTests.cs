using Bl;
using Bl.DI;
using Common.Search;
using Dal.Interfaces;
using Entities;
using Microsoft.Extensions.Options;
using Moq;

namespace Tests.BL
{
    public class UserBLTests
    {
        private readonly Mock<IUserDal> _userDalMock;
        private readonly Mock<IOptions<BusinessLogicOptions>> _optionsMock;
        private readonly UserBL _userBL;
        private readonly BusinessLogicOptions _defaultOptions;

        public UserBLTests()
        {
            _userDalMock = new Mock<IUserDal>();
            _optionsMock = new Mock<IOptions<BusinessLogicOptions>>();
            _defaultOptions = new BusinessLogicOptions
            {
                MinPasswordLength = 8,
                EnableExtendedValidation = true,
                RequireComplexPassword = true
            };
            _optionsMock.Setup(o => o.Value).Returns(_defaultOptions);

            _userBL = new UserBL(_userDalMock.Object, _optionsMock.Object);
        }

        #region AddOrUpdateAsync Tests

        /// <summary>
        /// Проверяет обработку случая, когда передан null вместо пользователя.
        /// Что делаем: Вызываем AddOrUpdateAsync с null.
        /// Что ожидаем: Метод выбрасывает ArgumentNullException.
        /// Зачем нужен: Убеждаемся, что метод корректно обрабатывает некорректный входной параметр.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_NullUser_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userBL.AddOrUpdateAsync(null));
        }

        /// <summary>
        /// Проверяет обработку неверного формата email.
        /// Что делаем: Создаем пользователя с невалидным email и вызываем AddOrUpdateAsync.
        /// Что ожидаем: ValidationCommand выбрасывает ArgumentException с сообщением EmailInvalidFormat.
        /// Зачем нужен: Убеждаемся, что ValidationCommand проверяет формат email.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_InvalidEmail_ThrowsArgumentException()
        {
            // Arrange
            var user = new User
            {
                Email = "invalid-email",
                PasswordHash = "ComplexPass123!",
                Role = new Role { Id = 1 }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.ValidationCommand(user));
            Assert.Equal(ErrorMessages.EmailInvalidFormat + " (Parameter 'Email')", exception.Message);
        }

        /// <summary>
        /// Проверяет обработку существующего email.
        /// Что делаем: Создаем пользователя, мок указывает, что email уже существует.
        /// Что ожидаем: ValidationCommand выбрасывает InvalidOperationException с сообщением EmailAlreadyExists.
        /// Зачем нужен: Проверяем, что ValidationCommand не допускает дубликаты email.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_EmailExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "ComplexPass123!",
                Role = new Role { Id = 1 }
            };
            _userDalMock.Setup(d => d.ExistsAsync(user.Email)).ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _userBL.ValidationCommand(user));
            Assert.Equal(ErrorMessages.EmailAlreadyExists, exception.Message);
            _userDalMock.Verify(d => d.ExistsAsync(user.Email), Times.Once());
        }



        /// <summary>
        /// Проверяет обработку несложного пароля при требовании сложности.
        /// Что делаем: Создаем пользователя с паролем без сложности и вызываем AddOrUpdateAsync.
        /// Что ожидаем: ValidationCommand выбрасывает ArgumentException с сообщением ComplexPasswordRequired.
        /// Зачем нужен: Проверяем валидацию сложного пароля в ValidationCommand.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_NonComplexPassword_ThrowsArgumentException()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "SimplePassword",
                Role = new Role { Id = 1 }
            };
            _userDalMock.Setup(d => d.ExistsAsync(user.Email)).ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.ValidationCommand(user));
            Assert.Equal(ErrorMessages.ComplexPasswordRequired + " (Parameter 'password')", exception.Message);
        }

        #endregion

        #region ValidationCommand Tests

        /// <summary>
        /// Проверяет обработку null пользователя в ValidationCommand.
        /// Что делаем: Вызываем ValidationCommand с null.
        /// Что ожидаем: Метод выбрасывает ArgumentNullException.
        /// Зачем нужен: Убеждаемся, что ValidationCommand корректно обрабатывает null.
        /// </summary>
        [Fact]
        public async Task ValidationCommand_NullUser_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userBL.ValidationCommand(null));
        }

        /// <summary>
        /// Проверяет обработку пустого email в ValidationCommand.
        /// Что делаем: Создаем пользователя с пустым email и вызываем ValidationCommand.
        /// Что ожидаем: Метод выбрасывает ArgumentException с сообщением EmailEmpty.
        /// Зачем нужен: Проверяем валидацию email в ValidationCommand.
        /// </summary>
        [Fact]
        public async Task ValidationCommand_EmptyEmail_ThrowsArgumentException()
        {
            // Arrange
            var user = new User { Email = "", PasswordHash = "ComplexPass123!", Role = new Role { Id = 1 } };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.ValidationCommand(user));
            Assert.Equal(ErrorMessages.EmailInvalidFormat + " (Parameter 'Email')", exception.Message);
        }

        /// <summary>
        /// Проверяет обработку неверного формата email в ValidationCommand.
        /// Что делаем: Создаем пользователя с невалидным email и вызываем ValidationCommand.
        /// Что ожидаем: Метод выбрасывает ArgumentException с сообщением EmailInvalidFormat.
        /// Зачем нужен: Убеждаемся, что ValidationCommand проверяет формат email.
        /// </summary>
        [Fact]
        public async Task ValidationCommand_InvalidEmail_ThrowsArgumentException()
        {
            // Arrange
            var user = new User
            {
                Email = "invalid-email",
                PasswordHash = "ComplexPass123!",
                Role = new Role { Id = 1 }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.ValidationCommand(user));
            Assert.Equal(ErrorMessages.EmailInvalidFormat + " (Parameter 'Email')", exception.Message);
        }

        /// <summary>
        /// Проверяет обработку отсутствия роли в ValidationCommand.
        /// Что делаем: Создаем пользователя с null ролью и вызываем ValidationCommand.
        /// Что ожидаем: Метод выбрасывает ArgumentException с сообщением RoleNotSpecified.
        /// Зачем нужен: Проверяем валидацию роли в ValidationCommand.
        /// </summary>
        [Fact]
        public async Task ValidationCommand_NullRole_ThrowsArgumentException()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "ComplexPass123!",
                Role = null
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.ValidationCommand(user));
            Assert.Equal(ErrorMessages.RoleNotSpecified + " (Parameter 'Role')", exception.Message);
        }

        /// <summary>
        /// Проверяет обработку роли с нулевым ID в ValidationCommand.
        /// Что делаем: Создаем пользователя с ролью, у которой Id = 0, и вызываем ValidationCommand.
        /// Что ожидаем: Метод выбрасывает ArgumentException с сообщением RoleNotSpecified.
        /// Зачем нужен: Проверяем валидацию роли в ValidationCommand.
        /// </summary>
        [Fact]
        public async Task ValidationCommand_ZeroRoleId_ThrowsArgumentException()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "ComplexPass123!",
                Role = new Role { Id = 0 }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.ValidationCommand(user));
            Assert.Equal(ErrorMessages.RoleNotSpecified + " (Parameter 'Role')", exception.Message);
        }

        /// <summary>
        /// Проверяет обработку слабого пароля в ValidationCommand.
        /// Что делаем: Создаем пользователя с коротким паролем и вызываем ValidationCommand.
        /// Что ожидаем: Метод выбрасывает ArgumentException с сообщением PasswordTooShort.
        /// Зачем нужен: Убеждаемся, что ValidationCommand проверяет длину пароля.
        /// </summary>
        [Fact]
        public async Task ValidationCommand_WeakPassword_ThrowsArgumentException()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "weak",
                Role = new Role { Id = 1 }
            };
            _userDalMock.Setup(d => d.ExistsAsync(user.Email)).ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.ValidationCommand(user));
            Assert.Equal(string.Format(ErrorMessages.PasswordTooShort + " (Parameter 'password')", _defaultOptions.MinPasswordLength), exception.Message);
        }

        /// <summary>
        /// Проверяет обработку несложного пароля в ValidationCommand.
        /// Что делаем: Создаем пользователя с паролем без сложности и вызываем ValidationCommand.
        /// Что ожидаем: Метод выбрасывает ArgumentException с сообщением ComplexPasswordRequired.
        /// Зачем нужен: Проверяем валидацию сложного пароля в ValidationCommand.
        /// </summary>
        [Fact]
        public async Task ValidationCommand_NonComplexPassword_ThrowsArgumentException()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "SimplePassword",
                Role = new Role { Id = 1 }
            };
            _userDalMock.Setup(d => d.ExistsAsync(user.Email)).ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.ValidationCommand(user));
            Assert.Equal(ErrorMessages.ComplexPasswordRequired + " (Parameter 'password')", exception.Message);
        }

        /// <summary>
        /// Проверяет обработку существующего email в ValidationCommand.
        /// Что делаем: Создаем пользователя, мок указывает, что email уже существует.
        /// Что ожидаем: Метод выбрасывает InvalidOperationException с сообщением EmailAlreadyExists.
        /// Зачем нужен: Проверяем, что ValidationCommand не допускает дубликаты email.
        /// </summary>
        [Fact]
        public async Task ValidationCommand_EmailExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "ComplexPass123!",
                Role = new Role { Id = 1 }
            };
            _userDalMock.Setup(d => d.ExistsAsync(user.Email)).ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _userBL.ValidationCommand(user));
            Assert.Equal(ErrorMessages.EmailAlreadyExists, exception.Message);
            _userDalMock.Verify(d => d.ExistsAsync(user.Email), Times.Once());
        }

        /// <summary>
        /// Проверяет успешную валидацию в ValidationCommand.
        /// Что делаем: Создаем валидного пользователя и вызываем ValidationCommand.
        /// Что ожидаем: Метод не выбрасывает исключений.
        /// Зачем нужен: Убеждаемся, что ValidationCommand пропускает валидных пользователей.
        /// </summary>
        [Fact]
        public async Task ValidationCommand_ValidUser_Passes()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "ComplexPass123!",
                Role = new Role { Id = 1 }
            };
            _userDalMock.Setup(d => d.ExistsAsync(user.Email)).ReturnsAsync(false);

            // Act
            await _userBL.ValidationCommand(user);

            // Assert
            _userDalMock.Verify(d => d.ExistsAsync(user.Email), Times.Once());
        }

        #endregion

        #region Other Tests (Unchanged)

        /// <summary>
        /// Проверяет проверку существования пользователя по ID.
        /// </summary>
        [Fact]
        public async Task ExistsAsync_ById_ReturnsTrue()
        {
            // Arrange
            const long id = 1;
            _userDalMock.Setup(d => d.ExistsAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _userBL.ExistsAsync(id);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Проверяет получение пользователя по ID с включением роли.
        /// </summary>
        [Fact]
        public async Task GetAsync_ByIdWithRole_ReturnsUser()
        {
            // Arrange
            const long id = 1;
            var user = new User { Id = id, Email = "test@example.com", Role = new Role { Id = 2 } };
            _userDalMock.Setup(d => d.GetAsync(id, true))
                .ReturnsAsync(user);

            // Act
            var result = await _userBL.GetAsync(id, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.NotNull(result.Role);
        }

        /// <summary>
        /// Проверяет удаление пользователя по ID.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ValidId_ReturnsTrue()
        {
            // Arrange
            const long id = 1;
            _userDalMock.Setup(d => d.DeleteAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _userBL.DeleteAsync(id);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Проверяет получение списка пользователей с параметрами поиска и ролью.
        /// </summary>
        [Fact]
        public async Task GetAsync_WithSearchParams_ReturnsSearchResult()
        {
            // Arrange
            var searchParams = new UserSearchParams { Email = "test@example.com" };
            var user = new User { Id = 1, Email = "test@example.com", Role = new Role { Id = 2 } };
            var searchResult = new SearchResult<User> { Objects = new[] { user }, Total = 1 };
            _userDalMock.Setup(d => d.GetAsync(It.Is<UserSearchParams>(p => p.Email == searchParams.Email), true))
                .ReturnsAsync(searchResult);

            // Act
            var result = await _userBL.GetAsync(searchParams, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
            Assert.Single(result.Objects);
            Assert.Equal(user.Email, result.Objects.First().Email);
            Assert.NotNull(result.Objects.First().Role);
        }
        #endregion
    }
}