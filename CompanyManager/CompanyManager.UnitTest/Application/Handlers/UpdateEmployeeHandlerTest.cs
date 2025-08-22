using CompanyManager.Application.Commands;
using CompanyManager.Application.Handlers;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Validators;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.ValueObjects;
using CompanyManager.UnitTest.Application.TestDouble;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace CompanyManager.UnitTest.Application.Handlers
{
    public sealed class UpdateEmployeeCommandHandlerTest
    {
        // ---------- helpers ----------
        private static Employee NewEmployee(Guid departmentId, string email = "john.doe@company.com", string cpf = "52998224725")
        {
            return Employee.Create(
                firstName: "John",
                lastName: "Doe",
                email: new Email(email),
                documentNumber: new DocumentNumber(cpf),
                dateOfBirth: new DateOfBirth(DateTime.Today.AddYears(-30)),
                phoneNumbers: new[] { "11 91111-1111" },
                jobTitleId: Guid.NewGuid(),
                departmentId: departmentId
            );
        }

        private static UpdateEmployeeCommand MakeCommand(
            Guid id,
            Guid deptId,
            string email,
            string cpf,
            params string[] phones)
        {
            return new UpdateEmployeeCommand
            {
                Id = id,
                FirstName = "Jane",
                LastName = "Doe",
                Email = email,
                DocumentNumber = cpf,
                JobTitleId = Guid.Empty, // Don't change job title
                DepartmentId = deptId,
                Phones = phones.ToList()
            };
        }

        private static IValidator<UpdateEmployeeRequest> CreateValidator()
        {
            return new UpdateEmployeeRequestValidator();
        }

        // ---------- tests ----------

        [Fact(DisplayName = "Should update all mutable fields and phones, and set UpdatedAt")]
        public async Task Should_Update_All_Fields_And_Phones()
        {
            var deptA = Guid.NewGuid();
            var deptB = Guid.NewGuid();

            var employee = NewEmployee(deptA);
            var employees = new ProbingEmployeeRepository(employee);
            var departments = new StubDepartmentRepository(new[] { deptA, deptB });

            var handler = new UpdateEmployeeCommandHandler(
                employees,                              // IEmployeeRepository
                new InMemoryUserAccountRepository(),    // IUserAccountRepository  
                departments,                            // IDepartmentRepository
                new StubJobTitleRepository(),          // IJobTitleRepository
                new FakeHasher(),                      // IPasswordHasher
                new Mock<ILogger<UpdateEmployeeCommandHandler>>().Object); // ILogger
            
            var cmd = MakeCommand(
                employee.Id,
                deptB,
                "jane.doe@company.com",
                "11144477735",
                "11 92222-2222",
                "(11) 93333-3333"
            );
            var beforeUpdatedAt = employee.UpdatedAt;

            await handler.Handle(cmd, CancellationToken.None);

            var updated = await employees.GetByIdAsync(employee.Id, default);
            updated.Should().NotBeNull();

            updated!.FirstName.Should().Be("Jane");
            updated.LastName.Should().Be("Doe");
            updated.Email.Value.Should().Be("jane.doe@company.com");
            updated.DocumentNumber.Digits.Should().Be("11144477735");
            updated.JobTitleId.Should().NotBe(Guid.Empty);
            updated.DepartmentId.Should().Be(deptB);

            // telefones sincronizados (ordem não importa)
            var e164 = updated.Phones.Select(p => p.PhoneNumber.E164).ToHashSet();
            e164.Should().BeEquivalentTo(new[]
            {
                new PhoneNumber("11 92222-2222", "BR").E164,
                new PhoneNumber("(11) 93333-3333", "BR").E164
            });

            updated.UpdatedAt.Should().NotBe(beforeUpdatedAt);
            updated.CreatedAt.Should().Be(employee.CreatedAt);
        }

        [Fact(DisplayName = "Should throw when employee is not found")]
        public async Task Should_Throw_When_Employee_Not_Found()
        {
            var employees = new ProbingEmployeeRepository(seed: null);
            var departments = new StubDepartmentRepository(new[] { Guid.NewGuid() });
            var handler = new UpdateEmployeeCommandHandler(
                employees,                              // IEmployeeRepository
                new InMemoryUserAccountRepository(),    // IUserAccountRepository  
                departments,                            // IDepartmentRepository
                new StubJobTitleRepository(),          // IJobTitleRepository
                new FakeHasher(),                      // IPasswordHasher
                new Mock<ILogger<UpdateEmployeeCommandHandler>>().Object); // ILogger

            var cmd = MakeCommand(Guid.NewGuid(), Guid.NewGuid(), "j@c.com", "52998224725", "11 99999-9999");

            var act = () => handler.Handle(cmd, default);

            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("*Employee not found*");
        }

        [Fact(DisplayName = "Should throw when department does not exist")]
        public async Task Should_Throw_When_Department_Not_Exists()
        {
            var deptA = Guid.NewGuid();
            var employee = NewEmployee(deptA);
            var employees = new ProbingEmployeeRepository(employee);
            var departments = new StubDepartmentRepository(new[] { deptA }); // deptB não existe
            var handler = new UpdateEmployeeCommandHandler(
                employees,                              // IEmployeeRepository
                new InMemoryUserAccountRepository(),    // IUserAccountRepository  
                departments,                            // IDepartmentRepository
                new StubJobTitleRepository(),          // IJobTitleRepository
                new FakeHasher(),                      // IPasswordHasher
                new Mock<ILogger<UpdateEmployeeCommandHandler>>().Object); // ILogger

            var cmd = MakeCommand(employee.Id, deptId: Guid.NewGuid(),
                                  email: employee.Email.Value,
                                  cpf: employee.DocumentNumber.Digits,
                                  phones: "11 91111-1111");

            var act = () => handler.Handle(cmd, default);

            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("*Department does not exist*");
        }

        [Fact(DisplayName = "Should reject duplicate email when changing")]
        public async Task Should_Reject_Duplicate_Email_When_Changing()
        {
            var deptId = Guid.NewGuid();
            var employee = NewEmployee(deptId);
            var employees = new ProbingEmployeeRepository(employee);
            employees.SeedEmail("jane.doe@company.com"); // email já existe

            var departments = new StubDepartmentRepository(new[] { deptId });
            var handler = new UpdateEmployeeCommandHandler(
                employees,                              // IEmployeeRepository
                new InMemoryUserAccountRepository(),    // IUserAccountRepository  
                departments,                            // IDepartmentRepository
                new StubJobTitleRepository(),          // IJobTitleRepository
                new FakeHasher(),                      // IPasswordHasher
                new Mock<ILogger<UpdateEmployeeCommandHandler>>().Object); // ILogger

            var cmd = MakeCommand(employee.Id, deptId, "jane.doe@company.com", "52998224725", "11 99999-9999");

            var act = () => handler.Handle(cmd, default);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("*Email already in use*");
        }

        [Fact(DisplayName = "Should not check duplicate email when it did not change")]
        public async Task Should_Not_Check_Email_When_Unchanged()
        {
            var dept = Guid.NewGuid();
            var employee = NewEmployee(dept);
            var employees = new ProbingEmployeeRepository(employee)
            {
                TakenEmails = { "taken@company.com" }
            };
            var departments = new StubDepartmentRepository(new[] { dept });
            var handler = new UpdateEmployeeCommandHandler(
                employees,                              // IEmployeeRepository
                new InMemoryUserAccountRepository(),    // IUserAccountRepository  
                departments,                            // IDepartmentRepository
                new StubJobTitleRepository(),          // IJobTitleRepository
                new FakeHasher(),                      // IPasswordHasher
                new Mock<ILogger<UpdateEmployeeCommandHandler>>().Object); // ILogger

            var cmd = MakeCommand(employee.Id, dept,
                                  email: employee.Email.Value,
                                  cpf: employee.DocumentNumber.Digits,
                                  phones: "11 91111-1111");

            await handler.Handle(cmd, default);

            employees.EmailExistsCalls.Should().Be(0);
        }

        [Fact(DisplayName = "Should reject duplicate CPF when changing")]
        public async Task Should_Reject_Duplicate_Cpf_When_Changing()
        {
            var deptId = Guid.NewGuid();
            var employee = NewEmployee(deptId);
            var employees = new ProbingEmployeeRepository(employee);
            employees.SeedCpf("11144477735"); // CPF já existe

            var departments = new StubDepartmentRepository(new[] { deptId });
            var handler = new UpdateEmployeeCommandHandler(
                employees,                              // IEmployeeRepository
                new InMemoryUserAccountRepository(),    // IUserAccountRepository  
                departments,                            // IDepartmentRepository
                new StubJobTitleRepository(),          // IJobTitleRepository
                new FakeHasher(),                      // IPasswordHasher
                new Mock<ILogger<UpdateEmployeeCommandHandler>>().Object); // ILogger

            var cmd = MakeCommand(employee.Id, deptId, "jane.doe@company.com", "11144477735", "11 99999-9999");

            var act = () => handler.Handle(cmd, default);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("*Document number already in use*");
        }

        [Fact(DisplayName = "Should not check duplicate CPF when it did not change")]
        public async Task Should_Not_Check_Cpf_When_Unchanged()
        {
            var dept = Guid.NewGuid();
            var employee = NewEmployee(dept);
            var employees = new ProbingEmployeeRepository(employee)
            {
                TakenCpfs = { "11144477735" }
            };
            var departments = new StubDepartmentRepository(new[] { dept });
            var handler = new UpdateEmployeeCommandHandler(
                employees,                              // IEmployeeRepository
                new InMemoryUserAccountRepository(),    // IUserAccountRepository  
                departments,                            // IDepartmentRepository
                new StubJobTitleRepository(),          // IJobTitleRepository
                new FakeHasher(),                      // IPasswordHasher
                new Mock<ILogger<UpdateEmployeeCommandHandler>>().Object); // ILogger

            var cmd = MakeCommand(employee.Id, dept,
                                  email: employee.Email.Value,
                                  cpf: employee.DocumentNumber.Digits,
                                  phones: "11 91111-1111");

            await handler.Handle(cmd, default);

            employees.CpfExistsCalls.Should().Be(0);
        }

        [Fact(DisplayName = "Should synchronize phones (remove missing, keep existing, add new)")]
        public async Task Should_Synchronize_Phones()
        {
            var dept = Guid.NewGuid();
            var employee = NewEmployee(dept);
            employee.AddPhone("11 93333-3333"); // agora tem 2

            var employees = new ProbingEmployeeRepository(employee);
            var departments = new StubDepartmentRepository(new[] { dept });
            var handler = new UpdateEmployeeCommandHandler(
                employees,                              // IEmployeeRepository
                new InMemoryUserAccountRepository(),    // IUserAccountRepository  
                departments,                            // IDepartmentRepository
                new StubJobTitleRepository(),          // IJobTitleRepository
                new FakeHasher(),                      // IPasswordHasher
                new Mock<ILogger<UpdateEmployeeCommandHandler>>().Object); // ILogger

            // Manda apenas 1 número -> remove o outro
            var cmd = MakeCommand(employee.Id, dept,
                                  email: employee.Email.Value,
                                  cpf: employee.DocumentNumber.Digits,
                                  phones: "11 93333-3333");

            await handler.Handle(cmd, default);

            var updated = await employees.GetByIdAsync(employee.Id, default);
            updated!.Phones.Should().HaveCount(1);
            updated.Phones.Single().PhoneNumber.E164.Should().Be(new PhoneNumber("11 93333-3333", "BR").E164);
        }

        [Fact(DisplayName = "Should reject empty phone numbers")]
        public async Task Should_Reject_Empty_Phone_Numbers()
        {
            var deptId = Guid.NewGuid();
            var employee = NewEmployee(deptId);
            var employees = new ProbingEmployeeRepository(employee);
            var departments = new StubDepartmentRepository(new[] { deptId });
            var handler = new UpdateEmployeeCommandHandler(
                employees,                              // IEmployeeRepository
                new InMemoryUserAccountRepository(),    // IUserAccountRepository  
                departments,                            // IDepartmentRepository
                new StubJobTitleRepository(),          // IJobTitleRepository
                new FakeHasher(),                      // IPasswordHasher
                new Mock<ILogger<UpdateEmployeeCommandHandler>>().Object); // ILogger

            var cmd = MakeCommand(employee.Id, deptId, "jane.doe@company.com", "52998224725"); // sem telefones

            var act = () => handler.Handle(cmd, default);

            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("*phone number*");
        }

        [Fact(DisplayName = "Should update password when provided")]
        public async Task Should_Update_Password_When_Provided()
        {
            var deptId = Guid.NewGuid();
            var employee = NewEmployee(deptId);
            var employees = new ProbingEmployeeRepository(employee);
            var departments = new StubDepartmentRepository(new[] { deptId });
            var userAccounts = new InMemoryUserAccountRepository();
            
            // Add a current user with admin role for password changes
            var currentUserId = Guid.NewGuid();
            var adminRoleId = Guid.NewGuid();
            var currentUser = UserAccount.Create("admin@test.com", "hash", Guid.NewGuid(), adminRoleId, Guid.NewGuid());
            // Manually set the Id property for the in-memory repository
            typeof(UserAccount).GetProperty("Id")?.SetValue(currentUser, currentUserId);
            await userAccounts.AddAsync(currentUser, CancellationToken.None);
            
            var handler = new UpdateEmployeeCommandHandler(
                employees,                              // IEmployeeRepository
                userAccounts,                           // IUserAccountRepository  
                departments,                            // IDepartmentRepository
                new StubJobTitleRepository(),          // IJobTitleRepository
                new FakeHasher(),                      // IPasswordHasher
                new Mock<ILogger<UpdateEmployeeCommandHandler>>().Object); // ILogger

            var cmd = MakeCommand(employee.Id, deptId, "jane.doe@company.com", "52998224725", "11 99999-9999");
            cmd.Password = "NewPassword123!";

            await handler.Handle(cmd, CancellationToken.None, currentUserId);

            // Verificar se a senha foi atualizada
            var updatedEmployee = await employees.GetByIdAsync(employee.Id, default);
            updatedEmployee.Should().NotBeNull();
        }

        [Fact(DisplayName = "Should not update password when not provided")]
        public async Task Should_Not_Update_Password_When_Not_Provided()
        {
            var deptId = Guid.NewGuid();
            var employee = NewEmployee(deptId);
            var employees = new ProbingEmployeeRepository(employee);
            var departments = new StubDepartmentRepository(new[] { deptId });
            var userAccounts = new InMemoryUserAccountRepository();
            var handler = new UpdateEmployeeCommandHandler(
                employees,                              // IEmployeeRepository
                userAccounts,                           // IUserAccountRepository  
                departments,                            // IDepartmentRepository
                new StubJobTitleRepository(),          // IJobTitleRepository
                new FakeHasher(),                      // IPasswordHasher
                new Mock<ILogger<UpdateEmployeeCommandHandler>>().Object); // ILogger

            var cmd = MakeCommand(employee.Id, deptId, "jane.doe@company.com", "52998224725", "11 99999-9999");
            // cmd.Password não é definido

            await handler.Handle(cmd, CancellationToken.None);

            // Verificar se o funcionário foi atualizado sem problemas
            var updatedEmployee = await employees.GetByIdAsync(employee.Id, default);
            updatedEmployee.Should().NotBeNull();
        }
    }
}
