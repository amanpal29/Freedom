using DemoDataBuilder.ComponentModel;

namespace DemoDataBuilder.InputModel
{
    public class FacilityData
    {
        public string SiteName { get; set; }
        
        public string FacilityName { get; set; }

        public string WorkArea { get; set; }

        public string EmailAddress { get; set; }

        public string ProgramArea { get; set; }
        
        public string FacilityCategory { get; set; }

        public string FacilityType { get; set; }

        public string BusinessType { get; set; }

        public string OperationsType { get; set; }

        [AlternateName("FeeCategory")]
        public string PermitFeeCategory { get; set; }

        public string LegalName { get; set; }

        public string Region { get; set; }

        public string Area { get; set; }

        public string Zone { get; set; }
        
        public string Community { get; set; }

        [AlternateName("Site Unit #")]
        public string SiteUnitNumber { get; set; }

        [AlternateName("Site Street #")]
        public string SiteStreetNumber { get; set; }

        public string SiteStreetName { get; set; }

        public string SiteCity { get; set; }

        public string SiteProvince { get; set; }

        public string SiteCountry { get; set; }

        public string SitePostalCode { get; set; }

        public string MailingUnitNumber { get; set; }

        public string MailingStreetNumber { get; set; }

        public string MailingStreetName { get; set; }

        public string MailingCity { get; set; }

        public string MailingProvince { get; set; }

        public string MailingCountry { get; set; }

        public string MailingPostalCode { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }
    }
}
