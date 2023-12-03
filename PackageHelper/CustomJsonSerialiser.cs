using System.Text;
using System.Text.Json;

namespace PackageHelper
{
    public class CustomJsonSerialiser
    {
        public static async Task<byte[]> SerialiseToBytes(object obj, JsonSerializerOptions options = default!)
                => await Task.Run(() => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj, options)));

        public static async Task<T> Deserialise<T>(string txt, JsonSerializerOptions options = default!)
            => await Task.Run(() => JsonSerializer.Deserialize<T>(txt, options)!);
    }
}
