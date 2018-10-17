using System;

namespace DemoDataBuilder.OutputModel
{
    public class Account : Entity
    {
        public Guid AccountOwner;
        public bool IsActive;
        public string Name;
        public string Number;
        public Guid WorkArea;
    }
}
