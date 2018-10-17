using System;

namespace DemoDataBuilder.OutputModel
{
    public class FacilityContact : Entity
    {
        public FacilityContact()
        {
        }

        public FacilityContact(Guid facilityId, Guid contactId)
        {
            Facility = facilityId;
            Contact = contactId;
        }

        public Guid Facility;
        public Guid Contact;
    }
}
