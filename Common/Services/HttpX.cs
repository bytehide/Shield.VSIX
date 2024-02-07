using System.Threading.Tasks;

namespace ShieldVSExtension.Common.Services;

internal class HttpX
{
    public static async Task<string> Get(string url)
    {
        using (var client = new System.Net.Http.HttpClient())
        {
            return await client.GetStringAsync(url);
        }
    }
}