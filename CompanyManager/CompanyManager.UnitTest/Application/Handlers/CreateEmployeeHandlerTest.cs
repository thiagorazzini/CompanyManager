using CompanyManager.Application.Commands;
using CompanyManager.Application.Handlers;
using CompanyManager.Application.Services;
using CompanyManager.Application.Validators;
using CompanyManager.Domain.AccessControl;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Domain.ValueObjects;
using CompanyManager.UnitTest.Application.TestDouble;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;

namespace CompanyManager.UnitTest.Application.Handlers
{
    public class CreateEmployeeHandlerTest
    {
        private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
        private readonly Mock<IUserAccountRepository> _mockUserRepository;
        private readonly Mock<IDepartmentRepository> _mockDepartmentRepository;
        private readonly Mock<IJobTitleRepository> _mockJobTitleRepository;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<IHierarchicalAuthorizationService> _mockAuthorizationService;
        private readonly Mock<IRoleManagementService> _mockRoleManagementService;
        private readonly Mock<IEmployeeValidationService> _mockValidationService;
        private readonly Mock<ILogger<CreateEmployeeHandler>> _mockLogger;
        private readonly CreateEmployeeRequestValidator _validator;
        private readonly CreateEmployeeHandler _handler;

        public CreateEmployeeHandlerTest()
        {
            _mockEmployeeRepository = new Mock<IEmployeeRepository>();
            _mockUserRepository = new Mock<IUserAccountRepository>();
            _mockDepartmentRepository = new Mock<IDepartmentRepository>();
            _mockJobTitleRepository = new Mock<IJobTitleRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockAuthorizationService = new Mock<IHierarchicalAuthorizationService>();
            _mockRoleManagementService = new Mock<IRoleManagementService>();
            _mockValidationService = new Mock<IEmployeeValidationService>();
            _mockLogger = new Mock<ILogger<CreateEmployeeHandler>>();
            _validator = new CreateEmployeeRequestValidator();

            _handler = new CreateEmployeeHandler(
                _validator,
                _mockEmployeeRepository.Object,
                _mockUserRepository.Object,
                _mockJobTitleRepository.Object,
                _mockPasswordHasher.Object,
                _mockAuthorizationService.Object,
                _mockRoleManagementService.Object,
                _mockValidationService.Object,
                _mockLogger.Object);
        }

