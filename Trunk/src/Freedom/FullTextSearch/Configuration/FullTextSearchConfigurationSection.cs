using System.ComponentModel;
using System.Configuration;

namespace Freedom.FullTextSearch.Configuration
{
    public class FullTextSearchConfigurationSection : ConfigurationSection
    {
        public const string SectionName = "fullTextSearch";

        internal static FullTextSearchConfigurationSection GetSection()
        {
            return ConfigurationManager.GetSection(SectionName) as FullTextSearchConfigurationSection;
        }

        [ConfigurationProperty("substitutions")]
        public SubstitutionElementCollection Substitutions => (SubstitutionElementCollection) base["substitutions"];

        [ConfigurationProperty("noiseWords")]
        [TypeConverter(typeof (CommaDelimitedStringCollectionConverter))]
        public CommaDelimitedStringCollection NoiseWords
        {
            get { return (CommaDelimitedStringCollection) base["noiseWords"]; }
            set { base["noiseWords"] = value; }
        }
    }
}
