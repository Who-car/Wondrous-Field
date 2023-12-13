using System.Text;
using System.Text.Json;

namespace PackageHelper
{
    public static class Serialiser
    {
        public static async Task<byte[]> SerialiseToBytes(object obj, JsonSerializerOptions options = default!)
            => await Task.Run(() => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj, options)));

        public static async Task<T> Deserialise<T>(string content, JsonSerializerOptions options = default!)
            => await Task.Run(() => JsonSerializer.Deserialize<T>(content, options)!);

        public static async Task<T> Deserialise<T>(byte[] content, JsonSerializerOptions options = default!)
            => await Task.Run(() => JsonSerializer.Deserialize<T>(content, options)!);
    }
}
