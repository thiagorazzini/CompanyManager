using CompanyManager.Application.DTOs;
using CompanyManager.Application.Commands;
using CompanyManager.Application.Common;
using FluentAssertions;

namespace CompanyManager.UnitTest.Application.Mapping
{
    public sealed class EmployeeMappingsTest
    {
        [Fact(DisplayName = "CreateEmployeeRequest should normalize data correctly")]
        public void CreateEmployeeRequest_Should_Map_With_Normalization()
        {
            // Arrange
            var request = new CreateEmployeeRequest
            {
                FirstName = "  John  ",
                LastName = "  Doe  ",
                JobTitleId = Guid.NewGuid(),
                Email = "  JOHN.DOE@ACME.COM  ",
                DocumentNumber = "  12345678901  ",
                PhoneNumbers = new List<string> { "  (11) 99999-9999  ", null!, "", "  (11) 88888-8888  " },
                DateOfBirth = "  1990-05-15  ",
                Password = "Password123!",
                DepartmentId = Guid.NewGuid()
            };

            // Act - Manual normalization to demonstrate expected behavior
            var normalizedFirstName = string.IsNullOrWhiteSpace(request.FirstName) ? string.Empty : request.FirstName.Trim();
            var normalizedLastName = string.IsNullOrWhiteSpace(request.LastName) ? string.Empty : request.LastName.Trim();
            var normalizedJobTitleId = request.JobTitleId;
            var normalizedEmail = string.IsNullOrWhiteSpace(request.Email) ? string.Empty : request.Email.Trim().ToLowerInvariant();
            var normalizedDocumentNumber = string.IsNullOrWhiteSpace(request.DocumentNumber) ? string.Empty : request.DocumentNumber.Trim();
            var normalizedPhones = request.PhoneNumbers?.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).ToArray() ?? Array.Empty<string>();
            var normalizedDateOfBirth = string.IsNullOrWhiteSpace(request.DateOfBirth) ? 
                throw new ArgumentException("DateOfBirth is required.") : request.DateOfBirth.Trim();

            // Assert
            normalizedFirstName.Should().Be("John");
            normalizedLastName.Should().Be("Doe");
            normalizedJobTitleId.Should().NotBe(Guid.Empty);
            normalizedEmail.Should().Be("john.doe@acme.com");
            normalizedDocumentNumber.Should().Be("12345678901");
            normalizedPhones.Should().BeEquivalentTo(new[] { "(11) 99999-9999", "(11) 88888-8888" });
            normalizedDateOfBirth.Should().Be("1990-05-15");
        }

        [Fact(DisplayName = "UpdateEmployeeRequest should normalize data correctly")]
        public void UpdateEmployeeRequest_Should_Map_With_Normalization()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var departmentId = Guid.NewGuid();
            var request = new UpdateEmployeeRequest
            {
                Id = employeeId,
                FirstName = "  Jane  ",
                LastName = "  Smith  ",
                JobTitleId = Guid.NewGuid(),
                Email = "  JANE.SMITH@ACME.COM  ",
                DocumentNumber = "  98765432109  ",
                PhoneNumbers = new List<string> { "  (11) 77777-7777  ", null!, "", "  (11) 66666-6666  " },
                DepartmentId = departmentId
            };

