using System.Collections.Generic;
using DemoDataBuilder.InputModel;

namespace DemoDataBuilder.Importer
{
    public interface IFacilityDataImporter
    {
        IEnumerable<FacilityData> GetData();
    }
}