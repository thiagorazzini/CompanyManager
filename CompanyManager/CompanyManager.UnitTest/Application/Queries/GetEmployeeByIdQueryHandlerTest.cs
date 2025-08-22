using CompanyManager.Application.DTOs;
using CompanyManager.Application.Queries;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.ValueObjects;
using CompanyManager.UnitTest.Application.TestDouble;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CompanyManager.UnitTest.Application.Queries
{
    public sealed class GetEmployeeByIdQueryHandlerTest
    {
        private static Employee NewEmployee(Guid deptId) =>
            Employee.Create(
                firstName: "John",
                lastName: "Doe",
                email: new Email("john.doe@company.com"),
                documentNumber: new DocumentNumber("52998224725"),
                dateOfBirth: new DateOfBirth(DateTime.Today.AddYears(-30)),
                phoneNumbers: new[] { "11 91111-1111" },
                jobTitleId: Guid.NewGuid(),
                departmentId: deptId
            );

        [Fact(DisplayName = "Should return employee when found")]
        public async Task Should_Return_Employee_When_Found()
        {
            // Arrange
            var employeeRepo = new InMemoryEmployeeRepository();
            var deptId = Guid.NewGuid();
            var departmentRepo = new StubDepartmentRepository(new[] { deptId });
            var logger = new Mock<ILogger<GetEmployeeByIdQueryHandler>>();
            var handler = new GetEmployeeByIdQueryHandler(employeeRepo, departmentRepo, logger.Object);
            var emp = NewEmployee(deptId);
            await employeeRepo.AddAsync(emp, CancellationToken.None);

            var request = new GetEmployeeByIdRequest { Id = emp.Id };

            // Act
            var found = await handler.Handle(request, CancellationToken.None);

            // Assert
            found.Should().NotBeNull();
            found!.Id.Should().Be(emp.Id);
            found.Email.Should().Be("john.doe@company.com");
            found.FirstName.Should().Be("John");
            found.LastName.Should().Be("Doe");
        }

        [Fact(DisplayName = "Should return null when not found")]
        public async Task Should_Return_Null_When_Not_Found()
        {
            // Arrange
            var employeeRepo = new InMemoryEmployeeRepository();
            var departmentRepo = new StubDepartmentRepository(Array.Empty<Guid>());
            var logger = new Mock<ILogger<GetEmployeeByIdQueryHandler>>();
            var handler = new GetEmployeeByIdQueryHandler(employeeRepo, departmentRepo, logger.Object);

            var request = new GetEmployeeByIdRequest { Id = Guid.NewGuid() };

            // Act
            var found = await handler.Handle(request, CancellationToken.None);

            // Assert
            found.Should().BeNull();
        }
    }
}
