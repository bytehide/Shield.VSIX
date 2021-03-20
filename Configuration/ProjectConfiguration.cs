using System.Collections.Generic;
using System.ComponentModel;

namespace ShieldVSExtension.Configuration
{
    public class ProjectConfiguration
    {
        [DefaultValue(true)]
        public bool IsEnabled { get; set; } = true;

        public string ProjectName { get; set; }

        public List<string> Files { get; private set; } = new List<string>();

        public bool IncludeSubDirectories { get; set; }

        public string TargetDirectory { get; set; }
    }
}
