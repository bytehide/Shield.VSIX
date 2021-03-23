using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace ShieldVSExtension.Configuration
{
    public class ProjectConfiguration
    {
        [DefaultValue(true)]
        public bool IsEnabled { get; set; } = true;

        public string ProjectName { get; set; }

        public List<string> Files { get; private set; } = new List<string>();

        [DefaultValue(true)]
        public bool InheritFromProject { get; set; } = true;
        /*
        *  maximum
        *  balance
        *  optimized
        *  custom
        */
        public ProjectPreset ProjectPreset { get; set; }
        [JsonProperty]

        public List<string> Protections { get; set; }

        public bool IncludeSubDirectories { get; set; }

        public string TargetDirectory { get; set; }
    }
}
