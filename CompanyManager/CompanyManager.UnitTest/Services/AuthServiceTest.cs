using CompanyManager.Application.Services;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace CompanyManager.UnitTest.Services
{
    public class AuthServiceTest
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthService _authService;
        private readonly Employee _testEmployee;
        private readonly string _validJwtSecret;
        private readonly string _validIssuer;
        private readonly string _validAudience;

        public AuthServiceTest()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _validJwtSecret = "super-secret-jwt-key-with-at-least-256-bits-for-security";
            _validIssuer = "CompanyManager.API";
            _validAudience = "CompanyManager.Client";

            SetupMockConfiguration();
            //_authService = new AuthService(_mockConfiguration.Object);
            //_testEmployee = CreateValidTestEmployee();
        }

        //[Fact(DisplayName = "Should generate JWT token with expected claims")]
        //public void Should_Generate_JWT_Token_With_Expected_Claims()
        //{
        //    // Arrange
        //    var loginRequest = new LoginRequest
        //    {
        //        Email = _testEmployee.Email.Value,
        //        Password = "validPassword123"
        //    };

        //    // Act
        //    var result = _authService.GenerateJwtToken(_testEmployee);

        //    // Assert
        //    result.Should().NotBeNullOrEmpty();

        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var jwtToken = tokenHandler.ReadJwtToken(result);

        //    jwtToken.Claims.Should().Contain(c => c.Type == "sub" && c.Value == _testEmployee.Id.ToString());
        //    jwtToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == _testEmployee.Email.Value);
        //    jwtToken.Claims.Should().Contain(c => c.Type == "role" && c.Value == _testEmployee.Role);
        //}

        //[Fact(DisplayName = "Should generate JWT token with correct subject claim")]
        //public void Should_Generate_JWT_Token_With_Correct_Subject_Claim()
        //{
        //    // Arrange
        //    var employee = CreateValidTestEmployee();

        //    // Act
        //    var token = _authService.GenerateJwtToken(employee);

        //    // Assert
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var jwtToken = tokenHandler.ReadJwtToken(token);

        //    var subjectClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub");
        //    subjectClaim.Should().NotBeNull();
        //    subjectClaim!.Value.Should().Be(employee.Id.ToString());
        //}

        //[Fact(DisplayName = "Should generate JWT token with correct email claim")]
        //public void Should_Generate_JWT_Token_With_Correct_Email_Claim()
        //{
        //    // Arrange
        //    var employee = CreateValidTestEmployee();

        //    // Act
        //    var token = _authService.GenerateJwtToken(employee);

        //    // Assert
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var jwtToken = tokenHandler.ReadJwtToken(token);

        //    var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email");
        //    emailClaim.Should().NotBeNull();
        //    emailClaim!.Value.Should().Be(employee.Email.Value);
        //}

        //[Fact(DisplayName = "Should generate JWT token with correct role claim")]
        //public void Should_Generate_JWT_Token_With_Correct_Role_Claim()
        //{
        //    // Arrange
        //    var employee = CreateValidTestEmployee();

        //    // Act
        //    var token = _authService.GenerateJwtToken(employee);

        //    // Assert
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var jwtToken = tokenHandler.ReadJwtToken(token);

        //    var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role");
        //    roleClaim.Should().NotBeNull();
        //    roleClaim!.Value.Should().Be(employee.Role);
        //}

        //[Fact(DisplayName = "Should generate JWT token with expiration in the future")]
        //public void Should_Generate_JWT_Token_With_Expiration_In_The_Future()
        //{
        //    // Arrange
        //    var employee = CreateValidTestEmployee();

        //    // Act
        //    var token = _authService.GenerateJwtToken(employee);

        //    // Assert
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var jwtToken = tokenHandler.ReadJwtToken(token);

        //    jwtToken.ValidTo.Should().BeAfter(DateTime.UtcNow);
        //    jwtToken.ValidTo.Should().BeAfter(DateTime.UtcNow.AddMinutes(1));
        //}

        //[Fact(DisplayName = "Should generate JWT token with correct expiration time")]
        //public void Should_Generate_JWT_Token_With_Correct_Expiration_Time()
        //{
        //    // Arrange
        //    var employee = CreateValidTestEmployee();
        //    var expectedExpiration = DateTime.UtcNow.AddHours(24);

        //    // Act
        //    var token = _authService.GenerateJwtToken(employee);

        //    // Assert
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var jwtToken = tokenHandler.ReadJwtToken(token);

        //    jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(5));
        //}

        //[Fact(DisplayName = "Should generate JWT token with correct issuer")]
        //public void Should_Generate_JWT_Token_With_Correct_Issuer()
        //{
        //    // Arrange
        //    var employee = CreateValidTestEmployee();

        //    // Act
        //    var token = _authService.GenerateJwtToken(employee);

        //    // Assert
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var jwtToken = tokenHandler.ReadJwtToken(token);

        //    jwtToken.Issuer.Should().Be(_validIssuer);
        //}

        //[Fact(DisplayName = "Should generate JWT token with correct audience")]
        //public void Should_Generate_JWT_Token_With_Correct_Audience()
        //{
        //    // Arrange
        //    var employee = CreateValidTestEmployee();

        //    // Act
        //    var token = _authService.GenerateJwtToken(employee);

        //    // Assert
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var jwtToken = tokenHandler.ReadJwtToken(token);

        //    jwtToken.Audiences.Should().Contain(_validAudience);
        //}

        //[Fact(DisplayName = "Should generate JWT token that validates against configured issuer and audience")]
        //public void Should_Generate_JWT_Token_That_Validates_Against_Configured_Issuer_And_Audience()
        //{
        //    // Arrange
        //    var employee = CreateValidTestEmployee();
        //    var tokenValidationParameters = new TokenValidationParameters
        //    {
        //        ValidateIssuer = true,
        //        ValidateAudience = true,
        //        ValidateLifetime = true,
        //        ValidateIssuerSigningKey = true,
        //        ValidIssuer = _validIssuer,
        //        ValidAudience = _validAudience,
        //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_validJwtSecret))
        //    };

        //    // Act
        //    var token = _authService.GenerateJwtToken(employee);

        //    // Assert
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

        //    principal.Should().NotBeNull();
        //    validatedToken.Should().NotBeNull();
        //}

        //[Fact(DisplayName = "Should generate JWT token with correct signing algorithm")]
        //public void Should_Generate_JWT_Token_With_Correct_Signing_Algorithm()
        //{
        //    // Arrange
        //    var employee = CreateValidTestEmployee();

        //    // Act
        //    var token = _authService.GenerateJwtToken(employee);

        //    // Assert
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var jwtToken = tokenHandler.ReadJwtToken(token);

        //    jwtToken.Header.Alg.Should().Be("HS256");
        //}

        //[Fact(DisplayName = "Should generate JWT token with correct token type")]
        //public void Should_Generate_JWT_Token_With_Correct_Token_Type()
        //{
        //    // Arrange
        //    var employee = CreateValidTestEmployee();

        //    // Act
        //    var token = _authService.GenerateJwtToken(employee);

        //    // Assert
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var jwtToken = tokenHandler.ReadJwtToken(token);

        //    jwtToken.Header.Typ.Should().Be("JWT");
        //}

        //[Fact(DisplayName = "Should generate JWT token with correct issued at time")]
        //public void Should_Generate_JWT_Token_With_Correct_Issued_At_Time()
        //{
        //    // Arrange
        //    var employee = CreateValidTestEmployee();
        //    var beforeGeneration = DateTime.UtcNow;

        //    // Act
        //    var token = _authService.GenerateJwtToken(employee);

        //    // Assert
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var jwtToken = tokenHandler.ReadJwtToken(token);

        //    jwtToken.IssuedAt.Should().BeCloseTo(beforeGeneration, TimeSpan.FromSeconds(5));
        //    jwtToken.IssuedAt.Should().BeBeforeOrEqualTo(DateTime.UtcNow);
        //}

        //[Fact(DisplayName = "Should generate JWT token with correct not before time")]
        //public void Should_Generate_JWT_Token_With_Correct_Not_Before_Time()
        //{
        //    // Arrange
        //    var employee = CreateValidTestEmployee();

        //    // Act
        //    var token = _authService.GenerateJwtToken(employee);

        //    // Assert
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var jwtToken = tokenHandler.ReadJwtToken(token);

        //    jwtToken.ValidFrom.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        //    jwtToken.ValidFrom.Should().BeBeforeOrEqualTo(DateTime.UtcNow);
        //}

        //[Fact(DisplayName = "Should generate different JWT tokens for different employees")]
        //public void Should_Generate_Different_JWT_Tokens_For_Different_Employees()
        //{
        //    // Arrange
        //    var employee1 = CreateValidTestEmployee("João", "Silva", "joao@empresa.com", "123.456.789-01");
        //    var employee2 = CreateValidTestEmployee("Maria", "Santos", "maria@empresa.com", "987.654.321-00");

        //    // Act
        //    var token1 = _authService.GenerateJwtToken(employee1);
        //    var token2 = _authService.GenerateJwtToken(employee2);

        //    // Assert
        //    token1.Should().NotBe(token2);
        //}

        //[Fact(DisplayName = "Should generate JWT token with correct claim types")]
        //public void Should_Generate_JWT_Token_With_Correct_Claim_Types()
        //{
        //    // Arrange
        //    var employee = CreateValidTestEmployee();

        //    // Act
        //    var token = _authService.GenerateJwtToken(employee);

        //    // Assert
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var jwtToken = tokenHandler.ReadJwtToken(token);

        //    var expectedClaimTypes = new[] { "sub", "email", "role", "iat", "nbf", "exp", "iss", "aud" };
        //    jwtToken.Claims.Select(c => c.Type).Should().Contain(expectedClaimTypes);
        //}

        //[Fact(DisplayName = "Should generate JWT token with correct claim values")]
        //public void Should_Generate_JWT_Token_With_Correct_Claim_Values()
        //{
        //    // Arrange
        //    var employee = CreateValidTestEmployee();

        //    // Act
        //    var token = _authService.GenerateJwtToken(employee);

        //    // Assert
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var jwtToken = tokenHandler.ReadJwtToken(token);

        //    var claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);

        //    claims["sub"].Should().Be(employee.Id.ToString());
        //    claims["email"].Should().Be(employee.Email.Value);
        //    claims["role"].Should().Be(employee.Role);
        //    claims["iss"].Should().Be(_validIssuer);
        //    claims["aud"].Should().Be(_validAudience);
        //}

        //[Fact(DisplayName = "Should generate JWT token with correct token structure")]
        //public void Should_Generate_JWT_Token_With_Correct_Token_Structure()
        //{
        //    // Arrange
        //    var employee = CreateValidTestEmployee();

        //    // Act
        //    var token = _authService.GenerateJwtToken(employee);

        //    // Assert
        //    token.Should().NotBeNullOrEmpty();
        //    token.Should().Contain(".");
        //    token.Split('.').Should().HaveCount(3); // Header.Payload.Signature

        //    var parts = token.Split('.');
        //    parts[0].Should().NotBeEmpty(); // Header
        //    parts[1].Should().NotBeEmpty(); // Payload
        //    parts[2].Should().NotBeEmpty(); // Signature
        //}

        //[Fact(DisplayName = "Should generate JWT token that can be decoded and validated")]
        //public void Should_Generate_JWT_Token_That_Can_Be_Decoded_And_Validated()
        //{
        //    // Arrange
        //    var employee = CreateValidTestEmployee();

        //    // Act
        //    var token = _authService.GenerateJwtToken(employee);

        //    // Assert
        //    var tokenHandler = new JwtSecurityTokenHandler();

        //    // Should not throw exception when reading token
        //    Action readToken = () => tokenHandler.ReadJwtToken(token);
        //    readToken.Should().NotThrow();

        //    var jwtToken = tokenHandler.ReadJwtToken(token);
        //    jwtToken.Should().NotBeNull();
        //    jwtToken.Header.Should().NotBeNull();
        //    jwtToken.Payload.Should().NotBeNull();
        //}

        private void SetupMockConfiguration()
        {
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(x => x["Secret"]).Returns(_validJwtSecret);
            jwtSection.Setup(x => x["Issuer"]).Returns(_validIssuer);
            jwtSection.Setup(x => x["Audience"]).Returns(_validAudience);
            jwtSection.Setup(x => x["ExpirationHours"]).Returns("24");

            _mockConfiguration.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSection.Object);
        }

        //private static Employee CreateValidTestEmployee(
        //    string firstName = "João",
        //    string lastName = "Silva",
        //    string email = "joao.silva@empresa.com",
        //    string documentNumber = "123.456.789-01")
        //{
        //    return new Employee(
        //        firstName,
        //        lastName,
        //        new Email(email),
        //        new DocumentNumber(documentNumber),
        //        new List<PhoneNumber> { new PhoneNumber("+55 11 99999-9999") },
        //        "Developer",
        //        new DateOfBirth(DateTime.Today.AddYears(-25)),
        //        "password123"
        //    );
        //}
    }
}
