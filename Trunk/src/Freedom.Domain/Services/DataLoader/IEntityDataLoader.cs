using System.Collections.Generic;
using System.IO;
using Freedom.Domain.Model;

namespace Freedom.Domain.Services.DataLoader
{
    public interface IEntityDataLoader
    {
        void LoadFromFiles(params string[] filenames);
        void LoadFromStreams(params Stream[] streams);
        IEnumerable<Entity> Results { get; }
    }
}