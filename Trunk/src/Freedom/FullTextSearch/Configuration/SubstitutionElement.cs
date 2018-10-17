using System.Configuration;

namespace Freedom.FullTextSearch.Configuration
{
    public class SubstitutionElement : ConfigurationElement
    {
        [ConfigurationProperty("find", IsRequired = true, IsKey = true)]
        public string Find
        {
            get { return ((string)(base["find"])); }
            set { base["find"] = value; }
        }

        [ConfigurationProperty("replaceWith", IsRequired = true)]
        public string ReplaceWith
        {
            get { return ((string)(base["replaceWith"])); }
            set { base["replaceWith"] = value; }
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(Find) && !string.IsNullOrWhiteSpace(ReplaceWith);
    }
}