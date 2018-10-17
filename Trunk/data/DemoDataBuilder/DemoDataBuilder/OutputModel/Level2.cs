using System;
using System.Globalization;
using System.Xml.Serialization;

namespace DemoDataBuilder.OutputModel
{
    [XmlRoot("Level2")]
    public class Level2 : LookupBase
    {
        public Level2()
        {
        }

        public Level2(Level3 level3, string description, int sortOrder = 1)
        {
            Id = Guid.NewGuid();
            Description = description;
            SortOrder = sortOrder.ToString(CultureInfo.InvariantCulture);
            Level3 = level3.Id;
        }

        public Guid Level3;
    }
}