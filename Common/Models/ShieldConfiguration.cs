using System.Collections.Generic;

namespace ShieldVSExtension.Common.Models;

// internal interface IProtection
// {
//     string Name { get; set; }
//     bool? Value { get; set; }
// }

// internal class Protection : IProtection
// {
//     public string Name { get; set; }
//     public bool? Value { get; set; }
// }

internal class ShieldConfiguration
{
    public string Name { get; set; }
    public string Preset { get; set; }
    public string ProjectToken { get; set; }
    public string ProtectionSecret { get; set; }
    public bool Enabled { get; set; }
    public string RunConfiguration { get; set; }
    public Dictionary<string, bool?> Protections { get; set; } = [];

    // public List<Protection> Protections { get; set; } = [];
}