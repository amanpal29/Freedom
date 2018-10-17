namespace Freedom.DomainGenerator.DomainDefinitionModel
{
    public class PrimitiveType
    {
        public PrimitiveType(string conceptualType, string storageType, string sqlDataType, string clrType, string xsdType)
        {
            ConceptualType = conceptualType;
            StorageType = storageType;
            SqlDataType = sqlDataType;
            ClrType = clrType;
            XsdType = xsdType;
        }

        public string ConceptualType { get; }

        public string StorageType { get; }

        public string SqlDataType { get; }

        public string ClrType { get; }

        public string XsdType { get; }
    }
}
