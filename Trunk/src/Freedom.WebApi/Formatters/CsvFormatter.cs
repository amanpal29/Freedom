using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using Freedom.Extensions;
using System.Collections;
using System.Text;

namespace Freedom.WebApi.Formatters
{
    public class CsvFormatter : BufferedMediaTypeFormatter
    {
        private static readonly char[] SpecialChars = { ',', '\n', '\r', '"' };
        
        public CsvFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/csv"));

            SupportedEncodings.Add(new UTF8Encoding(false));
            SupportedEncodings.Add(Encoding.ASCII);
            SupportedEncodings.Add(Encoding.GetEncoding("iso-8859-1"));
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            if (type == null || type.IsValueType || type == typeof(string))
                return false;

            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        private static string EscapeSpecialCharacters(string input)
        {
            if (input == null)
                return null;

            if (input.Length == 0)
                return "\"\"";

            if (input.IndexOfAny(SpecialChars) < 0)
                return input;

            return '"' + input.Replace("\"", "\"\"") + '"';
        }

        private static MethodInfo EscapeSpecialCharactersMethod
        {
            get
            {
                Expression<Func<string, string>> expression = x => EscapeSpecialCharacters(x);

                MethodCallExpression methodCallExpression = (MethodCallExpression) expression.Body;

                return methodCallExpression.Method;
            }
        }

        private static Func<object, string> BuildMemberAccessDelegate(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            if (property.DeclaringType == null)
                throw new ArgumentException("property must have a DeclaringType.", nameof(property));

            ParameterExpression parameter = Expression.Parameter(typeof(object), "x");

            UnaryExpression typedParameter = Expression.Convert(parameter, property.DeclaringType);

            MemberExpression member = Expression.MakeMemberAccess(typedParameter, property);

            Expression body;

            if (property.PropertyType == typeof(string))
            {
                body = Expression.Call(EscapeSpecialCharactersMethod, member);
            }
            else if (property.PropertyType.IsValueType)
            {
                MethodInfo toStringMethod = member.Type.GetMethod(nameof(ToString), new Type[0]);

                MethodCallExpression toString = Expression.Call(member, toStringMethod);

                body = Expression.Call(EscapeSpecialCharactersMethod, toString);
            }
            else
            {
                ConstantExpression nullObjectConstant = Expression.Constant(null, typeof(object));

                ConstantExpression nullStringConstant = Expression.Constant(null, typeof(string));

                BinaryExpression isNullConditional = Expression.Equal(member, nullObjectConstant);

                MethodInfo toStringMethod = member.Type.GetMethod(nameof(ToString), new Type[0]);

                MethodCallExpression toString = Expression.Call(member, toStringMethod);

                MethodCallExpression toStringEscaped = Expression.Call(EscapeSpecialCharactersMethod, toString);

                body = Expression.Condition(isNullConditional, nullStringConstant, toStringEscaped);
            }

            return Expression.Lambda<Func<object, string>>(body, parameter).Compile();
        }

        private static IList<KeyValuePair<string, Func<object, string>>> BuildColumnMap(Type inputType)
        {
            List<KeyValuePair<string, Func<object, string>>> result = new List<KeyValuePair<string, Func<object, string>>>();

            foreach (PropertyInfo property in inputType.GetProperties())
            {
                if (!property.IsBrowsable()) continue;

                string displayName = property.GetDisplayName();

                Func<object, string> value = BuildMemberAccessDelegate(property);

                result.Add(new KeyValuePair<string, Func<object, string>>(displayName, value));
            }

            return result;
        }

        public override void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
        {
            Encoding effectiveEncoding = SelectCharacterEncoding(content.Headers);

            IEnumerator enumerator = ((IEnumerable) value).GetEnumerator();

            try
            {
                using (StreamWriter writer = new StreamWriter(writeStream, effectiveEncoding))
                {
                    if (!enumerator.MoveNext()) return; // Nothing to write

                    IList<KeyValuePair<string, Func<object, string>>> columnMap =
                        BuildColumnMap(enumerator.Current.GetType());

                    List<string> keys = columnMap.Select(x => x.Key).ToList();

                    List<Func<object, string>> values = columnMap.Select(x => x.Value).ToList();

                    // Write the column name headers
                    writer.WriteLine(string.Join(",", keys));

                    do
                    {
                        object item = enumerator.Current;

                        writer.WriteLine(string.Join(",", values.Select(x => x(item))));
                    } while (enumerator.MoveNext());
                }

            }
            finally 
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }
    }
}
