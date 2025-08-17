using System.Linq;
using System.Text.RegularExpressions;

namespace CompanyManager.Domain.ValueObjects
{
    public sealed class DocumentNumber : IEquatable<DocumentNumber>
    {
        private static readonly Regex CpfPlain = new(@"^\d{11}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex CpfFormatted = new(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public string Raw { get; }   
        public string Digits { get; } 

        public DocumentNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Document number cannot be null or empty", nameof(value));

            var trimmed = value.Trim();
            var isCpfFormat = CpfPlain.IsMatch(trimmed) || CpfFormatted.IsMatch(trimmed);
            if (!isCpfFormat)
                throw new ArgumentException("Invalid document number format", nameof(value));

            var digits = new string(trimmed.Where(char.IsDigit).ToArray());
            if (!IsValidCpfDigits(digits))
                throw new ArgumentException("Invalid document number format", nameof(value));

            Raw = trimmed;
            Digits = digits;
        }

        public override string ToString() => Raw;

        public string Formatted =>
            $"{Digits.Substring(0, 3)}.{Digits.Substring(3, 3)}.{Digits.Substring(6, 3)}-{Digits.Substring(9, 2)}";

        public override bool Equals(object? obj) => obj is DocumentNumber other && Equals(other);
        public bool Equals(DocumentNumber? other) => other is not null && StringComparer.Ordinal.Equals(Digits, other.Digits);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Digits);
        public static bool operator ==(DocumentNumber? left, DocumentNumber? right) => Equals(left, right);
        public static bool operator !=(DocumentNumber? left, DocumentNumber? right) => !Equals(left, right);

        private static bool IsValidCpfDigits(string digits)
        {
            if (digits.Length != 11) return false;
            if (AllDigitsEqual(digits)) return false;

            var dv1 = CalculateDigit(digits, startFactor: 10); 
            var dv2 = CalculateDigit(digits, startFactor: 11); 

            return digits[9] - '0' == dv1 && digits[10] - '0' == dv2;
        }

        private static bool AllDigitsEqual(string digits)
        {
            for (int i = 1; i < digits.Length; i++)
                if (digits[i] != digits[0]) return false;
            return true;
        }

        private static int CalculateDigit(string digits, int startFactor)
        {
            int total = 0;
            int factor = startFactor;
            for (int i = 0; i < digits.Length && factor > 1; i++, factor--)
                total += (digits[i] - '0') * factor;

            int remainder = total % 11;
            return (remainder < 2) ? 0 : 11 - remainder;
        }
    }
}
