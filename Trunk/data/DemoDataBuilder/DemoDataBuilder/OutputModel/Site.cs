using System;
using System.Xml.Serialization;

namespace DemoDataBuilder.OutputModel
{
    [XmlRoot("Site")]
    public class Site : Entity
    {
        public string Name;

        public string Number;

        public string LegalName;
        
        public Guid Level1;
        
        public string MainTelephoneNumber;

        public string MainFaxNumber;

        public string EmailAddress;

        [XmlElement("SiteAddress.UnitNumber")]
        public string SiteAddressUnitNumber;

        [XmlElement("SiteAddress.City")]
        public string SiteAddressCity;

        [XmlElement("SiteAddress.StreetNumber")]
        public string SiteAddressStreetNumber;

        [XmlElement("SiteAddress.StreetName")]
        public string SiteAddressStreetName;

        [XmlElement("SiteAddress.Province")]
        public string SiteAddressProvince;

        [XmlElement("SiteAddress.Country")]
        public string SiteAddressCountry;

        [XmlElement("SiteAddress.PostalCode")]
        public string SiteAddressPostalCode;

        [XmlElement("GpsCoordinate.Latitude")]
        public double? Latitude;

        [XmlElement("GpsCoordinate.Longitude")]
        public double? Longitude;
    }
}
