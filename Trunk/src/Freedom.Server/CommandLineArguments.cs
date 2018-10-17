using Freedom.Parsers;

namespace Freedom.Server
{
    public class CommandLineArguments
    {
        [AlternateName("F")]
        public bool Foreground { get; set; }

        [AlternateName("I")]
        public bool Install { get; set; }

        [AlternateName("U")]
        public bool Uninstall { get; set; }

        [AlternateName("S")]
        public bool Status { get; set; }

        [AlternateName("?")]
        public bool Help { get; set; }

        public string InstanceName { get; set; }

        public string Account { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
