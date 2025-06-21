using Bl;
using Common.Search;
using Dal.Interfaces;
using Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Extensions;

namespace Tests.BL;

public class ProjectBLTests
{
    private readonly Mock<IProjectDal> _projectDalMock;
    private readonly Mock<ILogger<ProjectBL>> _loggerMock;
    private readonly ProjectBL _projectBL;

    public ProjectBLTests()
    {
        _projectDalMock = new Mock<IProjectDal>();
        _loggerMock = new Mock<ILogger<ProjectBL>>();
        _projectBL = new ProjectBL(_projectDalMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task AddOrUpdateAsync_ValidProject_ReturnsId()
    {
        // Arrange
        var project = new Project
        {
            Id = 0,
            Name = "Test Project",
            ClientId = 1,
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
        _loggerMock.VerifyLog(LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task AddOrUpdateAsync_NullProject_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _projectBL.AddOrUpdateAsync(null));
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task AddOrUpdateAsync_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var project = new Project { Name = "", ClientId = 1 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _projectBL.AddOrUpdateAsync(project));
        Assert.Equal(ErrorMessages.ProjectNameRequired + " (Parameter 'Name')", exception.Message);
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task AddOrUpdateAsync_NoClient_ThrowsArgumentException()
    {
        // Arrange
        var project = new Project { Name = "Test Project", ClientId = null };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _projectBL.AddOrUpdateAsync(project));
        Assert.Equal(ErrorMessages.ClientIdNotSpecified + " (Parameter 'ClientId')", exception.Message);
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task AddOrUpdateAsync_ProjectExists_SkipsDuplicateCheckAndReturnsId()
    {
        // Arrange
        var project = new Project
        {
            Id = 0,
            Name = "Test Project",
            ClientId = 1
        };

        _projectDalMock
            .Setup(d => d.ExistsAsync(project.Id))
            .ReturnsAsync(false);

        _projectDalMock
            .Setup(d => d.AddOrUpdateAsync(project))
            .ReturnsAsync(1L);

        // Act
        var result = await _projectBL.AddOrUpdateAsync(project);

        // Assert
        Assert.Equal(1L, result);
        _loggerMock.VerifyLog(LogLevel.Information, Times.Once());
    }

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
        _loggerMock.VerifyLog(LogLevel.Debug, Times.AtLeastOnce());
    }

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
        _loggerMock.VerifyLog(LogLevel.Debug, Times.AtLeastOnce());
    }

    [Fact]
    public async Task GetAsync_ByIdWithRelated_ReturnsProject()
    {
        // Arrange
        const long id = 1;
        var project = new Project { Id = id, Name = "Test Project", ClientId = 1 };
        _projectDalMock.Setup(d => d.GetAsync(id, true))
            .ReturnsAsync(project);

        // Act
        var result = await _projectBL.GetAsync(id, true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.NotNull(result.ClientId);
        _loggerMock.VerifyLog(LogLevel.Debug, Times.AtLeastOnce());
    }

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
        _loggerMock.VerifyLog(LogLevel.Information, Times.AtLeastOnce());
    }

    [Fact]
    public async Task GetAsync_WithSearchParams_ReturnsSearchResult()
    {
        // Arrange
        var searchParams = new ProjectSearchParams { Name = "Test", UserId = 1 };
        var project = new Project { Id = 1, Name = "Test Project", ClientId = 1 };
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
        Assert.NotNull(result.Objects.First().ClientId);
        _loggerMock.VerifyLog(LogLevel.Debug, Times.AtLeastOnce());
    }

    [Fact]
    public async Task GetAsync_NullSearchParams_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _projectBL.GetAsync(null, true));
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }

    [Fact]
    public async Task GetAsync_NoUserIdInParams_ThrowsArgumentException()
    {
        // Arrange
        var searchParams = new ProjectSearchParams { Name = "Test" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _projectBL.GetAsync(searchParams, true));
        Assert.Equal(ErrorMessages.UserIdRequired + " (Parameter 'UserId')", exception.Message);
        _loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
    }
}