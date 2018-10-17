using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Freedom.Domain.Model;
using Freedom.Domain.Services.DataLoader;
using Freedom;
using log4net;

namespace Hedgehog.MSBuild.Services.DataLoader
{
    public class XmlEntityDataLoader : IEntityDataLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<XmlDocument> _documents = new List<XmlDocument>();
        private readonly Dictionary<Tuple<string, Guid>, Entity> _entityDictionary = new Dictionary<Tuple<string, Guid>, Entity>();

        public void LoadFromFiles(params string[] filenames)
        {
            foreach (string filename in filenames)
            {
                Log.Info($"XmlEntityDataLoader loading data from {filename}");

                XmlDocument document = new XmlDocument();

                document.Load(filename);

                if (document.DocumentElement == null) continue;

                _documents.Add(document);
            }

            Log.Info("Converting data to objects (this can take a while).");

            LoadData();

            Log.Info("XmlEntityDataLoader finished loading data into memory.");
        }

        public void LoadFromStreams(Stream[] streams)
        {
            Log.InfoFormat("XmlEntityDataLoader loading data from stream.");

            foreach (Stream stream in streams)
            {
                XmlDocument document = new XmlDocument();

                document.Load(stream);

                if (document.DocumentElement == null) continue;

                _documents.Add(document);
            }

            Log.Info("Converting data to objects (this can take a while).");

            LoadData();

            Log.Info("XmlEntityDataLoader finished loading data into memory.");
        }

        public IEnumerable<Entity> Results => _entityDictionary.Values;

        private static Type GetEntityTypeForNode(XmlNode childNode)
        {
            string fullTypeName =
                $"{typeof (EntityBase).Namespace}.{childNode.Name}, {typeof (EntityBase).Assembly.FullName}";

            Type entityType = Type.GetType(fullTypeName);

            return typeof(Entity).IsAssignableFrom(entityType) ? entityType : null;
        }

        private static Guid GetEntityId(XmlNode entityNode)
        {
            Guid result = Guid.Empty;

            XmlNode entityIdNode = entityNode.ChildNodes.OfType<XmlNode>().Single(n => n.Name == "Id");

            if (entityIdNode?.FirstChild == null || entityIdNode.FirstChild.NodeType != XmlNodeType.Text)
                return result;

            string entityIdValueAsString = entityIdNode.FirstChild.Value;

            Guid.TryParse(entityIdValueAsString, out result);

            return result;
        }

        private static void SetEntityId(XmlElement entityNode, Guid id)
        {
            if (entityNode == null)
                throw new ArgumentNullException(nameof(entityNode));

            if (entityNode.OwnerDocument == null)
                throw new ArgumentException("entityNode must have an OwnerDocument");

            foreach (XmlNode badIdNode in entityNode.GetElementsByTagName("Id").OfType<XmlNode>().ToArray())
                entityNode.RemoveChild(badIdNode);

            XmlDocument ownerDocument = entityNode.OwnerDocument;

            if (ownerDocument != null)
            {
                entityNode
                    .AppendChild(ownerDocument.CreateElement("Id"))
                    .AppendChild(ownerDocument.CreateTextNode(id.ToString()));
            }
        }

