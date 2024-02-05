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

        public static ShieldConfiguration Parse(string source)
        {
            return Deserialize<ShieldConfiguration>(source);
        }

        public static string Stringify(ShieldConfiguration configuration)
        {
            return Serialize(configuration);
        }
    }
}