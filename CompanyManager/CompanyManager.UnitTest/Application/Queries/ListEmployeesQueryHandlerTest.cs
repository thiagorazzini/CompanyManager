using CompanyManager.Application.Common;
using CompanyManager.Application.Queries;
using CompanyManager.Application.DTOs;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.ValueObjects;
using CompanyManager.UnitTest.Application.TestDouble;
using FluentAssertions;

namespace CompanyManager.UnitTest.Application.Queries
{
    public sealed class ListEmployeesQueryHandlerTest
    {
        // ---------- helpers ----------
        private static Employee NewEmployee(
            Guid departmentId,
            string first = "John",
            string last = "Doe",
            string email = "john.doe@company.com",
            string cpf = "52998224725",
            string job = "Developer",
            string phone = "11 91111-1111")
        {
            return Employee.Create(
                firstName: first,
                lastName: last,
                email: new Email(email),
                documentNumber: new DocumentNumber(cpf),
                dateOfBirth: new DateOfBirth(DateTime.Today.AddYears(-30)),
                phoneNumbers: new[] { phone },
                jobTitleId: Guid.NewGuid(),
                departmentId: departmentId
            );
        }

        private static async Task SeedAsync(InMemoryEmployeeRepository repo, params Employee[] emps)
        {
            foreach (var e in emps)
                await repo.AddAsync(e, CancellationToken.None);
        }

        // ---------- tests ----------

        [Fact(DisplayName = "Should paginate results and expose total/hasNext/hasPrev (page 1)")]
        public async Task Should_Paginate_Page1()
        {
            var repo = new InMemoryEmployeeRepository();
            var handler = new ListEmployeesQueryHandler(repo);

            var deptA = Guid.NewGuid();
            var deptB = Guid.NewGuid();

            await SeedAsync(repo,
                NewEmployee(deptA, "John", "Doe", "john@x.com", "52998224725", "Developer"),
                NewEmployee(deptA, "Jane", "Doe", "jane@x.com", "11144477735", "QA"),
                NewEmployee(deptA, "Alice", "Smith", "alice@x.com", "93541134780", "Developer"),
                NewEmployee(deptB, "Bob", "Brown", "bob@x.com", "86420753171", "Manager"),   // <-- válido
                NewEmployee(deptB, "Eve", "Stone", "eve@x.com", "74685296397", "DevOps")     // <-- válido
            );

            var request = new ListEmployeesRequest { Page = 1, PageSize = 2 };

            var result = await handler.Handle(request, CancellationToken.None);

            result.Items.Should().HaveCount(2);
            result.Total.Should().Be(5);
            result.HasNext.Should().BeTrue();
            result.HasPrev.Should().BeFalse();
        }

        [Fact(DisplayName = "Should paginate results (page 3) and compute flags")]
        public async Task Should_Paginate_Page3()
        {
            var repo = new InMemoryEmployeeRepository();
            var handler = new ListEmployeesQueryHandler(repo);

            var dept = Guid.NewGuid();

            // gera 5 CPFs válidos
            var cpf1 = MakeCpf("123456789");
            var cpf2 = MakeCpf("987654321");
            var cpf3 = MakeCpf("012345678");
            var cpf4 = MakeCpf("102938475");
            var cpf5 = MakeCpf("564738291");

            await SeedAsync(repo,
                NewEmployee(dept, "Al", "Al", "a@x.com", cpf1),
                NewEmployee(dept, "Be", "Be", "b@x.com", cpf2),
                NewEmployee(dept, "Cy", "Cy", "c@x.com", cpf3),
                NewEmployee(dept, "Di", "Di", "d@x.com", cpf4),
                NewEmployee(dept, "Ed", "Ed", "e@x.com", cpf5)
            );

            var request = new ListEmployeesRequest { Page = 3, PageSize = 2 };
            var result = await handler.Handle(request, CancellationToken.None);

            result.Items.Should().HaveCount(1); // 5 itens, pageSize 2 -> página 3 tem 1
            result.Total.Should().Be(5);
            result.HasNext.Should().BeFalse();
            result.HasPrev.Should().BeTrue();
        }

