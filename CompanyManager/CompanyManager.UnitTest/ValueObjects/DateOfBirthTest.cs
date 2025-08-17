using FluentAssertions;
using CompanyManager.Domain.ValueObjects;
using System;
using System.Globalization;

namespace CompanyManager.UnitTest.ValueObjects
{
    public class DateOfBirthTest
    {
        [Theory(DisplayName = "Should accept past dates (any age) and today")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(17)]
        [InlineData(18)]
        [InlineData(25)]
        [InlineData(40)]
        [InlineData(100)]
        public void Should_Accept_Past_Dates_And_Today(int years)
        {
            // Arrange
            var date = DateTime.Today.AddYears(-years);

            // Act
            var dob = new DateOfBirth(date);

            // Assert
            dob.Should().NotBeNull();
            dob.BirthDate.Should().Be(date);
        }

        [Fact(DisplayName = "Should accept today's date")]
        public void Should_Accept_Today()
        {
            var today = DateTime.Today;
            var dob = new DateOfBirth(today);

            dob.Should().NotBeNull();
            dob.BirthDate.Should().Be(today);
        }

        [Fact(DisplayName = "Should throw for future date")]
        public void Should_Throw_For_Future_Date()
        {
            var future = DateTime.Today.AddDays(1);
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new DateOfBirth(future)); ex.Message.Should().Contain("in the future");
        }

        [Fact(DisplayName = "Should handle leap day (Feb 29) correctly")]
        public void Should_Handle_Leap_Day()
        {
            var leap = new DateTime(2012, 2, 29);
            var dob = new DateOfBirth(leap);

            dob.Should().NotBeNull();
            dob.BirthDate.Should().Be(leap);
        }

        [Fact(DisplayName = "Should handle month boundary (yesterday vs today)")]
        public void Should_Handle_Month_Boundaries()
        {
            var yesterday = DateTime.Today.AddDays(-1);
            var today = DateTime.Today;

            var dobY = new DateOfBirth(yesterday);
            var dobT = new DateOfBirth(today);

            dobY.BirthDate.Should().Be(yesterday);
            dobT.BirthDate.Should().Be(today);
        }

        [Fact(DisplayName = "Should implement equality by value")]
        public void Should_Implement_Equality()
        {
            var a = new DateOfBirth(DateTime.Today.AddYears(-25));
            var b = new DateOfBirth(DateTime.Today.AddYears(-25));
            var c = new DateOfBirth(DateTime.Today.AddYears(-30));

            a.Should().Be(b);
            a.Should().NotBe(c);
            a.Equals(b).Should().BeTrue();
            a.Equals(c).Should().BeFalse();
        }

        [Fact(DisplayName = "Should implement GetHashCode consistently")]
        public void Should_Implement_GetHashCode()
        {
            var a = new DateOfBirth(DateTime.Today.AddYears(-25));
            var b = new DateOfBirth(DateTime.Today.AddYears(-25));

            a.GetHashCode().Should().Be(b.GetHashCode());
        }

        [Fact(DisplayName = "Should implement ToString properly")]
        public void Should_Implement_ToString()
        {
            var value = DateTime.Today.AddYears(-25);
            var dob = new DateOfBirth(value);

            var text = dob.ToString();

            text.Should().Be(value.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }
    }
}
