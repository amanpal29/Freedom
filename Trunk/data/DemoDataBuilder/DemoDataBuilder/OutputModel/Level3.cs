using System;
using System.Globalization;
using System.Xml.Serialization;

namespace DemoDataBuilder.OutputModel
{
    [XmlRoot("Level3")]
    public class Level3 : LookupBase
    {
        public Level3()
        {
        }

        public Level3(Level4 level4, string description, int sortOrder = 1)
        {
            Id = Guid.NewGuid();
            Description = description;
            SortOrder = sortOrder.ToString(CultureInfo.InvariantCulture); 
            Level4 = level4.Id;
        }

        public Guid Level4;
    }
}