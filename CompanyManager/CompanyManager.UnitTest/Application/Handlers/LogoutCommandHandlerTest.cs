using CompanyManager.Application.Abstractions;
using CompanyManager.Application.Commands;
using CompanyManager.Application.Handlers;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.Interfaces;
using CompanyManager.UnitTest.Application.TestDouble;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CompanyManager.Application.Handlers
{
    public sealed class LogoutCommandHandlerTest
    {
        private readonly Mock<ILogger<LogoutCommandHandler>> _loggerMock;
        private readonly InMemoryUserAccountRepository _userRepository;
        private readonly LogoutCommandHandler _handler;

        public LogoutCommandHandlerTest()
        {
            _loggerMock = new Mock<ILogger<LogoutCommandHandler>>();
            _userRepository = new InMemoryUserAccountRepository();
            _handler = new LogoutCommandHandler(_userRepository, _loggerMock.Object);
        }

        [Fact(DisplayName = "Should invalidate user session successfully")]
        public async Task Should_Invalidate_User_Session_Successfully()
        {
            // Arrange
            var email = "john.doe@company.com";
            var command = new LogoutCommand { RefreshToken = email };
            
            // Create a user account in the repository
            var userAccount = UserAccount.Create(email, "hashed-password", Guid.NewGuid());
            await _userRepository.AddAsync(userAccount, CancellationToken.None);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            // Verify that the user session was invalidated
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User session invalidated")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact(DisplayName = "Should handle null refresh token gracefully")]
        public async Task Should_Handle_Null_Refresh_Token_Gracefully()
        {
            // Arrange
            var command = new LogoutCommand { RefreshToken = null };

            // Act & Assert
            await _handler.Handle(command, CancellationToken.None);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No refresh token provided")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact(DisplayName = "Should handle empty refresh token gracefully")]
        public async Task Should_Handle_Empty_Refresh_Token_Gracefully()
        {
            // Arrange
            var command = new LogoutCommand { RefreshToken = string.Empty };

            // Act & Assert
            await _handler.Handle(command, CancellationToken.None);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No refresh token provided")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact(DisplayName = "Should handle whitespace refresh token gracefully")]
        public async Task Should_Handle_Whitespace_Refresh_Token_Gracefully()
        {
            // Arrange
            var command = new LogoutCommand { RefreshToken = "   " };

            // Act & Assert
            await _handler.Handle(command, CancellationToken.None);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No refresh token provided")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact(DisplayName = "Should log successful logout operation")]
        public async Task Should_Log_Successful_Logout_Operation()
        {
            // Arrange
            var email = "test@company.com";
            var command = new LogoutCommand { RefreshToken = email };

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
    }
}
