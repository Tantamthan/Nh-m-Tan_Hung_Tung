using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Lab8_PhamVanTung_2324801030079.Extensions;

public static class SessionExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static void SetObject<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value, SerializerOptions));
    }

    public static T? GetObject<T>(this ISession session, string key)
    {
        var json = session.GetString(key);
        return string.IsNullOrWhiteSpace(json)
            ? default
            : JsonSerializer.Deserialize<T>(json, SerializerOptions);
    }
}
