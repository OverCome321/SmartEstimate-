using Bl;
using Common.Search;
using Dal.Interfaces;
using Entities;
using Moq;

namespace Tests.BL
{
    public class ProjectBLTests
    {
        private readonly Mock<IProjectDal> _projectDalMock;
        private readonly ProjectBL _projectBL;

        public ProjectBLTests()
        {
            _projectDalMock = new Mock<IProjectDal>();
            _projectBL = new ProjectBL(_projectDalMock.Object);
        }

        /// <summary>
        /// Проверяет успешное добавление или обновление валидного проекта.
        /// Что делаем: Создаем валидный проект, настраиваем мок для отсутствия дубликата и возврата ID.
        /// Что ожидаем: Метод возвращает ID от DAL, метод AddOrUpdateAsync вызывается один раз.
        /// Зачем нужен: Убеждаемся, что метод корректно сохраняет проект и возвращает его идентификатор.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_ValidProject_ReturnsId()
        {
            // Arrange
            var project = new Project
            {
                Id = 0,
                Name = "Test Project",
                Client = new Client { Id = 1 },
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            const long expectedId = 1;
            _projectDalMock.Setup(d => d.ExistsAsync(project.Id)).ReturnsAsync(false);
            _projectDalMock.Setup(d => d.AddOrUpdateAsync(It.IsAny<Project>())).ReturnsAsync(expectedId);

            // Act
            var result = await _projectBL.AddOrUpdateAsync(project);

            // Assert
            Assert.Equal(expectedId, result);
            _projectDalMock.Verify(d => d.AddOrUpdateAsync(It.IsAny<Project>()), Times.Once());
        }

        /// <summary>
        /// Проверяет обработку случая, когда передан null вместо проекта.
        /// Что делаем: Вызываем AddOrUpdateAsync с null.
        /// Что ожидаем: Метод выбрасывает ArgumentNullException.
        /// Зачем нужен: Убеждаемся, что метод корректно обрабатывает некорректный входной параметр.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_NullProject_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _projectBL.AddOrUpdateAsync(null));
        }

        /// <summary>
        /// Проверяет обработку пустого названия проекта.
        /// Что делаем: Создаем проект с пустым названием и вызываем AddOrUpdateAsync.
        /// Что ожидаем: Метод выбрасывает ArgumentException с сообщением ProjectNameRequired.
        /// Зачем нужен: Проверяем валидацию названия проекта.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_EmptyName_ThrowsArgumentException()
        {
            // Arrange
            var project = new Project { Name = "", Client = new Client { Id = 1 } };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _projectBL.AddOrUpdateAsync(project));
            Assert.Equal(ErrorMessages.ProjectNameRequired + " (Parameter 'Name')", exception.Message);
        }

