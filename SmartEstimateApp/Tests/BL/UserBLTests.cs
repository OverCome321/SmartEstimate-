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

        /// <summary>
        /// Проверяет успешное добавление или обновление валидного пользователя.
        /// Что делаем: Создаем валидного пользователя, настраиваем мок для отсутствия email и возврата ID.
        /// Что ожидаем: Метод возвращает ID от DAL, метод AddOrUpdateAsync вызывается один раз.
        /// Зачем нужен: Убеждаемся, что метод корректно сохраняет пользователя и возвращает его идентификатор.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_ValidUser_ReturnsId()
        {
            // Arrange
            var user = new User
            {
                Id = 0,
                Email = "test@example.com",
                PasswordHash = "ComplexPass123!",
                Role = new Role { Id = 1 }
            };
            const long expectedId = 1;
            _userDalMock.Setup(d => d.ExistsAsync(user.Email)).ReturnsAsync(false);
            _userDalMock.Setup(d => d.AddOrUpdateAsync(It.IsAny<User>())).ReturnsAsync(expectedId);

            // Act
            var result = await _userBL.AddOrUpdateAsync(user);

            // Assert
            Assert.Equal(expectedId, result);
            _userDalMock.Verify(d => d.AddOrUpdateAsync(It.IsAny<User>()), Times.Once());
        }

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
        /// Проверяет обработку пустого email.
        /// Что делаем: Создаем пользователя с пустым email и вызываем AddOrUpdateAsync.
        /// Что ожидаем: Метод выбрасывает ArgumentException с сообщением EmailEmpty.
        /// Зачем нужен: Проверяем валидацию email, чтобы убедиться, что пустой email не допускается.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_EmptyEmail_ThrowsArgumentException()
        {
            // Arrange
            var user = new User { Email = "", PasswordHash = "ComplexPass123!", Role = new Role { Id = 1 } };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.AddOrUpdateAsync(user));
            Assert.Equal(ErrorMessages.EmailEmpty + " (Parameter 'Email')", exception.Message);
        }

        /// <summary>
        /// Проверяет обработку неверного формата email.
        /// Что делаем: Создаем пользователя с невалидным email и вызываем AddOrUpdateAsync.
        /// Что ожидаем: Метод выбрасывает ArgumentException с сообщением EmailInvalidFormat.
        /// Зачем нужен: Убеждаемся, что метод проверяет формат email и отклоняет невалидные значения.
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
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.AddOrUpdateAsync(user));
            Assert.Equal(ErrorMessages.EmailInvalidFormat + " (Parameter 'Email')", exception.Message);
        }

        /// <summary>
        /// Проверяет обработку существующего email.
        /// Что делаем: Создаем пользователя, мок указывает, что email уже существует.
        /// Что ожидаем: Метод выбрасывает InvalidOperationException с сообщением EmailAlreadyExists.
        /// Зачем нужен: Проверяем, что метод не допускает дубликаты email.
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
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _userBL.AddOrUpdateAsync(user));
            Assert.Equal(ErrorMessages.EmailAlreadyExists, exception.Message);
        }

        /// <summary>
        /// Проверяет обработку слабого пароля.
        /// Что делаем: Создаем пользователя с коротким паролем и вызываем AddOrUpdateAsync.
        /// Что ожидаем: Метод выбрасывает ArgumentException с сообщением PasswordTooShort.
        /// Зачем нужен: Убеждаемся, что метод проверяет сложность пароля.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_WeakPassword_ThrowsArgumentException()
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
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.AddOrUpdateAsync(user));
            Assert.Equal(string.Format(ErrorMessages.PasswordTooShort + " (Parameter 'password')", _defaultOptions.MinPasswordLength), exception.Message);
        }

        /// <summary>
        /// Проверяет обработку несложного пароля при требовании сложности.
        /// Что делаем: Создаем пользователя с паролем без сложности и вызываем AddOrUpdateAsync.
        /// Что ожидаем: Метод выбрасывает ArgumentException с сообщением ComplexPasswordRequired.
        /// Зачем нужен: Проверяем валидацию сложного пароля.
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
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userBL.AddOrUpdateAsync(user));
            Assert.Equal(ErrorMessages.ComplexPasswordRequired + " (Parameter 'password')", exception.Message);
        }



        /// <summary>
        /// Проверяет проверку существования пользователя по ID.
        /// Что делаем: Настраиваем мок для возврата true, вызываем ExistsAsync.
        /// Что ожидаем: Метод возвращает true.
        /// Зачем нужен: Убеждаемся, что метод корректно проверяет наличие пользователя по ID.
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
        /// Что делаем: Создаем пользователя, настраиваем мок для возврата пользователя с ролью, вызываем GetAsync с includeRole=true.
        /// Что ожидаем: Метод возвращает пользователя с указанным ID и непустой ролью.
        /// Зачем нужен: Проверяем корректное получение пользователя с данными роли.
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
        /// Что делаем: Настраиваем мок для возврата true при удалении, вызываем DeleteAsync.
        /// Что ожидаем: Метод возвращает true.
        /// Зачем нужен: Убеждаемся, что метод корректно удаляет пользователя.
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
        /// Что делаем: Создаем параметры поиска, настраиваем мок для возврата результата, вызываем GetAsync с includeRole=true.
        /// Что ожидаем: Метод возвращает SearchResult с одним пользователем и непустой ролью.
        /// Зачем нужен: Проверяем корректный поиск пользователей с учетом параметров и ролей.
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

        /// <summary>
        /// Проверяет обработку null параметров поиска.
        /// Что делаем: Вызываем GetAsync с null параметрами поиска.
        /// Что ожидаем: Метод выбрасывает ArgumentNullException.
        /// Зачем нужен: Убеждаемся в корректной валидации параметров поиска.
        /// </summary>
        [Fact]
        public async Task GetAsync_NullSearchParams_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userBL.GetAsync(null, true));
        }
    }
}