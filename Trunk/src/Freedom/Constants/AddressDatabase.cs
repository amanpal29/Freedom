using System.Collections.Generic;
using Freedom.Extensions;

namespace Freedom.Constants
{
    public static class AddressDatabase
    {
        #region Data

        private static readonly AddressEntry Canada = new AddressEntry("CA", "Canada");
        private static readonly AddressEntry Usa = new AddressEntry("US", "United States", "United States of America", "USA", "États-Unis", "États-Unis d'Amérique");
        private static readonly AddressEntry Australia = new AddressEntry("AU", "Australia");
        private static readonly AddressEntry Brazil = new AddressEntry("BR", "Brazil");
        private static readonly AddressEntry China = new AddressEntry("CN", "China");
        private static readonly AddressEntry France = new AddressEntry("FR", "France");
        private static readonly AddressEntry Germany = new AddressEntry("DE", "Germany");
        private static readonly AddressEntry India = new AddressEntry("IN", "India");
        private static readonly AddressEntry Italy = new AddressEntry("IT", "Italy");
        private static readonly AddressEntry Japan = new AddressEntry("JP", "Japan");
        private static readonly AddressEntry Korea = new AddressEntry("KR", "South Korea");
        private static readonly AddressEntry Mexico = new AddressEntry("MX", "Mexico");
        private static readonly AddressEntry Russia = new AddressEntry("RU", "Russia");
        private static readonly AddressEntry Spain = new AddressEntry("ES", "Spain");
        private static readonly AddressEntry Uk = new AddressEntry("UK", "United Kingdom");

        public static readonly AddressEntry[] Countries =
            {
                Canada, Usa, Australia, Brazil, China, France, Germany, India,
                Italy, Japan, Korea, Mexico, Russia, Spain, Uk
            };

