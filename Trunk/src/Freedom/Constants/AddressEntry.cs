namespace Freedom.Constants
{
    public class AddressEntry
    {
        public AddressEntry(string code, string name, params string[] alternateNames)
        {
            Code = code;
            Name = name;
            AlternateNames = alternateNames;
        }

        public AddressEntry(AddressEntry parent, string code, string name, params string[] alternateNames)
        {
            Parent = parent;
            Code = code;
            Name = name;
            AlternateNames = alternateNames;
        }

        public string Code { get; }

        public string Name { get; }

        public string[] AlternateNames { get; }

        public AddressEntry Parent { get; }
    }
}