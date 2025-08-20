using CompanyManager.Domain.Entities;
using CompanyManager.Domain.ValueObjects;
using FluentAssertions;

namespace CompanyManager.UnitTest.Entities
{
    public class EmployeeTest
    {
        // ---------- helpers ----------
        private static Email EmailOf(string v = "john.doe@company.com") => new(v);
        private static DocumentNumber Cpf(string v = "52998224725") => new(v); // valid CPF
        private static DateOfBirth DobYearsAgo(int years = 25) => new(DateTime.Today.AddYears(-years));
        private static PhoneNumber BrMobile(string v = "11999999999") => new(v, defaultCountry: "BR");
        private static Guid Dept() => Guid.NewGuid();

        private static Employee NewEmployee(
            string first = "John",
            string last = "Doe",
            string email = "john.doe@company.com",
            string cpf = "52998224725",
            int ageYears = 25,
            string phone = "11999999999",
            string jobTitle = "Developer",
            Guid? managerId = null)
        {
            return Employee.Create(
                first, last,
                new Email(email),
                new DocumentNumber(cpf),
                new DateOfBirth(DateTime.Today.AddYears(-ageYears)),
                new[] { new PhoneNumber(phone, defaultCountry: "BR") },
                jobTitle,
                Guid.NewGuid(),
                managerId);
        }

        // ---------- creation ----------

        [Fact(DisplayName = "Should create employee with required value objects")]
        public void Should_Create_Employee()
        {
            var emp = NewEmployee();

            emp.Id.Should().NotBeEmpty();
            emp.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
            emp.UpdatedAt.Should().BeNull();
            emp.FirstName.Should().Be("John");
            emp.LastName.Should().Be("Doe");
            emp.Email.Value.Should().Be("john.doe@company.com");
            emp.DocumentNumber.Digits.Should().Be("52998224725");
            emp.DateOfBirth.AgeInYears(DateTime.Today).Should().BeGreaterThan(0);
            emp.Phones.Should().ContainSingle();
            emp.JobTitle.Should().Be("Developer");
            emp.DepartmentId.Should().NotBe(Guid.Empty);
            emp.ManagerId.Should().BeNull();
            emp.HasManager.Should().BeFalse();
            emp.HierarchyLevel.Should().Be(0);
        }

        [Fact(DisplayName = "Should create employee with manager")]
        public void Should_Create_Employee_With_Manager()
        {
            var managerId = Guid.NewGuid();
            var emp = NewEmployee(managerId: managerId);

            emp.ManagerId.Should().Be(managerId);
            emp.HasManager.Should().BeTrue();
            emp.HierarchyLevel.Should().Be(1);
        }

        [Theory(DisplayName = "Should trim and validate names (min length 2)")]
        [InlineData("", "Doe")]
        [InlineData("  ", "Doe")]
        [InlineData("J", "Doe")]
        [InlineData("John", "")]
        [InlineData("John", " ")]
        [InlineData("John", "D")]
        public void Should_Reject_Invalid_Names(string first, string last)
        {
            Action act = () => Employee.Create(
                first, last, EmailOf(), Cpf(), DobYearsAgo(), new[] { BrMobile() }, jobTitle: "Developer", Dept());

            act.Should().Throw<ArgumentException>().WithMessage("*name*");
        }

        [Fact(DisplayName = "Should require at least one phone at creation")]
        public void Should_Require_AtLeast_One_Phone()
        {
            Action act = () => Employee.Create(
                "John", "Doe", EmailOf(), Cpf(), DobYearsAgo(), Array.Empty<PhoneNumber>(), jobTitle: "Developer", Dept());

            act.Should().Throw<ArgumentException>().WithMessage("*phone*");
        }

        // ---------- phones ----------

        [Fact(DisplayName = "Should not allow duplicated phones in the aggregate")]
        public void Should_Not_Allow_Duplicated_Phones()
        {
            var emp = NewEmployee();
            var sameNumberDifferentMask = new PhoneNumber("+55 11 99999-9999");

            Action act = () => emp.AddPhone(sameNumberDifferentMask);

            act.Should().Throw<InvalidOperationException>().WithMessage("*duplicate*");
            emp.Phones.Should().HaveCount(1);
        }

        [Fact(DisplayName = "Should not remove the last phone")]
        public void Should_Not_Remove_Last_Phone()
        {
            var emp = NewEmployee();
            var only = emp.Phones.Single();

            Action act = () => emp.RemovePhone(only);

            act.Should().Throw<InvalidOperationException>().WithMessage("*at least one phone*");
            emp.Phones.Should().ContainSingle();
        }

        [Fact(DisplayName = "Should add and remove phones updating UpdatedAt")]
        public void Should_Add_Remove_Phones_And_Update_Timestamps()
        {
            var emp = NewEmployee();
            var before = emp.UpdatedAt;

            var another = new PhoneNumber("11987654321", defaultCountry: "BR");
            emp.AddPhone(another);
            emp.Phones.Should().HaveCount(2);
            emp.UpdatedAt.Should().NotBe(before);

            before = emp.UpdatedAt;
            emp.RemovePhone(another);
            emp.Phones.Should().HaveCount(1);
            emp.UpdatedAt.Should().NotBe(before);
        }

        // ---------- name / role / department ----------

        [Fact(DisplayName = "Should change name (trim) and update UpdatedAt")]
        public void Should_Change_Name_And_Update_Timestamp()
        {
            var emp = NewEmployee();
            var before = emp.UpdatedAt;

            emp.ChangeName("  Jane ", "  Smith ");
            emp.FirstName.Should().Be("Jane");
            emp.LastName.Should().Be("Smith");
            emp.UpdatedAt.Should().NotBe(before);
        }

        [Fact(DisplayName = "Should change department and update UpdatedAt")]
        public void Should_Change_Department()
        {
            var emp = NewEmployee();
            var before = emp.UpdatedAt;
            var newDept = Guid.NewGuid();

            emp.ChangeDepartment(newDept);

            emp.DepartmentId.Should().Be(newDept);
            emp.UpdatedAt.Should().NotBe(before);
        }

        [Fact(DisplayName = "Should expose full name")]
        public void Should_Expose_FullName()
        {
            var emp = NewEmployee("Jane", "Smith");
            emp.FullName.Should().Be("Jane Smith");
        }

        [Fact(DisplayName = "Should reject empty department id")]
        public void Should_Reject_Empty_Department()
        {
            var emp = NewEmployee();
            Action act = () => emp.ChangeDepartment(Guid.Empty);
            act.Should().Throw<ArgumentException>().WithMessage("*department*");
        }

        [Fact(DisplayName = "Should change job title and update UpdatedAt")]
        public void Should_Change_JobTitle()
        {
            var emp = NewEmployee(jobTitle: "Developer");
            var before = emp.UpdatedAt;

            emp.ChangeJobTitle("Senior Developer");

            emp.JobTitle.Should().Be("Senior Developer");
            emp.UpdatedAt.Should().NotBe(before);
        }
    }
}
