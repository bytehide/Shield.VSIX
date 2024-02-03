using System.Threading.Tasks;

namespace ShieldVSExtension.Common.Services
{
    internal class ProtectionService
    {
        public static async Task<string> GetAllByToken(string token = "67b6c137-f59f-4c32-8974-6e52c3602bf9")
        {
            return await HttpX.Get($"{ApiPath.ServerApiPath}/protections/{token}");
        }
    }
}