        private void LoadData()
        {
            try
            {
                List<XmlElement> childNodes = _documents
                    .Where(doc => doc.DocumentElement != null)
                    .SelectMany(doc => doc.DocumentElement.ChildNodes.OfType<XmlElement>())
                    .ToList();

                // Load all the entities

                foreach (XmlElement childNode in childNodes)
                {
                    Type entityType = GetEntityTypeForNode(childNode);

                    if (entityType != null)
                    {
                        Entity entity = (Entity) Activator.CreateInstance(entityType);

                        LoadScalarProperties(entity, childNode);

                        Guid entityId = (entity as EntityBase)?.Id ?? Guid.NewGuid();

                        SetEntityId(childNode, entityId);

                        Tuple<string, Guid> key = Tuple.Create(entityType.Name, entityId);

                        if (_entityDictionary.ContainsKey(key))
                        {
                            Log.Error($"Unable to load entity of type {entity.GetType().Name} with id {entityId}, an entity with this id already exists.");
                        }
                        else
                        {
                            _entityDictionary.Add(key, entity);
                        }
                    }
                    else
                    {
                        Log.WarnFormat("Unable to load entity of type {0}, no such type exists.", childNode.Name);
                    }
                }

                // Linkup any navigation properties

                foreach (XmlElement childNode in childNodes)
                {
                    Type entityType = GetEntityTypeForNode(childNode);

                    if (entityType != null)
                    {
                        Tuple<string, Guid> key = Tuple.Create(entityType.Name, GetEntityId(childNode));

                        if (key.Item2 != Guid.Empty && _entityDictionary.ContainsKey(key))
                        {
                            Entity entity = _entityDictionary[key];

                            LoadNavigationProperties(entity, childNode);
                        }
                        else
                        {
                            // This "should" never happen, it means that when we made the second pass through the file to linkup
                            // navigation properties, we couldn't find the entity the second time.
                            Log.ErrorFormat("Internal consistancy error, unable to find {0} entity with id {1} in the dictionary.", key.Item1, key.Item2);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error("An exception occurred while loading xml data", exception);

                throw;
            }
        }

        private static bool IsNavigationPropertyType(Type type)
        {
            return typeof(Entity).IsAssignableFrom(type);
        }

        private static Enum StringToEnum(Type enumType, string value)
        {
            // First just check if this is a valid value for the enum and if so return it.

            if (Enum.IsDefined(enumType, value))
                return (Enum) Enum.Parse(enumType, value);

            string[] enumValues = Enum.GetValues(enumType).Cast<object>().Select(obj => obj.ToString()).ToArray();

            // Second, check if it is a valid value if we ignore case

            string[] matches = enumValues.Where(p => string.Compare(p, value, StringComparison.InvariantCultureIgnoreCase) == 0).ToArray();

            if (matches.Length == 1)
                return (Enum) Enum.Parse(enumType, matches[0]);

            // Third, check if there's is (only one) valid value that starts with this value

            matches = enumValues.Where(p => p.StartsWith(value, StringComparison.InvariantCultureIgnoreCase)).ToArray();

            if (matches.Length == 1)
                return (Enum) Enum.Parse(enumType, matches[0]);

            // Otherwise return the default

            Log.WarnFormat("The value \"{0}\" could not be matched any of the know values of {1}",
                           value, enumType.Name);

            return (Enum) Enum.ToObject(enumType, 0);
        }

        private static bool TryGetObjectAndProperty(object baseObject, string propertyPath, out object obj, out PropertyInfo propertyInfo)
        {
            string[] propertyNames = propertyPath.Split(".".ToCharArray());

            obj = baseObject;
            propertyInfo = null;

            object nextObject = baseObject;
            
            foreach (string propertyName in propertyNames)
            {
                obj = nextObject;

                if (obj == null)
                {
                    Log.WarnFormat("Unable to set property {0} on base object of type {1}, one of the properties in the path is null.",
                        propertyPath, baseObject.GetType().Name);

                    propertyInfo = null;

                    return false;
                }

                propertyInfo = obj.GetType().GetProperty(propertyName);

                if (propertyInfo == null)
                {
                    Log.WarnFormat("Unable to find property {0} on object of type {1} when loading properties for {2} with path {3}.",
                        propertyName, obj.GetType().FullName, baseObject.GetType().Name, propertyPath);

                    obj = null;

                    return false;
                }

                nextObject = propertyInfo.GetValue(obj, null);
            }

            return true;
        }

        private static void LoadScalarProperties(object entity, XmlNode entityNode)
        {
            foreach (XmlElement childNode in entityNode.ChildNodes.OfType<XmlElement>())
            {
                object obj;
                PropertyInfo propertyInfo;

                if (!TryGetObjectAndProperty(entity, childNode.Name, out obj, out propertyInfo))
                    continue;

                string valueAsString = (childNode.FirstChild != null && (childNode.FirstChild.NodeType == XmlNodeType.Text || childNode.FirstChild.NodeType == XmlNodeType.CDATA))
                                           ? childNode.FirstChild.Value
                                           : null;

                if (string.IsNullOrEmpty(valueAsString))
                    continue;

                if (propertyInfo.PropertyType == typeof(Guid) || propertyInfo.PropertyType == typeof(Guid?))
                {
                    Guid value;

                    if (Guid.TryParse(valueAsString, out value))
                    {
                        propertyInfo.SetValue(obj, value, null);
                    }
                    else
                    {
                        Log.WarnFormat("Error parsing guid value {0} for property {1} of {2}", valueAsString,
                                       propertyInfo.Name, obj.GetType().Name);
                    }
                }
                else if (propertyInfo.PropertyType == typeof(string))
                {
                    propertyInfo.SetValue(obj, valueAsString, null);
                }
                else if (propertyInfo.PropertyType == typeof (int) || propertyInfo.PropertyType == typeof (int?))
                {
                    int value;

                    if (int.TryParse(valueAsString, out value))
                        propertyInfo.SetValue(obj, value, null);
                }
                else if (propertyInfo.PropertyType == typeof(double) || propertyInfo.PropertyType == typeof(double?))
                {
                    double value;

                    if (double.TryParse(valueAsString, out value))
                        propertyInfo.SetValue(obj, value, null);
                }
                else if (propertyInfo.PropertyType == typeof(decimal) || propertyInfo.PropertyType == typeof(decimal?))
                {
                    decimal value;

                    if (decimal.TryParse(valueAsString, out value))
                        propertyInfo.SetValue(obj, value, null);
                }
                else if (propertyInfo.PropertyType == typeof(bool) || propertyInfo.PropertyType == typeof(bool?))
                {
                    string[] trueValues = { "-1", "1", "T", "TRUE", "Y", "YES" };
                    string[] falseValues = { "0", "F", "FALSE", "N", "NO" };

                    bool? value = null;

                    if (trueValues.Any(p => string.Compare(p, valueAsString, StringComparison.InvariantCultureIgnoreCase) == 0))
                        value = true;

                    if (falseValues.Any(p => string.Compare(p, valueAsString, StringComparison.InvariantCultureIgnoreCase) == 0))
                        value = false;

                    if (value.HasValue)
                        propertyInfo.SetValue(obj, value, null);
                }
                else if (propertyInfo.PropertyType == typeof(DateTime) || propertyInfo.PropertyType == typeof(DateTime?))
                {
                    DateTime value;

                    if (DateTime.TryParse(valueAsString, out value) && (value != DateTime.MinValue) && (value != new DateTime()))
                        propertyInfo.SetValue(obj, value, null);
                }
                else if (propertyInfo.PropertyType == typeof(DateTimeOffset) || propertyInfo.PropertyType == typeof(DateTimeOffset?))
                {
                    DateTime value;

                    if (DateTime.TryParse(valueAsString, out value) && (value.Ticks != 0))
                    {
                        DateTimeOffset valueWithOffset = value;

                        propertyInfo.SetValue(obj, valueWithOffset, null);
                    }
                }
                else if (propertyInfo.PropertyType == typeof(byte[]))
                {
                    byte[] value = Convert.FromBase64String(valueAsString); 
                    propertyInfo.SetValue(obj,value,null);
                }
                else if (propertyInfo.PropertyType == typeof (ShortTimeSpan) || propertyInfo.PropertyType == typeof (ShortTimeSpan?))
                {
                    ShortTimeSpan value;

                    if (!ShortTimeSpan.TryParse(valueAsString, out value))
                        value = 15;

                    propertyInfo.SetValue(obj, value, null);
                }
                else if (propertyInfo.PropertyType.IsEnum)
                {
                    propertyInfo.SetValue(obj, StringToEnum(propertyInfo.PropertyType, valueAsString), null);
                }
                else if (IsNavigationPropertyType(propertyInfo.PropertyType))
                {
                    // Skip it, Navigation Properties will be done in a seperate pass
                }
                else
                {
                    Log.WarnFormat(
                        "Unable to load data for property {0}, no data conversion has been specified for type {1}",
                        childNode.Name, propertyInfo.PropertyType.FullName);
                }
            }
        }

        private void LoadNavigationProperties(object entity, XmlNode entityNode)
        {
            foreach (XmlElement childNode in entityNode.ChildNodes.OfType<XmlElement>())
            {
                string propertyName = childNode.Name;
                
                // Skip the Id column
                if (string.Compare(propertyName, "Id", StringComparison.InvariantCultureIgnoreCase) == 0)
                    continue;

                object obj;
                PropertyInfo propertyInfo;

                if (!TryGetObjectAndProperty(entity, propertyName, out obj, out propertyInfo))
                    continue;

                string valueAsString = (childNode.FirstChild != null && childNode.FirstChild.NodeType == XmlNodeType.Text)
                            ? childNode.FirstChild.Value : null;

                if (!string.IsNullOrEmpty(valueAsString) && IsNavigationPropertyType(propertyInfo.PropertyType))
                {
                    Guid relatedEntityId;

                    if (Guid.TryParse(valueAsString, out relatedEntityId) && relatedEntityId != Guid.Empty)
                    {
                        Entity relatedEntity;

                        Tuple<string, Guid> relatedEntityKey = Tuple.Create(propertyInfo.PropertyType.Name, relatedEntityId);

                        if (_entityDictionary.TryGetValue(relatedEntityKey, out relatedEntity))
                        {
                            propertyInfo.SetValue(obj, relatedEntity, null);
                        }
                        else
                        {
                            Log.Warn($"Error could not find related {propertyInfo.PropertyType.Name} entity with Id " +
                                     $"of {relatedEntityId} for property {propertyInfo.Name} of {propertyInfo.DeclaringType?.Name}");
                        }
                    }
                    else
                    {
                        Log.Warn($"Error parsing guid value {valueAsString} for property " +
                                 $"{propertyInfo.Name} of {propertyInfo.DeclaringType?.Name}");
                    }
                }
            }
        }
    }
}
