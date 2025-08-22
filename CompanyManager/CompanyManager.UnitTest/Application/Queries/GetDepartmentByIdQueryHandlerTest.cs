using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

using CompanyManager.Application.Queries;
using CompanyManager.Domain.Entities;
using CompanyManager.UnitTest.Application.TestDouble; // InMemoryDepartmentRepository

namespace CompanyManager.UnitTest.Application.Queries
{
    public sealed class GetDepartmentByIdQueryHandlerTest
    {
        private static Department Dept(string name = "Engineering") => Department.Create(name);

        [Fact(DisplayName = "Should return department when found")]
        public async Task Should_Return_Department_When_Found()
        {
            // Arrange
            var repo = new InMemoryDepartmentRepository();
            var dept = Dept("Engineering");
            await repo.AddAsync(dept, CancellationToken.None);

            var handler = new GetDepartmentByIdQueryHandler(repo);

            // Act
            var found = await handler.Handle(dept.Id, CancellationToken.None);

            // Assert
            found.Should().NotBeNull();
            found!.Id.Should().Be(dept.Id);
            found.Name.Should().Be("Engineering");
        }

        [Fact(DisplayName = "Should return null when not found")]
        public async Task Should_Return_Null_When_Not_Found()
        {
            // Arrange
            var repo = new InMemoryDepartmentRepository();
            var handler = new GetDepartmentByIdQueryHandler(repo);

            // Act
            var found = await handler.Handle(Guid.NewGuid(), CancellationToken.None);

            // Assert
            found.Should().BeNull();
        }
    }
}
