namespace ShieldVSExtension.Common.Models
{
    internal interface IProtection
    {
        string Name { get; set; }
        bool Value { get; set; }
    }

    internal class ShieldConfiguration
    {
        public string Name { get; set; }
        public EPresetType Preset { get; set; }
        public string ProjectToken { get; set; }
        public string ProtectionSecret { get; set; }
        public bool Enabled { get; set; }
        public string RunConfiguration { get; set; }
        public IProtection[] Protections { get; set; }
    }
}