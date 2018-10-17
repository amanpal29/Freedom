using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Freedom.Parsers
{
    public static class CommandLineParser
    {
        private static readonly Regex SwitchRegex = new Regex(@"^(-{1,2}|/)(?<Switch>(\w|\?)+)((:|=)(?<Value>.*))?$", RegexOptions.Compiled);

        private static HashSet<PropertyDescriptor> GetRequiredProperties(Type componentType)
        {
            IEnumerable<PropertyDescriptor> requiredProperties =
                TypeDescriptor.GetProperties(componentType).OfType<PropertyDescriptor>()
                    .Where(pd => pd.Attributes.OfType<RequiredParameterAttribute>().Any());

            return new HashSet<PropertyDescriptor>(requiredProperties);
        }

        private static IDictionary<string, PropertyDescriptor> GetPropertyDescriptors(Type componentType)
        {
            Dictionary<string, PropertyDescriptor> result = new Dictionary<string, PropertyDescriptor>();

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(componentType);

            foreach (PropertyDescriptor property in properties)
            {
                if (property.IsBrowsable)
                    result[property.Name.ToLowerInvariant()] = property;

                foreach (AlternateNameAttribute attribute in property.Attributes.OfType<AlternateNameAttribute>().Where(a => !string.IsNullOrEmpty(a.Name)))
                    result[attribute.Name.ToLowerInvariant()] = property;
            }

            return result;
        }

        public static bool TryParse<TObject>(IEnumerable<string> args, out TObject result)
            where TObject : new()
        {
            return TryParse(args, TextWriter.Null, out result);
        }

        public static bool TryParse<TObject>(IEnumerable<string> args, TextWriter errors, out TObject result)
            where TObject : new()
        {
            bool success = true;

            result = new TObject();

            HashSet<PropertyDescriptor> requiredProperties = GetRequiredProperties(typeof (TObject));

            IDictionary<string, PropertyDescriptor> properties = GetPropertyDescriptors(typeof (TObject));

            foreach (string arg in args)
            {
                Match match = SwitchRegex.Match(arg);

                string switchName = match.Success ? match.Groups["Switch"].Value.ToLowerInvariant() : string.Empty;

                if (properties.ContainsKey(switchName))
                {
                    PropertyDescriptor property = properties[switchName];

                    if (property.PropertyType == typeof (bool))
                        property.SetValue(result, true);
                    else if (property.PropertyType == typeof (string))
                        property.SetValue(result, match.Groups["Value"].Value);

                    requiredProperties.Remove(property);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(switchName))
                        errors.WriteLine("Unrecognised argument: {0}", switchName);

                    success = false;
                }
            }

            foreach (PropertyDescriptor property in requiredProperties)
            {
                errors.WriteLine("Argument {0} is required", property.DisplayName);

                success = false;
            }

            return success;
        }
    }
}
