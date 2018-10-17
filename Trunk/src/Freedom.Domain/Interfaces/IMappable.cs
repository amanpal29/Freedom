

namespace Freedom.Domain.Interfaces
{
    public interface IMappable 
    {
        double? Latitude { get; }

        double? Longitude { get; }

        string MapMarkerHeader { get; }
    }
}
