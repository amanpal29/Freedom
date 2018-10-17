using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using DemoDataBuilder.Extensions;
using DemoDataBuilder.Importer;
using DemoDataBuilder.InputModel;
using DemoDataBuilder.OutputModel;
using log4net;

namespace DemoDataBuilder
{
    public class Engine
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static Guid AdministratorUserId = new Guid("{3B526C4E-50F3-425D-9787-6DB0696290FF}");

        private readonly List<IFacilityDataImporter> _facilityDataImporters = new List<IFacilityDataImporter>();

        private readonly List<Entity> _entities = new List<Entity>();

        private readonly Dictionary<Guid, LookupBase> _forwardLookup = new Dictionary<Guid, LookupBase>();
        private readonly Dictionary<string, Guid> _reverseLookup = new Dictionary<string, Guid>();

        private readonly List<KeyValuePair<int, double>> _areaCodes = new List<KeyValuePair<int, double>>();

        private int _nextSiteNumber = 1;
        private int _nextFacilityNumber = 1;

        public bool IsTollFreeAreaCode(int areaCode)
        {
            return 800 <= areaCode && areaCode < 900
                   && areaCode/10%10 == areaCode%10;
        }

        public void Add(IFacilityDataImporter facilityDataImporter)
        {
            _facilityDataImporters.Add(facilityDataImporter);
        }

        public void AddAreaCode(int areaCode, double probability)
        {
            _areaCodes.Add(new KeyValuePair<int, double>(areaCode, probability));
        }

        public bool ImportCore(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log.WarnFormat("File not found: {0}", filePath);
                return false;
            }

            filePath = new FileInfo(filePath).FullName;

            try
            {
                IList<Entity> entityCollection;

                using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Log.InfoFormat("Importing File {0}...", new FileInfo(filePath).FullName);
                    entityCollection = Serializer.Deserialize(stream);
                    stream.Close();
                }

                foreach (LookupBase lookupBase in entityCollection.OfType<LookupBase>())
                {
                    _forwardLookup.Add(lookupBase.Id, lookupBase);

                    Log.DebugFormat("Adding {0} '{1}' as {2}", lookupBase.GetType().Name.ToDisplayName(),
                                    lookupBase.Description, lookupBase.Description.ToKey());

                    if (!string.IsNullOrWhiteSpace(lookupBase.Description))
                    {
                        if (_reverseLookup.ContainsKey(lookupBase.Description.ToKey()))
                            Log.WarnFormat("{0} '{1}' already exists.", lookupBase.GetType().Name.ToDisplayName(), lookupBase.Description);
                        else
                            _reverseLookup.Add(lookupBase.Description.ToKey(), lookupBase.Id);
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                string message = string.Format("Unable to import file: {0}", filePath);
                Log.Warn(message, exception);
            }

            return false;
        }

        private void CreateWorkAreas(IEnumerable<FacilityData> data)
        {
            List<string> workAreaNames =
                data.Select(d => d.WorkArea).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            foreach (string workAreaName in workAreaNames)
            {
                WorkArea workArea = new WorkArea();

                workArea.Description = workAreaName;

                _entities.Add(workArea);
                
                _forwardLookup.Add(workArea.Id, workArea);

                if (_reverseLookup.ContainsKey(workArea.Description.ToKey()))
                    Log.WarnFormat("{0} '{1}' already exists.", workArea.GetType().Name.ToDisplayName(), workArea.Description);
                else
                    _reverseLookup.Add(workArea.Description.ToKey(), workArea.Id);
            }
        }

        private void CreateLocationHierarchy(IEnumerable<FacilityData> data)
        {
            List<string[]> hierarchy = data
                .Select(row => new []
                    {
                        row.Region ?? "Unnamed Region",
                        row.Zone ?? "Unnamed Zone",
                        row.Area ?? "Unnamed Area",
                        row.Community ?? "Unnamed Community"
                    })
                .ToList();

            foreach (string region in hierarchy.Select(row => row[0]).Distinct())
            {
                Level4 level4 = new Level4(region);

                _entities.Add(level4);

                List<string[]> zones = hierarchy.Where(row => row[0] == region).ToList();

                foreach (string zone in zones.Select(row => row[1]).Distinct())
                {
                    Level3 level3 = new Level3(level4, zone);

                    _entities.Add(level3);

                    List<string[]> areas = zones.Where(row => row[1] == zone).ToList();

                    foreach (string area in areas.Select(row => row[2]).Distinct())
                    {
                        Level2 level2 = new Level2(level3, area);

                        _entities.Add(level2);

                        List<string[]> communities = areas.Where(row => row[2] == area).ToList();

                        foreach (string community in communities.Select(row => row[3]).Distinct())
                        {
                            Level1 level1 = new Level1(level2, community);

                            _entities.Add(level1);

                            _forwardLookup.Add(level1.Id, level1);

                            if (_reverseLookup.ContainsKey(level1.Description.ToKey()))
                                Log.WarnFormat("Community '{0}' already exists.", level1.Description);
                            else
                                _reverseLookup.Add(level1.Description.ToKey(), level1.Id);
                        }
                    }
                }
            }
        }

