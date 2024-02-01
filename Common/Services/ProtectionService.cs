using System.Threading.Tasks;

namespace ShieldVSExtension.Common.Services
{
    internal class ProtectionService
    {
        public static async Task<string> GetAllByToken(string token = "af941759-5b2f-475c-b442-1f3c287e672a")
        {
            return await HttpX.Get($"{ApiPath.ServerApiPath}/protections/{token}");
        }
    }
}