using Bl;
using Bl.DI;
using Common.Convert;
using Common.Search;
using Common.Security;
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

        /// <summary>
        /// Проверяет успешное добавление или обновление валидного пользователя.
        /// Что делаем: Создаем валидного пользователя, настраиваем мок для проверки отсутствия email и возврата Guid.
        /// Что ожидаем: Метод возвращает Guid, переданный DAL, и метод AddOrUpdateAsync вызывается один раз.
        /// Зачем нужен тест: Убеждаемся, что метод корректно сохраняет нового пользователя и возвращает его идентификатор.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_ValidUser_ReturnsGuid()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.Empty,
                Email = "test@example.com",
                PasswordHash = "ComplexPass123!",
                Role = new Role { Id = Guid.NewGuid() }
            };
            var expectedGuid = Guid.NewGuid();
            _userDalMock.Setup(d => d.ExistsAsync(user.Email)).ReturnsAsync(false);
            _userDalMock.Setup(d => d.AddOrUpdateAsync(It.IsAny<User>())).ReturnsAsync(expectedGuid);

            // Act
            var result = await _userBL.AddOrUpdateAsync(user);

            // Assert
            Assert.Equal(expectedGuid, result);
            _userDalMock.Verify(d => d.AddOrUpdateAsync(It.IsAny<User>()), Times.Once());
        }

        /// <summary>
        /// Проверяет обработку случая, когда передан null вместо пользователя.
        /// Что делаем: Вызываем метод AddOrUpdateAsync с null в качестве аргумента.
        /// Что ожидаем: Метод выбрасывает ArgumentNullException.
        /// Зачем нужен тест: Убеждаемся, что метод корректно обрабатывает некорректный входной параметр.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_NullUser_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userBL.AddOrUpdateAsync(null));
        }

        /// <summary>
        /// Проверяет обработку случая, когда email пользователя пустой.
        /// Что делаем: Создаем пользователя с пустым email и вызываем AddOrUpdateAsync.
        /// Что ожидаем: Метод выбрасывает ArgumentException.
        /// Зачем нужен тест: Проверяем валидацию email в методе, чтобы убедиться, что пустой email не допускается.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_EmptyEmail_ThrowsArgumentException()
        {
            // Arrange
            var user = new User { Email = "", PasswordHash = "ComplexPass123!", Role = new Role { Id = Guid.NewGuid() } };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _userBL.AddOrUpdateAsync(user));
        }

        /// <summary>
        /// Проверяет обработку случая, когда email пользователя имеет неверный формат.
        /// Что делаем: Создаем пользователя с невалидным email и вызываем AddOrUpdateAsync.
        /// Что ожидаем: Метод выбрасывает ArgumentException.
        /// Зачем нужен тест: Убеждаемся, что метод проверяет формат email и отклоняет невалидные значения.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_InvalidEmail_ThrowsArgumentException()
        {
            // Arrange
            var user = new User
            {
                Email = "invalid-email",
                PasswordHash = "ComplexPass123!",
                Role = new Role { Id = Guid.NewGuid() }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _userBL.AddOrUpdateAsync(user));
        }

        /// <summary>
        /// Проверяет обработку случая, когда email уже существует в системе.
        /// Что делаем: Создаем пользователя и настраиваем мок, чтобы он указывал на существование email.
        /// Что ожидаем: Метод выбрасывает InvalidOperationException.
        /// Зачем нужен тест: Проверяем, что метод не позволяет создавать дубликаты пользователей с одинаковым email.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_EmailExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "ComplexPass123!",
                Role = new Role { Id = Guid.NewGuid() }
            };
            _userDalMock.Setup(d => d.ExistsAsync(user.Email)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _userBL.AddOrUpdateAsync(user));
        }

        /// <summary>
        /// Проверяет обработку случая, когда пароль пользователя слишком слабый.
        /// Что делаем: Создаем пользователя с коротким паролем и вызываем AddOrUpdateAsync.
        /// Что ожидаем: Метод выбрасывает ArgumentException.
        /// Зачем нужен тест: Убеждаемся, что метод проверяет сложность пароля согласно настройкам.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_WeakPassword_ThrowsArgumentException()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "weak",
                Role = new Role { Id = Guid.NewGuid() }
            };
            _userDalMock.Setup(d => d.ExistsAsync(user.Email)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _userBL.AddOrUpdateAsync(user));
        }

        /// <summary>
        /// Проверяет успешную верификацию пользователя с правильными учетными данными.
        /// Что делаем: Создаем пользователя, настраиваем мок для возврата результата поиска по email, вызываем VerifyPasswordAsync.
        /// Что ожидаем: Метод возвращает объект пользователя с указанным email.
        /// Зачем нужен тест: Проверяем, что метод корректно верифицирует пользователя при правильном пароле.
        /// </summary>
        [Fact]
        public async Task VerifyPasswordAsync_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var email = "test@example.com";
            var password = "ComplexPass123!";
            var hashedPassword = PasswordHasher.HashPassword(password);
            var user = new User { Email = email, PasswordHash = hashedPassword };
            var searchResult = new SearchResult<User> { Objects = new[] { user }, Total = 1 };
            var searchParams = new UserSearchParams(email);
            _userDalMock.Setup(d => d.GetAsync(It.Is<UserSearchParams>(p => p.Email == email), null))
                .ReturnsAsync(searchResult);

            // Act
            var result = await _userBL.VerifyPasswordAsync(email, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }

        /// <summary>
        /// Проверяет обработку неверного пароля при верификации пользователя.
        /// Что делаем: Создаем пользователя, настраиваем мок для возврата результата поиска, вызываем VerifyPasswordAsync с неверным паролем.
        /// Что ожидаем: Метод возвращает null.
        /// Зачем нужен тест: Убеждаемся, что метод корректно отклоняет неверные учетные данные.
        /// </summary>
        [Fact]
        public async Task VerifyPasswordAsync_InvalidPassword_ReturnsNull()
        {
            // Arrange
            var email = "test@example.com";
            var password = "ComplexPass123!";
            var wrongPassword = "WrongPass123!";
            var hashedPassword = PasswordHasher.HashPassword(password);
            var user = new User { Email = email, PasswordHash = hashedPassword };
            var searchResult = new SearchResult<User> { Objects = new[] { user }, Total = 1 };
            var searchParams = new UserSearchParams(email);
            _userDalMock.Setup(d => d.GetAsync(It.Is<UserSearchParams>(p => p.Email == email), null))
                .ReturnsAsync(searchResult);

            // Act
            var result = await _userBL.VerifyPasswordAsync(email, wrongPassword);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Проверяет проверку существования пользователя по идентификатору.
        /// Что делаем: Настраиваем мок для возврата true при проверке существования пользователя по ID, вызываем ExistsAsync.
        /// Что ожидаем: Метод возвращает true.
        /// Зачем нужен тест: Убеждаемся, что метод корректно проверяет наличие пользователя по идентификатору.
        /// </summary>
        [Fact]
        public async Task ExistsAsync_ById_ReturnsTrue()
        {
            // Arrange
            var id = Guid.NewGuid();
            _userDalMock.Setup(d => d.ExistsAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _userBL.ExistsAsync(id);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Проверяет получение пользователя по идентификатору с включением роли.
        /// Что делаем: Создаем пользователя, настраиваем мок для возврата пользователя с ролью, вызываем GetAsync с includeRole=true.
        /// Что ожидаем: Метод возвращает объект пользователя с указанным ID и непустой ролью.
        /// Зачем нужен тест: Проверяем, что метод корректно получает пользователя с данными о роли.
        /// </summary>
        [Fact]
        public async Task GetAsync_ByIdWithRole_ReturnsUser()
        {
            // Arrange
            var id = Guid.NewGuid();
            var user = new User { Id = id, Email = "test@example.com", Role = new Role { Id = Guid.NewGuid() } };
            var convertParams = new UserConvertParams { IncludeRole = true };
            _userDalMock.Setup(d => d.GetAsync(id, It.Is<UserConvertParams>(p => p.IncludeRole == true)))
                .ReturnsAsync(user);

            // Act
            var result = await _userBL.GetAsync(id, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.NotNull(result.Role);
        }

        /// <summary>
        /// Проверяет удаление пользователя по идентификатору.
        /// Что делаем: Настраиваем мок для возврата true при удалении, вызываем DeleteAsync.
        /// Что ожидаем: Метод возвращает true.
        /// Зачем нужен тест: Убеждаемся, что метод корректно удаляет пользователя.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ValidId_ReturnsTrue()
        {
            // Arrange
            var id = Guid.NewGuid();
            _userDalMock.Setup(d => d.DeleteAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _userBL.DeleteAsync(id);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Проверяет получение списка пользователей с параметрами поиска и включением роли.
        /// Что делаем: Создаем параметры поиска, настраиваем мок для возврата результата поиска, вызываем GetAsync с includeRole=true.
        /// Что ожидаем: Метод возвращает SearchResult с одним пользователем, соответствующим параметрам, и непустой ролью.
        /// Зачем нужен тест: Проверяем, что метод корректно выполняет поиск пользователей с учетом параметров и ролей.
        /// </summary>
        [Fact]
        public async Task GetAsync_WithSearchParams_ReturnsSearchResult()
        {
            // Arrange
            var searchParams = new UserSearchParams { Email = "test@example.com" };
            var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", Role = new Role { Id = Guid.NewGuid() } };
            var searchResult = new SearchResult<User> { Objects = new[] { user }, Total = 1 };
            var convertParams = new UserConvertParams { IncludeRole = true };
            _userDalMock.Setup(d => d.GetAsync(It.Is<UserSearchParams>(p => p.Email == searchParams.Email), It.Is<UserConvertParams>(p => p.IncludeRole == true)))
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
    }
}