        private Site CreateSite(FacilityData data)
        {
            Site result = _entities.OfType<Site>().FirstOrDefault(s => s.Name == data.SiteName);

            if (result == null)
            {
                result = new Site();

                result.Name = data.SiteName;
                result.LegalName = data.LegalName;

                if (_reverseLookup.ContainsKey(data.Community.ToKey()))
                    result.Level1 = _reverseLookup[data.Community.ToKey()];
                else
                    Log.WarnFormat("Community {0} was not found.", data.Community);

                if (Rand.Boolean(0.9))
                {
                    result.MainTelephoneNumber = Rand.Phone(_areaCodes);

                    if (Rand.Boolean(0.8))
                        result.MainFaxNumber = Rand.Phone(_areaCodes);
                }

                if (!string.IsNullOrWhiteSpace(data.EmailAddress))
                    result.EmailAddress = data.EmailAddress;

                result.SiteAddressUnitNumber = data.SiteUnitNumber;
                result.SiteAddressStreetNumber = data.SiteStreetNumber;
                result.SiteAddressStreetName = data.SiteStreetName;
                result.SiteAddressCity = data.SiteCity;
                result.SiteAddressProvince = data.SiteProvince;
                result.SiteAddressCountry = "Canada";
                result.SiteAddressPostalCode = data.SitePostalCode;

                try
                {
                    result.Latitude = double.Parse(data.Latitude);
                    result.Longitude = double.Parse(data.Longitude);
                }
                catch (ArgumentNullException)
                {
                }
                catch (FormatException)
                {
                }

                _entities.Add(result);
            }

            return result;
        }

        private void CreateFacility(FacilityData data)
        {
            Site site = CreateSite(data);

            Facility facility = new Facility();
            
            facility.Site = site.Id;

            facility.IsActive = true;

            if (_reverseLookup.ContainsKey(data.FacilityType.ToKey()))
                facility.FacilityType = _reverseLookup[data.FacilityType.ToKey()];
            else
                Log.WarnFormat("Facility Type {0} was not found.", data.FacilityType);

            if (_reverseLookup.ContainsKey(data.WorkArea.ToKey()))
                facility.WorkArea = _reverseLookup[data.WorkArea.ToKey()];
            else
                Log.WarnFormat("Work Area {0} was not found.", data.WorkArea);

            if (_reverseLookup.ContainsKey(data.BusinessType.ToKey()))
                facility.BusinessType = _reverseLookup[data.BusinessType.ToKey()];
            else
                Log.ErrorFormat("Business Type {0} was not found.", data.BusinessType);

            if (_reverseLookup.ContainsKey(data.OperationsType.ToKey()))
                facility.OperationsType = _reverseLookup[data.OperationsType.ToKey()];
            else
                Log.WarnFormat("Operations Type {0} was not found.", data.OperationsType);

            if (!string.IsNullOrWhiteSpace(data.FacilityName) && data.FacilityName != data.SiteName)
            {
                if (data.FacilityName.StartsWith(data.SiteName))
                {
                    int firstChar = data.SiteName.Length + 1;

                    while (firstChar < data.FacilityName.Length && !char.IsLetterOrDigit(data.FacilityName[firstChar]))
                        firstChar++;

                    if (firstChar < data.FacilityName.Length)
                        facility.Name = data.FacilityName.Substring(firstChar);
                }
                else
                {
                    facility.Name = data.FacilityName;
                }
            }

            facility.OperationStartDate = Rand.Date(DateTime.Today.AddYears(-20), DateTime.Today);
            
            if (Rand.Boolean(0.8))
            {
                facility.LastInspectionDate = Rand.Date(DateTime.Today.AddYears(-1), DateTime.Today);
                facility.NextInspectionDate = Rand.Date(facility.LastInspectionDate.Value.AddMonths(1),
                                                       facility.LastInspectionDate.Value.AddMonths(12));
            }
            else
            {
                facility.NextInspectionDate = Rand.Date(DateTime.Today.AddMonths(-1), DateTime.Today.AddMonths(12));
            }

            facility.CopyAddress(site);

            _entities.Add(facility);

            if (facility.LastInspectionDate.HasValue)
                CreateRiskAssessment(facility, Rand.WeightedInt(0d, 0.2, 0.6, 0.2));

            if (Rand.Boolean(0.8))
            {
                facility.PrimaryOwner = facility.PrimaryOperator = CreateRandomContact(facility.Id);
            }
            else
            {
                facility.PrimaryOwner = CreateRandomContact(facility.Id);
                facility.PrimaryOperator = CreateRandomContact(facility.Id);
            }

            while (Rand.Boolean(0.2))
                CreateRandomContact(facility.Id);
        }

