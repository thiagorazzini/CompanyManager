using CompanyManager.Application.Commands;
using CompanyManager.Application.Handlers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CompanyManager.Application.Handlers
{
    /// <summary>
    /// Tests for LogoutCommandHandler - JWT-based authentication
    /// </summary>
    public sealed class LogoutCommandHandlerTest
    {
        private readonly Mock<ILogger<LogoutCommandHandler>> _loggerMock;
        private readonly LogoutCommandHandler _handler;

        public LogoutCommandHandlerTest()
        {
            _loggerMock = new Mock<ILogger<LogoutCommandHandler>>();
            _handler = new LogoutCommandHandler(_loggerMock.Object);
        }

        [Fact(DisplayName = "Should handle logout successfully")]
        public async Task Should_Handle_Logout_Successfully()
        {
            // Arrange
            var command = new LogoutCommand();

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User logged out successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact(DisplayName = "Should handle cancellation token")]
        public async Task Should_Handle_Cancellation_Token()
        {
            // Arrange
            var command = new LogoutCommand();
            var cancellationToken = new CancellationToken(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<OperationCanceledException>(
                () => _handler.Handle(command, cancellationToken));

            exception.Should().NotBeNull();
        }
    }
}
