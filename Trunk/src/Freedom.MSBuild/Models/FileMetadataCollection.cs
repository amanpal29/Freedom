using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Freedom.MSBuild.Models
{
    [CollectionDataContract(Namespace = FileMetadata.Namespace, ItemName = "File")]
    public class FileMetadataCollection : List<FileMetadata>
    {

    }
}
