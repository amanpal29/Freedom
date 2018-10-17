﻿using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace DemoDataBuilder.Extensions
{
    public static class StringExtensions
    {
        public static string ToCSV(this string value)
        {
            if (value == null)
                return null;

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        public static string ToKey(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            char[] result = new char[value.Length];

            int i = 0;

            foreach (char c in value)
            {
                if (char.IsWhiteSpace(c)) continue;

                switch (c)
                {
                    case '²':
                        result[i++] = '2';
                        break;
                    case '³':
                        result[i++] = '3';
                        break;
                    default:
                        result[i++] = char.ToLowerInvariant(c);
                        break;
                }
            }

            return new string(result, 0, i);
        }

        /// <summary>
        /// Converts a Camel Case string to a full string.
        /// (e.g. "TheValue".ToDisplayName() == "The Value")
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string ToDisplayName(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            StringBuilder builder = new StringBuilder();

            builder.Append(char.ToUpper(value[0]));

            for (int i = 1; i < value.Length; i++ )
            {
                if (char.IsLower(value[i - 1]) && (char.IsUpper(value[i]) || char.IsDigit(value[i])))
                    builder.Append(' ');

                builder.Append(value[i]);
            }

            return builder.ToString();
        }


        /// <summary>
        /// Converts the string to title case.
        /// i.e. Makes the first letter of each word upper case.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The value in title case.</returns>
        public static string ToTitleCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
        }

        /// <summary>
        /// Removes the diacritics (ancillary glyphs added to a letter) from the characters in a string.
        /// i.e. it replaces the accented characters in a string with their non-accented equivilents.
        /// e.g. "Québec".RemoveDiacritics() == "Quebec"
        /// </summary>
        /// <param name="value">The value with accents.</param>
        /// <returns>The value without diacritics.</returns>
        public static string RemoveDiacritics(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            string formD = value.Normalize(NormalizationForm.FormD);

            StringBuilder sb = new StringBuilder();

            foreach (char ch in formD)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Converts the string to a valid file name by replacing invalid chars with underscores.
        /// (e.g. "Backup on 09/11/2001".ToValidFileName() == "Backup on 09_11_2001")
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A valid filename.</returns>
        public static string ToValidFileName(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            StringBuilder result = new StringBuilder();

            char[] invalidChars = Path.GetInvalidFileNameChars();

            foreach (char c in value.RemoveDiacritics())
            {
                if (' ' <= c && c <= '~' && Array.IndexOf(invalidChars, value) < 0)
                {
                    result.Append(c);
                }
                else
                {
                    result.Append("_");
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Splits a string based on a delimiter including previous values
        /// 
        /// e.g. "this.is.an.example".RunningSplit('.') would return:
        ///     this
        ///     this.is
        ///     this.is.an
        ///     this.is.an.example
        /// </summary>
        /// <param name="value">the string to be split</param>
        /// <param name="seperator">the char to use as a seperator when splitting</param>
        /// <returns>The running splits of the string.</returns>
        public static string[] RunningSplit(this string value, char seperator)
        {
            string[] splits = value.Split(new[] {seperator}, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 1; i < splits.Length; i++)
                splits[i] = splits[i - 1] + seperator + splits[i];

            return splits;
        }
    }
}
