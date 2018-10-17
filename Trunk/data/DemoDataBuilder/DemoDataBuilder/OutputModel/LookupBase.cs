namespace DemoDataBuilder.OutputModel
{
    public abstract class LookupBase : Entity
    {
        public string InternalCode;
        public string ExternalCode;
        public string Description;
        public string SortOrder;
    }

    public class BusinessType : LookupBase
    {
    }

    public class ContactType : LookupBase
    {
    }

    public class FacilityType : LookupBase
    {
    }

    public class OperationsType : LookupBase
    {
        public bool IsYearRound;
    }

    public class WorkArea : LookupBase
    {
    }
}
