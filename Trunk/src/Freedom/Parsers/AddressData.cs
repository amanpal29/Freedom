using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Freedom.Extensions;

namespace Freedom.Parsers
{
    [Flags]
    public enum AddressTextFormatOptions
    {
        None = 0x0000,
        ExcludeCountry = 0x0001,
        SingleLine = 0x0002
    }

    public struct AddressData
    {
        #region Fields

        public static readonly AddressData Empty = new AddressData();

        #endregion

        #region Constants

        private static readonly string[] UnitNames = { "APARTMENT", "APT", "SUITE", "STE", "UNIT" };

        private static readonly string[] ProvincesAndStates = {
                "AL", "AK", "AS", "AZ", "AR", "CA", "CO", "CT", "DE", "DC", "FM", "FL", "GA",
                "GU", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MH", "MD", "MA",
                "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ", "NM", "NY", "NC", "ND",
                "MP", "OH", "OK", "OR", "PW", "PA", "PR", "RI", "SC", "SD", "TN", "TX", "UT",
                "VT", "VI", "VA", "WA", "WV", "WI", "WY",

                "Alabama", "Alaska", "American Samoa", "Arizona", "Arkansas", "California",
                "Colorado", "Connecticut", "Delaware", "District Of Columbia",
                "Federated States Of Micronesia", "Florida", "Georgia", "Guam", "Hawaii",
                "Idaho", "Illinois", "Indiana", "Iowa", "Kansas", "Kentucky", "Louisiana",
                "Maine", "Marshall Islands", "Maryland", "Massachusetts", "Michigan",
                "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada",
                "Newhampshire", "New Jersey", "New Mexico", "New York", "North Carolina",
                "North Dakota", "Northern Mariana Islands", "Ohio", "Oklahoma", "Oregon",
                "Palau", "Pennsylvania", "Puerto Rico", "Rhode Island", "South Carolina",
                "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virgin Islands",
                "Virginia", "Washington", "West Virginia", "Wisconsin", "Wyoming",

                "AB", "BC", "MB", "NB", "NL", "NT", "NS", "NU", "ON", "PE", "QC", "SK", "YT",

                "Alberta", "British Columbia", "Colombie-Britannique", "Manitoba",
                "New Brunswick", "Nouveau-Brunswick", "Newfoundland And Labrador",
                "Terre-Neuve-Et-Labrador", "Newfoundland", "Labrador", "Terre-Neuve",
                "Northwest Territories", "Territoires Du Nord-Ouest", "Nova Scotia",
                "Nouvelle-Écosse", "Nunavut", "Ontario", "Prince Edward Island",
                "Ile-Du-Prince-Édouard", "Québec", "Saskatchewan", "Yukon"
        };

