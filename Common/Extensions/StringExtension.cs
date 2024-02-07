using System.Linq;
using System.Text;
using Utils = ShieldVSExtension.Common.Helpers.Utils;

namespace ShieldVSExtension.Common.Extensions;

public static class StringExtension
{
    public static bool IsEmpty(this string source) => !(source?.Length > 0 && !string.IsNullOrWhiteSpace(source));

    public static string TitleCase(this string source)
    {
        if (source.Length <= 0) return source;

        return source.Any(char.IsUpper) ? source : UppercaseFirstLetter(source);
    }

    public static string UppercaseFirstLetter(this string source)
    {
        if (source.Length <= 0) return source;

        var letters = source.ToLower().ToCharArray();
        letters[0] = char.ToUpper(letters[0]);
        return new string(letters);
    }

    public static byte[] ConvertToBytes(this string source) =>
        source.Length <= 0 ? [] : Encoding.ASCII.GetBytes(source);

    public static string Truncate(this string value, int maxLength) => Utils.Truncate(value, maxLength);
}