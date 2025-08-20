using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Xunit;

using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Handlers;
using CompanyManager.Application.Validators;
using CompanyManager.Domain.Entities;
using CompanyManager.UnitTest.Application.TestDouble; // InMemoryDepartmentRepository

namespace CompanyManager.UnitTest.Application.Handlers
{
    public sealed class UpdateDepartmentCommandHandlerTest
    {
        private static Department NewDept(string name = "Engineering") => Department.Create(name);

        private static IValidator<UpdateDepartmentRequest> CreateValidator() => 
            new UpdateDepartmentRequestValidator();

        [Fact(DisplayName = "Should rename department and set UpdatedAt")]
        public async Task Should_Rename_And_Set_UpdatedAt()
        {
            var repo = new InMemoryDepartmentRepository();
            var dept = NewDept("Engineering");
            await repo.AddAsync(dept, CancellationToken.None);

            var handler = new UpdateDepartmentCommandHandler(repo, CreateValidator());
            var before = dept.UpdatedAt;

            var cmd = new UpdateDepartmentCommand { Id = dept.Id, NewName = "Product" };
            await handler.Handle(cmd, CancellationToken.None);

            var saved = await repo.GetByIdAsync(dept.Id, CancellationToken.None);
            saved!.Name.Should().Be("Product");
            saved.CreatedAt.Should().Be(dept.CreatedAt);
            saved.UpdatedAt.Should().NotBe(before);
            saved.UpdatedAt.Should().NotBeNull();
        }

        [Fact(DisplayName = "Should trim new name")]
        public async Task Should_Trim_NewName()
        {
            var repo = new InMemoryDepartmentRepository();
            var dept = NewDept("HR");
            await repo.AddAsync(dept, CancellationToken.None);

            var handler = new UpdateDepartmentCommandHandler(repo, CreateValidator());
            var cmd = new UpdateDepartmentCommand { Id = dept.Id, NewName = "   People Ops   " };

            await handler.Handle(cmd, CancellationToken.None);

            var saved = await repo.GetByIdAsync(dept.Id, CancellationToken.None);
            saved!.Name.Should().Be("People Ops");
        }

        [Theory(DisplayName = "Should reject invalid names (null/empty/whitespace or too short)")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(" \t ")]
        [InlineData("A")] // mínimo 2
        public async Task Should_Reject_Invalid_Names(string newName)
        {
            var repo = new InMemoryDepartmentRepository();
            var dept = NewDept("Finance");
            await repo.AddAsync(dept, CancellationToken.None);

            var handler = new UpdateDepartmentCommandHandler(repo, CreateValidator());
            var cmd = new UpdateDepartmentCommand { Id = dept.Id, NewName = newName };

            var act = () => handler.Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*Validation failed*");
        }

        [Fact(DisplayName = "Should be idempotent when name is unchanged (UpdatedAt not touched)")]
        public async Task Should_Not_Touch_When_Same_Name()
        {
            var repo = new InMemoryDepartmentRepository();
            var dept = NewDept("Data");
            await repo.AddAsync(dept, CancellationToken.None);

            var handler = new UpdateDepartmentCommandHandler(repo, CreateValidator());
            var before = dept.UpdatedAt;

            // mesmo nome (mesmo após Trim)
            var cmd = new UpdateDepartmentCommand { Id = dept.Id, NewName = "  Data  " };
            await handler.Handle(cmd, CancellationToken.None);

            var saved = await repo.GetByIdAsync(dept.Id, CancellationToken.None);
            saved!.Name.Should().Be("Data");
            saved.UpdatedAt.Should().Be(before);
        }

        [Fact(DisplayName = "Should throw when department not found")]
        public async Task Should_Throw_When_Not_Found()
        {
            var repo = new InMemoryDepartmentRepository();
            var handler = new UpdateDepartmentCommandHandler(repo, CreateValidator());

            var cmd = new UpdateDepartmentCommand { Id = Guid.NewGuid(), NewName = "Whatever" };

            var act = () => handler.Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("*Department not found*");
        }
    }
}
