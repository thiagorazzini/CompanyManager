using System;
using System.Text.RegularExpressions;

namespace CompanyManager.Domain.ValueObjects
{
    public sealed class PhoneNumber : IEquatable<PhoneNumber>
    {
        private static readonly Regex ExtRegex =
            new(@"(?i)\b(?:ext\.?|x|ramal)\s*(\d+)\b", RegexOptions.Compiled);

        public string Raw { get; }             // entrada original (trimada)
        public string E164 { get; }            // canônico com DDI, ex.: +5511999999999
        public string CountryCode { get; }     // ex.: "55", "1"
        public string NationalNumber { get; }  // ex.: "11999999999"
        public string? Extension { get; }      // ex.: "1234"

        /// <param name="defaultCountry">
        /// País padrão para quando a entrada não vier com '+'. Ex.: "BR".
        /// </param>
        public PhoneNumber(string value, string? defaultCountry = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone number cannot be null or empty", nameof(value));

            var trimmed = value.Trim();
            Raw = trimmed;

            string? ext = null;

            var baseWithoutExt = ExtRegex.Replace(trimmed, m =>
            {
                ext ??= m.Groups[1].Value;   // ok: variável local
                return string.Empty;
            }).Trim();

            Extension = ext;

            // '+' deve existir no máximo uma vez e (se existir) deve ser o primeiro caractere
            var plusCount = baseWithoutExt.Count(c => c == '+');
            if (plusCount > 1 || (plusCount == 1 && baseWithoutExt[0] != '+'))
                throw new ArgumentException("Invalid phone number format", nameof(value));

            // 2) Remove tudo exceto dígitos; preserva a informação se havia '+'
            var hasPlus = baseWithoutExt.StartsWith("+");
            var digitsOnly = new string(baseWithoutExt.Where(char.IsDigit).ToArray());

            if (hasPlus)
            {
                // 2a) Com DDI explícito: validar faixa de tamanho total (E.164 = máx. 15)
                if (digitsOnly.Length < 8 || digitsOnly.Length > 15)
                    throw new ArgumentException("Invalid phone number format", nameof(value));

                // 2b) Separar DDI e número nacional (suporte explícito para +55 e +1; fallback 3 dígitos)
                string cc, nsn;
                if (digitsOnly.StartsWith("55"))
                {
                    cc = "55";
                    nsn = digitsOnly.Substring(2);
                }
                else if (digitsOnly.StartsWith("1"))
                {
                    cc = "1";
                    nsn = digitsOnly.Substring(1);
                }
                else
                {
                    // Fallback genérico: assume DDI de 3 dígitos
                    cc = digitsOnly.Length > 3 ? digitsOnly[..3] : digitsOnly;
                    nsn = digitsOnly.Length > 3 ? digitsOnly[3..] : string.Empty;
                }

                if (string.IsNullOrEmpty(nsn))
                    throw new ArgumentException("Invalid phone number format", nameof(value));

                CountryCode = cc;
                NationalNumber = nsn;
                E164 = $"+{cc}{nsn}";
            }
            else
            {
                // 3) Sem '+': exige defaultCountry conhecido para normalizar
                if (!string.Equals(defaultCountry, "BR", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("Invalid phone number format", nameof(value));

                // Remove formatação e valida BR (DDD + número)
                var digits = digitsOnly;
                if (digits.Length is not (10 or 11))
                    throw new ArgumentException("Invalid phone number format", nameof(value));

                var ddd = digits[..2];
                var subscriber = digits[2..];

                // DDD não pode ser "00"
                if (ddd == "00")
                    throw new ArgumentException("Invalid phone number format", nameof(value));

                // Móvel (11 dígitos): começa com 9
                if (digits.Length == 11)
                {
                    if (subscriber[0] != '9')
                        throw new ArgumentException("Invalid phone number format", nameof(value));
                }
                else // Fixo (10 dígitos): não pode iniciar com 0 ou 9 (usa-se 2..8 na prática)
                {
                    var first = subscriber[0];
                    if (first == '0' || first == '9')
                        throw new ArgumentException("Invalid phone number format", nameof(value));
                }

                CountryCode = "55";
                NationalNumber = digits;
                E164 = $"+55{digits}";
            }
        }

        // Representação mascarada amigável (mantém DDI/DDD/últimos 4)
        public string Masked
        {
            get
            {
                var last4 = NationalNumber.Length >= 4 ? NationalNumber[^4..] : NationalNumber;

                if (CountryCode == "55" && NationalNumber.Length == 11)
                {
                    var ddd = NationalNumber[..2];
                    var first = NationalNumber[2];
                    return $"+55 {ddd} {first}XXXX-{last4}";
                }

                // Genérico: esconde miolo e mostra últimos 4
                return $"+{CountryCode} XXXX-{last4}";
            }
        }

        public override string ToString() => E164;

        // Igualdade/Hash por valor canônico (formato com DDI)
        public override bool Equals(object? obj) => obj is PhoneNumber other && Equals(other);
        public bool Equals(PhoneNumber? other) =>
            other is not null && StringComparer.Ordinal.Equals(E164, other.E164);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(E164);

        public static bool operator ==(PhoneNumber? left, PhoneNumber? right) => Equals(left, right);
        public static bool operator !=(PhoneNumber? left, PhoneNumber? right) => !Equals(left, right);
    }
}
