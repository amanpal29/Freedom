using System;
using System.Xml.Serialization;

namespace DemoDataBuilder.OutputModel
{
    public class Facility : Entity
    {
        public void CopyAddress(Site site)
        {
            MailingAddressStreet = string.Format("{0} {1}", site.SiteAddressStreetNumber, site.SiteAddressStreetName);
            MailingAddressCity = site.SiteAddressCity;
            MailingAddressProvince = site.SiteAddressProvince;
            MailingAddressCountry = site.SiteAddressCountry;
            MailingAddressPostalCode = site.SiteAddressPostalCode;
        }

        public DateTime OperationStartDate;
        public bool IsActive;
        public string Name;
        public string Number;
        public Guid Site;
        public Guid FacilityType;
        public Guid? BusinessType;
        public DateTime? LastInspectionDate;
        public DateTime? NextInspectionDate;
        public Guid WorkArea;
        public Guid PrimaryOperator;
        public Guid PrimaryOwner;
        public Guid? OperationsType;
        public string RiskRating;
        public Guid? BillingAccount;
        public Guid? PermitFeeCategory;
        public int? BillingCycleStartMonth;
        public bool IsExemptFromBilling;
        public int? MonthsBilledAnnually;
        public DateTime? BillingStartDate;
        public bool IsFirstYearPartialBilled;
        public int? MonthsBilledInFirstYear;
        
        [XmlElement("MailingAddress.City")]
        public string MailingAddressCity;

        [XmlElement("MailingAddress.Street")]
        public string MailingAddressStreet;

        [XmlElement("MailingAddress.Province")]
        public string MailingAddressProvince;

        [XmlElement("MailingAddress.Country")]
        public string MailingAddressCountry;

        [XmlElement("MailingAddress.PostalCode")]
        public string MailingAddressPostalCode;
    }
}
