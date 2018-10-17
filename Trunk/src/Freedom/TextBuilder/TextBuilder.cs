using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Freedom.TextBuilder
{
    public class TextBuilder
    {
        #region Fields

        private static readonly Regex FieldRegex = new Regex(
            @"\{(?<FieldName>[A-Z_][A-Z0-9_]*)(\[(?<ArrayIndex>\d+)])?(:(?<Format>[^:\}]+))?\}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly StringBuilder _sb = new StringBuilder();
        private readonly string _format;
        private readonly int _length;

        private BindingFlags _bindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
        
        private List<TextBuilderError> _errors;

        #endregion

        #region Public Static Methods

        public static string Format(string format, params object[] args)
        {
            if (string.IsNullOrEmpty(format))
                return format;

            TextBuilder textBuilder = new TextBuilder(format);

            if (textBuilder.Build(args) > 0)
                throw new TextBuilderException(textBuilder.Errors);

            return textBuilder.ToString();
        }

        public static int TryFormat(string format, out string result, params object[] args)
        {
            if (string.IsNullOrEmpty(format))
            {
                result = format;
                return 0;
            }

            TextBuilder textBuilder = new TextBuilder(format);

            int errors = textBuilder.Build(args);

            result = textBuilder.ToString();

            return errors;
        }

        public static string[] GetFieldList(string formatString)
        {
            if (string.IsNullOrWhiteSpace(formatString))
                return new string[0];

            MatchCollection matches = FieldRegex.Matches(formatString);

            string[] result = new string[matches.Count];

            for (int i = 0; i < matches.Count; i++)
                result[i] = matches[i].Groups["FieldName"].Value;

            return result;
        }

        #endregion

        #region Constructor

        public TextBuilder(string format)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            _format = format;
            _length = format.Length;
        }

        #endregion

        #region Properties

        public BindingFlags BindingFlags
        {
            get { return _bindingFlags; }
            set { _bindingFlags = value; }
        }

        public string FormatString => _format;

        public TextBuilderError[] Errors => _errors?.ToArray() ?? TextBuilderException.DefaultErrors;

        #endregion

        #region Public Methods

        public void Reset()
        {
            _sb.Clear();
            _errors = null;
        }

        public int Build(params object[] args)
        {
            if (string.IsNullOrEmpty(_format))
                return 0;

            for (int i = 0; i < _length; i++)
            {
                switch (_format[i])
                {
                    case '{':
                        // Check if the string ends with an opening brace
                        if (i == _length - 1)
                        {
                            AddError(i, TextBuilderErrorCode.OpeningBraceWithoutClosingBrace);
                            return _errors.Count;
                        }

                        // Check if this is two braces in a row
                        if (_format[i + 1] == '{')
                        {
                            _sb.Append('{');
                            i++;
                            continue;
                        }

                        // Find the end of the variable
                        int variableStart = i;

                        for (i++; ; i++)
                        {
                            if (i == _length)
                            {
                                AddError(variableStart, TextBuilderErrorCode.OpeningBraceWithoutClosingBrace);
                                variableStart++;
                                _sb.Append(_format, variableStart, _length - variableStart);
                                return _errors.Count;
                            }

                            if (_format[i] == '}' || _format[i] == ':')
                                break;
                        }

                        int variableLength = i - variableStart - 1;

                        if (variableLength < 1)
                        {
                            // There is nothing between the opening an closing brace
                            AddError(variableStart, TextBuilderErrorCode.EmptyBraces);
                            continue;
                        }

                        string variableFormat = null;

                        if (_format[i] == ':')
                        {
                            int formatStart = i;

                            for (i++;; i++)
                            {
                                if (i == _length)
                                {
                                    AddError(variableStart, TextBuilderErrorCode.OpeningBraceWithoutClosingBrace);
                                    variableStart++;
                                    _sb.Append(_format, variableStart, _length - variableStart);
                                    return _errors.Count;
                                }

                                if (_format[i] == '}')
                                    break;
                            }

                            if (i - formatStart <= 1)
                            {
                                AddError(formatStart, TextBuilderErrorCode.EmptyFormat);
                            }
                            else
                            {
                                variableFormat = _format.Substring(formatStart + 1, i - formatStart - 1);
                            }
                        }

                        object value = Evaluate(variableStart + 1, variableLength, args);

                        if (value != null)
                        {
                            IFormattable formattable = value as IFormattable;
                            if (formattable != null && variableFormat != null)
                            {
                                try
                                {
                                    _sb.Append(formattable.ToString(variableFormat, null));
                                }
                                catch (FormatException)
                                {
                                    AddError(variableStart, TextBuilderErrorCode.FormatException);
                                }
                            }
                            else
                            {
                                _sb.Append(value);
                            }
                        }
                        break;

                    case '}':
                        _sb.Append('}');    
                    
                        // Unless this is two closing braces in a row, a brace outside of a variable is an error
                        if (i == _length - 1 || _format[i + 1] != '}')
                        {
                            AddError(i, TextBuilderErrorCode.ClosingBraceWithoutOpeningBrace);
                        }
                        else
                        {
                            // It is two closing braces in a row, skip the second
                            i++;
                        }

                        break;

                    default:
                        _sb.Append(_format[i]);
                        break;
                }
            }

            return _errors?.Count ?? 0;
        }

        #endregion

        #region Private Helper Methods
        
        private object Evaluate(int startIndex, int length, object[] args)
        {
            string[] parts = _format.Substring(startIndex, length).Split('.');

            foreach (string part in parts)
            {
                if (string.IsNullOrEmpty(part))
                {
                    AddError(startIndex, TextBuilderErrorCode.BadPath);
                    return null;
                }
            }

            int i = 0;
            object currentObject;

            if (IsAsciiDigit(parts[0][0]))
            {
                int index;

                if (!IsAsciiDigits(parts[0]) || !int.TryParse(parts[0], out index))
                {
                    AddError(startIndex, TextBuilderErrorCode.BadPath);
                    return null;
                }

                if (index >= args.Length)
                {
                    AddError(startIndex, TextBuilderErrorCode.ArgumentNotFound);
                    return null;
                }

                currentObject = args[index];
                i++;
            }
            else
            {
                currentObject = args[0];
            }

            bool hadNullReference = false;
            Type currentObjectType = currentObject?.GetType() ?? typeof(object);

            for (; i < parts.Length; i++)
            {
                TextBuilderErrorCode error = Evaluate(ref currentObject, ref currentObjectType, parts[i]);

                switch (error)
                {
                    case TextBuilderErrorCode.NoError:
                        break;

                    case TextBuilderErrorCode.NullReference:
                        hadNullReference = true;
                        break;

                    default:
                        AddError(startIndex, error);
                        return null;
                }
            }

            if (hadNullReference)
                AddError(startIndex, TextBuilderErrorCode.NullReference);

            return currentObject;
        }

        private TextBuilderErrorCode Evaluate(ref object argument, ref Type argumentType, string path)
        {
            if (string.IsNullOrEmpty(path))
                return TextBuilderErrorCode.BadPath;

            if (IsAsciiDigit(path[0]))
            {
                int index;

                if (!IsAsciiDigits(path) || !int.TryParse(path, out index))
                    return TextBuilderErrorCode.BadPath;

                if (!typeof (IEnumerable).IsAssignableFrom(argumentType))
                    return TextBuilderErrorCode.AccessByIndexOfNonEnumerableType;

                object result;

                if (!TryGetAt((IEnumerable) argument, index, out result))
                {
                    argument = null;
                    argumentType = GetEnumerableType(argumentType);
                    return TextBuilderErrorCode.ArgumentOutOfRange;
                }

                argument = result;
                argumentType = result?.GetType() ?? GetEnumerableType(argumentType);
                return TextBuilderErrorCode.NoError;
            }

            foreach (char ch in path)
                if (!IsAsciiLetter(ch) && !IsAsciiDigit(ch) && ch != '_')
                    return TextBuilderErrorCode.BadPath;

            return TryGet(ref argument, ref argumentType, path);
        }

        private static bool IsAsciiDigit(char ch)
        {
            return '0' <= ch && ch <= '9';
        }

        private static bool IsAsciiLetter(char ch)
        {
            return ('A' <= ch && ch <= 'Z') || ('a' <= ch && ch <= 'z');
        }

        private static bool IsAsciiDigits(string s)
        {
            foreach (char ch in s)
                if (!IsAsciiDigit(ch))
                    return false;

            return true;
        }

        private TextBuilderErrorCode TryGet(ref object argument, ref Type argumentType, string memberName)
        {
            try
            {
                PropertyInfo property = argumentType.GetProperty(memberName, _bindingFlags);

                if (property != null)
                {
                    if (argument == null)
                    {
                        argumentType = property.PropertyType;
                        return TextBuilderErrorCode.NullReference;
                    }

                    argument = property.GetValue(argument, null);
                    argumentType = argument?.GetType() ?? property.PropertyType;
                    return TextBuilderErrorCode.NoError;
                }

                FieldInfo field = argumentType.GetField(memberName, _bindingFlags);

                if (field != null)
                {
                    if (argument == null)
                    {
                        argumentType = field.FieldType;
                        return TextBuilderErrorCode.NullReference;
                    }

                    argument = field.GetValue(argument);
                    argumentType = argument?.GetType() ?? field.FieldType;

                    return TextBuilderErrorCode.NoError;
                }

                argument = null;
                argumentType = typeof(object);
                return TextBuilderErrorCode.MemberNotFound;
            }
            catch (AmbiguousMatchException)
            {
                argument = null;
                argumentType = typeof(object);
                return TextBuilderErrorCode.AmbiguousMatch;
            }
        }

        private static bool TryGetAt(IEnumerable enumerable, int index, out object result)
        {
            result = null;

            if (index < 0)
                return false;

            // Try to shortcut if it's a list...

            IList list = enumerable as IList;

            if (list != null)
            {
                if (index >= list.Count)
                    return false;

                result = list[index];

                return true;
            }

            // Or a string...

            string stringValue = enumerable as string;

            if (stringValue != null)
            {
                if (index >= stringValue.Length)
                    return false;

                result = stringValue[index];

                return true;
            }

            // Otherwise Iterate the enumerable...

            IEnumerator enumerator = enumerable.GetEnumerator();

            try
            {
                while (enumerator.MoveNext())
                {
                    if (index-- != 0)
                        continue;

                    result = enumerator.Current;

                    return true;
                }

                return false;
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }

        private static Type GetEnumerableType(Type type)
        {
            // Check if it's a string
            if (type == typeof(string))
                return typeof(char);

            // Check if it's an IEnumerable<T>
            foreach (Type iface in type.GetInterfaces())
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof (IEnumerable<>))
                    return iface.GetGenericArguments()[0];

            // Check if it's an array
            if (type.IsArray)
                return type.GetElementType();

            // It must be a generic list then
            return typeof (object);
        }

        private void AddError(int index, TextBuilderErrorCode code)
        {
            if (_errors == null)
                _errors = new List<TextBuilderError>();

            int row = 1;
            int column = 0;

            for (int i = 0; i <= index; i++)
            {
                switch (_format[i])
                {
                    case '\r':
                        if (i < _length - 1 && _format[i + 1] == '\n')
                            i++;
                        goto case '\n';

                    case '\n':
                        row++;
                        column = 0;
                        break;

                    default:
                        column++;
                        break;
                }
            }

            _errors.Add(new TextBuilderError(row, column, index, code));
        }

        #endregion

        #region Overrides of object

        public override string ToString()
        {
            return _sb.ToString();
        }

        #endregion
    }
}
