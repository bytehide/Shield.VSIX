using System;

namespace ShieldVSExtension.Common.Extensions
{
    public static class PresetExtension
    {
        public static string ToFriendlyString(this EPresetType preset)
        {
            return preset switch
            {
                EPresetType.Maximum => "Maximum",
                EPresetType.Balance => "Balance",
                EPresetType.Optimized => "Optimal",
                EPresetType.Custom => "Custom",
                _ => throw new ArgumentOutOfRangeException(nameof(preset), preset, null)
            };
        }
    }
}