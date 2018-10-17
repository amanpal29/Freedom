using System.Text.RegularExpressions;
using Freedom.Exceptions;

namespace Freedom.Parsers
{
    public struct EmailAddress
    {
        private static readonly Regex EmailRegex = new Regex(@"^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9][a-z0-9-]{0,61}[a-z0-9]\.?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static readonly EmailAddress Empty = new EmailAddress();

        private readonly string _emailAddress;

        public EmailAddress(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                _emailAddress = null;
                return;
            }

            _emailAddress = emailAddress.Trim();
        }

        private static string ParseCore(string input, out EmailAddress result)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                result = Empty;
                return null;
            }

            input = input.Trim();

            if (IsValid(input))
            {
                result = new EmailAddress(input);
                return null;
            }

            result = Empty;
            return $"\"{input}\" is not a valid email address.";
        }

        public static EmailAddress Parse(string emailAddress)
        {
            EmailAddress result;

            string errorMessage = ParseCore(emailAddress, out result);

            if (!string.IsNullOrEmpty(errorMessage))
                throw new AmbiguousValueException(errorMessage);

            return result;
        }

        public static bool TryParse(string input, out EmailAddress result)
        {
            return string.IsNullOrEmpty(ParseCore(input, out result));
        }

        public static bool IsValid(string input)
        {
            return string.IsNullOrEmpty(input) || EmailRegex.IsMatch(input);
        }

        public override string ToString()
        {
            return _emailAddress ?? string.Empty;
        }

        public static implicit operator string(EmailAddress emailAddress)
        {
            return emailAddress.ToString();
        }
    }
}