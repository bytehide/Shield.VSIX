namespace ShieldVSExtension.Common.Helpers
{
    internal static class Utils
    {
        public static string Truncate(this string value, int maxLength, string tail = "...") =>
            value.Length <= maxLength ? value : $"{value.Substring(0, maxLength)}{tail}";
    }
}