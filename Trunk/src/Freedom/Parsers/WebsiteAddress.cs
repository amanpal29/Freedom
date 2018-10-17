using System.Text.RegularExpressions;
using Freedom.Exceptions;

namespace Freedom.Parsers
{
    public struct WebsiteAddress
    {
        private static readonly Regex WebsiteRegex = new Regex(@"^(https?://)?(([0-9a-z_!~*'().&=+$%-]+: )?[0-9a-z_!~*'().&=+$%-]+@)?(([0-9]{1,3}\.){3}[0-9]{1,3}|([0-9a-z_!~*'()-]+\.)*([0-9a-z][0-9a-z-]{0,61})?[0-9a-z]\.[a-z]{2,6})(:[0-9]{1,4})?((/?)|(/[0-9a-z_!~*'().;?:@&=+$,%#-]+)+/?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static readonly WebsiteAddress Empty = new WebsiteAddress();

        private readonly string _websiteAddress;

        public WebsiteAddress(string websiteAddress)
        {
            _websiteAddress = websiteAddress;
        }

        private static string ParseCore(string input, out WebsiteAddress result)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                result = Empty;
                return null;
            }

            input = input.Trim();

            if (IsValid(input))
            {
                result = new WebsiteAddress(input);
                return null;
            }

            result = Empty;

            return $"\"{input}\" is not a valid website address.";
        }

        public static WebsiteAddress Parse(string websiteAddress)
        {
            WebsiteAddress result;

            string errorMessage = ParseCore(websiteAddress, out result);

            if (!string.IsNullOrEmpty(errorMessage))
                throw new AmbiguousValueException(errorMessage);

            return result;
        }

        public static bool TryParse(string input, out WebsiteAddress result)
        {
            return string.IsNullOrEmpty(ParseCore(input, out result));
        }

        public static bool IsValid(string input)
        {
            return string.IsNullOrEmpty(input) || WebsiteRegex.IsMatch(input);
        }

        public override string ToString()
        {
            return _websiteAddress ?? string.Empty;
        }

        public static implicit operator string(WebsiteAddress websiteAddress)
        {
            return websiteAddress.ToString();
        }
    }
}
