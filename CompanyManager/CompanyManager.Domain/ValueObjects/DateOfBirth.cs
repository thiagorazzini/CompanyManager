using System;
using System.Globalization;

namespace CompanyManager.Domain.ValueObjects
{
    public sealed class DateOfBirth : IEquatable<DateOfBirth>
    {
        public DateTime BirthDate { get; }

        public DateOfBirth(DateTime value)
        {
            var date = value.Date;       
            var today = DateTime.Today;

            if (date > today)
                throw new ArgumentOutOfRangeException(nameof(value), "Date of birth cannot be in the future.");

            BirthDate = date;
        }

        public int AgeInYears(DateTime on)
        {
            var onDate = on.Date;
            var age = onDate.Year - BirthDate.Year;
            if (onDate < BirthDate.AddYears(age)) age--;
            return age;
        }
        public override string ToString() =>
            BirthDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        public override bool Equals(object? obj) => obj is DateOfBirth other && Equals(other);
        public bool Equals(DateOfBirth? other) => other is not null && BirthDate == other.BirthDate;
        public override int GetHashCode() => BirthDate.GetHashCode();
        public static bool operator ==(DateOfBirth? left, DateOfBirth? right) => Equals(left, right);
        public static bool operator !=(DateOfBirth? left, DateOfBirth? right) => !Equals(left, right);
    }
}
