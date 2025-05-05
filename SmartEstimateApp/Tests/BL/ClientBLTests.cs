using Bl;
using Common.Search;
using Dal.Interfaces;
using Entities;
using Moq;

namespace Tests.BL
{
    public class ClientBLTests
    {
        private readonly Mock<IClientDal> _clientDalMock;
        private readonly ClientBL _clientBL;

        public ClientBLTests()
        {
            _clientDalMock = new Mock<IClientDal>();
            _clientBL = new ClientBL(_clientDalMock.Object);
        }

        /// <summary>
        /// Проверяет успешное добавление или обновление валидного клиента.
        /// Что делаем: Создаем валидного клиента с объектом User, настраиваем мок для отсутствия email и телефона, возврата ID.
        /// Что ожидаем: Метод возвращает ID от DAL, метод AddOrUpdateAsync вызывается один раз.
        /// Зачем нужен: Убеждаемся, что метод корректно сохраняет клиента и возвращает его идентификатор.
        /// </summary>
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
        }

        /// <summary>
        /// Проверяет обработку неверного формата email.
        /// Что делаем: Создаем клиента с невалидным email и вызываем AddOrUpdateAsync.
        /// Что ожидаем: Метод выбрасывает ArgumentException с сообщением о неверном формате email.
        /// Зачем нужен: Убеждаемся, что метод проверяет формат email и отклоняет невалидные значения.
        /// </summary>
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
        }

        /// <summary>
        /// Проверяет обработку неверного формата телефона.
        /// Что делаем: Создаем клиента с невалидным телефоном и вызываем AddOrUpdateAsync.
        /// Что ожидаем: Метод выбрасывает ArgumentException с сообщением о неверном формате телефона.
        /// Зачем нужен: Убеждаемся, что метод проверяет формат телефона и отклоняет невалидные значения.
        /// </summary>
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
        }

        /// <summary>
        /// Проверяет обработку отсутствия User.
        /// Что делаем: Создаем клиента с Id = 0 и без User, вызываем AddOrUpdateAsync.
        /// Что ожидаем: Метод выбрасывает ArgumentException с сообщением о необходимости User.
        /// Зачем нужен: Убеждаемся, что метод проверяет наличие User.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_MissingUser_ThrowsArgumentException()
        {
            // Arrange
            var client = new Client
            {
                Id = 0,
                Email = "test@example.com",
                Phone = "+1234567890",
                User = null
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _clientBL.AddOrUpdateAsync(client));
            Assert.Equal($"{ErrorMessages.UserIdNotSpecified} (Parameter 'Id')", exception.Message);
        }

        /// <summary>
        /// Проверяет обработку существующего email.
        /// Что делаем: Создаем клиента, мок указывает, что email уже существует.
        /// Что ожидаем: Метод выбрасывает InvalidOperationException с сообщением о существующем email.
        /// Зачем нужен: Проверяем, что метод не допускает дубликаты email.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_EmailExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var client = new Client
            {
                Id = 1,
                Email = "test@example.com",
                Phone = "+1234567890",
                User = new User { Id = 1 }
            };
            _clientDalMock.Setup(d => d.ExistsAsync(client.Email, client.User.Id)).ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _clientBL.AddOrUpdateAsync(client));
            Assert.Equal(ErrorMessages.ClientEmailAlreadyExists, exception.Message);
        }

        /// <summary>
        /// Проверяет обработку существующего телефона.
        /// Что делаем: Создаем клиента, мок указывает, что телефон уже существует.
        /// Что ожидаем: Метод выбрасывает InvalidOperationException с сообщением о существующем телефоне.
        /// Зачем нужен: Проверяем, что метод не допускает дубликаты телефона.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_PhoneExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var client = new Client
            {
                Id = 1,
                Email = "test@example.com",
                Phone = "+1234567890",
                User = new User { Id = 1 }
            };
            _clientDalMock.Setup(d => d.ExistsAsync(client.Email, client.User.Id)).ReturnsAsync(false);
            _clientDalMock.Setup(d => d.ExistsPhoneAsync(client.Phone, client.User.Id)).ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _clientBL.AddOrUpdateAsync(client));
            Assert.Equal(ErrorMessages.ClientPhoneAlreadyExists, exception.Message);
        }

        /// <summary>
        /// Проверяет проверку существования клиента по ID.
        /// Что делаем: Настраиваем мок для возврата true, вызываем ExistsAsync.
        /// Что ожидаем: Метод возвращает true.
        /// Зачем нужен: Убеждаемся, что метод корректно проверяет наличие клиента по ID.
        /// </summary>
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
        }

        /// <summary>
        /// Проверяет проверку существования клиента по email и User.
        /// Что делаем: Настраиваем мок для возврата true, вызываем ExistsAsync с email и ID пользователя.
        /// Что ожидаем: Метод возвращает true.
        /// Зачем нужен: Убеждаемся, что метод корректно проверяет наличие клиента по email и ID пользователя.
        /// </summary>
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
        }

        /// <summary>
        /// Проверяет проверку существования клиента по телефону и User.
        /// Что делаем: Настраиваем мок для возврата true, вызываем ExistsPhoneAsync с телефоном и ID пользователя.
        /// Что ожидаем: Метод возвращает true.
        /// Зачем нужен: Убеждаемся, что метод корректно проверяет наличие клиента по телефону и ID пользователя.
        /// </summary>
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
        }

        /// <summary>
        /// Проверяет получение клиента по ID.
        /// Что делаем: Создаем клиента, настраиваем мок для возврата клиента, вызываем GetAsync.
        /// Что ожидаем: Метод возвращает клиента с указанным ID.
        /// Зачем нужен: Проверяем корректное получение клиента по ID.
        /// </summary>
        [Fact]
        public async Task GetAsync_ById_ReturnsClient()
        {
            // Arrange
            const long id = 1;
            var client = new Client { Id = id, Email = "test@example.com", User = new User { Id = 1 } };
            _clientDalMock.Setup(d => d.GetAsync(id, null)).ReturnsAsync(client);

            // Act
            var result = await _clientBL.GetAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.NotNull(result.User);
            Assert.Equal(1, result.User.Id);
        }

        /// <summary>
        /// Проверяет удаление клиента по ID.
        /// Что делаем: Настраиваем мок для возврата true при удалении, вызываем DeleteAsync.
        /// Что ожидаем: Метод возвращает true.
        /// Зачем нужен: Убеждаемся, что метод корректно удаляет клиента.
        /// </summary>
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
        }

        /// <summary>
        /// Проверяет получение списка клиентов с параметрами поиска.
        /// Что делаем: Создаем параметры поиска с ID пользователя, настраиваем мок для возврата результата, вызываем GetAsync.
        /// Что ожидаем: Метод возвращает SearchResult с одним клиентом.
        /// Зачем нужен: Проверяем корректный поиск клиентов с учетом параметров.
        /// </summary>
        [Fact]
        public async Task GetAsync_WithSearchParams_ReturnsSearchResult()
        {
            // Arrange
            var searchParams = new ClientSearchParams { Email = "test@example.com", UserId = 1 };
            var client = new Client { Id = 1, Email = "test@example.com", User = new User { Id = 1 } };
            var searchResult = new SearchResult<Client> { Objects = new[] { client }, Total = 1 };
            _clientDalMock.Setup(d => d.GetAsync(It.Is<ClientSearchParams>(p => p.Email == searchParams.Email && p.UserId == searchParams.UserId), null))
                .ReturnsAsync(searchResult);

            // Act
            var result = await _clientBL.GetAsync(searchParams);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
            Assert.Single(result.Objects);
            Assert.Equal(client.Email, result.Objects.First().Email);
            Assert.NotNull(result.Objects.First().User);
            Assert.Equal(1, result.Objects.First().User.Id);
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
            await Assert.ThrowsAsync<ArgumentNullException>(() => _clientBL.AddOrUpdateAsync(null));
        }

        /// <summary>
        /// Проверяет обработку отсутствия UserId в параметрах поиска.
        /// Что делаем: Создаем параметры поиска без UserId, вызываем GetAsync.
        /// Что ожидаем: Метод выбрасывает ArgumentException с сообщением о необходимости UserId.
        /// Зачем нужен: Убеждаемся, что метод проверяет наличие UserId в параметрах поиска.
        /// </summary>
        [Fact]
        public async Task GetAsync_MissingUserIdInSearchParams_ThrowsArgumentException()
        {
            // Arrange
            var searchParams = new ClientSearchParams { Email = "test@example.com" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _clientBL.GetAsync(searchParams));
            Assert.Equal($"{ErrorMessages.UserIdRequired} (Parameter 'UserId')", exception.Message);
        }
    }
}