        private static readonly string[] Countries = {
                "Afghanistan", "Akrotiri", "Albania", "Algeria", "American Samoa", "Andorra",
                "Angola", "Anguilla", "Antarctic Lands", "Antarctica", "Antigua", "Argentina",
                "Armenia", "Aruba", "Ashmore", "Australia", "Austria", "Azerbaijan", "Bahamas",
                "Bahrain", "Bangladesh", "Barbados", "Barbuda", "Bassas da India", "Belarus",
                "Belgium", "Belize", "Benin", "Bermuda", "Bhutan", "Bolivia", "Bosnia",
                "Botswana", "Bouvet Island", "Brazil", "British Indian Ocean Territory",
                "British Virgin Islands", "Brunei", "Bulgaria", "Burkina Faso", "Burma",
                "Burundi", "Caicos Islands", "Cambodia", "Cameroon", "Canada", "Cape Verde",
                "Cartier Islands", "Cayman Islands", "Central African Republic", "Chad",
                "Chile", "China", "Christmas Island", "Clipperton Island", "Cocos Islands",
                "Colombia", "Comoros", "Congo", "Cook Islands", "Coral Sea Islands",
                "Costa Rica", "Cote d'Ivoire", "Croatia", "Cuba", "Cyprus", "Czech Republic",
                "Denmark", "Dhekelia", "Djibouti", "Dominica", "Dominican Republic", "Ecuador",
                "Egypt", "El Salvador", "Equatorial Guinea", "Eritrea", "Estonia", "Ethiopia",
                "Europa Island", "Falkland Islands", "Faroe Islands", "Fiji", "Finland",
                "France", "French Guiana", "French Polynesia", "French Southern", "Futuna",
                "Gabon", "Gambia", "Gaza Strip", "Georgia", "Germany", "Ghana", "Gibraltar",
                "Glorioso Islands", "Greece", "Greenland", "Grenada", "Grenadines",
                "Guadeloupe", "Guam", "Guatemala", "Guernsey", "Guinea", "Guinea-Bissau",
                "Guyana", "Haiti", "Heard Island", "Herzegovina", "Holy See", "Honduras",
                "Hong Kong", "Hungary", "Iceland", "India", "Indonesia", "Iran", "Iraq",
                "Ireland", "Islas Malvinas", "Isle of Man", "Israel", "Italy", "Jamaica",
                "Jan Mayen", "Japan", "Jersey", "Jordan", "Juan de Nova Island", "Kazakhstan",
                "Keeling Islands", "Kenya", "Kiribati", "Korea", "Kuwait", "Kyrgyzstan",
                "Laos", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libya", "Liechtenstein",
                "Lithuania", "Luxembourg", "Macau", "Macedonia", "Madagascar", "Malawi",
                "Malaysia", "Maldives", "Mali", "Malta", "Marshall Islands", "Martinique",
                "Mauritania", "Mauritius", "Mayotte", "McDonald Islands", "Mexico",
                "Micronesia", "Miquelon", "Moldova", "Monaco", "Mongolia", "Montenegro",
                "Montserrat", "Morocco", "Mozambique", "Namibia", "Nauru", "Navassa Island",
                "Nepal", "Netherlands", "Netherlands Antilles", "Nevis", "New Caledonia",
                "New Zealand", "Nicaragua", "Niger", "Nigeria", "Niue", "Norfolk Island",
                "North Korea", "Northern Mariana Islands", "Norway", "Oman", "Pakistan",
                "Palau", "Panama", "Papua New Guinea", "Paracel Islands", "Paraguay", "Peru",
                "Philippines", "Pitcairn Islands", "Poland", "Portugal", "Puerto Rico",
                "Qatar", "Reunion", "Romania", "Russia", "Rwanda", "Saint Helena",
                "Saint Kitts", "Saint Lucia", "Saint Pierre", "Saint Vincent", "Samoa",
                "San Marino", "Sao Tome and Principe", "Saudi Arabia", "Senegal", "Serbia",
                "Seychelles", "Sierra Leone", "Singapore", "Slovakia", "Slovenia",
                "Solomon Islands", "Somalia", "South Africa", "South Georgia", "South Korea",
                "South Sandwich Islands", "Spain", "Spratly Islands", "Sri Lanka", "Sudan",
                "Suriname", "Svalbard", "Swaziland", "Sweden", "Switzerland", "Syria",
                "Taiwan", "Tajikistan", "Tanzania", "Thailand", "Timor-Leste", "Togo",
                "Tokelau", "Tonga", "Trinidad and Tobago", "Tromelin Island", "Tunisia",
                "Turkey", "Turkmenistan", "Turks", "Tuvalu", "Uganda", "Ukraine",
                "United Arab Emirates", "United Kingdom", "United States of America",
                "United States", "Uruguay", "USA", "Uzbekistan", "Vanuatu",
                "Vatican City", "Venezuela", "Vietnam", "Virgin Islands", "Wake Island",
                "Wallis", "West Bank", "Western Sahara", "Yemen", "Zambia", "Zimbabwe"
        };

        public static readonly string[] Directions = {
                                                         "N", "S", "E", "W", "NE", "NW", "SE", "SW",
                                                         "North", "South", "East", "West",
                                                         "North-East", "North-West", "South-East", "South-West"
                                                     };

