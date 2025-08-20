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

            // Validar idade mínima de 18 anos
            var age = CalculateAge(date, today);
            if (age < 18)
                throw new ArgumentOutOfRangeException(nameof(value), "Person must be at least 18 years old.");

            BirthDate = date;
        }

        public int AgeInYears(DateTime on)
        {
            return CalculateAge(BirthDate, on.Date);
        }

        private static int CalculateAge(DateTime birthDate, DateTime referenceDate)
        {
            var age = referenceDate.Year - birthDate.Year;
            if (referenceDate < birthDate.AddYears(age)) 
                age--;
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
