using CompanyManager.Application.DTOs;
using CompanyManager.Application.Validators;
using FluentAssertions;

namespace CompanyManager.UnitTest.Application.Validators
{
    public sealed class CreateDepartmentRequestValidatorTest
    {
        private static CreateDepartmentRequest Valid()
        {
            return new CreateDepartmentRequest
            {
                Name = "Engineering"
            };
        }

        [Fact(DisplayName = "Should accept a fully valid request")]
        public void Should_Accept_Valid_Request()
        {
            var req = Valid();

            var v = new CreateDepartmentRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeTrue(r.ToString());
        }

        [Theory(DisplayName = "Should trim and validate department name (min length 2)")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("E")]
        [InlineData("  ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void Should_Reject_Invalid_Names(string name)
        {
            var req = new CreateDepartmentRequest { Name = name };

            var v = new CreateDepartmentRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeFalse();
            r.Errors.Should().Contain(e => e.PropertyName == nameof(CreateDepartmentRequest.Name));
        }

        [Theory(DisplayName = "Should accept valid department names")]
        [InlineData("Engineering")]
        [InlineData("Human Resources")]
        [InlineData("IT")]
        [InlineData("Sales")]
        [InlineData("Marketing")]
        public void Should_Accept_Valid_Names(string name)
        {
            var req = new CreateDepartmentRequest { Name = name };

            var v = new CreateDepartmentRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeTrue(r.ToString());
        }

        [Theory(DisplayName = "Should trim fields (no false negatives)")]
        [InlineData("  Engineering  ")]
        [InlineData("  Human Resources  ")]
        [InlineData("  IT  ")]
        public void Should_Trim_Fields(string name)
        {
            var req = new CreateDepartmentRequest { Name = name };

            var v = new CreateDepartmentRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeTrue(r.ToString());
        }
    }
}
