using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Handlers;
using CompanyManager.Application.Services;
using CompanyManager.Application.Validators;
using CompanyManager.Domain.Entities;
using CompanyManager.UnitTest.Application.TestDouble;
using FluentAssertions;
using FluentValidation;

namespace CompanyManager.UnitTest.Application.Handlers
{
    public sealed class AuthenticateCommandHandlerTest
    {
        private static UserAccount NewUser(string loginEmail, string rawPassword)
        {
            var hasher = new PasswordHasher();  
            var hash = hasher.Hash(rawPassword);

            return UserAccount.Create(
                userName: loginEmail,
                passwordHash: hash,
                employeeId: Guid.NewGuid()
            );
        }

        private static IValidator<AuthenticateRequest> CreateValidator() => 
            new AuthenticateRequestValidator();

        [Fact(DisplayName = "Should authenticate with valid credentials and return token")]
        public async Task Should_Authenticate_And_Return_Token()
        {
            var repo = new InMemoryUserAccountRepository();
            var user = NewUser("john.doe@acme.com", "Strong123!");
            await repo.AddAsync(user, CancellationToken.None);

            var tokens = new StubTokenService();
            var handler = new AuthenticateCommandHandler(repo, new PasswordHasher(), tokens, CreateValidator());

            var cmd = new AuthenticateCommand
            {
                Email = "  John.Doe@Acme.com  ", 
                Password = "Strong123!"
            };

            var result = await handler.Handle(cmd, CancellationToken.None);

            result.AccessToken.Should().Be("stub.jwt.token");
            result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
            result.UserId.Should().Be(user.Id);
            result.Email.Should().Be(user.UserName);
            tokens.Calls.Should().Be(1);
            tokens.LastUserId.Should().Be(user.Id);
        }

        [Fact(DisplayName = "Should fail when email does not exist")]
        public async Task Should_Fail_When_Email_Not_Found()
        {
            var repo = new InMemoryUserAccountRepository();
            var tokens = new StubTokenService();
            var handler = new AuthenticateCommandHandler(repo, new PasswordHasher(), tokens, CreateValidator());

            var cmd = new AuthenticateCommand { Email = "missing@acme.com", Password = "validpassword123" };

            var act = () => handler.Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*Invalid credentials*");

            tokens.Calls.Should().Be(0);
        }

        [Fact(DisplayName = "Should fail when password is wrong")]
        public async Task Should_Fail_When_Password_Wrong()
        {
            var repo = new InMemoryUserAccountRepository();
            var user = NewUser("john@acme.com", "CorrectPassword1");
            await repo.AddAsync(user, CancellationToken.None);

            var tokens = new StubTokenService();
            var handler = new AuthenticateCommandHandler(repo, new PasswordHasher(), tokens, CreateValidator());

            var cmd = new AuthenticateCommand { Email = "john@acme.com", Password = "WrongPassword123" };

            var act = () => handler.Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*Invalid credentials*");

            tokens.Calls.Should().Be(0);
        }

        [Theory(DisplayName = "Should reject invalid credentials during validation")]
        [InlineData("", "password123")]
        [InlineData("invalid-email", "password123")]
        [InlineData("john@acme.com", "")]
        [InlineData("john@acme.com", "12345")]
        public async Task Should_Reject_Invalid_Credentials_During_Validation(string email, string password)
        {
            var repo = new InMemoryUserAccountRepository();
            var tokens = new StubTokenService();
            var handler = new AuthenticateCommandHandler(repo, new PasswordHasher(), tokens, CreateValidator());

            var cmd = new AuthenticateCommand { Email = email, Password = password };

            var act = () => handler.Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Validation failed*");

            tokens.Calls.Should().Be(0);
        }
    }
}
