using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Extensions;

public static class LoggerMockExtensions
{
    /// <summary>
    /// Проверяет, был ли вызван логгер с определенным уровнем.
    /// </summary>
    public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel level, Times times)
    {
        loggerMock.Verify(x => x.Log(
            level,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            times);
    }
}
