using CompanyManager.Domain.ValueObjects;
using FluentAssertions;
using System.Globalization;

namespace CompanyManager.UnitTest.ValueObjects;

public sealed class DateOfBirthTest
{
    [Theory(DisplayName = "Should accept past dates for adults (18+ years)")]
    [InlineData(18)]
    [InlineData(25)]
    [InlineData(40)]
    [InlineData(100)]
    public void Should_Accept_Past_Dates_For_Adults(int years)
    {
        var date = DateTime.Today.AddYears(-years);

        var dob = new DateOfBirth(date);

        dob.Should().NotBeNull();
        dob.BirthDate.Should().Be(date);
    }

    [Theory(DisplayName = "Should reject dates for minors (under 18 years)")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(17)]
    public void Should_Reject_Dates_For_Minors(int years)
    {
        var date = DateTime.Today.AddYears(-years);

        Action act = () => new DateOfBirth(date);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*must be at least 18 years old*");
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
        var leap = new DateTime(2000, 2, 29);
        var dob = new DateOfBirth(leap);

        dob.Should().NotBeNull();
        dob.BirthDate.Should().Be(leap);
    }

    [Fact(DisplayName = "Should handle month boundary (yesterday vs today) for adults")]
    public void Should_Handle_Month_Boundaries()
    {
        var yesterday = DateTime.Today.AddYears(-20).AddDays(-1);
        var today = DateTime.Today.AddYears(-20);

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
