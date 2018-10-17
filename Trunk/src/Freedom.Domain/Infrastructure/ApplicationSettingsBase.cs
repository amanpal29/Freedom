using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Freedom.Collections;

namespace Freedom.Domain.Infrastructure
{
    public abstract class ApplicationSettingsBase
    {
        #region Default Value
        
        #endregion

        #region Conversion Methods

        private static bool? ToBool(string value)
        {
            bool result;

            return bool.TryParse(value, out result) ? result : (bool?) null;
        }

        private static int? ToInt(string value)
        {
            int result;

            return int.TryParse(value, out result) ? result : (int?) null;
        }

        private static int? ToInt(string value, int? minimum, int? maximum)
        {
            int result;

            if (minimum.HasValue && maximum.HasValue && minimum > maximum)
                throw new ArgumentException("minimum must be less than maximum.");

            if (int.TryParse(value, out result))
            {
                if (minimum.HasValue && result < minimum)
                    return minimum;

                if (maximum.HasValue && result > maximum)
                    return maximum;

                return result;
            }

            return null;
        }

        private static ShortTimeSpan? ToShortTimeSpan(string value)
        {
            ShortTimeSpan result;

            return ShortTimeSpan.TryParse(value, out result) ? result : (ShortTimeSpan?) null;
        }

        private static decimal? ToDecimal(string value, decimal? minimum, decimal? maximum)
        {
            decimal result;

            if (minimum.HasValue && maximum.HasValue && minimum > maximum)
                throw new ArgumentException("minimum must be less than maximum.");

            if (decimal.TryParse(value, out result))
            {
                if (minimum.HasValue && result < minimum)
                    return minimum;

                if (maximum.HasValue && result > maximum)
                    return maximum;

                return result;
            }

            return null;
        }

        private static AutoNumberSettings ToAutoNumberSettings(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? new AutoNumberSettings() : AutoNumberSettings.FromString(value);
        }

        private static Guid? ToGuid(string value)
        {
            Guid result;

            return Guid.TryParse(value, out result) ? result : (Guid?) null;
        }

        private static TEnum ToEnum<TEnum>(string value, TEnum defaultValue = default(TEnum))
            where TEnum : struct
        {
            TEnum result;

            if (!Enum.TryParse(value, out result))
                result = defaultValue;

            return result;
        }

        private static string FromEnum<TEnum>(TEnum value, params TEnum[] invalidValues)
            where TEnum : struct
        {
            if (invalidValues.Any(invalidValue => value.Equals(invalidValue)))
                return null;

            return Enum.IsDefined(typeof(TEnum), value) ? value.ToString() : null;
        }

        private static OrderedPairCollection<bool, int?> ToOrderedPairCollection(string delimitedString, int? maximumSize = null)
        {
            OrderedPairCollection<bool, int?> collection = new OrderedPairCollection<bool, int?>(maximumSize);

            IEnumerable<string> tokens = Enumerable.Empty<string>();

            if (!string.IsNullOrEmpty(delimitedString))
            {
                tokens = delimitedString.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }

            foreach (string token in tokens)
            {
                string item1String = token.Substring(0, token.IndexOf(','));
                string item2String = token.Substring(token.IndexOf(',') + 1);

                Tuple<bool, int?> item = !string.IsNullOrEmpty(item2String) 
                    ? new Tuple<bool, int?>(bool.Parse(item1String), int.Parse(item2String)) 
                    : new Tuple<bool, int?>(bool.Parse(item1String), null);

                collection.Add(item);
            }

            return collection;
        }

        #endregion

        #region Settings

        #region System Access

        public int MinimumPasswordLength
        {
            get { return ToInt(this["MinimumPasswordLength"], 0, 127) ?? 8; }
            set { this["MinimumPasswordLength"] = value.ToString("d"); }
        }

        public int MinimumPasswordComplexity
        {
            get { return ToInt(this["MinimumPasswordComplexity"], 0, 5) ?? 3; }
            set { this["MinimumPasswordComplexity"] = value.ToString("d"); }
        }

        #endregion
                   
        #region DateTime Format

        public string DateFormat
        {
            get { return this["DateFormat"] ?? CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern; }
            set { this["DateFormat"] = value; }
        }

        public string TimeFormat
        {
            get { return this["TimeFormat"] ?? "HH:mm"; }
            set { this["TimeFormat"] = value; }
        }

        public string DateTimeStampFormat => $"{DateFormat} {TimeFormat}";

        #endregion 

        #region Database

        public int? DatabaseRevision => ToInt(this["DatabaseRevision"]);

        public Guid GlobalId => ToGuid(this["GlobalId"]) ?? Guid.Empty;

        public Guid? UpgradeFromGlobalId => ToGuid(this["UpgradeFromGlobalId"]);

        public string GlobalInstanceName
        {
            get { return this["GlobalInstanceName"]; }
            set { this["GlobalInstanceName"] = value; }
        }

        #endregion

        #endregion

        #region Indexer

        public abstract string this[string key] { get; set; }

        #endregion
    }
}