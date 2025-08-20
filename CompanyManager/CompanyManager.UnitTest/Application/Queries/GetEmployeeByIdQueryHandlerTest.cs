using CompanyManager.Application.Queries;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.ValueObjects;
using CompanyManager.UnitTest.Application.TestDouble;
using FluentAssertions;

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
                phones: new[] { new PhoneNumber("11 91111-1111", "BR") },
                jobTitle: "Developer",
                departmentId: deptId
            );

        [Fact(DisplayName = "Should return employee when found")]
        public async Task Should_Return_Employee_When_Found()
        {
            // Arrange
            var repo = new InMemoryEmployeeRepository();
            var handler = new GetEmployeeByIdQueryHandler(repo);

            var deptId = Guid.NewGuid();
            var emp = NewEmployee(deptId);
            await repo.AddAsync(emp, CancellationToken.None);

            // Act
            var found = await handler.Handle(emp.Id, CancellationToken.None);

            // Assert
            found.Should().NotBeNull();
            found!.Id.Should().Be(emp.Id);
            found.Email.Value.Should().Be("john.doe@company.com");
        }

        [Fact(DisplayName = "Should return null when not found")]
        public async Task Should_Return_Null_When_Not_Found()
        {
            // Arrange
            var repo = new InMemoryEmployeeRepository();
            var handler = new GetEmployeeByIdQueryHandler(repo);

            // Act
            var found = await handler.Handle(Guid.NewGuid(), CancellationToken.None);

            // Assert
            found.Should().BeNull();
        }
    }
}
