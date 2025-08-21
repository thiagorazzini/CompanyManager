using CompanyManager.Application.Commands;
using CompanyManager.Application.Handlers;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.ValueObjects;
using CompanyManager.UnitTest.Application.TestDouble;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyManager.UnitTest.Application.Handlers
{
    public sealed class DeleteEmployeeCommandHandlerTest
    {
        private static Employee NewEmployee(Guid deptId) =>
            Employee.Create(
                firstName: "John",
                lastName: "Doe",
                email: new Email("john.doe@company.com"),
                documentNumber: new DocumentNumber("52998224725"),
                dateOfBirth: new DateOfBirth(DateTime.Today.AddYears(-30)),
                phones: new[] { new PhoneNumber("11 91111-1111", "BR") },
                jobTitleId: Guid.NewGuid(),
                departmentId: deptId
            );

        [Fact(DisplayName = "Should delete existing employee (idempotent on repo)")]
        public async Task Should_Delete_Existing_Employee()
        {
            var repo = new InMemoryEmployeeRepository();
            var handler = new DeleteEmployeeCommandHandler(repo);

            var deptId = Guid.NewGuid();
            var emp = NewEmployee(deptId);
            await repo.AddAsync(emp, CancellationToken.None);

            var cmd = new DeleteEmployeeCommand { Id = emp.Id };

            await handler.Handle(cmd, CancellationToken.None);

            var after = await repo.GetByIdAsync(emp.Id, CancellationToken.None);
            after.Should().BeNull();
        }

        [Fact(DisplayName = "Should be idempotent when employee does not exist")]
        public async Task Should_Be_Idempotent_When_Not_Found()
        {
            var repo = new InMemoryEmployeeRepository();
            var handler = new DeleteEmployeeCommandHandler(repo);

            var cmd = new DeleteEmployeeCommand { Id = Guid.NewGuid() };

            // não deve lançar
            await handler.Handle(cmd, CancellationToken.None);
        }
    }
}
