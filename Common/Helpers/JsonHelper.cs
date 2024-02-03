using ShieldVSExtension.Common.Models;

namespace ShieldVSExtension.Common.Helpers
{
    internal class JsonHelper
    {
        public static string Serialize<T>(T obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

        public static T Deserialize<T>(string source)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(source);
        }
    }
}