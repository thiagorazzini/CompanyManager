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
        /// Pa�s padr�o para quando a entrada n�o vier com '+'. Ex.: "BR".
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
                ext ??= m.Groups[1].Value;   // ok: vari�vel local
                return string.Empty;
            }).Trim();

            Extension = ext;

            // '+' deve existir no m�ximo uma vez e (se existir) deve ser o primeiro caractere
            var plusCount = baseWithoutExt.Count(c => c == '+');
            if (plusCount > 1 || (plusCount == 1 && baseWithoutExt[0] != '+'))
                throw new ArgumentException("Invalid phone number format", nameof(value));

            // 2) Remove tudo exceto d�gitos; preserva a informa��o se havia '+'
            var hasPlus = baseWithoutExt.StartsWith("+");
            var digitsOnly = new string(baseWithoutExt.Where(char.IsDigit).ToArray());

            if (hasPlus)
            {
                // 2a) Com DDI expl�cito: validar faixa de tamanho total (E.164 = m�x. 15)
                if (digitsOnly.Length < 8 || digitsOnly.Length > 15)
                    throw new ArgumentException("Invalid phone number format", nameof(value));

                // 2b) Separar DDI e n�mero nacional (suporte expl�cito para +55 e +1; fallback 3 d�gitos)
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
                    // Fallback gen�rico: assume DDI de 3 d�gitos
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

                // Remove formata��o e valida BR (DDD + n�mero)
                var digits = digitsOnly;
                if (digits.Length is not (10 or 11))
                    throw new ArgumentException("Invalid phone number format", nameof(value));

                var ddd = digits[..2];
                var subscriber = digits[2..];

                // DDD n�o pode ser "00"
                if (ddd == "00")
                    throw new ArgumentException("Invalid phone number format", nameof(value));

                // M�vel (11 d�gitos): come�a com 9
                if (digits.Length == 11)
                {
                    if (subscriber[0] != '9')
                        throw new ArgumentException("Invalid phone number format", nameof(value));
                }
                else // Fixo (10 d�gitos): n�o pode iniciar com 0 ou 9 (usa-se 2..8 na pr�tica)
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

        // Representa��o mascarada amig�vel (mant�m DDI/DDD/�ltimos 4)
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

                // Gen�rico: esconde miolo e mostra �ltimos 4
                return $"+{CountryCode} XXXX-{last4}";
            }
        }

        public override string ToString() => E164;

        // Igualdade/Hash por valor can�nico (formato com DDI)
        public override bool Equals(object? obj) => obj is PhoneNumber other && Equals(other);
        public bool Equals(PhoneNumber? other) =>
            other is not null && StringComparer.Ordinal.Equals(E164, other.E164);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(E164);

        public static bool operator ==(PhoneNumber? left, PhoneNumber? right) => Equals(left, right);
        public static bool operator !=(PhoneNumber? left, PhoneNumber? right) => !Equals(left, right);
    }
}
