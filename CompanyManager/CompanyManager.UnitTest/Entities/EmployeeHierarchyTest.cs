using FluentAssertions;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.ValueObjects;

namespace CompanyManager.UnitTest.Entities
{
    public class EmployeeHierarchyTest
    {
        [Fact]
        public void Hierarchy_ShouldAlwaysBeZero()
        {
            var emp = NewEmployee();
            emp.HierarchyLevel.Should().Be(0);
            emp.HasManager.Should().BeFalse();
        }

        private static Employee NewEmployee()
        {
            return Employee.Create(
                "John",
                "Doe",
                new Email("john.doe@company.com"),
                new DocumentNumber("12345678901"),
                new DateOfBirth(new DateTime(1990, 1, 1)),
                new[] { new PhoneNumber("+5511999999999") },
                Guid.NewGuid(),
                Guid.NewGuid());
        }
    }
}
