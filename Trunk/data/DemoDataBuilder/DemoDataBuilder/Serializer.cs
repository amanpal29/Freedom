using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using DemoDataBuilder.OutputModel;
using log4net;

namespace DemoDataBuilder
{
    [XmlRoot("Entities")]
    public class EntityCollection
    {
        private EntityCollection()
        {
        }

        internal EntityCollection(IEnumerable<Entity> entities)
        {
            Entities = new List<Entity>(entities);
        }

        [XmlElement("Account", typeof(Account))]
        [XmlElement("BusinessType", typeof(BusinessType))]
        [XmlElement("Contact", typeof(Contact))]
        [XmlElement("ContactType", typeof(ContactType))]
        [XmlElement("Facility", typeof(Facility))]
        [XmlElement("FacilityAttribute", typeof(FacilityAttribute))]
        [XmlElement("FacilityContact", typeof(FacilityContact))]
        [XmlElement("FacilityContactType", typeof(FacilityContactType))]
        [XmlElement("FacilityType", typeof(FacilityType))]
        [XmlElement("Level1", typeof(Level1))]
        [XmlElement("Level2", typeof(Level2))]
        [XmlElement("Level3", typeof(Level3))]
        [XmlElement("Level4", typeof(Level4))]
        [XmlElement("OperationsType", typeof(OperationsType))]
        [XmlElement("RiskAssessment", typeof(RiskAssessment))]
        [XmlElement("RiskAssessmentPage", typeof(RiskAssessmentPage))]
        [XmlElement("RiskAssessmentSelectedAnswer", typeof(RiskAssessmentSelectedAnswer))]
        [XmlElement("Site", typeof(Site))]
        [XmlElement("WorkArea", typeof(WorkArea))]
        public List<Entity> Entities { get; set; }
    }

    public static class Serializer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string DefaultNamespace = "http://schemas.hedgerowsoftware.com/hedgehogdata";


        private static XmlSerializer XmlSerializer
        {
            get
            {
                XmlSerializer serializer = new XmlSerializer(
                    typeof (EntityCollection), null, Entity.GetChildTypes(),
                    (XmlRootAttribute) Attribute.GetCustomAttribute(typeof (EntityCollection), typeof (XmlRootAttribute)),
                    DefaultNamespace);

                serializer.UnknownNode += LogUnknownNode;

                return serializer;
            }
        }

        private static void LogUnknownNode(object sender, XmlNodeEventArgs e)
        {
            using (ThreadContext.Stacks["Position"].Push(string.Format("({0},{1})", e.LineNumber, e.LinePosition)))
                Log.DebugFormat("Ignoring unrecognized {0} {1}", e.NodeType, e.Name);
        }

        public static void Serialize(Stream stream, IEnumerable<Entity> entities)
        {
            FileStream fileStream = stream as FileStream;

            if (fileStream != null)
            {
                using (ThreadContext.Stacks["FileName"].Push(fileStream.Name))
                    XmlSerializer.Serialize(stream, new EntityCollection(entities));
            }
            else
            {
                XmlSerializer.Serialize(stream, entities);
            }
        }

        public static void Serialize(TextWriter writer, IEnumerable<Entity> entities)
        {
            XmlSerializer.Serialize(writer, new EntityCollection(entities));
        }

        public static void Serialize(XmlWriter writer, IEnumerable<Entity> entities)
        {
            XmlSerializer.Serialize(writer, new EntityCollection(entities));
        }

        public static IList<Entity> Deserialize(Stream stream)
        {
            FileStream fileStream = stream as FileStream;

            EntityCollection entityCollection;

            if (fileStream != null)
            {
                using (ThreadContext.Stacks["FileName"].Push(fileStream.Name))
                    entityCollection = (EntityCollection) XmlSerializer.Deserialize(stream);
            }
            else
            {
                entityCollection = (EntityCollection) XmlSerializer.Deserialize(stream);
            }

            return entityCollection.Entities;
        }

        public static IList<Entity> Deserialize(TextReader reader)
        {
            return ((EntityCollection) XmlSerializer.Deserialize(reader)).Entities;
        }

        public static IList<Entity> Deserialize(XmlReader reader)
        {
            return ((EntityCollection) XmlSerializer.Deserialize(reader)).Entities;
        }
    }
}