        private static string MakeCpf(string nineDigits)
        {
            if (nineDigits is null || nineDigits.Length != 9 || !nineDigits.All(char.IsDigit))
                throw new ArgumentException("Provide exactly 9 digits.", nameof(nineDigits));

            int CalcDigit(ReadOnlySpan<int> arr, int startFactor)
            {
                int sum = 0, factor = startFactor;
                for (int i = 0; i < arr.Length; i++) sum += arr[i] * factor--;
                int rest = sum % 11;
                return rest < 2 ? 0 : 11 - rest;
            }

            var base9 = nineDigits.Select(c => c - '0').ToArray();
            var d1 = CalcDigit(base9, 10);
            var base10 = base9.Concat(new[] { d1 }).ToArray();
            var d2 = CalcDigit(base10, 11);

            return nineDigits + d1.ToString() + d2.ToString();
        }
        [Fact(DisplayName = "Should filter by NameOrEmail (case-insensitive, contains)")]
        public async Task Should_Filter_By_NameOrEmail()
        {
            var repo = new InMemoryEmployeeRepository();
            var handler = new ListEmployeesQueryHandler(repo);

            var dept = Guid.NewGuid();
            await SeedAsync(repo,
                NewEmployee(dept, "John", "Doe", "john@doe.com", "52998224725"),
                NewEmployee(dept, "Jane", "Doe", "jane@doe.com", "11144477735"),
                NewEmployee(dept, "Alice", "Smith", "alice@acme.com", "93541134780")
            );

            var request = new ListEmployeesRequest { NameContains = "doe", Page = 1, PageSize = 10 };
            var result = await handler.Handle(request, CancellationToken.None);

            result.Total.Should().Be(2);
            result.Items.Select(i => i.Email.Value).Should().BeEquivalentTo(new[]
            {
                "john@doe.com","jane@doe.com"
            });
        }

        [Fact(DisplayName = "Should filter by DepartmentId")]
        public async Task Should_Filter_By_Department()
        {
            var deptA = Guid.NewGuid();
            var deptB = Guid.NewGuid();

            var repo = new InMemoryEmployeeRepository();
            var handler = new ListEmployeesQueryHandler(repo);

            await SeedAsync(repo,
                NewEmployee(deptA, "John", "Doe", "john@x.com", "86420753171"),
                NewEmployee(deptA, "Jane", "Doe", "jane@x.com", "86420753171"),
                NewEmployee(deptB, "Bob", "Brown", "bob@x.com", "86420753171")
            );

            var request = new ListEmployeesRequest { DepartmentId = deptA, Page = 1, PageSize = 10 };
            var result = await handler.Handle(request, CancellationToken.None);

            result.Total.Should().Be(2);
            result.Items.All(i => i.DepartmentId == deptA).Should().BeTrue();
        }

        [Fact(DisplayName = "Should filter by JobTitleId")]
        public async Task Should_Filter_By_JobTitleId()
        {
            var dept = Guid.NewGuid();
            var jobTitleId1 = Guid.NewGuid();
            var jobTitleId2 = Guid.NewGuid();

            var repo = new InMemoryEmployeeRepository();
            var handler = new ListEmployeesQueryHandler(repo);

            // Criar employees com JobTitleIds específicos
            var emp1 = Employee.Create("John", "Doe", new Email("a@x.com"), new DocumentNumber("52998224725"), 
                new DateOfBirth(DateTime.Today.AddYears(-30)), new[] { "11 99999-9999" }, 
                jobTitleId1, dept);
            var emp2 = Employee.Create("Jane", "Smith", new Email("b@x.com"), new DocumentNumber("11144477735"), 
                new DateOfBirth(DateTime.Today.AddYears(-25)), new[] { "11 98888-8888" }, 
                jobTitleId2, dept);
            var emp3 = Employee.Create("Bob", "Brown", new Email("c@x.com"), new DocumentNumber("93541134780"), 
                new DateOfBirth(DateTime.Today.AddYears(-35)), new[] { "11 97777-7777" }, 
                jobTitleId1, dept);

            await SeedAsync(repo, emp1, emp2, emp3);

            var request = new ListEmployeesRequest { JobTitleId = jobTitleId1, Page = 1, PageSize = 10 };
            var result = await handler.Handle(request, CancellationToken.None);

            result.Total.Should().Be(2); // emp1 e emp3 têm jobTitleId1
            result.Items.All(i => i.JobTitleId == jobTitleId1).Should().BeTrue();
        }

        [Fact(DisplayName = "Should return empty page when no matches")]
        public async Task Should_Return_Empty_When_No_Matches()
        {
            var repo = new InMemoryEmployeeRepository();
            var handler = new ListEmployeesQueryHandler(repo);

            var request = new ListEmployeesRequest { NameContains = "zzz-not-found", Page = 1, PageSize = 5 };
            var result = await handler.Handle(request, CancellationToken.None);

            result.Total.Should().Be(0);
            result.Items.Should().BeEmpty();
            result.HasNext.Should().BeFalse();
            result.HasPrev.Should().BeFalse();
        }
    }
}
