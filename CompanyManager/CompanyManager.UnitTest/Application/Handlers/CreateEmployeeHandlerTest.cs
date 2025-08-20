using CompanyManager.Domain.Interfaces;
using CompanyManager.Domain.AccessControl;
using CompanyManager.Domain.Entities;
using CompanyManager.Application.Handlers;
using CompanyManager.Application.Validators;
using CompanyManager.UnitTest.Application.TestDouble;
using CompanyManager.UnitTest.Builders;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CompanyManager.Application.Handlers;
public sealed class CreateEmployeeHandlerTest
{
    private readonly CreateEmployeeRequestValidator _validator = new();

    private (CreateEmployeeHandler sut,
             InMemoryEmployeeRepository employees,
             InMemoryUserAccountRepository users,
             InMemoryDepartmentRepository departments,
             IPasswordHasher hasher, Guid currentUserId)
        BuildSut(Guid existingDepartmentId)
    {
        var employees = new InMemoryEmployeeRepository();
        var users = new InMemoryUserAccountRepository();
        var departments = new InMemoryDepartmentRepository(new[] { existingDepartmentId });
        var hasher = new FakeHasher();

        // Create current user with Director role to allow creating any employee
        var currentUser = UserAccount.Create("current@company.com", "hash", Guid.NewGuid());
        var directorRole = new Role("Director", HierarchicalRole.Director);
        currentUser.AddRole(directorRole);
        users.AddAsync(currentUser, CancellationToken.None).Wait();

        var currentUserId = currentUser.Id;

        var loggerMock = new Mock<ILogger<CreateEmployeeHandler>>();
        var sut = new CreateEmployeeHandler(_validator, employees, users, departments, hasher, loggerMock.Object);
        return (sut, employees, users, departments, hasher, currentUserId);
    }

    [Fact(DisplayName = "Should create Employee and UserAccount successfully")]
    public async Task Should_Create_Employee_And_UserAccount()
    {
        var departmentId = Guid.NewGuid();
        var (sut, employees, users, _, hasher, currentUserId) = BuildSut(departmentId);

        var request = CreateEmployeeRequestBuilder.New()
            .WithDepartment(departmentId)
            .Build();

        var employeeId = await sut.Handle(request, currentUserId, CancellationToken.None);

        employeeId.Should().NotBeEmpty();

        var employee = await employees.GetByIdAsync(employeeId, default);
        employee.Should().NotBeNull();
        employee!.FirstName.Should().Be("John");
        employee.LastName.Should().Be("Doe");
        employee.Email.Value.Should().Be("john.doe@company.com");
        employee.ManagerId.Should().BeNull();
        employee.HasManager.Should().BeFalse();
        employee.HierarchyLevel.Should().Be(0);

        // Verify UserAccount is created with same ID as Employee
        var userAccounts = await users.GetAllAsync(default);
        var userAccount = userAccounts.FirstOrDefault(u => u.EmployeeId == employeeId);
        userAccount.Should().NotBeNull();
        userAccount!.UserName.Should().Be("john.doe@company.com");
    }

    [Fact(DisplayName = "Should create Employee with manager successfully")]
    public async Task Should_Create_Employee_With_Manager_Successfully()
    {
        var departmentId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var (sut, employees, users, _, hasher, currentUserId) = BuildSut(departmentId);

        var request = CreateEmployeeRequestBuilder.New()
            .WithDepartment(departmentId)
            .WithManager(managerId)
            .Build();

        var employeeId = await sut.Handle(request, currentUserId, CancellationToken.None);

        employeeId.Should().NotBeEmpty();

        var employee = await employees.GetByIdAsync(employeeId, default);
        employee.Should().NotBeNull();
        employee!.ManagerId.Should().Be(managerId);
        employee.HasManager.Should().BeTrue();
        employee.HierarchyLevel.Should().Be(1);
    }

