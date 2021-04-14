using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShieldVSExtension.Configuration
{
    internal class ShieldExtensionConfiguration
    {
        public string ApiToken { get; set; }
        public string LogsPath { get; set; }
        public bool StoreLogs { get; set; }
    }
}