        private void SetupCommonMocks()
        {
            _mockRoleManagementService.Setup(x => x.GetOrCreateRoleByHierarchyLevelAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Role("TestRole", HierarchicalRole.Pleno));
            _mockAuthorizationService.Setup(x => x.CanCreateEmployeeWithRoleAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockValidationService.Setup(x => x.ValidateEmailNotInUseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockValidationService.Setup(x => x.ValidateDocumentNotInUseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockValidationService.Setup(x => x.ValidateDepartmentExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockValidationService.Setup(x => x.ValidateJobTitleExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldCreateEmployeeAndUserAccount()
        {
            // Arrange
            SetupCommonMocks();
            
            var command = new CreateEmployeeCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                DocumentNumber = "52998224725",
                DateOfBirth = "1990-01-01",
                Phones = new List<string> { "11999999999" },
                JobTitleId = Guid.NewGuid(),
                DepartmentId = Guid.NewGuid(),
                Password = "TestPassword123!"
            };
            var currentUserId = Guid.NewGuid();
            var currentUserRoleId = Guid.NewGuid();
            var currentUserRole = new Role("Manager", HierarchicalRole.Manager);
            
            var currentUser = UserAccount.Create("manager@test.com", "hash", Guid.NewGuid(), currentUserRoleId, Guid.NewGuid());
            var department = Department.Create("IT", "Information Technology");
            var jobTitle = JobTitle.Create("Developer", 4, "Software Developer"); // Nível 4 = Pleno
            var employee = Employee.Create("John", "Doe", new Email("john@test.com"), new DocumentNumber("52998224725"), 
                new DateOfBirth(DateTime.Parse("1990-01-01")), new[] { "+5511999999999" }, jobTitle.Id, department.Id);

            _mockUserRepository.Setup(x => x.GetByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentUser);
            _mockDepartmentRepository.Setup(x => x.GetByIdAsync(command.DepartmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(department);
            _mockJobTitleRepository.Setup(x => x.GetByIdAsync(command.JobTitleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jobTitle);
            _mockPasswordHasher.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashedPassword");
            _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserAccount?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            _mockEmployeeRepository.Verify(x => x.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CurrentUserNotFound_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var command = new CreateEmployeeCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                DocumentNumber = "52998224725",
                DateOfBirth = "1990-01-01",
                Phones = new List<string> { "11999999999" },
                JobTitleId = Guid.NewGuid(),
                DepartmentId = Guid.NewGuid(),
                Password = "TestPassword123!"
            };
            var currentUserId = Guid.NewGuid();

            // Setup mocks to pass validation but fail on user authorization
            var department = Department.Create("IT", "Information Technology");
            var jobTitle = JobTitle.Create("Developer", 4, "Software Developer");
            
            _mockDepartmentRepository.Setup(x => x.GetByIdAsync(command.DepartmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(department);
            _mockJobTitleRepository.Setup(x => x.GetByIdAsync(command.JobTitleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jobTitle);
            _mockValidationService.Setup(x => x.ValidateEmailNotInUseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockValidationService.Setup(x => x.ValidateDocumentNotInUseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockValidationService.Setup(x => x.ValidateDepartmentExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockValidationService.Setup(x => x.ValidateJobTitleExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            _mockUserRepository.Setup(x => x.GetByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserAccount?)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _handler.Handle(command, CancellationToken.None, currentUserId));
        }

        [Fact]
        public async Task Handle_CurrentUserRoleNotFound_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var command = new CreateEmployeeCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                DocumentNumber = "52998224725",
                DateOfBirth = "1990-01-01",
                Phones = new List<string> { "11999999999" },
                JobTitleId = Guid.NewGuid(),
                DepartmentId = Guid.NewGuid(),
                Password = "TestPassword123!"
            };
            var currentUserId = Guid.NewGuid();
            var currentUser = UserAccount.Create("user@test.com", "hash", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            // Setup mocks to pass validation but fail on role authorization
            var department = Department.Create("IT", "Information Technology");
            var jobTitle = JobTitle.Create("Developer", 4, "Software Developer");
            
            _mockDepartmentRepository.Setup(x => x.GetByIdAsync(command.DepartmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(department);
            _mockJobTitleRepository.Setup(x => x.GetByIdAsync(command.JobTitleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jobTitle);
            _mockValidationService.Setup(x => x.ValidateEmailNotInUseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockValidationService.Setup(x => x.ValidateDocumentNotInUseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockValidationService.Setup(x => x.ValidateDepartmentExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockValidationService.Setup(x => x.ValidateJobTitleExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            _mockUserRepository.Setup(x => x.GetByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentUser);
            _mockAuthorizationService.Setup(x => x.CanCreateEmployeeWithRoleAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnauthorizedAccessException("Current user role not found."));

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _handler.Handle(command, CancellationToken.None, currentUserId));
        }

        [Fact]
        public async Task Handle_UserCannotCreateHigherRole_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var command = new CreateEmployeeCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                DocumentNumber = "52998224725",
                DateOfBirth = "1990-01-01",
                Phones = new List<string> { "11999999999" },
                JobTitleId = Guid.NewGuid(),
                DepartmentId = Guid.NewGuid(),
                Password = "TestPassword123!"
            };
            var currentUserId = Guid.NewGuid();
            var currentUserRoleId = Guid.NewGuid();
            var currentUserRole = new Role("Junior", HierarchicalRole.Junior);
            
            var currentUser = UserAccount.Create("junior@test.com", "hash", Guid.NewGuid(), currentUserRoleId, Guid.NewGuid());
            var department = Department.Create("IT", "Information Technology");
            var jobTitle = JobTitle.Create("Director", 1, "Company Director"); // Nível 1 = Director (mais alto que Junior)

            _mockUserRepository.Setup(x => x.GetByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentUser);
            _mockDepartmentRepository.Setup(x => x.GetByIdAsync(command.DepartmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(department);
            _mockJobTitleRepository.Setup(x => x.GetByIdAsync(command.JobTitleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jobTitle);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _handler.Handle(command, CancellationToken.None, currentUserId));
        }

        [Fact]
        public async Task Handle_UserCanCreateLowerRole_ShouldSucceed()
        {
            // Arrange
            SetupCommonMocks();
            
            var command = new CreateEmployeeCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                DocumentNumber = "52998224725",
                DateOfBirth = "1990-01-01",
                Phones = new List<string> { "11999999999" },
                JobTitleId = Guid.NewGuid(),
                DepartmentId = Guid.NewGuid(),
                Password = "TestPassword123!"
            };
            var currentUserId = Guid.NewGuid();
            var currentUserRoleId = Guid.NewGuid();
            var currentUserRole = new Role("Manager", HierarchicalRole.Manager);
            
            var currentUser = UserAccount.Create("manager@test.com", "hash", Guid.NewGuid(), currentUserRoleId, Guid.NewGuid());
            var department = Department.Create("IT", "Information Technology");
            var jobTitle = JobTitle.Create("Developer", 5, "Software Developer"); // Nível 5 = Junior (mais baixo que Manager)
            var employee = Employee.Create("John", "Doe", new Email("john@test.com"), new DocumentNumber("52998224725"), 
                new DateOfBirth(DateTime.Parse("1990-01-01")), new[] { "+5511999999999" }, jobTitle.Id, department.Id);

            _mockUserRepository.Setup(x => x.GetByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentUser);
            _mockDepartmentRepository.Setup(x => x.GetByIdAsync(command.DepartmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(department);
            _mockJobTitleRepository.Setup(x => x.GetByIdAsync(command.JobTitleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jobTitle);
            _mockPasswordHasher.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashedPassword");
            _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserAccount?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
        }

        [Fact]
        public async Task Handle_SuperUserCanCreateAnyRole_ShouldSucceed()
        {
            // Arrange
            SetupCommonMocks();
            
            var command = new CreateEmployeeCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                DocumentNumber = "52998224725",
                DateOfBirth = "1990-01-01",
                Phones = new List<string> { "11999999999" },
                JobTitleId = Guid.NewGuid(),
                DepartmentId = Guid.NewGuid(),
                Password = "TestPassword123!"
            };
            var currentUserId = Guid.NewGuid();
            var currentUserRoleId = Guid.NewGuid();
            var currentUserRole = new Role("SuperUser", HierarchicalRole.SuperUser);
            
            var currentUser = UserAccount.Create("superuser@test.com", "hash", Guid.NewGuid(), currentUserRoleId, Guid.NewGuid());
            var department = Department.Create("IT", "Information Technology");
            var jobTitle = JobTitle.Create("Director", 1, "Company Director"); // Nível 1 = Director
            var employee = Employee.Create("John", "Doe", new Email("john@test.com"), new DocumentNumber("52998224725"), 
                new DateOfBirth(DateTime.Parse("1990-01-01")), new[] { "+5511999999999" }, jobTitle.Id, department.Id);

            _mockUserRepository.Setup(x => x.GetByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentUser);
            _mockDepartmentRepository.Setup(x => x.GetByIdAsync(command.DepartmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(department);
            _mockJobTitleRepository.Setup(x => x.GetByIdAsync(command.JobTitleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jobTitle);
            _mockPasswordHasher.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashedPassword");
            _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserAccount?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
        }

        [Fact]
        public async Task Handle_EmailAlreadyInUse_ShouldThrowArgumentException()
        {
            // Arrange
            var command = new CreateEmployeeCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                DocumentNumber = "52998224725",
                DateOfBirth = "1990-01-01",
                Phones = new List<string> { "11999999999" },
                JobTitleId = Guid.NewGuid(),
                DepartmentId = Guid.NewGuid(),
                Password = "TestPassword123!"
            };
            var currentUserId = Guid.NewGuid();
            var currentUserRoleId = Guid.NewGuid();
            var currentUserRole = new Role("Manager", HierarchicalRole.Manager);
            
            var currentUser = UserAccount.Create("manager@test.com", "hash", Guid.NewGuid(), currentUserRoleId, Guid.NewGuid());
            var existingUser = UserAccount.Create("john@test.com", "hash", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            _mockUserRepository.Setup(x => x.GetByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentUser);
            _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => 
                _handler.Handle(command, CancellationToken.None, currentUserId));
        }

        [Fact]
        public async Task Handle_DepartmentNotFound_ShouldThrowArgumentException()
        {
            // Arrange
            var command = new CreateEmployeeCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                DocumentNumber = "52998224725",
                DateOfBirth = "1990-01-01",
                Phones = new List<string> { "11999999999" },
                JobTitleId = Guid.NewGuid(),
                DepartmentId = Guid.NewGuid(),
                Password = "TestPassword123!"
            };
            var currentUserId = Guid.NewGuid();
            var currentUserRoleId = Guid.NewGuid();
            var currentUserRole = new Role("Manager", HierarchicalRole.Manager);
            
            var currentUser = UserAccount.Create("manager@test.com", "hash", Guid.NewGuid(), currentUserRoleId, Guid.NewGuid());

            _mockUserRepository.Setup(x => x.GetByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentUser);
            _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserAccount?)null);
            _mockDepartmentRepository.Setup(x => x.GetByIdAsync(command.DepartmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Department?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => 
                _handler.Handle(command, CancellationToken.None, currentUserId));
        }

        [Fact]
        public async Task Handle_JobTitleNotFound_ShouldThrowArgumentException()
        {
            // Arrange
            var command = new CreateEmployeeCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                DocumentNumber = "52998224725",
                DateOfBirth = "1990-01-01",
                Phones = new List<string> { "11999999999" },
                JobTitleId = Guid.NewGuid(),
                DepartmentId = Guid.NewGuid(),
                Password = "TestPassword123!"
            };
            var currentUserId = Guid.NewGuid();
            var currentUserRoleId = Guid.NewGuid();
            var currentUserRole = new Role("Manager", HierarchicalRole.Manager);
            
            var currentUser = UserAccount.Create("manager@test.com", "hash", Guid.NewGuid(), currentUserRoleId, Guid.NewGuid());
            var department = Department.Create("IT", "Information Technology");

            _mockUserRepository.Setup(x => x.GetByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentUser);
            _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserAccount?)null);
            _mockDepartmentRepository.Setup(x => x.GetByIdAsync(command.DepartmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(department);
            _mockJobTitleRepository.Setup(x => x.GetByIdAsync(command.JobTitleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((JobTitle?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => 
                _handler.Handle(command, CancellationToken.None, currentUserId));
        }
    }
}