    [Fact(DisplayName = "Should reject when department does not exist")]
    public async Task Should_Reject_Unknown_Department()
    {
        var unknownDept = Guid.NewGuid();
        var (sut, _, _, _, _, currentUserId) = BuildSut(existingDepartmentId: Guid.NewGuid());

        var request = CreateEmployeeRequestBuilder.New()
            .WithDepartment(unknownDept)
            .Build();

        Func<Task> act = () => sut.Handle(request, currentUserId, default);

        await act.Should().ThrowAsync<ArgumentException>()
                 .WithMessage("*department*");
    }

    [Fact(DisplayName = "Should reject duplicate email")]
    public async Task Should_Reject_Duplicate_Email()
    {
        var deptId = Guid.NewGuid();
        var (sut, employees, _, _, _, currentUserId) = BuildSut(deptId);

        employees.SeedEmail("john.doe@company.com");

        var request = CreateEmployeeRequestBuilder.New()
            .WithDepartment(deptId)
            .Build();

        Func<Task> act = () => sut.Handle(request, currentUserId, default);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*Email already in use*");
    }

    [Fact(DisplayName = "Should reject duplicate CPF")]
    public async Task Should_Reject_Duplicate_Cpf()
    {
        var deptId = Guid.NewGuid();
        var (sut, employees, _, _, _, currentUserId) = BuildSut(deptId);

        employees.SeedCpf("52998224725");

        var request = CreateEmployeeRequestBuilder.New()
            .WithDepartment(deptId)
            .Build();

        Func<Task> act = () => sut.Handle(request, currentUserId, default);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*Document number already in use*");
    }

    [Fact(DisplayName = "Should throw ValidationException when request is invalid")]
    public async Task Should_Throw_ValidationException_On_Invalid_Request()
    {
        var deptId = Guid.NewGuid();
        var (sut, _, _, _, _, currentUserId) = BuildSut(deptId);

        var request = CreateEmployeeRequestBuilder.New()
            .WithDepartment(deptId)
            .WithEmail("invalid-email")
            .Build();

        Func<Task> act = () => sut.Handle(request, currentUserId, default);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Should reject when user tries to create employee with higher role level")]
    public async Task Should_Reject_Creating_Employee_With_Higher_Role_Level()
    {
        var departmentId = Guid.NewGuid();
        var (sut, employees, users, _, hasher, currentUserId) = BuildSut(departmentId);

        // Create junior user
        var juniorUser = UserAccount.Create("junior@company.com", "hash", Guid.NewGuid());
        var juniorRole = new Role("Junior", HierarchicalRole.Junior);
        juniorUser.AddRole(juniorRole);
        await users.AddAsync(juniorUser, CancellationToken.None);
        
        var juniorUserId = juniorUser.Id;

        var request = CreateEmployeeRequestBuilder.New()
            .WithDepartment(departmentId)
            .WithRoleLevel("Manager")
            .Build();

        Func<Task> act = () => sut.Handle(request, juniorUserId, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
                 .WithMessage("*cannot create employees with role level 'Manager'*");
    }

    [Fact(DisplayName = "Should allow when user creates employee with equal or lower role level")]
    public async Task Should_Allow_Creating_Employee_With_Equal_Or_Lower_Role_Level()
    {
        var departmentId = Guid.NewGuid();
        var (sut, employees, users, _, hasher, currentUserId) = BuildSut(departmentId);

        // Create manager user
        var managerUser = UserAccount.Create("manager@company.com", "hash", Guid.NewGuid());
        var managerRole = new Role("Manager", HierarchicalRole.Manager);
        managerUser.AddRole(managerRole);
        await users.AddAsync(managerUser, CancellationToken.None);
        
        var managerUserId = managerUser.Id;

        var request = CreateEmployeeRequestBuilder.New()
            .WithDepartment(departmentId)
            .WithRoleLevel("Senior")
            .Build();

        var employeeId = await sut.Handle(request, managerUserId, CancellationToken.None);

        employeeId.Should().NotBeEmpty();
        
        var employee = await employees.GetByIdAsync(employeeId, default);
        employee.Should().NotBeNull();
    }
}