        public static readonly string[] StreetNames = {
                "ALLEY", "ANNEX", "ARCADE", "AVENUE", "BEND", "BOULEVARD", "BRIDGE", "BURG",
                "BYPASS", "CAMP", "CANYON", "CAPE", "CAUSEWAY", "CENTER", "CIRCLE", "CLIFF",
                "CLUB", "COMMON", "CORNER", "CORNERS", "COURSE", "COURT", "COURTS", "COVE",
                "COVES", "CREEK", "CRESCENT", "CREST", "CROSSING", "CROSSROAD", "CURVE",
                "DALE", "DAM", "DIVIDE", "DRIVE", "DRIVES", "ESTATE", "ESTATES", "EXPRESSWAY",
                "EXTENSION", "EXTENSIONS", "FALL", "FALLS", "FERRY", "FIELD", "FIELDS", "FLAT",
                "FLATS", "FORD", "FORDS", "FOREST", "FORGE", "FORGES", "FORK", "FORKS", "FORT",
                "FREEWAY", "GARDEN", "GARDENS", "GATEWAY", "GLEN", "GLENS", "GREEN", "GREENS",
                "GROVE", "GROVES", "HARBOR", "HARBORS", "HAVEN", "HEIGHTS", "HIGHWAY", "HILL",
                "HILLS", "HOLLOW", "INLET", "ISLAND", "ISLANDS", "ISLE", "JUNCTION",
                "JUNCTIONS", "KEY", "KEYS", "KNOLL", "KNOLLS", "LAKE", "LAKES", "LAND",
                "LANDING", "LANE", "LIGHT", "LIGHTS", "LOAF", "LOCK", "LOCKS", "LODGE", "LOOP",
                "MALL", "MANOR", "MANORS", "MEADOW", "MEADOWS", "MEWS", "MILL", "MILLS",
                "MISSION", "MOTORWAY", "MOUNT", "MOUNTAIN", "MOUNTAINS", "NECK", "ORCHARD",
                "OVAL", "OVERPASS", "PARK", "PARKS", "PARKWAY", "PARKWAYS", "PASS", "PASSAGE",
                "PATH", "PIKE", "PINE", "PINES", "PLACE", "PLAIN", "PLAINS", "PLAZA", "POINT",
                "POINTS", "PORT", "PORTS", "PRAIRIE", "RADIAL", "RAMP", "RANCH", "RAPID",
                "RAPIDS", "REST", "RIDGE", "RIDGES", "RIVER", "ROAD", "ROADS", "ROUTE", "ROW",
                "RUE", "RUN", "SHOAL", "SHOALS", "SHORE", "SHORES", "SKYWAY", "SPRING",
                "SPRINGS", "SPUR", "SPURS", "SQUARE", "SQUARES", "STATION", "STRAVENUE",
                "STREAM", "STREET", "STREETS", "SUMMIT", "TERRACE", "THROUGHWAY", "TRACE",
                "TRACK", "TRAFFICWAY", "TRAIL", "TUNNEL", "TURNPIKE", "UNDERPASS", "UNION",
                "UNIONS", "VALLEY", "VALLEYS", "VIADUCT", "VIEW", "VIEWS", "VILLAGE",
                "VILLAGES", "VILLE", "VISTA", "WALK", "WALKS", "WALL", "WAY", "WAYS", "WELL",
                "WELLS",

                "ALY", "ANX", "ARC", "AVE", "BND", "BLVD", "BRG", "BG", "BYP", "CP",
                "CYN", "CPE", "CSWY", "CTR", "CIR", "CLF", "CLB", "CMN", "COR", "CORS", "CRSE",
                "CT", "CTS", "CV", "CVS", "CRK", "CRES", "CRST", "XING", "XRD", "CURV", "DL",
                "DM", "DV", "DR", "DRS", "EST", "ESTS", "EXPY", "EXT", "EXTS", "FLS", "FRY",
                "FLD", "FLDS", "FLT", "FLTS", "FRD", "FRDS", "FRST", "FRG", "FRGS", "FRK",
                "FRKS", "FT", "FWY", "GDN", "GDNS", "GTWY", "GLN", "GLNS", "GRN", "GRNS",
                "GRV", "GRVS", "HBR", "HBRS", "HVN", "HTS", "HWY", "HL", "HLS", "HOLW", "INLT",
                "IS", "ISS", "JCT", "JCTS", "KY", "KYS", "KNL", "KNLS", "LK", "LKS", "LNDG",
                "LN", "LGT", "LGTS", "LF", "LCK", "LCKS", "LDG", "MNR", "MNRS", "MDW", "MDWS",
                "ML", "MLS", "MSN", "MTWY", "MT", "MTN", "MTNS", "NCK", "ORCH", "OPAS", "PKWY",
                "PSGE", "PNE", "PNES", "PL", "PLN", "PLNS", "PLZ", "PT", "PTS", "PRT", "PRTS",
                "PR", "RADL", "RNCH", "RPD", "RPDS", "RST", "RDG", "RDGS", "RIV", "RD", "RDS",
                "RTE", "SHL", "SHLS", "SHR", "SHRS", "SKWY", "SPG", "SPGS", "SQ", "SQS", "STA",
                "STRA", "STRM", "ST", "STS", "SMT", "TER", "TRWY", "TRCE", "TRAK", "TRFY",
                "TRL", "TUNL", "TPKE", "UPAS", "UN", "UNS", "VLY", "VLYS", "VIA", "VW", "VWS"
        };

