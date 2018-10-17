using System;
using System.Collections.Generic;
using System.Linq;
using Freedom.Exceptions;

namespace Freedom.Parsers
{
    public struct PersonName
    {
        #region Fields

        public static readonly PersonName Empty = new PersonName();

        #endregion

        #region Constants

        private static readonly string[] Titles =  {
                                                     "Mr.", "Mrs.", "Ms.", "Miss",
                                                     "Dr.", "Doctor", "Prof.", "Professor", "Judge", "Sir", "Dame",
                                                     "Bishop", "Father", "Pastor", "Rabbi", "Rev.", "Reverend", "Sister" 
                                                   };

        private static readonly string[] Suffixes = {
                                                        "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X",
                                                        "Jr.", "Sr."
                                                    };

        #endregion

        #region Constructors

        public PersonName(string title, string firstName, string middleName, string lastName, string suffix)
        {
            Title = title;
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            Suffix = suffix;
        }

        #endregion

        #region ParseCore

        private static bool TryParseTitle(string input, out string title)
        {
            string value = input.TrimEnd('.');

            foreach (string str in Titles)
            {
                if (string.Compare(value, str.TrimEnd('.'), StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    title = str;
                    return true;
                }
            }

            title = null;
            return false;
        }

        private static bool TryParseSuffix(string input, out string suffix)
        {
            string value = input.TrimEnd('.');

            foreach (string str in Suffixes)
            {
                if (string.Compare(value, str.TrimEnd('.'), StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    suffix = str;
                    return true;
                }
            }

            suffix = null;
            return false;
        }

        private static string ParseCore(string input, out PersonName result)
        {
            result = Empty;

            string title = null;
            string firstName = null;
            string middleName = null;
            string lastName;
            string suffix = null;

            if (string.IsNullOrWhiteSpace(input))
                return null;

            List<string> nameParts = input.Split(" \t\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

            if (nameParts.Count > 1 && TryParseTitle(nameParts[0], out title))
                nameParts.RemoveAt(0);

            if (nameParts.Count > 1 && TryParseSuffix(nameParts.Last(), out suffix))
                nameParts.RemoveAt(nameParts.Count - 1);

            switch (nameParts.Count)
            {
                case 0:
                    return "Unable to parse string.";

                case 1:
                    lastName = nameParts[0];
                    break;

                case 2:
                    firstName = nameParts[0];
                    lastName = nameParts[1];
                    break;

                case 3:
                    firstName = nameParts[0];
                    middleName = nameParts[1];
                    lastName = nameParts[2];
                    break;

                default:
                    firstName = nameParts[0];
                    middleName = string.Join(" ", nameParts.Skip(1).Take(nameParts.Count - 2));
                    lastName = nameParts.Last();
                    break;
            }

            result = new PersonName(title, firstName, middleName, lastName, suffix);

            return null;
        }

        #endregion

        #region ParseAndTryParse

        public static PersonName Parse(string fullName)
        {
            PersonName result;

            string message = ParseCore(fullName, out result);

            if (!string.IsNullOrEmpty(message))
                throw new AmbiguousValueException(message);

            return result;
        }

        public static bool TryParse(string fullName, out PersonName result)
        {
            return string.IsNullOrEmpty(ParseCore(fullName, out result));
        }

        #endregion

        #region Properties

        public string Title { get; }
        public string FirstName { get; }
        public string MiddleName { get; }
        public string LastName { get; }
        public string Suffix { get; }

        #endregion

        #region Equality Operations

        public static bool operator == (PersonName left, PersonName right)
        {
            return Equals(left, right);
        }

        public static bool operator != (PersonName left, PersonName right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            PersonName other = (PersonName) obj;

            return
                Title == other.Title &&
                FirstName == other.FirstName &&
                MiddleName == other.MiddleName &&
                LastName == other.LastName &&
                Suffix == other.Suffix;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            string[] nameParts = { Title, FirstName, MiddleName, LastName, Suffix };

            return string.Join(" ", nameParts.Where(p => !string.IsNullOrEmpty(p)));
        }
    
        #endregion
    }
}