        public void CreateRiskAssessment(Facility facility, int riskRating)
        {
            string[] ratings = new[] {"Invalid", "Low", "Moderate", "High"};

            Debug.Assert(facility.LastInspectionDate != null, "facility.LastInspectionDate != null");

            RiskAssessment riskAssessment = new RiskAssessment();
            riskAssessment.Id = Guid.NewGuid();
            riskAssessment.Facility = facility.Id;
            riskAssessment.InspectorId = AdministratorUserId;
            riskAssessment.RiskAssessmentDate = facility.LastInspectionDate.Value;
            riskAssessment.RiskScore = Rand.Int(0, 100);
            facility.RiskRating = riskAssessment.RiskRating = ratings[riskRating];

            RiskAssessmentPage riskAssessmentPage = new RiskAssessmentPage();
            riskAssessmentPage.Id = Guid.NewGuid();
            riskAssessmentPage.RiskAssessment = riskAssessment.Id;
            riskAssessmentPage.RiskAssessmentModel = new Guid("{a6290f08-a237-4e37-bd31-06cb4f13560a}");

            RiskAssessmentSelectedAnswer selectedAnswer = new RiskAssessmentSelectedAnswer();
            selectedAnswer.Id = Guid.NewGuid();
            selectedAnswer.Page = riskAssessmentPage.Id;
            selectedAnswer.Question = new Guid("{1eae253c-8966-4a24-aee1-9d7c79c375ca}"); // The only question

            switch (riskRating)
            {
                case 1:
                    selectedAnswer.Answer = new Guid("{361fc74c-f963-4931-8310-74e47c555907}"); // Low Risk
                    break;

                case 2:
                    selectedAnswer.Answer = new Guid("{a5d95920-bffb-4023-952c-d39748948aff}"); // Moderate Risk
                    break;

                case 3:
                    selectedAnswer.Answer = new Guid("{607cb509-3d7a-44c0-8e29-e60c48818cda}"); // High Risk
                    break;
            }

            _entities.Add(riskAssessment);
            _entities.Add(riskAssessmentPage);
            _entities.Add(selectedAnswer);
        }

        public Guid CreateRandomContact(params Guid[] facilityIds)
        {
            Contact contact = new Contact();

            contact.Id = Guid.NewGuid();
            contact.Title = Rand.Title();
            contact.FirstName = Rand.FirstName(contact.Title);
            contact.MiddleName = Rand.MiddleName();
            contact.LastName = Rand.LastName();
            contact.MobilePhoneNumber = Rand.Phone(_areaCodes.Where(kvp => !IsTollFreeAreaCode(kvp.Key)).ToList());
            contact.WorkPhoneNumber = Rand.Phone(_areaCodes.Where(kvp => !IsTollFreeAreaCode(kvp.Key)).ToList());

            contact.City = "Calgary";
            contact.Street = string.Format("{0} {1}", Rand.Int(10, 1000), Rand.CalgaryStreetName());
            contact.Province = "AB";
            contact.Country = "Canada";
            contact.PostalCode = Rand.PostalCode();

            _entities.Add(contact);

            if (facilityIds != null)
            {
                foreach (Guid facilityId in facilityIds)
                    _entities.Add(new FacilityContact(facilityId, contact.Id));
            }

            return contact.Id;
        }

        public void AssignNumbersToSitesAndFacilities()
        {
            foreach (Facility facility in _entities.OfType<Facility>().OrderBy(f => f.OperationStartDate))
            {
                Site site = _entities.OfType<Site>().Single(s => s.Id == facility.Site);

                if (string.IsNullOrEmpty(site.Number))
                {
                    site.Number = string.Format("ST{0:d5}", _nextSiteNumber);
                    _nextSiteNumber += Rand.Int(1, 5);
                }

                facility.Number = string.Format("FA{0:d6}", _nextFacilityNumber);
                _nextFacilityNumber += Rand.Int(1, 8);
            }
        }

        public void Execute()
        {
            List<FacilityData> facilityData = new List<FacilityData>();

            foreach (IFacilityDataImporter facilityDataImporter in _facilityDataImporters)
                facilityData.AddRange(facilityDataImporter.GetData());

            CreateWorkAreas(facilityData);

            CreateLocationHierarchy(facilityData);

            foreach (FacilityData data in facilityData)
                CreateFacility(data);

            AssignNumbersToSitesAndFacilities();

            using (FileStream fileStream = new FileStream(@"D:\Trunk\data\SampleData\Demo\DemoFacilities.xml", FileMode.Create, FileAccess.ReadWrite))
            {
                Serializer.Serialize(fileStream, _entities);

                fileStream.Close();
            }
        }
    }
}

