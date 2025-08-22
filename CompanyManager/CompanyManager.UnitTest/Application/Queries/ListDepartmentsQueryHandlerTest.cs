using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

using CompanyManager.Application.Common;   // PageRequest, PageResult, DepartmentFilter
using CompanyManager.Application.Queries;  // ListDepartmentsQueryHandler
using CompanyManager.Application.DTOs;     // ListDepartmentsRequest
using CompanyManager.Domain.Entities;
using CompanyManager.UnitTest.Application.TestDouble; // InMemoryDepartmentRepository

namespace CompanyManager.UnitTest.Application.Queries
{
    public sealed class ListDepartmentsQueryHandlerTest
    {
        private static Department Dept(string name) => Department.Create(name);

        private static async Task SeedAsync(InMemoryDepartmentRepository repo, params Department[] depts)
        {
            foreach (var d in depts) await repo.AddAsync(d, CancellationToken.None);
        }

        [Fact(DisplayName = "Should paginate results and expose total/flags (page 1)")]
        public async Task Should_Paginate_Page1()
        {
            var repo = new InMemoryDepartmentRepository();
            var handler = new ListDepartmentsQueryHandler(repo);

            await SeedAsync(repo,
                Dept("Engineering"),
                Dept("HR"),
                Dept("Finance"),
                Dept("Marketing"),
                Dept("Operations")
            );

            var request = new ListDepartmentsRequest { Page = 1, PageSize = 2 };
            var result = await handler.Handle(request, CancellationToken.None);

            result.Items.Should().HaveCount(2);
            result.Total.Should().Be(5);
            result.HasNext.Should().BeTrue();
            result.HasPrev.Should().BeFalse();
        }

        [Fact(DisplayName = "Should paginate results (page 3) and compute flags")]
        public async Task Should_Paginate_Page3()
        {
            var repo = new InMemoryDepartmentRepository();
            var handler = new ListDepartmentsQueryHandler(repo);

            await SeedAsync(repo,
                Dept("Engineering"),
                Dept("HR"),
                Dept("Finance"),
                Dept("Marketing"),
                Dept("Operations")
            );

            var request = new ListDepartmentsRequest { Page = 3, PageSize = 2 };
            var result = await handler.Handle(request, CancellationToken.None);

            result.Items.Should().HaveCount(1); // 5 itens, pageSize 2 ? pág 3 tem 1
            result.Total.Should().Be(5);
            result.HasNext.Should().BeFalse();
            result.HasPrev.Should().BeTrue();
        }

        [Fact(DisplayName = "Should filter by NameContains (case-insensitive, contains)")]
        public async Task Should_Filter_By_NameContains()
        {
            var repo = new InMemoryDepartmentRepository();
            var handler = new ListDepartmentsQueryHandler(repo);

            await SeedAsync(repo,
                Dept("Engineering"),
                Dept("Finance"),
                Dept("IT Support"),
                Dept("Audit")
            );

            var request = new ListDepartmentsRequest { NameContains = "it", Page = 1, PageSize = 10 };
            var result = await handler.Handle(request, CancellationToken.None);

            result.Total.Should().Be(2);
            result.Items.Select(d => d.Name).Should().BeEquivalentTo(new[] { "IT Support", "Audit" });
        }

        [Fact(DisplayName = "Should trim and be case-insensitive on filtering")]
        public async Task Should_Trim_And_CaseInsensitive_Filter()
        {
            var repo = new InMemoryDepartmentRepository();
            var handler = new ListDepartmentsQueryHandler(repo);

            await SeedAsync(repo,
                Dept("People Ops"),
                Dept("Operations")
            );

            var request = new ListDepartmentsRequest { NameContains = "  op ", Page = 1, PageSize = 10 };
            var result = await handler.Handle(request, CancellationToken.None);

            result.Total.Should().Be(2);

        }

        [Fact(DisplayName = "Should return empty page when no matches")]
        public async Task Should_Return_Empty_When_No_Matches()
        {
            var repo = new InMemoryDepartmentRepository();
            var handler = new ListDepartmentsQueryHandler(repo);

            await SeedAsync(repo, Dept("Engineering"), Dept("HR"));

            var request = new ListDepartmentsRequest { NameContains = "zzz", Page = 1, PageSize = 5 };
            var result = await handler.Handle(request, CancellationToken.None);

            result.Total.Should().Be(0);
            result.Items.Should().BeEmpty();
            result.HasNext.Should().BeFalse();
            result.HasPrev.Should().BeFalse();
        }
    }
}
