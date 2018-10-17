using System.ComponentModel;

namespace Freedom.DomainGenerator.DomainDefinitionModel
{
    public enum Multiplicity
    {
        ZeroOrOne,
        One,
        Many
    }

    public static class MultiplicityExtensions
    {
        public static string GetAttribute(this Multiplicity value)
        {
            switch (value)
            {
                case Multiplicity.ZeroOrOne:
                    return " Multiplicity=\"0..1\"";

                case Multiplicity.One:
                    return " Multiplicity=\"1\"";

                case Multiplicity.Many:
                    return " Multiplicity=\"*\"";

                default:
                    throw new InvalidEnumArgumentException(nameof(value), (int) value, typeof (Multiplicity));
            }
        }
    }
}