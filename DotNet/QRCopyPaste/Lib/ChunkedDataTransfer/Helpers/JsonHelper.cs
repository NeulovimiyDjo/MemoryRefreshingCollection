using System.Text.Json;

namespace ChunkedDataTransfer
{
    internal static class JsonHelper
    {
        internal static bool TryDeserialize<TData>(string dataStr, out TData data)
        {
            try
            {
                data = JsonSerializer.Deserialize<TData>(dataStr);
                return true;
            }
            catch (JsonException)
            {
                data = default;
                return false;
            }
        }
    }
}
