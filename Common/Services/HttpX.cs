using System.Threading.Tasks;

namespace ShieldVSExtension.Common.Services;

internal class HttpX
{
    public static async Task<string> GetAsync(string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;

            using var client = new System.Net.Http.HttpClient();
            return await client.GetStringAsync(url);
        }
        catch (System.Exception e)
        {
            System.Diagnostics.Debug.WriteLine(e);
            return string.Empty;
        }
    }
}