using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using FluentValidation;

using CompanyManager.Application.Commands;
using CompanyManager.Application.Handlers;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Validators;
using CompanyManager.Domain.Entities;
using CompanyManager.UnitTest.Application.TestDouble;

namespace CompanyManager.UnitTest.Application.Handlers
{
    public sealed class CreateDepartmentCommandHandlerTest
    {
        private static IValidator<CreateDepartmentRequest> CreateValidator()
        {
            return new CreateDepartmentRequestValidator();
        }

        [Fact(DisplayName = "Should create department with valid name")]
        public async Task Should_Create_Department()
        {
            var repo = new InMemoryDepartmentRepository();
            var handler = new CreateDepartmentCommandHandler(repo, CreateValidator());

            var cmd = new CreateDepartmentCommand { Name = "Engineering" };

            var id = await handler.Handle(cmd, CancellationToken.None);

            id.Should().NotBeEmpty();

            var saved = await repo.GetByIdAsync(id, CancellationToken.None);
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("Engineering");
            saved.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
            saved.UpdatedAt.Should().BeNull();
        }

        [Fact(DisplayName = "Should trim name before creating")]
        public async Task Should_Trim_Name()
        {
            var repo = new InMemoryDepartmentRepository();
            var handler = new CreateDepartmentCommandHandler(repo, CreateValidator());

            var cmd = new CreateDepartmentCommand { Name = "   HR   " };

            var id = await handler.Handle(cmd, CancellationToken.None);
            var saved = await repo.GetByIdAsync(id, CancellationToken.None);

            saved!.Name.Should().Be("HR");
        }

        [Theory(DisplayName = "Should reject invalid names (null/empty/whitespace or too short)")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(" \t ")]
        [InlineData("A")] // mínimo 2
        public async Task Should_Reject_Invalid_Names(string name)
        {
            var repo = new InMemoryDepartmentRepository();
            var handler = new CreateDepartmentCommandHandler(repo, CreateValidator());

            var cmd = new CreateDepartmentCommand { Name = name };

            Func<Task> act = () => handler.Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*Validation failed*");
        }
    }
}