        #endregion

        #region Constructors

        public AddressData(string unitNumber, string streetNumber, string streetName, string city, string province,
                           string country, string postalCode)
        {
            UnitNumber = unitNumber;
            StreetNumber = streetNumber;
            StreetName = streetName;
            City = city;
            Province = province;
            Country = country;
            PostalCode = postalCode;
            Latitude = Empty.Latitude;
            Longitude = Empty.Longitude;
        }

        public AddressData(string unitNumber, string streetNumber, string streetName, string city, string province,
                           string country, string postalCode, double latitude, double longitude)
        {
            UnitNumber = unitNumber;
            StreetNumber = streetNumber;
            StreetName = streetName;
            City = city;
            Province = province;
            Country = country;
            PostalCode = postalCode;
            Latitude = latitude;
            Longitude = longitude;
        }

        #endregion

        #region ParseCore

        private enum CharacterType
        {
            WhiteSpace,
            LetterOrDigit,
            SymbolOrPuncuation
        }

        private static CharacterType GetCharacterType(char c)
        {
            if (char.IsWhiteSpace(c))
                return CharacterType.WhiteSpace;

            if (char.IsLetterOrDigit(c))
                return CharacterType.LetterOrDigit;

            return CharacterType.SymbolOrPuncuation;
        }

        private static List<string> SplitStringByCharacterTypes(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new List<string>();

            List<string> result = new List<string>();

            StringBuilder part = new StringBuilder();

            CharacterType lastCharacterType = GetCharacterType(input[0]);

            foreach (char c in input)
            {
                CharacterType thisCharacterType = GetCharacterType(c);

                if (thisCharacterType != lastCharacterType)
                {
                    result.Add(part.ToString());
                    part.Clear();
                }

                part.Append(c);
                lastCharacterType = thisCharacterType;
            }

            result.Add(part.ToString());

            return result;
        }

        private struct MatchResult
        {
            public bool FoundMatch;
            public int Start;
            public int End;
            public int Length => End - Start + 1;
        }

