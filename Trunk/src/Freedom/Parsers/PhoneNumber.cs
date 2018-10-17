using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Freedom.Exceptions;

namespace Freedom.Parsers
{
    public struct PhoneNumber
    {
        public static readonly PhoneNumber Empty = new PhoneNumber();

        private static readonly Regex ExtensionRegex = new Regex(@"(([xX]|[eE][xX][tT])\.?\s*(?<Extension>\d+))$", RegexOptions.Compiled);
        private static readonly Regex InternationalRegex = new Regex(@"^(\+|(011))\s*(?<Country>[234567890]\d*)\s+(?<Number>.*)$", RegexOptions.Compiled);

        public PhoneNumber(string country, string areaCode, string number, string extension)
        {
            Country = country;
            Extension = extension;
            Number = number;
            AreaCode = areaCode;
        }

        public string Country { get; }
        public string AreaCode { get; }
        public string Number { get; }
        public string Extension { get; }

        private static string ParseCore(string rawText, out PhoneNumber phoneNumber)
        {
            string errorMessage = null;

            string country = null;
            string areaCode = null;
            string number;
            string extension = null;

            // Ignore empty values
            if (string.IsNullOrWhiteSpace(rawText))
            {
                phoneNumber = Empty;
                return null;
            }

            string text;

            // Look for an extension...
            Match match = ExtensionRegex.Match(rawText);
            if (match.Success)
            {
                extension = match.Groups["Extension"].Value;
                text = rawText.Substring(0, match.Groups[0].Index).Trim();
            }
            else
            {
                text = rawText.Trim();
            }

            // Look for international (to North America) prefix
            match = InternationalRegex.Match(text);
            if (match.Success)
            {
                country = match.Groups["Country"].ToString();
                number = match.Groups["Number"].ToString();
            }
            else
            {
                string digitsOnly = new string(text.Where(char.IsDigit).ToArray());

                if (digitsOnly.StartsWith("1"))
                {
                    country = "1";
                    digitsOnly = digitsOnly.Substring(1);
                }

                string exchange;
                string station;

                if (digitsOnly.Length <= 3)
                {
                    exchange = digitsOnly;
                    station = string.Empty;
                }
                else if (digitsOnly.Length <= 7)
                {
                    exchange = digitsOnly.Substring(0, 3);
                    station = digitsOnly.Substring(3);
                }
                else
                {
                    areaCode = digitsOnly.Substring(0, 3);
                    exchange = digitsOnly.Substring(3, 3);
                    station = digitsOnly.Substring(6);
                }

                number = $"{exchange}-{station}";

                if (text.Any(char.IsLetter))
                {
                    errorMessage = "Unexpected letter in phone number.";
                }
                else if (digitsOnly.Length != 7 && digitsOnly.Length != 10)
                {
                    errorMessage = "Invalid phone number length.  Expected a 7 or 10 digit phone number.";
                }
            }

            phoneNumber = new PhoneNumber(country, areaCode, number, extension);

            return errorMessage;
        }

        #region Parse and TryParse

        public static PhoneNumber Parse(string rawText)
        {
            PhoneNumber result;

            string message = ParseCore(rawText, out result);

            if (!string.IsNullOrEmpty(message))
                throw new AmbiguousValueException(message);

            return result;
        }

        public static bool TryParse(string rawText, out PhoneNumber phoneNumber)
        {
            return string.IsNullOrEmpty(ParseCore(rawText, out phoneNumber));
        }

        #endregion

        #region Equality

        public static bool operator==(PhoneNumber left, PhoneNumber right)
        {
            return Equals(left, right);
        }

        public static bool operator!=(PhoneNumber left, PhoneNumber right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            PhoneNumber other = (PhoneNumber) obj;

            return
                Country == other.Country &&
                AreaCode == other.AreaCode &&
                Number == other.Number &&
                Extension == other.Extension;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        #endregion

        #region String Formatting

        public override string ToString()
        {
            if (this == Empty)
                return string.Empty;

            StringBuilder stringBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(Country))
            {
                stringBuilder.Append('+');
                stringBuilder.Append(Country);
                stringBuilder.Append(' ');
            }

            if (!string.IsNullOrEmpty(AreaCode))
            {
                stringBuilder.AppendFormat("({0}) ", AreaCode);
            }

            stringBuilder.Append(Number);

            if (!string.IsNullOrEmpty(Extension))
            {
                stringBuilder.AppendFormat(" x {0}", Extension);
            }

            return stringBuilder.ToString();
        }

        public static implicit operator string(PhoneNumber number)
        {
            return number.ToString();
        }

        #endregion
    }
}
