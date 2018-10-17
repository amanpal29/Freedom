using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Freedom.DomainGenerator
{
    public static class CodeGenerationTools
    {
        public static string ToFieldName(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            if (input.Length == 1)
                return "_" + input.ToLower(CultureInfo.CurrentCulture);

            return "_" + char.ToLower(input[0], CultureInfo.CurrentCulture) + input.Substring(1);
        }

        public static string ToDisplayName(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            StringBuilder builder = new StringBuilder();

            builder.Append(char.ToUpper(value[0]));

            for (int i = 1; i < value.Length; i++)
            {
                if (char.IsLower(value[i - 1]) && (char.IsUpper(value[i]) || char.IsDigit(value[i])))
                    builder.Append(' ');

                builder.Append(value[i]);
            }

            return builder.ToString();
        }

        public static string Format(this IEnumerable<string> list, string seperator,
            string prefix = null, string suffix = null)
        {
            if (list == null)
                return string.Empty;

            using (IEnumerator<string> enumerator = list.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return string.Empty;

                StringBuilder result = new StringBuilder();

                if (!string.IsNullOrEmpty(prefix))
                    result.Append(prefix);

                result.Append(enumerator.Current);

                while (enumerator.MoveNext())
                {
                    result.Append(seperator);
                    result.Append(enumerator.Current);
                }

                if (!string.IsNullOrEmpty(suffix))
                    result.Append(suffix);

                return result.ToString();
            }
        }
    }
}
