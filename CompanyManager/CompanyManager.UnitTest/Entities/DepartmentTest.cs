using CompanyManager.Domain.Entities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyManager.UnitTest.Entities
{
    public class DepartmentTest
    {
        [Fact(DisplayName = "Should create active department with valid name")]
        public void Should_Create_Department()
        {
            var d = Department.Create("Engineering");

            d.Id.Should().NotBeEmpty();
            d.Name.Should().Be("Engineering");
            d.IsActive.Should().BeTrue();
            d.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
            d.UpdatedAt.Should().BeNull();
        }

        [Theory(DisplayName = "Should trim name on creation")]
        [InlineData("  Engineering  ", "Engineering")]
        [InlineData("\tHR\n", "HR")]
        public void Should_Trim_Name_On_Create(string input, string expected)
        {
            var d = Department.Create(input);
            d.Name.Should().Be(expected);
        }

        [Theory(DisplayName = "Should reject invalid department names (min length 2)")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\n")]
        [InlineData("A")]
        public void Should_Reject_Invalid_Names(string name)
        {
            Action act = () => Department.Create(name);
            act.Should().Throw<ArgumentException>()
               .WithMessage("*department name*");
        }

        [Fact(DisplayName = "Should rename and update UpdatedAt")]
        public void Should_Rename_And_Update_Timestamp()
        {
            var d = Department.Create("Engineering");
            var before = d.UpdatedAt;

            d.Rename("Tech");

            d.Name.Should().Be("Tech");
            d.UpdatedAt.Should().NotBe(before);
            d.UpdatedAt.Should().NotBeNull();
        }

        [Theory(DisplayName = "Should reject invalid names on rename")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("A")]
        public void Should_Reject_Invalid_Rename(string newName)
        {
            var d = Department.Create("Engineering");
            Action act = () => d.Rename(newName);
            act.Should().Throw<ArgumentException>()
               .WithMessage("*department name*");
        }

        [Fact(DisplayName = "Should deactivate and then activate, updating UpdatedAt each time")]
        public void Should_Deactivate_And_Activate()
        {
            var d = Department.Create("Engineering");

            d.Deactivate();
            d.IsActive.Should().BeFalse();
            var afterDeactivate = d.UpdatedAt;

            d.Activate();
            d.IsActive.Should().BeTrue();
            d.UpdatedAt.Should().NotBe(afterDeactivate);
        }

        [Fact(DisplayName = "Should be idempotent when calling Activate/Deactivate repeatedly")]
        public void Should_Be_Idempotent_On_State_Toggles()
        {
            var d = Department.Create("Engineering");

            d.Deactivate();
            var firstChange = d.UpdatedAt;
            d.Deactivate();
            d.UpdatedAt.Should().Be(firstChange);

            d.Activate();
            var secondChange = d.UpdatedAt;
            d.Activate();
            d.UpdatedAt.Should().Be(secondChange);
        }
    }
}
