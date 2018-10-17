using System;
using System.ComponentModel;
using System.Text;
using Freedom.Constants;
using Freedom.Parsers;

namespace Freedom.Domain.Model
{
    public partial class MailingAddress
    {
        public MailingAddress()
        {
            PropertyChanged += ReflectPropertyChange;
        }

        private void ReflectPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Street":
                case "City":
                case "Province":
                case "Country":
                case "PostalCode":
                    OnPropertyChanged(nameof(AddressText));
                    OnPropertyChanged(nameof(IsEmpty));
                    break;
            }
        }

        public string AddressText => GetAddressText();

        public string GetAddressText(AddressTextFormatOptions options = AddressTextFormatOptions.None)
        {
            bool hasStreet = !string.IsNullOrWhiteSpace(Street);
            bool hasCity = !string.IsNullOrWhiteSpace(City);
            bool hasProvince = !string.IsNullOrWhiteSpace(Province);
            bool hasCountry = !string.IsNullOrWhiteSpace(Country);
            bool hasPostalCode = !string.IsNullOrWhiteSpace(PostalCode);

            StringBuilder stringBuilder = new StringBuilder();

            if (hasStreet)
            {
                string[] lines = Street.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    stringBuilder.Append(line);

                    if (options.HasFlag(AddressTextFormatOptions.SingleLine))
                        stringBuilder.Append(", ");
                    else
                        stringBuilder.AppendLine();
                }
            }

            if (hasCity)
                stringBuilder.Append(_city.Trim());

            if (hasCity && hasProvince)
                stringBuilder.Append(' ');

            if (hasProvince)
                stringBuilder.Append(_province.Trim());

            if (hasPostalCode && (hasCity || hasProvince))
                stringBuilder.Append("  ");

            if (hasPostalCode)
                stringBuilder.Append(_postalCode.Trim());

            if (hasCountry && !options.HasFlag(AddressTextFormatOptions.ExcludeCountry))
            {
                if (stringBuilder.Length > 0)
                {
                    if (options.HasFlag(AddressTextFormatOptions.SingleLine))
                        stringBuilder.Append(", ");
                    else
                        stringBuilder.AppendLine();
                }

                stringBuilder.Append(_country.Trim());
            }

            return stringBuilder.ToString();
        }

        public void Clear()
        {
            Street = null;
            City = null;
            Province = null;
            Country = null;
            PostalCode = null;
        }

        public bool IsEmpty
            => string.IsNullOrEmpty(Street) && string.IsNullOrEmpty(City) && string.IsNullOrEmpty(Province) &&
               string.IsNullOrEmpty(Country) && string.IsNullOrEmpty(PostalCode);

        public static bool TryParse(string newValue, MailingAddress result)
        {
            AddressData parsed = AddressData.ParseMailing(newValue);

            AddressEntry country = AddressDatabase.FindCountry(parsed.Country);
            AddressEntry province = AddressDatabase.FindProvince(parsed.Province);

            result.Country = country != null ? country.Name : parsed.Country;

            if (province != null)
            {
                result.Province = province.Code;
                result.Country = province.Parent.Name;
            }
            else
            {
                result.Country = parsed.Country;
            }

            result.Street = parsed.StreetName;
            result.City = parsed.City;
            result.PostalCode = parsed.PostalCode;

            return false;
        }

        public void Copy(MailingAddress source)
        {
            Street = source.Street;
            PostalCode = source.PostalCode;
            City = source.City;
            Province = source.Province;
            Country = source.Country;
        }

        public override string ToString()
        {
            return GetAddressText();
        }

    }
}
