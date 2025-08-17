using System.Globalization;
using System.Text.RegularExpressions;

namespace CompanyManager.Domain.ValueObjects
{
    public sealed class Email : IEquatable<Email>
    {
        private static readonly Regex LocalPartRegex =
            new(@"^[A-Za-z0-9.!#$%&'*+/=?^_`{|}~-]+$", RegexOptions.Compiled);

        private static readonly Regex HasInnerWhitespace =
            new(@"\s", RegexOptions.Compiled);

        public string Value { get; }
        public string LocalPart { get; }
        public string Domain { get; }
        public string DomainAscii { get; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be null or empty", nameof(value));

            var trimmed = value.Trim();

            if (HasInnerWhitespace.IsMatch(trimmed))
                throw new ArgumentException("Invalid email format", nameof(value));

            var atFirst = trimmed.IndexOf('@');
            var atLast = trimmed.LastIndexOf('@');
            if (atFirst <= 0 || atLast != atFirst || atFirst == trimmed.Length - 1)
                throw new ArgumentException("Invalid email format", nameof(value));

            var local = trimmed[..atFirst];
            var domainRaw = trimmed[(atFirst + 1)..];


            if (local.Length is < 1 or > 64)
                throw new ArgumentException("Invalid email format", nameof(value));
            if (trimmed.Length > 254)        // total até 254
                throw new ArgumentException("Invalid email format", nameof(value));


            if (!LocalPartRegex.IsMatch(local) ||
                local.StartsWith('.') || local.EndsWith('.') ||
                local.Contains(".."))
                throw new ArgumentException("Invalid email format", nameof(value));


            string domainAscii;
            try
            {
                var idn = new IdnMapping();
                domainAscii = idn.GetAscii(domainRaw);
            }
            catch
            {
                throw new ArgumentException("Invalid email format", nameof(value));
            }

            var localLower = local.ToLowerInvariant();
            var domainLower = domainRaw.ToLowerInvariant();
            var domainAsciiLower = domainAscii.ToLowerInvariant();

            if (string.IsNullOrEmpty(domainAsciiLower) ||
                domainAsciiLower.StartsWith('.') || domainAsciiLower.EndsWith('.') ||
                domainAsciiLower.Contains("..") || !domainAsciiLower.Contains('.'))
                throw new ArgumentException("Invalid email format", nameof(value));

            var labels = domainAsciiLower.Split('.');
            foreach (var label in labels)
            {
                if (label.Length is < 1 or > 63) throw new ArgumentException("Invalid email format", nameof(value));
                if (label.StartsWith('-') || label.EndsWith('-')) throw new ArgumentException("Invalid email format", nameof(value));
                if (!label.All(ch => (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '-'))
                    throw new ArgumentException("Invalid email format", nameof(value));
            }

            LocalPart = localLower;
            Domain = domainLower;
            DomainAscii = domainAsciiLower;

            Value = $"{LocalPart}@{Domain}";
        }

        public override string ToString() => Value;

        public override bool Equals(object? obj) => obj is Email other && Equals(other);

        public bool Equals(Email? other)
            => other is not null && StringComparer.Ordinal.Equals(Value, other.Value);

        public override int GetHashCode()
            => StringComparer.Ordinal.GetHashCode(Value);

        public static bool operator ==(Email? left, Email? right) => Equals(left, right);
        public static bool operator !=(Email? left, Email? right) => !Equals(left, right);
    }
}