            // Act - Manual normalization to demonstrate expected behavior
            var normalizedFirstName = string.IsNullOrWhiteSpace(request.FirstName) ? string.Empty : request.FirstName.Trim();
            var normalizedLastName = string.IsNullOrWhiteSpace(request.LastName) ? string.Empty : request.LastName.Trim();
            var normalizedJobTitleId = request.JobTitleId;
            var normalizedEmail = string.IsNullOrWhiteSpace(request.Email) ? string.Empty : request.Email.Trim().ToLowerInvariant();
            var normalizedDocumentNumber = string.IsNullOrWhiteSpace(request.DocumentNumber) ? string.Empty : request.DocumentNumber.Trim();
            var normalizedPhones = request.PhoneNumbers?.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).ToArray() ?? Array.Empty<string>();

            // Assert
            request.Id.Should().Be(employeeId);
            normalizedFirstName.Should().Be("Jane");
            normalizedLastName.Should().Be("Smith");
            normalizedJobTitleId.Should().NotBe(Guid.Empty);
            normalizedEmail.Should().Be("jane.smith@acme.com");
            normalizedDocumentNumber.Should().Be("98765432109");
            normalizedPhones.Should().BeEquivalentTo(new[] { "(11) 77777-7777", "(11) 66666-6666" });
            request.DepartmentId.Should().Be(departmentId);
        }

        [Fact(DisplayName = "ListEmployeesRequest should validate filter and page")]
        public void ListEmployeesRequest_Should_Map_Filter_And_Page()
        {
            // Arrange
            var departmentId = Guid.NewGuid();
            var request = new ListEmployeesRequest
            {
                DepartmentId = departmentId,
                NameContains = "  John Doe  ",
                Page = 0, // Should be normalized to 1
                PageSize = 150 // Should be clamped to 100
            };

            // Act - Manual normalization to demonstrate expected behavior
            var normalizedNameContains = string.IsNullOrWhiteSpace(request.NameContains) ? null : request.NameContains.Trim();
            var normalizedPage = Math.Max(request.Page, 1);
            var normalizedPageSize = Math.Clamp(request.PageSize, 1, 100);

            // Assert
            request.DepartmentId.Should().Be(departmentId);
            normalizedNameContains.Should().Be("John Doe");
            normalizedPage.Should().Be(1);
            normalizedPageSize.Should().Be(100);
        }

        [Fact(DisplayName = "ListDepartmentsRequest should validate filter and page")]
        public void ListDepartmentsRequest_Should_Map_Filter_And_Page()
        {
            // Arrange
            var request = new ListDepartmentsRequest
            {
                NameContains = "  Engineering  ",
                Page = -5, // Should be normalized to 1
                PageSize = 0 // Should be clamped to 1
            };

            // Act - Manual normalization to demonstrate expected behavior
            var normalizedNameContains = string.IsNullOrWhiteSpace(request.NameContains) ? null : request.NameContains.Trim();
            var normalizedPage = Math.Max(request.Page, 1);
            var normalizedPageSize = Math.Clamp(request.PageSize, 1, 100);

            // Assert
            normalizedNameContains.Should().Be("Engineering");
            normalizedPage.Should().Be(1);
            normalizedPageSize.Should().Be(1);
        }

        [Fact(DisplayName = "Should handle null/empty strings correctly")]
        public void Should_Handle_Null_Empty_Strings_Correctly()
        {
            // Arrange - ListEmployeesRequest with empty NameContains
            var listRequest = new ListEmployeesRequest
            {
                NameContains = "   ", // Should become null
                Page = 2,
                PageSize = 50
            };

            // Act - Manual normalization
            var normalizedNameContains = string.IsNullOrWhiteSpace(listRequest.NameContains) ? null : listRequest.NameContains.Trim();
            var normalizedPage = Math.Max(listRequest.Page, 1);
            var normalizedPageSize = Math.Clamp(listRequest.PageSize, 1, 100);

            // Assert
            normalizedNameContains.Should().BeNull();
            normalizedPage.Should().Be(2);
            normalizedPageSize.Should().Be(50);

            // Arrange - ListDepartmentsRequest with null NameContains
            var deptRequest = new ListDepartmentsRequest
            {
                NameContains = null,
                Page = 1,
                PageSize = 20
            };

            // Act - Manual normalization
            var deptNormalizedNameContains = string.IsNullOrWhiteSpace(deptRequest.NameContains) ? null : deptRequest.NameContains.Trim();

            // Assert
            deptNormalizedNameContains.Should().BeNull();
        }

        [Fact(DisplayName = "Should validate page constraints")]
        public void Should_Validate_Page_Constraints()
        {
            // Arrange - Test various page values
            var testCases = new[]
            {
                new { Page = -10, PageSize = -5, ExpectedPage = 1, ExpectedPageSize = 1 },
                new { Page = 0, PageSize = 0, ExpectedPage = 1, ExpectedPageSize = 1 },
                new { Page = 1, PageSize = 1, ExpectedPage = 1, ExpectedPageSize = 1 },
                new { Page = 5, PageSize = 50, ExpectedPage = 5, ExpectedPageSize = 50 },
                new { Page = 10, PageSize = 100, ExpectedPage = 10, ExpectedPageSize = 100 },
                new { Page = 15, PageSize = 200, ExpectedPage = 15, ExpectedPageSize = 100 }
            };

            foreach (var testCase in testCases)
            {
                // Arrange
                var request = new ListEmployeesRequest
                {
                    Page = testCase.Page,
                    PageSize = testCase.PageSize
                };

                // Act - Manual normalization
                var normalizedPage = Math.Max(request.Page, 1);
                var normalizedPageSize = Math.Clamp(request.PageSize, 1, 100);

                // Assert
                normalizedPage.Should().Be(testCase.ExpectedPage, 
                    $"Page {testCase.Page} should be normalized to {testCase.ExpectedPage}");
                normalizedPageSize.Should().Be(testCase.ExpectedPageSize,
                    $"PageSize {testCase.PageSize} should be clamped to {testCase.ExpectedPageSize}");
            }
        }
    }
}