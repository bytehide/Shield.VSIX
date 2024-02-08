using System.Threading.Tasks;

namespace ShieldVSExtension.Common.Services;

internal class ProtectionService
{
    public static async Task<string> GetAllByTokenAsync(string token)
    {
        return await HttpX.GetAsync($"{ApiPath.ServerApiPath}/protections/{token}");
    }
}