        public static readonly AddressEntry[] Provinces =
            {
                new AddressEntry(Canada, "AB", "Alberta"),
                new AddressEntry(Canada, "BC", "British Columbia", "Colombie Britannique"),
                new AddressEntry(Canada, "MB", "Manitoba"),
                new AddressEntry(Canada, "NB", "New Brunswick", "Nouveau Brunswick"),
                new AddressEntry(Canada, "NL", "Newfoundland and Labrador", "Terre Neuve et Labrador", "Newfoundland", "Labrador", "Terre-Neuve"),
                new AddressEntry(Canada, "NT", "Northwest Territories", "Territoires du Nord Ouest", "NWT"),
                new AddressEntry(Canada, "NS", "Nova Scotia", "Nouvelle Écosse"),
                new AddressEntry(Canada, "NU", "Nunavut"),
                new AddressEntry(Canada, "ON", "Ontario"),
                new AddressEntry(Canada, "PE", "Prince Edward Island", "Île du Prince Édouard"),
                new AddressEntry(Canada, "QC", "Québec"),
                new AddressEntry(Canada, "SK", "Saskatchewan"),
                new AddressEntry(Canada, "YT", "Yukon", "Yukon Territories"),

                new AddressEntry(Usa, "AL", "Alabama"),
                new AddressEntry(Usa, "AK", "Alaska"),
                new AddressEntry(Usa, "AZ", "Arizona"),
                new AddressEntry(Usa, "AR", "Arkansas"),
                new AddressEntry(Usa, "CA", "California"),
                new AddressEntry(Usa, "CO", "Colorado"),
                new AddressEntry(Usa, "CT", "Connecticut"),
                new AddressEntry(Usa, "DE", "Delaware"),
                new AddressEntry(Usa, "DC", "District of Columbia"),
                new AddressEntry(Usa, "FL", "Florida"),
                new AddressEntry(Usa, "GA", "Georgia"),
                new AddressEntry(Usa, "HI", "Hawaii"),
                new AddressEntry(Usa, "ID", "Idaho"),
                new AddressEntry(Usa, "IL", "Illinois"),
                new AddressEntry(Usa, "IN", "Indiana"),
                new AddressEntry(Usa, "IA", "Iowa"),
                new AddressEntry(Usa, "KS", "Kansas"),
                new AddressEntry(Usa, "KY", "Kentucky"),
                new AddressEntry(Usa, "LA", "Louisiana"),
                new AddressEntry(Usa, "ME", "Maine"),
                new AddressEntry(Usa, "MD", "Maryland"),
                new AddressEntry(Usa, "MA", "Massachusetts"),
                new AddressEntry(Usa, "MI", "Michigan"),
                new AddressEntry(Usa, "MN", "Minnesota"),
                new AddressEntry(Usa, "MS", "Mississippi"),
                new AddressEntry(Usa, "MO", "Missouri"),
                new AddressEntry(Usa, "MT", "Montana"),
                new AddressEntry(Usa, "NE", "Nebraska"),
                new AddressEntry(Usa, "NV", "Nevada"),
                new AddressEntry(Usa, "NH", "New Hampshire"),
                new AddressEntry(Usa, "NJ", "New Jersey"),
                new AddressEntry(Usa, "NM", "New Mexico"),
                new AddressEntry(Usa, "NY", "New York"),
                new AddressEntry(Usa, "NC", "North Carolina"),
                new AddressEntry(Usa, "ND", "North Dakota"),
                new AddressEntry(Usa, "OH", "Ohio"),
                new AddressEntry(Usa, "OK", "Oklahoma"),
                new AddressEntry(Usa, "OR", "Oregon"),
                new AddressEntry(Usa, "PA", "Pennsylvania"),
                new AddressEntry(Usa, "RI", "Rhode Island"),
                new AddressEntry(Usa, "SC", "South Carolina"),
                new AddressEntry(Usa, "SD", "South Dakota"),
                new AddressEntry(Usa, "TN", "Tennessee"),
                new AddressEntry(Usa, "TX", "Texas"),
                new AddressEntry(Usa, "UT", "Utah"),
                new AddressEntry(Usa, "VT", "Vermont"),
                new AddressEntry(Usa, "VA", "Virginia"),
                new AddressEntry(Usa, "WA", "Washington"),
                new AddressEntry(Usa, "WV", "West Virginia"),
                new AddressEntry(Usa, "WI", "Wisconsin"),
                new AddressEntry(Usa, "WY", "Wyoming")
            };

        #endregion

        #region Fields

        private static readonly Dictionary<string, AddressEntry> CountryDictionary;
        private static readonly Dictionary<string, AddressEntry> ProvinceDictionary;

        #endregion

        #region Static Constructor

        static AddressDatabase()
        {
            CountryDictionary = BuildDictionary(Countries);
            ProvinceDictionary = BuildDictionary(Provinces);
        }

        private static Dictionary<string, AddressEntry> BuildDictionary(IEnumerable<AddressEntry> entries)
        {           
            Dictionary<string, AddressEntry> result = new Dictionary<string, AddressEntry>();

            foreach (AddressEntry addressEntry in entries)
            {
                result.Add(addressEntry.Code, addressEntry);
                result.Add(addressEntry.Name.RemoveDiacritics().ToUpper(), addressEntry);

                if (addressEntry.AlternateNames != null)
                {
                    foreach (string name in addressEntry.AlternateNames)
                        result.Add(name.RemoveDiacritics().ToUpper(), addressEntry);
                }
            }

            return result;
        }

        #endregion

        #region Methods

        public static AddressEntry FindCountry(string countryName)
        {
            if (string.IsNullOrEmpty(countryName))
                return null;

            string key = countryName.RemoveDiacritics().ToUpper();

            if (!CountryDictionary.ContainsKey(key))
                return null;

            return CountryDictionary[key];
        }

        public static AddressEntry FindProvince(string provinceName)
        {
            if (string.IsNullOrEmpty(provinceName))
                return null; 
            
            string key = provinceName.RemoveDiacritics().ToUpper().Replace('-', ' ');

            if (!ProvinceDictionary.ContainsKey(key))
                return null;

            return ProvinceDictionary[key];
        }

        #endregion
    }
}
