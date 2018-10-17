using System;
using System.Globalization;
using System.Xml.Serialization;

namespace DemoDataBuilder.OutputModel
{
    [XmlRoot("Level4")]
    public class Level4 : LookupBase
    {
        public Level4()
        {
        }

        public Level4(string description, int sortOrder = 1)
        {
            Id = Guid.NewGuid();
            this.Description = description;
            this.SortOrder = sortOrder.ToString(CultureInfo.InvariantCulture);
        }
    }
}