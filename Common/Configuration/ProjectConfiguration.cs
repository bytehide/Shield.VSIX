using System.Collections.Generic;
using System.ComponentModel;

namespace ShieldVSExtension.Common.Configuration
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
        public ProjectPreset ApplicationPreset { get; set; }
        
        public List<string> Protections { get; set; }

        public bool IncludeSubDirectories { get; set; }

        public string TargetDirectory { get; set; }

        /*Shield Application*/

        public string FileToProtect { get; set; }

        [DefaultValue(true)]
        public bool ReplaceOriginalFile { get; set; } = true;
    }
}
