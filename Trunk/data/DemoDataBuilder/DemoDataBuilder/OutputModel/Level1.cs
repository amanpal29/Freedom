using System;
using System.Globalization;
using System.Xml.Serialization;

namespace DemoDataBuilder.OutputModel
{
    [XmlRoot("Level1")]
    public class Level1 : LookupBase
    {
        public Level1()
        {
        }

        public Level1(Level2 level2, string description, int sortOrder = 1)
        {
            Id = Guid.NewGuid();
            Description = description;
            SortOrder = sortOrder.ToString(CultureInfo.InvariantCulture);
            Level2 = level2.Id;
        }

        public bool DiscloseInspections = true;
        public Guid Level2;
    }
}