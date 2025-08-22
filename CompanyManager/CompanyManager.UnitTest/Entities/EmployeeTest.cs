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
            string firstName = "John",
            string lastName = "Doe",
            string email = "john.doe@company.com",
            string documentNumber = "12345678901",
            DateTime? dateOfBirth = null,
            string[] phones = null!)
        {
            var dob = dateOfBirth ?? new DateTime(1990, 1, 1);
            var phoneNumbers = phones ?? new[] { "+5511999999999" };
            
            return Employee.Create(
                firstName,
                lastName,
                new Email(email),
                new DocumentNumber(documentNumber),
                new DateOfBirth(dob),
                phoneNumbers,
                Guid.NewGuid(),
                Guid.NewGuid());
        }

        // ---------- creation ----------

        [Fact]
        public void Create_ValidData_ShouldCreateEmployeeWithoutManager()
        {
            // Arrange & Act
            var emp = NewEmployee();

            // Assert
            emp.Should().NotBeNull();
            emp.FirstName.Should().Be("John");
            emp.LastName.Should().Be("Doe");
            emp.Email.Value.Should().Be("john.doe@company.com");
            emp.DocumentNumber.Raw.Should().Be("12345678901");
            emp.DateOfBirth.BirthDate.Should().Be(new DateTime(1990, 1, 1));
            emp.Phones.Should().HaveCount(1);
            emp.Phones.First().PhoneNumber.E164.Should().Be("+5511999999999");
            emp.JobTitleId.Should().Be(Guid.NewGuid());
            emp.DepartmentId.Should().Be(Guid.NewGuid());
            emp.HasManager.Should().BeFalse();
            emp.HierarchyLevel.Should().Be(0);
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
                first, last, EmailOf(), Cpf(), DobYearsAgo(), new[] { "11999999999" }, Guid.NewGuid(), Dept());

            act.Should().Throw<ArgumentException>().WithMessage("*name*");
        }

        [Fact(DisplayName = "Should require at least one phone at creation")]
        public void Should_Require_AtLeast_One_Phone()
        {
            Action act = () => Employee.Create(
                "John", "Doe", EmailOf(), Cpf(), DobYearsAgo(), Array.Empty<string>(), Guid.NewGuid(), Dept());

            act.Should().Throw<ArgumentException>().WithMessage("*phone*");
        }

        // ---------- phones ----------

        [Fact(DisplayName = "Should not allow duplicated phones in the aggregate")]
        public void Should_Not_Allow_Duplicated_Phones()
        {
            var emp = NewEmployee();
            var sameNumberDifferentMask = new PhoneNumber("+55 11 99999-9999");

            Action act = () => emp.AddPhone(sameNumberDifferentMask.E164);

            act.Should().Throw<InvalidOperationException>().WithMessage("*duplicate*");
            emp.Phones.Should().HaveCount(1);
        }

        [Fact(DisplayName = "Should not remove the last phone")]
        public void Should_Not_Remove_Last_Phone()
        {
            var emp = NewEmployee();
            var only = emp.Phones.Single();

            Action act = () => emp.RemovePhone(only.Id);

            act.Should().Throw<InvalidOperationException>().WithMessage("*at least one phone*");
            emp.Phones.Should().ContainSingle();
        }

        [Fact(DisplayName = "Should add and remove phones updating UpdatedAt")]
        public void Should_Add_Remove_Phones_And_Update_Timestamps()
        {
            var emp = NewEmployee();
            var before = emp.UpdatedAt;

            var anotherPhoneNumber = "11987654321";
            emp.AddPhone(anotherPhoneNumber);
            emp.Phones.Should().HaveCount(2);
            emp.UpdatedAt.Should().NotBe(before);

            before = emp.UpdatedAt;
            var phoneToRemove = emp.Phones.First(p => p.PhoneNumber.E164.Contains("987654321"));
            emp.RemovePhone(phoneToRemove.Id);
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
            var emp = NewEmployee();
            var before = emp.UpdatedAt;

            emp.ChangeJobTitle(Guid.NewGuid());

            emp.JobTitleId.Should().NotBe(Guid.Empty);
            emp.UpdatedAt.Should().NotBe(before);
        }

        // ---------- hierarchy ----------
        
        [Fact]
        public void Hierarchy_ShouldAlwaysBeZero()
        {
            var emp = NewEmployee();
            emp.HierarchyLevel.Should().Be(0);
            emp.HasManager.Should().BeFalse();
        }

        [Fact]
        public void ChangeJobTitle_ShouldUpdateJobTitleId()
        {
            var emp = NewEmployee();
            var newJobTitleId = Guid.NewGuid();
            var beforeUpdatedAt = emp.UpdatedAt;

            emp.ChangeJobTitle(newJobTitleId);

            emp.JobTitleId.Should().Be(newJobTitleId);
            emp.UpdatedAt.Should().NotBe(beforeUpdatedAt);
        }
    }
}
