using System;
using System.Threading.Tasks;
using ShieldVSExtension.Common.Helpers;

namespace ShieldVSExtension.Common.Services
{
    internal static class CoreService
    {
        public static async Task<VersionInfo> GetVersionInfoAsync(EPackageType packageType, string version = "")
        {
            var product = packageType switch
            {
                EPackageType.Vsix => "vs",
                EPackageType.Msbuilder => "msbuilder",
                _ => throw new ArgumentException("Invalid package type")
            };

            var url = $"{ApiPath.ServerApiPath}/version/check?product={product}";
            if (!string.IsNullOrEmpty(version))
            {
                url += $"&version=${version}";
            }

            var data = await HttpX.GetAsync(url);
            return JsonHelper.Deserialize<VersionInfo>(data);

            // var buffer = JsonHelper.Deserialize<VersionInfo>(data);
            // buffer.ForceUpdate = true;
            // return buffer;
        }
    }
}