        private static string LettersOrDigitsOnly(params string[] strings)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (string s in strings)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    foreach (char c in s)
                    {
                        if (char.IsLetterOrDigit(c))
                            stringBuilder.Append(c);
                    }
                }
            }

            return stringBuilder.ToString();
        }

        private static MatchResult MatchStringParts(ICollection<string> parts, string match)
        {
            string searchString = LettersOrDigitsOnly(match).RemoveDiacritics();

            for (int start = 0; start < parts.Count; start++)
            {
                for (int end = start; end < parts.Count; end++)
                {
                    string partialString = LettersOrDigitsOnly(parts.Skip(start).Take(end - start + 1).ToArray()).RemoveDiacritics();

                    if (string.Compare(partialString, searchString, StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        MatchResult matchResult;

                        matchResult.FoundMatch = true;
                        matchResult.Start = start;
                        matchResult.End = end;

                        return matchResult;
                    }
                }
            }

            return new MatchResult { FoundMatch = false, Start = -1, End = -1 };
        }

        private static MatchResult MatchStringPartsInReverseOrder(ICollection<string> parts, IList<string> recognizedValues)
        {
            for (int end = parts.Count; end >= 0; end--)
            {
                for (int start = end; start >= 0; start--)
                {
                    string partialString = LettersOrDigitsOnly(parts.Skip(start).Take(end - start).ToArray()).RemoveDiacritics();

                    if (recognizedValues.Any(v => string.Compare(partialString, LettersOrDigitsOnly(v).RemoveDiacritics(), StringComparison.CurrentCultureIgnoreCase) == 0))
                    {
                        MatchResult matchResult;

                        matchResult.FoundMatch = true;
                        matchResult.Start = start;
                        matchResult.End = end;

                        return matchResult;
                    }
                }
            }

            return new MatchResult { FoundMatch = false, Start = -1, End = -1 };
        }

        private static string ExtractUnitNumberWithPrefix(IList<string> parts)
        {
            string unitNumber = null;

            for (int i = 0; i < parts.Count; i++)
            {
                string part = parts[i];

                if (UnitNames.Any(unitName => string.Compare(part, unitName, StringComparison.CurrentCultureIgnoreCase) == 0))
                {
                    parts.RemoveAt(i);

                    while (i < parts.Count && unitNumber == null)
                    {
                        if (GetCharacterType(parts[i][0]) == CharacterType.LetterOrDigit)
                            unitNumber = parts[i];

                        parts.RemoveAt(i);
                    }

                    break;
                }
            }

            return unitNumber;
        }

        private static string ExtractUnitNumberAtStartOfAddress(IList<string> remainingParts)
        {
            string unitNumber = null;

            List<string> remainingPartsWithoutWhiteSpace =
                remainingParts.Where(part => !char.IsWhiteSpace(part[0])).Take(3).ToList();

            // If we don't have a unit number yet and there's at least three parts left))
            if (remainingPartsWithoutWhiteSpace.Count >= 3)
            {
                // And those three parts match the pattern of a:
                // 1) a string starting with digits,
                // 2) followed by a '-' or a ','
                // 3) followed by another string starting with digits

                if (char.IsDigit(remainingParts[0][0]) &&
                    (remainingPartsWithoutWhiteSpace[1] == "-" || remainingPartsWithoutWhiteSpace[1] == ",") &&
                    char.IsDigit(remainingPartsWithoutWhiteSpace[2][0]))
                {
                    unitNumber = remainingParts[0];
                    remainingParts.RemoveAt(0);
                }
            }

            return unitNumber;
        }

        private static string ExtractRecognizedValue(IList<string> parts, IList<string> recognizedValues)
        {
            MatchResult result = MatchStringPartsInReverseOrder(parts, recognizedValues);

            if (result.FoundMatch)
            {
                string partialString = LettersOrDigitsOnly(parts.Skip(result.Start).Take(result.End - result.Start).ToArray()).RemoveDiacritics();
                string recognizedValue = recognizedValues.Single(v => string.Compare(partialString, LettersOrDigitsOnly(v).RemoveDiacritics(), StringComparison.CurrentCultureIgnoreCase) == 0);
                
                for (int i = 0; i < (result.Length - 1); i++)
                    parts.RemoveAt(result.Start);

                return recognizedValue;
            }

            return null;
        }

        private static string ExtractCanadianPostalCode(IList<string> parts)
        {
            Regex canPostalCode = new Regex(@"^[ABCEGHJKLMNPRSTVXY]{1}\d{1}[A-Z]{1}\d{1}[A-Z]{1}\d{1}$", RegexOptions.Compiled);

            for (int i = 0; i < parts.Count; i++)
            {
                if (canPostalCode.IsMatch(parts[i].ToUpper()))
                {
                    string result = parts[i];

                    parts.RemoveAt(i);

                    return result.ToUpper().Insert(3, " ");
                }
            }

            Regex canPostalCode1 = new Regex(@"^[ABCEGHJKLMNPRSTVXY]{1}\d{1}[A-Z]{1}$", RegexOptions.Compiled);
            Regex canPostalCode2 = new Regex(@"^\d{1}[A-Z]{1}\d{1}$", RegexOptions.Compiled);

            for (int i = 0; i < parts.Count; i++)
            {
                if (canPostalCode1.IsMatch(parts[i].ToUpper()))
                {
                    int j = i + 1;

                    while (j < parts.Count)
                    {
                        if (char.IsLetterOrDigit(parts[j][0]))
                        {
                            if (canPostalCode2.IsMatch(parts[j]))
                            {
                                string result = $"{parts[i]} {parts[j]}".ToUpper();

                                for (int k = i; k <= j; k++)
                                    parts.RemoveAt(i);

                                return result;
                            }
                        }

                        j++;
                    }
                }
            }

            return null;
        }

        private static string ExtractAmericanPostalCode(IList<string> parts)
        {
            string result = null;

            Regex usaPostalCode1 = new Regex(@"^\d{5}$", RegexOptions.Compiled);
            Regex usaPostalCode2 = new Regex(@"^\d{4}$", RegexOptions.Compiled);

            for (int i = 0; i < parts.Count; i++)
            {
                if (usaPostalCode1.IsMatch(parts[i]))
                {
                    result = parts[i];

                    parts.RemoveAt(i);

                    if (i < parts.Count - 1 && parts[i] == "-" && usaPostalCode2.IsMatch(parts[i + 1]))
                    {
                        result = result + "-" + parts[i + 1];
                        parts.RemoveAt(i);
                        parts.RemoveAt(i);
                    }
                }
            }

            return result;
        }

        private static string ExtractPostalCode(IList<string> parts, ref string country)
        {
            string postalCode = null;

            string canadianPostalCode = ExtractCanadianPostalCode(parts);

            if (!string.IsNullOrEmpty(canadianPostalCode))
            {
                postalCode = canadianPostalCode;
                country = country ?? "Canada";
            }
            else
            {
                string usaPostalCode = ExtractAmericanPostalCode(parts);

                if (!string.IsNullOrEmpty(usaPostalCode))
                {
                    postalCode = usaPostalCode;
                    country = country ?? "USA";
                }
            }

            return postalCode;
        }

        private static string ExtractCityName(IList<string> parts)
        {
            string cityName = null;

            // Search for words that mean street
            IEnumerable<MatchResult> streets = StreetNames
                .Select(value => MatchStringParts(parts, value))
                .Where(matchResult => matchResult.FoundMatch)
                .ToReadOnlyCollection();

            // Search for directions or quadrants indicators North, South, etc...
            IEnumerable<MatchResult> directions = Directions
                .Select(value => MatchStringParts(parts, value))
                .Where(matchResult => matchResult.FoundMatch)
                .ToReadOnlyCollection();

            if (streets.Any())
            {
                // if we found a street name, the city name probably starts after that...
                int probableStartOfCityName = streets.Min(s => s.End) + 1;

                // if we also found a direction...
                if (directions.Any())
                {
                    int firstDirection = directions.Min(s => s.End) + 1;

                    // and that direction is after the street name, then the city name
                    // probably is after the direction too..
                    if (firstDirection > probableStartOfCityName)
                        probableStartOfCityName = firstDirection;
                }

                // if there are parts of the address after the point we deamed to be
                // the end of the street name
                if (probableStartOfCityName < parts.Count)
                {
                    // join them back togeather to form the city name
                    cityName = Join(parts.Skip(probableStartOfCityName));

                    // and remove those parts from the remaining address
                    while (probableStartOfCityName < parts.Count)
                        parts.RemoveAt(probableStartOfCityName);
                }
            }

            return cityName.ToTitleCase();
        }

        private static string Join(IEnumerable<string> parts, bool preserveLineBreaks = false)
        {
            StringBuilder result = new StringBuilder();
            StringBuilder partial = new StringBuilder();

            foreach (string part in parts)
            {
                switch (GetCharacterType(part[0]))
                {
                    case CharacterType.WhiteSpace:
                        if (result.Length > 0)
                        {
                            if (preserveLineBreaks && (part.Contains("\r") || part.Contains("\n")))
                                partial.AppendLine();
                            else
                                partial.Append(' ');
                        }
                        break;

                    case CharacterType.SymbolOrPuncuation:
                        if (result.Length > 0)
                        {
                            partial.Append(part);
                        }
                        break;

                    case CharacterType.LetterOrDigit:
                        result.Append(partial);
                        result.Append(part);
                        partial.Clear();
                        break;
                }
            }

            return result.Length > 0 ? result.ToString() : null;
        }

        private static void Trim(IList<string> remainingParts)
        {
            while (remainingParts.Count > 0 && !char.IsLetterOrDigit(remainingParts[0][0]))
                remainingParts.RemoveAt(0);

            while (remainingParts.Count > 0 && !char.IsLetterOrDigit(remainingParts[remainingParts.Count - 1][0]))
                remainingParts.RemoveAt(remainingParts.Count - 1);
        }

        public static AddressData Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Empty;

            List<string> remainingParts = SplitStringByCharacterTypes(input);

            string unitNumber = ExtractUnitNumberWithPrefix(remainingParts);
            string country = ExtractRecognizedValue(remainingParts, Countries);
            string postalCode = ExtractPostalCode(remainingParts, ref country);
            string province = ExtractRecognizedValue(remainingParts, ProvincesAndStates);
            string city = ExtractCityName(remainingParts);

            Trim(remainingParts);

            if (remainingParts.Count > 0 && string.IsNullOrEmpty(unitNumber))
            {
                unitNumber = ExtractUnitNumberAtStartOfAddress(remainingParts);

                Trim(remainingParts);
            }

            string streetNumber = null;
            string streetName = null;

            if (remainingParts.Count > 0 && char.IsDigit(remainingParts[0][0]))
            {
                streetNumber = remainingParts[0];
                remainingParts.RemoveAt(0);
            }

            if (remainingParts.Count > 0)
            {
                streetName = Join(remainingParts);
            }

            return new AddressData(unitNumber, streetNumber, streetName, city, province, country, postalCode);
        }

        public static AddressData ParseMailing(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Empty;

            List<string> remainingParts = SplitStringByCharacterTypes(input);

            string country = ExtractRecognizedValue(remainingParts, Countries);
            string postalCode = ExtractPostalCode(remainingParts, ref country);
            string province = ExtractRecognizedValue(remainingParts, ProvincesAndStates);
            string city = ExtractCityName(remainingParts);

            Trim(remainingParts);

            string streetName = Join(remainingParts, true);

            return new AddressData(null, null, streetName, city, province, country, postalCode);
        }

        public static AddressData ParseStreet(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Empty;

            List<string> remainingParts = SplitStringByCharacterTypes(input);

            string unitNumber = ExtractUnitNumberWithPrefix(remainingParts);

            Trim(remainingParts);

            if (remainingParts.Count > 0 && string.IsNullOrEmpty(unitNumber))
            {
                unitNumber = ExtractUnitNumberAtStartOfAddress(remainingParts);

                Trim(remainingParts);
            }

            string streetNumber = null;
            string streetName = null;

            if (remainingParts.Count > 0 && char.IsDigit(remainingParts[0][0]))
            {
                streetNumber = remainingParts[0];
                remainingParts.RemoveAt(0);
            }

            if (remainingParts.Count > 0)
            {
                streetName = Join(remainingParts);
            }

            return new AddressData(unitNumber, streetNumber, streetName, null, null, null, null);
        }

        #endregion

        #region Properties

        public string UnitNumber { get; }
        public string StreetNumber { get; }
        public string StreetName { get; }
        public string City { get; }
        public string Province { get; }
        public string Country { get; }
        public string PostalCode { get; }
        public double Latitude { get; }
        public double Longitude { get; }

        public string SingleLineAddressText => GetAddressText(AddressTextFormatOptions.SingleLine);

        public string MultiLineAddressText => GetAddressText();

        #endregion

        #region Methods

        public AddressData Merge(AddressData fallbackData)
        {
            return new AddressData(
                UnitNumber ?? fallbackData.UnitNumber,
                StreetNumber ?? fallbackData.StreetNumber,
                StreetName ?? fallbackData.StreetName,
                City ?? fallbackData.City,
                Province ?? fallbackData.Province,
                Country ?? fallbackData.Country,
                PostalCode ?? fallbackData.PostalCode,
                Math.Abs(Latitude) > 0.00000000001 ? Latitude : fallbackData.Latitude,
                Math.Abs(Longitude) > 0.00000000001 ? Longitude : fallbackData.Latitude);
        }

        public string GetAddressText(AddressTextFormatOptions options = AddressTextFormatOptions.None)
        {
            if (this == Empty) return string.Empty;

            bool hasUnitNumber = !string.IsNullOrWhiteSpace(UnitNumber);
            bool hasStreetNumber = !string.IsNullOrWhiteSpace(StreetNumber);
            bool hasStreetName = !string.IsNullOrWhiteSpace(StreetName);
            bool hasCity = !string.IsNullOrWhiteSpace(City);
            bool hasProvince = !string.IsNullOrWhiteSpace(Province);
            bool hasCountry = !string.IsNullOrWhiteSpace(Country);
            bool hasPostalCode = !string.IsNullOrWhiteSpace(PostalCode);

            StringBuilder stringBuilder = new StringBuilder();

            if (hasUnitNumber)
                stringBuilder.Append(UnitNumber.Trim());

            if (hasUnitNumber && (hasStreetName || hasStreetNumber))
                stringBuilder.Append('-');

            if (hasStreetNumber)
                stringBuilder.Append(StreetNumber.Trim());

            if (hasStreetName && hasStreetNumber)
                stringBuilder.Append(' ');

            if (hasStreetName)
                stringBuilder.Append(StreetName.Trim());

            if (stringBuilder.Length > 0)
            {
                if (options.HasFlag(AddressTextFormatOptions.SingleLine))
                    stringBuilder.Append(", ");
                else
                    stringBuilder.AppendLine();
            }

            if (hasCity)
                stringBuilder.Append(City.Trim());

            if (hasCity && hasProvince)
                stringBuilder.Append(' ');

            if (hasProvince)
                stringBuilder.Append(Province.Trim());

            if (hasPostalCode && (hasCity || hasProvince))
                stringBuilder.Append("  ");

            if (hasPostalCode)
                stringBuilder.Append(PostalCode.Trim());

            if (hasCountry && !options.HasFlag(AddressTextFormatOptions.ExcludeCountry))
            {
                if (stringBuilder.Length > 0)
                {
                    if (options.HasFlag(AddressTextFormatOptions.SingleLine))
                        stringBuilder.Append(", ");
                    else
                        stringBuilder.AppendLine();
                }

                stringBuilder.Append(Country.Trim());
            }

            return stringBuilder.ToString();
        }

        #endregion

        #region Equality Operations

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            AddressData other = (AddressData)obj;

            return
                UnitNumber == other.UnitNumber &&
                StreetNumber == other.StreetNumber &&
                StreetName == other.StreetName &&
                City == other.City &&
                Province == other.Province &&
                Country == other.Country &&
                PostalCode == other.PostalCode &&
                Equals(Latitude, other.Latitude) &&
                Equals(Longitude, other.Longitude);
        }

        public static bool operator ==(AddressData left, AddressData right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AddressData left, AddressData right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return GetAddressText(AddressTextFormatOptions.SingleLine);
        }

        #endregion
    }
}