        /// <summary>
        /// Проверяет обработку отсутствия клиента у проекта.
        /// Что делаем: Создаем проект без клиента и вызываем AddOrUpdateAsync.
        /// Что ожидаем: Метод выбрасывает ArgumentException с сообщением ClientIdNotSpecified.
        /// Зачем нужен: Убеждаемся, что проект нельзя создать без привязки к клиенту.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_NoClient_ThrowsArgumentException()
        {
            // Arrange
            var project = new Project { Name = "Test Project", Client = null };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _projectBL.AddOrUpdateAsync(project));
            Assert.Equal(ErrorMessages.ClientIdNotSpecified + " (Parameter 'Client')", exception.Message);
        }

        /// <summary>
        /// Проверяет обработку добавления или обновления проекта, даже если проект с таким названием уже существует у клиента.
        /// Что делаем: Создаем проект, мок указывает, что проект с таким названием уже существует у клиента.
        /// Что ожидаем: Метод успешно выполняется и возвращает идентификатор проекта.
        /// Зачем нужен: Проверяем, что метод корректно обрабатывает добавление или обновление проекта.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_ProjectExists_ReturnsProjectId()
        {
            // Arrange
            var project = new Project
            {
                Id = 0,
                Name = "Test Project",
                Client = new Client { Id = 1, User = new User { Id = 1 } }
            };

            _projectDalMock
                .Setup(d => d.ExistsAsync(project.Name, project.Client.Id, project.Client.User.Id))
                .ReturnsAsync(true);

            _projectDalMock
                .Setup(d => d.AddOrUpdateAsync(project))
                .ReturnsAsync(1L);

            // Act
            var result = await _projectBL.AddOrUpdateAsync(project);

            // Assert
            Assert.Equal(1L, result);
        }
        /// <summary>
        /// Проверяет проверку существования проекта по ID.
        /// Что делаем: Настраиваем мок для возврата true, вызываем ExistsAsync.
        /// Что ожидаем: Метод возвращает true.
        /// Зачем нужен: Убеждаемся, что метод корректно проверяет наличие проекта по ID.
        /// </summary>
        [Fact]
        public async Task ExistsAsync_ById_ReturnsTrue()
        {
            // Arrange
            const long id = 1;
            _projectDalMock.Setup(d => d.ExistsAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _projectBL.ExistsAsync(id);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Проверяет проверку существования проекта по имени, ID клиента и ID пользователя.
        /// Что делаем: Настраиваем мок для возврата true, вызываем ExistsAsync с параметрами.
        /// Что ожидаем: Метод возвращает true.
        /// Зачем нужен: Убеждаемся, что метод корректно проверяет наличие проекта по комплексным параметрам.
        /// </summary>
        [Fact]
        public async Task ExistsAsync_ByNameClientAndUser_ReturnsTrue()
        {
            // Arrange
            const string name = "Test Project";
            const long clientId = 1;
            const long userId = 1;
            _projectDalMock.Setup(d => d.ExistsAsync(name, clientId, userId)).ReturnsAsync(true);

            // Act
            var result = await _projectBL.ExistsAsync(name, clientId, userId);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Проверяет получение проекта по ID с включением связанных данных.
        /// Что делаем: Создаем проект, настраиваем мок для возврата проекта с клиентом, вызываем GetAsync с includeRelated=true.
        /// Что ожидаем: Метод возвращает проект с указанным ID и непустым клиентом.
        /// Зачем нужен: Проверяем корректное получение проекта с связанными данными.
        /// </summary>
        [Fact]
        public async Task GetAsync_ByIdWithRelated_ReturnsProject()
        {
            // Arrange
            const long id = 1;
            var project = new Project { Id = id, Name = "Test Project", Client = new Client { Id = 1 } };
            _projectDalMock.Setup(d => d.GetAsync(id, true))
                .ReturnsAsync(project);

            // Act
            var result = await _projectBL.GetAsync(id, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.NotNull(result.Client);
        }

        /// <summary>
        /// Проверяет удаление проекта по ID.
        /// Что делаем: Настраиваем мок для возврата true при удалении, вызываем DeleteAsync.
        /// Что ожидаем: Метод возвращает true.
        /// Зачем нужен: Убеждаемся, что метод корректно удаляет проект.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ValidId_ReturnsTrue()
        {
            // Arrange
            const long id = 1;
            _projectDalMock.Setup(d => d.DeleteAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _projectBL.DeleteAsync(id);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Проверяет получение списка проектов с параметрами поиска и связанными данными.
        /// Что делаем: Создаем параметры поиска, настраиваем мок для возврата результата, вызываем GetAsync с includeRelated=true.
        /// Что ожидаем: Метод возвращает SearchResult с одним проектом и непустым клиентом.
        /// Зачем нужен: Проверяем корректный поиск проектов с учетом параметров и связанных данных.
        /// </summary>
        [Fact]
        public async Task GetAsync_WithSearchParams_ReturnsSearchResult()
        {
            // Arrange
            var searchParams = new ProjectSearchParams { Name = "Test", UserId = 1 };
            var project = new Project { Id = 1, Name = "Test Project", Client = new Client { Id = 1 } };
            var searchResult = new SearchResult<Project> { Objects = new[] { project }, Total = 1 };
            _projectDalMock.Setup(d => d.GetAsync(It.Is<ProjectSearchParams>(p => p.Name == searchParams.Name && p.UserId == searchParams.UserId), true))
                .ReturnsAsync(searchResult);

            // Act
            var result = await _projectBL.GetAsync(searchParams, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
            Assert.Single(result.Objects);
            Assert.Equal(project.Name, result.Objects.First().Name);
            Assert.NotNull(result.Objects.First().Client);
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
            await Assert.ThrowsAsync<ArgumentNullException>(() => _projectBL.GetAsync(null, true));
        }

        /// <summary>
        /// Проверяет обработку отсутствия UserId в параметрах поиска.
        /// Что делаем: Создаем параметры поиска без UserId, вызываем GetAsync.
        /// Что ожидаем: Метод выбрасывает ArgumentException с сообщением UserIdRequired.
        /// Зачем нужен: Убеждаемся, что поиск проектов требует указания UserId.
        /// </summary>
        [Fact]
        public async Task GetAsync_NoUserIdInParams_ThrowsArgumentException()
        {
            // Arrange
            var searchParams = new ProjectSearchParams { Name = "Test" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _projectBL.GetAsync(searchParams, true));
            Assert.Equal(ErrorMessages.UserIdRequired + " (Parameter 'UserId')", exception.Message);
        }
    }
}