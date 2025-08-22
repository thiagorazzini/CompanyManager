using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

using CompanyManager.Application.Commands;
using CompanyManager.Application.Handlers;
using CompanyManager.Domain.Entities;
using CompanyManager.UnitTest.Application.TestDouble; // InMemoryDepartmentRepository

namespace CompanyManager.UnitTest.Application.Handlers
{
    public sealed class DeleteDepartmentCommandHandlerTest
    {
        private static Department Dept(string name = "Engineering") => Department.Create(name);

        [Fact(DisplayName = "Should delete existing department")]
        public async Task Should_Delete_Existing_Department()
        {
            // Arrange
            var repo = new InMemoryDepartmentRepository();
            var dept = Dept("Engineering");
            await repo.AddAsync(dept, CancellationToken.None);

            var handler = new DeleteDepartmentCommandHandler(repo);
            var cmd = new DeleteDepartmentCommand { Id = dept.Id };

            // Act
            await handler.Handle(cmd, CancellationToken.None);

            // Assert
            var after = await repo.GetByIdAsync(dept.Id, CancellationToken.None);
            after.Should().BeNull();
        }

        [Fact(DisplayName = "Should be idempotent when department does not exist")]
        public async Task Should_Be_Idempotent_When_Not_Found()
        {
            // Arrange
            var repo = new InMemoryDepartmentRepository();
            var handler = new DeleteDepartmentCommandHandler(repo);
            var cmd = new DeleteDepartmentCommand { Id = Guid.NewGuid() };

            // Act & Assert: não deve lançar
            await handler.Handle(cmd, CancellationToken.None);
        }

        [Fact(DisplayName = "Should allow multiple deletions without error (idempotent)")]
        public async Task Should_Allow_Multiple_Deletions()
        {
            var repo = new InMemoryDepartmentRepository();
            var dept = Dept("HR");
            await repo.AddAsync(dept, CancellationToken.None);

            var handler = new DeleteDepartmentCommandHandler(repo);
            var cmd = new DeleteDepartmentCommand { Id = dept.Id };

            await handler.Handle(cmd, CancellationToken.None); // 1ª vez
            await handler.Handle(cmd, CancellationToken.None); // 2ª vez — idempotente

            var after = await repo.GetByIdAsync(dept.Id, CancellationToken.None);
            after.Should().BeNull();
        }
    }
}
