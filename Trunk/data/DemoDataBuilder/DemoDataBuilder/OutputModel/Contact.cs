using System.Xml.Serialization;

namespace DemoDataBuilder.OutputModel
{
    public class Contact : Entity
    {
        public string Title;

        public string FirstName;

        public string MiddleName;

        public string LastName;

        public string Company;

        public string WorkPhoneNumber;

        public string MobilePhoneNumber;

        public string EmailAddress;

        [XmlElement("WorkAddress.City")]
        public string City;

        [XmlElement("WorkAddress.Street")]
        public string Street;

        [XmlElement("WorkAddress.Province")]
        public string Province;

        [XmlElement("WorkAddress.Country")]
        public string Country;

        [XmlElement("WorkAddress.PostalCode")]
        public string PostalCode;
    }
}
