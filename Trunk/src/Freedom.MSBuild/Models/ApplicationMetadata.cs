using System.Runtime.Serialization;

namespace Freedom.MSBuild.Models
{
    [DataContract(Namespace = Namespace)]
    public class ApplicationMetadata
    {
        public const string Namespace = "http://schemas.automatedstocktrader.com/bootstrapper";

        private bool _isSerializing;
        private FileMetadataCollection _files;

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Version { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public FileMetadataCollection Files
        {
            get
            {
                if (!_isSerializing && _files == null)
                    _files = new FileMetadataCollection();
                
                return _files;
            }
            set { _files = value; }
        }

        [DataMember]
        public string LaunchTarget { get; set; }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            _isSerializing = true;
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext context)
        {
            _isSerializing = false;
        }
    }
}
